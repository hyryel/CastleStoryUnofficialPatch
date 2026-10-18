using System.Collections.Generic;
using Brix.Engine;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Brix.Pathfinding.Test;
using Brix.Utils;
using Motus.Behavior;
using UnityEngine;

namespace Brix.Game.AI.KnowledgeSpace;

public class KnowledgeUpdater : Behavior
{
	public KnowledgeDisjointRegionsFinder RegionsFinder;

	public List<KnowledgeRegion> Regions = new List<KnowledgeRegion>();

	public Dictionary<GameObject, List<KnowledgeRegion>> RegionsOf = new Dictionary<GameObject, List<KnowledgeRegion>>();

	private HashSet<GameObject> _participantObjects = new HashSet<GameObject>();

	private List<GameObject> _toRemove = new List<GameObject>();

	protected override Node CreateTree()
	{
		return (StaticNode)Node.Sequence.Do((KnowledgeUpdater self) => self.ComputeDisjointRegionsNode(), this).Label("Compute Regions").Do(delegate(KnowledgeUpdater self)
		{
			self.UpdateRegions();
		}, this)
			.Label("Update Regions")
			.Do(Node.WaitForTicks(200))
			.Label("Wait")
			.Loop();
	}

	private Node ComputeDisjointRegionsNode()
	{
		return (RegionsFinder == null) ? Node.Empty : RegionsFinder.ComputeDisjointRegionsNode();
	}

	private void UpdateRegions()
	{
		if (RegionsFinder == null)
		{
			return;
		}
		int count = Regions.Count;
		int count2 = RegionsFinder.Results.Count;
		if (count > count2)
		{
			for (int i = 0; i < count2; i++)
			{
				Regions[i].ResetObject();
			}
			for (int j = count2; j < count; j++)
			{
				Regions[j].Dispose();
			}
			Regions.RemoveRange(count2, count - count2);
		}
		else
		{
			for (int k = 0; k < count; k++)
			{
				Regions[k].ResetObject();
			}
			for (int l = count; l < count2; l++)
			{
				Regions.Add(ObjectPool<KnowledgeRegion>.Reserve());
			}
		}
		for (int m = 0; m < count2; m++)
		{
			foreach (KeyValuePair<IVolume, int> item in RegionsFinder.Results[m])
			{
				_participantObjects.Add(item.Key.GameObject);
			}
		}
		for (int n = 0; n < count2; n++)
		{
			Regions[n].AddAll(RegionsFinder.Results[n]);
		}
		foreach (KeyValuePair<GameObject, List<KnowledgeRegion>> item2 in RegionsOf)
		{
			if (_participantObjects.Remove(item2.Key))
			{
				item2.Value.Clear();
				continue;
			}
			_toRemove.Add(item2.Key);
			item2.Value.Dispose();
		}
		foreach (GameObject item3 in _toRemove)
		{
			RegionsOf.Remove(item3);
		}
		foreach (GameObject participantObject in _participantObjects)
		{
			RegionsOf.Add(participantObject, CollectionPool.RequestList<KnowledgeRegion>());
		}
		_toRemove.Clear();
		_participantObjects.Clear();
		foreach (KnowledgeRegion region in Regions)
		{
			foreach (KeyValuePair<IVolume, int> member in region.Members)
			{
				GameObject gameObject = ((member.Key == null) ? null : member.Key.GameObject);
				if (gameObject != null)
				{
					RegionsOf[gameObject].Add(region);
				}
			}
		}
	}

	protected override void Start()
	{
		base.Start();
		GameSignals.LateConnectOnce(TestPathfinding.InitializedSignal, delegate
		{
			RegionsFinder = new KnowledgeDisjointRegionsFinder();
		});
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (RegionsFinder != null)
		{
			RegionsFinder.Destroy();
			RegionsFinder = null;
		}
		foreach (KnowledgeRegion region in Regions)
		{
			region.Dispose();
		}
		Regions.Clear();
		foreach (KeyValuePair<GameObject, List<KnowledgeRegion>> item in RegionsOf)
		{
			item.Value.Dispose();
		}
		RegionsOf.Clear();
	}
}
