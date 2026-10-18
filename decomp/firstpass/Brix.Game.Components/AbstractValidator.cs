using System;
using System.Collections.Generic;
using Brix.Engine;
using Brix.Game.Components.Validators.Graphs;
using Brix.Game.Components.Volumes;
using Brix.Utils;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.Components;

public abstract class AbstractValidator : IValidator
{
	public Type GetValidatorType => GetType();

	protected abstract VPropertyMask SupportSettings { get; }

	protected virtual VPropertyMask Obstructions => VPropertyMask.occupy;

	public abstract SupportState SupportStateType { get; }

	public abstract SupportState AcceptedSupportState { get; }

	public abstract SupportState AcceptedSolidity { get; }

	public virtual ValidationResult Validate(ref ValidationData vData, IEnumerable<ValidationData> validationVolumes = null)
	{
		Dictionary<XYZ, SupportState> dictionary = BuildSupportStates(ref vData, validationVolumes);
		ValidationResult validationResult = GetValidationResult(ref vData, dictionary);
		dictionary.Dispose();
		return validationResult;
	}

	public ValidationResult GetCurrentState(ref ValidationData vData, IEnumerable<ValidationData> validationVolumes)
	{
		return Validate(ref vData, validationVolumes);
	}

	protected virtual ValidationResult GetValidationResult(ref ValidationData vData, Dictionary<XYZ, SupportState> supportStates)
	{
		return (!IsValid(ref vData, supportStates)) ? ValidationResult.Invalid : ValidationResult.Valid;
	}

	protected virtual bool IsValid(ref ValidationData vData, Dictionary<XYZ, SupportState> supportStates)
	{
		return AnySupportedCount(supportStates, AcceptedSupportState, vData) >= supportStates.Count - vData.MaxMissingSupport;
	}

	public abstract SupportState GetSolidity(ref ValidationData vData, IEnumerable<ValidationData> validationVolumes);

	public virtual bool IsEnoughSupport(SupportState state)
	{
		return (state & AcceptedSupportState) != 0;
	}

	public virtual bool IsEnoughSolidity(SupportState solidity)
	{
		return (solidity & AcceptedSolidity) != 0;
	}

	public virtual void PropagateRevalidation(ref ValidationData vData)
	{
		List<IVolume> list = VUtil.ImpactedVolumes(vData.Volume);
		list.Slinq().ForEach(delegate(IVolume v)
		{
			v.Validate();
		});
		list.Dispose();
	}

	protected virtual bool HasGroundSupportAt(ref ValidationData vData, XYZ pos)
	{
		using IVoxel voxel = Voxel.GetTerrainBlockData(pos);
		return !voxel.IsNull && !voxel.IsCrust;
	}

	protected virtual int AllSupportedCount(Dictionary<XYZ, SupportState> supportStates, SupportState currentState, ValidationData vData)
	{
		int num = 0;
		foreach (KeyValuePair<XYZ, SupportState> supportState in supportStates)
		{
			if (((supportState.Value & currentState) == currentState && supportState.Value.HasFlag(SupportState.Valid)) || HasGroundSupportAt(ref vData, supportState.Key))
			{
				num++;
			}
		}
		return num;
	}

	protected virtual int AnySupportedCount(Dictionary<XYZ, SupportState> supportStates, SupportState currentState, ValidationData vData)
	{
		int num = 0;
		foreach (KeyValuePair<XYZ, SupportState> supportState in supportStates)
		{
			if (((supportState.Value & currentState) != 0 && supportState.Value.HasFlag(SupportState.Valid)) || HasGroundSupportAt(ref vData, supportState.Key))
			{
				num++;
			}
		}
		return num;
	}

	public abstract ValidationNode GenerateValidationNode(ref ValidationData vData, BlueprintGraph graph, HashSet<GameObject> gosInGraph);

	public abstract ValidationNode GenerateBlueprintValidationNode(ref ValidationData vData, BlueprintGraph graph, HashSet<GameObject> gosInGraph);

	public abstract bool IsObstructed(ref ValidationData vData);

	public abstract void SetUnstable(ref ValidationData vData);

	protected virtual Dictionary<XYZ, SupportState> BuildSupportStates(ref ValidationData vData, IEnumerable<ValidationData> validationVolumes)
	{
		Dictionary<XYZ, SupportState> dictionary = CollectionPool.RequestDictionary<XYZ, SupportState>();
		foreach (XYZ @case in vData.Volume.GetCases(VProperty.mainSupportReq))
		{
			dictionary.Add(@case, VUtil.SupportStateAt(@case, SupportSettings, vData.Faction, validationVolumes));
		}
		return dictionary;
	}
}
