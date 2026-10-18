using System;

namespace LibNoise.Generator;

public class Spheres : ModuleBase
{
	private double _frequency = 1.0;

	public double Frequency
	{
		get
		{
			return _frequency;
		}
		set
		{
			_frequency = value;
		}
	}

	public Spheres()
		: base(0)
	{
	}

	public Spheres(double frequency)
		: base(0)
	{
		Frequency = frequency;
	}

	public override double GetValue(double x, double y, double z)
	{
		x *= _frequency;
		y *= _frequency;
		z *= _frequency;
		double num = Math.Sqrt(x * x + y * y + z * z);
		double num2 = num - Math.Floor(num);
		double val = 1.0 - num2;
		double num3 = Math.Min(num2, val);
		return 1.0 - num3 * 4.0;
	}
}
