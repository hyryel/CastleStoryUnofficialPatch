using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.Components.Validators.Graphs;

public class BlueprintValidationNode : ValidationNode
{
	[JsonProperty]
	private int _validNodesNeeded;

	[JsonProperty]
	private List<ValidationNode> _childNodes = new List<ValidationNode>();

	private List<ValidationNode> _parentNodes = new List<ValidationNode>();

	private GameObject _target;

	private Guid _guid;

	private bool _isPropagated;

	private bool _isSupported;

	private Blueprint _blueprint;

	public override bool IsObstructed { get; protected set; }

	private bool IsCurrentlyValid => _isSupported && !IsObstructed;

	public override bool IsRootNode => _parentNodes.Count == 0;

	public override List<ValidationNode> ChildNodes => _childNodes;

	public override List<ValidationNode> ParentNodes => _parentNodes;

	public override GameObject Target
	{
		get
		{
			return _target;
		}
		protected set
		{
			_target = value;
			_validator = _target.GetComponent<ValidatorComponent>();
			_blueprint = value.GetComponent<Blueprint>();
		}
	}

	[JsonConstructor]
	protected BlueprintValidationNode()
	{
	}

	public BlueprintValidationNode(GameObject go, List<ValidationNode> parentNodes, int validNodesNeeded)
		: base(go)
	{
		Target = go;
		_isPropagated = false;
		_parentNodes = parentNodes;
		_validNodesNeeded = validNodesNeeded;
		parentNodes.Slinq().ForEach(delegate(ValidationNode parent, BlueprintValidationNode self)
		{
			parent.AddChildNode(self);
		}, this);
	}

	public override void SetFaction(Faction faction)
	{
		if (_target != null)
		{
			_target.GetComponent<AffiliationComponent>().faction = faction;
		}
	}

	public override void AddChildNode(ValidationNode node)
	{
		_childNodes.Add(node);
	}

	public override void UpdateNode(Guid newGuid)
	{
		Validate(newGuid);
		PropagateToChildNodes(newGuid);
	}

	public override bool IsValid(Guid guid)
	{
		Validate(guid);
		return IsCurrentlyValid;
	}

	public override void PropagateAddToParents()
	{
		_childNodes.Slinq().ForEach(delegate(ValidationNode child, BlueprintValidationNode self)
		{
			child.AddToParentsThenPropagate(self);
		}, this);
	}

	public override void AddToParentsThenPropagate(ValidationNode node)
	{
		int count = _parentNodes.Count;
		_parentNodes.Add(node);
		if (count == 0)
		{
			_childNodes.Slinq().ForEach(delegate(ValidationNode child, BlueprintValidationNode self)
			{
				child.AddToParentsThenPropagate(self);
			}, this);
		}
	}

	public override GraphSupportState GetSupportState(Guid guid)
	{
		if (guid != _guid)
		{
			Validate(guid);
		}
		if (_isSupported)
		{
			return GraphSupportState.FullSupport;
		}
		return GraphSupportState.Unsupported;
	}

	private void Validate(Guid newGuid)
	{
		if (newGuid != _guid)
		{
			_guid = newGuid;
			_isPropagated = false;
			if (IsRootNode)
			{
				ValidateRootNode();
			}
			else
			{
				ValidateRegularNode();
			}
		}
	}

	private void ValidateRegularNode()
	{
		int num = _validNodesNeeded;
		bool flag = true;
		bool flag2 = num <= ParentNodes.Count;
		int num2 = 0;
		while (flag2)
		{
			ValidationNode validationNode = _parentNodes[num2];
			if (validationNode.IsValid(_guid))
			{
				flag &= validationNode.IsObstructed;
				if (--num == 0)
				{
					break;
				}
			}
			flag2 = num <= _parentNodes.Count - (num2 + 1);
			num2++;
		}
		if (flag && num > 0)
		{
			ValidateRootNode();
		}
		else
		{
			SetValidity(flag2, _validator.IsObstructed());
		}
	}

	private void ValidateRootNode()
	{
		SetValidity(_validator.Validate());
	}

	private void PropagateToChildNodes(Guid newGuid)
	{
		if (!_isPropagated)
		{
			_childNodes.Slinq().ForEach(delegate(ValidationNode node, Guid guid)
			{
				node.UpdateNode(guid);
			}, newGuid);
			_isPropagated = true;
		}
	}

	private void SetValidity(ValidationResult vResult)
	{
		SetValidity(vResult >= ValidationResult.Valid, vResult == ValidationResult.Obstructed);
	}

	private void SetValidity(bool isSupported, bool isObstructed)
	{
		bool flag = isSupported && !isObstructed;
		if (IsCurrentlyValid != flag)
		{
			if (flag)
			{
				_blueprint.State |= PlacementFlags.valid;
			}
			else
			{
				_blueprint.State &= ~PlacementFlags.valid;
			}
		}
		_isSupported = isSupported;
		IsObstructed = isObstructed;
	}
}
