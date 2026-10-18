using System;

namespace LibNoise.Operator;

public class Power : ModuleBase
{
	public Power()
		: base(2)
	{
	}

	public Power(ModuleBase lhs, ModuleBase rhs)
		: base(2)
	{
		base.Modules[0] = lhs;
		base.Modules[1] = rhs;
	}

	public override double GetValue(double x, double y, double z)
	{
		return Math.Pow(base.Modules[0].GetValue(x, y, z), base.Modules[1].GetValue(x, y, z));
	}
}
