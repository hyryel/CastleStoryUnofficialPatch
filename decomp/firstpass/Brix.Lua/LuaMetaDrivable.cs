using System;

namespace Brix.Lua;

[AttributeUsage(AttributeTargets.Class)]
public class LuaMetaDrivable : Attribute
{
	public Type targetType;

	public LuaMetaDrivable(Type ofType)
	{
		targetType = ofType;
	}
}
