using System.Collections.Generic;
using Brix.Engine;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Brix.Pathfinding.Test;
using Brix.Utils;
using Motus.Behavior;
using Smooth.Algebraics;
using UnityEngine;

namespace Brix.Game.AI.KnowledgeSpace;

public class KnowledgeExcluder : MonoBehaviour
{
	public KnowledgeConnectedAccessFinder AccessFinder;

	public bool Working;

	public Dictionary<Tuple<GameObject, GameObject>, int> Exclusions2 = new Dictionary<Tuple<GameObject, GameObject>, int>();

	public bool AreExclusive(GameObject A, GameObject B, int regionAccessIndices)
	{
		int valueOrDefault = Exclusions2.GetValueOrDefault(Tuple.Create(A, B));
		return (regionAccessIndices & ~valueOrDefault) == 0;
	}

	public void ResetExclusionsFor(GameObject A)
	{
		if (!(A == null))
		{
			Exclusions2.RemoveAll((KeyValuePair<Tuple<GameObject, GameObject>, int> kv, GameObject a) => kv.Key._1 == a || kv.Key._2 == a, A);
		}
	}

	public Node ExcludeAndSpread(GameObject A, GameObject B, VProperty accessProperty = VProperty.access)
	{
		if (A == null || B == null || AccessFinder == null)
		{
			return Node.Empty;
		}
		return (StaticNode)Node.Sequence.Do(GetMutex()).Do((KnowledgeExcluder self, GameObject b, VProperty p) => self.ComputeConnectedAccessesNode(b, p), this, B, accessProperty).Label("Find")
			.Do(delegate(KnowledgeExcluder self, GameObject a, GameObject b, VProperty p)
			{
				self.ExcludeFoundObjects(a, b, p);
			}, this, A, B, accessProperty)
			.Label("Exclude")
			.Finally(delegate(KnowledgeExcluder self)
			{
				self.ReleaseMutex();
			}, this);
	}

	private Node ComputeConnectedAccessesNode(GameObject b, VProperty accessProperty)
	{
		if (b.IsNullOrReleased())
		{
			return Node.Empty;
		}
		IVolume component = b.GetComponent<IVolume>();
		if (component == null)
		{
			return Node.Empty;
		}
		VolumePropertyRequest cases = component.GetCases(accessProperty);
		return AccessFinder.ComputeConnectedAccessesNode(cases);
	}

	private StaticNode GetMutex()
	{
		return Node.Selector.When((KnowledgeExcluder self) => !self.Working, this).Do((KnowledgeExcluder self) => self.Working = true, this).Label("Acquire Mutex")
			.Else()
			.Do(Node.WaitFor.Ever())
			.Label("Wait forever");
	}

	private void ReleaseMutex()
	{
		Working = false;
	}

	private void ExcludeFoundObjects(GameObject a, GameObject b, VProperty accessProperty)
	{
		if (a.IsNullOrReleased() || b.IsNullOrReleased())
		{
			return;
		}
		IVolume component = b.GetComponent<IVolume>();
		if (component == null)
		{
			return;
		}
		int propertyBitMaskSharedByOtherProperty = component.GetPropertyBitMaskSharedByOtherProperty(VProperty.access, accessProperty);
		Exclude(a, b, propertyBitMaskSharedByOtherProperty);
		foreach (KeyValuePair<IVolume, int> result in AccessFinder.Results)
		{
			Exclude(a, result.Key.GameObject, result.Value);
		}
	}

	private void Exclude(GameObject A, GameObject B, int accessIndices)
	{
		int valueOrDefault = Exclusions2.GetValueOrDefault(Tuple.Create(A, B));
		Exclusions2[Tuple.Create(A, B)] = accessIndices | valueOrDefault;
	}

	private void Start()
	{
		GameSignals.LateConnectOnce(TestPathfinding.InitializedSignal, delegate
		{
			AccessFinder = new KnowledgeConnectedAccessFinder();
		});
	}

	private void OnDestroy()
	{
		if (AccessFinder != null)
		{
			AccessFinder.Destroy();
			AccessFinder = null;
		}
		Exclusions2.Clear();
	}
}
