using System;
using Brix.External.Factories;
using Brix.UI.Builder.Menu;

namespace Brix.UI.Builder.Panel;

public class MinimapPanel : BaseWidgetPanel<MinimapPanelComponent, MinimapPanel.WidgetId>
{
	public enum WidgetId
	{
		LayoutElement,
		Minimap
	}

	public enum Variant
	{
		Default
	}

	private Factory.AssetKey _assetKey = Images.MiniMap;

	protected override Factory.AssetKey AssetKey => _assetKey;

	public MinimapPanel(bool luaDriven = false)
		: base((Action<MinimapPanelComponent>)null, (Action<MinimapPanelComponent>)null, luaDriven)
	{
	}

	public MinimapPanel(Action<MinimapPanelComponent> onReset, Action<MinimapPanelComponent> onRefresh)
		: base(onReset, onRefresh, luaDriven: false)
	{
	}

	public override IWidgetComponent GetWidgetComponent(WidgetId widgetId)
	{
		return widgetId switch
		{
			WidgetId.LayoutElement => base.Instance._layoutElementWidget, 
			WidgetId.Minimap => base.Instance._minimapWidgetComponent, 
			_ => throw new NotImplementedException(), 
		};
	}

	public MinimapPanel As(Variant variant)
	{
		if (variant == Variant.Default)
		{
			_assetKey = Images.MiniMap;
		}
		return this;
	}

	public static MinimapPanel LuaNew()
	{
		return new MinimapPanel(luaDriven: true);
	}

	public static MinimapPanel New(Action<MinimapPanelComponent> onReset = null, Action<MinimapPanelComponent> onRefresh = null)
	{
		return new MinimapPanel(onReset, onRefresh);
	}
}
