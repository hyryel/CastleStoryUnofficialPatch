using System;
using Brix.External.Factories;
using Brix.UI.Builder.Menu;

namespace Brix.UI.Builder.Panel;

public class HeaderPanel : BaseWidgetPanel<HeaderPanelComponent, HeaderPanel.WidgetId>
{
	public enum WidgetId
	{
		LayoutElement,
		Text,
		Collapsable
	}

	private readonly Factory.AssetKey _assetKey = Panels.CollapsableHeader;

	protected override Factory.AssetKey AssetKey => _assetKey;

	public HeaderPanel(bool luaDriven = false)
		: base((Action<HeaderPanelComponent>)null, (Action<HeaderPanelComponent>)null, luaDriven)
	{
	}

	public HeaderPanel(Action<HeaderPanelComponent> onReset, Action<HeaderPanelComponent> onRefresh)
		: base(onReset, onRefresh, luaDriven: false)
	{
	}

	public override IWidgetComponent GetWidgetComponent(WidgetId widgetId)
	{
		return widgetId switch
		{
			WidgetId.Text => base.Instance._textWidget, 
			WidgetId.Collapsable => base.Instance._collapsableWidget, 
			WidgetId.LayoutElement => base.Instance._layoutElementWidget, 
			_ => throw new NotImplementedException(), 
		};
	}

	public static HeaderPanel LuaNew()
	{
		return new HeaderPanel(luaDriven: true);
	}

	public static HeaderPanel New(Action<HeaderPanelComponent> onReset = null, Action<HeaderPanelComponent> onRefresh = null)
	{
		return new HeaderPanel(onReset, onRefresh);
	}
}
