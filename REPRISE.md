# Reprise — RE Castle Story (2026-10-18, 15h)

## Règles (aussi en mémoire Claude)

| Règle | Détail |
|---|---|
| Style | Court, précis, ton expert. Pas de politesse, pas de reformulation. Récap = tableau |
| Question ≠ ordre | Question → réponse seule, aucune action. Agir uniquement sur ordre |
| Validation | Plan soumis par étape, exécution sur ordre |
| Fichiers du jeu | Lecture seule, sauf modding autorisé 2026-10-17 : BepInEx + `BepInEx/plugins` + `run_bepinex.sh`. DLL d'origine intactes, sauf `Assembly-CSharp-firstpass.dll` remplacée (autorisé 2026-10-17, original `.orig`, `patch/restore.sh`) |
| Productions | Travailler via `/games/steamapps/common/Castle Story/re/` (symlink vers `~/castle-story-re/`) |
| Dynamique | Frida/gdb uniquement sur ordre |
| Périmètre | `Assembly-CSharp`, `Assembly-CSharp-firstpass`, `Brix.Core`, `brix-common` |
| Objectif | 1) moteur, 2) API Lua pour mods |

## Environnement

| Élément | État |
|---|---|
| REA (MCP) | `rea-agents@6.5.0`, OK. Pas de décompilation .NET ; budget dépassé sur Brix.Core/firstpass sans `type_names` |
| Node | nvm, v24.22 |
| .NET SDK | 8.0.426 dans `~/.dotnet` (hors PATH) |
| ilspycmd | 9.1.0.7988 → via `tools/decomp.sh <Type> [dll]` |
| Agents | `.claude/agents/` (symlink `Castle Story/.claude`) : `re-meta` (haiku/low), `re-lua` (haiku/low), `re-rea` (sonnet/medium), `re-analyst` (sonnet/high). **Chargés après redémarrage** |

## Fichiers

| Chemin | Contenu |
|---|---|
| `01-architecture.md` | Brix.Core : moteur voxel, FlatWorld, couches, réseau, pooling, sauvegardes |
| `02-firstpass.md` | Firstpass : composition, orchestration (`GameParam`), pont Lua (`Brix.Lua`, 997 membres `[LuaDrivable]`) |
| `tools/clrmeta.py` | Parseur métadonnées : `--methods`, `--attrs`, `--all-attrs` |
| `tools/decomp.sh` | Décompilation d'un type, cache dans `decomp/` |
| `dumps/*.types.tsv`, `*.attrs.tsv` | Types/méthodes/attributs des 4 DLL |
| `decomp/*.cs` | Types décompilés unitairement ; `decomp/firstpass/` = firstpass complet |
| `03-moteur.md` | Flux moteur : scènes, boot, StartGame, GameLoader.Load, horloges, fin de partie |
| `04-api-lua.md`, `04-api-lua-ref.tsv` | API Lua mods : mécanisme, hooks, chargement, limites ; référence 1259 entrées |

## Étapes

| # | Étape | État |
|---|---|---|
| 1 | Architecture Brix.Core | Fait |
| 2 | Architecture firstpass + pont Lua | Fait |
| 3 | Moteur : `03-moteur.md` (boot, chargement map, GameClock, IA 50 Hz, fin de partie) | Fait |
| 3' | API mods : `04-api-lua.md` + `04-api-lua-ref.tsv` (outils `tools/luaapi_*.py`, décompilation complète `decomp/firstpass/`) | Fait |
| — | Bug construction (voir ci-dessous) | Investigation amorcée |

## Bug construction

| Point | État |
|---|---|
| Symptôme | Ordre « poser un bloc » : tous les Brictron dispo vont chercher un bloc ; le premier pose et clôt l'ordre ; les autres repartent |
| Faisable en Lua ? | Non. Contournement : `maxCrewCount` (= `Project.MaximumCrewSize`, défaut 4, clamp 0..8). `Project.MaxSize` = rayon de zone, pas une limite d'équipe |
| Détail | `05-bug-construction.md` (decomp `decomp/firstpass/Brix.Game.AI/`, DLL firstpass) |
| Hypothèse « inscription tardive » | Réfutée : `workers.Add` dès la création de la Task (`Goal.WrapTask`), retrait en `Finally` |
| Cause racine | `BuildGoal.IsMatchFor` (BuildGoal.cs:244) ne teste `HasOtherWorkerAssigned` que si `IsAnti` ; seul Goal sans `GoalTooCrowded`. `AcceptMultiWorkers` = champ mort |
| Annexe | `ReserveUntil` des blocs sources ignoré par `ClosestObjectToPickup` / `FindOneResource` |
| Patch A | `BuildGoal.IsMatchFor` : rejeter si workers (hors labor) ≥ slots restants du récepteur |
| Patch B | `Project.GetPrepareFunction` avant `Hire` : même test, ferme la course |
| Vérif. patch (05 §Vérification) | 1 mort/fin Task : RISQUE (fuites `workers` : AbortWork sur PendingTask, remplacement PendingTask, arbre gelé, PhaseOut) ; 2 pool Goal : RISQUE modéré (`workers` jamais vidé) ; 3 unité capacité = objets : OK ; 4 Prepare/Send même tick : OK (B atomique) |
| v1.1 (installée) | Comptage vivant : worker `enabled` dont `CurrentTask` (ni Finished ni Failed) ou `PendingTask` a `Giver == goal.gameObject` ; purge des entrées null/released ; log unique `BaseCapacity`/`CurrentCapacity` par clé Blueprint (`LogCapacity`) |
| Échec BepInEx | Préchargeur BepInEx planté : NRE dans `MonoMod.RuntimeDetour.NativeDetour` (Mono Unity 5.6.5 Linux). `HarmonyInteropFix` est inconditionnel → `ApplyRuntimePatches=false` ne suffit pas. Harmony inutilisable |
| Échec chargement mémoire | Doorstop + Cecil + `Assembly.Load` : Unity 5.6 recharge firstpass depuis le disque dans « Unity Child Domain » → 2 firstpass, scripts « missing » (Crystal, Crawler, Construction…), plus de cartes, crash au chargement de carte. Abandonné ; `run_bepinex.sh` remis sur le préchargeur BepInEx, loader retiré |
| Étape 4 (v1.1, DLL sur disque) | `patch/install.sh` : sauvegarde `Managed/Assembly-CSharp-firstpass.dll.orig` (sha a5bad8e3…), réécrit firstpass via `patch/BuildGoalFix.Patcher` (Cecil : chaque `ret` de `BuildGoal.IsMatchFor` → `Hooks.AfterIsMatchFor`, `Project.GetPrepareFunction` → `Hooks.WrapPrepare`), copie `CastleStory.BuildGoalFix.dll` dans `Managed`. Relançable après mise à jour Steam. `patch/restore.sh` = retour à l'original. Log/config : `Managed/BuildGoalFix.log` / `.cfg`. Métadonnées identiques à l'original (types/méthodes/champs) + 1 assemblyref. `steam_appid.txt`=227860 ajouté |
| Test v1.1 | OK en jeu (Steam) : cartes présentes, hooks chargés, 1 bloc → 1 Brictron. Capacités : `Stone_DoubleBrick` StoneBlock=1, `Pallet` PlankBlock=1. `Verbose=true` activé |
| Limite v1.1 (palettes) | Un Brictron ramasse n = min(20, stock de même clé, place de son récepteur) (`Transaction.TakeFromEnumerable`) sans regarder la demande ; enchaîne les Blueprints voisins avec le surplus. v1.1 : il est refusé là où un autre est inscrit → surplus rapporté au stock (`IdleProject.GoStore`). Détail : 05 §« Ramassage multi-blocs » |
| Limite de port | Donnée prefab `Recepteur._baseCapacity` du GameObject `Bricktron` (level7) : PlankBlock 3, StoneBlock 1, WoodBlock 1, Iron/Glass/Terracotta 3, granulaires 20, Bomb 10 ; pas d'Encombrement ; 1 type à la fois ; Builder seul (autres occupations = 0). Hook : `recepteur.CanFitUpTo(key)`. Outil `tools/unity_adjectives_dump.py`. Détail : 05 §« Limite de port » |
| v1.2 (installée) | P3. Restant(type) = Σ CurrentCapacity des BuildGoal commités/enabled du `BuildGoalProvider` ; couvert = Σ par Brictron vivant (porté si > 0, sinon place libre `CurrentCapacity.Value(type)`). Refus si couvert (hors candidat) ≥ restant pour chaque type attendu : dans `IsMatchFor` (stats en cache par `Time.fixedTime`) et `WrapPrepare` (recalcul, atomique). Borne : entrée de `PickUpNode.TakeSome` → `Hooks.ClampPickup` : `upTo = max(1, restant − couvert_autres)` si Task d'un BuildGoal non-anti. Config : `ProjectLimit`, `ClampPickup` (ajoutées au .cfg au 1er chargement). Vérifié hors jeu |
| Test v1.2 | 3 palettes : 1 Brictron (Hanjo, borne 3) ; après la 1re palette, sa Task finie → plus inscrit nulle part ~10 ticks → couverture 0 → Ada embauchée (borne 2) sur palette 7 → Hanjo refusé (per-goal) et rapporte 2 planches au stock. 2e série (17/22/23/24) : Sierra 3 + Ada 3 → ordres multiples ou même défaut, à retester |
| v1.3 (installée) | Registre des porteurs (enregistrés au ramassage, comptés sur leur charge réelle entre deux tâches, retirés si vides/détruits, ignorés s'ils travaillent pour un autre projet) ; porteur exempté du contrôle projet et non bloqué par des inscrits à vide ; journal par ordre `[Ordre #id]` (composition, embauche, ramassage, fin de tâche via hook en entrée de `Goal.EndWork`), clé `LogOrders` |
| Test v1.3 | `BuildGoalProvider` unique : tous les ordres = même projet (#-991794). 3 palettes : Hanjo seul (3) enchaîne ; 10 palettes : ramassages 3+3+3+1. Défaut : porteur juste après une pose non compté (CurrentTask = Task finie, Giver détruit → classé « ailleurs ») → embauches inutiles. Embauches « rien à livrer » = démolitions (anti). `WorkerFailed` en rafale au début des 10 palettes, cause inconnue |
| v1.3.1 (installée) | Porteur ignoré seulement si Task en cours (ni Finished ni Failed) pour un autre chantier ; journal : démolitions étiquetées, raison d'échec (`Task.Exception` ou dernier `Task.Logs`) |
| Test v1.3.1 | 3 palettes : 6 embauches, 2 échecs, 1 porteur ; 10 palettes : 19 embauches, 7 échecs, 12 ramassés ; pierre OK. Défaut porteur persiste (3 cas) : à la recomposition juste après une pose, la Task est encore en cours, `Giver` non null mais sans `BuildGoal` valide → `working == null` → ignoré. Échecs : `ObjectHasBeenMovedFromRecepteur` (tous visent le même objet : `FindOneResource` = `FirstObjectOf`, `ClosestObjectToPickup` ignore `IsReserved`) et `ObjectIsInSomeoneElseBag` (filtre `_ClosestObjectToPickup_Test` accepte les sacs de Brictron comme source). Détail : 05 §« Test v1.3.1 » |
| v1.3.2 (installée) | 1) `AddCarriers` : porteur ignoré seulement si Task en cours pour un `BuildGoal` vivant d'un **autre** Provider ; 2) borne de ramassage plafonnée à `CanFitUpTo(key)` ; 3) `PickupFilter` : hooks E `Knowledge._ClosestObjectToPickup_Test` (sacs de Brictron exclus, objets isolés réservés exclus), F `Labor.FindOneResource` (si l'objet du conteneur est réservé → premier objet non réservé, sinon null), G `PickUpNode._GoPickUp` (réserve l'objet jusqu'à `WorkEnded`). Build + installation : `patch/build.sh`. Métadonnées identiques à l'original |
| Test v1.3.2 | 3 palettes : 6 embauches, 1 échec ; 10 palettes : 13 embauches, 4 échecs, 13 ramassés. Plus de `SomeoneElseBag`. Porteur toujours non compté : cause réelle = `RegisterCarrier` puis `ComputeStats` en entrée de `TakeSome`, porteur encore vide → retiré aussitôt (registre jamais effectif depuis v1.3). Échecs `Moved` restants = cible issue du cache par `decisionFrame` (trouvée à l'évaluation, prise par un autre avant exécution). Détail : 05 §« Test v1.3.2 » |
| v1.3.3 (installée 22:45, jeu en cours → effet au prochain lancement) | 1) porteur vide retiré seulement sans Task en cours ; 2) hook G remplaçant : `item = Hooks.OnGoPickUp(labor, item)` → si réservé par un autre, premier objet libre du conteneur (`FirstFreeSibling`), sinon null ; table `ReservedBy` ; 3) `build.sh` `--no-incremental`, `install.sh` copie atomique. Métadonnées identiques |
| Test v1.3.3 | 3 palettes : 3 embauches, 0 échec, porteur compté (`couvert 2 [Ada=2]`) ; 10 palettes : 14 embauches, 4 échecs, 11 ramassés, couverture exacte en fin. Échecs : `Moved` ×2 (`TransferSpecificObjects` prend les premiers objets du conteneur, pas l'objet réservé) ; `SomeoneElseBag` ×2 (G énumérait le sac du ramasseur). Détail : 05 §« Test v1.3.3 » |
| v1.3.4 (installée 22:55, jeu en cours → effet au prochain lancement) | Hook H entrée `Transaction.TakeFromEnumerable` : `OrderPickup` (ses réservés, libres, réservés d'autrui) ; G : cible réservée par un autre ou déjà dans un sac → recherche fraîche `ClosestObjectToPickup` ; `build.sh` rebuild Patcher si sortie incomplète. Métadonnées identiques |
| Test v1.3.4 | Session réelle 70 min, 12 ordres, 236 embauches : **0** `Moved`/`SomeoneElseBag`, 11 `CannotFind` (plus de pierre libre, échec immédiat), 4 `PathFailed`. **Régression** : G remplaçait aussi les objets déjà portés (flux bûches/Rope/RawFiber) → 636 échecs forcés en boucle (jusqu'à 309/min). E excluait aussi le propre sac du chercheur. Détail : 05 §« Test v1.3.4 » |
| v1.3.5 (installée, effet au prochain lancement) | G : objet déjà porté rendu tel quel ; hook I entrée `Knowledge.ClosestObjectToPickup` → `BeginSearch(source)` ; E n'exclut que les sacs des autres Brictron. Métadonnées identiques |
| Test v1.3.5 | 12 min : 9 palettes = 9 embauches / 3 ramassages de 3, 7 palettes = 7 embauches / 0 ramassage, 0 « déjà dans le sac », 0 `Moved`/`SomeoneElseBag`. **Bug palettes réglé.** Pierre (15 blocs) : 35 embauches, 7 `CannotFind` = palette la plus proche épuisée (objets tous réservés), F/G rendent null. Détail : 05 §« Test v1.3.5 » |
| v1.3.6 (installée 00:33, effet au prochain lancement) | E n'admet un conteneur que s'il a un objet libre (ou réservé par le chercheur) de la ressource → la recherche saute les palettes épuisées ; `FirstFreeSibling(…, self)`. Vérif : `patch/verify/{orig,patched}.tsv` identiques |
| Étape 5 — logistique (2026-10-18) | Périmètre élargi par l'utilisateur : journaliser et optimiser tout le transport de ressources et la prise en charge des travaux (construction, artisanat armes/casques/corde, récolte, stockage) + surveillance des distances pour repérer les optimisations de trajet (ex. chantiers éloignés → répartir les Brictron libres). Analyse statique : `06-logistique.md` (artisanat = `CraftingStation` IOperable, pas un Goal ; pivots uniques ramasser/déposer ; défauts similaires ailleurs) |
| v1.4 (installée 11:40, jeu en cours → effet au prochain lancement) | Journal unifié `Managed/Logistique.log` (clé `LogAll`, `Journal.cs`, 9 hooks J1-J9 : Project.Hire, IOperableExtender.Hire, Labor.SendOrder, Transaction.StoreEverything entrée+ret, AbsorbGarnottes, OperationSwitchTask, Recipe.Finish, DropAll, Task.OnFinally), odomètre par Brictron (`JournalTracker`), distances à chaque événement, `étendue`/`centre` des ordres multiples dans BuildGoalFix.log. `WrapPrepare` appliqué à tout Goal (garde BuildGoal inchangée). Outil `tools/logistique_report.py`. Détail : 06 §7 |
| Session v1.4 (12:38 → 14:29) | 10 564 lignes, 6 BuildProject, Mine, 2 Harvest, 5 ChoppingBlock, Loom/Workbench/Furnace. Gaspillage n° 1 : stockage plein à l'arrivée (21 `StorageBecameFull`, 423 m), cause `ClosestStorageWithRoomFor` = place pour 1 objet, sans réservation ni charge réelle. Répartition : 1 seule embauche lointaine sur 301, mais pas de test à deux chantiers simultanés. Détail : 06 §8 |
| v1.4.1 (installée, effet au prochain lancement) | Journal : source de ramassage lue avant transfert, ABSORBE seulement si prise, EMBAUCHE idle supprimée, `dest_plein` ; rapport : noms avec espaces |
| Windows (2026-10-18) | Jeu sous `E:\SteamLibrary\steamapps\common\Castle Story`. firstpass Windows (sha 6f11399f…) = Linux hors code plateforme (`Ak*`, `lzip`). v1.4.1 installée (patché 1c10d34d…, métadonnées identiques). Scripts bi-plateforme : `patch/env.sh` (dossier du jeu : `CS_GAME`, sinon `re/../..`, sinon chemins connus ; `~/.dotnet` si présent ; `cygpath`), `build.sh` passe `-p:GameManaged=…` aux `.csproj` (défaut Linux conservé). Testé sous Git Bash : build + install OK |
| Test Windows v1.4.1 (23:18 → 23:40) | Copies : `patch/verify/*-2026-10-18-win.log`. Build : palettes OK (1 `Moved`), pierre 26 blocs depuis 8 palettes, **0 `CannotFind` → v1.3.6 validée**. Journal v1.4.1 OK (source réelle, 0 EMBAUCHE idle, `dest_plein`). Gaspillage n° 1 désormais `NoCorrespondingStorage` en récolte : 36 échecs, 566 m (sac plein RawIron/OrangeCrystal=20, aucun stockage) ; `StorageBecameFull` 14, 231 m. Test « deux chantiers simultanés » **non réalisé** (un seul BuildProject actif à la fois ; un ordre de 67 m d'étendue, d_embauche moy 17 m) |
| v1.5 volet A (installée, effet au prochain lancement) | Réservation de capacité des stockages : classe `Storage`, hooks K-O, clé `StorageReserve`, journal `STOCK_REDIRIGE`. Détail : 06 §9. Vérif : métadonnées identiques, 25/25 appels de hooks résolus |
| Test v1.5 A (2026-10-19, 00:04 → 00:36) | Copies : `patch/verify/*-2026-10-19-v1.5.log`. Stockage saturé (156 dépôts sur 324 remplissent la destination). **`StorageBecameFull` 0** (14 / 231 m avant), **`NoCorrespondingStorage` 0** (36 / 566 m), `CouldNotFindStorage` 0, `AU_SOL` 0, 0 erreur de hook. `STOCK_REDIRIGE` 0 : tout l'effet passe par le filtre L à la décision (pas d'embauche sans stockage libre). Nouveau 1er poste : `PathExecutionFailed` sur BuildProject, 11 échecs / 745 m (Blueprints inaccessibles, ex. merlons ; vanilla). Test deux chantiers simultanés toujours non réalisé (5 BuildProject successifs) |
| v1.6 (installée, effet au prochain lancement) | Ordre de construction (classe `BuildOrder`, clés `BuildReach`/`BuildPlan`, hook P sur `BuildGoal.PriorityFor`) : 1a accès (journal), 1b refus d'un Blueprint dont la pose couperait l'accès d'un autre, 2 bonus « intérieur → extérieur ». Échafaudage auto refusé par l'utilisateur. Détail : 06 §10. Vérif : `patch/verify/grid_sim.py`, 26/26 appels résolus |
| Test v1.6 (2026-10-19) | Ordre respecté, construction terminée. `PathExecutionFailed` BuildProject 9 / 369 m (v1.5 : 11 / 745 m), surtout créneaux/poutres en hauteur (modèle trop permissif). 2 Brictron enfermés à l'étage. Gel ~0,5 s : calcul de 647 ms. Détail : 06 §10.3 |
| v1.6.1 (installée, effet au prochain lancement) | Calcul accéléré (cases debout précalculées, test local) ; protection des Brictron (pas d'enfermement, poseur compris) ; évacuation des Brictron inactifs menacés (`Évacuation :` dans BuildGoalFix.log) |
| Prochaine action | Test v1.6.1 : même construction (étage sans escalier, créneaux) ; vérifier absence de gel, lignes `accès :` (ms), `Évacuation :`, Brictron bloqués. Piste : forme réelle des créneaux/poutres dans le modèle. Deux chantiers simultanés toujours à faire |

## Notes pour plus tard

| Date | Idée |
|---|---|
| 2026-10-17 (précisé le 2026-10-18 : l'objectif est un repérage général des optimisations de trajet, nord/sud n'est qu'un exemple ; décision après les chiffres du journal v1.4) | Position des chantiers : savoir si on connaît l'emplacement de chaque Blueprint (`BuildGoal` / `Blueprint` → `voxel`/`Location.Of`). Permettrait d'optimiser : si un Blueprint est trop loin des autres (ou du stock), embaucher plusieurs Brictron sur le projet pour aller plus vite au lieu d'un porteur unique qui enchaîne |
