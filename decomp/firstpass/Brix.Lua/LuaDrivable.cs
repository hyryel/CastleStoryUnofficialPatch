using System;

namespace Brix.Lua;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field)]
public class LuaDrivable : Attribute
{
	public string Name { get; private set; }

	public bool UseExtern { get; set; }

	public LuaDrivable(string drivename)
	{
		Name = drivename;
	}
}
