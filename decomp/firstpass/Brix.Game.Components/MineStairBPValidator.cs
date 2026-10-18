namespace Brix.Game.Components;

public class MineStairBPValidator : DefaultBPValidator
{
	public override AbstractValidator GetBlockValidatorCounterpart()
	{
		return SingletonSpawner.Get<MineStairBlockValidator>();
	}
}
