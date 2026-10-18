using System.Collections.Generic;
using Brix.Engine;
using Brix.Game.Components.Validators.Graphs;
using Brix.Game.Components.Volumes;
using Brix.Utils;
using Brix.Utils.Collections;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.Components;

internal class BeamBPValidator : BlueprintValidator
{
	protected override Dictionary<XYZ, SupportState> BuildSupportStates(ref ValidationData vData, IEnumerable<ValidationData> validationVolumes)
	{
		return BaseBeamValidation.BuildSupportStates(ref vData, validationVolumes);
	}

	protected override bool HasGroundSupportAt(ref ValidationData vData, XYZ i)
	{
		return false;
	}

	protected override bool IsBuildable(ref ValidationData vData, Dictionary<XYZ, SupportState> supportStates)
	{
		return AllSupportedCount(supportStates, SupportState.CCSupport | SupportState.NearSupport | SupportState.Valid | SupportState.ConcreteEnd, vData) >= 1 || BaseRecursiveValidation.IsFarSupportedCC(supportStates);
	}

	protected override bool IsValid(ref ValidationData vData, Dictionary<XYZ, SupportState> supportStates)
	{
		return AllSupportedCount(supportStates, SupportState.BPSupport | SupportState.NearSupport | SupportState.Valid, vData) >= 1 || AllSupportedCount(supportStates, SupportState.CCSupport | SupportState.NearSupport | SupportState.Valid, vData) >= 1 || BaseRecursiveValidation.IsFarSupportedBP(supportStates);
	}

	public override bool IsObstructed(ref ValidationData vData)
	{
		VolumeMultiPropertyRequest cases = vData.Volume.GetCases(VPropertyMask.occupy | VPropertyMask.fantome);
		List<IVolume> list = VolumeRegistry.VolumesWithPropertyAt(cases, VProperty.plankSupport).Slinq().ToPooledList();
		list.Add(vData.Volume);
		bool result = false;
		foreach (XYZ @case in vData.Volume.GetCases(VPropertyMask.occupy | VPropertyMask.fantome))
		{
			if (VUtil.IsObstructed(@case, Obstructions, vData.Faction, list))
			{
				result = true;
			}
		}
		list.Dispose();
		return result;
	}

	public override SupportState GetSolidity(ref ValidationData vData, IEnumerable<ValidationData> validationVolumes)
	{
		return BaseBeamValidation.GetSolidity(vData, validationVolumes);
	}

	public override void PropagateRevalidation(ref ValidationData vData)
	{
		BaseBeamValidation.PropagateRevalidation(vData);
	}

	public override ValidationNode GenerateBlueprintValidationNode(ref ValidationData vData, BlueprintGraph graph, HashSet<GameObject> gosInGraph)
	{
		BeamValidationNode beamValidationNode = new BeamValidationNode(vData.Volume.GameObject, GetBraceParentNode(ref vData, graph, gosInGraph));
		graph.AddNode(beamValidationNode);
		List<IVolume> orderedSupportVolumes = BaseBeamValidation.GetOrderedSupportVolumes(ref vData);
		if (orderedSupportVolumes.Count > 0)
		{
			beamValidationNode.StrongNode = orderedSupportVolumes[0].VData.GenerateBlueprintValidationNode(graph, gosInGraph);
			if (orderedSupportVolumes.Count > 1)
			{
				beamValidationNode.WeakNode = orderedSupportVolumes[1].VData.GenerateBlueprintValidationNode(graph, gosInGraph);
			}
		}
		return beamValidationNode;
	}

	private static ValidationNode GetBraceParentNode(ref ValidationData vData, BlueprintGraph graph, HashSet<GameObject> gosInGraph)
	{
		ValidationNode result = null;
		VolumePropertyRequest cases = vData.Volume.GetCases(VProperty.auxiliarySupportReq);
		for (int i = 0; i < cases.Count; i++)
		{
			List<ValidationData> supports = VUtil.GetSupports(cases[i], VProperty.braceSupport, vData.Faction);
			ValidationData validationData = supports.Slinq().Where((ValidationData brace, ValidationData beam) => brace.Orientation.IsAlignedTo(beam.Orientation), vData).FirstOrDefault();
			if (validationData.IsNotDefault && gosInGraph.Contains(validationData.Target))
			{
				result = validationData.GenerateBlueprintValidationNode(graph, gosInGraph);
			}
			supports.Dispose();
		}
		return result;
	}

	public override AbstractValidator GetBlockValidatorCounterpart()
	{
		return SingletonSpawner.Get<BeamBlockValidator>();
	}
}
