using Brix.Engine;
using Brix.Game.Components.Volumes;

namespace Brix.Game.Components;

public class RopeBridgePlankBlockValidator : ConcreteValidator
{
	protected override VPropertyMask SupportSettings => VPropertyMask.ropeSupport;

	public override AbstractValidator GetBPValidatorCounterpart()
	{
		return SingletonSpawner.Get<RopeBridgePlankBPValidator>();
	}

	public override void SetUnstable(ref ValidationData vData)
	{
		base.SetUnstable(ref vData);
		Architecte.AddVoxelToFreedList(vData.Volume.Position);
	}
}
