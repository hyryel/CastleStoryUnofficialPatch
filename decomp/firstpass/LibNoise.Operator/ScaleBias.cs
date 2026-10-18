namespace LibNoise.Operator;

public class ScaleBias : ModuleBase
{
	public double Bias { get; set; }

	public double Scale { get; set; }

	public ScaleBias()
		: base(1)
	{
		Scale = 1.0;
	}

	public ScaleBias(ModuleBase input)
		: base(1)
	{
		base.Modules[0] = input;
		Scale = 1.0;
	}

	public ScaleBias(double scale, double bias, ModuleBase input)
		: base(1)
	{
		base.Modules[0] = input;
		Bias = bias;
		Scale = scale;
	}

	public override double GetValue(double x, double y, double z)
	{
		return base.Modules[0].GetValue(x, y, z) * Scale + Bias;
	}
}
