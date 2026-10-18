using System;
using System.Collections.Generic;
using Brix.Common.Collections;
using Brix.Engine;
using Brix.Engine.Trackers;
using Brix.Game.Components.Validators.Graphs;
using Brix.Game.Components.Volumes;
using Brix.Utils;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.Components;

public class DefaultTerrainValidator : ConcreteValidator
{
	protected override VPropertyMask SupportSettings => VPropertyMask.stoneSupport | VPropertyMask.logSupport | VPropertyMask.beamSupport;

	public override SupportState AcceptedSupportState => SupportState.CCSupport;

	public override SupportState AcceptedSolidity => SupportState.CCSolid;

	public override SupportState SupportStateType => SupportState.CCSupport;

	public override ValidationResult Validate(ref ValidationData vData, IEnumerable<ValidationData> validationVolumes = null)
	{
		if (GeostaticityTracker.IsGeostatic(vData.Position))
		{
			return ValidationResult.Valid;
		}
		return base.Validate(ref vData, validationVolumes);
	}

	protected override Dictionary<XYZ, SupportState> BuildSupportStates(ref ValidationData vData, IEnumerable<ValidationData> validationVolumes)
	{
		Dictionary<XYZ, SupportState> dictionary = CollectionPool.RequestDictionary<XYZ, SupportState>();
		foreach (XYZ @case in vData.Volume.GetCases(VProperty.mainSupportReq))
		{
			if (HasGroundSupportAt(ref vData, @case))
			{
				dictionary.Add(@case, SupportState.CCSupport | SupportState.Valid);
			}
			else
			{
				dictionary.Add(@case, VUtil.SupportStateAt(@case, SupportSettings, vData.Faction, validationVolumes));
			}
		}
		foreach (TerrainBlockData item in (from p in vData.Volume.GetCases(VProperty.auxiliarySupportReq).Slinq()
			select Voxel.GetTerrainBlockData(p) as TerrainBlockData).NotNull().NoLeak())
		{
			FreeVolume volume = item.Volume;
			foreach (XYZ case2 in volume.GetCases(VProperty.mainSupportReq))
			{
				ValidationData vData2 = default(ValidationData);
				if (HasGroundSupportAt(ref vData2, case2))
				{
					dictionary.Add(item.Position, SupportState.CCSupport | SupportState.Valid);
				}
				else
				{
					dictionary.Add(item.Position, VUtil.SupportStateAt(case2, SupportSettings, vData.Faction, validationVolumes));
				}
			}
			volume.Dispose();
			item.Dispose();
		}
		return dictionary;
	}

	protected override bool IsValid(ref ValidationData vData, Dictionary<XYZ, SupportState> supportStates)
	{
		return AllSupportedCount(supportStates, SupportState.CCSupport | SupportState.Valid, vData) >= 1;
	}

	protected override int AllSupportedCount(Dictionary<XYZ, SupportState> supportStates, SupportState currentState, ValidationData vData)
	{
		int num = 0;
		foreach (KeyValuePair<XYZ, SupportState> supportState in supportStates)
		{
			if ((supportState.Value & currentState) == currentState)
			{
				num++;
			}
		}
		return num;
	}

	public override bool IsObstructed(ref ValidationData vData)
	{
		return false;
	}

	public override SupportState GetSolidity(ref ValidationData VData, IEnumerable<ValidationData> validationVolumes)
	{
		return SupportState.CCSolid;
	}

	public override ValidationNode GenerateValidationNode(ref ValidationData vdata, BlueprintGraph graph, HashSet<GameObject> gosInGraph)
	{
		throw new InvalidOperationException();
	}

	public override ValidationNode GenerateBlueprintValidationNode(ref ValidationData vData, BlueprintGraph graph, HashSet<GameObject> gosInGraph)
	{
		throw new InvalidOperationException();
	}

	public override AbstractValidator GetBPValidatorCounterpart()
	{
		return null;
	}
}
