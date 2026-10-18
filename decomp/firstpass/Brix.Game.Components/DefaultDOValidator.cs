using Brix.Game.Components.Volumes;
using Brix.Lifecycle.Pooling;

namespace Brix.Game.Components;

public class DefaultDOValidator : ConcreteValidator
{
	protected override VPropertyMask SupportSettings => VPropertyMask.stoneSupport;

	public override void SetUnstable(ref ValidationData vData)
	{
		base.SetUnstable(ref vData);
		ObjectPoolSingleton.Release(vData.Volume.GameObject);
	}

	public override AbstractValidator GetBPValidatorCounterpart()
	{
		return SingletonSpawner.Get<DefaultBPValidator>();
	}
}
