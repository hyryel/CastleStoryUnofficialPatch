namespace LibNoise.Generator;

public class Const : ModuleBase
{
	private double _value;

	public double Value
	{
		get
		{
			return _value;
		}
		set
		{
			_value = value;
		}
	}

	public Const()
		: base(0)
	{
	}

	public Const(double value)
		: base(0)
	{
		Value = value;
	}

	public override double GetValue(double x, double y, double z)
	{
		return _value;
	}
}
