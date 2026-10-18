using Brix.External.Factories.Templates;
using Brix.Game.AI.InstructionType;
using Brix.Game.AI.Nodes;
using Brix.Game.Components;
using Brix.Pathfinding;
using Brix.UI.Icons;
using Motus.Behavior;
using Smooth.Algebraics;
using UnityEngine;

namespace Brix.Game.AI;

public class Order
{
	public class AboutTools
	{
		public static LaborInstruction<Accessoire> GoEquipBestToolOfType = new LaborInstruction<Accessoire>("GoEquipBestToolOfType", IconKeys.DropThere, ToolNode.GoEquipBestToolOfType);

		public static LaborInstruction<Accessoire> FindRackAndEquip = new LaborInstruction<Accessoire>("GoEquipBestToolFromRack", IconKeys.DropThere, ToolNode.GoEquipBestToolFromRack);

		public static LaborInstruction<GameObject> FindWeponAndRegroup = new LaborInstruction<GameObject>("GoFindWeponAndRegroup", IconKeys.DropThere, (Labor labor, GameObject value) => (StaticNode)Node.Sequence.Do(labor.GoEquipBestToolFromRack(Adjectif.accessoire)).Do(labor.RunToExactly(Location.Of(value))));

		public static LaborInstruction StoreWeapons = new LaborInstruction("GoStoreWeapons", IconKeys.DropThere, (Labor labor) => (StaticNode)Node.Sequence.Do(labor.GoStoreTools(Adjectif.accessoire)));

		public static LaborInstruction<GameObject> GoEquipRackContent = new LaborInstruction<GameObject>("GoEquipRackContent", IconKeys.DropThere, ToolNode.GoEquipRackContent);

		public static LaborInstruction<GameObject> GoStoreInRack = new LaborInstruction<GameObject>("GoStoreInRack", IconKeys.DropThere, ToolNode.GoStoreToolsInRack);

		public static LaborInstruction<FactoryTemplate> SpawnEquipTool = new LaborInstruction<FactoryTemplate>("SpawnEquip", IconKeys.DropThere, (Labor labor, FactoryTemplate value) => labor.SpawnEquip(value, storeFirst: true));

		public static LaborInstruction<GameObject> GoEquipTool = new LaborInstruction<GameObject>("GoEquipTool", IconKeys.PickUp, (Labor labor, GameObject value) => labor.GoEquipTool(value, storeFirst: false));

		public static LaborInstruction UnequipTool = new LaborInstruction("UnequipTool", IconKeys.PickUp, ToolNode.UnequipTools);
	}

	public class ForLabor
	{
		public static LaborInstruction Sit = new LaborInstruction("Sit", IconKeys.JumpOff, NavigationNodes.Sit);

		public static LaborInstruction<Location> CleanUpAnythingAround = new LaborInstruction<Location>("CleanUp", IconKeys.BroomGroup, HarvestNode.CleanUpAnythingAround);

		public static LaborInstruction<GameObject> CleanUpSimilarObjectsAround = new LaborInstruction<GameObject>("CleanUp", IconKeys.BroomGroup, HarvestNode.CleanUpSimilarObjectsAround);

		public static LaborInstruction<Location> GoCutNewTree = new LaborInstruction<Location>("CutNewTree", IconKeys.AxeGroup, WorkNode.CutTree);

		public static LaborInstruction<Location> GoDig = new LaborInstruction<Location>("GoDig", IconKeys.DigVoxel, WorkNode.Dig);

		public static LaborInstruction<Vector3> HarvestNatureAt = new LaborInstruction<Vector3>("HarvestNature", IconKeys.PickUp, HarvestNode.HarvestNatureAt);

		public static LaborInstruction<GameObject> TransformLog = new LaborInstruction<GameObject>("TransformLog", IconKeys.AxeGroup, HarvestNode.TransformLog);

		public static LaborInstruction<Location> Repair = new LaborInstruction<Location>("TransformLog", IconKeys.RepairTask, WorkNode.Repair);

		public static LaborInstruction<GameComponent> Detonate = new LaborInstruction<GameComponent>("Detonate", IconKeys.BarrelExplode, WorkNode.Detonate);

		public static LaborInstruction<float> Wait = new LaborInstruction<float>("Wait", IconKeys.Cancel, (Labor labor, float time) => Node.WaitForTicks(Mathf.RoundToInt(time * 30f)));
	}

	public class Move
	{
		public static MoveInstruction<Location> RunToExactly = new MoveInstruction<Location>("RunToExactly", IconKeys.MoveTo, NavigationNodes.RunToExactly);

		public static MoveInstruction<Location> WalkToExactly = new MoveInstruction<Location>("WalkToExactly", IconKeys.MoveTo, NavigationNodes.WalkToExactly);

		public static MoveInstruction<Location> RunToward = new MoveInstruction<Location>("RunToward", IconKeys.MoveTo, (Labor labor, Location value) => labor.RunToward(value, precise: true, aggressive: false));

		public static MoveInstruction<Location> RunAggressivelyToward = new MoveInstruction<Location>("RunAggressivelyToward", IconKeys.MoveTo, (Labor labor, Location value) => labor.RunToward(value, precise: false, aggressive: true));

		public static MoveInstruction<Location> TurnAndJumpToward = new MoveInstruction<Location>("TurnAndJumpToward", IconKeys.MoveTo, NavigationNodes.TurnAndJumpToward);

		public static MoveInstruction<Location> WalkToward = new MoveInstruction<Location>("WalkToward", IconKeys.MoveTo, (Labor labor, Location value) => labor.WalkToward(value, precise: true, aggressive: false));

		public static MoveInstruction<Location> WalkAggressivelyToward = new MoveInstruction<Location>("WalkAggressivelyToward", IconKeys.MoveTo, (Labor labor, Location value) => labor.WalkToward(value, precise: false, aggressive: true));

		public static MoveInstruction<Location> TurnTo = new MoveInstruction<Location>("TurnTo", IconKeys.MoveTo, NavigationNodes.TurnToward);

		public static MoveInstruction<PathBranch> UnstackStep = new MoveInstruction<PathBranch>("SingleStep", IconKeys.MoveTo, (Labor labor, PathBranch value) => labor.navigation.UnstackStep(value));

		public static MoveInstruction<GameObject> FollowCharacter = new MoveInstruction<GameObject>("FollowCharacter", IconKeys.Bricktron, (Labor labor, GameObject value) => labor.RunToExactly(Location.ToFollow(value.GetComponent<Labor>())));

		public static MoveInstruction<GameObject> MoveToVolume = new MoveInstruction<GameObject>("MoveToVolume", IconKeys.MoveTo, (Labor labor, GameObject value) => labor.RunToExactly(Location.Of(value)));

		public static MoveInstruction<GameObject> MoveToVolumeAndHold = new MoveInstruction<GameObject>("MoveToObjectAndHold", IconKeys.MoveTo, (Labor labor, GameObject value) => (StaticNode)Node.Sequence.Do(labor.RunToward(Location.Of(value), precise: true, aggressive: false)).Do(delegate
		{
			AutonomyStatus holdingPosition = AutonomyStatus.HoldingPosition;
			labor.Autonomy = holdingPosition;
			return holdingPosition;
		}));

		public static MoveInstruction<GameObject> InterceptCharacter = new MoveInstruction<GameObject>("InterceptCharacter", IconKeys.MoveTo, (Labor labor, GameObject value) => labor.RunToExactly(Location.ToIntercept(value.GetComponent<Labor>())));

		public static MoveInstruction Halt = new MoveInstruction("Halt", IconKeys.MoveTo, (Labor labor) => labor.navigation.HaltAndWaitForInPlace());

		public static MoveInstruction HaltWhatever = new MoveInstruction("HaltWhatever", IconKeys.MoveTo, (Labor labor) => labor.HaltWhatever());

		public static MoveInstruction HaltAndWait = new MoveInstruction("HaltAndWait", IconKeys.MoveTo, (Labor labor) => (StaticNode)Node.Sequence.Label("CompleteStop Sequence").Do(labor.HaltWhatever()).Do(delegate
		{
			AutonomyStatus autonomyStatus = AutonomyStatus.FromId(AutonomyStatus.Ids.waitForOrder);
			labor.Autonomy = autonomyStatus;
			return autonomyStatus;
		}));

		public static MoveInstruction<float> CompleteStop = new MoveInstruction<float>("CompleteStop", IconKeys.StopWatch, NavigationNodes.CompleteStop);

		public static MoveInstruction JumpOff = new MoveInstruction("JumpOff", IconKeys.MoveTo, NavigationNodes.JumpOff);
	}

	public class Transfer
	{
		public static LaborInstruction<GameObject> SmartTransfer = new LaborInstruction<GameObject>("SmartTransfer", IconKeys.MoveTo, (Labor labor, GameObject value) => labor.GetSmartTransferNode(value, alternateAction: true));

		public static LaborInstruction<GameObject> SmartTransfer_AlternateAction = new LaborInstruction<GameObject>("SmartTransfer", IconKeys.PickUp, (Labor labor, GameObject value) => labor.GetSmartTransferNode(value, alternateAction: false));

		public static LaborInstruction<GameComponent> GoStoreInto = new LaborInstruction<GameComponent>("GoStoreInto", IconKeys.PickUp, StoreNode.GoStoreIn);

		public static LaborInstruction GoStore = new LaborInstruction("GoStore", IconKeys.PickUp, StoreNode.GoStoreInBestStorage);

		public static LaborInstruction GoStoreOrDrop = new LaborInstruction("GoStoreOrDrop", IconKeys.PickUp, (Labor labor) => (StaticNode)Node.Catcher.Do(labor.GoStoreInBestStorage()).Catch((Exceptions.WorkFailedException e, Labor l) => e.DoOnError(SharedWorkException<Exceptions.CouldNotFindStorageForInventory>.Get.Problem, l.Drop()), labor));

		public static LaborInstruction<Location> GoStoreOrDropAt = new LaborInstruction<Location>("GoStoreOrDropAt", IconKeys.PickUp, (Labor labor, Location value) => labor.NestedGoStoreInBestStorageOrDrop());

		public static LaborInstruction Drop = new LaborInstruction("Drop", IconKeys.DropHere, DropNode.Drop);

		public static LaborInstruction<Location> GoDropAt = new LaborInstruction<Location>("GoDropAt", IconKeys.DropThere, (Labor labor, Location value) => labor.GoDropAt(value, canFail: true));

		public static LaborInstruction<GameObject> GoPickUpOne = new LaborInstruction<GameObject>("GoPickUpOne", IconKeys.DropThere, (Labor labor, GameObject value) => labor.GoPickUp(value.GetComponent<GameComponent>()));

		public static LaborInstruction<Tuple<GameObject, int>> GoPickUp = new LaborInstruction<Tuple<GameObject, int>>("GoPickUp", IconKeys.DropThere, (Labor labor, Tuple<GameObject, int> value) => labor.GoPickUp(value._1.GetComponent<GameComponent>(), value._2));

		public static LaborInstruction<GameObject> GoPickUpFrom = new LaborInstruction<GameObject>("GoPickUpFrom", IconKeys.DropThere, (Labor labor, GameObject value) => labor.GoPickUp(value.GetComponent<GameComponent>(), -1));

		public static LaborInstruction<GameComponent> GoPickUpAmmo = new LaborInstruction<GameComponent>("GoPickUpAmmo", IconKeys.DropThere, PickUpNode.GoPickUpAmmo);
	}

	public class Combat
	{
		public static CombatInstruction<GameObject> GoAttack = new CombatInstruction<GameObject>("GoAttack", IconKeys.Corruptron, (Labor labor, GameObject value) => labor.GoAttack(value));

		public static CombatInstruction<GameObject> GoAttackWithPriority = new CombatInstruction<GameObject>("GoAttackWithPriority", IconKeys.Corruptron, (Labor labor, GameObject value) => labor.GoAttackWithPriority(value));

		public static CombatInstruction<GameObject> GoAttackDirectly = new CombatInstruction<GameObject>("GoAttackDirectly", IconKeys.Corruptron, (Labor labor, GameObject value) => labor.GoAttackDirectly(value));

		public static CombatInstruction<Location> HoldPositionAndAttack = new CombatInstruction<Location>("HoldPositionAndAttack", IconKeys.Corruptron, (Labor labor, Location value) => labor.HoldPositionAndAttack());

		public static CombatInstruction<GameObject> HoldPositionAndAttackDirectly = new CombatInstruction<GameObject>("HoldPositionAndAttackDirectly", IconKeys.Corruptron, (Labor labor, GameObject value) => labor.HoldPositionAndAttackDirectly(value));

		public static CombatInstruction<GameObject> HoldPositionAndAttackWithPriority = new CombatInstruction<GameObject>("HoldPositionAndAttackWithPriority", IconKeys.Corruptron, (Labor labor, GameObject value) => labor.HoldPositionAndAttackWithPriority(value));

		public static CombatInstruction<Location> ShootAt = new CombatInstruction<Location>("ShootAt", IconKeys.Archer, (Labor labor, Location value) => labor.Occupation.Profession.ShootAt(value));

		public static CombatInstruction<GameComponent> BreakObject = new CombatInstruction<GameComponent>("BreakObject", IconKeys.FlagRed, CombatNodes.BreakObject);

		public static CombatInstruction<Location> MoveAttack = new CombatInstruction<Location>("MoveAttack", IconKeys.FlagRed, (Labor labor, Location value) => labor.MoveAttack(value));

		public static CombatInstruction<Location> WalkMoveAttack = new CombatInstruction<Location>("WalkMoveAttack", IconKeys.FlagRed, (Labor labor, Location value) => labor.WalkMoveAttack(value));
	}

	public class Package
	{
		public static OrderPackage<TLabor> Empty<TLabor>() where TLabor : BaseLabor
		{
			return OrderPackage<TLabor>.Empty;
		}

		public static OrderPackage<TLabor> Of<TLabor>(WorkInstruction<TLabor> instruction, Labor.DecisionSource decisionSource = Labor.DecisionSource.Unspecified) where TLabor : BaseLabor
		{
			OrderPackage<TLabor> orderPackage = new OrderPackage<TLabor>(instruction);
			orderPackage.DecisionSource = decisionSource;
			return orderPackage;
		}

		public static OrderPackage<TLabor, GameComponent> OfGameComponent<TLabor>(WorkInstruction<TLabor, GameComponent> instruction, GameComponent data, Labor.DecisionSource decisionSource = Labor.DecisionSource.Unspecified) where TLabor : BaseLabor
		{
			return new OrderPackage<TLabor, GameComponent>(instruction, data, decisionSource);
		}

		public static OrderPackage<TLabor, Location> OfLocation<TLabor>(WorkInstruction<TLabor, Location> instruction, Location data, Labor.DecisionSource decisionSource = Labor.DecisionSource.Unspecified) where TLabor : BaseLabor
		{
			return new OrderPackage<TLabor, Location>(instruction, data, decisionSource);
		}

		public static OrderPackage<TLabor, GameObject> OfGameObject<TLabor>(WorkInstruction<TLabor, GameObject> instruction, GameObject data, Labor.DecisionSource decisionSource = Labor.DecisionSource.Unspecified) where TLabor : BaseLabor
		{
			return new OrderPackage<TLabor, GameObject>(instruction, data, decisionSource);
		}

		public static OrderPackage<TLabor, Recipe> OfRecipe<TLabor>(WorkInstruction<TLabor, Recipe> instruction, Recipe data, Labor.DecisionSource decisionSource = Labor.DecisionSource.Unspecified) where TLabor : BaseLabor
		{
			return new OrderPackage<TLabor, Recipe>(instruction, data, decisionSource);
		}

		public static OrderPackage<TLabor, Description> OfDescription<TLabor>(WorkInstruction<TLabor, Description> instruction, Description data, Labor.DecisionSource decisionSource = Labor.DecisionSource.Unspecified) where TLabor : BaseLabor
		{
			return new OrderPackage<TLabor, Description>(instruction, data, decisionSource);
		}

		public static OrderPackage<TLabor, IOperable<TLabor>> OfOperable<TLabor>(WorkInstruction<TLabor, IOperable<TLabor>> instruction, IOperable<TLabor> data, Labor.DecisionSource decisionSource = Labor.DecisionSource.Unspecified) where TLabor : BaseLabor
		{
			return new OrderPackage<TLabor, IOperable<TLabor>>(instruction, data, decisionSource);
		}

		public static OrderPackage<TLabor, Vector3> OfVector3<TLabor>(WorkInstruction<TLabor, Vector3> instruction, Vector3 data, Labor.DecisionSource decisionSource = Labor.DecisionSource.Unspecified) where TLabor : BaseLabor
		{
			return new OrderPackage<TLabor, Vector3>(instruction, data, decisionSource);
		}

		public static OrderPackage<TLabor, float> OfFloat<TLabor>(WorkInstruction<TLabor, float> instruction, float data, Labor.DecisionSource decisionSource = Labor.DecisionSource.Unspecified) where TLabor : BaseLabor
		{
			return new OrderPackage<TLabor, float>(instruction, data, decisionSource);
		}

		public static OrderPackage<TLabor, Tuple<GameObject, int>> OfGoIntTuple<TLabor>(WorkInstruction<TLabor, Tuple<GameObject, int>> instruction, Tuple<GameObject, int> data, Labor.DecisionSource decisionSource = Labor.DecisionSource.Unspecified) where TLabor : BaseLabor
		{
			return new OrderPackage<TLabor, Tuple<GameObject, int>>(instruction, data, decisionSource);
		}
	}

	public static EmptyInstruction<Labor> Empty = new EmptyInstruction<Labor>();
}
