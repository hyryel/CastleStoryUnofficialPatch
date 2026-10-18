using System;
using System.Collections.Generic;
using Brix.Engine;
using Brix.Game.AI.Pathfinding;
using Brix.Pathfinding;
using Brix.Pathfinding.Test;
using Brix.Util;
using Brix.Utils;
using Brix.Utils.Collections;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Input.ContextualOrders;

public class Formation : IPoolable, IResetable, IDisposable
{
	private readonly List<Vector3> _generatedPositions = new List<Vector3>();

	private readonly FormationSearch _search = new FormationSearch();

	public void Dispose()
	{
		ResetObject();
	}

	public void ResetObject()
	{
		_generatedPositions.Clear();
	}

	public Formation Init(Vector3 targetPos, int nbToGenerate)
	{
		GetDestinationPositions(targetPos, nbToGenerate, _generatedPositions);
		return this;
	}

	public Formation Init(Vector3 targetPos, List<GameObject> actors)
	{
		GetDestinationPositions(targetPos, actors.Count, _generatedPositions);
		List<Vector3> list = (from a in actors.Slinq()
			select a.transform.position).ToPooledList();
		SortDestinations(_generatedPositions, list);
		list.Dispose();
		return this;
	}

	public Vector3 Spot(int index)
	{
		return _generatedPositions[index];
	}

	private void GetDestinationPositions(Vector3 destination, int count, List<Vector3> result)
	{
		WalkablePathNode walkablePathNode = PathNodeRegistry.WalkableAt(XYZ.FromVector3(destination));
		if (walkablePathNode == null || walkablePathNode.IsEmpty)
		{
			for (int i = 0; i < count; i++)
			{
				result.Add(destination);
			}
			return;
		}
		HashSet<PathNode> hashSet = CollectionPool.RequestHashSet<PathNode>();
		HashSet<PathNode> hashSet2 = CollectionPool.RequestHashSet<PathNode>();
		_search.Search(hashSet, hashSet2, walkablePathNode, count);
		PopulateDestinations(destination, hashSet, hashSet2, count, result);
		hashSet.Dispose();
		hashSet2.Dispose();
	}

	private static void PopulateDestinations(Vector3 destination, HashSet<PathNode> destinationNodes, HashSet<PathNode> neighborNodes, int count, List<Vector3> result)
	{
		(from p in destinationNodes.Slinq()
			select p.RepresentativePosition.ToVector3()).AddTo(result);
		if (result.Count >= count)
		{
			return;
		}
		foreach (PathNode neighborNode in neighborNodes)
		{
			result.Add(neighborNode.RepresentativePosition.ToVector3());
			if (result.Count >= count)
			{
				break;
			}
		}
		if (result.Count == 0)
		{
			result.Add(destination);
		}
		int count2 = result.Count;
		for (int num = 0; num < count - count2; num++)
		{
			result.Add(result[num % count2]);
		}
	}

	private static void SortDestinations(List<Vector3> destinations, List<Vector3> origins)
	{
		Vector3 originCenter = Calcule.AverageVector(origins);
		Vector3 destinationCenter = Calcule.AverageVector(destinations);
		int count = destinations.Count;
		List<int> list = CollectionPool.RequestList<int>();
		HashSet<int> hashSet = CollectionPool.RequestHashSet<int>();
		for (int i = 0; i < count; i++)
		{
			hashSet.Add(i);
		}
		for (int j = 0; j < count; j++)
		{
			int num = -1;
			float num2 = 0f;
			foreach (int item in hashSet)
			{
				float distanceFromIdealPosition = GetDistanceFromIdealPosition(origins[item], originCenter, destinations[j], destinationCenter);
				if (num < 0 || distanceFromIdealPosition < num2)
				{
					num = item;
					num2 = distanceFromIdealPosition;
				}
			}
			list.Add(num);
			hashSet.Remove(num);
		}
		List<Vector3> list2 = destinations.ToPooledList();
		for (int k = 0; k < count; k++)
		{
			destinations[list[k]] = list2[k];
		}
		list2.Dispose();
		list.Dispose();
		hashSet.Dispose();
	}

	private static float GetDistanceFromIdealPosition(Vector3 origin, Vector3 originCenter, Vector3 destination, Vector3 destinationCenter)
	{
		return (origin - originCenter - (destination - destinationCenter)).sqrMagnitude;
	}
}
