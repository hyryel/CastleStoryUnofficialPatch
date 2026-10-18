# BigRE — Reconstruction de Castle Story en projet Unity compilable

## 0. Mission

Produire, à partir de l'installation Steam de Castle Story, un **projet Unity 5.6.5f1** qui :
1. s'ouvre dans l'éditeur sans erreur de compilation ;
2. contient tous les assets récupérables (meshes, textures, matériaux, animations, prefabs, scènes, UI, Lua, données) ;
3. produit un build qui démarre, charge une carte et permet une partie solo comparable à l'original.

Usage strictement personnel, d'étude et d'interopérabilité (modding). **Aucune redistribution** du projet, des assets ou des binaires tiers (Wwise, Steamworks, Rewired…).

## 1. Règles de travail (obligatoires)

| Règle | Détail |
|---|---|
| Style | Court, précis, ton expert. Récapitulatifs en tableaux. Pas de politesse |
| Question ≠ ordre | Une question appelle une réponse, pas une action |
| Validation | Soumettre le plan de chaque phase ; exécuter sur ordre |
| Fichiers du jeu | **Lecture seule.** Tout export va dans le dossier de travail, jamais dans le dossier du jeu |
| Patch existant | L'installation contient `Assembly-CSharp-firstpass.dll` **patchée** (BuildGoalFix). Source de vérité pour le code : `Castle Story_Data/Managed/Assembly-CSharp-firstpass.dll.orig` |
| Preuves | Marquer chaque affirmation [O] observé (fichier:ligne, donnée) ou [D] déduit |
| Dynamique | Frida/gdb uniquement sur ordre |
| Juridique | Ne rien publier ni pousser vers un service externe |

## 2. Environnement connu

| Élément | Valeur |
|---|---|
| Jeu (Windows) | `E:\SteamLibrary\steamapps\common\Castle Story` |
| Jeu (Linux, autre poste) | `/games/steamapps/common/Castle Story` |
| Dossier RE existant | `<jeu>/re/` (docs `01`→`06`, `REPRISE.md`, `tools/`, `decomp/`, `dumps/`, `patch/`) |
| Moteur | Unity **5.6.5f1**, scripting **Mono** (pas d'IL2CPP), .NET 3.5 |
| Réseau | UNet (NetworkLobbyManager) ; le solo est un host local |
| Lua | MoonSharp (dans firstpass, `Brix.Lua`) |
| Audio | Wwise (`Ak*`) |
| Entrées | Rewired |
| Plateforme | Steamworks.NET |
| UI | Unity UI + LUI (Lua) ; présence de `content_shell.pak` / ressources Blink → UI web embarquée probable (**à vérifier**) |
| Outils dispo | .NET SDK (10 sous Windows, 8 sous Linux), ilspycmd 9.1 (Linux), Python 3.10, MCP REA (pas de décompilation .NET) |

## 3. Ce qu'on sait déjà (ne pas refaire)

| Sujet | Où |
|---|---|
| Architecture Brix.Core (voxel en colonnes, FlatWorld GPU/CPU, couches, pooling, saves JSON) | `re/01-architecture.md` |
| Composition de firstpass (5 191 types, 42 940 méthodes), pont Lua | `re/02-firstpass.md` |
| Boot, scènes (ordre du build), GameLoader, horloges, fin de partie | `re/03-moteur.md` |
| API Lua des mods (1 259 entrées) | `re/04-api-lua.md`, `re/04-api-lua-ref.tsv` |
| IA de construction, logistique | `re/05-*.md`, `re/06-*.md` |
| Décompilé complet de firstpass | `re/decomp/firstpass/` |
| Parseur de serialized file v17 | `re/tools/unity_adjectives_dump.py` |
| Métadonnées des 4 DLL | `re/dumps/*.tsv`, `re/tools/clrmeta.py` |

Scènes (ordre du build) : 0 `SceneSplashScreen`, 1 `SceneMenu`, 2 `ScenePrincipale`, 3 `PersistentScene`, puis ~20 `Factories/*Factory.unity`, chargées en additif au boot puis déchargées.

## 4. Outils à installer (hors dossier du jeu)

| Outil | Usage | Remarque |
|---|---|---|
| Unity Editor **5.6.5f1** (Unity Hub / archive) | Projet cible | Version exacte, pour éviter les migrations de format |
| AssetRipper (version stable récente) | Export complet en projet Unity | Choisir le mode d'export « projet Unity » ; scripts en mode « DLL » d'abord, voir §6 |
| ILSpy / ilspycmd | Décompilation en projets `.csproj` | `ilspycmd -p -o <dir> <dll>` |
| Outils Wwise (extraction `.bnk` / `.wem`, ex. bnkextr + vgmstream ou ww2ogg) | Audio | Conversion vers `.ogg`/`.wav` |
| Python 3 | Scripts de contrôle | Lancer avec `-I` sur des fichiers non fiables |

Tout téléchargement va dans un **dossier vide dédié** ; les scripts de travail sont dans un autre dossier.

## 5. Arborescence de travail

```
BigRE/
  00-inventaire/        listes de fichiers, sha256, versions
  01-ripper/            sortie brute AssetRipper (ne pas modifier)
  02-decomp/            projets C# décompilés (ILSpy), un par DLL
  03-unity/             projet Unity 5.6.5f1 final
  04-audio/             bnk/wem extraits + conversions
  05-shaders/           shaders reconstruits + notes
  journal.md            décisions, écarts, état par phase
```

## 6. Phases

Chaque phase : plan soumis → ordre → exécution → tableau de résultats dans `journal.md`.

### Phase 0 — Inventaire (modèle léger)
- Lister `Castle Story_Data/` : `level*`, `sharedassets*`, `resources.assets`, `*.resS`, `StreamingAssets`, `Plugins`, `Managed`.
- sha256 de chaque DLL de `Managed`. Rappel : firstpass d'origine Windows = `6f11399f…` (`.orig`), Linux = `a5bad8e3…`.
- Identifier l'UI web : qui charge `content_shell.pak`, et depuis quelle DLL ou quel exécutable (`castlestory-launcher.exe` probable → peut-être le seul launcher, **à vérifier**).
- Lister les plugins natifs (Wwise, Steam, Rewired, lzip) et leurs versions.

Livrable : `00-inventaire/inventaire.md`.

### Phase 1 — Export AssetRipper (modèle léger)
- Entrée : copie du dossier du jeu avec `firstpass.dll.orig` remis à la place de la DLL patchée (dans la copie seulement).
- Exporter en projet Unity dans `01-ripper/`.
- Contrôle : nombre de prefabs, scènes, meshes, textures, AnimationClip, AnimatorController ; scènes Factories présentes.

### Phase 2 — Code C# recompilable (cœur du travail)
Deux stratégies, à tenter dans cet ordre :

| Stratégie | Principe | Quand |
|---|---|---|
| **A. DLL d'origine** | Garder `Assembly-CSharp-firstpass.dll`, `Brix.Core.dll`, `brix-common.dll`, `Assembly-CSharp.dll` comme **plugins managés** dans `Assets/Plugins/`. AssetRipper relie les MonoBehaviour aux types de ces DLL | Premier jalon : projet qui s'ouvre et se lance vite |
| **B. Source** | Décompiler chaque DLL en `.csproj` (ILSpy), corriger jusqu'à compilation, remplacer les DLL par les sources | Ensuite, pour un projet modifiable |

Pour B :
1. `ilspycmd -p -o 02-decomp/<dll> <dll>` pour les 4 DLL.
2. Ordre de compilation : `brix-common` → `Brix.Core` → `Assembly-CSharp-firstpass` → `Assembly-CSharp`. Vérifier qu'il n'y a pas de dépendance circulaire (firstpass ↔ Brix.Core, **à vérifier**).
3. Corrections typiques : machines à états `yield` mal reconstruites, noms générés par le compilateur (`<>c__DisplayClass`), propriétés auto, `fixed`/`unsafe`, surcharges ambiguës, `out` non assignés, littéraux décimaux.
4. Exclure ou remplacer par des stubs le code tiers déjà disponible en DLL (Steamworks.NET, Rewired, I2 Localization, PostProcessing, Wwise C#, GameAnalytics, MoonSharp si séparé) : ne décompiler que le code `Brix.*` et le namespace global du jeu.
5. **Garder noms, namespaces et noms d'assembly identiques** : les scripts sérialisés sont liés par (assembly, namespace, classe). Sinon, les MonoBehaviour deviennent « missing ».
6. Travail parallélisable par namespace (`Brix.Game.AI`, `Brix.UI`, `Brix.Engine`…).

Contrôle : `re/tools/clrmeta.py --methods` sur la DLL recompilée vs l'originale → mêmes types et méthodes.

### Phase 3 — Remappage et import (modèle moyen)
- Copier les assets AssetRipper dans `03-unity/Assets/`.
- Vérifier les références de scripts (GUID / fileID). Compter les composants « missing » par scène et par prefab ; objectif 0.
- Vérifier les données sérialisées connues : `Recepteur._baseCapacity` du `Bricktron` = PlankBlock 3, StoneBlock 1, WoodBlock 1 (cf. `re/05` §« Limite de port »). Un écart signale un mauvais remappage.
- Build Settings : même ordre de scènes que l'original (§3).

### Phase 4 — Shaders (modèle fort, effort maximal)
- Les shaders sont compilés ; AssetRipper ne restitue pas la source.
- Priorité : rendu voxel FlatWorld (`Brix.Engine.FlatWorld.Shaders`, repli CPU `Soft` dans Brix.Core). Lire le C# appelant (noms de propriétés, keywords, passes) pour reconstruire des shaders compatibles.
- Ensuite : terrain, eau, ciel (`Journee`), post-traitement (PostProcessing Stack v1 : source publique, à réintégrer telle quelle), AmplifyOcclusion (asset tiers).
- Livrable : liste shader → statut (original public / reconstruit / factice).

### Phase 5 — Audio (modèle moyen)
- Extraire les SoundBanks Wwise (`StreamingAssets`, **à vérifier**) → `.wem` → `.ogg`.
- Décision à faire valider : (a) recréer un projet Wwise 2016/2017 compatible, ou (b) remplacer `SoundEngine` par un backend Unity AudioSource (adaptateur derrière la même API).

### Phase 6 — Lua, données, UI
- Copier `Info/` tel quel (Lua, Maps, GMPresets, Languages, Blueprints).
- UI : prefabs Unity UI (AssetRipper) + LUI (Lua). Tracer l'UI web éventuelle (phase 0) ; si elle ne sert qu'au launcher, l'exclure.

### Phase 7 — Build et validation (modèle fort)
| Critère | Méthode |
|---|---|
| Compilation | 0 erreur dans l'éditeur |
| Boot | Splash → menu sans exception (`output_log.txt`) |
| Partie | `sandbox` sur `Invictus` : terrain, Brictron, construction d'un bloc |
| IA | Le comportement vanilla doit être reproduit (le bug « poser un bloc » de `re/05` doit être présent : preuve de fidélité) |
| Lua | Gamemodes chargés, `hk_OnLevelReady` déclenché |

## 7. Répartition modèles / effort

| Phase | Modèle | Effort |
|---|---|---|
| 0, 1 | Haiku 5.5 | low |
| 2 (volume), 3, 5 | Sonnet 5.5 | medium |
| 2 (cas durs), 6 | Sonnet 5.5 / Opus 5.5 | high |
| 4 | Opus 5.5 | xhigh |
| Cadrage, 7 | Opus 5.5 | high |

Orchestration multi-agents (ex. phase 2 par namespace) seulement si l'utilisateur la demande explicitement.

## 8. Risques connus

| Risque | Parade |
|---|---|
| DLL patchée prise pour l'original | Toujours partir de `.orig` |
| Scripts « missing » après recompilation | Noms d'assembly, namespaces et classes identiques ; contrôle `clrmeta` |
| Shaders factices → rendu noir/rose | Phase 4 dédiée ; jalon intermédiaire avec shaders standard |
| Wwise absent → exceptions au boot | Stub ou adaptateur `SoundEngine` |
| Steam requis | `steam_appid.txt` (227860) dans le dossier du build, ou stub `SteamManager` |
| Ordre d'Update non spécifié | Ne pas ajouter de Script Execution Order (l'original n'en a pas) |
| Valeurs de scène non lues jusqu'ici (`maxThreadCount`, `WorkTime`, `SplashScreen.nextScene`) | Les vérifier dans l'export AssetRipper |

## 9. Livrables finaux

1. `03-unity/` : projet Unity 5.6.5f1 complet.
2. `journal.md` : décisions, écarts avec l'original, tableau d'état par phase.
3. `00-inventaire/inventaire.md` : inventaire et empreintes.
4. Liste des éléments non récupérés ou reconstruits (shaders, audio, UI web).
