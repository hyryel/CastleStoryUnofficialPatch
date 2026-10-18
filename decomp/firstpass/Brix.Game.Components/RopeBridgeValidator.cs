using Brix.Game.Components.Volumes;

namespace Brix.Game.Components;

internal class RopeBridgeValidator : DefaultDOValidator
{
	private static VPropertyMask _supportSettings = VPropertyMask.stoneSupport | VPropertyMask.logSupport | VPropertyMask.plankSupport;

	protected override VPropertyMask SupportSettings => _supportSettings;

	public override AbstractValidator GetBPValidatorCounterpart()
	{
		return SingletonSpawner.Get<RopeBridgeBPValidator>();
	}
}
