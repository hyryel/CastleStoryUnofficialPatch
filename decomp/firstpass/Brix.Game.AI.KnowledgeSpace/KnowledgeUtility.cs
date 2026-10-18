using System;
using System.Collections.Generic;
using Brix.Game.Components;
using Brix.Game.Semantique;
using Brix.Utils.Factories;
using Smooth.Algebraics;
using UnityEngine;

namespace Brix.Game.AI.KnowledgeSpace;

public static class KnowledgeUtility
{
	private static Collider[] hits = new Collider[500];

	public static float ThreeWayDistance(GameObject A, GameObject B, GameObject C)
	{
		if (!A || !B || !C)
		{
			return float.MaxValue;
		}
		return Vector3.Distance(A.transform.position, B.transform.position) + Vector3.Distance(B.transform.position, C.transform.position);
	}

	public static float LaborGoalDistance(Labor labor, GameObject goal)
	{
		if (!labor || !goal)
		{
			return float.MaxValue;
		}
		return Vector3.Distance(labor.transform.position, goal.transform.position);
	}

	public static float EuclidianDistance(GameObject A, GameObject B)
	{
		if (!A || !B)
		{
			return float.MaxValue;
		}
		return Vector3.Distance(A.transform.position, B.transform.position);
	}

	public static float EuclidianDistance(IGameComponent A, IGameComponent B)
	{
		if (A.IsNullOrReleased() || B.IsNullOrReleased())
		{
			return float.MaxValue;
		}
		return Vector3.Distance(A.GameObject.transform.position, B.GameObject.transform.position);
	}

	public static GameObject ClosestFromA(GameObject A, GameObject B, GameObject C)
	{
		if (!A)
		{
			return null;
		}
		if (!B && !C)
		{
			return null;
		}
		if (!B)
		{
			return C;
		}
		if (!C)
		{
			return B;
		}
		if (Vector3.Distance(B.transform.position, A.transform.position) < Vector3.Distance(C.transform.position, A.transform.position))
		{
			return B;
		}
		return C;
	}

	public static Func<Labor, IGameComponent, IGameComponent, float> GetStorageDistance()
	{
		return (Labor agent, IGameComponent storage, IGameComponent toStore) => storage.WithRecepteur((Recepteur r, Tuple<Labor, IGameComponent> t) => StorageDistance(t._1, r, t._2), EuclidianDistance(agent, storage), Tuple.Create(agent, toStore));
	}

	private static float StorageDistance(IGameComponent agent, Recepteur storage, IGameComponent toStore)
	{
		if (agent.IsNullOrReleased() || storage.IsNullOrReleased())
		{
			return float.MaxValue;
		}
		float num = EuclidianDistance(agent, storage);
		float num2 = storage.StoreDistanceBonusFor(Apparence.PrincipaleOf(toStore));
		return num - num2;
	}

	public static Func<Labor, IGameComponent, float> GetPickupDistance()
	{
		return (Labor agent, IGameComponent storage) => storage.WithRecepteur((Recepteur r, Labor t) => PickupDistance(t, r), EuclidianDistance(agent, storage), agent);
	}

	private static float PickupDistance(IGameComponent agent, Recepteur storage)
	{
		if (agent.IsNullOrReleased() || storage.IsNullOrReleased())
		{
			return float.MaxValue;
		}
		float num = EuclidianDistance(agent, storage);
		float num2 = storage.PickupDistanceBonus();
		return num - num2;
	}

	public static Func<Labor, IGameComponent, float> GetBestToolDistance()
	{
		return BestToolDistance;
	}

	public static float BestToolDistance(IGameComponent agent, IGameComponent tool)
	{
		if (agent.IsNullOrReleased() || tool.IsNullOrReleased())
		{
			return float.MaxValue;
		}
		return EuclidianDistance(agent, tool) - Tool.EfficiencyOf(tool);
	}

	public static float GetValueFromDistance(KeyValuePair<GameObject, float> kpv)
	{
		return kpv.Value;
	}

	public static void FindAllInRadius<TResult>(Vector3 center, float radius, int layerMask, Func<GameObject, bool> filter, Func<GameObject, TResult> map, ref HashSet<TResult> result)
	{
		int num = Physics.OverlapSphereNonAlloc(center, radius, hits, layerMask);
		for (int i = 0; i < num; i++)
		{
			GameObject gameObject = SignificantParent.Of(hits[i].gameObject);
			if (!gameObject.IsNullOrReleased() && filter(gameObject))
			{
				result.Add(map(gameObject));
			}
		}
		Array.Clear(hits, 0, num);
	}
}
