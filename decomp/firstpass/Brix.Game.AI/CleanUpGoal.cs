using System.Collections.Generic;
using Brix.External.Factories;
using Brix.Game.AI.KnowledgeSpace;
using Brix.Game.AI.Nodes;
using Brix.UI.Icons;
using Motus.Behavior;
using UnityEngine;

namespace Brix.Game.AI;

public class CleanUpGoal : StorableGoal
{
	public override IconKey IconKey => IconKeys.Missing;

	public override Node GetWorkNode(Labor Worker)
	{
		return Worker.GoPickUp(this, -1);
	}

	public override bool IsMatchFor(Labor labor, AIErrorsStore errorDestination, ref float time, ref bool unmappedFlag)
	{
		if (HasOtherWorkerAssigned(labor))
		{
			errorDestination.AddDecisionError(SharedWorkException<Exceptions.GoalTooCrowded>.Get.Problem, labor);
			return false;
		}
		time += 0.05f;
		if (!Knowledge.Instance.CanReach(labor, this))
		{
			errorDestination.AddDecisionError(SharedWorkException<Exceptions.CannotFindWhatINeedToPickUp>.Get.Problem, labor);
			return false;
		}
		time += 0.05f;
		Labor componentInParent = GetComponentInParent<Labor>();
		if (componentInParent != null && componentInParent != labor)
		{
			errorDestination.AddDecisionError(SharedWorkException<Exceptions.ObjectIsInSomeoneElseBag>.Get.Problem, labor);
			return false;
		}
		time += 0.05f;
		if (!labor.recepteur.HasRoomFor(base.gameObject))
		{
			errorDestination.AddDecisionError(SharedWorkException<Exceptions.InventoryIsFull>.Get.Problem, labor);
			return false;
		}
		time += 1f;
		if (labor.BestRecepteurToStore(base.gameObject) == null)
		{
			errorDestination.AddDecisionError(SharedWorkException<Exceptions.NoCorrespondingStorage>.Get.Problem, labor);
			return false;
		}
		return true;
	}

	public override int PriorityFor(Labor labor, ref float time)
	{
		int num = (Knowledge.Instance.IsReserved(base.gameObject) ? 20 : 0);
		int num2 = Mathf.RoundToInt(KnowledgeUtility.EuclidianDistance(labor, this));
		return -num2 - num;
	}

	public override void UniqueItems(ref HashSet<Factory.AssetKey> keys)
	{
		keys.Add(AssetKey);
	}
}
