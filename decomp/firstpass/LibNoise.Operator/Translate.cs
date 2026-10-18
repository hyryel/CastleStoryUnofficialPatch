namespace LibNoise.Operator;

public class Translate : ModuleBase
{
	private double _x = 1.0;

	private double _y = 1.0;

	private double _z = 1.0;

	public double X
	{
		get
		{
			return _x;
		}
		set
		{
			_x = value;
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
			_y = value;
		}
	}

	public double Z
	{
		get
		{
			return _z;
		}
		set
		{
			_z = value;
		}
	}

	public Translate()
		: base(1)
	{
	}

	public Translate(ModuleBase input)
		: base(1)
	{
		base.Modules[0] = input;
	}

	public Translate(double x, double y, double z, ModuleBase input)
		: base(1)
	{
		base.Modules[0] = input;
		X = x;
		Y = y;
		Z = z;
	}

	public override double GetValue(double x, double y, double z)
	{
		return base.Modules[0].GetValue(x + _x, y + _y, z + _z);
	}
}
