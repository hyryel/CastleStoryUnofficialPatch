using System;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Brix.Lua;

public class LuaObjectStaticProxyDescriptor : IUserDataDescriptor
{
	public static LuaObjectStaticProxyDescriptor desc = new LuaObjectStaticProxyDescriptor();

	private static readonly UserDataRef Nil = new UserDataRef();

	public string Name => "LuaObjectStaticProxy";

	public Type Type => typeof(LuaObjectStaticProxy);

	public LuaObjectStaticProxyDescriptor()
	{
		UserData.RegisterType(typeof(LuaObjectStaticProxy), this);
	}

	public DynValue Index(Script script, IUserData obj, DynValue index, bool isDirectIndexing)
	{
		obj.TryGet<LuaObjectStaticProxy>(out var param);
		return param.descriptor.Index(script, Nil, index, isDirectIndexing);
	}

	public bool SetIndex(Script script, IUserData obj, DynValue index, DynValue value, bool isDirectIndexing)
	{
		obj.TryGet<LuaObjectStaticProxy>(out var param);
		return param.descriptor.SetIndex(script, Nil, index, value, isDirectIndexing);
	}

	public string AsString(IUserData obj)
	{
		obj.TryGet<LuaObjectStaticProxy>(out var param);
		return param.descriptor.AsString(Nil);
	}

	public DynValue MetaIndex(Script script, IUserData obj, string metaname)
	{
		obj.TryGet<LuaObjectStaticProxy>(out var param);
		return param.descriptor.MetaIndex(script, Nil, metaname);
	}

	public bool IsTypeCompatible(Type type, IUserData obj)
	{
		obj.TryGet<LuaObjectStaticProxy>(out var param);
		return param.descriptor.IsTypeCompatible(type, null);
	}
}
