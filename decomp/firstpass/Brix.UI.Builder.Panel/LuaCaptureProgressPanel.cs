using System;
using Brix.External.Factories;
using Brix.UI.Builder.Menu;

namespace Brix.UI.Builder.Panel;

public class LuaCaptureProgressPanel : BaseWidgetPanel<LuaCaptureProgressPanelComponent, LuaCaptureProgressPanel.WidgetId>
{
	public enum BinId
	{
		Default
	}

	public enum WidgetId
	{
		CaptureProgressLayoutElement
	}

	public enum Variant
	{
		Default
	}

	private Factory.AssetKey _assetKey = Images.LuaCompass;

	protected override Factory.AssetKey AssetKey => _assetKey;

	public LuaCaptureProgressPanel(bool luaDriven = false)
		: base((Action<LuaCaptureProgressPanelComponent>)null, (Action<LuaCaptureProgressPanelComponent>)null, luaDriven)
	{
	}

	public LuaCaptureProgressPanel(Action<LuaCaptureProgressPanelComponent> onReset, Action<LuaCaptureProgressPanelComponent> onRefresh)
		: base(onReset, onRefresh, luaDriven: false)
	{
	}

	public override IWidgetComponent GetWidgetComponent(WidgetId widgetId)
	{
		if (widgetId == WidgetId.CaptureProgressLayoutElement)
		{
			return base.Instance._layoutElementWidget;
		}
		throw new NotImplementedException();
	}

	public LuaCaptureProgressPanel As(Variant variant)
	{
		if (variant == Variant.Default)
		{
			_assetKey = Images.LuaCompass;
			return this;
		}
		throw new NotImplementedException();
	}

	public static LuaCaptureProgressPanel LuaNew()
	{
		return new LuaCaptureProgressPanel(luaDriven: true);
	}

	public static LuaCaptureProgressPanel New(Action<LuaCaptureProgressPanelComponent> onReset = null, Action<LuaCaptureProgressPanelComponent> onRefresh = null)
	{
		return new LuaCaptureProgressPanel(onReset, onRefresh);
	}
}
