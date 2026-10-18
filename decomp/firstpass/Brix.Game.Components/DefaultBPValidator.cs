using Brix.Game.Components.Volumes;

namespace Brix.Game.Components;

public class DefaultBPValidator : BlueprintValidator
{
	protected override VPropertyMask SupportSettings => VPropertyMask.stoneSupport;

	public override AbstractValidator GetBlockValidatorCounterpart()
	{
		return SingletonSpawner.Get<DefaultBlockValidator>();
	}
}
