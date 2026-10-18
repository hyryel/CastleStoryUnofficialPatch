using System;
using System.Reflection;
using MoonSharp.Interpreter;

namespace Brix.Lua;

public class LuaStaticProperty<R> : InfoContainer
{
	private Func<R> _get;

	private Action<R> _set;

	public LuaStaticProperty(PropertyInfo info)
	{
		_state |= InfoState.IsStatic;
		MethodInfo getMethod = info.GetGetMethod(nonPublic: true);
		MethodInfo setMethod = info.GetSetMethod(nonPublic: true);
		if (getMethod != null)
		{
			_get = (Func<R>)Delegate.CreateDelegate(typeof(Func<R>), getMethod);
		}
		if (setMethod != null)
		{
			_set = (Action<R>)Delegate.CreateDelegate(typeof(Action<R>), setMethod);
		}
	}

	public override DynValue GetValue(Script script, IUserData target)
	{
		return DynValue.FromObject(script, _get());
	}

	public override void SetValue(Script script, IUserData target, DynValue value)
	{
		_set(value.ToObject<R>());
	}
}
