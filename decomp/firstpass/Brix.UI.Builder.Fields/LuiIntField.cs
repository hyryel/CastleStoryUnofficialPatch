using Brix.Lua;
using Brix.UI.Builder.Widget;
using MoonSharp.Interpreter;

namespace Brix.UI.Builder.Fields;

public class LuiIntField
{
	private readonly LuiWidget _luiWidget;

	private bool _isFirstRun = true;

	private int _value;

	private static LuaObjectDescriptor<LuiIntField> _desc = new LuaObjectDescriptor<LuiIntField>();

	public int Value
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

	public LuiIntField(LuiWidget luiWidget)
	{
		_luiWidget = luiWidget;
	}

	private void Set(int value)
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
		LuiIntField luiIntField = ca.AsUserData<LuiIntField>(0, "Set");
		int value = ca.AsInt(1, "Set");
		luiIntField.Set(value);
		return DynValue.Void;
	}
}
