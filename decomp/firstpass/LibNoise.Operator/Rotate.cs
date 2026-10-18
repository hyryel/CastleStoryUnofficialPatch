using System;

namespace LibNoise.Operator;

public class Rotate : ModuleBase
{
	private double _x;

	private double _x1Matrix;

	private double _x2Matrix;

	private double _x3Matrix;

	private double _y;

	private double _y1Matrix;

	private double _y2Matrix;

	private double _y3Matrix;

	private double _z;

	private double _z1Matrix;

	private double _z2Matrix;

	private double _z3Matrix;

	public double X
	{
		get
		{
			return _x;
		}
		set
		{
			SetAngles(value, _y, _z);
		}
	}

	public double Y
	{
		get
		{
			return _y;
		}
		set
		{
			SetAngles(_x, value, _z);
		}
	}

	public double Z
	{
		get
		{
			return _x;
		}
		set
		{
			SetAngles(_x, _y, value);
		}
	}

	public Rotate()
		: base(1)
	{
		SetAngles(0.0, 0.0, 0.0);
	}

	public Rotate(ModuleBase input)
		: base(1)
	{
		base.Modules[0] = input;
	}

	public Rotate(double x, double y, double z, ModuleBase input)
		: base(1)
	{
		base.Modules[0] = input;
		SetAngles(x, y, z);
	}

	private void SetAngles(double x, double y, double z)
	{
		double num = Math.Cos(x * 0.01745329238474369);
		double num2 = Math.Cos(y * 0.01745329238474369);
		double num3 = Math.Cos(z * 0.01745329238474369);
		double num4 = Math.Sin(x * 0.01745329238474369);
		double num5 = Math.Sin(y * 0.01745329238474369);
		double num6 = Math.Sin(z * 0.01745329238474369);
		_x1Matrix = num5 * num4 * num6 + num2 * num3;
		_y1Matrix = num * num6;
		_z1Matrix = num5 * num3 - num2 * num4 * num6;
		_x2Matrix = num5 * num4 * num3 - num2 * num6;
		_y2Matrix = num * num3;
		_z2Matrix = (0.0 - num2) * num4 * num3 - num5 * num6;
		_x3Matrix = (0.0 - num5) * num;
		_y3Matrix = num4;
		_z3Matrix = num2 * num;
		_x = x;
		_y = y;
		_z = z;
	}

	public override double GetValue(double x, double y, double z)
	{
		double x2 = _x1Matrix * x + _y1Matrix * y + _z1Matrix * z;
		double y2 = _x2Matrix * x + _y2Matrix * y + _z2Matrix * z;
		double z2 = _x3Matrix * x + _y3Matrix * y + _z3Matrix * z;
		return base.Modules[0].GetValue(x2, y2, z2);
	}
}
