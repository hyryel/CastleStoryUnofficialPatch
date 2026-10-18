using System.Collections.Generic;
using Brix.Engine;
using Brix.Game.Components.Validators.Graphs;
using Brix.Game.Components.Volumes;
using Brix.Utils;
using Brix.Utils.Collections;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.Components;

internal class PlankBPValidator : BlueprintValidator
{
	public override bool IsObstructed(ref ValidationData vData)
	{
		List<IVolume> list = VolumeRegistry.VolumesWithPropertyAt(vData.Volume.GetCases(VPropertyMask.occupy | VPropertyMask.fantome), VProperty.beamSupport).Slinq().ToPooledList();
		list.Add(vData.Volume);
		bool result = false;
		foreach (XYZ @case in vData.Volume.GetCases(VPropertyMask.occupy | VPropertyMask.fantome))
		{
			if (VUtil.IsObstructed(@case, Obstructions, vData.Faction, list))
			{
				result = true;
				break;
			}
		}
		list.Dispose();
		return result;
	}

	protected override Dictionary<XYZ, SupportState> BuildSupportStates(ref ValidationData vData, IEnumerable<ValidationData> validationVolumes)
	{
		return BasePlankValidation.BuildSupportStates(vData, validationVolumes);
	}

	protected override bool IsBuildable(ref ValidationData vData, Dictionary<XYZ, SupportState> supportStates)
	{
		if (AllSupportedCount(supportStates, SupportState.CCSupport | SupportState.NearSupport | SupportState.Valid | SupportState.ConcreteEnd, vData) >= 1)
		{
			return true;
		}
		if (BaseRecursiveValidation.IsFarSupportedCC(supportStates))
		{
			return true;
		}
		return false;
	}

	protected override bool IsValid(ref ValidationData vData, Dictionary<XYZ, SupportState> supportStates)
	{
		if (AllSupportedCount(supportStates, SupportState.BPSupport | SupportState.NearSupport | SupportState.Valid, vData) >= 1)
		{
			return true;
		}
		if (BaseRecursiveValidation.IsFarSupportedBP(supportStates))
		{
			return true;
		}
		return false;
	}

	protected override bool HasGroundSupportAt(ref ValidationData vData, XYZ i)
	{
		return false;
	}

	public override SupportState GetSolidity(ref ValidationData VData, IEnumerable<ValidationData> validationVolumes)
	{
		return SupportState.Unsupported;
	}

	public override ValidationNode GenerateBlueprintValidationNode(ref ValidationData vData, BlueprintGraph graph, HashSet<GameObject> gosInGraph)
	{
		PlankValidationNode plankValidationNode = new PlankValidationNode(vData.Volume.GameObject, GetBeamParentNode(ref vData, graph, gosInGraph));
		graph.AddNode(plankValidationNode);
		List<IVolume> orderedSupportVolumes = BasePlankValidation.GetOrderedSupportVolumes(ref vData);
		if (orderedSupportVolumes.Count > 0)
		{
			plankValidationNode.StrongNode = orderedSupportVolumes[0].VData.GenerateBlueprintValidationNode(graph, gosInGraph);
			if (orderedSupportVolumes.Count > 1)
			{
				plankValidationNode.WeakNode = orderedSupportVolumes[1].VData.GenerateBlueprintValidationNode(graph, gosInGraph);
			}
		}
		return plankValidationNode;
	}

	private static BeamValidationNode GetBeamParentNode(ref ValidationData vData, BlueprintGraph graph, HashSet<GameObject> gosInGraph)
	{
		BeamValidationNode result = null;
		VolumePropertyRequest cases = vData.Volume.GetCases(VProperty.auxiliarySupportReq);
		if (cases.Count > 0)
		{
			XYZ i = cases.Slinq().First();
			ValidationData validationData = VUtil.GetSupports(i, VProperty.beamSupport, vData.Faction).Slinq().FirstOrDefault();
			if (validationData.IsNotDefault && gosInGraph.Contains(validationData.Target))
			{
				result = validationData.GenerateBlueprintValidationNode(graph, gosInGraph) as BeamValidationNode;
			}
		}
		return result;
	}

	public override AbstractValidator GetBlockValidatorCounterpart()
	{
		return SingletonSpawner.Get<PlankBlockValidator>();
	}
}
