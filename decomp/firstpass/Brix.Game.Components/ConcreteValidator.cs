using System;
using System.Collections.Generic;
using Brix.Game.Components.Validators.Graphs;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Components;

public abstract class ConcreteValidator : AbstractValidator
{
	public override SupportState AcceptedSupportState => SupportState.CCSupport;

	public override SupportState AcceptedSolidity => SupportState.CCSolid;

	public override SupportState SupportStateType => SupportState.CCSupport;

	public override ValidationResult Validate(ref ValidationData vData, IEnumerable<ValidationData> validationVolumes = null)
	{
		if (!NetworkServer.active)
		{
			return ValidationResult.Valid;
		}
		if ((bool)vData.Target && (bool)vData.Target.GetComponent<Rigidbody>())
		{
			return ValidationResult.Valid;
		}
		return base.Validate(ref vData, validationVolumes);
	}

	public override SupportState GetSolidity(ref ValidationData vData, IEnumerable<ValidationData> validationVolumes)
	{
		return SupportState.CCSolid;
	}

	public override void SetUnstable(ref ValidationData vData)
	{
		vData.Volume.Unregister();
		PropagateRevalidation(ref vData);
	}

	public override ValidationNode GenerateValidationNode(ref ValidationData vdata, BlueprintGraph graph, HashSet<GameObject> gosInGraph)
	{
		throw new NotImplementedException();
	}

	public override ValidationNode GenerateBlueprintValidationNode(ref ValidationData vData, BlueprintGraph graph, HashSet<GameObject> gosInGraph)
	{
		return GetBPValidatorCounterpart().GenerateBlueprintValidationNode(ref vData, graph, gosInGraph);
	}

	public override bool IsObstructed(ref ValidationData vData)
	{
		return false;
	}

	public abstract AbstractValidator GetBPValidatorCounterpart();
}
