using System;
using System.Reflection;
using MoonSharp.Interpreter;

namespace Brix.Lua;

public class InfoContainer
{
	[Flags]
	protected enum InfoState
	{
		None = 0,
		IsCalled = 1,
		IsStatic = 2
	}

	protected InfoState _state;

	protected DynValue _callback;

	public Type target;

	private static Type[] _initPropertyType = new Type[1] { typeof(PropertyInfo) };

	private static Type[] _initFieldType = new Type[1] { typeof(FieldInfo) };

	private static object[] _initObjectBuffer = new object[1];

	public bool isCalled => (_state & InfoState.IsCalled) == InfoState.IsCalled;

	public bool isStatic => (_state & InfoState.IsStatic) == InfoState.IsStatic;

	protected InfoContainer()
	{
	}

	public virtual DynValue GetValue(Script script, IUserData target)
	{
		throw new NotSupportedException();
	}

	public virtual void SetValue(Script script, IUserData target, DynValue value)
	{
		throw new NotSupportedException();
	}

	public DynValue GetCallback()
	{
		return _callback;
	}

	public static InfoContainer From(MethodInfo info, Type target)
	{
		LuaCall luaCall = new LuaCall(info);
		luaCall.target = target;
		return luaCall;
	}

	public static InfoContainer From(PropertyInfo info, Type target)
	{
		MethodInfo getMethod = info.GetGetMethod(nonPublic: true);
		MethodInfo setMethod = info.GetSetMethod(nonPublic: true);
		Type type = (((getMethod == null || !getMethod.IsStatic) && (setMethod == null || !setMethod.IsStatic)) ? typeof(LuaProperty<, >).MakeGenericType(info.DeclaringType, info.PropertyType) : typeof(LuaStaticProperty<>).MakeGenericType(info.PropertyType));
		_initObjectBuffer[0] = info;
		InfoContainer infoContainer = (InfoContainer)type.GetConstructor(_initPropertyType).Invoke(_initObjectBuffer);
		infoContainer.target = target;
		return infoContainer;
	}

	public static InfoContainer From(FieldInfo info, Type target)
	{
		_initObjectBuffer[0] = info;
		Type type = typeof(LuaField<, >).MakeGenericType(info.DeclaringType, info.FieldType);
		InfoContainer infoContainer = (InfoContainer)type.GetConstructor(_initFieldType).Invoke(_initObjectBuffer);
		infoContainer.target = target;
		return infoContainer;
	}
}
