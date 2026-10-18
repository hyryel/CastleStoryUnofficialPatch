using System;
using Brix.External.Factories;
using Brix.UI.Builder.Menu;

namespace Brix.UI.Builder.Panel;

public class ProgressbarPanel : BaseWidgetPanel<ProgressbarPanelComponent, ProgressbarPanel.WidgetId>
{
	public enum WidgetId
	{
		LayoutElement,
		BgImage,
		FillImage
	}

	private readonly Factory.AssetKey _assetKey = Panels.Progressbar;

	protected override Factory.AssetKey AssetKey => _assetKey;

	public ProgressbarPanel(bool luaDriven = false)
		: base((Action<ProgressbarPanelComponent>)null, (Action<ProgressbarPanelComponent>)null, luaDriven)
	{
	}

	public ProgressbarPanel(Action<ProgressbarPanelComponent> onReset, Action<ProgressbarPanelComponent> onRefresh)
		: base(onReset, onRefresh, luaDriven: false)
	{
	}

	public override IWidgetComponent GetWidgetComponent(WidgetId widgetId)
	{
		return widgetId switch
		{
			WidgetId.BgImage => base.Instance._bgImageWidget, 
			WidgetId.FillImage => base.Instance._fillImageWidget, 
			WidgetId.LayoutElement => base.Instance._layoutElementWidget, 
			_ => throw new NotImplementedException(), 
		};
	}

	public static ProgressbarPanel LuaNew()
	{
		return new ProgressbarPanel(luaDriven: true);
	}

	public static ProgressbarPanel New(Action<ProgressbarPanelComponent> onReset = null, Action<ProgressbarPanelComponent> onRefresh = null)
	{
		return new ProgressbarPanel(onReset, onRefresh);
	}
}
