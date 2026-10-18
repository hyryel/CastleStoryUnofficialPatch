using System;
using System.Collections;
using System.Collections.Generic;
using Brix.Components;
using Brix.Engine;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.AI;
using Brix.Game.Components.Validators.Graphs;
using Brix.Game.Compounders;
using Brix.Game.Network;
using Brix.Lifecycle.Pooling;
using Brix.Utils;
using UnityEngine;

namespace Brix.Game.Components;

[RequireComponent(typeof(TrackableVoxelTransform))]
public class BlueprintCompoundComponent : BrixComponent
{
	public BlueprintGraph Graph;

	public Project DestinationProject;

	private IEnumerator placeBlueprints;

	public Signal PlaceBlueprintsSignal = new Signal();

	public Signal DoneInstantiatingSignal = new Signal();

	public bool NeedsGraphRebuild;

	private int _currentRotationIndex;

	private static AxisOrient[] _possibleRotations = new AxisOrient[4]
	{
		AxisOrient.forward,
		AxisOrient.right,
		AxisOrient.back,
		AxisOrient.left
	};

	private static Dictionary<Factory.AssetKey, Factory.AssetKey> _restrictedRotationKeys = new Dictionary<Factory.AssetKey, Factory.AssetKey>
	{
		{
			Blueprints.Wood_Ramp_TopA,
			Blueprints.Wood_Ramp_TopB
		},
		{
			Blueprints.Wood_Ramp_TopB,
			Blueprints.Wood_Ramp_TopA
		},
		{
			Blueprints.Wood_Ramp_MidA,
			Blueprints.Wood_Ramp_MidB
		},
		{
			Blueprints.Wood_Ramp_MidB,
			Blueprints.Wood_Ramp_MidA
		},
		{
			Blueprints.Wood_Ramp_BotA,
			Blueprints.Wood_Ramp_BotB
		},
		{
			Blueprints.Wood_Ramp_BotB,
			Blueprints.Wood_Ramp_BotA
		},
		{
			Blueprints.Wood_Ramp_JunctionA,
			Blueprints.Wood_Ramp_JunctionB
		},
		{
			Blueprints.Wood_Ramp_JunctionB,
			Blueprints.Wood_Ramp_JunctionA
		},
		{
			Blueprints.Wood_Ramp_JunctionC,
			Blueprints.Wood_Ramp_JunctionD
		},
		{
			Blueprints.Wood_Ramp_JunctionD,
			Blueprints.Wood_Ramp_JunctionC
		}
	};

	public override void OnRequested()
	{
		base.OnRequested();
		GetComponent<TrackableVoxelTransform>().AddMoveListener(OnMoved);
		PlaceBlueprintsSignal.Connect(StartPlaceBlueprints);
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		Clear();
	}

	public void OnMoved()
	{
		Validate();
	}

	public void SetActive(bool b)
	{
		base.gameObject.SetActive(value: true);
	}

	[ContextMenu("Revalidate Compound")]
	private void Validate()
	{
		if (NeedsGraphRebuild)
		{
			NeedsGraphRebuild = false;
			List<GameObject> list = CollectionPool.RequestList<GameObject>();
			foreach (Transform item in base.transform)
			{
				list.Add(item.gameObject);
			}
			Graph = BlueprintCompounder.GenerateBlueprintGraph(list);
			Graph.ExpandGraph();
			list.Dispose();
		}
		Graph.Validate();
	}

	public void MoveAt(Vector3 newPosition)
	{
		base.transform.position = newPosition;
	}

	public void Rotate()
	{
		_currentRotationIndex++;
		_currentRotationIndex %= 4;
		base.transform.rotation = PreprocessedRotations.RotationForAxisOrients[(int)_possibleRotations[_currentRotationIndex]];
	}

	public void StartPlaceBlueprints()
	{
		placeBlueprints = PlaceBlueprints(Architecte.editorMode);
		StartCoroutine(placeBlueprints);
	}

	public IEnumerator PlaceBlueprints(bool instantly)
	{
		foreach (ValidationNode node2 in Graph.Nodes)
		{
			node2.Target.SetActive(value: false);
		}
		foreach (ValidationNode node in Graph.Nodes)
		{
			if (!node.IsObstructed)
			{
				Hack_GetSanitizedCompoundKeyAndRotation(node, out var key, out var rotation);
				GameObject buildProject = ((!(DestinationProject == null)) ? DestinationProject.gameObject : null);
				User.LocalUser.GetComponent<UNetBlueprint>().CallCmdSpawnPlacedBlueprint(key.Factory, key.Name, node.Target.transform.position, rotation, instantly, buildProject, spawnSelected: false);
			}
			yield return null;
		}
		DoneInstantiatingSignal.Invoke();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void Hack_GetSanitizedCompoundKeyAndRotation(ValidationNode node, out Factory.AssetKey key, out Quaternion rotation)
	{
		key = node.Type;
		rotation = node.Target.transform.rotation;
		if (_restrictedRotationKeys.TryGetValue(key, out var value))
		{
			Quaternion q = Quaternion.Euler(node.Target.transform.localEulerAngles);
			AxisOrient bestAxisOrientFromRotation = PreprocessedRotations.GetBestAxisOrientFromRotation(q);
			int num = Array.IndexOf(_possibleRotations, bestAxisOrientFromRotation);
			AxisOrient axisOrient = _possibleRotations[(_currentRotationIndex + num) % 4];
			if (axisOrient == AxisOrient.back || axisOrient == AxisOrient.left)
			{
				rotation = PreprocessedRotations.RotationForAxisOrients[(int)PreprocessedRotations.InverseAxisOrient[(int)axisOrient]];
				key = value;
			}
			else
			{
				rotation = PreprocessedRotations.RotationForAxisOrients[(int)axisOrient];
			}
		}
	}

	public void PlaceBackOriginalBlueprints()
	{
		if (DestinationProject != null)
		{
			DestinationProject.AutoDeleteDisabled = false;
		}
		MoveAt(Graph.OriginalSourcePosition);
		base.transform.rotation = Quaternion.identity;
		StartPlaceBlueprints();
	}

	public void Populate(BlueprintGraph graph, Faction f)
	{
		Graph = graph;
		Graph.SpawnBlueprintsUnder(base.transform, f);
		Graph.ExpandGraph();
		IgnoreRaycast();
	}

	public void Clear()
	{
		int num = base.transform.childCount;
		while (--num >= 0)
		{
			ObjectPoolSingleton.Release(base.transform.GetChild(num).gameObject);
		}
	}

	public void IgnoreRaycast()
	{
		for (int i = 0; i < Graph.Nodes.Count; i++)
		{
			ValidationNode validationNode = Graph.Nodes[i];
			Transform[] componentsInChildren = validationNode.Target.GetComponentsInChildren<Transform>();
			foreach (Transform transform in componentsInChildren)
			{
				transform.gameObject.layer = 2;
			}
		}
	}
}
