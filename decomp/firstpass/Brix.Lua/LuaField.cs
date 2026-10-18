using System.Reflection;
using Brix.Utils;
using MoonSharp.Interpreter;

namespace Brix.Lua;

public class LuaField<Target, R> : InfoContainer
{
	private Getter<Target, R> _get;

	private Setter<Target, R> _set;

	public LuaField(FieldInfo info)
	{
		if (info.IsStatic)
		{
			_state |= InfoState.IsStatic;
		}
		_get = info.DelegateForGet<Target, R>();
		_set = info.DelegateForSet<Target, R>();
	}

	public override DynValue GetValue(Script script, IUserData target)
	{
		target.TryGet<Target>(out var param);
		return DynValue.FromObject(script, _get(param));
	}

	public override void SetValue(Script script, IUserData target, DynValue value)
	{
		target.TryGet<Target>(out var param);
		_set(ref param, value.ToObject<R>());
	}
}
