namespace Brix.Game.Components;

internal class CobbleBrickBPValidator : DefaultBPValidator
{
	public override AbstractValidator GetBlockValidatorCounterpart()
	{
		return SingletonSpawner.Get<CobbleBrickValidator>();
	}
}
