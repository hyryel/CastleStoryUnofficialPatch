using System;

namespace LibNoise.Operator;

public class Abs : ModuleBase
{
	public Abs()
		: base(1)
	{
	}

	public Abs(ModuleBase input)
		: base(1)
	{
		base.Modules[0] = input;
	}

	public override double GetValue(double x, double y, double z)
	{
		return Math.Abs(base.Modules[0].GetValue(x, y, z));
	}
}
