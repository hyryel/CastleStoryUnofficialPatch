using System;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Brix.UI.Builder.Lui;

public static class HasWidgetsLuiExtensions
{
	public static void InitWidgets(this IHasWidgetsLui _this)
	{
		_this.T.Set("Widgets", DynValue.NewTable(_this.OwnerScript));
		_this.T.Set("SetWidgetHandle", DynValue.NewCallback((ScriptExecutionContext con, CallbackArguments args) => _this.SetWidgetHandle(con, args)));
	}

	public static Table GetWidgets(this IHasWidgetsLui _this)
	{
		return _this.T.Get("Widgets").Table;
	}

	public static DynValue SetWidgetHandle(this IHasWidgetsLui _this, ScriptExecutionContext sec, CallbackArguments ca)
	{
		Table table = ca.AsTable(0, "SetWidgetHandle");
		Table table2 = ca.AsTable(1, "SetWidgetHandle");
		Closure function = ca.AsType(2, "SetWidgetHandle", DataType.Function).Function;
		DynValue dynValue = ca.RawGet(3, translateVoids: true);
		dynValue = ((dynValue.Type != DataType.Boolean) ? DynValue.False : dynValue);
		string key = table.Get("Name").String;
		string key2 = table2.Get("Name").String;
		try
		{
			Table table3 = _this.T.Get("Widgets").Table;
			DynValue value = table3.Get(key);
			if (value.IsNil())
			{
				LuiBuilder.WidgetLui widgetLui = new LuiBuilder.WidgetLui(_this.OwnerScript, table.Get("Name").String, string.Empty);
				value = DynValue.NewTable(widgetLui.T);
				table3.Set(widgetLui.TypeName, value);
			}
			Table table4 = value.Table.Get("Handles").Table;
			DynValue value2 = table4.Get(key2);
			if (value2.IsNil())
			{
				LuiBuilder.HandleLui handleLui = new LuiBuilder.HandleLui(_this.OwnerScript, table2.Get("Name").String, string.Empty);
				value2 = DynValue.NewTable(handleLui.T);
				table4.Set(handleLui.TypeName, value2);
			}
			value2.Table.Set("LiveValue", DynValue.NewClosure(function));
			value2.Table.Set("LiveValueIsOnce", dynValue);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		return _this.D;
	}
}
