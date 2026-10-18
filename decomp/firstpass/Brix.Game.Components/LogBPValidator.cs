using Brix.Game.Components.Volumes;

namespace Brix.Game.Components;

public class LogBPValidator : BlueprintValidator
{
	private const VPropertyMask _supportSettings = VPropertyMask.stoneSupport | VPropertyMask.logSupport;

	protected override VPropertyMask SupportSettings => VPropertyMask.stoneSupport | VPropertyMask.logSupport;

	public override AbstractValidator GetBlockValidatorCounterpart()
	{
		return SingletonSpawner.Get<LogBlockValidator>();
	}
}
