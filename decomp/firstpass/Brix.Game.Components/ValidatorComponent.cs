using System;
using System.Collections.Generic;
using Brix.Core;
using Brix.Game.Components.Validators.Graphs;
using Brix.Lifecycle.Pooling;
using UnityEngine;

namespace Brix.Game.Components;

public class ValidatorComponent : GameComponent, IValidator
{
	private AbstractValidator _validator;

	[TypeReference.Config(typeof(IValidator), excludedTypes = new Type[] { typeof(ValidatorComponent) })]
	public TypeReference validatorType;

	public int MaxMissingSupport = 1;

	public bool CanPlaceInvalid = true;

	private Guid _currentVID;

	private ValidationResult _validateCache;

	public AbstractValidator Validator
	{
		get
		{
			if (_validator == null)
			{
				_validator = (AbstractValidator)SingletonSpawner.Get(validatorType);
			}
			return _validator;
		}
	}

	public Type GetValidatorType => Validator.GetType();

	public ValidationData VData => new ValidationData(this);

	public ValidationResult ValidateCache => _validateCache;

	public bool IsBlueprint => Blueprint != null;

	public SupportState SupportStateType => Validator.SupportStateType;

	public void ResetValidatorCache()
	{
		_validator = null;
	}

	public override void Initialize()
	{
		base.Initialize();
		if (IsBlueprint)
		{
			Blueprint.OnStateChangedSignal.Connect(OnStateChange);
		}
	}

	[ContextMenu("Force Revalidation")]
	public void ForceValidation()
	{
		VUtil.NewVID();
		Validate();
	}

	public ValidationResult Validate(IEnumerable<ValidationData> validationVolumes = null)
	{
		ValidationData vData = VData;
		return Validate(ref vData, validationVolumes);
	}

	public ValidationResult Validate(ref ValidationData vData, IEnumerable<ValidationData> validationVolumes = null)
	{
		_currentVID = VUtil.CurrentVID;
		_validateCache = Validator.Validate(ref vData, validationVolumes);
		if (IsBlueprint)
		{
			UpdateBlueprintState();
		}
		else if (_validateCache == ValidationResult.Invalid)
		{
			SetUnstable(ref vData);
		}
		return ValidateCache;
	}

	public bool IsEnoughSupport(SupportState state)
	{
		return Validator.IsEnoughSupport(state);
	}

	public bool IsEnoughSolidity(SupportState solidity)
	{
		return Validator.IsEnoughSolidity(solidity);
	}

	private void UpdateBlueprintState()
	{
		if (Blueprint.IsAnti)
		{
			if (_validateCache == ValidationResult.Invalid)
			{
				Blueprint.State &= ~PlacementFlags.buildable;
				ObjectPoolSingleton.Release(base.gameObject);
			}
			else
			{
				Blueprint.State |= PlacementFlags.valid | PlacementFlags.buildable;
			}
		}
		else if (_validateCache == ValidationResult.Buildable)
		{
			SetBuildable();
		}
		else if (_validateCache == ValidationResult.Valid)
		{
			SetValid();
		}
		else
		{
			SetInvalid();
		}
	}

	public ValidationResult GetCurrentState(IEnumerable<ValidationData> validationVolumes = null)
	{
		ValidationData vData = VData;
		return GetCurrentState(ref vData, validationVolumes);
	}

	public ValidationResult GetCurrentState(ref ValidationData vData, IEnumerable<ValidationData> validationVolumes = null)
	{
		if (_currentVID == VUtil.CurrentVID)
		{
			return ValidateCache;
		}
		_currentVID = VUtil.CurrentVID;
		_validateCache = Validator.Validate(ref vData, validationVolumes);
		return ValidateCache;
	}

	protected void SetBuildable()
	{
		Blueprint.State |= PlacementFlags.valid | PlacementFlags.buildable;
	}

	protected void SetValid()
	{
		Blueprint.State = (Blueprint.State & ~PlacementFlags.buildable) | PlacementFlags.valid;
	}

	protected void SetInvalid()
	{
		Blueprint.State &= ~(PlacementFlags.valid | PlacementFlags.buildable);
	}

	protected void OnStateChange(Blueprint blueprint, PlacementFlags beforeFlags, PlacementFlags afterFlags)
	{
		if (afterFlags.HasFlag(PlacementFlags.placed) && ((afterFlags ^ beforeFlags) & (PlacementFlags.valid | PlacementFlags.placed)) != PlacementFlags.none)
		{
			ValidationData vData = VData;
			Validator.PropagateRevalidation(ref vData);
		}
	}

	public void AttachBlockValidatorCounterpart(GameObject go)
	{
		AbstractValidator blockValidatorCounterpart = (Validator as BlueprintValidator).GetBlockValidatorCounterpart();
		if (blockValidatorCounterpart != null)
		{
			ValidatorComponent validatorComponent = go.AddComponent<ValidatorComponent>();
			validatorComponent._validator = blockValidatorCounterpart;
			validatorComponent.validatorType = new TypeReference(blockValidatorCounterpart.GetType());
			validatorComponent.MaxMissingSupport = MaxMissingSupport;
		}
	}

	public void SetUnstable(ref ValidationData vData)
	{
		_validateCache = ValidationResult.Invalid;
		_currentVID = VUtil.NewVID();
		Validator.SetUnstable(ref vData);
	}

	public SupportState GetSolidity(IEnumerable<ValidationData> validationVolumes = null)
	{
		ValidationData vData = VData;
		return GetSolidity(ref vData, validationVolumes);
	}

	public SupportState GetSolidity(ref ValidationData vData, IEnumerable<ValidationData> validationVolumes = null)
	{
		return Validator.GetSolidity(ref vData, validationVolumes);
	}

	public ValidationNode AddBlueprintValidationNodeToGraph(BlueprintGraph graph, HashSet<GameObject> gosInGraph)
	{
		ValidationData vData = VData;
		return GenerateBlueprintValidationNode(ref vData, graph, gosInGraph);
	}

	public ValidationNode GenerateBlueprintValidationNode(ref ValidationData vData, BlueprintGraph graph, HashSet<GameObject> gosInGraph)
	{
		if (!graph.HasNode(base.gameObject))
		{
			return Validator.GenerateBlueprintValidationNode(ref vData, graph, gosInGraph);
		}
		return graph.GetNode(base.gameObject);
	}

	public bool IsObstructed()
	{
		return IsObstructed(VData);
	}

	public bool IsObstructed(ValidationData vData)
	{
		return Validator.IsObstructed(ref vData);
	}
}
