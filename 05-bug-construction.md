# 05 - Bug construction : « poser une brique » attire tous les Labors

Chemins : `D` = `~/castle-story-re/decomp/firstpass/Brix.Game.AI/` (sauf mention). Tous les types sont dans `Assembly-CSharp-firstpass`.
[O] = lu dans le code, [D] = déduit.

## Verdict

- Hypothèse « inscription dans `workers` trop tardive » : **réfutée** [O]. `workers.Add` est exécuté à la création de la Task (`Goal.WrapTask`, `Goal.cs:100-106`), donc au `Send` de l'ordre, avant tout déplacement.
- Cause racine [O] : `BuildGoal.IsMatchFor` n'appelle `HasOtherWorkerAssigned` **que si `IsAnti`** (démolition) (`BuildGoal.cs:244`). Pour un bloc normal il n'y a aucun test de saturation.
  - `BuildGoal.PriorityFor` n'applique qu'une pénalité de -10 si un autre worker est inscrit (`BuildGoal.cs:207`). Avec un seul Goal candidat, la pénalité ne change rien.
  - `BuildGoal.AcceptMultiWorkers` (`BuildGoal.cs:46`) est un champ mort : aucune lecture dans tout firstpass [O].
  - Les autres Goals (Dig, Tree, Repair, Transform, Litter, CleanUp, Landscape, NatureHarvest, BuildStairs) rejettent un Goal déjà pris (`GoalTooCrowded`). BuildGoal est le seul à ne pas le faire. [O]
- Le design est « N Labors remplissent le récepteur du Blueprint jusqu'à ce qu'il soit plein » (`BuildGoal.IsValid = !recepteur.IsFull()`, `BuildGoal.cs:128`). Il est correct pour un blueprint à capacité N. Pour une capacité de 1 bloc, tout le monde part. [D]
- La réservation des blocs sources (`KnowledgeReserver.ReserveUntil`) est **inopérante pour le choix de la source** [O] : `IsReserved` n'est lu que par `TreeGoal.cs:97` et `CleanUpGoal.cs:57`. `Knowledge.ClosestObjectToPickup` (`Knowledge.cs:236-246`) et `Labor.FindOneResource` (`Labor.cs:~700`) l'ignorent.
- Correction de REPRISE.md : `Project.MaxSize` (=16, `Project.cs:113`) est un **rayon de zone** (`Beacon.cs:185`, `LandscapeController.cs:28`), pas une limite d'équipe. La limite d'équipe est `Project.MaximumCrewSize` = `WorkForceProvider.MaxPopulation` (`_maxPopulation=4` par défaut, clamp 0..8, `Project.cs:398-411`), Lua `maxCrewCount`. [O]

## Séquence d'un Labor (BuildProject, bloc unique)

| # | Étape | Méthode (fichier:ligne) | `workers` du Goal / réservation / crew projet |
|---|---|---|---|
| 1 | Boucle IA (Motus, 1 tick = 1 FixedUpdate) | `Labor.Initialize` : `AddOrSetSequence(LookForWorkSequence(), 10, ...)` (`Labor.cs:385`) | rien |
| 2 | Garde `LookingForWork()` | `Labor.cs:535-558, 560` : Activity Chilling, ou Idle && `IsAvailable()` && file d'ordres vide | `IsAvailable()` = ni CurrentWork ni PendingTask ni délai (`BaseLabor.cs:186`) |
| 3 | Liste des projets | `CheckProjectForWork` -> `RefreshAvailableProjects` (`Labor.cs:574, 599`), triés par `PriorityInput` | rien |
| 4 | Test projet | `TryWorkInProject` : `operable.HasWorkFor(this)` (`Labor.cs:614-623`) = `HasGoals() && HasRoomFor(labor)` (`Project.cs:701-704, 806`) | `HasRoomFor` : crew plein -> seulement si `labor.Project==this`. Crew **non modifié** ici |
| 5 | Décalage anti-rafale | `SendProjectInstruction` : `WaitFor.Ticks(OffsetWaitTime)` (`Labor.cs:638-645, 288-302`) : `(n % 8) * 2 + 1` ticks pour le n-ième Labor à interroger dans une fenêtre de 2 ticks | rien. C'est un étalement, pas un verrou |
| 6 | Vérif. ressources | `Project.VerifyWorkFor` -> `VerifyResourceAccessFor` : `labor.Find(desc)` + `WaitForTicks(1)` (`Project.cs:706-722, ~770`) | rien |
| 7 | Choix du Goal | `BuildProject.SelectBestGoal` -> `goals.FulfillableBy(labor)` -> `GoalSelector.LocalMaximum` (`BuildProject.cs:21, 64`; `LocalMaximumSelector.cs`) | lecture seule. Le selector (`GoalSelector.cs:196-214`) appelle `goal.IsMatchFor` puis `PriorityFor`. Pour `BuildGoal`, **aucun filtre sur `workers`**. `BuildGoalFinder.Visit` (`:303-328`) ne filtre pas non plus. `IsValid()` n'est pas appelé dans ce chemin (`Project.GoalNeedsEvaluation` ne sert pas BuildProject) |
| 8 | Fabrication du paquet | `Project.GetInstruction` -> `_GetInstruction` -> `goal.Package(Project)` + `SetPrepare(GetPrepareFunction(goal))` (`Project.cs:1039-1063`) | rien |
| 9 | `AssignInstruction` | `Labor.AssignInstruction` (`Labor.cs:654`) : `Prepare` puis `Send` | |
| 9a | Prepare | `GetPrepareFunction` : `goal.ConnectProject(this); return Hire(labor)` (`Project.cs:1057-1063`) | **Crew projet +1** (`workforceProvider.AddObject`, `Project.cs:889-912`). Si `CanHire` faux : `Node.Fail`, rien n'est créé |
| 9b | Send | `OrderPackage<Labor,T>.Send` -> `WorkInstruction<,T>.Send` -> `GetTask` = `Goal.WrapTask` (`OrderPackage.cs:25`, `WorkInstruction.cs` `Send(...value...)`, `Goal.cs:54, 100-109`) | **`workers.Add(labor)`** (`Goal.cs:103-106`). `Task.CreateFromGoal` -> `BuildGoal.GetWorkNode` (`Task.cs:242-256`, `BuildGoal.cs:106-126`) |
| 9c | SendOrder | `Labor.SendOrder` -> `BaseLabor.SendOrder` -> `Do(task)` : `PendingTask=task` (`Labor.cs:1085`, `BaseLabor.cs:250-260`) | `IsAvailable()` devient faux -> le Labor sort de LookForWork |
| 10 | Prise en charge | `ProcessPendingWorkSequence` : `CurrentTask=PendingTask` (`BaseLabor.cs:133-150`), puis `ProcessCurrentWorkSequence` exécute `CurrentWork` (`BaseLabor.cs:93-131`) | |
| 11 | Fetch | `BuildGoal.GetWorkNode` : si le Labor porte déjà de quoi remplir -> `GoStoreIn`. Sinon `FindAndPickUp(recepteur)` puis `GoStoreIn` (`BuildGoal.cs:120-125`). `PickUpNode._GoPickUp` : `Knowledge.ReserveUntil(source, labor.signal.WorkEnded)` -> `RunToExactly` -> `TakeSome` (`PickUpNode.cs:29-57, 83-91`) | réservation de la **source** retirée à `WorkEnded` (`KnowledgeReserver.cs:24-34`). Ni lue ni bloquante pour un autre fetch (cf. verdict) |
| 12 | Déplacement + pose | `StoreNode.GoStoreIn` : `NavigateAndCheck` (`RunToExactly` + FailWhen NullStorage si le Blueprint est détruit, InventoryBecameEmpty, StorageBecameFull seulement si `Fonction==entrepot`) puis `StoreIn` -> `transaction.StoreEverything` (`StoreNode.cs:22-46`, `Transaction.cs:65`) | |
| 13 | Complétion | `Recepteur.InventoryChanged` -> `Blueprint.OnReceivedNewItem` : si `recepteur.IsFull()` -> `MorphToObject` -> `Transactor.SpawnUNet(BlueprintResult)` + `SafeNetworkDestroy` du Blueprint (`Blueprint.cs:306-320, 322-363`) | Goal et Blueprint = même GameObject, donc détruit |
| 14 | Fin de Task (réussite, échec, annulation) | `Task.Catches(...).Finally(OnFinally)` -> `EndAction(this)` = `Goal.EndWork` -> `UnassignWorker` : `workers.Remove(labor)`, `DelayBeforeAvailable=0.05`, `WorkEnded.Invoke()` (`Task.cs:175-184, 191-205`; `Goal.cs:111-134`) | **`workers.Remove`** : toujours exécuté dans le Finally. `WorkEnded` libère les réservations de sources |
| 15 | Les autres Labors | Leur `NavigateAndCheck` voit `contenant.IsNullOrReleased()` : `NullStorage` -> Task en échec (`StoreNode.cs:30-34`), puis `DelayOnFail=30` ticks (`Task.cs:34, 182`) | `workers.Remove` via Finally. Ils portent le bloc pris : ils le rangent ensuite (instinct, GoStore) [D] |

## Sites d'écriture de `Goal.workers` [O]

`Goal.cs:23` (déclaration, `[SerializeField]`), `:105` Add (`WrapTask`), `:130` Remove (`UnassignWorker`). Aucun Clear. Aucune sous-classe n'écrit dedans (`private`). `UnassignWorker` n'a que `Goal.EndWork` pour appelant.

## Appelants de `HasOtherWorkerAssigned`

| Goal | Site | Effet |
|---|---|---|
| BuildGoal | `:207` PriorityFor | -10 |
| BuildGoal | `:244` IsMatchFor, **seulement `IsAnti`** | rejet `GoalTooCrowded` |
| DigGoal | `:111` IsMatchFor / `:145` PriorityFor | rejet / -100 |
| BuildStairsGoal `:103`, CleanUpGoal `:22`, LandscapeGoal `:154`, LitterGoal `:122`, NatureHarvestGoal `:89`, RepairGoal `:55`, TransformGoal `:99` | IsMatchFor | rejet |
| TreeGoal | `:68` IsMatchFor (rejet) / `:97` PriorityFor (+100, aussi si `IsReserved`) | |

`AssignedWorkerCount()` : aucun appelant [O]. `RequiredLabors` : aucun appelant [O].

## Capacités / tests d'effectif

| Élément | Valeur [O] |
|---|---|
| Crew projet | `MaximumCrewSize` = `WorkForceProvider._maxPopulation` (4 par défaut dans le code, valeur prefab non lue ; clamp 0..8). Testé dans `Project.HasRoomFor`/`CanHire`. Un Labor déjà membre passe même si le projet est plein |
| `Project.MaxSize` | 16, rayon de zone, sans rapport avec le crew |
| Capacité du Goal | `BuildGoal.recepteur` : fini par `Recepteur.IsFull()` (`Recepteur.cs:539`). Il n'y a pas de nombre de Tasks générées par `BuildGoalProvider` : 1 Goal par Blueprint (`BuildGoalProvider.OnObjectTracked`, `:264-294`), et le Goal est réutilisable par un nombre illimité de Labors |

## Concurrence (tâche 3)

- Plusieurs Labors peuvent tous passer le test [O+D] : `IsMatchFor` ne teste pas `workers` pour un bloc normal, donc l'ordre des inscriptions n'a aucune importance.
- Même avec un test, il y aurait une fenêtre de course [D]. Entre l'évaluation de `IsMatchFor` (étape 7) et `workers.Add` (9b) peuvent s'écouler plusieurs ticks : `WaitFor.Ticks(1)` de `ProcessGoal` (`GoalSelector.cs:171,179`), attentes de `BuildGoalProvider.FulfillableEnumerator` (`WaitForPathfinding`, `SearchStep` par tranches de 10), `OffsetWaitTime` jusqu'à 15 ticks.
- Le seul point **synchrone** juste avant l'inscription est `Prepare`/`Send` (9a/9b). Je n'ai pas vérifié la sémantique Motus pour affirmer qu'ils s'exécutent dans le même tick [D].
- `OffsetWaitTime` réduit la simultanéité mais ne garantit rien : il étale les décisions de 2 ticks entre Labors. Son but probable est de laisser le -10 de `PriorityFor` jouer entre plusieurs Goals [D].
- Effet côté équipe : `Hire` au 9a fait entrer tous les Labors dans le projet jusqu'à `MaximumCrewSize`. Au-delà, `HasWorkFor` répond faux pour les non-membres [O]. Avec un effectif projet de 8 ou moins, tout le monde peut partir.

## Autres causes possibles (si la bascule ci-dessus ne suffisait pas) [D]

- Le Goal reste énuméré par `BuildGoalProvider` tant que le Blueprint est `buildable` ; `IsValid()` (récepteur plein) n'est jamais évalué pour BuildProject. La fenêtre est courte car le Blueprint est détruit à la pose.
- Fuite de `workers` si le GameObject Goal est recyclé par pool alors qu'une Task est encore active : le Finally de la Task appelle `EndWork` sur le Goal capturé. Le risque dépend de la sémantique du pool et n'est pas vérifié [zone floue].

## Points de patch

Chaque option est à valider côté serveur uniquement : l'IA ne tourne que sur l'hôte (`MultiBehavior.ToTree` -> `NetworkServer.active`, cf. 03-moteur.md). [O/D]

| # | Où | Changement | Risques |
|---|---|---|---|
| A (recommandé) | `BuildGoal.IsMatchFor` (`BuildGoal.cs:244`) | Étendre la condition : `(IsAnti || RemainingSlots() <= OtherWorkers(labor)) && HasOtherWorkerAssigned(labor)`. Pour un Blueprint à capacité 1, dès qu'un autre Labor est inscrit, rejeter avec `GoalTooCrowded`. `RemainingSlots` s'obtient par `recepteur.GetProportionEncombrement()` (base - contenu, `Recepteur.cs:512`) ; `OtherWorkers` = `AssignedWorkerCount() - (contains(labor)?1:0)` | Un Labor qui a déjà pris sa Task n'est pas affecté (le test exclut `labor`). Les gros Blueprints (capacité > 1) restent multi-Labors. Si un Labor meurt sans passer par `Finally`, le Goal reste « pris » (fuite) [zone floue]. Aucun impact réseau (état purement serveur). Aucune donnée de sauvegarde (`workers` n'est pas dans la save [D]) |
| B (atomique, complémentaire) | `Project.GetPrepareFunction` (`Project.cs:1057-1063`) | Dans le delegate, avant `Hire` : si `goal` est un `BuildGoal` non-anti et que `AssignedWorkerCount() >= RemainingSlots()`, `return false`. `Prepare` renvoie alors `Node.Fail(FailException.Generic)`. `TryWorkInProject` attrape l'échec et ne crée aucune Task, donc ni inscription ni `Hire` | Ferme la fenêtre de course (le test est juste avant `WrapTask`). Le Labor retombe dans la boucle LookForWork et réessaie 10 ticks plus tard : prévoir une charge CPU supplémentaire minime. `Hire` n'est pas appelé, donc pas d'entrée dans le crew pour rien |
| C (non retenu) | `Goal.WrapTask` : refuser de créer la Task | `WrapTask` doit retourner une Task non nulle (`WorkInstruction.Send` ignore `null`, c'est tolérable : `if (task != null)`), mais `Prepare` / `Hire` ont déjà été faits : le Labor est recruté pour rien | Fuite d'effectif projet |

Désinscription : déjà couverte par `Task...Finally(OnFinally) -> Goal.EndWork -> UnassignWorker` pour échec / annulation / abandon (`Task.cs:183`). Pas de nouveau code requis pour A/B. À vérifier si le Labor est détruit en cours de Task (zone floue).

Détail du test pour A/B, côté capacité : pour un seul bloc, `GetProportionEncombrement().Item1 - Item2 == 1`. Un Labor peut porter plusieurs blocs (`FindAndPickUp(..., upTo: 20)`, `PickUpNode.cs:64`), donc « un worker = un slot » est une simplification : un Labor qui apporte 2 blocs pour un Blueprint de capacité 2 serait compté comme 1 worker. Le test `workers >= slots` reste donc conservateur (autorise plus de workers que nécessaire, jamais moins), sans risque de blocage. [D]

## Zones floues

- Valeur prefab de `_maxPopulation` des BuildProject (non lue, données Unity).
- Sémantique Motus : un `Sequence.Do(...)` enchaîne-t-il `Prepare` et `Send` dans le même tick ? Non vérifié (Brix.Core / Motus.Behavior non lu).
- Comportement du pool (`ObjectPoolSingleton`) sur les composants Goal : `workers` réinitialisé ou pas à la réutilisation.
- Capacité exacte du Blueprint « 1 brique » et ce que « brique » désigne côté ressource (le Labor porte `upTo=20` ?) : non lu côté données.
- Mort d'un Labor : le Finally de la Task s'exécute-t-il quand l'arbre Motus est arrêté ? Non vérifié.

## Vérification du patch (points 1-4)

Vocabulaire : Brictron = `Labor`, bloc = élément de construction. Chemins : `FP` = `decomp/firstpass/`, `BC` = `decomp/Brix.Core/` (DLL `Brix.Core` décompilée en entier pour cette vérification), `BG` = `FP/Brix.Game.AI/`. [O] = lu dans le code, [D] = déduit. Les données prefab (Unity assets) n'ont pas été lues : toute valeur prefab est marquée « non vérifiée ».

Patch examiné : `patch/BuildGoalFix/Plugin.cs` (non modifié). Refus d'un Brictron sur un BuildGoal non-anti si `workers` hors lui >= max(1, somme des `CurrentCapacity.Value(r)`).

### Synthèse

| Point | Verdict | Raison courte |
|---|---|---|
| 1. Mort / fin de Task | RISQUE | Mort : le Finally s'exécute [O+D]. Mais 3 chemins laissent un Brictron dans `workers` sans Finally : Task jamais démarrée (abort ou remplacement de `PendingTask`), `Behavior` désactivé après exception, Brictron « phased out ». |
| 2. Pool / Blueprint détruit | RISQUE (modéré) | `workers` n'est jamais vidé (aucun reset dans `Goal`/`BuildGoal`). Fuite transférable à un nouveau Blueprint si la GameObject est réellement poolée. Task en cours : retrait différé de 30 ticks. |
| 3. Unité de `CurrentCapacity` | OK avec réserve | Unité = `Ressource.valeur` (objets), pas l'encombrement. Valeurs prefab non vérifiées. Plancher à 1 : pas de blocage par capacité. |
| 4. Tick Prepare/Send | OK | Test de B, `Hire`, `Send`, `WrapTask`, `workers.Add` dans le même `Behavior.Tick`, sans yield, thread unique. B ferme la course. |

### Point 1 : fins de Task et mort d'un Brictron

Mécanisme du Finally [O] :
- `Task.Catches` pose `Finally(OnFinally)` comme finalizer du `CatcherNodeRun` de la Task (`BG/Task.cs:175-184`). `OnFinally` appelle `EndAction(this)` = `Goal.EndWork` -> `UnassignWorker` -> `workers.Remove` (`Task.cs:191-205`, `Goal.cs:111-134`, `Task.cs:253`).
- Un finalizer s'exécute dans `NodeRun.Halt()` (`FP/Motus.Behavior/NodeRun.cs:85-93`), appelé à la fin normale (`Run`, `NodeRun.cs:69-72`) et par `PooledNodeRun.Dispose()` (`PooledNodeRun.cs:31-46`).
- Tous les noeuds composites (Sequence, Catcher, Brancher, Parallel, Condition, Dynamic) disposent leur enfant dans `Terminate` (`Sequencer.cs:80`, `Catcher.cs:139`, `Brancher.cs:90`, `ParallelSequencer.cs:93`, `ConditionNode.cs:112`, `DynamicNode.cs:84`). Donc `Halt` de la racine déclenche en cascade le Finally de la Task en cours (enfants d'abord, finalizer du parent ensuite).

Chemins de fin de Task :

| # | Chemin | Finally / `workers.Remove` ? | Preuve | Délai de retrait |
|---|---|---|---|---|
| 1 | Succès | Oui | `Task.CoreNode` : `WaitForTicks(DelayOnFinished=10)` puis Done (`Task.cs:207-215`, `:32`) | +10 ticks après la livraison |
| 2 | Échec (FailException, WorkFailed, PathFailed) | Oui | `Catches` : handlers puis `WaitAfterFailure(DelayOnFail=30)` (`Task.cs:177-183`, `Catcher.cs:Tick/IsDelayed`) | handlers + 30 ticks |
| 3 | Interruption par nouvel ordre | Oui | `ProcessPendingWorkSequence` : `CurrentTask.TaskCancelled.Invoke(ReceivedNewInstruction)` si `CanInterrupt` (`BaseLabor.cs:139-146`) puis branche `When(TaskCancelled)` (`Task.cs:141`) | quelques ticks. Si `CanInterrupt=false`, la Task finit seule (`Node.Forever` d'attente) |
| 4 | Abort (`CmdStop`, `Project.CancelAllWork`, `CmdRemoveAllLabor`) sur Task démarrée | Oui | branche `When(Worker.AbortWorkSignal)` (`Task.cs:143`), `UNetContextualCmd.cs:155`, `Project.cs:997`, `UNetProjectCmd.cs:421` | quelques ticks |
| 5 | Abort sur Task encore dans `PendingTask` | **Non : fuite permanente** | `Labor.HookInsructionSignals` : `AbortWorkSignal.Connect(PendingTask = null)` (`Labor.cs:1054-1060`). La Task n'a jamais été `Spawn` : ni `ConnectSignals` ni finalizer | jamais |
| 6 | Remplacement de `PendingTask` par un autre ordre avant prise en charge | **Non : fuite permanente** | `BaseLabor.Do(task)` : `PendingTask = work` sans nettoyage (`BaseLabor.cs:157-160`). `WrapTask` a déjà fait `workers.Add` (`Goal.cs:103-106`) | jamais |
| 7 | Mort du Brictron | Oui si détruit (voir ci-dessous) | `Behavior.OnDestroy` -> `TreeRun.Halt()` (`FP/Brix.Game.Components/Behavior.cs:31-38`) | à la destruction (fin de frame) |
| 8 | Échange de Tasks entre deux Brictrons (`ConflictResolver.SwitchInstructions`) | Oui | `result=Switched` puis re-`Send` du `InitialPackage` : `WrapTask` ajoute le nouveau, l'ancien est annulé par le chemin 3 (`ConflictResolver.cs:110-172`). Effectif stable | quelques ticks |
| 9 | Ordre direct du joueur | Oui (chemin 3), sauf fenêtre du chemin 6 | `SendOrder` (`Labor.cs:1085-1099`) | quelques ticks |
| 10 | Changement d'occupation | Pas de fin directe | `Labor.OnOccupationChange` ne touche ni Task ni signal (`Labor.cs:~459-472`). Effet indirect via `Autonomy` ou `Operable` (`Order.Move.HaltWhatever.Send`, `Labor.cs:194`) = chemins 3 ou 6 | - |
| 11 | Chargement de sauvegarde | Sans objet | `workers`, `CurrentTask`, `PendingTask` n'ont aucun `[JsonProperty]` (seul `Labor.cs:97` en a un, `OriginalSpawnCrystal`) ; `GameComponent` est `JsonObject(OptIn)` (`GameComponent.cs:22`). Les Goals sont recréés avec `workers` vide [O] | - |
| 12 | Exception dans l'arbre du Brictron | **Non : arbre gelé, jamais Halt avant destruction** | `Behavior.OnFixedUpdateError` : `base.enabled=false` si `StopOnError` (`Behavior.cs:45-56`, `UpdatableGameComponent.cs:157-168`). `OnDisable` coupe `GameClock` (`UpdatableGameComponent.cs:213-221`) | jamais (jusqu'à la mort) |
| 13 | `Labor.PhaseOut` (`BaseLabor.enabled=false`) | Non : suspendu, reprend au `PhaseIn` | `Labor.cs:993-1006`. Seul appelant dans firstpass : `Labor.Start` si `IsPhasedOut` (`:335-337`) ; appel possible côté Lua non vérifié | jusqu'au `PhaseIn` |

Mort d'un Brictron :
- [O] `Locomotion4.RpcDie` -> `Corpse.Spawn` puis `ObjectPoolSingleton.Release(gameObject)` (`BG/Locomotion4.cs:1544-1560`). Déclenché par `BricktronDamageReceiver._Kill` -> `Locomotion.Kill` (`FP/Brix.Game.AI.Damage/BricktronDamageReceiver.cs:378-390`).
- [O] `Release` : signaux, `ResetComponent` de tous les `IResetable` (`BC/Brix.Lifecycle.Pooling/ObjectPoolSingleton.cs:194-225,293-301`), puis `ReleaseToPool`.
  - `DummyGameObjectPool` : `SetActive(false)` puis `Object.Destroy` (`DummyGameObjectPool.cs:33-43`). `OnDestroy` s'exécute -> `Behavior.OnDestroy` -> `TreeRun.Halt()` -> cascade -> `OnFinally` -> `workers.Remove`.
  - `GameObjectPool` (pool réel, uniquement si le prefab porte un `PoolConfiguration`, `AbstractGameObjectPool`/`ObjectPoolSingleton.GeneratePools:242-265`) : `SetActive(KeepActive)` (`GameObjectPool.cs:58`), pas de `Destroy`. `Behavior` n'a pas de `ResetComponent`, `Labor.ResetComponent` ne fait que `AbortWorkSignal.Clear()` (`Labor.cs:445-449`). L'arbre ne serait jamais Halt : Finally jamais exécuté.
- [D] Les Brictrons sont détruits, pas poolés : `Labor.OnDestroy` est le seul site qui émet `LaborDeleted`/`DestroyedSignal` (`Labor.cs:474-484`, aucune autre émission dans firstpass) alors que ces signaux alimentent l'UI (`UIGameObserver.cs:488`) ; `HookInsructionSignals` est branché dans `Start` (une seule fois) et `ResetComponent` vide le signal : incompatible avec une réutilisation. Non confirmé par les données prefab.
- [D] Détails de l'exécution à la mort : `Task.result` vaut `Succes` par défaut (`TaskResult` valeur 0). `OnFinally` appelle donc `Knowledge.ResetExclusionsFor` (`Task.cs:188-195`) et `EndWork` joue le son de réussite (`Goal.cs:111-121,135-140`). Anodin. `UnassignWorker` ne dépend d'aucun test `IsNullOrReleased` sur le Brictron. Une exception dans un finalizer interne de la cascade (non observée) sauterait le Finally de la Task [D, risque résiduel].

Constat sur la gravité [D] : une entrée fantôme dans `workers` bloque déjà, dans le jeu d'origine, tous les Goals qui testent `HasOtherWorkerAssigned` (Dig, Tree, Repair, Anti-blueprint, etc., cf. tableau des appelants). Ces fuites doivent donc être rares en pratique. Avec le patch, le coût d'une fuite change : un Blueprint de 1 bloc avec une entrée fantôme n'est plus constructible par personne (`others >= max(1, slots)`), alors que l'ancien comportement laissait un autre Brictron le construire.

Verdict point 1 : **RISQUE**. Correctif proposé pour le patch (description, pas d'édition) :
- Remplacer `OtherWorkers(goal, labor)` par un comptage « vivant » : pour chaque `w` dans `workers`, `w != labor` et `w` non nul (`UnityEngine.Object`), `!w.IsNullOrReleased()`, `w.enabled`, et `w` travaille réellement pour ce Goal : `(w.CurrentTask != null && w.CurrentTask.Giver == goal.gameObject && w.CurrentTask.Status` différent de `Finished` et `Failed`) ou `(w.PendingTask != null && w.PendingTask.Giver == goal.gameObject)`. `Giver = goal.gameObject` est posé par `Task.CreateFromGoal` (`Task.cs:246`) et `Task.Status` démarre à `NotStarted` (`Task.cs:40`, enum `TaskProgression`).
- Purger au passage les entrées périmées de `workers` (`RemoveAll`). Liste manipulée uniquement sur le thread principal. Cela corrige aussi `HasOtherWorkerAssigned`/`PriorityFor` pour ce Goal.
- Effet de bord utile : un Brictron en `Finished` (10 ticks) ou `Failed` (30 ticks + handlers) n'occupe plus de place, donc moins de latence.
- Option : ignorer un worker dont `Time.time - Task.TaskStartedTimestamp` dépasse un seuil (Task bloquée sans fin). `TaskStartedTimestamp` est posé au démarrage de la Task (`Task.cs:156`). Risque : un Brictron lointain légitime serait doublé ; seuil à régler.
- Aucun impact réseau (état serveur uniquement, `MultiBehavior.ToTree` ne crée l'arbre que si `NetworkServer.active`, `MultiBehavior.cs:18-25`). Aucune donnée de sauvegarde touchée.

### Point 2 : pool, réutilisation et destruction du Blueprint

Observé [O] :
- `Goal.workers` : `[SerializeField] private List<Labor>` (`Goal.cs:22-23`). Sites d'écriture : `:105` Add, `:130` Remove. Aucun `Clear`.
- `Goal` et `BuildGoal` n'ont ni `ResetComponent`, ni `OnRequested`, ni `OnDestroy` (grep). `GameComponent`/`BrixComponent` ne réinitialisent que leur poco (`BrixComponentPoco.ResetComponent`, `BrixComponent.cs:120-130`). `Release` ne vide donc pas `workers`.
- `BuildGoal` est un composant du Blueprint : même GameObject (`BuildGoalProvider.OnObjectTracked`, `:264`, `GetComponent<BuildGoal>()`).
- Le Blueprint est conçu pour la réutilisation : `Blueprint.OnRequested`/`ResetComponent` (`Blueprint.cs:174-198`). Mais `BuildGoal.Start` branche `DisconnectWhen(OnReleaseSignal)` (`BuildGoal.cs:~81-88`), et `Start` ne s'exécute qu'une fois par composant. [D] Le pool réel pour les Blueprints n'est pas démontré ; dépend de `PoolConfiguration` dans les prefabs (non lus). Traiter les deux cas.
- Destruction du Blueprint pendant des Tasks : `Blueprint.MorphToObject` -> `SafeNetworkDestroy` (`Blueprint.cs:320-363`). Les autres Brictrons voient `contenant.IsNullOrReleased()` dans `NavigateAndCheck` et échouent avec `NullStorage` (`StoreNode.cs:30-34`, déjà noté plus haut). Leur Task suit le chemin 2 : `workers.Remove` sur le Goal capturé après handlers + 30 ticks. `UnassignWorker` ne lit que des champs managés : pas d'exception si le Goal est détruit ou libéré.
- Task déjà en `PendingTask` au moment de la destruction : démarre, `GetWorkNode` a déjà été construit ; sinon `BuildGoal.GetWorkNode` renvoie `FailWork<NullTarget>` si le Goal est libéré à la création (`BuildGoal.cs:~97-101`). Dans les deux cas Finally normal.

Déduit [D] :
- Pool réel : une GameObject libérée peut être redemandée immédiatement (`GameObjectPool.RequestFromPool` prend le dernier enfant du conteneur, `GameObjectPool.cs:72-90`). Les Brictrons dont la Task est encore en attente de Finally (jusqu'à 30 ticks + handlers) restent dans `workers` du Goal « nouveau Blueprint ». Effet : refus temporaire de tous les Brictrons sur ce Blueprint tant que ces Tasks ne se terminent pas. Les entrées sont retirées par leur propre Finally (no-op si absent). Autre effet de bord hors patch : `IsNullOrReleased` repasse à faux après `OnRequested`, une Task périmée pourrait alors livrer dans le nouveau Blueprint.
- Une entrée fantôme (chemins 5, 6, 12) dans un Goal poolé survivrait à `Release` et irait au Blueprint suivant : blocage permanent d'un Blueprint de 1 bloc (cf. point 1).
- `CurrentCapacity` après réutilisation : `Recepteur.ResetComponent` -> `Wipe` vide `_storedItems` mais ne touche pas les descriptions (`Recepteur.cs:372-381,1178-1182`). `ResetDescriptions` n'est appelé que par `Recepteur.Start` et `ClearExclusions` (`:906,1048`). La remise à zéro dépend de `ObjectUntracked` -> `RemoveFrom` (`:1118-1133`). Si elle ne passe pas, `CurrentCapacity` reste à 0 sur un Blueprint neuf. Conséquence pour le patch : `RemainingSlots` renvoie `max(1, 0) = 1`, donc au pire 1 Brictron, jamais zéro. Le plancher protège.

Verdict point 2 : **RISQUE (modéré)**. Correctifs proposés (description) :
- Le comptage « vivant » du point 1 neutralise les entrées périmées (Brictron mort, détruit, pas sur ce Goal).
- Option : postfix sur `Blueprint.OnRequested` (méthode déclarée dans `Blueprint`, `Blueprint.cs:174`) qui vide `workers` du `BuildGoal` du même objet (`__instance.GetComponent<BuildGoal>()`, champ privé via `AccessTools.FieldRefAccess`). Sans danger : un Finally tardif fait `Remove` d'un absent.
- À ne pas faire : postfix sur `BrixComponent.ResetComponent` (chemin chaud, tous les composants).
- Aucun impact réseau ni sauvegarde.

### Point 3 : unité de `CurrentCapacity`

Observé [O] :
- `Recepteur._baseCapacity` : `[SerializeField]`, donc donnée du prefab (`Recepteur.cs:58-62`). Aucune écriture de code hors sérialisation Unity : toutes les occurrences sont des lectures (`:386,514,530,955,984`). Il n'existe pas de calcul de la capacité à partir du volume du Blueprint ni de Factory qui la fabrique à l'exécution.
- Construction du courant : `ResetDescriptions` / `RegenerateDescriptionsWithExclusions` : `_baseCapacityWithExclusions = _baseCapacity - _exclusions`, `_currentCapacity = copie`, puis `Subtract(_contentDescription)` (`Recepteur.cs:948-966,979-996`). Mise à jour incrémentale : `AddTo` soustrait la `Description` de l'objet stocké (`:1167-1176`), `RemoveFrom` rajoute (`:1118-1133`). Cette mise à jour est immédiate au stockage ; seul `InventoryChanged` est différé d'un `FixedUpdate` (`:1186-1215`).
- Unité : `Description.Value(adjectif)` lit `Quantite.valeur` (`Description.cs:529-539`, `Quantite.cs`). Une `Ressource` est une `Quantite` (`Ressource.cs`) ; le constructeur par défaut met `_valeur = 1` (`Quantite.cs:26`). `Description.Subtract` ne soustrait que les `Quantite` de même type (`Description.cs:643-660`).
- `Encombrement` est un `Limite` (`Encombrement.cs`, `Limite.cs`), donc pas une `Ressource`. `GetRessources()` ne renvoie que des `Ressource` (hors outils) (`Description.cs:602-609`). Le patch (qui somme `Value(r)` sur `GetRessources()`) ne compte donc jamais l'encombrement. Le plan A d'origine, qui parlait de `GetProportionEncombrement`, n'est pas ce que le plugin fait.
- `IsFull()` = `CurrentCapacity.NoneOrZeros()` (`Recepteur.cs:539-542`, `Description.cs:371`). Cohérent avec le patch : somme nulle = plein.
- `DealSingleResourceRecepteur` (qui force « un seul type de ressource ») ne s'applique qu'à la palette et aux personnages (`Recepteur.cs:1158-1165`). Pas aux Blueprints. Le drapeau `_acceptMultipleResouceType` d'un Blueprint est celui du prefab.
- `BuildGoal._compoundRecepteur` est assigné (`BuildGoal.cs:~170-182`) mais jamais lu. `ProjectData.BlueprinRecepteurCompound` (`Brix.Game.Storage/CompoundRecepteur.cs`, `BlueprintCompounder.cs:61`) additionne les `BaseCapacity` des Blueprints du projet avec la même unité (`Value(adj) - ContentDescription.Value(adj)`). Il sert à l'UI et au Lua (`LuaProjectStorage.cs:163`), pas au choix des Brictrons.
- Un Brictron prend jusqu'à 20 objets d'une source (`FindAndPickUp(Recepteur, upTo=20)`, `PickUpNode.cs:64`), limité par la capacité de son propre récepteur.

Blueprints concernés [O] : la factory `Blueprints` contient 108 gabarits (`FP/Brix.External.Factories/Blueprints.cs`) : blocs simples (`Stone_Brick`, `Cobble_Brick`, `Clay_Brick`, `Hedge_Block`), multi-voxels (`Stone_DoubleBrick*`, `Stone_Arch`, `Stone_Staircase`, `Stone_Stair*`, colonnes, `Wood_Ramp_*`, `Wood_Beam`), objets (`Door`, `HeavyDoor`, `Window`, `Chest`, `Catapult`, `Forge`, `Workbench`, `BearTrap`, `*Ward`, `Pallet`, etc.), pont de corde (`RopeBridge*`, avec `FindPairedBlueprint` pour les paires de planches, `Blueprint.cs:~370`). Chaque gabarit est un seul GameObject avec un seul `Recepteur`. La capacité de chacun est prefab, non vérifiée : bloc standard = probablement 1 unité d'une ressource ; `Stone_DoubleBrick`, escalier, porte = valeurs non lues.

Déduit [D] :
- Pour un bloc standard : `slots = Value(type)` restant = 1 tant que rien n'est livré, 0 après. Plancher 1. Le calcul donne bien « un seul Brictron » si le bloc vaut 1 unité.
- Pour un objet multi-ressources (porte : bois + fer, etc.) : somme des unités restantes sur tous les types = nombre total d'objets encore attendus, si chaque objet porte `valeur=1`. Si un objet livré porte `valeur=k`, la somme est exprimée en unités de `k` : le patch autorise alors jusqu'à `k` fois trop de Brictrons (jamais trop peu).
- Capacités « au choix » (plusieurs types alternatifs avec `_acceptMultipleResouceType=false`, Blueprint vide) : la somme compte les alternatives, donc le patch est trop permissif pour ce cas seulement. Non vérifié sur les prefabs.
- Un Brictron peut apporter plusieurs objets (limite = capacité de son récepteur) : un worker compte pour 1 alors qu'il peut remplir plusieurs unités. Le test reste conservateur côté permissif, sans blocage.
- Latence : un Brictron qui vient de livrer reste inscrit 10 ticks (chemin 1). Pendant ce temps, `others` le compte alors que son unité est déjà décrémentée : refus transitoire d'un autre Brictron (≤ 10 ticks, puis réessai toutes les ~10 ticks). Corrigé par le filtre `Status` du point 1.

Verdict point 3 : **OK avec réserve**. Le calcul donne le nombre d'unités de `Ressource` encore attendues, qui égale le nombre de blocs si la valeur prefab par objet est 1 (non vérifié). Vérification recommandée en jeu (hors périmètre statique) : un log unique par clé de Blueprint de `BaseCapacity` (type, valeur) au premier `IsCrowded`, puis comparer pour `Stone_Brick`, `Stone_DoubleBrick`, `Stone_Staircase`, `Door`. Pas de correctif de code obligatoire.

### Point 4 : sémantique Motus de Prepare / Send

Observé [O] :
- Chemin : `Labor.SendProjectInstruction` est une Sequence `Wait OffsetWaitTime` -> `VerifyWorkFor` -> `GetInstruction` -> `AssignInstruction` (`Labor.cs:638-652`). `AssignInstruction` construit un `Selector.If(pkg != null).Do(Prepare).Do(Send)` (`Labor.cs:654-668`).
- `Brancher.Builder.AddNode` regroupe deux `Do` successifs d'une même branche dans un `SequenceNode` (`Brancher.cs:698-720`). `Prepare` est un `DynamicNode` : la fonction `_package.Prepare(self)` s'exécute dans `AbstractDynamicNodeRun.Init` (`DynamicNode.cs:38-42`), et `OrderPackage.Prepare` appelle `prepare(orderReceiver)` immédiatement (`OrderPackage.cs:94-101`). C'est ce moment que B (wrap du delegate de `GetPrepareFunction`) teste. Puis `Hire`.
- `SequenceNodeRun.TickSequenceNodes` : `while (_nodeRun.Run() == Done && ++idx < count) { spawn suivant }` (`Sequencer.cs:94-102`). Après `Prepare` Done (noeud vide : `EmptyNodeRun` est Done d'emblée, `EmptyNode.cs`), le noeud `Send` est exécuté dans la même boucle, donc dans le même `Run`/`Tick`. `Send` (`ActionNode.Tick` exécute et passe Done, `ActionNode.cs:48-60`) -> `WorkInstruction<,T>.Send` -> `GetTask` = `Goal.WrapTask` -> `workers.Add` (`Goal.cs:100-109`) -> `SendOrder` -> `PendingTask` (`WorkInstruction.cs:88-100`, `BaseLabor.cs:250-262`). Aucun `WaitFor` entre les deux.
- Si le test refuse : `Node.Fail(FailException.Generic)`. Le noeud `Fail` passe en état Fail dans son `Init`, la Sequence s'arrête avant `Send` : ni `WrapTask`, ni Task (`Failer.cs`, `OrderPackage.cs:96-99`).
- Un tick de Brictron = `Behavior.BrixFixedUpdate` -> `Tick` -> `TreeRun.Run` (`Behavior.cs:40-74`), appelé séquentiellement par `GameClock.FixedUpdateSignal.UnorderedSafeInvoke` (boucle `for` avec try/catch, `GameClock.cs:83-98`, appelé dans `FixedUpdate` `:381`). Pas de thread ni de coroutine côté arbre.

Déduit [D] :
- Entre le test de B et `workers.Add` il n'y a ni yield Motus, ni autre Brictron : le test et l'inscription sont atomiques vis-à-vis des autres arbres. Deux Brictrons ne peuvent pas passer le test B simultanément pour la dernière unité. Seul A (postfix `IsMatchFor`) reste évalué dans `SelectBestGoal`, des ticks avant `Prepare` (étape 7 du tableau principal) : A est indicatif, B est la vérification qui fait foi.
- La chaîne `GetInstruction` -> `AssignInstruction` s'enchaîne dans le même tick à la fin de `SelectBestGoal` (même boucle `Sequencer`), donc l'écart entre A et B est surtout dû à `WaitFor` internes de la sélection.
- `Hire` (appelé dans `original(labor)` après le test de B) peut appeler `Order.Move.HaltWhatever.Send` (`WorkForceProvider.OnObjectTracked`, `WorkForceProvider.cs:213`) qui pose un `PendingTask` d'arrêt. Ce `PendingTask` est remplacé par la Task de construction juste après (chemin 6), sans conséquence : la Task d'arrêt n'a pas de Goal, donc pas d'entrée dans `workers`. Le test de B se fait avant `Hire`, donc sans effet de bord en cas de refus.
- Le chemin d'échec de B est celui d'un échec ordinaire du même noeud : `GetInstruction` renvoie déjà `Node.Fail(FailException.Generic)` (`Project.cs:1043-1045`) et `Prepare` renvoie le même Fail quand `Hire` répond faux. Capturé par le `Catcher` de `TryWorkInProject` (`Labor.cs:627-636`). Le comportement au-delà (`Node.Fail(error)` en fin de boucle de `CheckProjectForWork`) n'a pas été tracé jusqu'à la racine de l'arbre ; il est identique à celui d'un échec vanille au même noeud.
- Les ordres directs du joueur (`Order.*.Send(..., User)`) n'appellent ni `IsMatchFor` ni `Prepare` : ni A ni B ne s'appliquent. Voulu : ordre explicite.
- Échanges de Tasks (`ConflictResolver`) re-`Send` sans `Prepare` non plus, mais l'effectif par Goal reste constant : sans risque pour le plafond.

Verdict point 4 : **OK**. Aucun correctif. Précision à garder : B est l'unique test atomique ; A est un filtre d'économie (évite un `Hire` et des `Prepare` inutiles).

### Correctifs proposés (récapitulatif, patch non modifié)

1. Remplacer `OtherWorkers` par un comptage vivant + purge (points 1 et 2). Priorité haute : c'est le seul scénario où le patch peut rendre un Blueprint de 1 bloc définitivement non constructible.
2. Option : vider `workers` à `Blueprint.OnRequested` (point 2, pool réel).
3. Option : seuil d'ancienneté `TaskStartedTimestamp` (point 1, Task bloquée).
4. Log de diagnostic de `BaseCapacity` par clé de Blueprint (point 3), pour lever la réserve sur les valeurs prefab.
5. Aucune modification de A/B nécessaire pour le point 4.

### Zones floues restantes

- Valeurs prefab : `BaseCapacity` des 108 Blueprints, présence de `PoolConfiguration` sur Blueprint et Brictron (donc pool réel ou non), valeur `valeur` des objets livrés.
- Propagation d'un `Node.Fail(error)` en sortie de `CheckProjectForWork` jusqu'à la racine (`ParallelSequencer` en `WhenAllDone` par défaut) : non tracée ; sans effet sur le verdict car identique au vanille.
- Appel de `Labor.PhaseOut` côté Lua : non cherché.

## Ramassage multi-blocs (palettes)

Vocabulaire : Brictron = `Labor`, bloc = élément de construction. Chemins : `FP` = `decomp/firstpass/`, `BG` = `FP/Brix.Game.AI/`, `BN` = `FP/Brix.Game.AI.Nodes/`, `BS` = `FP/Brix.Game.Semantique/`. [O] = lu dans le code (fichier:ligne), [D] = déduit. Source firstpass = `Managed/Assembly-CSharp-firstpass.dll.orig` (la DLL installée est patchée).

Symptôme analysé : 3 palettes (Blueprint `Pallet`, capacité `PlankBlock`=1 chacune), 3 Brictron partent ; le premier ramasse plusieurs planches, pose la 1re palette puis enchaîne les deux autres ; les deux autres arrivent sur des palettes déjà construites.

### 1. Quantité ramassée

Séquence [O] :

| # | Étape | Méthode (fichier:ligne) | Quantité |
|---|---|---|---|
| 1 | Noeud de travail | `BuildGoal.GetWorkNode` (`BG/BuildGoal.cs:106-126`) : si `labor.recepteur.FirstThatFitsIn(recepteur) != null` -> `GoStoreIn` direct (`:120-123`), sinon `FindAndPickUp(aBuildGoal.recepteur)` puis `GoStoreIn` (`:124-125`) | `upTo` non passé -> défaut |
| 2 | Surcharge Recepteur | `PickUpNode.FindAndPickUp(Labor, Recepteur toFill, int upTo = 20)` (`BN/PickUpNode.cs:64-67`). Seul appelant de cette surcharge dans firstpass : `BuildGoal.cs:124` (grep). Les autres surcharges : `LandscapeGoal.cs:208` (Description, 20), `CraftingInstructions.cs:50` (Description, 20 par défaut), `CatapultOperations.cs:182` (Profile, 1) | `upTo = 20` (constante compilée dans la lambda) |
| 3 | Choix de la source | `Labor.Find(Recepteur)` (`BG/Labor.cs:697-700`) = `FirstThatFitsIn(target) ?? ClosestResource(target.CurrentCapacity)` ; `ClosestResource` (`:712-738`) parcourt les `Ressource` de la capacité restante du Blueprint, `qty = item.quantifiable.valeur` est **calculé puis ignoré** (seul `closest` est utilisé, `:716-718`) ; `FindOneResource` (`:740-744`) -> `Knowledge.ClosestObjectToPickup` (`FP/Brix.Game.AI.KnowledgeSpace/Knowledge.cs:236-247`) : objet au sol de ce type sans parent, ou récepteur qui `Carries(ressource)` ; distance par `KnowledgeUtility.GetPickupDistance` | 1 objet cible (`item`), pas un nombre |
| 4 | Noeud de ramassage | `NodeFindAndPickUp` (`:74-81`) -> `GoPickUp(item, upTo)` (`:15-18`) -> `_GoPickUp` (`:29-57`) : `ReserveUntil`, `RunToExactly`, `TakeSome(labor, item, upTo)` (`:45`). Gardes : `!labor.recepteur.HasRoomFor(item)` -> `ObjectDoesNotFitInDestination` (`:47-48`) | |
| 5 | Prise | `TakeSome` (`:83-91`) : refus si l'objet est dans un Blueprint ; `labor.transaction.StoreEverything(item.gameObject, labor.gameObject, upTo)` (`:89`) | |
| 6 | Transfert | `Transaction.StoreEverything` (`BG/Transaction.cs:65-98`) : `parentRecepteur = item.ParentRecepteur`. (a) objet dans un stock/palette (`parent != null`, `parent.gameObject != item`) -> `TransferSpecificObjects(parent, laborRecepteur, item, upTo)` (`:100-108`) : liste de **tous** les objets de même `AssetKey` du stock -> `TakeFromEnumerable`. (b) objet au sol (`parent == null`) et `upTo != 1` -> `SimilarSurroundingItems` (`:129-149`) : objets de même `AssetKey` dont le volume touche les voxels `obstacle`/`access` de l'objet -> `TakeFromEnumerable`. (c) `upTo == 1` -> un seul `AddObject` | |
| 7 | Boucle | `TakeFromEnumerable(destination, key, liste, upTo)` (`:151-164`) : `while (destination.HasRoomFor(key) && (upTo == -1 || num < upTo)) AddObject` | **n = min(upTo, |liste|, place dans le récepteur du Brictron)** |
| 8 | Place du Brictron | `Recepteur.HasRoomFor(AssetKey)` (`BS/Recepteur.cs:621-624`) -> `CurrentCapacity.PeutPrendre(Description)` (`FP/Description.cs:326-348`) : chaque adjectif de la capacité vote ; `Ressource.PeutPrendre` = `valeur >= valeur de l'objet` (`BG/Ressource.cs:55-62`) ; `Encombrement : Limite` = `valeur >= encombrement de l'objet` (`BG/Limite.cs:10-13`). `CurrentCapacity` du Brictron = `_baseCapacity` (prefab, `Recepteur.cs:58-62`) moins le contenu (`Recepteur.AddTo`, `:1167-1176`) ; le Brictron est mono-ressource : `DealSingleResourceRecepteur` (`:1158-1165`) + `IntersectWith` dans `AddTo` (`:1171-1174`) | |

Formule [O] : **n = min(20, nombre d'objets de même `AssetKey` dans la source (stock entier ou amas au sol), nombre que le récepteur du Brictron accepte encore)**. La place du Brictron = min sur les adjectifs de sa capacité restante (nombre de `Ressource` du type et `Encombrement`), valeurs prefab non lues [zone floue].

Ce qui n'intervient **pas** [O] :
- la capacité du Blueprint visé : `toFill` ne sert qu'à choisir le type d'objet (`Find`), jamais à borner `upTo` ;
- la demande du projet (autres Blueprints) : aucune lecture de `BlueprintCompounder`/`CompoundRecepteur` ni de `Provider` dans ce chemin ;
- le sac à dos : `Accessories.Backpack` est la poche à bombes de l'alchimiste (`Profession.cs:383,416`, `PickUpNode.GoPickUpAmmo :99`), sans effet sur `Recepteur` ;
- « deux mains » : `Recepteur.RequiresBothHands` (`:600-609`) ne sert qu'à l'animation/prop, pas à la capacité.

Dépôt [O] : `StoreNode.StoreIn` (`BN/StoreNode.cs:29-35`) -> `StoreEverything(labor, blueprint)` sans `upTo` (-1) -> `ParentRecepteur` du Brictron = son propre récepteur (`GameComponentPoco.cs:82-88`) -> `TransferWhatever(laborRecepteur, blueprintRecepteur, -1)` (`Transaction.cs:110-127`) : `break` dès `destination.IsFull()`. Une palette (capacité 1) prend 1 planche ; le reste **reste en main**. `Blueprint.OnReceivedNewItem` (`FP/Brix.Game.Components/Blueprint.cs:306-315`) -> `MorphToObject` -> `SafeNetworkDestroy` (`:318-363`).

### 2. Enchaînement vers le Blueprint suivant

| # | Étape | Méthode (fichier:ligne) | Hooks du correctif |
|---|---|---|---|
| 1 | Fin de Task | `Task.CoreNode` : `Status=Finished`, `WaitForTicks(DelayOnFinished=10)` (`BG/Task.cs:207-215`) ; `TaskEnded` -> `Labor.OnTaskEnded` : `Activity = Idle` (`BG/Labor.cs:1094-1103`) ; Finally -> `workers.Remove` (`Goal.cs:124-133`) | - |
| 2 | Boucle IA | `LookForWorkSequence` (`Labor.cs:535-555`) : `LookingForWork` = Idle && `IsAvailable()` (`:558-561`, `BaseLabor.cs:186-192`) -> `CheckProjectForWork` (`:574-591`) -> `RefreshAvailableProjects` (`:593-611`) : projet courant, puis projets de la faction triés, puis `IdleProject` | - |
| 3 | Projet | `TryWorkInProject` (`:613-636`) : `HasWorkFor` = `HasGoals() && HasRoomFor(labor)` (`Project.cs:701-704, 806-816` ; membre du crew : passe même si plein) -> `SendProjectInstruction` (`:638-652`) : `VerifyWorkFor` (`Project.cs:706-722`) -> `VerifyResourceAccessFor` : `labor.Find(description)` = `FirstObjectOf(desc) ?? ClosestResource` (`Labor.cs:692-695`) : **satisfait par les planches en main** | - |
| 4 | Candidats | `BuildProject.SelectBestGoal` (`BG/BuildProject.cs:64-67`) -> `goals.FulfillableBy(labor)` : `FulfillableEnumerator` (`BG/BuildGoalProvider.cs:53-67`) : si `labor.recepteur` **non vide**, `fetcheableUnroll = clés portées` (`:63-66`), sinon ressources atteignables ; `BuildGoalFinder.Visit(IVolume)` (`BG/BuildGoalFinder.cs:303-328`) : BFS de chemin depuis le Brictron, retient les BuildGoal `valid`+`buildable`+`Commited`+enfant du projet dont `recepteur.HasBaseCapacityForOneOf(Fetchables)` | - |
| 5 | Filtre | `GoalSelector.ProcessGoalAndReturnIsBest` (`BG/GoalSelector.cs:196-214`) : `IsMatchFor` puis `PriorityFor` ; `IsBetter = HasBetterPriorityOrAccess` (`chooseWithAccess: true`, `BuildProject.cs:18`, `GoalSelector.cs:232-250`) | - |
| 6 | `BuildGoal.IsMatchFor` (`BuildGoal.cs:232-287`) | `Find(recepteur)` = planche en main (`FirstThatFitsIn`, `Recepteur.cs:840-843`) ; `labor.recepteur.HasRoomFor(o)` vrai par `Carries(o)` (`Recepteur.cs:776-786`) -> `true` (`:279-283`) | **`Hooks.AfterIsMatchFor`** sur chaque `ret` (`Loader.cs:82-97`) : `IsCrowded` = autres workers vivants ≥ `max(1, unités restantes)` |
| 7 | `BuildGoal.PriorityFor` (`:205-211`) | `-10` si autre worker, hauteur ×10, `-100` si `AdjacentLower`, `-distance` | - |
| 8 | Instruction | `Project.GetInstruction` -> `_GetInstruction` : `goal.Package` + `SetPrepare(GetPrepareFunction(goal))` (`Project.cs:1039-1064`) -> `AssignInstruction` : `Prepare` puis `Send` (`Labor.cs:654-668`) | **`Hooks.WrapPrepare`** (`Loader.cs:99-108`) : même test juste avant `Hire` |
| 9 | Task suivante | `Goal.WrapTask` -> `Task.CreateFromGoal` -> `GetWorkNode` : branche **`FirstThatFitsIn` -> `GoStoreIn` direct** (`BuildGoal.cs:120-123`), sans nouveau ramassage | - |
| 10 | Aucun Goal accepté | `_SelectBestGoal` : `GoalIsRejected` -> `Fail(GoalUnavailable)` (`GoalSelector.cs:139-146`) -> `Catch` de `TryWorkInProject` (`Labor.cs:627-636`) -> projet suivant -> `IdleProject.HasWorkFor` = true (`IdleProject.cs:198-201`) ; `DecideWhatToDo` : récepteur non vide -> `GoStore` (`:76-81`) -> `Order.Transfer.GoStoreOrDropAt` (`:100-104`, `Order.cs:120`) -> `NestedGoStoreInBestStorageOrDrop` (`StoreNode.cs:53-64`) -> `BestRecepteurToStoreInventory` (`Labor.cs:757-764`) -> `Knowledge.ClosestStorageWithRoomFor` (entrepôt, `Knowledge.cs:224-234`) ; sinon dépôt au sol (`DropNode.Drop` après 5 essais) | - |

Conclusions :
- [O] Vanille : le porteur enchaîne les palettes sans repasser par le stock (étape 9) ; `IsMatchFor` ne le refuse jamais (pas de test de saturation hors `IsAnti`). Les deux autres Brictron trouvent le Blueprint détruit (`NavigateAndCheck` -> `NullStorage`, `StoreNode.cs:22`), échouent (30 ticks), puis rangent leur planche via `IdleProject.GoStore` (étape 10).
- [O] Avec le correctif v1.1 : le porteur passe bien par `AfterIsMatchFor` et `WrapPrepare` (hooks sur les `ret`, sans condition sur le contenu du récepteur). Sur une palette où un autre Brictron (encore sans planche) a une Task vivante (`IsLive` : `CurrentTask.Giver == goal.gameObject`, `Hooks.cs:330-339`), `others = 1 >= slots = max(1, 1)` -> `GoalTooCrowded`. S'il reste une palette libre, il y va ; sinon (étape 10) il rapporte ses planches à l'entrepôt le plus proche, ou les dépose au sol. Il ne les garde jamais « pour plus tard » : `GatherAtIdlePoint` exige un récepteur vide (`IdleProject.cs:203-206`).
- [D] Bilan 3 palettes / 3 Brictron avec le correctif : A ramasse n planches (n ≤ min(20, stock, capacité)), pose la palette 1, est refusé sur 2 et 3 (B et C inscrits), rapporte n-1 planches au stock. B et C ramassent **chacun** jusqu'à n planches, posent 1 palette et rapportent le reste. Plus de trajet « pour rien », mais 3 allers-retours chargés au lieu d'un, et du stock déplacé inutilement.

### 3. Projet : regroupement et demande totale

- [O] `BuildProject` porte un `BuildGoalProvider goals` (`BuildProject.cs:9-10, 20`). Le provider tient `blueprints : HashSet<Blueprint>` (tous les Blueprints suivis, `BuildGoalProvider.cs:150`, `TrackedBlueprints :167`) et `goals : LayeredGoals` (seulement les `buildable`, `:136, 274, 302-314`). `All()` (`:317-320`, public) énumère ces `BuildGoal`. 1 Blueprint = 1 `BuildGoal` = 1 `Recepteur` (même GameObject). `BuildGoal.Provider` est public (`BuildGoal.cs:48`), `Goal.project` protégé.
- [O] Demande totale par ressource : `Project.ProjectData` (champ public, `Project.cs:123`) -> `ProjectData.BlueprinRecepteurCompound` (`FP/Brix.Game.Storage/ProjectData.cs:30`) -> `BlueprintCompounder.QtyNeeded(adj)` = `BaseCapacity.Value(adj) - ContentDescription.Value(adj)` du `CompoundRecepteur` (`Brix.Game.Storage/BlueprintCompounder.cs:59-62`). Le compound additionne `BaseCapacity`/`CurrentCapacity` de **tous** les `TrackedBlueprints` (`:35-36`, `CompoundRecepteur.Add :243-271`) et suit `OnResourceAdded/Removed` (`:322-334`). Il inclut donc les Blueprints non `buildable`/non `Commited`. Pour la demande « travaillable », sommer `goal.recepteur.CurrentCapacity.Value(res)` sur `Provider.All()` filtré `!IsAnti && Commited` est plus juste [D]. `UniqueItemsNeeded` (`BuildGoalProvider.cs:331-337`) ne donne que les clés, pas les quantités.
- [O] Ce que les Brictron transportent : `labor.recepteur.ContentDescription.Value(res)` ; ce qu'un Brictron peut encore prendre : `Recepteur.CanFitUpTo(key)` (`Recepteur.cs:612-619`, `CurrentCapacity.Value(principale)`, sans l'encombrement) ; sa Task : `BaseLabor.CurrentTask`/`PendingTask` (`BaseLabor.cs:18-20`), `Task.Giver` = GameObject du Goal (`Task.cs:246`), `Task.Status`. Brictron engagés sur un projet : `Goal.workers` (privé, déjà lu par réflexion dans `Hooks.OtherWorkers`) de chaque `BuildGoal` de `Provider.All()`, ou le crew `WorkForceProvider.GetAs<WorkForce>()` (`HashSet<Labor>`, `WorkForceProvider.cs:121`, `WorkForce.cs:7`) via `project.GetComponent<WorkForceProvider>()`. Le crew contient aussi des Brictron sans Task de construction (membres au repos) : filtrer par `CurrentTask.Giver`.
- [O] Il n'existe aucune réservation « en cours de transport » dans le jeu : `ReserveUntil` ne concerne que la source (cf. verdict), et rien ne décrémente la demande du projet avant le dépôt effectif (`Recepteur.AddTo`).

### 4. Pistes de correction

Cadre : réécriture Cecil de firstpass, hooks sur `ret` (`Loader.WrapReturns`, `Loader.cs:115-137`) ou en entrée de méthode (même technique : insérer `ldarg…, call Hooks.X, starg` en tête du corps), appels statiques vers `CastleStory.BuildGoalFix.Hooks`. Tout est côté hôte (`MultiBehavior.ToTree` -> `NetworkServer.active`, cf. 03) : aucun message réseau nouveau, aucune donnée de sauvegarde (`workers`, Tasks, ledger non sérialisés, cf. point 1 chemin 11).

| Piste | Point(s) d'injection | Calcul | Faisabilité | Effet / risques |
|---|---|---|---|---|
| **P1** Limiter au Blueprint visé | Entrée de `PickUpNode.FindAndPickUp(Labor, Recepteur toFill, int upTo)` (`PickUpNode.cs:64`, surcharge appelée **uniquement** par `BuildGoal.GetWorkNode`) : `upTo = Hooks.ClampToBlueprint(labor, toFill, upTo)`. Variante plus fraîche : entrée de `TakeSome(Labor, GameComponent item, int upTo)` (`:83`), goal = `labor.CurrentTask.Giver.GetComponent<BuildGoal>()`, ne rien faire si null/`IsAnti` | `min(upTo, max(1, Σ_res toFill.CurrentCapacity.Value(res)))` ; avec l'objet connu : `toFill.CanFitUpTo(item.AssetKey)` | Simple (3 `ldarg` + `call` + `starg.s 2`). Méthode statique, pas de lambda à toucher | Règle le cas palette avec le correctif actuel (1 planche par Brictron, 1 palette chacun). **Régression de débit** : 1 Brictron seul pour 10 palettes fait 10 allers-retours au lieu de ~1 (vanille enchaîne grâce aux planches en main). Ne traite pas les gros Blueprints (3 Brictron × capacité 10 = surplus) |
| **P2** Refus au niveau projet | `Hooks.WrapPrepare` (existant, `Project.GetPrepareFunction`), avant `original(labor)` ; optionnel en plus dans `AfterIsMatchFor` avec cache par provider et par `Time.frameCount` (sinon O(goals²) par sélection) | Pour chaque `res` de `goal.recepteur.CurrentCapacity` : `restant = Σ_{g ∈ Provider.All(), !IsAnti, Commited} g.recepteur.CurrentCapacity.Value(res)` ; `couvert = Σ_{w vivant sur un BuildGoal du même provider, w != labor} (w porte ? w.recepteur.ContentDescription.Value(res) : min(20, w.recepteur.CanFitUpTo(key)))` ; refuser si `couvert >= restant` pour toutes les ressources du goal | Réutilise `IsLive`/purge existants ; `Provider.All()`, `CanFitUpTo`, `ContentDescription` sont publics | Empêche B et C de partir si A couvre la demande. Mais sans P3, A prend n = min(20, stock, capacité) planches pour 3 palettes : surplus rapporté au stock (étape 10). « Prévu » est une **estimation haute** (capacité prefab, encombrement ignoré par `CanFitUpTo`) : sous-recrutement possible sur un gros Blueprint tant que le porteur n'a pas ramassé ; corrigé dès le ramassage (contenu réel). Plancher : jamais de refus si aucun worker vivant |
| **P3** Ramassage borné par la demande résiduelle du projet + P2 | (a) P2 dans `WrapPrepare` ; (b) entrée de `TakeSome` (`PickUpNode.cs:83`) : `upTo = Hooks.ClampToProject(labor, item, upTo)` ; (c) optionnel : `ret` de `TakeSome` ou entrée de `StoreNode.StoreIn` pour journaliser le contenu réel | (b) : `min(upTo, max(1, restant(res) − couvert_autres(res)))` avec les mêmes sommes qu'en P2, calculées **au moment du ramassage** (le Brictron est au stock, `CurrentTask` est la Task de construction, `Giver` -> `BuildGoal` -> `Provider`) | Deux hooks d'entrée + le hook `WrapPrepare` existant. `TakeSome` est statique, signature stable ; filtrer sur `Giver` BuildGoal non-anti pour ne pas toucher Landscape/Crafting/Catapulte | 3 palettes / 3 Brictron : A seul est recruté (P2) et prend exactement 3 planches (P3), enchaîne les 3 palettes (branche `FirstThatFitsIn`). 10 palettes / 1 Brictron : prend min(10, capacité). Gros Blueprint : plusieurs Brictron, chacun borné par ce qui reste à couvrir. Résidu possible si un autre Brictron ramasse entre-temps (fenêtre entre deux `TakeSome`) : rangé par `IdleProject.GoStore`, comme en vanille. Pas de blocage : plancher 1 et refus P2 seulement si `couvert >= restant` |

Interaction avec le test par Goal (v1.1) : à conserver. Avec P2, le cas « porteur refusé partout » devient rare (les autres ne sont recrutés que si le porteur ne couvre pas la demande). Si un Brictron est quand même refusé en portant des blocs, il les rapporte au stock (étape 10) : coût borné, pas de perte.

Recommandation : **P3** (P2 + borne au ramassage), avec P2 placé dans `WrapPrepare` (atomique vis-à-vis des autres arbres, cf. point 4) et la borne dans `TakeSome` (valeur la plus fraîche, un seul site, scope limité par `Giver`). P1 seul est à écarter (régression de débit pour un Brictron isolé).

Zones floues :
- Capacité prefab du récepteur des Brictron (`_baseCapacity` : nombre par ressource et `Encombrement`), encombrement d'une planche : « prévu » de P2 n'est qu'un majorant tant que le ramassage n'a pas eu lieu.
- Ordre d'arrivée des 3 Brictron dans `Prepare` : P2 refuse les suivants selon l'estimation du premier ; si l'estimation est trop basse (capacité réelle < demande), un seul Brictron est recruté jusqu'à son ramassage, puis les autres passent à la boucle suivante (10 ticks).
- `KnowledgeUtility.GetPickupDistance` (choix de la source) non lu ; sans incidence sur la quantité.
- `Recepteur.CanFitUpTo` ignore l'encombrement : utiliser plutôt une boucle `HasRoomFor(key)` simulée n'est pas possible sans modifier l'état ; accepter le majorant.

## Limite de port d'un Brictron

Chemins : `FP` = `decomp/firstpass/`, `S` = `FP/Brix.Game.Semantique/`, `AI` = `FP/Brix.Game.AI/`. [O] = lu (fichier:ligne) ou extrait des données Unity, [D] = déduit. Les données Unity ont été lues par `re/tools/unity_adjectives_dump.py` (parseur minimal de serialized file v17, lecture seule, rien installé ; UnityPy absent).

### Verdict

La limite n'est ni codée en dur ni calculée au ramassage : c'est une **donnée de prefab** (`Recepteur._baseCapacity` du GameObject `Bricktron`), consommée par un test générique de quantité par `Ressource`. L'encombrement n'intervient pas pour un Brictron. [O]

**Un Brictron porte 3 planches (`PlankBlock`), 1 bloc de pierre (`StoneBlock`), 1 bloc de bois brut (`WoodBlock`)**, un seul type de ressource à la fois. [O]

### 1. Chaîne du test « peut encore prendre »

| # | Étape | Référence | Effet |
|---|---|---|---|
| 1 | `Transaction.TakeFromEnumerable(dest, key, liste, upTo)` : `while (dest.HasRoomFor(key) && num < upTo) AddObject` | `AI/Transaction.cs:151-164` | clé = `AssetKey` du premier objet, test refait avant chaque objet |
| 2 | `Recepteur.HasRoomFor(AssetKey)` -> `HasRoomFor((IDescriptor)Factory.Peek(key).GetComponent<Apparence>())` | `S/Recepteur.cs:621-624`, `:794-797` | description du **prefab** de l'objet (`Apparence.description`, sérialisée) |
| 3 | `_currentCapacity.PeutPrendre(d)` -> `Description.PeutPrendre(Description autre)` | `FP/Description.cs:321-348` | rejette si l'objet a `Structure` ou `ProjectAdj` ; sinon chaque adjectif de la **capacité** vote : `Echec` = refus immédiat, `Succes` = accept immédiat, `Positif` = mémorisé ; résultat = au moins un `Positif` et aucun `Echec` |
| 4 | Vote d'une `Ressource` (`PlankBlock`, `StoneBlock`, ...) | `AI/Ressource.cs:50-62` | si l'objet a le même type : `capacité.valeur >= objet.valeur` -> `Positif`, sinon `Echec` ; type absent de l'objet -> `Neutre` (type de base `Ressource` : accepte tout sous-type). Pas de pondération : `valeur` = nombre d'unités |
| 5 | Vote d'un `Encombrement` (`Limite`) | `AI/Limite.cs:10-13` | `Positif` si absent de l'objet ou `capacité >= objet`, sinon `Echec`. **Le Brictron n'a pas d'`Encombrement` dans sa capacité** (voir 2) : aucun vote |
| 6 | Capacité courante | `ResetDescriptions` `S/Recepteur.cs:948-966`, `Regenerate…` `:978-996`, `AddTo` `:1167-1176`, `RemoveFrom` `:1120-1133` | `current = base - exclusions - contenu` (`Description.Subtract` ne soustrait que les `Quantite` de même type, `FP/Description.cs:654-660`) ; si récepteur non vide et non multi-type : `IntersectWith(contenu)` met à 0 les autres types (`:632-642`, `SetToZero :691-701`) |
| 7 | Mono-ressource | `DealSingleResourceRecepteur` `S/Recepteur.cs:1158-1165` (palette et `Labor` seulement) | après le 1er objet, les autres types tombent à 0 |
| 8 | `Apparence.PrincipaleOf` | `S/Apparence.cs:154-163` | 1re `Ressource` de la description de l'objet (cache `Initialize`). Sert à `CanFitUpTo`, `CarryType`, `Find`, pas à `HasRoomFor` |

Formule [O+D], pour un Brictron non exclu, portant `c` objets du type T et recevant un objet de description D :
- accepté ⇔ `base_cap[T] - c*D[T] >= D[T]` ET aucun autre objet d'un autre type n'est porté ;
- donc `max_T = floor(base_cap[T] / D[T])`. Ici `D[T] = 1` pour tous les blocs (voir 2), donc **`max_T = base_cap[T]`**.
- Encombrement : ignoré (absent de la capacité du Brictron). Il ne borne que les palettes/coffres/entrepôts.

### 2. Origine des valeurs (données Unity)

- `Recepteur._baseCapacity` : `[SerializeField]` `Description` -> `DicoAdjectif` -> chaîne JSON `adjectives` + `md5` désérialisées par Json.NET (`FP/DicoAdjectif.cs:~112-137`, `S/Recepteur.cs:58-62`). Aucune écriture dans le code (lectures seules `:386,514,530,955,984` ; hors `Recepteur.cs` : `StaticRecepteurReader.cs:88,143`, `CraftingInventory.cs:44`, `BuildGoalFinder.cs:320`). Pas de `[JsonProperty]` : non sauvegardée, donc non modifiable par une sauvegarde. [O]
- `Quantite._valeur = 1` par défaut (`AI/Quantite.cs:26`), `[SerializeField][JsonProperty]` (`:8-10`). [O]
- **Brictron** : `Castle Story_Data/level7`, GameObject `Bricktron` (pathID 62 ; MonoBehaviour `Recepteur` pathID 1144, script `(1,2432)`), probablement la factory `Test.Bricktron` (`Brix.External.Factories/Test.cs:15`, [D] pour le lien factory/GameObject). `_baseCapacity` (valeurs `_valeur` = `valeur`) [O, données] :

| Type | Capacité | | Type | Capacité |
|---|---|---|---|---|
| `PlankBlock` | **3** | | `Stones`, `OrangeCrystal`, `RawIron`, `Plant` | 20 |
| `StoneBlock` | **1** | | `Bomb` | 10 |
| `WoodBlock` | **1** | | `Iron`, `Glass`, `Terracotta`, `PurifiedBrimstone` | 3 |
| `Rope`, `Fabric`, `Cog`, `Pumpkin`, `KettleHat`, `Outil`, `BearTrap`, `ExplosiveBarrel`, wards (4 types + `PylonWardAdj`) | 1 | | `Soulevable` | présence (non quantifiable) |

  Pas d'`Encombrement` ni de `Libre` dans cette liste. Le seul `Recepteur` du GameObject est le pathID 1144 (liste des composants vérifiée). Aucun autre GameObject `Bricktron`/`Labor` portant un `Recepteur` dans `level*`.
- **Objets** (`Apparence.description`, script `(1,1835)`) [O, données] : tous les objets bloc ont `valeur = 1` pour leur ressource ; seul l'`Encombrement` diffère :

| Objet (factory `Blocks`, level15/17/21) | Description | Occurrences |
|---|---|---|
| blocs de pierre (`Stone_Brick`, `Stone_DoubleBrick`, `Clay_Brick`, escaliers, arches, colonnes, merlons...) | `Libre`, `Encombrement 20`, `StoneBlock 1` | 46 |
| planches (`Wood_SinglePlank`, `Wood_DoublePlank_AA`, `Wood_Beam`, rampes `Wood_Ramp_*`, `RopeBridgePlank`) | `Libre`, `Encombrement 6`, `PlankBlock 1` | 17 |
| `Wood_VerticalBeam` | `Libre`, `Encombrement 20`, `PlankBlock 1` | 1 |
| bois brut (`Wood_Stump`, `Wood_SingleLog`, `Wood_DoubleLog_AA`) | `Libre`, `Encombrement 20`, `WoodBlock 1` | 3 |
| granulaires (`Stones`, `Iron`, cristaux, `Plant`, `Clay`, `RawGlass`) (level9) | `Encombrement 1`, `Granulaire`, `Libre`, `StoredGeneric` | 7 |

  Un bloc double (`Stone_DoubleBrick`) compte donc comme un bloc simple (`StoneBlock 1`). La valeur d'encombrement n'est définie dans aucune classe : c'est une donnée prefab, `Adjectif.encombrement` (`AI/Adjectif.cs:313`) n'est qu'un prototype.
- Contenants, pour comparaison [O, données] : `Palette` (level6) `StoneBlock 4`, `WoodBlock 4`, `PlankBlock 12`, `Encombrement 80`, 80 par granulaire ; `Chest` `Encombrement 10` + 10 par granulaire ; monstres (`Biftron`, `Minitron`, `Crawler`...) `Ressource 1`, `Encombrement 20`, `Soulevable`. Blueprints (capacités de construction) : `StoneBlock 1` ou `PlankBlock 1` pour les blocs simples (cohérent avec le log `LogCapacity`).
- Constantes : seules `20` (`upTo` par défaut, `PickUpNode.cs:64`) et `-1` (`StoreEverything`) bornent en dur ; elles ne sont que des plafonds supérieurs aux capacités ci-dessus.

### 3. Modificateurs

| Facteur | Effet sur la limite | Référence |
|---|---|---|
| Occupation | `Labor.OnOccupationChange` : `Builder.IsWorker()` -> `ClearExclusions` (capacité pleine) ; toute autre occupation -> `DoExcludeAll` (capacité mise à 0 : un combattant ne porte rien) | `AI/Labor.cs:459-468`, `AI/Builder.cs:136-139`, `S/Recepteur.cs:1041-1070` |
| Exclusions | `AddExclusion/MakeExclusive` retirent des types de la capacité (`ExceptWith`) ; utilisées par l'UI/ToolRack (`ToolRack.cs:322`, `UNetProjectCmd.cs:848`) ; ne peuvent que **réduire** | `S/Recepteur.cs:955-957,984-986` |
| Contenu déjà porté | un autre type porté -> 0 pour le type demandé (mono-ressource) | `:1158-1165,1171-1174` |
| Outils (`Toolbag`) | **aucun** : le `Toolbag` est un autre tracker, ne consomme pas la capacité du `Recepteur` ; changer d'outil exige un récepteur vide | `AI/Toolbag.cs:551-558`, `AI.Nodes/ToolNode.cs:118` |
| Deux mains | `RequiresBothHands` (animation/prop, interdit d'utiliser un outil) : n'affecte pas la capacité | `S/Recepteur.cs:~600-609`, `AI/Profession.cs:269-284` |
| Sac (`Accessories.Bag`, `BagStuff`) | choix du prop porté ; pas de capacité supplémentaire | `S/Apparence.cs:32-42`, `S/Recepteur.cs:~700-726` |
| Adjectifs de l'objet | seuls `Structure`/`ProjectAdj` (refus) et le type de ressource comptent ; `Encombrement` ne compte que si la capacité en possède | `FP/Description.cs:326-348` |
| Lua / gamemode | **aucun** : pas d'API Lua écrivant `BaseCapacity` (`04-api-lua-ref.tsv`: Recepteur n'expose que `FillWithRessource`, `GetQuantity`, `IsFull`... en lecture/remplissage) ; `grep -i "capacity\|encombr\|carry" Info/Lua` ne renvoie que `backernames.json` | [O] |
| Sauvegarde | `_baseCapacity` non sérialisée en sauvegarde (pas de `[JsonProperty]`) ; `_exclusions` l'est | `S/Recepteur.cs:58-72` |

### 4. Lire les valeurs

- Statique : fait. `python3 -I re/tools/unity_adjectives_dump.py "Castle Story_Data" [filtre_nom]` (parseur maison, UnityPy non installé ; les index de script `(1,1835)`/`(1,2432)` sont propres à ce build). Résultats ci-dessus.
- Runtime (description seulement) : `Hooks` peut logger une fois par `AssetKey` : `labor.recepteur.BaseCapacity`, `CurrentCapacity` et `Factory.Peek(key).GetComponent<Apparence>().Description` (valeurs de `Quantite` par type). Ne pas compter sur `Encombrement`.

### 5. Conclusion et fonction à appeler depuis un hook

- Nombre max d'un type T pour un Brictron vide : `BaseCapacityWithExclusions.Value(T) / D[T]` = **3 `PlankBlock`, 1 `StoneBlock`, 1 `WoodBlock`** [O données] (aucune exclusion pour un `Builder`).
- Place restante en temps réel : `n_restant = CurrentCapacity.Value(T) / max(1, D[T])`, où `D[T]` = valeur de la `Ressource` principale dans `Factory.Peek(key).GetComponent<Apparence>().Description`, avec 0 si `IsFull()` ou si un autre type est porté. [D]
- Fonction la plus fiable : `Recepteur.CanFitUpTo(AssetKey)` (`S/Recepteur.cs:611-619`) = `IsFull() ? 0 : CurrentCapacity.Value(PrincipaleOf(Peek(key)))` : exacte pour le Brictron (valeur par objet = 1, encombrement absent), utilisée en vanilla (`NatureHarvestGoal.cs:104`, `LandscapeGoal.cs:167`). Réserve [D] : `Apparence._principale` est renseigné par `Initialize()` ; non vérifié sur un template `Peek` hors instance, donc entourer d'un `try/catch` ou retomber sur `CurrentCapacity.Value(typeof(PlankBlock)/…)` lu par le type du `Principale` d'une instance.
- Une simulation `PeutPrendre`/`HasRoomFor` répétée n'est pas utilisable sans modifier l'état (aucun `AddObject` à blanc) ; inutile car la formule est fermée. Pour une borne générale (objets avec `Encombrement` dans la capacité, ex. coffre) : `min(CurrentCapacity.Value(T)/D[T], CurrentCapacity.Value<Encombrement>()/D[Encombrement])` si la capacité contient l'`Encombrement`.
- Conséquence pour le correctif P3 : la « place du récepteur » du Brictron vaut 3 pour des planches (palette = 1 planche : 2 en surplus par trajet), 1 pour la pierre. Le cas à 1 bloc (pierre) n'a donc pas de ramassage multiple ; le problème palette est propre aux planches (et aux granulaires). Remplacer le majorant `min(20, CanFitUpTo)` du P2 par `CanFitUpTo(key)` (≤ 3) est exact pour `PlankBlock`.
- Risques réseau/sauvegarde : lecture seule de valeurs prefab côté hôte ; aucune écriture de `_baseCapacity` ne serait propagée (non synchronisée, non sauvegardée) : ne pas la modifier pour changer la limite.

## Test v1.3.1 et correctif v1.3.2 (2026-10-17, 20:45–20:52)

### Résultats v1.3.1

| Série | Embauches | Échecs | Ramassages | Verdict |
|---|---|---|---|---|
| 3 palettes | 6 | 2 | Pebble 3 | 1 porteur au final, Sierra embauchée à vide pour rien |
| 10 palettes | 19 | 7 | 3+1+3+3+2 = 12 | Fonctionne, ~2 blocs de surplus, beaucoup de bruit |
| 2 pierres | 3 | 1 | 1+1 | OK |
| 3 pierres | 8 | 5 | 1+1+1 | OK au final, 5 courses perdues |
| 1 + 2 pierres | 1, 3 | 0 | 1, 1+1 | Propre |

### Défaut 1 : porteur juste après une pose toujours non compté

Trois occurrences (`20:45:13` Pebble=2, `20:45:28` Sierra=2, `20:45:40` Euporie=1) : à la recomposition qui suit une pose, `couvert 0 []` → un Brictron vide est embauché, puis le porteur arrive ~1 s plus tard et prend le même Blueprint (`2 workers / 1 slots`, exemption porteur).

Cause (`Hooks.AddCarriers`) : la Task du porteur est encore en cours (`Status` ni Finished ni Failed) et `task.Giver` n'est pas null, mais le Blueprint transformé n'a plus de `BuildGoal` valide → `GetComponent<BuildGoal>()` null (ou libéré / sans Provider) → `continue`. Le commentaire v1.3.1 supposait `Giver == null`.

Correctif v1.3.2 : ignoré seulement si `working != null && !working.IsNullOrReleased() && working.Provider != null && working.Provider != provider`.

### Défaut 2 : échecs en rafale (15 `WorkerFailed`), cause vanilla

| Exception | Mécanisme | Référence |
|---|---|---|
| `ObjectHasBeenMovedFromRecepteur` | `Labor.FindOneResource` = `ClosestObjectToPickup` puis `recepteur.FirstObjectOf(res)` : tous les Brictron envoyés au même stock visent le **même** objet ; un seul l'obtient. `_ClosestObjectToPickup_Test` n'interroge jamais `IsReserved` (seuls `TreeGoal.cs:97` et `CleanUpGoal.cs:57` l'utilisent) | `AI/Labor.cs:740-744`, `KS/Knowledge.cs:236-246`, `S/Recepteur.cs:750-753` |
| `ObjectIsInSomeoneElseBag` (échec instantané à l'embauche) | le filtre accepte tout conteneur `recepteur.Carries(ressource)` sans exclure `Fonction == personnage` : la source choisie est le sac d'un Brictron porteur voisin, puis `PickUpNode` refuse | `KS/Knowledge.cs:245`, `AI.Nodes/PickUpNode.cs:51-52` |

La réservation vanilla (`PickUpNode.cs:44`) porte sur le **conteneur** (`parentRecepteur ?? item`) jusqu'au `WorkEnded` du Brictron (`Goal.UnassignWorker`, `Goal.cs:127`), soit toute la tâche : l'honorer sur un stock sérialiserait les ramassages.

Correctif v1.3.2 (`PickupFilter`, 3 hooks IL) :

| Hook | Point | Effet |
|---|---|---|
| E `AfterPickupTest` | chaque `ret` de `Knowledge._ClosestObjectToPickup_Test` | rejette les sacs de Brictron (`Fonction == personnage`) et les objets **isolés** (`recepteur == null`) réservés ; conteneurs réservés toujours admis |
| F `AfterFindOneResource` | chaque `ret` de `Labor.FindOneResource` | si l'objet rendu est réservé et vient d'un conteneur : premier objet non réservé du conteneur (`StoredItems` + `Apparence.PrincipaleIs`), sinon `null` → `CannotFindWhatINeedToPickUp` sans déplacement |
| G `OnGoPickUp` | entrée de `PickUpNode._GoPickUp` | `Knowledge.ReserveUntil(item, labor.signal.WorkEnded)` : réserve l'objet lui-même, même durée que la réservation vanilla |

Limite connue : le test E est statique (pas de `Labor`), un Brictron qui recherche à nouveau pendant sa propre tâche saute son propre objet réservé et en prend un autre ; sans conséquence.

### Défaut 3 : borne de ramassage au-dessus du port

`borne 7` pour Sierra alors que la place PlankBlock = 3 (§ Limite de port). Sans effet (le port tronque). v1.3.2 : `limit = min(limit, recepteur.CanFitUpTo(item.AssetKey))` si > 0 ; journal `borne X (demandé Y, port Z)`.

### Build et vérification

`dotnet build -c Release` dans l'ordre `BuildGoalFix.Loader` → `BuildGoalFix.Patcher` → `BuildGoalFix` (le Patcher référence la DLL compilée du Loader : sans rebuild du Loader, l'ancien patch est réappliqué silencieusement), puis `patch/install.sh`. Vérifié : `clrmeta.py --methods` identique entre `.orig` et patché ; `ilspycmd` montre les appels `Hooks.AfterPickupTest`, `Hooks.AfterFindOneResource`, `Hooks.OnGoPickUp` aux bons endroits.

## Test v1.3.2 et correctif v1.3.3 (2026-10-17, 22:35)

| Série | Embauches | Échecs | Ramassages | Verdict |
|---|---|---|---|---|
| 3 palettes | 6 | 1 | Euporie 3 | 1 porteur ; Hanjo et Ada embauchés à vide juste après chaque pose |
| 10 palettes | 13 | 4 | 3+1+3+3+3 = 13 | 19 → 13 embauches, 7 → 4 échecs, surplus ~3 |

Plus aucun `ObjectIsInSomeoneElseBag` (hook E efficace) ; retargetage F observé 9 fois ; borne/port corrects ; aucune erreur de hook. Tous les échecs restants = `ObjectHasBeenMovedFromRecepteur`.

### Cause réelle du défaut 1 : le registre des porteurs n'a jamais fonctionné

`ClampPickup` (entrée de `TakeSome`) appelle `RegisterCarrier` puis `ComputeStats` **avant** la prise effective : `AddCarriers` voyait `recepteur.IsEmpty()` et retirait le porteur aussitôt. Le registre était donc vide à chaque recomposition (`couvert 0 []`) ; les conditions « travaille ailleurs » des v1.3 → v1.3.2 n'ont jamais été exercées. v1.3.3 : un porteur vide n'est retiré que s'il n'a pas de Task en cours (vide + Task en cours → ignoré sans retrait).

### Cause des échecs `Moved` restants : cache par frame de décision

`Labor.Find` → `resourceCache` (`UnbindedCache` clé `decisionFrame`, `AI/Labor.cs:148`) : la cible est trouvée à l'évaluation du Goal (avant l'embauche : logs « réservé » de Brictron non embauchés) et réutilisée telle quelle quand l'arbre s'exécute. Entre les deux, un autre Brictron a réservé et pris le même objet ; F n'a rien vu (objet libre à l'évaluation) et G réservait un objet déjà parti.

v1.3.3 : G devient remplaçant, `item = Hooks.OnGoPickUp(labor, item)` en entrée de `_GoPickUp`. Si l'objet est réservé par un autre (table `ReservedBy` pour reconnaître sa propre réservation) → premier objet libre du même conteneur (`FirstFreeSibling`, partagé avec F) ; aucun → `null` → `CannotFindWhatINeedToPickUp` sans déplacement. Puis réservation de l'objet jusqu'à `WorkEnded`.

### Outillage

- `patch/build.sh` : builds `--no-incremental` (un build incrémental a produit un dossier Patcher sans `runtimeconfig`/dépendances → `libhostpolicy.so` introuvable) puis `install.sh`.
- `install.sh` : copie de `CastleStory.BuildGoalFix.dll` atomique (tmp + `mv`), installable jeu en cours ; effet au prochain lancement.

## Test v1.3.3 et correctif v1.3.4 (2026-10-17, 22:49)

| Série | Embauches | Échecs | Ramassages | Verdict |
|---|---|---|---|---|
| 3 palettes | 3 | 0 | Ada 3 | Parfait : `couvert 2 [Ada=2]` dès la recomposition |
| 10 palettes | 14 | 4 | 1+3+3+3+1 = 11 | Couverture exacte en fin, 1 bloc de surplus |

Registre des porteurs opérationnel (défaut 1 clos). Deux causes pour les 4 échecs restants :

| Exception | Cause | Correctif v1.3.4 |
|---|---|---|
| `ObjectHasBeenMovedFromRecepteur` ×2 | `Transaction.TransferSpecificObjects` (Transaction.cs:100-108) ignore l'objet désigné et prend les `upTo` **premiers** objets de même clé du conteneur : Ada (borne 1) a pris le (10) réservé par Hanjo au lieu de son (11) | Hook H, entrée de `Transaction.TakeFromEnumerable` : `enumerable = Hooks.OrderPickup(destination, enumerable)` → réservés par ce Brictron (`ReservedBy`), puis libres, puis réservés par d'autres. Couvre conteneurs et objets isolés groupés (`SimilarSurroundingItems`) |
| `ObjectIsInSomeoneElseBag` ×2 | hook G : objet du cache déjà ramassé → `item.ParentRecepteur` = sac du ramasseur → `FirstFreeSibling` y choisissait un objet | G : si réservé par un autre **ou** parent `personnage` → recherche fraîche `Knowledge.ClosestObjectToPickup(labor, res, faction)` (filtre E) puis premier objet libre du conteneur trouvé ; aucun → null |

Outillage : `build.sh` rebuild le Patcher seul si sa sortie n'a pas de `runtimeconfig.json` (le build enchaîné après celui du Loader laisse parfois un dossier incomplet, reproduit deux fois).

## Test v1.3.4 et correctif v1.3.5 (2026-10-17/18, 22:58 → 00:08, session réelle de 70 min)

12 ordres (palettes, pierres, Furnace, Workbench, Toolrack, Loom, séries de 12-13 blocs, construction 15-25 Blueprints, 2 démolitions) : 236 embauches, 84 ramassages, **0** `Moved` / `SomeoneElseBag` (objectif v1.3.4 atteint), 11 `CannotFindWhatINeedToPickUp` (échec immédiat quand plus aucune pierre libre, équivalent vanilla sans trajet), 4 `PathFailed` (hors sujet). Porteurs comptés partout.

### Régression v1.3.4 (hook G) : boucles d'échec hors chantier

1 062 lignes « déjà dans le sac », dont 1 059 = **le propre sac du Brictron** (`Wood_DoubleLog_AA` 797, `Rope` 218, `RawFiber` 44). Les flux transport/artisanat appellent `_GoPickUp` avec un objet déjà porté ; vanilla répond `Node.Empty` via `labor.recepteur.Carries(item)` (PickUpNode.cs:35), mais G s'exécute avant : parent `personnage` → recherche fraîche → `null` → échec → ré-embauche → … Boucles : Janus/Rope 128 échecs/min (23:19), Hanjo/bûches 309/min (00:01). 636 échecs forcés, 423 retargetages inutiles.

Second défaut, par lecture : E excluait **tous** les sacs de Brictron, y compris celui du chercheur ; vanilla trouve son propre sac (distance 0) et utilise ce qu'il porte.

### Correctif v1.3.5

| Hook | Changement |
|---|---|
| G | si `parent == labor.recepteur` ou `labor.recepteur.Carries(item)` → objet rendu tel quel |
| I (nouveau) | entrée de `Knowledge.ClosestObjectToPickup(source, …)` → `Hooks.BeginSearch(source)` mémorise le chercheur |
| E | sac de Brictron admis seulement si c'est celui du chercheur |

Note : `OnEndWork` ne journalise pas les succès des BuildGoal normaux (seulement échecs et démolitions) : le chemin de succès ne passe pas par `Goal.EndWork` ou passe par un `EndAction` différent ; à vérifier si le journal des succès devient utile.

## Test v1.3.5 et correctif v1.3.6 (2026-10-18, 00:16 → 00:28)

| Ordre | Embauches | Ramassages | Échecs | Verdict |
|---|---|---|---|---|
| 9 palettes | 9 | 3 × 3 = 9 | 0 | Parfait |
| 7 palettes | 7 | 0 (porteurs déjà chargés) | 0 | Parfait |
| 1 palette | 1 | 1 | 0 | Parfait |
| 15 blocs pierre + porte | 35 | 28 | 7 `CannotFind` | Correct, 7 courses perdues |

Régression v1.3.4 close (0 « déjà dans le sac », 0 `Moved`, 0 `SomeoneElseBag`, 0 erreur). **Le bug de construction est réglé pour les palettes.**

Rendement pierre : les blocs sont stockés sur des palettes à faible contenu ; quand les objets de la palette la plus proche sont tous réservés, F (126 « cible aucune ») et G (5) rendent `null` → échec immédiat → ré-embauche 1-2 s plus tard. La recherche fraîche de G retombe toujours sur la même palette (la plus proche), alors que d'autres ont des pierres libres.

Correctif v1.3.6 : E n'admet un conteneur (hors sac du chercheur) que si `FirstFreeSibling` y trouve un objet de la ressource non réservé ou réservé par le chercheur (`ReservedBy`) ; `ClosestObjectToPickup` renvoie donc la palette libre la plus proche. `FirstFreeSibling` prend un paramètre `self` (F : `labor`, G : `labor`, E : `_searcher`).
