using LibNoise;
using LibNoise.Operator;

namespace Brix.Noise;

public class NoiseModifier : NoiseBaseComponent
{
	public ModifierType ModifierType;

	public NoiseBaseComponent NoiseComponent;

	public double ValueA;

	public double ValueB;

	public bool Inverted;

	public override void Refresh()
	{
		base.Refresh();
		if ((bool)NoiseComponent)
		{
			NoiseComponent.Refresh();
		}
	}

	public override void SetSeed(int seed)
	{
		NoiseComponent.SetSeed(seed);
	}

	protected override ModuleBase InitNoiseModule()
	{
		if (NoiseComponent == null)
		{
			return null;
		}
		switch (ModifierType)
		{
		case ModifierType.Abs:
			base.NoiseModuleBase = new Abs(NoiseComponent.NoiseModuleBase);
			break;
		case ModifierType.Clamp:
			base.NoiseModuleBase = new Clamp(ValueA, ValueB, NoiseComponent.NoiseModuleBase);
			break;
		case ModifierType.Exponent:
			base.NoiseModuleBase = new Exponent(ValueA, NoiseComponent.NoiseModuleBase);
			break;
		case ModifierType.Invert:
			base.NoiseModuleBase = new Invert(NoiseComponent.NoiseModuleBase);
			break;
		case ModifierType.ScaleBias:
			base.NoiseModuleBase = new ScaleBias(ValueA, ValueB, NoiseComponent.NoiseModuleBase);
			break;
		case ModifierType.Terrace:
			base.NoiseModuleBase = new Terrace(Inverted, NoiseComponent.NoiseModuleBase);
			break;
		}
		return base.NoiseModuleBase;
	}
}
