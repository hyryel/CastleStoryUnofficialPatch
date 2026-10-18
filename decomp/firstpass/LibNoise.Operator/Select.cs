namespace LibNoise.Operator;

public class Select : ModuleBase
{
	private double _fallOff;

	private double _raw;

	private double _min = -1.0;

	private double _max = 1.0;

	public ModuleBase Controller
	{
		get
		{
			return base.Modules[2];
		}
		set
		{
			base.Modules[2] = value;
		}
	}

	public double FallOff
	{
		get
		{
			return _fallOff;
		}
		set
		{
			double num = _max - _min;
			_raw = value;
			_fallOff = ((!(value > num / 2.0)) ? value : (num / 2.0));
		}
	}

	public double Maximum
	{
		get
		{
			return _max;
		}
		set
		{
			_max = value;
			FallOff = _raw;
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
			FallOff = _raw;
		}
	}

	public Select()
		: base(3)
	{
	}

	public Select(ModuleBase inputA, ModuleBase inputB, ModuleBase controller)
		: base(3)
	{
		base.Modules[0] = inputA;
		base.Modules[1] = inputB;
		base.Modules[2] = controller;
	}

	public Select(double min, double max, double fallOff, ModuleBase inputA, ModuleBase inputB)
		: this(inputA, inputB, null)
	{
		_min = min;
		_max = max;
		FallOff = fallOff;
	}

	public void SetBounds(double min, double max)
	{
		_min = min;
		_max = max;
		FallOff = _fallOff;
	}

	public override double GetValue(double x, double y, double z)
	{
		double value = base.Modules[2].GetValue(x, y, z);
		if (_fallOff > 0.0)
		{
			if (value < _min - _fallOff)
			{
				return base.Modules[0].GetValue(x, y, z);
			}
			if (value < _min + _fallOff)
			{
				double num = _min - _fallOff;
				double num2 = _min + _fallOff;
				double position = Utils.MapCubicSCurve((value - num) / (num2 - num));
				return Utils.InterpolateLinear(base.Modules[0].GetValue(x, y, z), base.Modules[1].GetValue(x, y, z), position);
			}
			if (value < _max - _fallOff)
			{
				return base.Modules[1].GetValue(x, y, z);
			}
			if (value < _max + _fallOff)
			{
				double num3 = _max - _fallOff;
				double num4 = _max + _fallOff;
				double position = Utils.MapCubicSCurve((value - num3) / (num4 - num3));
				return Utils.InterpolateLinear(base.Modules[1].GetValue(x, y, z), base.Modules[0].GetValue(x, y, z), position);
			}
			return base.Modules[0].GetValue(x, y, z);
		}
		if (value < _min || value > _max)
		{
			return base.Modules[0].GetValue(x, y, z);
		}
		return base.Modules[1].GetValue(x, y, z);
	}
}
