# 01 — Architecture (périmètre : Assembly-CSharp, brix-common, Brix.Core)

Source : métadonnées CLI (REA `inspect_managed_artifact` + `tools/clrmeta.py`). Dumps : `dumps/*.types.tsv` (type, kind, base, méthodes).
Statique uniquement, aucun code décompilé à ce stade.

## Assemblies

| DLL | Taille | Types | Méthodes | Rôle |
|---|---|---|---|---|
| Assembly-CSharp | 8 Ko | 6 | 17 | Résidus legacy : `DistantCloud*`, `MouseLook`, `Steering`, `TestSceneLoader` |
| brix-common | 11 Ko | 10 | 48 | Extensions de collections (Slinq), `PoolableObject<T>` |
| Brix.Core | 1,4 Mo | 1208 | 8598 | Moteur voxel « Brix » |
| *Assembly-CSharp-firstpass* | *6,2 Mo* | *5191* | *42940* | *Hors périmètre — contient vraisemblablement le gameplay et le pont Lua* |

Dépendances Brix.Core : UnityEngine, UnityEngine.Networking (UNet), Newtonsoft.Json, log4net, Smooth.Foundations, brix-common, Boo.Lang(.Compiler), Mono.CSharp, UnityScript.
**Aucune référence à MoonSharp** dans les 3 DLL du périmètre.

## Brix.Core par sous-système

| Namespace | Types | Classes clés | Fonction |
|---|---|---|---|
| `Brix.Engine` | 31 | `VoxelEngineImpl` (104 m.), `VoxelHandle`, `XYZ`/`XZ`/`XY`, `IntegerRotation`, `MondeAtlas`, `TerrainLodProvider` | API voxel : get/set type/volume, colonnes, surface, raycast, Load/Save/RenderMap |
| `Brix.Engine.FlatWorld` | 39 | `FlatWorld` (125 m.), `Soft` (95), `Shaders` (57), `Tex` (60), `Worker`, `FlatWorldThreadPool`, `LodUpdater`, `ColliderUpdater` | Représentation monde en textures 2D (heightmap, colonnes, profondeur, normales, UV) ; deux backends : GPU (`Shaders`) et CPU (`Soft`) ; workers threadés ; LOD ; colliders |
| `Brix.Engine.FlatWorld.Layers` | 19 | `Layer` (53), `LayerVoxelEngine : VoxelEngineImpl`, `HeightmapLayer`, `NegativeLayer`, `FillRangeLayer`, `TerrainSurfaceLayer`, `LowResLayer` | Monde = empilement de couches : heightmap + ajouts positifs + retraits négatifs (cf. `Maps/*/` : Heightmap.png + blocs posés + voxels retirés) |
| `Brix.Engine.Volumes` | 18 | `VoxelBox/Sphere/Cylinder`, `VoxelVolumeUnion/Intersection/Subtraction`, `VoxelRotatedBox` | CSG de volumes voxel |
| `Brix.Pathfinding` (+Collections, Map) | 63 | — | Pathfinding des Briques |
| `Brix.Network` | 24 | `AbstractBackend : MonoBehaviour`, `Reseau*Info` (PlanMove, Equip, Aim, Path, Damage, Disconnection…) | Messages réseau gameplay + client HTTP backend (auth/token) ; transport UNet/RakNet |
| `Brix.Components` | 5 | `BrixComponent`, `NetworkBrixComponent : NetworkBehaviour`, `BrixSingleton<T>` | Base MonoBehaviour avec cycle de vie (Requested/Released/Destroyed + signaux) |
| `Brix.Lifecycle.Pooling` | 13 | `ObjectPoolSingleton`, `GameObjectPool` | Pooling de GameObjects |
| `Brix.External.Factories` | 19 | `Factory` (51), `FactoryImprint`, `GameObjectRegistry` | Instanciation par templates/prefabs + AssetBundles |
| `Brix.External.Signals` | 66 | — | Bus de signaux/événements |
| `Brix.IO.Saves` / `Serialization` | 64 | `JObjectFetcher/Modifier/Injector`, convertisseurs Json | Sauvegardes JSON (Newtonsoft) avec patch par chemins |
| `Brix.Transactions` | 8 | `Transaction`, `TransactionMonitor`, `TransactionPayload` | Opérations asynchrones avec état succès/échec |
| `Brix.Compile` + `Brix.Shell` | 22 | `CSharpEvaluator`, `BooEvaluator`, `UnityScriptEvaluator`, `Lua.Eval`, `GameShell` (OnGUI) | Console de debug in-game : évaluation C#/Boo/JS/Lua à chaud |
| `Brix.Core` | 11 | `IJoueur`, `ITerrainInfo`, `SerializableDictionary` | Interfaces de base |

## Constats

1. Le « moteur » du jeu est Brix.Core : voxels, rendu, LOD, pathfinding, réseau, sauvegardes. Le vocabulaire mêle français (`Monde`, `Reseau`, `Joueur`, `Couche`, `Soft`) et anglais.
2. Le terrain n'est pas stocké en voxels 3D bruts : c'est un monde en colonnes (heightmap + couches positives/négatives) encodé en textures et rendu par shaders, avec un repli CPU (`Soft`).
3. `Brix.Compile.Lua.Eval` existe mais Brix.Core ne référence pas MoonSharp : soit l'appel passe par réflexion, soit le pont Lua est ailleurs.
4. `Assembly-CSharp.dll` est quasi vide. Le code gameplay compilé est dans `Assembly-CSharp-firstpass.dll` (5191 types), hors périmètre actuel.
