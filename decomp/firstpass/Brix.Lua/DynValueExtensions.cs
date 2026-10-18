using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Brix.Lua;

public static class DynValueExtensions
{
	public static T ToEnum<T>(this DynValue dv) where T : struct
	{
		return dv.ToObject<LuaEnumProxy<T>>().value;
	}
}
