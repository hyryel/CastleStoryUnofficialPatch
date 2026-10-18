using System.Collections.Generic;
using Brix.Engine;
using Brix.Game.Components.Validators.Graphs;
using Brix.Game.Components.Volumes;
using Brix.Game.Rules;
using Brix.Utils;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.Components;

public abstract class BlueprintValidator : AbstractValidator
{
	protected override VPropertyMask SupportSettings => ~VPropertyMask.All;

	protected override VPropertyMask Obstructions => VPropertyMask.occupy | VPropertyMask.fantome;

	public override SupportState AcceptedSupportState => SupportState.BPSupport | SupportState.CCSupport;

	public override SupportState AcceptedSolidity => SupportState.BPSolid | SupportState.CCSolid;

	public SupportState BuildableSupportState => SupportState.CCSupport;

	public override SupportState SupportStateType => SupportState.BPSupport;

	public override SupportState GetSolidity(ref ValidationData VData, IEnumerable<ValidationData> validationVolumes)
	{
		return SupportState.BPSolid;
	}

	public override void SetUnstable(ref ValidationData vData)
	{
	}

	public override bool IsObstructed(ref ValidationData vData)
	{
		foreach (XYZ @case in vData.Volume.GetCases(VPropertyMask.occupy | VPropertyMask.fantome))
		{
			if (VUtil.IsObstructed(@case, Obstructions, vData.Faction, vData.Volume))
			{
				return true;
			}
		}
		return false;
	}

	public override ValidationResult Validate(ref ValidationData vData, IEnumerable<ValidationData> validationVolumes = null)
	{
		if (vData.Blueprint.IsAnti)
		{
			return ValidateAnti(vData);
		}
		return ValidateBlueprint(ref vData, validationVolumes);
	}

	protected virtual ValidationResult ValidateBlueprint(ref ValidationData vData, IEnumerable<ValidationData> validationVolumes)
	{
		if (IsObstructed(ref vData))
		{
			return ValidationResult.Obstructed;
		}
		return base.Validate(ref vData, validationVolumes);
	}

	protected override ValidationResult GetValidationResult(ref ValidationData vData, Dictionary<XYZ, SupportState> supportStates)
	{
		if (IsBuildable(ref vData, supportStates))
		{
			return ValidationResult.Buildable;
		}
		return base.GetValidationResult(ref vData, supportStates);
	}

	protected virtual bool IsBuildable(ref ValidationData vData, Dictionary<XYZ, SupportState> supportStates)
	{
		return AllSupportedCount(supportStates, BuildableSupportState, vData) >= supportStates.Count - vData.MaxMissingSupport && !supportStates.Values.Slinq().Any((SupportState s) => (s & SupportState.BPSupport) > (SupportState)0);
	}

	public override ValidationNode GenerateBlueprintValidationNode(ref ValidationData vData, BlueprintGraph graph, HashSet<GameObject> gosInGraph)
	{
		VolumePropertyRequest cases = vData.Volume.GetCases(VProperty.mainSupportReq);
		List<ValidationNode> list = new List<ValidationNode>();
		for (int i = 0; i < cases.Count; i++)
		{
			XYZ i2 = cases[i];
			List<ValidationData> toConsider = (from v in gosInGraph.Slinq()
				select v.GetComponent<ValidatorComponent>().VData).ToList();
			List<ValidationData> supports = VUtil.GetSupports(i2, SupportSettings, toConsider);
			foreach (ValidationData item in supports)
			{
				list.Add(item.GenerateBlueprintValidationNode(graph, gosInGraph));
			}
			supports.Dispose();
		}
		int validNodesNeeded = cases.Count - vData.MaxMissingSupport;
		ValidationNode validationNode = new BlueprintValidationNode(vData.Volume.GameObject, list, validNodesNeeded);
		graph.AddNode(validationNode);
		return validationNode;
	}

	public override ValidationNode GenerateValidationNode(ref ValidationData vData, BlueprintGraph graph, HashSet<GameObject> gosInGraph)
	{
		return GenerateBlueprintValidationNode(ref vData, graph, gosInGraph);
	}

	private ValidationResult ValidateAnti(ValidationData VData)
	{
		VolumePropertyRequest cases = VData.Volume.GetCases(VProperty.fantome);
		bool flag = VolumeRegistry.HasVolumesWithPropertyAt(cases, VProperty.occupy);
		bool flag2 = false;
		bool flag3 = true;
		for (int i = 0; i < cases.Count; i++)
		{
			XYZ position = cases[i];
			if (!Voxel.GetVoxel(position).IsNull)
			{
				flag2 = true;
				if (!TerrainRules.Main.CanDigAt(position.ToVector3()))
				{
					flag3 = false;
				}
			}
		}
		if (!flag3 || (!flag2 && !flag))
		{
			return ValidationResult.Invalid;
		}
		return ValidationResult.Valid;
	}

	public abstract AbstractValidator GetBlockValidatorCounterpart();
}
