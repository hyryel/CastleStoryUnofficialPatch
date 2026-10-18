using Brix.Lua;
using Brix.UI.Builder.Widget;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Brix.UI.Builder.Fields;

public class LuiVector4Field
{
	private readonly LuiWidget _luiWidget;

	private bool _isFirstRun = true;

	private Vector4 _value;

	private static LuaObjectDescriptor<LuiVector4Field> _desc = new LuaObjectDescriptor<LuiVector4Field>();

	public Vector4 Value
	{
		get
		{
			return _value;
		}
		set
		{
			Set(value);
		}
	}

	public LuiVector4Field(LuiWidget luiWidget)
	{
		_luiWidget = luiWidget;
	}

	private void Set(Vector4 value)
	{
		if (_isFirstRun || !value.Equals(_value))
		{
			_value = value;
			_isFirstRun = false;
			_luiWidget.SetDirty();
		}
	}

	[LuaDrivable("Set")]
	private static DynValue lua_Set(ScriptExecutionContext sec, CallbackArguments ca)
	{
		LuiVector4Field luiVector4Field = ca.AsUserData<LuiVector4Field>(0, "Set");
		Vector4 value = ca.AsUserData<Vector4>(1, "Set");
		luiVector4Field.Set(value);
		return DynValue.Void;
	}
}
