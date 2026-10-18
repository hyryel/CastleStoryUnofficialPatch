using System;
using System.Collections.Generic;
using Brix.Game.Components.Validators.Graphs;
using Brix.Game.Components.Volumes;
using UnityEngine;

namespace Brix.Game.Components.Validators;

public class NullValidator : AbstractValidator
{
	public override SupportState AcceptedSolidity
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public override SupportState AcceptedSupportState
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public override SupportState SupportStateType
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	protected override VPropertyMask SupportSettings
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public override ValidationResult Validate(ref ValidationData vData, IEnumerable<ValidationData> validationVolumes = null)
	{
		return ValidationResult.Buildable;
	}

	public override ValidationNode GenerateBlueprintValidationNode(ref ValidationData vData, BlueprintGraph graph, HashSet<GameObject> gosInGraph)
	{
		throw new NotImplementedException();
	}

	public override ValidationNode GenerateValidationNode(ref ValidationData vData, BlueprintGraph graph, HashSet<GameObject> gosInGraph)
	{
		throw new NotImplementedException();
	}

	public override SupportState GetSolidity(ref ValidationData vData, IEnumerable<ValidationData> validationVolumes)
	{
		throw new NotImplementedException();
	}

	public override bool IsObstructed(ref ValidationData vData)
	{
		throw new NotImplementedException();
	}

	public override void SetUnstable(ref ValidationData vData)
	{
		throw new NotImplementedException();
	}
}
