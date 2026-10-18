using System.Collections.Generic;
using Brix.Engine;

namespace Brix.Game.Components;

public class DefaultWEOValidator : BlueprintValidator
{
	protected override ValidationResult GetValidationResult(ref ValidationData vData, Dictionary<XYZ, SupportState> supportStates)
	{
		if (IsValid(ref vData, supportStates))
		{
			return ValidationResult.Valid;
		}
		return ValidationResult.Invalid;
	}

	public override AbstractValidator GetBlockValidatorCounterpart()
	{
		return null;
	}
}
