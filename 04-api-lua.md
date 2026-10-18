# 04 — API Lua (mods)

Source : décompilation complète de `Assembly-CSharp-firstpass` (`decomp/firstpass/`, ilspycmd), lecture de `Brix.Lua/*`, `Brix.Assets/*`, `Brix.Engine/GameLoader`, MoonSharp (`PlatformAutoDetector` en IL). Corpus : 505 fichiers `.lua` de `Info/Lua` (commentaires retirés) + `Info/Maps` (aucun code Lua, seulement des marqueurs). Analyse statique uniquement.

Référence brute : `04-api-lua-ref.tsv` (1259 lignes : 997 `[LuaDrivable]` + 262 globales/membres de tables enregistrés en C#). Outils : `tools/luaapi_extract.py` (C# → JSON), `tools/luaapi_ref.py` (JSON + Lua → TSV).

## 1. Surfaces d'exposition

| Couche | Mécanisme | Volume | Accès Lua |
|---|---|---|---|
| `[LuaDrivable("nom")]` | Attribut sur méthode/propriété/champ (`UseExtern` = membre pris sur le type cible) | 997 membres, 108 types (594 méthodes, 210 propriétés, 106 champs, 87 hooks) | via *driver* (composant) ou via proxy statique d'une lib (`Vector3`, `Math`…) |
| `[LuaMetaDrivable(typeof(T))]` | Classe « méta » qui déclare des drivables pour un composant Unity non modifiable | 5 : `Transform`, `RectTransform`, `Collider`, `SphereCollider`, `Light` | via driver (`x.position`, `x.lightColor`…) |
| Globales `RegisterClass(Script)` | `script.Globals[...] =` dans ~45 méthodes appelées par `LuaLibs.BigLib` | 262 noms (globales, tables `Project.*`, `Factory.*`, `CraftingStation.*`, enums, hooks) | globales directes |
| MoonSharp standard | `UserData.RegisterType` réflexion (membres publics) | `LuaMarker`, `IconKeys` (statique) ; `[MoonSharpVisible(false)]` = masquage (14) | `Markers`, `IconKeys` |

Aucun `[LuaDrivable]` hors firstpass (Assembly-CSharp, Brix.Core, brix-common : 0).

## 2. Mécanisme

### Résolution des noms (drivers)

| Étape | Code | Effet |
|---|---|---|
| Scan | `LuaDriverHQ.InitInfoList` | Parcourt tous les types `Component` de UnityEngine, UnityEngine.UI, firstpass, Assembly-CSharp (+ `LuaStaticDriver`) ; pour chaque membre `[LuaDrivable]` crée un `InfoContainer` (`LuaCall`, `LuaProperty<T,R>`, `LuaStaticProperty<R>`, `LuaField<T,R>`) |
| Table globale | `_allInfos : Dictionary<string, InfoContainer[]>` | **Espace de noms plat** : le nom Lua est la clé, toutes classes confondues |
| Handles | `LuaDriverHQ.CompileFactoryDrivers` / `AddNewDriver` | Chaque prefab de factory et chaque objet de scène porteur d'un `LuaDriver` (champ `Handle`) crée un `LuaStaticDriver` exposé en **globale** du même nom (`LuaStaticDriver.RegisterClass`) |
| Accès `x.nom` | `LuaDriverDescriptor.Index` | Parcourt les `InfoContainer[]` du nom ; retient le premier dont le type cible est assignable depuis un des `drivableComponents` du GameObject (enfants inclus si `IgnoreChildren=false`). Statiques : cible nulle, il suffit que le composant soit présent |
| Repli 1 | `SubHandles` | `x.SousHandle` → sous-driver |
| Repli 2 | `UserTable` | Clé inconnue en écriture → stockée dans la table utilisateur du driver (tables sérialisées en `Dictionary`), **persistée dans la sauvegarde** (`[JsonProperty] _userTable`) |
| Driver mort | `TargetIsNullOrReleased` | Exception `Target is null or released.` ; tester avec `IsNilOrReleased(x)` |

Handles observés dans le Lua (receveur → composant) : `UIGame` (UIGameObserver, 785 accès), `MainCamera` (CameraScript), `Picking`, `Rules` (GeneralRules + CombatRules + TaskRules + TerrainRules + CameraRules sur le même objet), `Sound` (SoundEngine), `Skybox` (Journee), `GameClock`, `Mergeback`, `GS_*` (menus LUI, LuaMenuComponent), `fy_*` (≈240 factories : `fy_Bricktron`, `fy_Crystal`, `fy_User`, `fy_RaidProject`, `fy_TunnelProject`…). Les handles sont définis dans les assets Unity, pas dans le code.

### Méthodes

Toutes les méthodes `[LuaDrivable]` ont la même signature C# : `static DynValue f(ScriptExecutionContext, CallbackArguments)` (`LuaCall` = `DynValue.NewCallback`). Il n'existe pas de méthode d'instance : l'objet est passé en argument 0. Comme `ColonOperatorClrCallbackBehaviour = TreatAsColon`, `obj:M(a)` donne `args[0]=obj, args[1]=a`, et `Lib.M(a)` donne `args[0]=a`. La colonne `params` du TSV reprend les index C# exacts (`0:driver<Faction>` = self).

| Lecture d'argument (C#) | Type Lua attendu |
|---|---|
| `AsFloat/AsInt/AsLong` | number |
| `AsString`, `AsBool`, `AsTable`, `AsType(i, DataType.Function)` | string, bool, table, function |
| `AsUserData<Vector3>` etc. | userdata créée par `Vector3.New`… |
| `AsComponent<T>` | driver dont le GameObject porte `T` (`CachedGetComponent<T>`) |
| `AsGameObject` | driver |
| `AsEnum<T>` | valeur d'enum exposée (`Occupations.X`, `TerrainType.X`…, `LuaEnumProxy<T>`) |

### Conversions

| Sens | Règle |
|---|---|
| C#→Lua `Component` | → driver (`IGameComponent.luaDriver` ou `GetComponent<LuaDriver>`), sinon nil (`LuaDriver.RegisterClass`) |
| C#→Lua `GameObject` | → driver ou nil |
| Lua→C# userdata → `Component`/`GameObject` | `CachedGetComponent(t)` / `.gameObject` |
| Types valeur Unity | `LuaObjectDescriptor<T>(typeof(LuaT))` : `Vector2/3/4`, `Quaternion`, `Color`, `Matrix4x4`, `Texture2D`, `Material`, `Mesh`, `Factory.AssetKey`, `IVoxel`, `Random`, `Sprite` ; opérateurs via drivables `__add`, `__sub`, `__mul`, `__eq` |
| Propriétés/champs | `DynValue.FromObject` en lecture, `value.ToObject<R>()` en écriture |
| Enums | globales `Occupations`, `Jobs`, `Diggables`, `TaskType`, `Relation`, `DamageType`, `SelectionType`, `DriverTag`, `NatureType`, `TerrainType`, `TextureFormat`, `CrystalColor`, `Recipe`, `Resource`, `LUI2.Enums.*` |
| Listes | `FromObject(List<…>)` → table Lua |

### Événements C#→Lua (hooks)

| Élément | Détail |
|---|---|
| Objet | `LuaHook`, `LuaHook<T1..T4>` = enveloppe d'un `Signal` Brix. Exposé en globale (`hk_*`) ou en membre `[LuaDrivable("hk_…")]` d'un driver (87 membres) |
| API | `Hooks.Connect(hook, fn[, dishook])`, `Hooks.ConnectOnce`, `Hooks.LateConnect(Once)` (hook non générique seulement), `Hooks.Disconnect(hook, fn)`, `Hooks.Invoke(hook, {args})` |
| Arguments | convertis par `DynValue.FromObject` (composants → drivers) |
| Nettoyage | auto-déconnexion à la mort du script (`hk_OnScriptKilled`) ou au déclenchement de `dishook` |
| Appels C# de fonctions Lua par nom | **Aucun.** Le C# ne lit que `sv_Settings` (table), la valeur `return` de `factions.lua` et `map_validator.lua`, et les `LiveValue` LUI. Une fonction `OnLevelReady` n'est appelée que si elle est connectée à un hook |

Hooks globaux (`LuaLibs.RegisterHooks`) : `hk_OnFactoriesReady`, `hk_OnBeforeTerrainLoaded`, `hk_OnGameObjectsLoaded`, `hk_OnLevelLoaded`, `hk_OnLevelReady`, `hk_OnNewLevelLoaded`, `hk_OnNewLevelReady` (= `GameSignals.LoadGamemode`), `hk_OnSaveLevelLoaded`, `hk_OnSaveLevelReady`, `hk_OnTerrainReady`, `hk_DisclaimerAcknowledged`, `hk_OnLaborCreated(Labor)`, `hk_OnLaborDeleted(Labor)`, `hk_OnPlayerLost(User)`, `hk_OnFactoryInstantiated(AssetKey, driver)`, `hk_OnAdditiveMapLoaded`, `hk_CallEndGame`, `hk_OnGameModeTimersStarted`, `hk_OnSceneChanged`, `hk_OnLeaveGame`, `hk_OnLoadUI`, `hk_NewWave(int)`, `hk_OnVictory`, `hk_OnDefeat`.
Hooks de cycle de vie (après `ConnectLifecycle()`, `LuaRegistry.HookScript`) : `hk_Start`, `hk_Update`, `hk_FixedUpdate`, `hk_SlowUpdate` (1 s), `hk_OnEnd`, `hk_OnSerializing`, `hk_OnSerialized`, `hk_OnDeserializing`, `hk_OnDeserialized`, `hk_OnServerMessage(table)`. Update/FixedUpdate ne tournent qu'après `LevelReady`.
Par type de factory : `fy_X.hk_OnNew(driver)`, `fy_X.hk_OnTypeDestroy(driver)`.

## 3. Hooks implémentés par les gamemodes et mods

| Hook | Usages | Gamemodes |
|---|---|---|
| `hk_OnLevelReady` | 17 | tous (9) |
| `hk_OnServerMessage` | 33 | conquest, creditisland, grabthegems, invasion, sandbox, tutorial |
| `hk_SlowUpdate` | 19 | idem |
| `hk_OnLevelLoaded`, `hk_OnLeaveGame` | 8, 7 | idem + world_editor |
| `hk_OnNewLevelReady`, `hk_OnLoadUI`, `hk_OnGameModeTimersStarted` | 7 chacun | 6 gamemodes |
| `hk_OnSerializing` | 6 | sauvegarde de l'état Lua (`SetSaveData`) |
| `hk_CallEndGame`, `hk_OnVictory`, `hk_OnDefeat` | 9, 3, 3 | conquest, grabthegems, invasion, tutorial |
| `UIGame.hk_OnOccupationChanged`, `UIGame.hk_OpenModalMenu/CloseModalMenu`, `UIGame.hk_SetSidebarSpacer` | 35, 13/16, 5 | UI des modes |
| `fy_*.hk_OnNew` / `hk_OnTypeDestroy` | ~20 | invasion, tutorial, grabthegems (Corruptron, Minitron, Biftron, Magitron, RaidProject, CrystalPatch) |
| Membres de drivers : `crystal.hk_OnSetFaction`, `hk_OnExplosion(Faction)`, `homeCrystal.hk_OnSetFireflyCount`, `_capturable.hk_OnSetCapture*`, `fy_Corruptron.hk_OnKilled/OnEnteredFog/OnLeftFog`, `raid.hk_OnAddLabor`, `Rules.hk_OnSetCanCaptureCapturables`, `FactionStorage.hk_OnSetContent`, `Voxel.hk_OnTerrainRemoved` | 1–11 | conquest, sandbox… |
| `hk_OnFactoryInstantiated`, `hk_OnAdditiveMapLoaded`, `hk_Update` | 3, 2, 1 | rttt |

`hk_OnHostLoaded` et `hk_OnHostMessage` (rttt) ne sont enregistrés nulle part en C# : code mort.

Données lues par le C# dans un gamemode : `meta.json` (`__CODENAME`, `__DISPLAYNAME`, `__PLAYABLECONTEXT`, `__GAMEMODE_ORDER`), globale `sv_Settings` (remplie par le preset), `factions.lua` (`return {…}` → `FactionPrototype`), `map_validator.lua` (`return results`, reçoit `__LUAMARKER_VALIDATION_LIST__` et `Markers`), `victory.png`/`defeat.png`/`intro.png`. `Descriptor.json` n'est pas lu par le C#.
`Info/Maps` : pas de Lua ; chaque carte fournit `LuaMarkersCount.json` (`markerTag`, `markerGroups` : `PlayerStartingPoint`, `CorruptronSpawnPoint`…) consommé via `Markers.GetGroup/GetTagged` et les validateurs.

## 4. Chargement et points d'entrée d'un mod

| Type | Emplacement | Chargement |
|---|---|---|
| **Mod** | `Info/Lua/Mods/<nom>/` : `meta.json` + `clientside.lua` et/ou `serverside.lua` | `GameLoader.StartLua` : à **chaque** partie, **tous** les mods trouvés (pas d'activation ; désactiver = renommer, cf. `halloween2016/*.lua.off`). `clientside.lua` sur toutes les machines ; `serverside.lua` seulement si `NetworkServer.active` (hôte/solo), exécuté dans le même `Script` que clientside |
| Gamemode | `Info/Lua/Gamemodes/<code>/` (+ `Info/UGC/Gamemodes`, `Info/Downloads/Gamemodes`, Workshop) | même schéma clientside/serverside, puis `sv_Settings` écrasé par les clés du preset (après exécution de clientside, avant serverside) |
| Preset | `Info/GMPresets/`, `<gamemode>/Presets/*.json`, UGC, Workshop | `__PRESET_GAMEMODE`, autres clés → `sv_Settings` |
| Bindings | `Info/Lua/userconfigs.lua` | chargé avant les mods, globales `Bind/Unbind/UnbindAll` réelles |
| Autres | `gameplayTest.lua` (après `LoadUI`), menus LUI (`LuiMenu.RootPath`) | — |

Ordre dans `GameLoader.Load` : `StartLua` (userconfigs → mods → gamemode) → `BeforeTerrainLoaded` → terrain → `TerrainLoaded` → `GameObjectsLoaded` → `LevelLoaded` → `LevelFirstLoaded`/`LevelSaveLoaded` → pathfinding → `TerrainReady` → `LoadUI` → `gameplayTest.lua` → `LoadGamemode` (`hk_OnNewLevelReady`) + `LevelFirstReady` ou `LevelSaveReady` → `GamemodeLoaded` → `LevelReady` (`hk_OnLevelReady`, client ; l'hôte attend les joueurs). Un mod doit donc tout faire dans des hooks ; à l'exécution du chunk principal, la carte n'est pas chargée.

Squelette minimal (déduit de `nothing/clientside.lua` et `halloween2017`) :

```lua
-- Info/Lua/Mods/monmod/clientside.lua   (+ meta.json : {"__CODENAME":"monmod","__DISPLAYNAME":"Mon mod","__ASSETVERSION":1,"__PLAYABLECONTEXT":7})
ConnectLifecycle()                       -- active hk_Update/hk_SlowUpdate/hk_OnSerializing…
Hooks.Connect(hk_OnLevelReady, function()
  Rules.bricktronCap = 50
  for _, c in pairs(fy_Crystal:GetAll()) do print(c.name) end
end)
```

`require`/`dofile` (`BrixScriptLoader`) : dossier du script, puis `Info/Lua/?`, `Info/Lua/LUI/?`, `Info/Lua/Libs/?`, `?`, chemins relatifs au dossier du jeu. Cache invalidé par date de modification.

## 5. Limites

| Limite | Détail |
|---|---|
| Isolation | Un `Script` MoonSharp par mod/gamemode/menu : pas de globales partagées. Communication : hooks C#, `SetSaveData/GetSaveData(ns, clé, val)`, `UserTable` des drivers |
| Modules Lua | `new Script()` = `CoreModules.Preset_Default` : base, string, table, math, bit32, coroutine, metatables, `load/loadfile/dofile/require`, `os` (time + system), `io`, `json`, `dynamic`. **Pas de `debug`**. Syntaxe MoonSharp : lambdas `\|x\| expr` |
| Sandbox | **Aucune.** MoonSharp choisit `LimitedPlatformAccessor` sous Unity, mais le build embarqué force `IsRunningOnUnity = false` (IL : `ldc.i4.0 … and`) → `StandardPlatformAccessor` : `io.open`, `os.execute`, `os.remove` actifs (déduction statique, non vérifiée à l'exécution). Un mod/gamemode Workshop a donc accès fichiers et processus |
| Réseau | `hk_OnServerMessage` : hôte → clients uniquement, table ≤ 1024 octets, sérialisée par `BinaryFormatter` (désérialisation côté client = surface d'attaque pour un hôte malveillant) |
| Surface | Uniquement les membres listés ; la logique IA (`Labor`, `Goal`, `Task`) n'est pas pilotable au-delà de quelques réglages (`SetAutonomy*`, `Project.maxSize`, `AssignLabor`) |
| Résolution par nom | Homonymes entre classes : le premier composant compatible gagne |
| Mods hors `Info/Lua/Mods` | `Asset_Mod._luaRelativePath` = `path.Substring(IndexOf("Info/Lua/")+9)` : cassé pour `Info/UGC/Mods`, `Info/Downloads/Mods` et le Workshop (aucun `Info/Lua/` dans le chemin) |
| Bugs repérés | `hk_OnSerialized` branché sur `sig_serializing` ; `coroutine.resumeWhen` posé comme globale nommée littéralement `"coroutine.resumeWhen"` ; `GetIntroImagePath` renvoie `defeat.png` ; repli `LUA_PATH` passe la valeur de `LUA_PATH` comme nom de fichier ; `Bind/Unbind/UnbindAll` = no-op hors `userconfigs.lua` ; `BeginSample/EndSample/EndProfiling` = no-op |
| Support officiel | Readme halloween2017 : « modding is not yet officially supported » |

## 6. Usage et membres utiles

Comptage = occurrences `recv.nom` / `recv:nom` dans le Lua sans commentaires, définitions Lua exclues. Colonnes `homonyms` (autres membres C# de même nom) et `luadef` (définitions Lua de même nom) signalent les comptes ambigus.

| Statistique | Valeur |
|---|---|
| `[LuaDrivable]` jamais référencés | 404/997 (244 méthodes, 96 propriétés, 37 champs, 27 hooks) |
| dont libs maths | `Math` 46/47 (le Lua utilise `math.*`), `Vector3` 23/35, `Quaternion` 17/21, `Matrix4x4` 18/19, `Color` 20/30 ; les `__add/__sub/__eq/__mul` sont appelés implicitement par les opérateurs |
| dont gameplay | `UIGameObserver` 39, `CameraScript` 11, `Picking` 11, `LuaInput` 11/11, `Faction` 9, `RaidProject` 8, `GeneralRules` 8 (refunds, `fogOfWarDistance`), `LuaStaticDriver` 7, `Labor` 6 (`SetAutonomy*`, `hk_OnEnter/ExitCombat`), `TerrainRules` 6 (`CanDigAt`…), `Project` 6 |
| Globales C# jamais référencées | 89/262 : `Factory.Instantiate/PlaceBlock/PlaceBlueprint/PlaceObject/Explosion`, `SwitchToGamemode`, `FlushDrivers/FlushTags`, `SetAchievement`, `Set/GetFog*`, `TreePresentAt/BlockPresentAt`, `Physics`, `Input`, `Brush.Grow*`, `hk_OnLaborCreated/Deleted`, `hk_OnPlayerLost`… |

Membres les plus utiles pour un mod (usages, exemple) :

| Domaine | Membres |
|---|---|
| Énumérer / créer | `fy_X:GetAll()` (123 occ. dont `fy_Crystal` 29, `conquest/serverside.lua`), `GetAllOfFaction`, `Count`, `GetFirst`, `fy_X:New(faction, pos[, rot])` (`LuaStaticDriver.InstantiateSelf`), `fy_X.__Factory`, `Factory.*`, `driver:Destroy()` |
| Objets | `x.name`, `x.type`, `x.position/rotation` (méta Transform), `x.faction`, `x.isSelected`, `x.isInDanger`, `Occupation.GetOccupation`, `Toolbag:EquipTool`, `Recepteur:ContainsSome` |
| Joueurs / factions | `fy_User.localUser`, `user.alliedFaction`, `Faction.FindByName`, `faction:HasOwner()`, `IsAlliedTo/IsEnemyTo`, `factionColor`, `isAI`, `Faction.defaultNeutral/defaultEnemy` |
| Règles | `Rules.bricktronCap`, `canCaptureCapturables`, `builderCanCapture`, `playerRelations`, `proportionalRefund`, `allowCrafting`, `CombatRules.*Multiplier`, `magicArrows`, `Rules:SetAllowedTasks{}`, `Rules:SetDigRules{}`, `Rules:SetExplosionRules{}`, `SetMaximumPopulation` |
| Caméra / monde | `MainCamera:LookAt(v)`, `SetRotation`, `targetLocation`, `Skybox.dayPause`, `Random.VoxelAround`, `Voxel.*`, `Markers.GetGroup` |
| UI | `UIGame.RegisterAction/UnregisterAction` (121), `UIGame.GetSprite`, `IconDB:Get64`, `DisplayDialog`, `GS_*:UnloadMenu()`, `Sound.Play` (41), `TooltipBuilder` |
| Temps / état | `Timer` (`running`, `Check`), `hk_SlowUpdate`, `SetSaveData/GetSaveData`, `GameClock` |
| Assets | `Mesh.LoadOBJ`, `Texture.New`, `Material.New/SetOnDriver/SetOnAssetKey`, `AssetKey.New` (cf. `Mods/halloween2017`) |

Filtres utiles sur le TSV : `awk -F'\t' '$1=="LuaDrivable" && $13==0'` (jamais utilisés), `$5=="hook"` (hooks), `$2 ~ /^global/` (libs).
