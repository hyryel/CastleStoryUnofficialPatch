namespace LibNoise.Operator;

public class Multiply : ModuleBase
{
	public Multiply()
		: base(2)
	{
	}

	public Multiply(ModuleBase lhs, ModuleBase rhs)
		: base(2)
	{
		base.Modules[0] = lhs;
		base.Modules[1] = rhs;
	}

	public override double GetValue(double x, double y, double z)
	{
		return base.Modules[0].GetValue(x, y, z) * base.Modules[1].GetValue(x, y, z);
	}
}
