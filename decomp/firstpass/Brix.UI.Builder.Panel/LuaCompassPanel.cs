using System;
using Brix.External.Factories;
using Brix.UI.Builder.Menu;

namespace Brix.UI.Builder.Panel;

public class LuaCompassPanel : BaseWidgetPanel<LuaCompassPanelComponent, LuaCompassPanel.WidgetId>
{
	public enum BinId
	{
		Default
	}

	public enum WidgetId
	{
		CompassLayoutElement
	}

	public enum Variant
	{
		Default
	}

	private Factory.AssetKey _assetKey = Images.LuaCompass;

	protected override Factory.AssetKey AssetKey => _assetKey;

	public LuaCompassPanel(bool luaDriven = false)
		: base((Action<LuaCompassPanelComponent>)null, (Action<LuaCompassPanelComponent>)null, luaDriven)
	{
	}

	public LuaCompassPanel(Action<LuaCompassPanelComponent> onReset, Action<LuaCompassPanelComponent> onRefresh)
		: base(onReset, onRefresh, luaDriven: false)
	{
	}

	public override IWidgetComponent GetWidgetComponent(WidgetId widgetId)
	{
		if (widgetId == WidgetId.CompassLayoutElement)
		{
			return base.Instance._layoutElementWidget;
		}
		throw new NotImplementedException();
	}

	public LuaCompassPanel As(Variant variant)
	{
		if (variant == Variant.Default)
		{
			_assetKey = Images.LuaCompass;
			return this;
		}
		throw new NotImplementedException();
	}

	public static LuaCompassPanel LuaNew()
	{
		return new LuaCompassPanel(luaDriven: true);
	}

	public static LuaCompassPanel New(Action<LuaCompassPanelComponent> onReset = null, Action<LuaCompassPanelComponent> onRefresh = null)
	{
		return new LuaCompassPanel(onReset, onRefresh);
	}
}
