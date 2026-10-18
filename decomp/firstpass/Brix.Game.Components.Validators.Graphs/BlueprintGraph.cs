using System;
using System.Collections.Generic;
using Brix.External.Factories;
using Brix.Util;
using Brix.Utils;
using Brix.Utils.Collections;
using Newtonsoft.Json;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.Components.Validators.Graphs;

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class BlueprintGraph
{
	private class ClosestToComparer : IComparer<Vector3>
	{
		private Vector3 origin;

		public ClosestToComparer(Vector3 origin)
		{
			this.origin = origin;
		}

		public int Compare(Vector3 v1, Vector3 v2)
		{
			float num = SquareDistanceTo(v1);
			float num2 = SquareDistanceTo(v2);
			if (num > num2)
			{
				return 1;
			}
			if (num < num2)
			{
				return -1;
			}
			return 0;
		}

		private float SquareDistanceTo(Vector3 position)
		{
			return (position.x - origin.x) * (position.x - origin.x) + (position.z - origin.z) * (position.z - origin.z);
		}
	}

	private static Factory.AssetKey[] excludedKeys = new Factory.AssetKey[2]
	{
		Blueprints.RopeBridgeFrame,
		Blueprints.RopeBridgePlank
	};

	[JsonProperty]
	public List<ValidationNode> RootNodes = new List<ValidationNode>();

	[JsonProperty]
	private List<ValidationNode> _nodes = new List<ValidationNode>();

	private Dictionary<GameObject, ValidationNode> _goToNode = new Dictionary<GameObject, ValidationNode>();

	private Vector3 _originalSourcePosition;

	private Faction _faction;

	public Vector3 OriginalSourcePosition => _originalSourcePosition;

	public List<ValidationNode> Nodes => _nodes;

	public Faction Faction
	{
		get
		{
			return _faction;
		}
		set
		{
			_faction = value;
			Nodes.Slinq().ForEach(delegate(ValidationNode node, Faction faction)
			{
				node.SetFaction(faction);
			}, value);
		}
	}

	public void LegacyValidate()
	{
		List<ValidationData> list = (from n in _nodes.Slinq()
			select n.Target.GetComponent<ValidatorComponent>().VData).ToPooledList();
		_nodes.Slinq().ForEach(delegate(ValidationNode node, List<ValidationData> v)
		{
			node.Target.GetComponent<ValidatorComponent>().Validate(v);
		}, list);
		list.Dispose();
	}

	public void Validate()
	{
		RootNodes.Slinq().ForEach(delegate(ValidationNode node, Guid guid)
		{
			node.UpdateNode(guid);
		}, Guid.NewGuid());
	}

	public void AddNode(ValidationNode node)
	{
		if (node.IsRootNode)
		{
			RootNodes.Add(node);
		}
		_nodes.Add(node);
		_goToNode[node.Target] = node;
	}

	public ValidationNode GetNode(GameObject gameObject)
	{
		return _goToNode.GetValueOrDefault(gameObject);
	}

	public bool TryGetNode(GameObject gameObject, out ValidationNode node)
	{
		return _goToNode.TryGetValue(gameObject, out node);
	}

	public bool HasNode(GameObject gameObject)
	{
		return _goToNode.ContainsKey(gameObject);
	}

	private void RelativizeNodePositions()
	{
		Vector3 zero = Vector3.zero;
		int num = int.MaxValue;
		List<GameObject> list = CollectionPool.RequestList<GameObject>();
		foreach (ValidationNode node in _nodes)
		{
			zero += node.Pos;
			BaseVoxelTransform component = node.Target.GetComponent<BaseVoxelTransform>();
			int y = component.position.y;
			if (y <= num)
			{
				if (y != num)
				{
					num = y;
					list.Clear();
				}
				list.Add(component.gameObject);
			}
		}
		Vector3 vector = Calcule.RoundVecteur(new Vector3(zero.x / (float)_nodes.Count, num, zero.z / (float)_nodes.Count));
		Vector3 vector2 = Calcule.RoundVecteur((from go in list.Slinq()
			select go.transform.position).Min(new ClosestToComparer(vector)).ValueOr(vector));
		vector2.y = num;
		foreach (ValidationNode node2 in _nodes)
		{
			node2.Pos -= vector2;
		}
		_originalSourcePosition = vector2;
	}

	public void SpawnBlueprintsUnder(Transform parent, Faction faction)
	{
		RootNodes.Clear();
		foreach (ValidationNode node in _nodes)
		{
			node.SpawnUnder(parent);
			node.SetFaction(faction);
			if (node.IsRootNode)
			{
				RootNodes.Add(node);
			}
			_goToNode[node.Target] = node;
		}
	}

	public void ExpandGraph()
	{
		RootNodes.ForEach(delegate(ValidationNode node)
		{
			node.PropagateAddToParents();
		});
	}

	public void AddNodes(HashSet<GameObject> gosInGraph)
	{
		foreach (GameObject item in gosInGraph)
		{
			if (!excludedKeys.Slinq().Contains(item.GetComponent<FactoryImprint>().AssetKey) && !HasNode(item))
			{
				item.GetComponent<ValidatorComponent>().AddBlueprintValidationNodeToGraph(this, gosInGraph);
			}
		}
		RelativizeNodePositions();
	}
}
