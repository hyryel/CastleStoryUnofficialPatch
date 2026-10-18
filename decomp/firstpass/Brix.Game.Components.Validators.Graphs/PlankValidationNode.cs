using System;
using System.Collections.Generic;
using Brix.External.Factories;
using Newtonsoft.Json;
using UnityEngine;

namespace Brix.Game.Components.Validators.Graphs;

public class PlankValidationNode : ValidationNode
{
	[JsonProperty]
	public ValidationNode _weakNode;

	public ValidationNode _strongNode;

	private ValidationNode _beamValidationNode;

	private GameObject _target;

	private Guid _guid;

	private bool _isPropagated;

	private GraphSupportState _currentSupportState;

	private Blueprint _blueprint;

	public ValidationNode WeakNode
	{
		get
		{
			return _weakNode ?? NullValidationNode.Instance;
		}
		set
		{
			_weakNode = value;
			_weakNode.AddChildNode(this);
		}
	}

	public ValidationNode StrongNode
	{
		get
		{
			return _strongNode ?? NullValidationNode.Instance;
		}
		set
		{
			_strongNode = value;
			_strongNode.AddChildNode(this);
		}
	}

	public override bool IsObstructed { get; protected set; }

	private bool IsCurrentlyValid => _currentSupportState > GraphSupportState.Unsupported && !IsObstructed;

	public override bool IsRootNode => false;

	public override List<ValidationNode> ChildNodes => null;

	public override List<ValidationNode> ParentNodes => null;

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
	protected PlankValidationNode()
	{
		_beamValidationNode = NullValidationNode.Instance;
	}

	public PlankValidationNode(GameObject go, BeamValidationNode beamNode)
		: base(go)
	{
		Target = go;
		_isPropagated = false;
		_beamValidationNode = (ValidationNode)(((object)beamNode) ?? ((object)NullValidationNode.Instance));
		_currentSupportState = GraphSupportState.Unsupported;
		beamNode?.AddChildNode(this);
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
	}

	public override void UpdateNode(Guid newGuid)
	{
		Validate(newGuid);
		PropagateToChildNodes();
	}

	public override bool IsValid(Guid guid)
	{
		Validate(guid);
		return IsCurrentlyValid;
	}

	public override GraphSupportState GetSupportState(Guid guid)
	{
		Validate(guid);
		return (!IsObstructed) ? _currentSupportState : GraphSupportState.Unsupported;
	}

	public override void PropagateAddToParents()
	{
		WeakNode.AddToParentsThenPropagate(this);
	}

	public override void AddToParentsThenPropagate(ValidationNode node)
	{
		if (node.Type == Blueprints.Wood_Brace)
		{
			_beamValidationNode = node;
			return;
		}
		bool flag = _weakNode == node;
		if (_strongNode == null && !flag)
		{
			_strongNode = node;
			if (_weakNode != null)
			{
				_weakNode.AddToParentsThenPropagate(this);
			}
		}
	}

	public void AddBeamParent(BeamValidationNode node)
	{
		_beamValidationNode = node;
	}

	private void Validate(Guid newGuid)
	{
		if (newGuid != _guid)
		{
			_guid = newGuid;
			_isPropagated = false;
			ValidateBeamNode();
		}
	}

	private void ValidateBeamNode()
	{
		if (_beamValidationNode.GetSupportState(_guid) > GraphSupportState.Unsupported)
		{
			_currentSupportState = GraphSupportState.Near;
			return;
		}
		GraphSupportState currentSupportState = _currentSupportState;
		_currentSupportState = GraphSupportState.Unsupported;
		GraphSupportState supportState = StrongNode.GetSupportState(_guid);
		GraphSupportState newSupportState;
		switch (supportState)
		{
		case GraphSupportState.Unsupported:
			newSupportState = InferStateFromSingleBeam(WeakNode.GetSupportState(_guid));
			break;
		case GraphSupportState.Mutual:
		{
			newSupportState = (_currentSupportState = GraphSupportState.Unsupported);
			GraphSupportState supportState2 = WeakNode.GetSupportState(_guid);
			if (supportState2 > GraphSupportState.Far)
			{
				newSupportState = InferStateFromSingleBeam(WeakNode.GetSupportState(_guid));
			}
			else if (supportState2 == GraphSupportState.Far)
			{
				newSupportState = GraphSupportState.Mutual;
			}
			break;
		}
		case GraphSupportState.Far:
			newSupportState = (_currentSupportState = GraphSupportState.Mutual);
			if (WeakNode.GetSupportState(_guid) < GraphSupportState.Mutual)
			{
				newSupportState = GraphSupportState.Unsupported;
			}
			break;
		default:
			newSupportState = InferStateFromSingleBeam(supportState);
			break;
		}
		_currentSupportState = currentSupportState;
		SetValidity(newSupportState, _validator.IsObstructed());
	}

	private GraphSupportState InferStateFromSingleBeam(GraphSupportState secondaryParentState)
	{
		return secondaryParentState switch
		{
			GraphSupportState.Near => GraphSupportState.Far, 
			GraphSupportState.FullSupport => GraphSupportState.Near, 
			_ => GraphSupportState.Unsupported, 
		};
	}

	private void PropagateToChildNodes()
	{
		if (!_isPropagated)
		{
			_isPropagated = true;
			WeakNode.UpdateNode(_guid);
			if (StrongNode is PlankValidationNode)
			{
				StrongNode.UpdateNode(_guid);
			}
		}
	}

	private void SetValidity(GraphSupportState newSupportState, bool isObstructed)
	{
		bool flag = newSupportState > GraphSupportState.Unsupported && !isObstructed;
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
		_currentSupportState = newSupportState;
		IsObstructed = isObstructed;
	}
}
