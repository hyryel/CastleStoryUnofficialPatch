using System;

namespace LibNoise.Operator;

public class Exponent : ModuleBase
{
	private double _exponent = 1.0;

	public double Value
	{
		get
		{
			return _exponent;
		}
		set
		{
			_exponent = value;
		}
	}

	public Exponent()
		: base(1)
	{
	}

	public Exponent(ModuleBase input)
		: base(1)
	{
		base.Modules[0] = input;
	}

	public Exponent(double exponent, ModuleBase input)
		: base(1)
	{
		base.Modules[0] = input;
		Value = exponent;
	}

	public override double GetValue(double x, double y, double z)
	{
		double value = base.Modules[0].GetValue(x, y, z);
		return Math.Pow(Math.Abs((value + 1.0) / 2.0), _exponent) * 2.0 - 1.0;
	}
}
