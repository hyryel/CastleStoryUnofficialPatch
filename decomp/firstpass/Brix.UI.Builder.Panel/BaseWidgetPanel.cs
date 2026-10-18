using System;
using System.Collections.Generic;
using Brix.UI.Builder.Menu;

namespace Brix.UI.Builder.Panel;

public abstract class BaseWidgetPanel<TPanelComponent, TWidgetId> : BasePanel<TPanelComponent>, IPanel<TWidgetId>, IPanel, IRefresh, IInstance, ILuaId, IHierarchyName where TPanelComponent : BasePanelComponent where TWidgetId : struct
{
	public Dictionary<TWidgetId, IWidget> Widget { get; set; }

	protected BaseWidgetPanel()
	{
		this.CreateWidgets();
	}

	protected BaseWidgetPanel(Action<TPanelComponent> onReset, Action<TPanelComponent> onRefresh, bool luaDriven = false)
		: base(onReset, onRefresh, luaDriven)
	{
		this.CreateWidgets();
	}

	~BaseWidgetPanel()
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
				BasePanel.logger.WarnFormat("WidgetName not supported.\n{0}.{1}", typeof(TWidgetId).Name, key);
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
