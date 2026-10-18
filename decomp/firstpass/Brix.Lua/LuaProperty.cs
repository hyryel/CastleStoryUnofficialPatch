using System;
using System.Reflection;
using MoonSharp.Interpreter;

namespace Brix.Lua;

public class LuaProperty<Target, R> : InfoContainer
{
	private Func<Target, R> _get;

	private Action<Target, R> _set;

	public LuaProperty(PropertyInfo info)
	{
		MethodInfo getMethod = info.GetGetMethod(nonPublic: true);
		MethodInfo setMethod = info.GetSetMethod(nonPublic: true);
		if (getMethod != null)
		{
			_get = (Func<Target, R>)Delegate.CreateDelegate(typeof(Func<Target, R>), null, getMethod);
		}
		if (setMethod != null)
		{
			_set = (Action<Target, R>)Delegate.CreateDelegate(typeof(Action<Target, R>), null, setMethod);
		}
	}

	public override DynValue GetValue(Script script, IUserData target)
	{
		target.TryGet<Target>(out var param);
		return DynValue.FromObject(script, _get(param));
	}

	public override void SetValue(Script script, IUserData target, DynValue value)
	{
		target.TryGet<Target>(out var param);
		_set(param, value.ToObject<R>());
	}
}
