using System;
using System.Collections.Generic;
using Brix.Utils;
using UnityEngine;

namespace Brix.Game.Components.Validators.Graphs;

public class NullValidationNode : ValidationNode
{
	private static NullValidationNode _instance = new NullValidationNode();

	public static NullValidationNode Instance => _instance;

	public override bool IsRootNode => false;

	public override GameObject Target
	{
		get
		{
			return null;
		}
		protected set
		{
		}
	}

	public override List<ValidationNode> ChildNodes => Empty.List<ValidationNode>();

	public override List<ValidationNode> ParentNodes => Empty.List<ValidationNode>();

	public override bool IsObstructed
	{
		get
		{
			return false;
		}
		protected set
		{
		}
	}

	public override void UpdateNode(Guid newGuid)
	{
	}

	public override bool IsValid(Guid guid)
	{
		return false;
	}

	public override void SetFaction(Faction faction)
	{
	}

	public override void AddChildNode(ValidationNode node)
	{
	}

	public override void PropagateAddToParents()
	{
	}

	public override void AddToParentsThenPropagate(ValidationNode node)
	{
	}

	public override GraphSupportState GetSupportState(Guid guid)
	{
		return GraphSupportState.Unsupported;
	}
}
