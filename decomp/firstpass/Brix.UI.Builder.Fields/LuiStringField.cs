using Brix.Lua;
using Brix.UI.Builder.Widget;
using MoonSharp.Interpreter;

namespace Brix.UI.Builder.Fields;

public class LuiStringField
{
	private readonly LuiWidget _luWidget;

	private bool _isFirstRun = true;

	private string _value;

	private static LuaObjectDescriptor<LuiStringField> _desc = new LuaObjectDescriptor<LuiStringField>();

	public string Value
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

	public LuiStringField(LuiWidget luWidget)
	{
		_luWidget = luWidget;
	}

	private void Set(string value)
	{
		if (_isFirstRun || !value.Equals(_value))
		{
			_value = value;
			_isFirstRun = false;
			_luWidget.SetDirty();
		}
	}

	[LuaDrivable("Set")]
	private static DynValue lua_Set(ScriptExecutionContext sec, CallbackArguments ca)
	{
		LuiStringField luiStringField = ca.AsUserData<LuiStringField>(0, "Set");
		string value = ca.AsString(1, "Set");
		luiStringField.Set(value);
		return DynValue.Void;
	}
}
