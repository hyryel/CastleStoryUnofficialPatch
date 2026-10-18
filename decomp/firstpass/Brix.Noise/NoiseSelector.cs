using LibNoise;
using LibNoise.Operator;

namespace Brix.Noise;

public class NoiseSelector : NoiseBaseComponent
{
	public SelectorType SelectorType;

	public NoiseBaseComponent NoiseComponentA;

	public NoiseBaseComponent NoiseComponentB;

	public NoiseBaseComponent NoiseComponentControl;

	public override void Refresh()
	{
		base.Refresh();
		NoiseComponentA.Refresh();
		NoiseComponentB.Refresh();
		NoiseComponentControl.Refresh();
	}

	public override void SetSeed(int seed)
	{
		NoiseComponentA.SetSeed(seed);
		NoiseComponentB.SetSeed(seed);
		NoiseComponentControl.SetSeed(seed);
	}

	protected override ModuleBase InitNoiseModule()
	{
		switch (SelectorType)
		{
		case SelectorType.Blend:
			base.NoiseModuleBase = new Blend(NoiseComponentA.NoiseModuleBase, NoiseComponentB.NoiseModuleBase, NoiseComponentControl.NoiseModuleBase);
			break;
		case SelectorType.Select:
			base.NoiseModuleBase = new Select(NoiseComponentA.NoiseModuleBase, NoiseComponentB.NoiseModuleBase, NoiseComponentControl.NoiseModuleBase);
			break;
		}
		base.NoiseModuleBase = new Blend(NoiseComponentA.NoiseModuleBase, NoiseComponentB.NoiseModuleBase, NoiseComponentControl.NoiseModuleBase);
		return base.NoiseModuleBase;
	}
}
