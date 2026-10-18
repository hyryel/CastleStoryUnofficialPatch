namespace LibNoise.Operator;

public class Add : ModuleBase
{
	public Add()
		: base(2)
	{
	}

	public Add(ModuleBase lhs, ModuleBase rhs)
		: base(2)
	{
		base.Modules[0] = lhs;
		base.Modules[1] = rhs;
	}

	public override double GetValue(double x, double y, double z)
	{
		return base.Modules[0].GetValue(x, y, z) + base.Modules[1].GetValue(x, y, z);
	}
}
