using LibNoise.Generator;

namespace LibNoise.Operator;

public class Turbulence : ModuleBase
{
	private const double X0 = 0.189422607421875;

	private const double Y0 = 0.99371337890625;

	private const double Z0 = 0.4781646728515625;

	private const double X1 = 0.4046478271484375;

	private const double Y1 = 0.276611328125;

	private const double Z1 = 0.9230499267578125;

	private const double X2 = 0.82122802734375;

	private const double Y2 = 0.1710968017578125;

	private const double Z2 = 0.6842803955078125;

	private double _power = 1.0;

	private readonly Perlin _xDistort;

	private readonly Perlin _yDistort;

	private readonly Perlin _zDistort;

	public double Frequency
	{
		get
		{
			return _xDistort.Frequency;
		}
		set
		{
			_xDistort.Frequency = value;
			_yDistort.Frequency = value;
			_zDistort.Frequency = value;
		}
	}

	public double Power
	{
		get
		{
			return _power;
		}
		set
		{
			_power = value;
		}
	}

	public int Roughness
	{
		get
		{
			return _xDistort.OctaveCount;
		}
		set
		{
			_xDistort.OctaveCount = value;
			_yDistort.OctaveCount = value;
			_zDistort.OctaveCount = value;
		}
	}

	public int Seed
	{
		get
		{
			return _xDistort.Seed;
		}
		set
		{
			_xDistort.Seed = value;
			_yDistort.Seed = value + 1;
			_zDistort.Seed = value + 2;
		}
	}

	public Turbulence()
		: base(1)
	{
		_xDistort = new Perlin();
		_yDistort = new Perlin();
		_zDistort = new Perlin();
	}

	public Turbulence(ModuleBase input)
		: base(1)
	{
		_xDistort = new Perlin();
		_yDistort = new Perlin();
		_zDistort = new Perlin();
		base.Modules[0] = input;
	}

	public Turbulence(double power, ModuleBase input)
		: this(new Perlin(), new Perlin(), new Perlin(), power, input)
	{
	}

	public Turbulence(Perlin x, Perlin y, Perlin z, double power, ModuleBase input)
		: base(1)
	{
		_xDistort = x;
		_yDistort = y;
		_zDistort = z;
		base.Modules[0] = input;
		Power = power;
	}

	public override double GetValue(double x, double y, double z)
	{
		double x2 = x + _xDistort.GetValue(x + 0.189422607421875, y + 0.99371337890625, z + 0.4781646728515625) * _power;
		double y2 = y + _yDistort.GetValue(x + 0.4046478271484375, y + 0.276611328125, z + 0.9230499267578125) * _power;
		double z2 = z + _zDistort.GetValue(x + 0.82122802734375, y + 0.1710968017578125, z + 0.6842803955078125) * _power;
		return base.Modules[0].GetValue(x2, y2, z2);
	}
}
