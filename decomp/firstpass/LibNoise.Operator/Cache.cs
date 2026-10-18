namespace LibNoise.Operator;

public class Cache : ModuleBase
{
	private double _value;

	private bool _cached;

	private double _x;

	private double _y;

	private double _z;

	public override ModuleBase this[int index]
	{
		get
		{
			return base[index];
		}
		set
		{
			base[index] = value;
			_cached = false;
		}
	}

	public Cache()
		: base(1)
	{
	}

	public Cache(ModuleBase input)
		: base(1)
	{
		base.Modules[0] = input;
	}

	public override double GetValue(double x, double y, double z)
	{
		if (!_cached || _x != x || _y != y || _z != z)
		{
			_value = base.Modules[0].GetValue(x, y, z);
			_x = x;
			_y = y;
			_z = z;
		}
		_cached = true;
		return _value;
	}
}
