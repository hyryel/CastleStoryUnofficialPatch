using LibNoise;
using LibNoise.Operator;

namespace Brix.Noise;

public class NoiseCombiner : NoiseBaseComponent
{
	public CombinerType CombinerType;

	public NoiseBaseComponent NoiseComponentA;

	public NoiseBaseComponent NoiseComponentB;

	public override void Refresh()
	{
		base.Refresh();
		if ((bool)NoiseComponentA)
		{
			NoiseComponentA.Refresh();
		}
		if ((bool)NoiseComponentB)
		{
			NoiseComponentB.Refresh();
		}
	}

	public override void SetSeed(int seed)
	{
		NoiseComponentA.SetSeed(seed);
		NoiseComponentB.SetSeed(seed);
	}

	protected override ModuleBase InitNoiseModule()
	{
		if (NoiseComponentA == null || NoiseComponentB == null)
		{
			return null;
		}
		switch (CombinerType)
		{
		case CombinerType.Add:
			base.NoiseModuleBase = new Add(NoiseComponentA.NoiseModuleBase, NoiseComponentB.NoiseModuleBase);
			break;
		case CombinerType.Substract:
			base.NoiseModuleBase = new Subtract(NoiseComponentA.NoiseModuleBase, NoiseComponentB.NoiseModuleBase);
			break;
		case CombinerType.Min:
			base.NoiseModuleBase = new Min(NoiseComponentA.NoiseModuleBase, NoiseComponentB.NoiseModuleBase);
			break;
		case CombinerType.Max:
			base.NoiseModuleBase = new Max(NoiseComponentA.NoiseModuleBase, NoiseComponentB.NoiseModuleBase);
			break;
		case CombinerType.Multiply:
			base.NoiseModuleBase = new Multiply(NoiseComponentA.NoiseModuleBase, NoiseComponentB.NoiseModuleBase);
			break;
		case CombinerType.Power:
			base.NoiseModuleBase = new Power(NoiseComponentA.NoiseModuleBase, NoiseComponentB.NoiseModuleBase);
			break;
		}
		return base.NoiseModuleBase;
	}
}
