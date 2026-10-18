using MoonSharp.Interpreter.Interop;

namespace Brix.Lua;

public class LuaObjectStaticProxy
{
	public IUserDataDescriptor descriptor;

	public LuaObjectStaticProxy(IUserDataDescriptor desc)
	{
		descriptor = desc;
	}
}
