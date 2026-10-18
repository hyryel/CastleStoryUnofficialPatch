using Brix.Game.Components.Volumes;

namespace Brix.Game.Components;

public class MineStairBlockValidator : DefaultBlockValidator
{
	private static VPropertyMask _supportSettings = VPropertyMask.stoneSupport | VPropertyMask.logSupport | VPropertyMask.plankSupport;

	protected override VPropertyMask SupportSettings => _supportSettings;

	public override AbstractValidator GetBPValidatorCounterpart()
	{
		return SingletonSpawner.Get<LogBPValidator>();
	}

	public override void SetUnstable(ref ValidationData vData)
	{
		DestroyBlock(vData);
	}
}
