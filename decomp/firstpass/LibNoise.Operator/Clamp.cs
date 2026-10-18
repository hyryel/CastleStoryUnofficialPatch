namespace LibNoise.Operator;

public class Clamp : ModuleBase
{
	private double _min = -1.0;

	private double _max = 1.0;

	public double Maximum
	{
		get
		{
			return _max;
		}
		set
		{
			_max = value;
		}
	}

	public double Minimum
	{
		get
		{
			return _min;
		}
		set
		{
			_min = value;
		}
	}

	public Clamp()
		: base(1)
	{
	}

	public Clamp(ModuleBase input)
		: base(1)
	{
		base.Modules[0] = input;
	}

	public Clamp(double min, double max, ModuleBase input)
		: base(1)
	{
		Minimum = min;
		Maximum = max;
		base.Modules[0] = input;
	}

	public void SetBounds(double min, double max)
	{
		_min = min;
		_max = max;
	}

	public override double GetValue(double x, double y, double z)
	{
		if (_min > _max)
		{
			double min = _min;
			_min = _max;
			_max = min;
		}
		double value = base.Modules[0].GetValue(x, y, z);
		if (value < _min)
		{
			return _min;
		}
		if (value > _max)
		{
			return _max;
		}
		return value;
	}
}
