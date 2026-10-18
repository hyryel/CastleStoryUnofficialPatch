using System;
using System.Collections.Generic;
using Brix.UI.Builder.Menu;

namespace Brix.UI.Builder.Panel;

public abstract class BaseLayoutWidgetPanel<TPanelComponent, TWidgetId> : BaseLayoutPanel<TPanelComponent>, IPanel<TWidgetId>, IPanel, IRefresh, IInstance, ILuaId, IHierarchyName where TPanelComponent : BasePanelComponent where TWidgetId : struct
{
	public Dictionary<TWidgetId, IWidget> Widget { get; set; }

	protected BaseLayoutWidgetPanel()
	{
		this.CreateWidgets();
	}

	protected BaseLayoutWidgetPanel(Action<TPanelComponent> onReset, Action<TPanelComponent> onRefresh, bool luaDriven = false)
		: base(onReset, onRefresh, luaDriven)
	{
		this.CreateWidgets();
	}

	~BaseLayoutWidgetPanel()
	{
		this.DestroyWidgets();
	}

	public abstract IWidgetComponent GetWidgetComponent(TWidgetId widgetId);

	public override void TrySetLuaWidgets()
	{
		Dictionary<string, TWidgetId> nameValueMap = LuaIdUtility.EnumStringMapper<TWidgetId>.NameValueMap;
		foreach (KeyValuePair<string, IWidget> item in base.LuaWidget)
		{
			string key = item.Key;
			if (!nameValueMap.ContainsKey(key))
			{
				BasePanel.logger.Info("WidgetName not supported.\n" + key);
				continue;
			}
			TWidgetId key2 = LuaIdUtility.EnumStringMapper<TWidgetId>.NameValueMap[key];
			Widget[key2] = base.LuaWidget[key];
		}
	}

	public override bool Refresh()
	{
		if (!base.Refresh())
		{
			return false;
		}
		bool result = true;
		if (!base.IgnoresWidgets)
		{
			result = this.RefreshAllWidgets();
		}
		return result;
	}
}
