using System;
using System.Collections.Generic;
using Brix.Game.Components.Validators.Graphs;
using UnityEngine;

namespace Brix.Game.Components;

public interface IValidator
{
	SupportState SupportStateType { get; }

	Type GetValidatorType { get; }

	ValidationResult Validate(ref ValidationData vData, IEnumerable<ValidationData> validationVolumes = null);

	bool IsEnoughSupport(SupportState state);

	bool IsEnoughSolidity(SupportState solidity);

	SupportState GetSolidity(ref ValidationData validationData, IEnumerable<ValidationData> validationVolumes);

	ValidationNode GenerateBlueprintValidationNode(ref ValidationData vData, BlueprintGraph graph, HashSet<GameObject> gosInGraph);

	ValidationResult GetCurrentState(ref ValidationData vData, IEnumerable<ValidationData> validationVolumes);
}
