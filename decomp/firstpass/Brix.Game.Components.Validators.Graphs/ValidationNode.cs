using System;
using System.Collections.Generic;
using Brix.Components;
using Brix.External.Factories;
using Brix.Game.Utils;
using Brix.Lifecycle.Pooling;
using Newtonsoft.Json;
using UnityEngine;

namespace Brix.Game.Components.Validators.Graphs;

[JsonObject(MemberSerialization.OptIn)]
public abstract class ValidationNode
{
	[JsonProperty]
	public Factory.AssetKey Type;

	[JsonProperty]
	public Vector3 Pos;

	[JsonProperty]
	public Quaternion Rot;

	protected ValidatorComponent _validator;

	public abstract bool IsRootNode { get; }

	public abstract GameObject Target { get; protected set; }

	public abstract List<ValidationNode> ChildNodes { get; }

	public abstract List<ValidationNode> ParentNodes { get; }

	public abstract bool IsObstructed { get; protected set; }

	[JsonConstructor]
	protected ValidationNode()
	{
	}

	protected ValidationNode(GameObject go)
	{
		Type = BrixSingleton<BlueprintMapper>.Instance.GetMatchingBlueprintAssetKey(go.GetComponent<FactoryImprint>().AssetKey);
		Pos = go.transform.position;
		Rot = go.transform.rotation;
	}

	public abstract void UpdateNode(Guid newGuid);

	public abstract bool IsValid(Guid guid);

	public void SpawnUnder(Transform parent)
	{
		ObjectPoolSingleton.Request(out var go, Type, Pos, Rot);
		go.transform.SetParent(parent);
		Target = go;
	}

	public abstract void SetFaction(Faction faction);

	public abstract void AddChildNode(ValidationNode node);

	public abstract void PropagateAddToParents();

	public abstract void AddToParentsThenPropagate(ValidationNode node);

	public abstract GraphSupportState GetSupportState(Guid guid);
}
