# 02 — Assembly-CSharp-firstpass.dll

Source : `tools/clrmeta.py` (métadonnées CLI). Dumps : `dumps/Assembly-CSharp-firstpass.types.tsv` (types/méthodes), `dumps/Assembly-CSharp-firstpass.attrs.tsv` (attributs par type/méthode/champ/propriété).
6,2 Mo · 5191 types (1832 imbriqués) · 42 940 méthodes. Statique, sans décompilation.

## Composition (types de premier niveau)

| Bloc | Types | Contenu |
|---|---|---|
| Code jeu `Brix.*` | ~2060 | gameplay, UI, input, Lua |
| Tiers embarqué | ~730 | Steamworks.NET, Rewired, I2 Localization, LibNoise, PostProcessing, Wwise (`Ak*`), GameAnalytics, UnityTest |
| Namespace global | ~400 | mélange : jeu (`GameParam`, `UIGameObserver`, `CaptureClock`, `Journee`, `SaveUpdater10to11…21to22`) + effets d'image + Wwise |

## Code jeu par namespace

| Namespace | Types | Rôle |
|---|---|---|
| `Brix.Game.AI` | 297 | IA des Briques : `Navigation`, `Goal`, `Profession`, `Project`/`RaidProject`/`TunnelProject`, `Labor`, `Knowledge`, `Toolbag`, dégâts |
| `Brix.Game.Components` | 278 | Entités : `GameComponent` (base), blueprints, crafting (`CraftingStation`), catapulte, pièges, lucioles… |
| `Brix.UI` / `Brix.NewUI` | 491 / 71 | Menus (lobby, SP/MP starter), tooltips, panneaux, réglages des modes de jeu |
| `Brix.Input` | 146 | `CameraScript`, `Picking`, `InputModeController` |
| `Brix.Engine` (ext.) | 103 | `GameSignals`, `FlatWorldInitializer`, extensions du moteur Brix.Core |
| `Brix.Game.Anim` / `Locomotion` | 60 / 39 | Animation, déplacement |
| `Brix.Lua` | 60 | **Pont C#↔Lua (MoonSharp)** |
| `Brix.Game.Network` | 39 | `Server`, `ServerGame`, `SteamManager`, `UNet*Cmd` (commandes réseau), trackers de parenté |
| `Brix.Transactions` | 59 | Transactions gameplay (extension de Brix.Core) |
| `Brix.Game.Rules` | 8 | `GeneralRules`, `CombatRules`, `TerrainRules` |
| `Brix.Legacy` | 91 | Ancien code (portes, cristaux, rendu) |

## Orchestration d'une partie

| Élément | Rôle observé (noms de méthodes) |
|---|---|
| `GameParam : NetworkBehaviour` | Paramètres de partie synchronisés : `Map`, `Gamemode`, `Preset`, factions/équipes/groupes, `StartGame`, `StartLocalOnly`, écran de chargement, `NetworkHostReady`, checksum de carte |
| `Brix.Network.LobbyPlayer : NetworkLobbyPlayer` | Joueur dans le lobby UNet |
| `Brix.Game.Network.Server` / `ServerGame` | Côté serveur |
| `Brix.Assets.Asset_Gamemode` / `Asset_GamemodePreset` | Chargement des modes de jeu (`Info/Lua/Gamemodes/*`) et de leurs presets |
| `Brix.Engine.GameSignals` | Bus de signaux globaux (Connect/Invoke/Once) |
| `SaveUpdater10to11` … `21to22` | Migration des sauvegardes, format v10 → v22 |

Pas de « GameManager » unique : le flux est **menu → `GameParam` (réseau) → `StartGame` → chargement map + gamemode Lua**. Le multijoueur utilise UNet (les UNet* sont des `NetworkBehaviour`), même en solo (`StartLocalOnly`).

## Pont Lua

| Classe | Rôle |
|---|---|
| `LuaLoader : MonoBehaviour` | Charge les scripts : `Load`, `LoadString`, `LoadPureBytecode`, `LoadUserKeyBindingLuaFile` |
| `BrixScriptLoader : IScriptLoader` | Résolution des `require` / chemins de fichiers MoonSharp |
| `LuaLibs` | Construit l'environnement : `RegisterUserData`, `RegisterClrToScript`, `RegisterScriptToClr`, `RegisterHooks`, `RegisterUI`, `RegisterUnity`, `RegisterInputs`, `RegisterLoading`, `RegisterPerf`, `RegisterHacks`, `BigLib` ; (dé)sérialisation de tables |
| `LuaRegistry : MonoBehaviour` | Boucle : `Update`/`FixedUpdate` → reprise des coroutines Lua ; hook/unhook de scripts ; sauvegarde de l'état Lua dans la save (`GetSaveData`/`SetSaveData`) |
| `LuaDriverHQ` | Registre des « drivers » : scanne les composants `[LuaDrivable]`, compile les drivers par factory |
| `LuaDriver : GameComponent` | Proxy Lua d'une instance (nom, type, sélection, matériau, mesh, table utilisateur) |
| `LuaStaticDriver` | Proxy Lua d'un **type** : `GetAll`, `GetAllOfFaction`, `GetFirst`, `Count`, `InstantiateSelf`… |
| `LuaHookBase` / `LuaHook<T1..T5>` | Événements C#→Lua : `Connect`, `ConnectOnce`, `LateConnect`, `Disconnect`, `Invoke` |
| `LuaMath`, `LuaVector2/3/4`, `LuaQuaternion`, `LuaMatrix4x4`, `LuaColor`, `LuaRandom`, `LuaTimer`, `LuaPhysics`, `LuaTexture`, `LuaMesh`, `LuaMaterial` | Bibliothèques de types exposées |
| `LuaDebugger`, `LuaPerf` | Debug et profiling |

**Surface exposée :** `[Brix.Lua.LuaDrivable]` est posé sur **997 membres** (594 méthodes, 212 propriétés, 191 champs) répartis sur **108 types**. `[MoonSharpVisible]` est utilisé une quinzaine de fois.

Principaux types exposés (nombre de membres) :

| Type | # | Type | # |
|---|---|---|---|
| `UIGameObserver` | 183 | `Brix.Game.Faction` | 24 |
| `Brix.Lua.LuaMath` | 47 | `Brix.NewUI.Tooltips.TooltipBuilder` | 17 |
| `Brix.Input.CameraScript` | 35 | `Brix.Game.Components.FireflyNest` | 17 |
| `Brix.Game.Rules.GeneralRules` | 33 | `Brix.Game.Semantique.Recepteur` | 14 |
| `Brix.Game.Compounders.BlueprintCompounder` | 28 | `Brix.Game.AI.Project` / `RaidProject` | 13 / 10 |
| `Brix.Input.Picking` | 27 | `Brix.Game.Rules.CombatRules` | 11 |

La liste complète s'obtient avec `grep -P '\tBrix\.Lua\.LuaDrivable$' dumps/Assembly-CSharp-firstpass.attrs.tsv`.

## Constats

1. La logique de partie (règles, IA, factions, raids) est en C#. Le Lua orchestre : modes de jeu, UI, événements, avec accès aux objets via drivers et hooks.
2. Ce qui est moddable en Lua se limite aux 997 membres `[LuaDrivable]` + hooks + libs `Lua*`. C'est la cible de la future étape « API mods ».
3. `Brix.Compile.Lua` (Brix.Core) n'est qu'un évaluateur de la console de debug. Le vrai pont est `Brix.Lua` dans firstpass.
4. Les réglages de gameplay les plus exposés sont `GeneralRules`, `CombatRules` et `TerrainRules`.
