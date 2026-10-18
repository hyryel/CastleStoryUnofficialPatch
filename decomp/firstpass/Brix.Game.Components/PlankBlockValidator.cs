using System.Collections.Generic;
using Brix.Engine;
using Brix.Utils;
using UnityEngine.Networking;

namespace Brix.Game.Components;

public class PlankBlockValidator : WoodBlockValidator
{
	public override ValidationResult Validate(ref ValidationData vData, IEnumerable<ValidationData> validationVolumes = null)
	{
		if (!NetworkServer.active)
		{
			return ValidationResult.Valid;
		}
		Dictionary<XYZ, SupportState> dictionary = BasePlankValidation.BuildSupportStates(vData, validationVolumes);
		ValidationResult result = ((AllSupportedCount(dictionary, SupportState.CCSupport | SupportState.NearSupport | SupportState.Valid | SupportState.ConcreteEnd, vData) >= 1) ? ValidationResult.Valid : ((!BaseRecursiveValidation.IsFarSupportedCC(dictionary)) ? ValidationResult.Invalid : ValidationResult.Valid));
		dictionary.Dispose();
		return result;
	}

	protected override bool HasGroundSupportAt(ref ValidationData vData, XYZ i)
	{
		return false;
	}

	public override SupportState GetSolidity(ref ValidationData vData, IEnumerable<ValidationData> validationVolumes)
	{
		return SupportState.Unsupported;
	}

	public override AbstractValidator GetBPValidatorCounterpart()
	{
		return SingletonSpawner.Get<PlankBPValidator>();
	}
}
