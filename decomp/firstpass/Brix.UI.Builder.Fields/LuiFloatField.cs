using Brix.Lua;
using Brix.UI.Builder.Widget;
using MoonSharp.Interpreter;

namespace Brix.UI.Builder.Fields;

public class LuiFloatField
{
	private readonly LuiWidget _luiWidget;

	private bool _isFirstRun = true;

	private float _value;

	private static LuaObjectDescriptor<LuiFloatField> _desc = new LuaObjectDescriptor<LuiFloatField>();

	public float Value
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

	public LuiFloatField(LuiWidget luiWidget)
	{
		_luiWidget = luiWidget;
	}

	private void Set(float value)
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
		LuiFloatField luiFloatField = ca.AsUserData<LuiFloatField>(0, "Set");
		float value = ca.AsFloat(1, "Set");
		luiFloatField.Set(value);
		return DynValue.Void;
	}
}
