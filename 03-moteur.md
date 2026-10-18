# 03 — Moteur : boot → menu → chargement → boucle → fin

Source : décompilation ILSpy (cache `decomp/*.cs`, noms `Namespace.Type.cs`), lecture de `Castle Story_Data/{globalgamemanagers,level*}` (strings, TimeManager) et de `Info/`. Statique uniquement.
Légende : **[O]** observé dans le code/les données, **[D]** déduit. FP = `Assembly-CSharp-firstpass`, BC = `Brix.Core`.
Unity 5.6.5f1 (level0), UNet (NetworkLobbyManager), MoonSharp.

## 0. Scènes (ordre du build, `globalgamemanagers`) [O]

| Idx | Scène | Contenu notable |
|---|---|---|
| 0 | `SceneSplashScreen` (level0) | `Brix.UI.SplashScreen`, `PersistLoader` ("Persists Loader"), LuaScripts |
| 1 | `SceneMenu` (level1) | `MainMenuViewController`, menus (LobbyMenu, NewsMenu...), `CameraMainMenu` |
| 2 | `ScenePrincipale` (level2) | `Architecte`, `Neo` ("/Jeu"), `GameClock`, `BlockEngine`, `TestPathfinding`, `NeutralTransactor`, `Users`, `LoadingScreen`, Canvas, `LuaRegistry` (via LuaScripts) |
| 3 | `PersistentScene` (level3) | `FactoryLoader`, `UNetManager` (NetworkManager), `SteamManager`, `PersistentCoroutineRunner`, `GameParam` ("Persistent Game Parameters"), GameAnalytics |
| 4+ | ~20 `Factories/*Factory.unity` | chargées en additif au boot, déchargées une fois les templates enregistrés |

`UNetManager` : `offlineScene`/`lobbyScene` = `SceneMenu`, `playScene` = `ScenePrincipale` (strings de level3 ; mapping des champs [D]).
Aucune entrée d'ordre d'exécution de scripts (MonoManager) dans `globalgamemanagers` [O, par absence de strings] → ordre Update inter-MonoBehaviour non spécifié [D].

## 1. Boot Unity → menu

| # | Classe.méthode (DLL) | Rôle / données |
|---|---|---|
| 1 | `Brix.Game.Utils.PersistLoader.Awake` (FP) | `SceneManager.LoadScene("PersistentScene", Additive)` une fois (`PersistsLoaded`) |
| 2 | `Brix.Utils.Factories.FactoryLoader.Start` (FP) | charge chaque scène de `_scenes` en additif |
| 3 | `Factory.Awake` → `FactoryLoader.OnFirstFactoryCreated` → coroutine `WaitForFactoriesLoaded` | fin de frame → décharge les scènes factory → `IFactoryPostProcessor.PostProcess` → `FactoriesLoadedEvent` → `IBrixComponent.Initialize()` de chaque template → `FactoriesReadyEvent` |
| 4 | `FactorySignalInvoker.OnFactoriesReady` | `OnFactoriesReadySig.Invoke()` (Signal statique, aussi exposé en Lua `hk_OnFactoriesReady`) |
| 5 | `Brix.UI.SplashScreen.Start` | `OnFactoriesReadySig.ConnectOnce` → `LoadNextScene()` si `skip`, sinon écran 1 (crédits Kickstarter) → bouton `LoadNextScene` → `SceneManager.LoadScene(nextScene)` (= `SceneMenu` [D]) |
| 6 | `Brix.UI.Login.MainMenuViewController` + menus Lua/C# (`Brix.UI.Builder.Menu.*`) | login, listes de maps (`AssetDispensary.maps/saves/gamemodes/presets/mods`), `BaseMapStarterMenu` |

Sources d'assets (`Brix.Assets.AssetDispensary`, FP) [O] : `Info/Maps/`, `Info/Saves/`, `Info/Lua/Gamemodes/`, `Info/GMPresets/`, `Info/Lua/Mods/`, `Info/Blueprints/`, `Info/Languages/`, + `Info/UGC/*`, `Info/Downloads/*`, backups `Info/Backups/`.

## 2. Menu → `StartGame` → chargement de ScenePrincipale

### 2.1 Solo (et World Editor / Credit Island)

| # | Classe.méthode (DLL) | Données / effet |
|---|---|---|
| 1 | `BaseMapStarterMenu.OnOk` (FP) | `BaseMapLibraryMenu.SetSelectedAsLoaded()` copie sélection → `GameParam.Map/Gamemode/Preset` (`Asset_Map`, `Asset_Gamemode`, `Asset_GamemodePreset`). Si save ancienne : `SaveUpdaterRunner.Update` (thread, backup + staging, migrations `SaveUpdater10to11…21to22`) avant. `NewsMenu` : lance en dur `Invictus` + `sandbox` + preset `sandbox_default_…` |
| 2 | `GameParam.StartGame(onAnimEnd)` (FP, static) | connecte `LoadingScreenAnimStart/End`, `GameSignals.Invoke(LoadingGame, menu)` |
| 3 | `LoadingScreenDriver.OnLoadGame(menu)` → `LoadingScreen.Close()` → `OnAnimationStart/End` | signaux `LoadingScreenAnimStart` (`GameParam._waitingForAnim=true`) puis `LoadingScreenAnimEnd` |
| 4 | `GameParam.OnLoadingScreenAnimEnd` → `StartLocalOnly` → `UNetManager.StartHostLocalOnly` | `maxConnections=1`, `networkPort=0`, `StartHost()` (UNet) : le solo **est** un host UNet local |
| 5 | `UNetManager.StartHost` → `ManuallyInitTransport` (2 canaux : ReliableSequenced, Unreliable ; Acks64) → `NetworkLobbyManager.StartHost` | |
| 6 | `LobbyPlayer.Start` (FP) | si `!Neo.multiplayer` : `SendReadyToBeginMessage()` immédiat ; sinon attend le lobby |
| 7 | `UNetManager.OnLobbyServerPlayersReady` | tous prêts → `ServerChangeScene(playScene)` ; instancie un `User` (`playerPrefab`) par joueur avec `factionNumber`/`TeamNumber` (`ReplacePlayerForConnection` + `SpawnWithClientAuthority`) ; `ServerPlayersReady` |
| 8 | Unity charge `ScenePrincipale` | `Neo.Awake`, `Architecte.Awake`, `GameClock.Awake`, `FlatWorldInitializer.Start` (cf. 3) |

### 2.2 Multijoueur (différences) [O]

| Étape | Détail |
|---|---|
| Host | `NetworkWaitingModalMenu` → `UNetManager.StartHostMultiplayer(ip,port)` : `Neo.multiplayer=true`, `Initialize()` (handlers `DeblocMessageHandler`, `LuaMessageHandler`, `FileTransfer`), NAT punchthrough, UPnP, LAN broadcast (port 3000), lobby Steam ; max 4 connexions |
| Params | `GameParam.UpdateParameters` (server) construit `GameParameters` (map/mode/preset codename, `AssetLastWriteTime`, min/max équipes/factions/joueurs depuis `Preset.settings["MP_*"]`) → SyncVar `NetworkParameters` |
| Client | `GameParam.ReadAndApplyNetworkParams` retrouve la map (saves puis maps, même codename **et** même `ModifiedTime`) sinon `UNetManager.RequestMapDownload(true)` → `SendFiles` (host → clients, `FileTransfer`) |
| Prêt | `LobbyMenu.SetIsReady` / `SendReadyToBeginMessage` ; l'hôte lance via `LobbyMenu.StartGame` → `OnLobbyServerPlayersReady` (idem 2.1 #7) |
| Debug | si `ScenePrincipale` est lancée directe (`!GameParam._isInitialized`) : `Architecte.Start` → `GameParam.LoadDebug` lit `Info/DebugLaunchSettings.txt` (`__MAPCODENAME`, `__GAMEMODECODENAME`, `__PRESETCODENAME`) → `UNetManager.LoadDebug` (host + User local, `debugLoader=true`) |

## 3. Chargement d'une map (ScenePrincipale)

### 3.1 Amorçage

| # | Classe.méthode (DLL) | Rôle |
|---|---|---|
| 1 | `Brix.Engine.Neo.Awake` (FP) | `jeu = Find("/Jeu")`, indexe les enfants (`ClasseMoi/EffaceMoi` = rangement des objets sous /Jeu) ; `Update` = file « Relais » différés ; `Start` : coroutine `UpdateEndOfFrame` |
| 2 | `Brix.Engine.Architecte.Awake` | `GetOrAddComponent` de `GameSignals`, `MusicLoop`, `TerrainLoop`, `PhysicsLoop` ; `GameOver` → `LeaveGame` |
| 3 | `Brix.Engine.FlatWorld.FlatWorldInitializer.Start` | `Init()` : active `engineProvider` (`TestWorldConfig3` : `IVoxelEngineProvider`) → `VoxelEngine.Impl = LayerVoxelEngine` ; `Voxel.SetVoxelEngine` ; biome = `GameParam.BiomeToLoad()` (`Map.biomeName` Green/Desert/Winter) → matériau ; `Refresh()` : threading, algos soft, LOD max/qualité depuis `Neo.*` |
| 4 | `Architecte.Start` (coroutine) | attend `OnFactoriesReadySig` ; `UserSettingManager.SetSettingIntoGame` ; `StaticInit` (`Livre` PieMenu_Buttons/Menus) ; `MegaVoxel.StaticInit` ; pool `TerrainData` ; **`InitLoader()`** ; connecte `VoxelChanged/BlockTypeChanged/TreeChanged/…` → `Changement/DirtyNature` |
| 5 | `Architecte.InitLoader` | `StartCoroutine(GameLoader.LoadNewGameFromGameParams(OnLoadError))` (`GameLoader` FP ; `Neo.EnumAll`/`TryEnum` BC pour exceptions) |

### 3.2 `GameLoader.Load` (FP, `decomp/Brix.Engine.GameLoader.cs`) — ordre exact [O]

| Phase | Appel | Détail / fichiers |
|---|---|---|
| — | `Load` | `LoadingStarted`, `LoadingGame(ingame)`, `MaxLoadingProgress(EstimateLoadingSteps())` |
| Build | attentes réseau | client : attend `GameParam._isInitialized` ; host : attend `User.connectionToClient.isReady` de tous ; `User.LocalUser` non nul |
| Build | `StartLua()` | `LuaLoader.LoadUserKeyBindingLuaFile` ; pour chaque `Asset_Mod` : `Disk_LoadMod` (`clientside.lua`, + `serverside.lua` si serveur) ; **gamemode** : `Asset_Gamemode.Disk_LoadGamemode(preset)` → `LuaLoader.Load(clientside.lua, LuaLibs.BigLib, ApplyPresetSettingsToScript)` puis `DoFile(serverside.lua)` si `NetworkServer.active` ; scripts tués sur `UNetManager.LateDisconnected` |
| Build | `SimpleHostScript.Run` (serveur) / `PopulateFactionsLocally` (client) | factions depuis `Gamemode.Disk_ReadFactionPrototypes` (`factions.lua`), `NetworkServer.Spawn` des `Faction`, propriétaires/relations (Allied si même équipe, Enemy sinon, `DefaultEnemy/DefaultNeutral`), puis `HostLoadedAllFactions=true` ; save : `Factions.json` (`assigned`) |
| Build | `BeforeTerrainLoaded` ; `InitNoise` | serveur : `VoxelNoiseConfigurator` (`noise.json` : `__GLOBALSEED`, `__NOISETYPES`) + `NetworkServer.Spawn` ; preset `blueCrystalDeeplayer/orangeCrystalDeeplayer/ironDeeplayer` |
| Terrain | `LoadBasicTerrain` → `VoxelEngine.Load(map.path)` → `LayerVoxelEngine.LoadMap` → `TestWorldConfig3.LoadMap` | `mapFolder=path` + `QuickDrawWorld()` ; `InitLayers` : `layersPopulator.Populate` puis `layer.Load(folder)` : `HeightmapLayer`←`Heightmap.png`, `FillRangeLayer`←`Columns.json`, `NegativeLayer`←`RemovedVoxels.json`, `TerrainSurfaceLayer`←`TerrainSurfaceLayer.json` |
| Terrain | `InitTerrainGravity` | `GeostaticityTracker.SetGeostatique` pour chaque voxel du fond naturel (10 000/frame) = ancrage statique du terrain |
| Nature | `LoadNature` | `trees.json` (tri par MegaVoxel, `TreeRegistry.Add`), `plants.json`, `boulders.json`, `iron.json`, `pyrite.json` |
| Terrain | `TerrainLoaded` ; `User.CallCmdSetTerrainLoaded` ; `BuildTerrainMeshes` → `VoxelEngine.StartRendering` → `RenderMap` → `FinishWork` | rendu complet des chunks (cf. §4.4) ; host attend `User.TerrainLoaded` de tous |
| Objets | `LoadGameObjects` (**serveur seulement**) | `LoadPlacedBlockData` (`PlacedBlocks.json` → `BlockEngine.LoadBlockData`) ; `gameobjects.json` → `GameObjectSerializer.LoadState("save", json)` → `GameObjectLoaded` par objet (entités des sauvegardes) ; `GameObjectsLoaded` |
| Fin build | `LoadGridGuide` (`Picking.WorldEditorGrid`) ; `LevelLoaded` ; `LevelFirstLoaded` (nouvelle partie) ou `LevelSaveLoaded` ; `WaitForFixedUpdate` | |
| Awake | `AwakeStep` : `WaitForVoxelEngine`, `InitPathfinding` → `TestPathfinding.Read_Coroutine` (si `Architecte.isPathfindingEnabled`) ; `TerrainReady` | |
| Start | `StartStep` : `LoadUI` ; attend `LuaMenuComponent.CoroutineCount==0` ; `LuaLoader.Load("gameplayTest.lua")` ; nouvelle partie : **`LoadGamemode`** (= Lua `hk_OnNewLevelReady`) + `LevelFirstReady`, sinon `LevelSaveReady` ; `GamemodeLoaded` | |
| Start | sync | host : `HostLoaded=true` ; client : `LevelReady` local, `CallCmdSetLevelReady`, attend `HostReady` ; host : attend `User.LevelReady` de tous → **`LevelReady`**, `HostReady=true` ; `SoundEngine.GameIsReady`, `SoundEngineReady` |

Barre de progression : `LoadingStepsCheckpoint` + `LoadingProgress/LoadingProgressLeap` → `LoadingScreenDriver`.
Erreur : `Architecte.OnLoadError` → `LeaveGame` / `StartError` (dialogue support).

### 3.3 Spawn des entités et init du gamemode Lua [O]

| Origine | Mécanisme |
|---|---|
| Sauvegarde | `gameobjects.json` (serveur) → `GameObjectSerializer.LoadState("save")` ; états `GameClock`/`LuaRegistry` (`[SerializerTarget("save")]`, `_saveData`) restaurés ; `hk_OnDeserialized` |
| Nouvelle partie | **Lua serveur** : `gamemode.OnNewLevelLoaded` (ex. `sandbox/serverside.lua`) lit `Markers.GetGroup("Player Starting Point")`, crée `fy_Crystal:New`, `fy_Bricktron:New(faction,pos)` × `startingWorkersCount`, cristaux neutres (`Crystal`) |
| Chemin C# | `LuaStaticDriver.InstantiateSelf` (`New`) → `LuaFactory.InstantiateFromCLR` → `ObjectPoolSingleton.Request` → `AffiliationComponent.faction` → `NetworkServer.Spawn(go, AssetId)` |
| Hooks (`LuaLibs.RegisterHooks`) | `hk_OnLevelLoaded`=`LevelLoaded`, `hk_OnLevelReady`=`LevelReady`, `hk_OnNewLevelLoaded`=`LevelFirstLoaded`, `hk_OnNewLevelReady`=`LoadGamemode`, `hk_OnSaveLevelLoaded/Ready`, `hk_OnTerrainReady`, `hk_OnLoadUI`, `hk_OnLeaveGame`=`UNetManager.Disconnected`, `hk_CallEndGame`=`GameOver`, … ; `ConnectLifecycle()` (Lua) = `LuaRegistry.HookScript` : `hk_Start/Update/FixedUpdate/SlowUpdate/OnEnd/On(De)Serializ*/OnServerMessage` |
| Pattern gamemode | `serverside.lua` : `Hooks.LateConnectOnce(hk_OnLevelLoaded, …)` puis `gamemode.OnLevelLoaded/OnLevelReady` ; `GetSaveData("sandbox","doneInit")` décide si `OnNewLevelLoaded` ; état sync client via `hk_OnServerMessage` (`LuaMessageHandler`) |

## 4. Boucle de jeu

### 4.1 Horloges [O]

| Horloge | Valeur | Source |
|---|---|---|
| Fixed timestep Unity | **0,02 s (50 Hz)**, max 0,333 s, particules 0,03 | TimeManager de `globalgamemanagers` (octets 0,02 / 0,3333) |
| `Time.timeScale` | 0 / 0,1 / 0,5 / 1 / 1,5 / 2 / 5 | `ClockConfig` (FP) |
| FPS cible | `Application.targetFrameRate` (réglage utilisateur) | `UserSettingManager` |
| Slow updates (`GameClock`) | ~0,047 / 0,101 / 0,227 / 0,467 / 0,983 s (scaled) ; mêmes seuils en unscaled (sauf `50` : 0,47) | `GameClock.SlowUpdate_*` |
| Tick IA | 1 tick = 1 `FixedUpdate` (50 Hz) ; `Node.WaitForTicks(n)` = n appels de `TreeRun.Run` [D] | `Behavior`, `TickCondition` |
| Envoi réseau lobby | `SendRate=0,1 s` ; canaux UNet 0 fiable / 1 non fiable (`UNetTransform` : channel 1) | `UNetManager`, `UNetTransform` |

Il n'y a **pas de tick de simulation à pas fixe séparé** : la « simulation » = `GameClock` (un seul MonoBehaviour) qui distribue Update/FixedUpdate/LateUpdate/SlowUpdate aux `IUpdatableComponent` enregistrés [O]. `GameClock` est désactivé (`enabled=false`) jusqu'à `LevelReady` (`EnableTicking`) [O].

### 4.2 Distribution par `Brix.Game.GameClock` (FP)

`UpdatableGameComponent<T>.OnEnable/Start` → `GameClock.AddOnUpdate/FixedUpdate/LateUpdate/SlowUpdate(this, SlowUpdateRate=0,2 s)` **seulement si** la sous-classe surcharge la méthode (`UpdatableProfiler.HasUpdate…`, réflexion) ; exceptions capturées par composant (`StopOnError` → `enabled=false` en FixedUpdate).

| Phase Unity | Ordre dans `GameClock` | Abonnés principaux [O] |
|---|---|---|
| `Update` | `_updateIndex++` ; `UpdateSig.SafeInvoke` ; `_SlowUpdate` (50, 100, 200, 500, 1s, puis unscaled 50…1s) | `UNetTransform` (interpolation client, `InterpolationRate=9`), `Locomotion4`, `Thinker`, `SharedFactionVision`, `Picking`/`MiniMap`/overlays UI, `EnvironmentSoundManager` |
| `FixedUpdate` | `FixedUpdateSig.UnorderedSafeInvoke` (ordre = ordre d'enregistrement, swap-remove) | **`Behavior.BrixFixedUpdate` → `Tick()`** (tous les arbres de comportement : `Labor`, `Project`, `CraftingLabor`…), `NetworkBehavior`, `UNetTransform` (envoi serveur), `RigidbodyTracker`, `DamageReceiver`, `TrackableVoxelTransform`, `AnimatorTracker` |
| `LateUpdate` | `LateUpdateSig.SafeInvoke` | `Facing`, `DistanceScale`, … |

### 4.3 Ordre des systèmes dans une frame (hors GameClock) [O sauf mention]

| Système | MonoBehaviour / méthode (DLL) | Hook | Condition |
|---|---|---|---|
| Terrain voxel | `Brix.Engine.Loop.TerrainLoop.Update` (FP) : `VoxelEngine.ApplyChanges()` → `Architecte.AppliquerChangements()` → `Architecte.UpdateJardinier()` | Update | actif après `LevelLoaded` (`DisableUntil`) |
| Rendu chunks | `TestWorldConfig3.Update` (BC) : `UpdateQuickDraw`, `Work()` (workLoad), `RedrawIfFocusLost` ; `Worker.Update` (BC, budget `WorkTime=0,02 s`/frame) ; `LodUpdater`/`ColliderSectorPopulator`/`LodRangeDriver` | Update | threads : `maxThreadCount` défaut code = 20 (valeur scène non lue) |
| Pathfinding | `Brix.Pathfinding.Test.TestPathfinding.Update` (FP) : revalide `DirtyVoxels/QueuedVoxels` après `VoxelEngine.IsWorking()` ; `Pathfinding.Update` (BC) LOD | Update | |
| Lua | `LuaRegistry.Update` : reprise des coroutines (`coroutine.resumeWhen`) → `sig_update` → `sig_slowUpdate` (≥1 s) ; `FixedUpdate` : `sig_fixedUpdate` | Update/FixedUpdate | seulement si `GameSignals.Count(LevelReady)>0` ; exceptions → `LuaLibs.LuaExceptionCatcher` |
| Relais | `Neo.Update` (délais) ; `Neo.UpdateEndOfFrame` (fin de frame) ; `PersistentCoroutineRunner` | Update / EOF | |
| Réseau | `UNetManager.FixedUpdate` (lobby uniquement : `LobbyRefresh`, `UpdateLobbyPlayersInfos`) ; `Update` : NAT servers ; UNet interne (SyncVar/Cmd/Rpc) | | |
| Ordre inter-scripts | non fixé (pas d'ordre d'exécution Unity configuré) | | [D] |

Chaîne de l'IA (serveur seulement) [O] : `MultiBehavior.ToTree()` → si `NetworkServer.active` : `Node.Parallel.ForEachIn(sequences)` sinon `Node.Forever`. `Labor.AddOrSetSequence` : `CurrentWork`(1 tick), `PendingWork`(1), **`LookForWork`(10 ticks + 2×`WaitFor.Ticks(10)`)** → `CheckProjectForWork`, `Autonomy`(1), `VoxelPresenceMaintenance`(0). `CraftingLabor` : CurrentWork 1, Queue 10, PendingWork 1. Les clients n'exécutent pas ces arbres (positions/animations via `UNetTransform`, SyncVars, Rpc) [D].

### 4.4 Rendu / maillage des chunks (BC `Brix.Engine.FlatWorld*`) [O]

| Étape | Méthode | Détail |
|---|---|---|
| Édition voxel | `Voxel.*` → `LayerVoxelEngine` (accumule `p2i/c2i/addedColumns`) | changements regroupés |
| Application | `TerrainLoop` → `VoxelEngine.ApplyChanges` → `LayerVoxelEngine.ApplyChanges` | écrit `FillRangeLayer`/`HeightmapLayer`/`NegativeLayer`, `test.SetDirty(voxel)` ; si changé : `test.DrawDirtyMegaVoxels()` (`+FinishWork` si `forceSynchronousUpdate`) |
| Dessin | `TestWorldConfig3.Draw(List<XYZ>)` | `drawer.MarkAsDrawn` ; threadé : `InitFullFromLayers_Init` + `workLoad.Enqueue(InitFullFromLayers_Loop)` (workers `FlatWorldThread`) ; sinon `_Draw` par MegaVoxel : `FlatWorld.InitFromLayers/InitPreRender/InitRender` (GPU shaders) ou `FlatWorldSoftRenderer` (CPU) → `RenderMegaVoxel` (mesh + collider) ; `lodUpdater.SetDirty` |
| Chargement initial | `QuickDrawWorld_Coroutine` | `QuickDrawAsFull` (monde 0 + LODs 1..n puis `DrawMegaVoxels`) ; `bouleDeLod3` |
| MegaVoxel | 16³ (`Architecte.GetOrCreateMegaVoxel`, `GVecteur*16`) | `Architecte.changedMegaVoxels` (flags Surface/Dommage/Trees/Plants/Minerals/Crystals) → `AppliquerMVSurface` (`MegaVoxel.List/AfficherSpecial`), `AppliquerNature` ; `Jardinier.Travailler` : ≤100 MegaVoxels/frame `CalculateNatureLod` |

### 4.5 Physique des blocs [O/D]

Unity Physics (50 Hz). Stabilité : `DefaultBlockValidator.Validate` (support `stoneSupport`, exempt si `GeostaticityTracker.IsGeostatic`) → `SetUnstable` → `Architecte.AddVoxelToFreedList` → (frame suivante, `TerrainLoop`) `AppliquerChangements` → `IBlockData.Physicalize()` (→ `RootBlockData.Physicalize`, bloc libre/rigidbody). `PhysicsLoop` : après `TerrainReady`, attend `Architecte.AuRepos()` (aucun MegaVoxel dirty, `!VoxelEngine.Working`) → `PhysicsReady` + `BlocLibre/Roche.ReactiverPhysics`.

## 5. Fin de partie / déchargement

| # | Déclencheur → appel | Effet |
|---|---|---|
| 1 | Lua `Hooks.Invoke(hk_CallEndGame)` (victoire/défaite UI) ou `AbortLoading`(+2 s) → `GameSignals.GameOver` | `Architecte` (connecté en Awake) → `LeaveGame()` |
| 1' | Menu « Leave game » : `ContainerGenericPopulator` → `Architecte.QuitToMainMenu()` | idem ; « Exit desktop » : `QuitThroughMenu` + `ApplicationQuitter.Quit` |
| 2 | `Architecte.LeaveGame` | `GameSignals.LeaveGame` ; `ProjectDatabase.ClearAll()` ; **`UNetManager.Disconnect()`** ; `editorMode=false` |
| 3 | `UNetManager.Disconnect` | stop discovery/serveur/NAT/client, `NetworkServer.ClearHandlers`, `NetworkTransport.Shutdown`, `ManuallyInitTransport` ; UNet recharge `offlineScene`=`SceneMenu` [D] ; `OnStopClient` → `Disconnected`, `LateDisconnected` (→ `Script.Kill` des scripts gamemode/mods) ; `OnClientSceneChanged` (buildIndex 1) → `Disconnect()` |
| 4 | Destruction de `ScenePrincipale` | `GameSignals.OnDestroy` : `LevelUnloaded` + `SceneDestroyed` (vide **tous** les `GameSignal`) ; `Architecte.OnDestroy` : `StopAllCoroutines`, `ClockConfig.Paused=false`, `ClearMegaVoxels`, `MegaVoxel.StaticDestroy`, **`VoxelEngine.Terminer`**, `SoundEngine.Terminer`, `Resources.UnloadUnusedAssets` ; `LuaRegistry.OnDestroy` : `sig_OnEnd` ; `LuaRegistry` vide `_saveData` à `activeSceneChanged` |
| 5 | Persistants | `UNetManager.OnLobbyStopClient` → `ResetToDefault` → `ResetPersistantNetworkSceneObjects` : `DontDestroyOnLoad.ResetObject` ré-instancie `GameParam` (clone propre, ancien détruit), `LobbyPlayerList/UserList` vidés, `Neo.multiplayer=false` |
| Sauvegarde | `Asset_Map.Disk_Overwrite/Disk_WriteAsNew` (`MoveToStaging` → `Disk_Save` → `ApplyStaging`) | écrit `meta.json`, couches voxel (`VoxelEngine.Save`), `trees/plants/boulders/iron/pyrite/TerrainDamage/PlacedBlocks/Factions/LuaMarkersCount/gameobjects.json`, `noise.json`, thumbnail ; `GameSaved` |

## 6. Diagramme de séquence (texte)

```
Unity ─ level0 SceneSplashScreen
  PersistLoader.Awake ──► Load(PersistentScene, additive)
  FactoryLoader.Start ──► Load(Factories/*, additive) ──► WaitForFactoriesLoaded
        └► unload factories ► PostProcess ► IBrixComponent.Initialize ► OnFactoriesReadySig
  SplashScreen ── ConnectOnce(OnFactoriesReadySig) ──► LoadScene(SceneMenu)
SceneMenu
  BaseMapStarterMenu.OnOk ─► GameParam.{Map,Gamemode,Preset}
  GameParam.StartGame ─► LoadingGame(menu) ─► LoadingScreenAnimEnd ─► StartLocalOnly
  UNetManager.StartHostLocalOnly ─► StartHost ─► LobbyPlayer.Start ─► SendReadyToBegin
  UNetManager.OnLobbyServerPlayersReady ─► ServerChangeScene(ScenePrincipale) + spawn User
ScenePrincipale
  Neo.Awake ► Architecte.Awake(+GameSignals,TerrainLoop,PhysicsLoop,MusicLoop) ► FlatWorldInitializer.Start(VoxelEngine.Impl)
  Architecte.Start ─► GameLoader.Load:
     Build : StartLua(mods, gamemode clientside+serverside) ► SimpleHostScript.Run(factions)
             ► InitNoise ► VoxelEngine.Load(layers) ► InitTerrainGravity ► LoadNature ► StartRendering
             ► LoadGameObjects(server) ► LevelLoaded ► LevelFirstLoaded|LevelSaveLoaded
     Awake : InitPathfinding ► TerrainReady ► (PhysicsLoop: AuRepos ► PhysicsReady)
     Start : LoadUI ► LoadGamemode(Lua OnNewLevelReady) ► GamemodeLoaded ► sync ► LevelReady
  GameClock.EnableTicking (LevelReady)
  ┌─ chaque frame ───────────────────────────────────────────────────────────────┐
  │ Update : TerrainLoop(ApplyChanges,AppliquerChangements,Jardinier) │ TestWorldConfig3/Worker │
  │          TestPathfinding │ LuaRegistry(coroutines,hk_Update,hk_SlowUpdate) │ GameClock.Update+Slow* │ Neo relais │
  │ FixedUpdate 50Hz : GameClock.FixedUpdateSig (Behavior.Tick = IA serveur, UNetTransform, trackers) │ LuaRegistry hk_FixedUpdate │
  │ LateUpdate : GameClock.LateUpdateSig │ fin de frame : Neo/Persistent relais │
  └──────────────────────────────────────────────────────────────────────────────┘
Fin : Lua hk_CallEndGame | menu Leave ─► Architecte.LeaveGame ─► UNetManager.Disconnect
      ─► (UNet) SceneMenu ─► Architecte.OnDestroy(VoxelEngine.Terminer…) ; GameParam reset
```

## 7. Fichiers decomp/ de référence

`GameParam`, `Brix.Network.UNetManager`, `Brix.Network.LobbyPlayer`, `Brix.Network.SimpleHostScript`, `Brix.Engine.GameLoader`, `Brix.Engine.Architecte`, `Brix.Engine.Neo`, `Brix.Engine.Abstract.Neo`, `Brix.Engine.GameSignals`, `Brix.Engine.VoxelEngine`, `Brix.Engine.Jardinier`, `Brix.Engine.Loop.{TerrainLoop,PhysicsLoop,MusicLoop,CinematiqueLoop}`, `Brix.Engine.FlatWorld.{FlatWorldInitializer,Worker,FlatWorldThreadPool}`, `Brix.Engine.FlatWorld.Layers.{TestWorldConfig3,LayerVoxelEngine}`, `Brix.Pathfinding.Pathfinding`, `Brix.Game.GameClock`, `Brix.Game.Components.{UpdatableGameComponent,Behavior,DefaultBlockValidator}`, `MultiBehavior`, `Brix.Game.AI.{Labor,Goal}`, `Brix.Game.Network.UNetTransform`, `Brix.Lua.{LuaRegistry,LuaLoader,LuaLibs,LuaFactory}`, `Brix.Assets.{Asset_Map,Asset_Gamemode,Asset_Mod,AssetDispensary}`, `Brix.UI.SplashScreen`, `Brix.UI.Builder.Menu.BaseMapStarterMenu`, `Brix.NewUI.LoadingScreen.LoadingScreenDriver`, `Brix.Utils.{DontDestroyOnLoad,PersistentCoroutineRunner}`, `Brix.Utils.Factories.{FactoryLoader,FactorySignalInvoker}`, `Brix.Game.Utils.PersistLoader`, `ClockConfig`. Décompilation complète de travail (hors cache) : scratchpad de session, non conservée.

## 8. Zones floues

| Sujet | Pourquoi |
|---|---|
| Transitions de scène UNet (online/offline) | Code UNet fermé : le chargement de `SceneMenu` (lobby) à `StartHost` et le retour à `offlineScene` à `Disconnect` sont déduits de la config (strings) |
| Valeurs sérialisées en scène | `SplashScreen.nextScene/skip`, `maxThreadCount`, `WorkTime` effectifs, `lodCount`, `SendRate` des `UNetTransform` : non lus (format Unity binaire) |
| Ordre Update entre MonoBehaviours | aucun ordre explicite ; ordre réel dépend d'Unity |
| `GameClock.WaitForEndOfFrame` | statique `null` dans le décompilé ; `yield return null` ≈ prochaine frame [D] (valeur éventuellement posée ailleurs) |
| Sérialisation de save | `GameObjectSerializer.SaveState/LoadState` (spawn des objets depuis `gameobjects.json`) non détaillé ; sauvegarde déclenchée par menu/Lua (appelant de `Disk_Overwrite` non tracé) |
| IA : Goal/Project/Knowledge | seulement le squelette de tick (Behavior/MultiBehavior) ; voir REPRISE.md pour le bug construction |
| Pont `Transactor`/`DeblocMessageHandler` | rôle réseau des blocs (pose/retrait) non détaillé |
| Brix.Core `AbstractBackend`, GameAnalytics, Steam | hors flux de jeu, non suivis |
