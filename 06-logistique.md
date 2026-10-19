# 06 - Logistique : transport de ressources et prise en charge des travaux

Racine : `D` = `re/decomp/firstpass/`. Tous les types sont dans `Assembly-CSharp-firstpass`. Lignes = fichier décompilé en cache.
[O] = lu dans le code, [D] = déduit (non confirmé dynamiquement). Vocabulaire : Brictron = `Labor`, bloc = objet posé.
Complète `05-bug-construction.md` (BuildGoal) ; hooks existants : `re/patch/BuildGoalFix/Hooks.cs` (A à I, voir §3.3).

## 0. Architecture commune (valable pour toutes les familles)

| Étape | Méthode | Fichier:ligne |
|---|---|---|
| Boucle de décision d'un Brictron | `Labor.LookForWorkSequence` → `CheckProjectForWork` | `Brix.Game.AI/Labor.cs:535,574` |
| Liste candidate : Project courant, Operable courant, tous les `IOperable` de la faction triés par `-PriorityInput*PriorityWeights`, IdleProject en dernier | `Labor.RefreshAvailableProjects` | `Labor.cs:593-612` ; `ProjectDatabase.cs:65-73` |
| Test grossier | `IOperable.HasWorkFor(labor)` | `Labor.cs:623` |
| Attente anti-rafale 1 à 15 ticks | `Labor.OffsetWaitTime` | `Labor.cs:288-303,644` |
| Vérif. stock/stockage du projet | `IOperable.VerifyWorkFor` → `Project.VerifyWorkFor` (`BestRecepteurToStore`, `Labor.Find`) | `Project.cs:706-804` |
| Choix : Project → `SelectBestGoal` (`IsMatchFor`, `PriorityFor`) ; station → `DecideWhatToDo` | `GoalSelector.ProcessGoalAndReturnIsBest` ; `CraftingStation.GetInstruction` | `GoalSelector.cs:196-214` ; `CraftingStation.cs:511` |
| Package + prepare (= embauche) | `Project._GetInstruction` + `GetPrepareFunction` ; `CraftingStation.GetPackage` (`SetPrepare(Hire)`) | `Project.cs:1050-1064` ; `CraftingStation.cs:485-509` |
| `Prepare` puis `Send` | `Labor.AssignInstruction` | `Labor.cs:654-665` |
| Création de la Task | goal : `Goal.WrapTask` → `Task.CreateFromGoal` (Giver = goal, `EndAction = goal.EndWork`, `workers.Add`) ; sinon : `Task.GetTaskFromInstruction` (Giver = null, `EndAction = BaseLabor.DefaultEndWork`) | `Goal.cs:100-109` ; `Task.cs:242-268` |
| Envoi | `WorkInstruction<,>.Send` (pose `InitialPackage`) → `Labor.SendOrder` | `WorkInstruction.cs:88-100` ; `Labor.cs:1085` |
| Fin | `Task.OnFinally` (toujours) → `EndAction` (`Goal.EndWork` ou `DefaultEndWork`) → `TaskEnded` | `Task.cs:191-205` |

Conséquence [O] : `Goal.EndWork` (hook D) ne voit **que** les Tasks nées d'un Goal enregistré dans un Project. Les ordres du joueur (`DecisionSource.User`), l'artisanat, l'IdleProject et les Goals « jetables » (`WorkNode.Dig/Repair`, `HarvestNode.HarvestNatureAt/TransformLog`, `WorkNode.CutTree`, `DestroyIfAbandonned`) utilisent `GetTaskFromInstruction` : pas de `Giver`, pas de `workers`, pas d'`IsMatchFor`, pas d'embauche de Project (`HarvestNode.cs:60-66`, `WorkNode.cs:23-64`).

Station d'artisanat : ce n'est **pas** un Goal ni un Project. `CraftingStation : IOperable<Labor>` (`CraftingStation.cs:25`), enregistrée dans `ProjectDatabase` (`:295`, serveur seulement), un seul opérateur (`MaximumCrewSize => 1`, `:251`).

## 1. Artisanat (Loom, Workbench, MachineShop, Lab, Furnace, Forge)

Six sous-classes de `CraftingStation` : seuls changent `CanStackOrders` (true pour Furnace uniquement), `PriorityWeights` (`Furnace` ou `WorkBench`), `WorkAnimation`, effets, et `Loom.FindResourceFunc` (`Loom.cs:53`). `SetDefault` met `AutoStore = true` partout.

### 1.1 Séquence reconstituée

| # | Événement | Classe.méthode | Fichier:ligne |
|---|---|---|---|
| 1 | Commande joueur / Lua : file de recettes (max 8) | `UNetProjectCmd.CmdcraftingQueueRecipe` / `CmdcraftingProduce` → `CraftingLabor.QueueRecipe` / `ProduceRecipe` (boucle) | `Brix.Game.Network/UNetProjectCmd.cs:756,769` ; `CraftingLabor.cs:96-115` |
| 2 | Besoins en ingrédients recalculés | `CraftingStation.OnDataChanged` → `CraftingInventory.SetRecipes/SetRecipe` → `RefreshAmounts` (`ingredientsToFetch = (requis plafonné à la capacité - contenu)`) | `CraftingStation.cs:299-320` ; `CraftingInventory.cs:49-98` |
| 3 | La station « cuisine » : sa propre séquence `Queue` (prio 10) dépile la recette et envoie la Task `Cook` à **CraftingLabor** (BaseLabor, pas un Brictron) | `CraftingLabor.ProcessQueue` → `PopRecipe` → `CookPackage(..).Send` | `CraftingLabor.cs:71,80-95` ; `CraftingInstructions.cs:27,33` |
| 4 | Un Brictron décide | `TryWorkInProject` → `CraftingStation.HasWorkFor` (= `Operator == null \|\| == labor` ET `DecideWhatToDo != Nothing`) | `Labor.cs:614-623` ; `CraftingStation.cs:376-404` |
| 5 | `DecideWhatToDo` (ordre de priorité) : `CanReach` → `CanStore` (Pickup) → `CanFetch` (GoFetch) → `NeedsWork && CanWork` (Work : profession Builder, mains libres) | `CraftingStation.cs:407-453` |
| 6 | Package : **Pickup** = `Order.Transfer.GoPickUp(produit, -1)` ; **GoFetch** = `FindIngredients(station.StuffToFetch())` ; **Work** = `WorkOnRecipe(recette)`. `SetPrepare(Hire)` | `CraftingStation.cs:485-509` ; `CraftingInstructions.cs:38-46` |
| 7 | Embauche : `AssignInstruction` → `Prepare` → `CraftingStation.Hire` → `IOperableExtender.Hire` : `labor.AssignToIdle()` (le Brictron passe dans l'IdleProject), `labor.Operable = station`, éjecte l'ancien opérateur, `Operator = labor`. Toujours `true` | `Labor.cs:654` ; `CraftingStation.cs:571` ; `IOperableExtender.cs:7-17` ; `Labor.cs:184-209` |
| 8 | Task : `OperableInstruction.GetTask` → `Task.GetTaskFromInstruction` (Description = « FindIngredients » / « WorkOnRecipe » / « GoPickUp », **Giver null**, `ValidateFunc = station.IsStillAlive`) | `OperableInstruction.cs:14-20` ; `Task.cs:258` |
| 9 | **GoFetch** : `FindAndPickUp(Description, upTo=20)` puis `GoStoreIn(station.recepteur)` | `CraftingInstructions.cs:48-51` |
| 9a | Recherche : `Labor.Find(Description)` = d'abord ce que le Brictron porte (`FirstObjectOf`), sinon `ClosestResource` → cache `resourceCache` (clé : frame de décision) → `FindOneResource` → `Knowledge.ClosestObjectToPickup` | `PickUpNode.cs:69-81` ; `Labor.cs:692,712-744` ; `Knowledge.cs:236-246` |
| 9b | Source admise : objets libres hors sac, ou conteneurs (Palette, Toolrack, Furnace, ChoppingBlock…) contenant la ressource ; l'**intérieur d'une station** n'est une source que pour ses produits (`NotInCraftingInventory`) | `Knowledge.cs:50-59,243-252` |
| 9c | Ramassage : `GoPickUp` → `_GoPickUp` (réserve le **conteneur**, `RunToExactly`, `TakeSome`) → `Transaction.StoreEverything(item, labor, upTo)` → `TransferSpecificObjects` → `TakeFromEnumerable` | `PickUpNode.cs:15-57,83-91` ; `Transaction.cs:65-164` |
| 9d | Livraison : `GoStoreIn` = `NavigateAndCheck` (cours, `NullStorage`, `InventoryBecameEmpty`, `StorageBecameFull`) puis `StoreIn` → `StoreEverything(labor, station)` → `TransferWhatever` | `StoreNode.cs:11-35` ; `Transaction.cs:110-127` |
| 10 | **Work** : `WorkNodeGenerator` (lieu = `OperableLocation(station)`, boucle), action = `Recipe.WorkOn()` (`work--`) ; fini quand `!NeedsWork` ; `WorkStarted/WorkEnded` autour | `CraftingInstructions.cs:80-122` ; `Recipe.cs:128` |
| 11 | **Cuisson** (Task `Cook`, boucle 30 ms) : si `IsReadyToFinish` et place → `station.Finish(recette)` ; sinon si chaleur requise → `HeatFrom` (consomme 1 cristal orange, `Process(BrimstoneBurnTimeMs)`) | `CraftingInstructions.cs:53-72` ; `Recipe.cs:163` |
| 12 | **Produit** : `CraftingLabor.FinishRecipe` → `Recipe.Finish` (serveur) : `ConsumeAllFrom` (objets libérés/dé-spawnés) + `ProduceInto` (`Transactor.SpawnUNet`, `AddObject` dans la station, ou `ScatterObjects` si `DropOnCompletion`) ; retire la recette, la ré-enfile si boucle | `CraftingLabor.cs:143-152` ; `Recipe.cs:86-171` |
| 13 | **Stockage du produit** : si `AutoStore` (vrai par défaut) la station propose `Pickup` (exige `BestRecepteurToStore(produit) != null`) ; le Brictron ne livre pas lui-même : il rejoint ensuite l'IdleProject qui lui donne `GoStoreOrDropAt` (voir 1.3) | `CraftingStation.cs:428-443` ; `IdleProject.cs:76-80,100-105` |

[D] pour 13 : la chaîne « Pickup station → Idle `GoStore` » est déduite de `RefreshAvailableProjects` (IdleProject ajouté si `TakeOrdersFromIdleProject`, vrai pour `FreeAgent` et `LockedToOperable`, `AutonomyStatus.cs:145-184`) et de `IdleProject.DecideWhatToDo` (sac non vide et `KnowsAbout`). Aucun code d'artisanat ne livre le produit.

### 1.2 Où est « embauché » qui ?
- Opérateur unique par station. `HasWorkFor` teste la place, **`GetInstruction` ne la reteste pas** (`CraftingStation.cs:511-518`) et `IOperableExtender.Hire` retourne toujours `true` en éjectant l'opérateur en place (`Fire`, `:13-14`). [D] Entre `HasWorkFor` et `Prepare` il y a 1 à 15 ticks (`OffsetWaitTime`) : un second Brictron peut prendre la station et en chasser le premier (`Labor.Operable` setter → `Order.Move.HaltWhatever.Send`, `Labor.cs:194`).
- `PriorityInput` : pas de terme distance dans les poids (`ProjectPriority.WorkBench/Furnace`, `ProjectPriority.cs:155-169`) ; `isCurrent`×3 et `hasCleanup`×10 (produit à récupérer) favorisent l'opérateur en place.

### 1.3 Exceptions d'échec (artisanat)
`CannotFindWhatINeedToPickUp` (rien trouvé, `PickUpNode.cs:33,78`), `NullTarget`, `ObjectDoesNotFitInDestination`, `ObjectHasBeenMovedFromRecepteur`, `ObjectIsInSomeoneElseBag`, `ObjectHasBeenConverted` (`PickUpNode.cs:47-56`), `NullStorage`, `InventoryBecameEmpty`, `StorageBecameFull` (`StoreNode.cs:22-26`), `CantReach` (`WorkNodeGenerator.cs:124`), famille `PathException`. Traitement : `Task.Catches` → `TaskResult.WorkerFailed/PathFailed`, `Labor.HandleWorkException` → `Project.OnLaborError` (journal seulement) (`Task.cs:175-240` ; `Labor.cs:1067-1073`). Pas de réaffectation : le Brictron redécide après `DelayOnFail = 30` ticks.

### 1.4 Défauts constatés
1. [D] Course d'embauche sur la station (1.2).
2. [O] `FindAndPickUp(Description, upTo=20)` : une seule ressource par Task (la première de `ingredients.GetRessources()`, `Labor.cs:725-736`), plafond 20 au lieu du manque réel de la station ; le surplus reste dans le sac car `TransferWhatever` s'arrête quand la station est pleine (`Transaction.cs:116-120`). [D] Il repart ensuite par l'Idle `GoStore`.
3. [O] `Loom` teste `CanFetch` avec `FindComplete` (`Loom.cs:53`) mais le nœud ramasse avec `Find` (`CraftingInstructions.cs:50`).
4. [O] Station : `KnowledgeReserver` n'est pas consulté pour choisir la source ; seuls les hooks E/F/G/H du correctif existant le font, et ils s'appliquent déjà à l'artisanat (flag `PickupFilter` global).
5. [O] `Recipe.Restart` met `heat = info.work` (`Recipe.cs:61-66`) : probable faute de frappe, appelant non trouvé dans firstpass.

## 2. Familles de Goal (tableau)

Chaîne commune : Project → `SelectBestGoal` (§0) → `Goal.Package` → `Project.GetPrepareFunction` (`goal.ConnectProject` + `Project.Hire`) → `Goal.WrapTask` → `goal.GetWorkNode(labor)` → fin `Goal.EndWork` (retire de `workers`).

| Goal (fichier) | Project (sélecteur) | Nœud de travail | Ramassage | Dépôt | Fin / remarques |
|---|---|---|---|---|---|
| `BuildGoal` (`BuildGoal.cs`) | `BuildProject` (LocalMaximum) ; aussi `RepairProject` | porte déjà quelque chose qui rentre → `GoStoreIn(goal)` sinon `FindAndPickUp(goal.recepteur)` + `GoStoreIn(goal)` (`:120-125`) ; anti : casse/creuse | `FindAndPickUp(Recepteur, 20)` | `GoStoreIn(blueprint)` | Blueprint plein → transformé en bloc ; voir `05` |
| `BuildStairsGoal` | `TunnelProject` (VoxelGoalsStorage) | `WorkNodeGenerator` : `RemoveCroute` + `SpawnStairs` (`BuildStairsGoal.cs:44-94`) | aucun | aucun | `FinishedSignal`, `ReleaseWhen = Ended\|Invalid` |
| `RepairGoal` | `RepairProject` (CompareAll) | `damageReceiver.Heal` en boucle (`RepairGoal.cs:22-27`) | aucun (exige sac **vide**, `:60`) | aucun | `WorkDoneCondition = !IsDamaged` |
| `TransformGoal` | aucun : créé par ordre joueur `HarvestNode.TransformLog` (`:85-95`) | `SpawnEm` 4 `Wood_SinglePlank` au sol + détruit le tronc (`TransformGoal.cs:118-127`) | aucun | produit au sol (ramassé ensuite par un `CleanUpGoal`) | Goal jetable, `IsMatchFor` jamais appelé |
| `DigGoal` (StorableGoal) | `MineProject` (CompareAll), `TunnelProject` ; jetable via `WorkNode.Dig` | dégâts au voxel (`DigGoal.cs:167-184`), `GarnotteEater` absorbe (`EatGarnotteWhileWorking`) | entrée **non transactionnelle** : `GarnotteEater.AbsorbGarnottes` → `Recepteur.AddObject` (`GarnotteEater.cs:94-103`) | aucun dans le Goal : Idle `GoStore` plus tard [D] | `FinishedSignal` ; refus si sac plein (`BagTooFullToDig`) |
| `NatureHarvestGoal` (StorableGoal) | `HarvestProject` (GoodEnoughInTypeCategory) ; jetable via `HarvestNode.HarvestNatureAt` | `ApplyDamage` à la nature + `NatureRegistry.HarvestAt` (`:134-155`) | idem Dig (garnottes) | Idle `GoStore` [D] | `OnWorkInvalid` → `NoCorrespondingStorage` / `InventoryIsFull` (`:166-177`) |
| `TreeGoal` | `HarvestProject` (TreeGoalProvider) ; jetable via `WorkNode.CutTree` | `DamagedTree.Strike` (`TreeGoal.cs:37-52`) | aucun : les troncs tombent au sol | `CleanUpGoal` ensuite | `WorkDone` = arbre mort/absent |
| `LandscapeGoal` (StorableGoal) | `LandscapeProject` (GoodEnoughFromSample) | **Dig** : `ReceiveAttack(60)` + garnottes ; **Fill** : `PrepareAction` = `FindAndPickUp(stones, 20)` puis `AddInGround` + `RemoveGarnotes` (détruit les pierres) (`LandscapeGoal.cs:205-280`) | Fill : `FindAndPickUp(Profile, 20)` ; Dig : garnottes | Fill consomme ; Dig : Idle `GoStore` [D] | `VoxelWorkIsDone` |
| `CleanUpGoal` (StorableGoal) | `CleanupProject`, `HarvestProject`, `RepairProject` (CleanUpGoalProvider, recalcul tous les 10 ticks) | `GoPickUp(this, -1)` (`CleanUpGoal.cs:15-18`) | `GoPickUp` direct sur l'objet | Idle `GoStore` [D] | `IsMatchFor` exige `BestRecepteurToStore != null` (`:47`) |
| `LitterGoal` (StorableGoal) | `CleanupProject`, `HarvestProject` (LitterGoalProvider) | `eater.EatOnly(EatThis)` + `RunToExactly` (`LitterGoal.cs:36-47`) | garnottes absorbées | Idle `GoStore` [D] | `IsValid` = garnottes restantes |
| `IdleProject.GoStore` | `IdleProject` | `Order.Transfer.GoStoreOrDropAt` → `NestedGoStoreInBestStorageOrDrop` (5 essais puis `Drop`) (`IdleProject.cs:100-105` ; `StoreNode.cs:53-69`) | aucun | `GoStoreInBestStorage` : `BestRecepteurToStoreInventory` → `GoStoreIn` (`StoreNode.cs:37-41`) | pas d'embauche (`Hires` faux pour GoStore, `IdleProject.cs:163`) ; `Drop` = `Recepteur.DropAll` |
| `DirectOrderProject` (classe statique) | aucun Project | commandes joueur → `UNetProjectCmd.Cmd*` → `Order.Transfer.*` / `Order.ForLabor.*` avec `DecisionSource.User` | `GoPickUp`, `GoPickUpFrom`, `SmartTransfer` (`UNetProjectCmd.cs:393,499`) | `GoStoreInto`, `GoDropAt` (`:373,383`) | `Labor.SendOrder` : `LeaveOperation` + autonomie réinitialisée (`Labor.cs:1089-1093`) |

Projets sans transport propre : `RepairProject` (3 providers : repair, build, cleanup), `TunnelProject` (Dig + Stairs), `CleanupProject`, `HarvestProject` (4 providers). `Project.VerifyWorkFor` filtre en amont : stockage disponible pour `UniqueItemsGiven` (`MineProject.cs:86-99`, `HarvestProject.cs:113-120` : ignoré s'il y a des arbres) ; `LandscapeProject.HasWorkFor` (`:46-49`).

## 3. Points communs et méthodes-pivot

### 3.1 Y a-t-il un seul chemin ?
- **Prendre** : non strictement.
  - Chemin « Brictron prend dans un conteneur/au sol » **unique** [O] : `PickUpNode.GoPickUp` → `_GoPickUp` → `TakeSome` → `Transaction.StoreEverything(item, labor)`. Appelants de `GoPickUp` : `Order.Transfer.GoPickUp/GoPickUpOne/GoPickUpFrom`, `CleanUpGoal`, `SmartTransfer`, `FindAndPickUp` (3 surcharges : `BuildGoal`, `LandscapeGoal` Fill, `CraftingInstructions`, `CatapultOperations.cs:182`).
  - Choix de la **cible** : `Labor.Find/FindComplete` → `ClosestResource` → `resourceCache` → `FindOneResource` → `Knowledge.ClosestObjectToPickup` est aussi unique pour les chemins `FindAndPickUp` ; il est en outre appelé en lecture seule par `IsMatchFor` / `HasWorkFor` / `VerifyWorkFor` (BuildGoal, LandscapeGoal, `LandscapeProject`, `Project.VerifyResourceAccessFor`, `CraftingStation.CanFetch`). Les ordres joueur passent un objet déjà choisi.
  - **Autres entrées dans un sac** qui contournent `Transaction` [O] : `GarnotteEater.AbsorbGarnottes` (Dig, NatureHarvest, Litter, Landscape Dig, CleanUpAnythingAround), `ConflictResolver.OperationSwitchTask` → `Recepteur.SwitchContentWith` (échange de sacs entre deux Brictron qui se croisent, `ConflictResolver.cs:128-143`, aussi échange des Tasks via `InitialPackage.Send`, sans `Prepare`), `CatapultOperations.cs:175`, `ToolRack`, `Toolbag.AddObject` (outils : hors périmètre, chemin `Toolbag`).
  - Production : `Recipe.Finish` (artisanat), `Recepteur.WipeAndFill` via `ResourceConverter` (stock : `Stones` → `StoneBlock`, `ResourceConverter.cs:61-65,159-168`), `TransformGoal.SpawnEm`, chute d'arbre.
- **Déposer** : un seul chemin pour le Brictron [O] : `StoreNode.GoStoreIn` → `StoreIn` → `Transaction.StoreEverything(labor, contenant)`. Seul le **choix** de la destination varie : cible explicite (BuildGoal, station, ordre `GoStoreInto`, SmartTransfer) ou `BestRecepteurToStore(Inventory)` → `Knowledge.ClosestStorageWithRoomFor` (Palette/Toolrack, `Fonction == entrepot`, place pour l'item, `Knowledge.cs:224-234`) via `GoStoreInBestStorage`, seul appelé par `Order.Transfer.GoStore/GoStoreOrDrop/GoStoreOrDropAt`. Sorties hors stockage : `DropNode.Drop` → `Recepteur.DropAll`, `GoDropAt`.
- Effet final unique pour tout ajout/retrait dans un `Recepteur` : `Recepteur.OnObjectTracked(GameObject)` / `ObjectUntracked(GameObject)` (`Recepteur.cs:1135,1095`), appelés par `IObjectTrackerExtender.ObjectTracked` et `TrackingPair.Track/UnTrack`. Ils ne connaissent pas l'acteur.

### 3.2 Méthodes-pivot à hooker (signatures exactes)

| Rôle | Signature | Portée | Données disponibles |
|---|---|---|---|
| Embauche Project (toutes familles) | `Brix.Game.AI.Project.Hire(Labor labor) : bool` | public virtual, instance | projet, labor, `labor.Project` avant/après, résultat |
| Embauche goal (déjà patché) | `Project.GetPrepareFunction(Goal goal) : Func<BaseLabor,bool>` | public, instance | + goal, `HasOtherWorkerAssigned`, `AssignedWorkerCount` |
| Embauche station/catapulte/activateur | `Brix.Game.AI.IOperableExtender.Hire(IOperable<Labor> operable, Labor labor) : bool` | public static | station, ancien `Operator` |
| Décision station | `CraftingStation.GetPackage(CraftingStation.Actions action) : OrderPackage<Labor>` | public, instance | action (Pickup/GoFetch/Work), `StuffToFetch`, `CurrentRecipe` |
| Rejets de décision | `AIErrorsStore.AddDecisionError(AIError error, Labor labor) : void` | public, instance | cause du refus (par Goal appelant, nécessite la pile ou un hook sur chaque `IsMatchFor`) |
| Prepare universel | `OrderPackage<Labor>.Prepare(BaseLabor orderReceiver) : StaticNode` | public, instance (générique) | package (`Giver`, `DecisionSource`, `IOrderPackageValue.value`) |
| Création de Task | `Brix.Game.AI.Labor.SendOrder(Task task, Labor.DecisionSource source) : void` | public override, instance | `task.Description/Giver/Target/InitialPackage`, `labor.Operable`, `labor.Project`, sac |
| Fin de Task universelle | `Brix.Game.AI.Task.OnFinally() : void` | **private**, instance | `result`, `Exception`, `Worker`, `Giver`, `TaskStartedTimestamp`, `Logs` (champ privé) |
| Fin de Task goal (déjà patché) | `Goal.EndWork(Task work) : void` | public virtual, instance | idem, goal seulement |
| Intention de ramassage (déjà patché) | `PickUpNode._GoPickUp(this Labor labor, GameComponent itemToPickUp, int upTo) : Node` | **private** static | labor, item, conteneur parent, upTo |
| Ramassage | `PickUpNode.TakeSome(this Labor labor, GameComponent itemToTake, int upTo = -1) : Node` | public static | idem (déjà patché en entrée) |
| **Transfert effectif (pivot recommandé)** | `Brix.Game.AI.Transaction.StoreEverything(GameObject from, GameObject into, int upTo = -1) : void` | public, instance | ramassage si `into` = Brictron, dépôt si `from` = Brictron ; diff de contenu avant/après |
| Filtre de prise (déjà patché) | `Transaction.TakeFromEnumerable(Recepteur destination, Factory.AssetKey key, ICollection<GameObject> enumerable, int upTo = -1) : void` | **private**, instance | objets pris, ordre |
| Dépôt | `StoreNode.StoreIn(this Labor labor, GameComponent contenant) : StaticNode` | public static | destination |
| Choix de destination | `Labor.BestRecepteurToStoreInventory() : GameObject` (idle) et `Labor.BestRecepteurToStore(IGameComponent gc) : GameObject` (tests) | public, instance | destination retenue ; le cache `bestRecepteurCache` est par objet et par frame |
| Recherche de source | `Knowledge.ClosestObjectToPickup(Labor source, Ressource resource, Faction faction) : IGameComponent` (déjà patché, hook I) | public, instance | source choisie |
| Entrée garnottes | `GarnotteEater.AbsorbGarnottes(HashSet<GameObject> gos) : void` | **private**, instance | Brictron (`recepteur`), objets absorbés |
| Échange de sacs | `ConflictResolver.OperationSwitchTask(Labor a, Labor b) : void` | **private**, instance | contenus avant/après, packages échangés |
| Production | `Recipe.Finish(Recepteur recepteur, Action<GameObject> postProcess = null) : void` | public, instance | station (`recepteur.gameObject`), `Info.id/Creates/QtyCreated`, ingrédients |
| Conversion de stock | `Recepteur.WipeAndFill(Ressource r) : void` | public, instance | stock, ressource |
| Dépôt au sol | `Recepteur.DropAll() : void` | public, instance | porteur, contenu |
| Effet universel (acteur inconnu) | `Recepteur.OnObjectTracked(GameObject itemGO) : void`, `Recepteur.ObjectUntracked(GameObject itemGO) : void` | public, instance | recepteur, item (`AssetKey`) ; filtrer `NetworkServer.active` (non vérifié : appel côté client) |

Pour 100 % des ramassages/dépôts de Brictron : `Transaction.StoreEverything` + `GarnotteEater.AbsorbGarnottes` + `ConflictResolver.OperationSwitchTask` + `Recepteur.DropAll`. Pour 100 % des mouvements d'objets, tous acteurs confondus : `Recepteur.OnObjectTracked/ObjectUntracked`, corrélés avec le dernier hook d'intention du même Brictron.

### 3.3 Hooks existants (rappel) et effet hors construction
A `BuildGoal.IsMatchFor` et B `Project.GetPrepareFunction` : BuildGoal seulement. C `TakeSome` (borne) : BuildGoal seulement (`ClampPickup` sort si le `Giver` n'est pas un BuildGoal). D `Goal.EndWork` : journal BuildGoal seulement. **E** `_ClosestObjectToPickup_Test`, **F** `FindOneResource`, **G** `_GoPickUp`, **H** `TakeFromEnumerable`, **I** `ClosestObjectToPickup` : **globaux** (artisanat, Landscape, ordres joueur compris, flag `PickupFilter`).

## 4. Mêmes défauts qu'ailleurs ? Test utilisé par Goal

Contrôle des doublons : `Goal.workers` (liste), `HasOtherWorkerAssigned(labor)` = `workers.Count > (contient labor ? 1 : 0)` (`Goal.cs:144-147`). `workers.Add` à `WrapTask` (`Goal.cs:103-106`), `Remove` à `EndWork` (`:120-133`).

| Goal / station | Test d'unicité dans `IsMatchFor` | Autres tests | Priorité |
|---|---|---|---|
| `BuildGoal` | `HasOtherWorkerAssigned` **seulement si `IsAnti`** (`:244`) | état Blueprint, `CanReach`, `Labor.Find(recepteur)` (`:280`) | -10 si autre worker (`:207`) |
| `BuildStairsGoal` | `HasOtherWorkerAssigned` (`:103`) | `CanReach`, `CanBreakVoxels` | distance |
| `RepairGoal` | idem (`:55`) | sac vide, `CanReach` | distance |
| `TransformGoal` | idem (`:99`), jamais évalué (jetable) | `CanReach` | distance |
| `TreeGoal` | idem (`:68`) | `CanChopTrees`, sac non plein, `CanReach` | -100 si `IsReserved` ou worker (`:97`) |
| `NatureHarvestGoal` | idem (`:89`) | profession, `CanFitUpTo`, `CouldStore` | distance |
| `DigGoal` | idem (`:111`) | `CanBreakVoxels`, place dans le sac, `CouldStore` | -100 si worker (`:145`) |
| `LandscapeGoal` | idem (`:154`) | `CanReach`, place, `Labor.Find(stones)` (Fill) | poids inventaire/hauteur |
| `CleanUpGoal` | idem (`:22`) | autre sac, place, `BestRecepteurToStore` | -20 si `IsReserved` (`:57`) |
| `LitterGoal` | idem (`:122`) | sac plein, `CouldStore` | distance |
| `CraftingStation` | `Operator == null \|\| == labor` (`:376-379`), **non retesté** à `Prepare` | `CanReach`, `DecideWhatToDo` | `PriorityInput` sans distance |
| `IdleProject.GoStore` | aucun (`HasWorkFor` = true) | sac non vide et `KnowsAbout` | dernier de la liste |

Défauts transposables (état : [D] sauf mention) :
1. **Course IsMatchFor → Send** : tous les Goals sauf BuildGoal testent l'unicité à l'évaluation (`ProcessGoalAndReturnIsBest`), mais `workers.Add` n'a lieu qu'au `Send`. Entre les deux : durée de `SelectBestGoal` (peut s'étaler sur plusieurs ticks, budget `CheckEnoughTimeConsumed`, `GoalSelector.cs:186-194`). Deux Brictron peuvent donc s'inscrire sur le même `DigGoal`, `CleanUpGoal`, `TreeGoal`… Le hook B (`WrapPrepare`) refait le test juste avant `Hire` **mais seulement pour `BuildGoal`**. `Project.GetPrepareFunction` est commun à tous les Goals.
2. **Fuite de `workers`** : si une Task est envoyée puis écrasée avant de démarrer (`PendingTask` écrasé par un nouvel ordre ; `AbortWorkSignal` → `PendingTask = null`, `Labor.cs:1056-1059` ; `BaseLabor.cs:133-155`), son nœud ne tourne jamais, `Task.OnFinally` n'est pas appelé, donc `Goal.EndWork` non plus : le Brictron reste dans `workers` et le Goal paraît « pris » jusqu'à sa destruction. Plausible, non observé.
3. **Cache par `decisionFrame`** : `resourceCache` (clé `Ressource`) et `bestRecepteurCache` (clé objet à stocker) sont vidés seulement quand `decisionFrame` avance (`Labor.cs:553`, `UnbindedCache.cs:61-69`). La cible calculée à l'évaluation (`IsMatchFor`, `HasWorkFor`) est reprise des ticks plus tard dans `FindAndPickUp` : le hook G la corrige. Idem destination de stockage (`GoStoreInBestStorage` rappelle `BestRecepteurToStoreInventory`, mais `CleanUpGoal.IsMatchFor`/`StorableGoal.CouldStore` valident sur une destination qui peut se remplir avant l'arrivée).
4. **Réservations ignorées** [O] : `KnowledgeReserver` n'est lu que par `TreeGoal.PriorityFor`, `CleanUpGoal.PriorityFor` (pénalités) et par le correctif. `NatureHarvestGoal.HarvestAction` réserve le **Brictron** (`labor.gameObject`), pas la nature (`:138`). `ReserveUntil` ne réserve qu'un objet à la fois et ignore un second réservant (`KnowledgeReserver.cs:24-34`).
5. **Place du stockage non réservée** : `ClosestStorageWithRoomFor` teste `HasRoomFor(1 objet)` ; plusieurs Brictron peuvent viser la même palette ; `NavigateAndCheck` ne lève `StorageBecameFull` que si le stock est **plein**, sinon `TransferWhatever` dépose ce qui rentre et laisse le reste dans le sac (`Transaction.cs:110-127`).
6. **Échange de sacs** : `ConflictResolver` (modèles Construction/Mining/Operation) échange contenu et Tasks de deux Brictron sans `Prepare` ni `Hire` : `workers` du Goal reçoit le nouveau porteur par `WrapTask`, l'ancien sort par `EndWork` (`TaskResult.Switched`). Hook B contourné.
7. **Station** : course d'embauche (§1.2) et sur-ramassage (§1.4).

## 5. Journal unifié proposé

Fichier : `BuildGoalFix.log` (ou `Logistique.log`). Une ligne par événement, champs `clé=valeur` séparés par ` ; `, préfixe commun :
`<t:F2> f<frame> <EVT> [<ordre>] <brictron>` avec `<ordre>` = `P#<id projet>:<TypeProjet>` (goal : champ `Goal.project`, protégé, lu par réflexion ; sinon `labor.Project`), `S#<id>:<TypeStation>` (artisanat : `labor.Operable`), `U` (ordre joueur) ou `I` (idle).

| EVT | Hook | Champs |
|---|---|---|
| `EMBAUCHE` | `Project.Hire`, `GetPrepareFunction` (wrapper), `IOperableExtender.Hire` | `cible`=type+nom+id (goal ou station), `ok`, `projet_avant→après`, `autres_workers`=`AssignedWorkerCount`, `operateur_avant`, `sac`=`Describe(labor.recepteur.ContentDescription)`, `source`=`DecisionSource`, `action` (Pickup/GoFetch/Work si station) |
| `REFUS` | `AIErrorsStore.AddDecisionError` (ou après `IsMatchFor` faux) | `goal`, `erreur`=`AIError.thought`, `labor` ; échantillonné (volume élevé) |
| `TACHE` | `Labor.SendOrder` | `desc`=`task.Description`, `giver`=nom/id/type ou `-`, `cible`=`task.Target`, `source`, `instruction`=`InitialPackage.Instruction.Description`, `operable`, `sac` |
| `RAMASSE` | `Transaction.StoreEverything` (into = Brictron) | `item`=nom+`AssetKey`, `conteneur`=nom+type+`Fonction`, `demandé`=`upTo`, `pris`=diff de sac, `reserve`=`Knowledge.IsReserved(item)`, `sac_après`, `tache`, `giver` |
| `ABSORBE` | `GarnotteEater.AbsorbGarnottes` | `labor`, `n`, `types`, `tache` (travail en cours) |
| `DEPOSE` | `Transaction.StoreEverything` (from = Brictron) | `dest`=nom+type+`Fonction`, `déposé`=diff, `reste_sac`, `dest_contenu_après`, `tache` |
| `ECHANGE` | `ConflictResolver.OperationSwitchTask` | `a`, `b`, `sac_a`, `sac_b` (avant), packages échangés |
| `PRODUIT` | `Recipe.Finish` | `station`, `recette`=`Info.id`, `crée`=`Creates`×`QtyCreated`, `consommé`=ingrédients, `drop`=`DropOnCompletion` |
| `CONVERTI` | `Recepteur.WipeAndFill` | `stock`, `ressource`, `n` |
| `AU_SOL` | `Recepteur.DropAll` | `porteur`, `contenu` |
| `FIN` | `Task.OnFinally` (filtre `Worker is Labor`) | `desc`, `giver`, `résultat`=`TaskResult`, `exception`=type de `Task.Exception`/dernier `Logs`, `durée`=`Time.time - TaskStartedTimestamp`, `sac`, `operable`, état du goal (`reste` pour BuildGoal) |

Notes d'implémentation :
- Journal d'intention (`TACHE`, `EMBAUCHE`) sur `Labor.SendOrder` / `Prepare` ; journal d'effet (`RAMASSE`, `DEPOSE`) sur `StoreEverything` : le Brictron est `into` ou `from`, la tâche courante est `labor.CurrentTask` (ou `PendingTask` si `SendOrder` vient d'être appelé).
- `Recepteur.OnObjectTracked/ObjectUntracked` en mode « audit » seulement (bruyant) : détecte les mouvements hors chemins listés.
- `Task.OnFinally` étant privé, le hook IL cible l'entrée de la méthode ; `Task.Worker` y est encore posé (`Goal.UnassignWorker` le met à null après).
- Multijoueur : toute la logique d'ordre/artisanat est serveur (`NetworkServer.active` : `CraftingStation.Start`, `Recipe.Finish`). Journaliser côté hôte uniquement ; les hooks n'altèrent pas le réseau. Sauvegardes : lecture seule, aucun champ sérialisé ajouté (garder les états de journal en dictionnaires statiques, comme `Carriers`/`ReservedBy`).

## 6. Non trouvé / à vérifier
- Appelant de `Transaction.Take/Store` : aucun dans firstpass (peut-être `Assembly-CSharp`) [O].
- Appelants de `Recipe.Restart` : aucun dans firstpass.
- Comportement client de `Recepteur.OnObjectTracked` (réplication) : non vérifié.
- Tout ce qui est [D] est issu de la lecture ; aucune vérification dynamique (Frida/gdb non lancé).
- Chemin des outils (`Toolbag`, `ToolRack`, `ToolNode`) volontairement exclu.

## 7. Implémentation v1.4 (2026-10-18)

Classe `Journal` (`patch/BuildGoalFix/Journal.cs`), fichier `Managed/Logistique.log`, clé `LogAll` (défaut true). Observation pure. Ligne : `[HH:mm:ss] t=<Time.time> EVT [ordre] Brictron ; clé=valeur …`, ordre = `P#id:TypeProjet` (Goal → champ `Goal.project` par réflexion, sinon `BuildGoal.Provider`, sinon `labor.Project`, préfixe `I` si IdleProject), `S#id:TypeStation` (`Giver` opérable ou `labor.Operable`), `U` (dernier `SendOrder` de source `User`), `?`.

| Hook IL | Point | EVT | Champs distance |
|---|---|---|---|
| J1 `Project.Hire` (3 ret) | toutes familles ; `Journal.CurrentGoal` posé par `WrapPrepare` (désormais appliqué à tout Goal, garde conservée pour BuildGoal seul) | `EMBAUCHE [P…]` cible=Goal ou projet, ok, autres=`AssignedWorkerCount`, sac | `d` Brictron→cible, `pos`, `cible_pos` |
| J2 `IOperableExtender.Hire` (ret) | stations, catapultes | `EMBAUCHE [S…]` recette, manque=`StuffToFetch` | `d`, `pos`, `cible_pos` |
| J3 `Labor.SendOrder` (entrée) | toute Task | `TACHE` desc, giver, cible, src | `d_giver`, `d_cible`, `pos` ; remise à zéro de l'odomètre de tâche |
| J4 `Transaction.StoreEverything` (entrée + ret) | tous ramassages/dépôts de Brictron | `RAMASSE` item, source (nom:type/fonction), demandé, pris (diff de sac) / `DEPOSE` dest, déposé, reste, dest_contenu | `d` Brictron→source/dest, `pos` |
| J5 `GarnotteEater.AbsorbGarnottes(HashSet)` (entrée) | Dig, récolte, Litter, Landscape | `ABSORBE` candidats par ressource, sac_avant | `pos` |
| J6 `ConflictResolver.OperationSwitchTask` (entrée) | échange de sacs/tâches | `ECHANGE` sac_a, sac_b, tache_a, tache_b | `pos` |
| J7 `Recipe.Finish` (entrée) | production | `PRODUIT [S…]` recette, crée, drop, opérateur | `pos` |
| J8 `Recepteur.DropAll` (entrée, Brictron seulement) | dépôt au sol | `AU_SOL` contenu | `pos` |
| J9 `Task.OnFinally` (entrée) | fin de toute Task (Worker encore posé) | `FIN` desc, giver, résultat (+ exception), durée | `trajet` = distance réellement parcourue depuis `TACHE` (odomètre), `pos` |
| — composition BuildGoal (`LogComposition`, BuildGoalFix.log) | ordres multiples | `étendue` = distance max entre deux Blueprints, `centre` | |

Odomètre : `JournalTracker` (MonoBehaviour sur un GameObject `HideAndDontSave`, `DontDestroyOnLoad`) somme à chaque frame les déplacements des Brictron déjà vus ; entrées purgées quand le Brictron est libéré. Aucune donnée sérialisée, aucune écriture réseau.

Analyse : `python3 -I tools/logistique_report.py [log] [--since HH:MM:SS] [--ordre P#id]` → par ordre : embauches (dont refusées), tâches, ramassages, dépôts, fins, échecs, `d` moyen/max à l'embauche, trajet moyen/max par tâche ; trajets les plus longs ; tâches non réussies.

Objectif utilisateur (2026-10-18) : repérer les optimisations de trajet possibles (ex. deux chantiers éloignés : répartir les Brictron libres entre eux au lieu de tous les envoyer sur le même) ; décision après lecture des chiffres.

## 8. Première session journalisée (2026-10-18, 12:38 → 14:29, 10 564 lignes) et v1.4.1

Copie du journal : `patch/verify/logistique-2026-10-18.log`. Ordres : 6 BuildProject, 1 MineProject (149 embauches), 2 HarvestProject, 5 ChoppingBlock, Loom, Workbench, Furnace, ordres directs.

| Constat | Chiffres | Lecture |
|---|---|---|
| Stockage plein à l'arrivée | 21 `GoStoreOrDropAt` → `StorageBecameFull`, **423 m** perdus (40 % des 1 055 m de trajets non aboutis) ; 128 succès | `GoStoreInBestStorage` (StoreNode.cs:37) choisit via `ClosestStorageWithRoomFor` = `HasRoomFor(1 objet)` (Knowledge.cs:231-234), sans compter les autres Brictron en route ni la charge réelle (20 pierres) ; `NavigateAndCheck` échoue si `IsFull()` à l'arrivée (StoreNode.cs:25). Cache `bestRecepteurCache` par frame. **Cible d'optimisation n° 1** |
| Récolte sans stockage | 14 `NoCorrespondingStorage` (Large/Medium/SmallHarvestGoal), 234 m | pas de palette acceptant la ressource ; le test se fait après le trajet |
| Blocs en bûches | 23 `CannotFindWhatINeedToPickUp` sur `Wood_DoubleLog_AA`, 0 m | échec immédiat (F/G), pas de bûche libre ; churn d'embauches sans trajet |
| Trajets longs | BuildProject #-1129882 : 57 m/tâche en moyenne (max 91) ; Loom `FindIngredients` 80 m ; Workbench 70 m | stock loin du chantier/station : choix du joueur, à confronter aux sources réelles (journal v1.4.1) |
| Équité d'embauche | 301 embauches Build/Harvest/Mine : 1 seule avec un Brictron inactif nettement plus proche | pas de problème de répartition observé, mais aucun test avec deux chantiers éloignés simultanés (ordres séquentiels) |
| Stations | 42 PRODUIT (IronIngot, Poleaxe, Rope, Fabric, Firefly) ; 0 ECHANGE, 0 AU_SOL | artisanat couvert |

Défauts du journal corrigés en v1.4.1 : `RAMASSE` lisait la source après le transfert (toujours le sac du Brictron, d=0) → capturée à l'entrée de `StoreEverything` ; `ABSORBE` 6 661 lignes à vide sur 7 253 → ligne seulement si le sac a changé (entrée + sortie) ; `EMBAUCHE` IdleProject (419 lignes, distance sans sens) supprimée ; `DEPOSE` : `dest_plein` ajouté ; rapport : noms avec espaces (`Loom (3)`) désormais parsés (PRODUIT était ignoré).

### Proposition v1.5 (cible n° 1 : stockage)

Réservation de capacité à la décision de stockage : hook en entrée de `StoreNode.GoStoreIn(labor, contenant)` → `Pending[contenant] += charge du Brictron` jusqu'à son `WorkEnded` (même mécanisme que `KnowledgeReserver`), et hook sur le `ret` de `Knowledge._ClosestStorageWithRoomFor_Test(gc, item)` → refus si `CanFitUpTo(clé) − Pending ≤ 0`. Option : exiger de la place pour toute la charge quand un autre stockage en a. Attendu : `StorageBecameFull` ≈ 0, −400 m par session équivalente.

## 9. v1.5 volet A : réservation de capacité des stockages (2026-10-18)

Classe `Storage` (`patch/BuildGoalFix/Storage.cs`), clé `StorageReserve` (défaut true). Stockage = `Recepteur.Fonction == entrepot` (Palette, Toolrack). État serveur, non sérialisé.

| Hook IL | Point | Effet |
|---|---|---|
| K | entrée `Knowledge.ClosestStorageWithRoomFor(source, item, faction)` | mémorise le chercheur |
| L | ret de `Knowledge._ClosestStorageWithRoomFor_Test(gc, item)` | refus si `place libre − réservations des autres ≤ 0` ; place = `CurrentCapacity.Value(ressource)/valeur de l'objet`, bornée par l'`Encombrement` si le stockage en a ; non calculable (type absent de la capacité, ex. outils) → vanilla |
| M | entrée `StoreNode.GoStoreIn(labor, contenant)` (remplaçant) | stockage déjà couvert par les réservations (cible issue du cache `bestRecepteurCache`) → recherche fraîche (K+L), sinon cible d'origine ; puis réserve la charge du Brictron (ressource principale du 1er objet du sac). Journal : `STOCK_REDIRIGE` |
| N | ret de `Transaction.StoreEverything(from, into, upTo)` | dépôt d'un Brictron → libère sa réservation sur `into` |
| O | entrée `Task.OnFinally` | libère la réservation posée pendant cette Task |

Expiration : 90 s, Brictron détruit, ou sac sans la ressource réservée. Une réservation par Brictron (la nouvelle remplace l'ancienne).
Effet de bord voulu : `StorableGoal.CouldStore`, `CleanUpGoal.IsMatchFor`, `Project.VerifyWorkFor`, `CraftingStation.CanStore` passent par L : un Brictron n'est plus embauché pour creuser/récolter/nettoyer si tous les stockages sont déjà couverts par des Brictron en route.
Non fait (option du §8) : exiger la place pour toute la charge quand un autre stockage l'a ; un dépôt partiel reste possible (vanilla : `GoStoreInBestStorage` boucle jusqu'au sac vide).
Vérification hors jeu : métadonnées identiques à l'original ; les 25 appels injectés vers `CastleStory.BuildGoalFix` résolus (Cecil `Resolve`).

## 10. v1.6 : ordre de construction (2026-10-19)

Constat utilisateur : un Brictron pose un bloc sur le trajet et rend inaccessible le Blueprint d'un autre (`PathExecutionFailed` après 100 m et plus, test v1.5). Pas d'échafaudage automatique (refusé).

### 10.1 Étude : modèle de déplacement [O]

| Point | Code |
|---|---|
| Case « debout » = `Voxel.IsOpenAndWalkable(p)` : `p+1` vide, et `p` en pente (non plein) ou `p` vide sur `p-1` plein | `FP/Brix.Engine/Voxel.cs:229-240` |
| Nœuds du graphe : cases debout hors arbres / blocage nature (`IsNatureBlocker`), plus les volumes `walkable` (escaliers, rampes posés) | `FP/Brix.Pathfinding.Test/TestPathfinding.cs:640-690, 778-800` |
| Voisins : 4 directions ; même hauteur, sinon ±1 voxel ; dénivelé < 10 tiers de niveau (1 voxel = 6) : **un Brictron monte ou descend 1 voxel au plus** | `TerrainPathNode.cs:172-239`, constantes `TestPathfinding.cs:95-111` |
| Branches `Breaking` (casser un bloc) et `Ladder` existent ; un Brictron constructeur n'accepte que jusqu'à `Climbing` | `SearchPathRequest.cs:45-51`, `PathBranch.cs` |
| Cases d'accès d'un Blueprint = positions de nœuds debout (`AccessPositions` indexé par `RepresentativePosition`) | `BuildGoalFinder.cs:291-300` |
| Cases qui deviennent pleines : `volume.blueprint` = `GetCases(fantome)` | `FP/Brix.Engine/MultiBlockVolume.cs:41` |
| Vanilla : priorité `(128−y)×10 − distance − 10 (autre worker) − 100 (Blueprint plus bas voisin)` ; `buildable` = support seulement | `BuildGoal.cs:205-210, 299-312` ; `ValidatorComponent.cs:105-156` |

### 10.2 Implémentation (classe `BuildOrder`, `patch/BuildGoalFix/BuildOrder.cs`)

Par projet (`BuildGoalProvider`), mis en cache, recalculé quand la liste des Blueprints change ou au bout de 3 s : grille locale (emprise des Blueprints + 6 voxels autour, 3 dessous, 4 dessus ; abandon au-delà de 600 000 cases), parcours en largeur des cases debout depuis le bord de la grille (= reste du monde).

| Étape | Clé | Règle |
|---|---|---|
| 1a accès | `BuildReach` | Blueprint accessible si une case d'accès est reliée au bord. **Informatif** (journal) : le modèle ignore les échelles et vanilla teste déjà l'accès réel au choix |
| 1b non-coupure | `BuildReach` | Blueprint X refusé (`CantReach`, dans `IsMatchFor` et avant `Hire` dans `WrapPrepare`) si sa pose retire du parcours toutes les cases d'accès d'un autre Blueprint accessible. Calculé seulement si X occupe une case debout du parcours ou la tête d'une case. Sécurités : aucun Blueprint accessible, ou tous coupent quelqu'un → contrôle levé |
| 2 plan | `BuildPlan` | Bonus ajouté à `BuildGoal.PriorityFor` (hook P) : `3 × min(profondeur, 20)`, profondeur = distance au bord des cases d'accès quand tout le projet est construit ; Blueprint enfermé dans l'état final → profondeur max + 5. Effet : de l'intérieur vers l'extérieur, en plus du « bas d'abord » vanilla |

Portes (`Door`, `HeavyDoor`) : comptées passables. Les autres objets (palettes, ateliers) sont comptés pleins.
Journal (BuildGoalFix.log) : `[Ordre #id] accès : N Blueprint(s), x inaccessible(s), y reporté(s) [X coupe Y…] ; cases, ms` à chaque changement ; sur échec d'une Task de construction : `accès oui/NON (reporté : coupe …)`.
Vérification hors jeu : `patch/verify/grid_sim.py` (portage Python de la grille) : couloir coupé par un bloc de 2 de haut → reporté ; un bloc seul ne coupe pas (on grimpe 1 voxel) ; pièce : la porte qui ferme est reportée, l'intérieur passe en tête ; mur ouvert : aucune coupure. DLL : métadonnées identiques, 26/26 appels de hooks résolus.
Limites [D] : obstacles mobiles et Brictron ignorés ; pentes simplifiées ; deux poses simultanées qui ne coupent qu'ensemble passent (rattrapé au calcul suivant).

### 10.3 Test v1.6 (2026-10-19, 13:20 → 13:41) et v1.6.1

Copies : `patch/verify/*-2026-10-19-v1.6.log`. Constat utilisateur : construction dans l'ordre et terminée ; deux Brictron restés bloqués à l'étage (pas d'escalier) ; gel d'environ 0,5 s régulier.

| Point | Mesure | Cause / correctif v1.6.1 |
|---|---|---|
| `PathExecutionFailed` BuildProject | 9 échecs / 369 m (v1.5 : 11 / 745 m) ; 6 sur des créneaux et poutres en haut de mur, modèle « accès oui » | Modèle trop permissif en hauteur [D] : les cases posées comptent comme blocs pleins sur lesquels on marche ; les créneaux (`Stone_Merlon*`) et poutres ne le sont probablement pas. Non corrigé (données de forme à lire) |
| Brictron enfermés | Kadir : 3 `WalkToward` en échec à 494,109,706 | La non-coupure protégeait les Blueprints, pas les Brictron. v1.6.1 : X refusé s'il enfermerait un Brictron aujourd'hui relié au bord, ou si aucune de ses propres cases d'accès ne reste reliée (le poseur) ; un Brictron inactif menacé reçoit `Order.Move.WalkToward` vers la case de bord la plus proche (au plus toutes les 8 s ; journal `Évacuation :`) |
| Gel | Calcul de 647 ms sur un ordre de 23 Blueprints (6 552 cases, 3 reportés), refait à chaque pose et toutes les 3 s | Un parcours par Blueprint testé, chaque case relisant le voxel. v1.6.1 : cases debout précalculées une fois ; un test ne réévalue que les cases touchées (case posée, dessous, dessus) ; parcours sur index ; liste des Blueprints relue au plus une fois par frame ; calcul ≥ 30 ms toujours journalisé |

### 10.4 Test v1.6.1 (2026-10-19, 14:27 → 14:35) et v1.6.2

Copie : `patch/verify/buildgoalfix-2026-10-19-v1.6.1.log`. Constat utilisateur : accès à la 4e rangée de mur bloqué en fin de construction.
Journal : Jaws attribué à `Stone_DoubleBrick (82)` à 14:34:34 (légitime) ; 87 et 88 n'apparaissent qu'à 14:34:40 (constructibles une fois la rangée du dessous posée) ; 82 posé → 87/88 `PathExecutionFailed` (14:35:17). Le calcul ne voyait que `Provider.All()` (Blueprints constructibles). Calcul ≈ 40 ms à chaque fois, 1-2 ms quand `FindObjectsOfType<Labor>` (cache 2 s) n'était pas relancé. 0 `Évacuation`.
v1.6.2 : victimes étendues aux Blueprints commités non constructibles (`BuildGoalProvider.TrackedBlueprints`), évalués dans l'état « Blueprints plus bas posés » (un état par hauteur) ; Blueprints attribués (Task vivante) comptés comme posés dans l'état de base, et pris dans la signature du cache ; registre des Brictron : `FindObjectsOfType` toutes les 30 s + signal `Labor.LaborStarted`.

## 11. v1.7 : délais de réflexion (2026-10-19)

Constat utilisateur : ~1 s d'attente entre « ne peut pas poser / plus de place » et le demi-tour. Constantes vanilla, en ticks de 20 ms :

| Délai | Vanilla | v1.7 | Code | Patch |
|---|---|---|---|---|
| Pause après Task en échec (`DelayOnFail`) | 30 (0,6 s) | 8 | `Task.cs:34`, appliqué à la création (`Catches`, `WaitAfterFailure`) | hook Q : `ldc 30` avant `stfld DelayOnFail` du constructeur → `call Hooks.DelayOnFail()` |
| Pause après Task réussie (`DelayOnFinished`) | 10 | 4 | `Task.cs:32`, `CoreNode` | hook Q, idem |
| Attentes de `LookForWorkSequence` (après instinct, après `CheckProjectForWork`) | 2 × 10 | 2 × 4 | `Labor.cs:543-552` | hook R : `ldc 10` avant `WaitFor.Ticks` → `call Hooks.LookForWorkTicks()` (le `Ticks(1)` « rien à faire » est conservé) |
| Période de la séquence LookForWork | 10 | 4 | `Labor.cs:385` (`AddOrSetSequence(..., 10, ...)`, `MultiBehavior.cs:27-36`) | hook S : `ldc 10` avant `ldstr "LookForWork Sequence"` |
| Décalage anti-rafale (`OffsetWaitTime`) | 1 à 15 | 1 à 8 | `Labor._tickOffset` 2 (`Labor.cs:146, 288-302`) | réflexion dans le constructeur statique de Hooks |

Les valeurs sont lues à la création de chaque Task : la modification d'un champ après coup serait sans effet (le nœud est construit dans le setter `WorkNode`, `Task.cs:80`).
Clé `FastThinking` (défaut true ; false = valeurs vanilla, sauf `_tickOffset` déjà modifié si la clé était vraie au chargement).
Coût : le cycle de recherche d'un Brictron inactif passe de ~20 à ~8 ticks (≈ 2,5 fois plus de `CheckProjectForWork`).
Vérification : 2 + 2 + 1 constantes remplacées (le Loader échoue sinon), métadonnées identiques, 29/29 appels résolus.

## 12. Palettes entamées (2026-10-19) et v1.8

Constat utilisateur : des palettes de pierre restent sous 80 (pas de conversion en 4 blocs).
Analyse des `DEPOSE` (pierre, 35 palettes) : pendant le creusement elles atteignent 80 ; il reste une palette entamée par fin de session (16, 6, 39). Causes : (1) le dernier porteur complète une palette puis dépose son reliquat dans la palette libre la plus proche (14:32:25 → 14:32:26) ; (2) quand la place d'une palette est déjà réservée (v1.5), le porteur suivant en entame une autre : remplissage en parallèle (`Palette (11)` et `(43)` à partir de 14:32:26) ; (3) le jeu choisit le stockage le plus proche ayant de la place, sans préférence pour un stockage entamé, et ne regroupe jamais.

Plan validé par l'utilisateur : A, puis B + C s'il reste des palettes entamées.

| # | Proposition | État |
|---|---|---|
| A | Compléter avant d'entamer | v1.8, installée |
| B | Regroupement par un Brictron inactif (palette entamée la moins remplie → autre palette entamée de même ressource) | si A ne suffit pas |
| C | Seuil : regrouper seulement si le total des palettes entamées complète au moins une palette | avec B |

v1.8 (A, `Storage.StartedElsewhere`, clé `CompleteFirst`) : dans le test L, un stockage vide de la ressource est refusé s'il existe un stockage connu (vu par L) déjà entamé de cette ressource, non plein, de la même faction, qui accepte l'objet (`HasRoomFor`), avec de la place libre après réservations, accessible (`Knowledge.CanReach`), à au plus 20 m de plus que le stockage vide. Le reliquat d'un porteur va donc à la palette entamée suivante ; une nouvelle palette n'est entamée que si les entamées sont pleines ou réservées. Effet aussi sur les planches, bûches, granulaires (toute ressource).
Mesure de contrôle : contenu final des palettes dans les `DEPOSE` (`dest_contenu`) ; attendu : au plus une palette entamée par ressource.

## 13. v1.9 : ordre de construction global (2026-10-19)

Point de départ : test v1.8 (`patch/verify/buildgoalfix-2026-10-19-v1.8.log`, `logistique-2026-10-19-v1.8.log`). Tour 6×6 (x 543..548, z 715..720, y 101..104) : la 4e rangée devient inaccessible, (71) et (72) partent quand même → `PathExecutionFailed` (26 m chacune) ; fin : 11 Blueprints restants dont 8 inaccessibles, 0 reporté. v1.6.2 ne pouvait pas l'éviter (futurs évalués dans l'état « dessous posé », déjà inaccessibles). Calcul : 1-3 ms, ~40 ms toutes les 30 s (`FindObjectsOfType`), deux pics de 607/613 ms sur des calculs ordinaires (5 200-5 760 cases) → hypothèse GC [D, non prouvée].

### 13.1 Règles lues dans les données [O]

- Formes des Blueprints extraites (level15/level17) dans `patch/verify/buildsim/shapes.json`. `Stone_DoubleBrick` : poseur sur une case voisine horizontale, de 2 voxels plus bas à 1 voxel plus haut que la brique → brique en y 104 posée depuis y ≥ 102, donc depuis le haut du mur. **Une tour fermée de 4 rangées sans escalier n'est pas entièrement constructible.**
- `ValidatorComponent.MaxMissingSupport` : 1 pour double brique, double créneau, planches, poutres ; 0 pour brique simple, créneau simple, porte. Un Blueprint posé sur un Blueprint non construit n'est pas constructible. Planches et poutres se soutiennent mutuellement (cycles à traiter).
- Créneaux, poutres, entretoises, colonnes, parapets, arcs : pleins, mais leur dessus ne compte pas comme sol (cause probable des `PathExecutionFailed` v1.6 [D]).

### 13.2 Algorithme (`BuildPlanCore.cs` sans Unity, `BuildOrder.cs` côté jeu)

1. Modèle : case debout, 4 voisins, ±1 voxel, coûts du pathfinder (10, +240 montée, +160 descente) ; porte passable, escalier = pente.
2. Groupes de Blueprints dont les emprises (avec marges) se touchent ; une grille et un plan par groupe.
3. Supports : Blueprint ordinaire après tous ses supports Blueprint ; planche/poutre : au moins un appui ancré ; sans support possible → signalé.
4. Pelage à rebours depuis l'état final : retrait d'un Blueprint si rien ne repose dessus et si toutes ses cases d'accès « probables » (coût ≤ min + 200) restent reliées au bord ; ordre = inverse du retrait.
5. Blocage : le plus haut retirable est déclaré non constructible, on recommence ; après 4 reprises, toute la rangée est exclue. Exclusions conservées jusqu'à l'ajout d'un Blueprint.
6. Garde à l'attribution de X : poseur et poseurs déjà attribués ni coupés ni enfermés, maintenant et après les poses en cours ; les Blueprints prévus avant X restent constructibles. Cache par état ; test « Brictron enfermés » refait à chaque appel ; évacuation des Brictron dans une poche qui va se fermer.
7. Blueprint inaccessible selon le modèle : refusé (`CantReach`), plus seulement journalisé.
8. Voies rapides : attribution d'un X déjà testé, pose d'un Blueprint attribué (un seul parcours). Recalcul complet sur échec, annulation, nouveau Blueprint ou délai de 3 s.
9. Budgets : tests d'attribution ≤ 2 ms/frame ; lecture des voxels étalée au-delà de 12 000 cases (2 ms/frame) ; relecture continue 0,1 ms/frame/groupe ; > 600 000 cases → contrôle levé pour le groupe. Plus de `FindObjectsOfType`, pas d'allocation dans les boucles.
10. Sécurité : 10 s sans attribution autorisée ni pose en cours alors qu'il reste un Blueprint non signalé → contrôle levé pour le groupe (journalisé).

Journal `BuildGoalFix.log` : `[Ordre #id] NON CONSTRUCTIBLE sans escalier ou appui : N Blueprint(s) : nom (raison)` (raisons : « inaccessible une fois le reste construit (escalier manquant ?) », « sans support ») ; ligne `plan :` (Blueprints, groupes, attribués, sans accès, non constructibles, cases, ms, voies rapides).

### 13.3 Validation hors jeu

Simulateur `patch/verify/buildsim/` (`run.py`, `scenarios.py`, `sim.py`, `world.py`, `buildorder.py`, `export.py`, harnais C# `cs/`). 15 scénarios × 6 graines : 0 Blueprint bloqué, 0 Brictron enfermé, aucune sécurité déclenchée. v1.8 et vanilla bloquent ou enferment sur 6 scénarios (dont `reel_v18`, reproduction du journal : 10 bloqués). Tour sans escalier : 37/40 posées (mur utilisé comme marche), 3 signalées. Durées égales à vanilla sur les cas simples. C# = Python sur 42/42 instantanés (`PlanHarness`, .NET 8). Temps .NET 8 : calcul complet 0,1-1,7 ms (pire : maison 85 Blueprints, 6 615 cases), test d'attribution 0,02-0,6 ms ; 1 à 5 calculs complets par chantier. Mono non mesuré (estimation ×2-4 → 3-7 ms au pire [D]).

Rejouer : `python -I run.py --seeds 6 --variants v1.9,v1.8,vanilla -v` ; `python -I export.py cases 15 ; python -I export.py --cost cases ; dotnet run -c Release --project cs -- cases 20` (attendu : 0 différent).

### 13.4 Risques et non couvert

Bloc posé sur un Brictron (comportement inconnu, simulateur pessimiste) ; choix de la case d'accès modélisé depuis le bord (palettes à moins de 6 cases → poche résiduelle possible) ; appui des planches simplifié (le jeu exige poutre/rondin à ≤ 4 cases ; filet : sécurité de 10 s) ; dessus des créneaux non praticable (prudent, peut sur-signaler) ; partie Unity (regroupement, voies rapides, lecture des voxels) non exécutée hors jeu ; hypothèse GC non prouvée, `ComputeStats` et `IsProjectCovered` allouent encore. Non couvert : rampes, échelles, pente, arbres, démolitions (`IsAnti`).

Installée le 2026-10-19 à 17:44 (`patch/build.sh`, 29/29 appels résolus, firstpass patché inchangé `e0372b7d…`). Installeur `CastleStoryPatch` non reconstruit (`package.sh`).

À mesurer au test : `Hooks v1.9 chargés` ; `plan : … Y ms` ≤ 5 ms ; tour sans escalier → `NON CONSTRUCTIBLE` (1 à 10 briques de la 4e rangée), reste posé, 0 `PathExecutionFailed` sur la tour, aucun Brictron bloqué sur le mur ; `Évacuation :` suivies d'une vraie descente ; aucune ligne « contrôle levé » ni « zone trop grande » ; durée murs/maison comparée à v1.8 ; escalier ajouté après coup → briques signalées réévaluées puis posées.

### 13.5 Test v1.9 (2026-10-19, 17:45 → 17:51) et v1.9.1

Copies : `patch/verify/*-2026-10-19-v1.9.log`. Constat utilisateur : tour non construite (un 5e niveau ajouté pendant la construction). Journal : 0 `PathExecutionFailed` sur le chantier, calculs 0,2-5,7 ms (plus de gel). Mais des briques des rangées intermédiaires passent « sans support » à tort et y restent : (27) en 554,102,719 repose sur (4) et (5), posées à 17:48:54 et 17:48:56, exclue au calcul de 17:48:57 ; (28) sur (8) posée à 17:48:57, exclue à 17:49:04 ; puis (38), (52), (42), (51) (cascade déduite). L'ajout de la 5e rangée (17:49:51) vide les exclusions retenues : (28) et (38) sont réattribuées, mais la 5e rangée est entièrement « inaccessible » (conforme, sans escalier). Fin : 20 restants, 19 non constructibles.

Cause [O journal + simulateur, mécanisme D] : le Blueprint posé quitte la liste avant que son bloc soit lisible dans les voxels ; le recalcul complet voit un support vide → « sans support », rendu permanent par `Sticky` ; un exclu ne compte plus comme support → cascade.

v1.9.1 : 1) « sans support » jamais mémorisé (`Sticky` ne retient que « inaccessible », vidé à l'ajout d'un Blueprint) ; 2) un Blueprint attribué qui disparaît de la liste entre dans `plan.Pending` : ses cases comptent comme posées (`PlanGrid.AssumePlaced`, `ApplyPending` au début de chaque calcul) jusqu'à ce que `BuiltInWorld` voie le bloc (5 s au plus, 64 entrées).
Simulateur : retard de lecture (`read_lag`) et événements de projet ; scénarios `reel_v19_tour5_relecture` (v1.9 : 10 posées / 30 signalées ; v1.9.1 : rangées 1-3 complètes + 2,8 de la 4e, 17,2 « inaccessible », 0 enfermé) et `reel_v19_tour5_escalier` (v1.9.1 : 65/65). Batterie des 15 scénarios identique à v1.9 ; sans retard v1.9.1 = v1.9. C# = Python 50/50. Installée le 2026-10-19 à 18:35.
Risques : recalcul complet à chaque pose lue en retard (31-61 par chantier au simulateur, 0,3-1,9 ms chacun) ; Blueprint attribué annulé : cases pleines jusqu'à 5 s (fausse exclusion « inaccessible » possible, non testé). Hors périmètre, déjà en v1.9 : avec escalier, sur 12 graines, échecs de trajet groupés (brique de 5e rangée posée pendant que des briques de 4e sont en route) et un enfermement (graine 11).

À mesurer : `Hooks v1.9.1 chargés` ; « sans support » seulement au-dessus d'une brique « inaccessible » ; tour sans escalier : rangées 1-3 complètes, une partie de la 4e, reste signalé, 0 `PathExecutionFailed` ; `plan :` ≤ 5 ms ; escalier ajouté ensuite → tout posé.

## 14. v1.9.2 : Brictron inactifs traversent les alliés (2026-10-19)

Constat utilisateur : des Brictron se bloquent entre eux dans certaines situations. Test v1.9.1 (`patch/verify/*-2026-10-19-v1.9.1.log`) : « sans support » transitoire corrigé ((27) rétablie en 5 s), rangées 1-3 montées sans échec ; mais 5 Brictron restés sur le haut du mur (y 103-105) après usage du mur comme marche, évacuations en `PathExecutionFailed` (problème non traité ici ; piste : saut d'évacuation `Order.Move.TurnAndJumpToward`, `Builder.CanJumpOff` faux, pas de dégâts de chute trouvés).

Mécanisme [O, décompilation `decomp/firstpass`] : le mode de collision est choisi par le marcheur, via `Profession.CanPassThrough()` (`Navigation.NavigateOrderData.Init`, `Locomotion4.RepathMoveNext`, `LocomotionMovementState.CanPassThroughBlocker`, `Labor.Unstack`). `VoxelPresence.OthersCanPassThroughMe` est toujours vrai ; `PreventsMovement` laisse passer un allié si le marcheur est passeur. `Builder.CanPassThrough` = `Activity != Fighting && Activity != Chilling && (IsWorker() || hackedDigGoal libéré)`. Un Brictron qui flâne (`Chilling` : errance, assis au ralliement) n'est donc pas passeur : face à un Brictron immobile, `TryPushingForward` échoue aussitôt (`IsIdleBlocker`), sinon après ~2 s d'attente (`WaitActively`, 20 × 0,1 s) ; halte, nouvelle recherche, éventuellement `PathUnfindable` → `SignalOutOfReach` sur la cible [D]. Les Brictron au travail traversent déjà les alliés. `Profession.CanMakeWay` = forcer le passage (casser voxels, portes), faux pour Builder, sans rapport.

v1.9.2, hook T : chaque `ret` de `Builder.CanPassThrough` → `Hooks.AfterCanPassThrough(result, this.labor)` = `result || Activity == Chilling` (clé `PassThroughIdle`, défaut true). Combat (`Fighting`), ennemis et neutres (test d'alliance) inchangés. `Labor.Unstack` teste sa propre case avec `CollideWithEveryone` (séparation des Brictron empilés inchangée) ; seule différence : un Brictron empilé qui flâne et marche n'est plus séparé (il s'en va déjà). Vérif : 30/30 appels de hooks résolus, installée le 2026-10-19.
À observer : moins de Brictron oisifs immobilisés face à face ; plus d'empilement possible (cosmétique) ; `PathExecutionFailed` des tâches `IdleProject`.
