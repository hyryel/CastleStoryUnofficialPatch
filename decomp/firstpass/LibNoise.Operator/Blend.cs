namespace LibNoise.Operator;

public class Blend : ModuleBase
{
	public ModuleBase Controller
	{
		get
		{
			return base.Modules[2];
		}
		set
		{
			base.Modules[2] = value;
		}
	}

	public Blend()
		: base(3)
	{
	}

	public Blend(ModuleBase lhs, ModuleBase rhs, ModuleBase controller)
		: base(3)
	{
		base.Modules[0] = lhs;
		base.Modules[1] = rhs;
		base.Modules[2] = controller;
	}

	public override double GetValue(double x, double y, double z)
	{
		double value = base.Modules[0].GetValue(x, y, z);
		double value2 = base.Modules[1].GetValue(x, y, z);
		double position = (base.Modules[2].GetValue(x, y, z) + 1.0) / 2.0;
		return Utils.InterpolateLinear(value, value2, position);
	}
}
