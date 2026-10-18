namespace LibNoise.Operator;

public class Displace : ModuleBase
{
	public ModuleBase X
	{
		get
		{
			return base.Modules[1];
		}
		set
		{
			base.Modules[1] = value;
		}
	}

	public ModuleBase Y
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

	public ModuleBase Z
	{
		get
		{
			return base.Modules[3];
		}
		set
		{
			base.Modules[3] = value;
		}
	}

	public Displace()
		: base(4)
	{
	}

	public Displace(ModuleBase input, ModuleBase x, ModuleBase y, ModuleBase z)
		: base(4)
	{
		base.Modules[0] = input;
		base.Modules[1] = x;
		base.Modules[2] = y;
		base.Modules[3] = z;
	}

	public override double GetValue(double x, double y, double z)
	{
		double x2 = x + base.Modules[1].GetValue(x, y, z);
		double y2 = y + base.Modules[2].GetValue(x, y, z);
		double z2 = z + base.Modules[3].GetValue(x, y, z);
		return base.Modules[0].GetValue(x2, y2, z2);
	}
}
