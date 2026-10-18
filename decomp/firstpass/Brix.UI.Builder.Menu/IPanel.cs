using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public interface IPanel : IRefresh, IInstance, ILuaId, IHierarchyName
{
	IMenu ParentMenu { get; set; }

	ILayoutPanel ParentPanel { get; set; }

	IWidget DefaultWidget { get; set; }

	Dictionary<string, IWidget> LuaWidget { get; set; }

	bool LuaDriven { get; }

	RectTransform GetTrs();

	RectTransform GetParentLayoutTrs();

	void TrySetLuaWidgets();

	void Lua_SetDefaultHandle(string handleKey, Closure closure);

	void Lua_ClearDefaultHandle(string handleKey);

	void Lua_ClearDefaultHandles(string[] handleKeys);

	void Lua_ClearAllDefaultHandles();
}
public interface IPanel<TWidgetId> : IPanel, IRefresh, IInstance, ILuaId, IHierarchyName where TWidgetId : struct
{
	Dictionary<TWidgetId, IWidget> Widget { get; set; }

	IWidgetComponent GetWidgetComponent(TWidgetId widgetId);
}
