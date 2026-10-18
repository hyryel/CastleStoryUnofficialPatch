using System;

namespace LibNoise.Operator;

public class Min : ModuleBase
{
	public Min()
		: base(2)
	{
	}

	public Min(ModuleBase lhs, ModuleBase rhs)
		: base(2)
	{
		base.Modules[0] = lhs;
		base.Modules[1] = rhs;
	}

	public override double GetValue(double x, double y, double z)
	{
		double value = base.Modules[0].GetValue(x, y, z);
		double value2 = base.Modules[1].GetValue(x, y, z);
		return Math.Min(value, value2);
	}
}
