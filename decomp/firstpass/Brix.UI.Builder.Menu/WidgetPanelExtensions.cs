using System;
using System.Collections.Generic;
using Brix.UI.Builder.Widget;
using Brix.Utils;

namespace Brix.UI.Builder.Menu;

public static class WidgetPanelExtensions
{
	private static class Utility<TWidgetId> where TWidgetId : struct
	{
		public static readonly Func<Dictionary<TWidgetId, IWidget>> NewDictionary = () => new Dictionary<TWidgetId, IWidget>(FastEnumIntEqualityComparer<TWidgetId>.Instance);
	}

	public static void CreateWidgets<TWidgetId>(this IPanel<TWidgetId> _this) where TWidgetId : struct
	{
		_this.Widget = CollectionPool.RequestDictionary(Utility<TWidgetId>.NewDictionary);
		TWidgetId[] values = LuaIdUtility.EnumStringMapper<TWidgetId>.Values;
		foreach (TWidgetId key in values)
		{
			_this.Widget.Add(key, (!_this.LuaDriven) ? null : new LuaWidget());
		}
	}

	public static void DestroyWidgets<TWidgetId>(this IPanel<TWidgetId> _this) where TWidgetId : struct
	{
		Dictionary<TWidgetId, IWidget> dictionary = _this.Widget;
		dictionary.Clear();
		CollectionPool.Release(ref dictionary);
		_this.Widget = null;
	}

	public static IPanel<TWidgetId> SetWidget<TWidgetId>(this IPanel<TWidgetId> _this, TWidgetId widgetId, IWidget widget) where TWidgetId : struct
	{
		_this.Widget[widgetId] = widget;
		return _this;
	}

	public static bool RefreshWidget<TWidgetId>(this IPanel<TWidgetId> _this, TWidgetId widgetId, int phase) where TWidgetId : struct
	{
		IWidget widget = _this.Widget[widgetId];
		widget = widget ?? new LuaWidget();
		return _this.GetWidgetComponent(widgetId)?.Refresh(widget, phase) ?? true;
	}

	public static bool RefreshAllWidgets<TWidgetId>(this IPanel<TWidgetId> _this, int phase = 0) where TWidgetId : struct
	{
		TWidgetId[] values = LuaIdUtility.EnumStringMapper<TWidgetId>.Values;
		foreach (TWidgetId widgetId in values)
		{
			if (!_this.RefreshWidget(widgetId, phase))
			{
				return false;
			}
		}
		return true;
	}
}
