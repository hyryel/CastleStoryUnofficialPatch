using System.Collections.Generic;
using Brix.Engine;

namespace Brix.Game.Components;

public class BeamBlockValidator : WoodBlockValidator
{
	protected override bool HasGroundSupportAt(ref ValidationData vData, XYZ i)
	{
		return false;
	}

	protected override Dictionary<XYZ, SupportState> BuildSupportStates(ref ValidationData vData, IEnumerable<ValidationData> validationVolumes)
	{
		return BaseBeamValidation.BuildSupportStates(ref vData, validationVolumes);
	}

	protected override bool IsValid(ref ValidationData vData, Dictionary<XYZ, SupportState> supportStates)
	{
		return AllSupportedCount(supportStates, SupportState.CCSupport | SupportState.NearSupport | SupportState.Valid | SupportState.ConcreteEnd, vData) >= 1 || BaseRecursiveValidation.IsFarSupportedCC(supportStates);
	}

	public override SupportState GetSolidity(ref ValidationData vData, IEnumerable<ValidationData> validationVolumes)
	{
		return BaseBeamValidation.GetSolidity(vData, validationVolumes);
	}

	public override void PropagateRevalidation(ref ValidationData vData)
	{
		BaseBeamValidation.PropagateRevalidation(vData);
	}

	public override AbstractValidator GetBPValidatorCounterpart()
	{
		return SingletonSpawner.Get<BeamBPValidator>();
	}
}
