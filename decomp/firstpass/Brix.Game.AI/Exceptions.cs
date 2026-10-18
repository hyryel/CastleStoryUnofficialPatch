using System;
using Brix.Utils;
using Motus.Behavior;

namespace Brix.Game.AI;

public static class Exceptions
{
	public class UnknownError : SharedWorkException<UnknownError>
	{
		public static readonly Thought Thought = new Thought("##thought_unknown", Thought.ThoughtType._None, string.Empty);

		public override AIError MakeError()
		{
			return new AIError(0, Thought);
		}
	}

	public class NotReady : SharedWorkException<NotReady>
	{
		public static Thought Thought = new Thought("##thought_thinking", Thought.ThoughtType.Info, "Bricktron_Message_Nothing");

		public override AIError MakeError()
		{
			return new AIError(0, Thought);
		}
	}

	public class NullLabor : SharedWorkException<NullLabor>
	{
		public override AIError MakeError()
		{
			return new AIError(0, UnknownError.Thought);
		}
	}

	public class NullTarget : SharedWorkException<NullTarget>
	{
		public override AIError MakeError()
		{
			return new AIError(0, UnknownError.Thought);
		}
	}

	public class NullStorage : SharedWorkException<NullStorage>
	{
		public static Thought Thought = new Thought("##thought_storage_noexist", Thought.ThoughtType.Warning, string.Empty);

		public override AIError MakeError()
		{
			return new AIError(0, Thought);
		}
	}

	public class CharacterCantRepair : SharedWorkException<CharacterCantRepair>
	{
		public static Thought Thought = new Thought("##thought_cant_repair", Thought.ThoughtType.Warning, string.Empty);

		public override AIError MakeError()
		{
			return new AIError(0, Thought);
		}
	}

	public class CharacterCantDig : SharedWorkException<CharacterCantDig>
	{
		public static Thought Thought = new Thought("##thought_cant_break", Thought.ThoughtType.Warning, string.Empty);

		public override AIError MakeError()
		{
			return new AIError(0, Thought);
		}
	}

	public class CharacterCantCutTrees : SharedWorkException<CharacterCantCutTrees>
	{
		public static readonly Thought Thought = new Thought("##thought_cant_choptrees", Thought.ThoughtType.Warning, string.Empty);

		public override AIError MakeError()
		{
			return new AIError(0, Thought);
		}
	}

	public class CharacterCantChopWood : SharedWorkException<CharacterCantChopWood>
	{
		public static readonly Thought Thought = new Thought("##thought_cant_transformlog", Thought.ThoughtType.Warning, string.Empty);

		public override AIError MakeError()
		{
			return new AIError(0, Thought);
		}
	}

	public class CharacterCantHarvestPlant : SharedWorkException<CharacterCantHarvestPlant>
	{
		public static Thought Thought = new Thought("##thought_cant_collect_plants", Thought.ThoughtType.Warning, string.Empty);

		public override AIError MakeError()
		{
			return new AIError(0, Thought);
		}
	}

	public class CharacterCantHarvestMinerals : SharedWorkException<CharacterCantHarvestMinerals>
	{
		public static Thought Thought = new Thought("##thought_cant harvest", Thought.ThoughtType.Warning, string.Empty);

		public override AIError MakeError()
		{
			return new AIError(0, Thought);
		}
	}

	public class CharacterCantJoinProject : SharedWorkException<CharacterCantJoinProject>
	{
		public static Thought Thought = new Thought("##thought_job_notforme", Thought.ThoughtType.Warning, "Bricktron_Message_Nothing");

		public override AIError MakeError()
		{
			return new AIError(0, Thought);
		}
	}

	public class CantReach : SharedWorkException<CantReach>
	{
		public override AIError MakeError()
		{
			return PathException.CouldNotFindPath;
		}
	}

	public class CantReachTree : SharedWorkException<CantReachTree>
	{
		public static Thought Thought = new Thought("##thought_looking_for_path_to_tree", Thought.ThoughtType.Warning, string.Empty);

		public override AIError MakeError()
		{
			return new AIError(0, Thought);
		}
	}

	public class CantMove : PathException
	{
		public override bool SpreadExclusions => true;

		public static AIError Problem_ => PathException.CouldNotFindPath;

		public override AIError Problem => Problem_;

		public override void Dispose()
		{
		}
	}

	public class PathInvalid : PathException
	{
		public override bool SpreadExclusions => false;

		public static AIError Problem_ => PathException.CouldNotFindPath;

		public override AIError Problem => Problem_;

		public override void Dispose()
		{
		}
	}

	public class PathBlocked : PathException
	{
		public override bool SpreadExclusions => false;

		public static AIError Problem_ => PathException.CouldNotFindPath;

		public override AIError Problem => Problem_;

		public override void Dispose()
		{
		}
	}

	public class PathBlockedSecondTime : PathException
	{
		public override bool SpreadExclusions => false;

		public static AIError Problem_ => PathException.CouldNotFindPath;

		public override AIError Problem => Problem_;

		public override void Dispose()
		{
		}
	}

	public class PathExecutionFailed : PathException
	{
		public override bool SpreadExclusions => false;

		public static AIError Problem_ => PathException.CouldNotFindPath;

		public override AIError Problem => Problem_;

		public override void Dispose()
		{
		}
	}

	public class PathUnfindable : PathException
	{
		public override bool SpreadExclusions => true;

		public static AIError Problem_ => PathException.CouldNotFindPath;

		public override AIError Problem => Problem_;

		public override void Dispose()
		{
		}
	}

	public class NoCorrespondingStorage : SharedWorkException<NoCorrespondingStorage>
	{
		public static readonly Thought Thought = new Thought("##thought_could_not_find_storage", Thought.ThoughtType.Warning, string.Empty);

		public override AIError MakeError()
		{
			return new AIError(0, Thought);
		}
	}

	public class CannotFindWhatINeedToPickUp : SharedWorkException<CannotFindWhatINeedToPickUp>
	{
		public static Thought Thought = new Thought("##thought_cant_find", Thought.ThoughtType.Warning, string.Empty);

		public override AIError MakeError()
		{
			return new AIError(0, Thought);
		}
	}

	public class CouldNotFindStorageForInventory : SharedWorkException<CouldNotFindStorageForInventory>
	{
		public static Thought InventoryIsFull = new Thought("##thought_inventory_full", Thought.ThoughtType.Warning, string.Empty);

		public override AIError MakeError()
		{
			return new AIError(0, NoCorrespondingStorage.Thought);
		}
	}

	public class LookingForStorage : SharedWorkException<LookingForStorage>
	{
		public static readonly Thought Thought = new Thought("##thought_looking_for_storage", Thought.ThoughtType.Warning, string.Empty);

		public override AIError MakeError()
		{
			return new AIError(0, Thought);
		}
	}

	public class InventoryBecameEmpty : SharedWorkException<InventoryBecameEmpty>
	{
		public static Thought Thought = new Thought("##thought_inventory_empty", Thought.ThoughtType.Warning, string.Empty);

		public override AIError MakeError()
		{
			return new AIError(0, Thought);
		}
	}

	public class StorageBecameFull : SharedWorkException<StorageBecameFull>
	{
		public static Thought Thought = new Thought("##thought_storage_full", Thought.ThoughtType.Warning, string.Empty);

		public override AIError MakeError()
		{
			return new AIError(0, Thought);
		}
	}

	public class ObjectHasBeenMovedFromRecepteur : SharedWorkException<ObjectHasBeenMovedFromRecepteur>
	{
		public static Thought Thought = new Thought("##thought_object_has_changed_container", Thought.ThoughtType.Warning, string.Empty);

		public override AIError MakeError()
		{
			return new AIError(0, CannotFindWhatINeedToPickUp.Thought);
		}
	}

	public class ObjectDoesNotFitInDestination : SharedWorkException<ObjectDoesNotFitInDestination>
	{
		public override AIError MakeError()
		{
			return new AIError(0, StorageBecameFull.Thought);
		}
	}

	public class ObjectHasBeenConverted : SharedWorkException<ObjectHasBeenConverted>
	{
		public static Thought Thought = new Thought("##thought_object_was_converted", Thought.ThoughtType.Warning, string.Empty);

		public override AIError MakeError()
		{
			return new AIError(0, Thought);
		}
	}

	public class InventoryIsFull : SharedWorkException<InventoryIsFull>
	{
		public static Thought Thought = new Thought("##thought_inventory_full", Thought.ThoughtType.Warning, string.Empty);

		public override AIError MakeError()
		{
			return new AIError(0, Thought);
		}
	}

	public class ObjectIsInSomeoneElseBag : SharedWorkException<ObjectIsInSomeoneElseBag>
	{
		public static Thought Thought = new Thought("##thought_object_in_someone_else_bag", Thought.ThoughtType.Warning, string.Empty);

		public override AIError MakeError()
		{
			return new AIError(0, Thought);
		}
	}

	public class ToolUnfindable : SharedWorkException<ToolUnfindable>
	{
		public static Thought Thought = new Thought("##thought_cant_find", Thought.ThoughtType.Warning, string.Empty);

		public override AIError MakeError()
		{
			return new AIError(0, Thought);
		}
	}

	public class CombatIsNotEnnemy : SharedWorkException<CombatIsNotEnnemy>
	{
		public static Thought Thought = new Thought("##thought_will_not_betray_my_friends", Thought.ThoughtType.Warning, string.Empty);

		public override AIError MakeError()
		{
			return new AIError(0, Thought);
		}
	}

	public class CombatTargetIsDead : SharedWorkException<CombatTargetIsDead>
	{
		public static Thought Thought = new Thought("##thought_target_is_dead", Thought.ThoughtType.Warning, string.Empty);

		public override AIError MakeError()
		{
			return new AIError(0, Thought);
		}
	}

	public class CombatTargetIsTooFarAway : SharedWorkException<CombatTargetIsTooFarAway>
	{
		public static Thought Thought = new Thought("##thought_target_is_too_far_away", Thought.ThoughtType.Warning, string.Empty);

		public override AIError MakeError()
		{
			return new AIError(0, Thought);
		}
	}

	public class CombatNotGrounded : SharedWorkException<CombatNotGrounded>
	{
		public static Thought Thought = new Thought("##thought_not_grounded", Thought.ThoughtType.Warning, string.Empty);

		public override AIError MakeError()
		{
			return new AIError(0, Thought);
		}
	}

	public class VoxelHasBeenBuildOver : SharedWorkException<VoxelHasBeenBuildOver>
	{
		public static Thought Thought = new Thought("##thought_voxel_was_built_over", Thought.ThoughtType.Warning, string.Empty);

		public override AIError MakeError()
		{
			return new AIError(0, Thought);
		}
	}

	public class BagTooFullToDig : SharedWorkException<BagTooFullToDig>
	{
		public static Thought Thought = new Thought("##thought_carrying_toomuch_todig", Thought.ThoughtType.Warning, string.Empty);

		public override AIError MakeError()
		{
			return new AIError(0, Thought);
		}
	}

	public class CantWorkNearEnemy : SharedWorkException<CantWorkNearEnemy>
	{
		public static Thought Thought = new Thought("##thought_area_is_too_dangerous", Thought.ThoughtType.Warning, string.Empty);

		public override AIError MakeError()
		{
			return new AIError(0, Thought);
		}
	}

	public class ObjectsIsBusy : SharedWorkException<ObjectsIsBusy>
	{
		public static readonly Thought Thought = new Thought("##thought_already_enough_workers_doing_goal", Thought.ThoughtType.Info, "Bricktron_Message_AllWorkAccountedFor");

		public override AIError MakeError()
		{
			return new AIError(0, Thought);
		}
	}

	public class GoalTooCrowded : SharedWorkException<GoalTooCrowded>
	{
		public override AIError MakeError()
		{
			return new AIError(0, ObjectsIsBusy.Thought);
		}
	}

	public class GoalUnavailable : SharedWorkException<GoalUnavailable>
	{
		public static readonly Thought Thought = new Thought("##thought_works_accounted", Thought.ThoughtType.Info, "Bricktron_Message_AllWorkAccountedFor");

		public override AIError MakeError()
		{
			return new AIError(0, Thought);
		}
	}

	public class ProjectWithoutGoal : SharedWorkException<ProjectWithoutGoal>
	{
		public static Thought Thought = new Thought("##thought_nothingtodo", Thought.ThoughtType.Warning, "Bricktron_Message_Nothing");

		public override AIError MakeError()
		{
			return new AIError(0, Thought);
		}
	}

	public class ProjectFullWorker : SharedWorkException<ProjectFullWorker>
	{
		public static Thought Thought = new Thought("##thought_enough_workers_in_project", Thought.ThoughtType.Warning, "Bricktron_Message_Nothing");

		public override AIError MakeError()
		{
			return new AIError(0, Thought);
		}
	}

	public class ProjectNoStorage : SharedWorkException<ProjectNoStorage>
	{
		public override AIError MakeError()
		{
			return new AIError(0, NoCorrespondingStorage.Thought);
		}
	}

	public class Thoughts
	{
	}

	public class WorkFailedException : FailException
	{
		protected AIError problem;

		public virtual AIError Problem => problem;

		protected WorkFailedException()
		{
			problem = MakeError();
		}

		public virtual AIError MakeError()
		{
			return null;
		}

		public Node DoOnError(AIError error, Node action)
		{
			if (error == Problem)
			{
				return action;
			}
			return Node.Fail(this);
		}
	}

	public abstract class PathException : WorkFailedException, IPoolable, IResetable, IDisposable
	{
		public static Thought CantReachDestination = new Thought("##thought_reach_destination", Thought.ThoughtType.Warning, string.Empty);

		public static readonly AIError CouldNotFindPath = new AIError(0, CantReachDestination);

		public Location Location { get; private set; }

		public virtual bool SpreadExclusions { get; protected set; }

		public static T To<T>(Location location) where T : PathException, new()
		{
			return new T
			{
				Location = location
			};
		}

		public abstract void Dispose();

		public void ResetObject()
		{
			Location = null;
		}
	}
}
