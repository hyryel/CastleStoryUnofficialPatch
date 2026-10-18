using System;
using Brix.External.Factories;
using Brix.UI.Builder.Menu;

namespace Brix.UI.Builder.Panel;

public class SliderPanel : BaseWidgetPanel<SliderPanelComponent, SliderPanel.WidgetId>
{
	public enum BinId
	{
		Default
	}

	public enum WidgetId
	{
		LayoutElement,
		Slider,
		Image
	}

	public enum Variant
	{
		Default
	}

	private Factory.AssetKey _assetKey = Sliders.Short;

	protected override Factory.AssetKey AssetKey => _assetKey;

	public SliderPanel(bool luaDriven = false)
		: base((Action<SliderPanelComponent>)null, (Action<SliderPanelComponent>)null, luaDriven)
	{
	}

	public SliderPanel(Action<SliderPanelComponent> onReset, Action<SliderPanelComponent> onRefresh)
		: base(onReset, onRefresh, luaDriven: false)
	{
	}

	public override IWidgetComponent GetWidgetComponent(WidgetId widgetId)
	{
		return widgetId switch
		{
			WidgetId.Slider => base.Instance._sliderWidget, 
			WidgetId.LayoutElement => base.Instance._layoutElementWidget, 
			WidgetId.Image => base.Instance._imageWidget, 
			_ => throw new NotImplementedException(), 
		};
	}

	public SliderPanel As(Variant variant)
	{
		if (variant == Variant.Default)
		{
			_assetKey = Sliders.Short;
		}
		return this;
	}

	public static SliderPanel LuaNew()
	{
		return new SliderPanel(luaDriven: true);
	}

	public static SliderPanel New(Action<SliderPanelComponent> onReset = null, Action<SliderPanelComponent> onRefresh = null)
	{
		return new SliderPanel(onReset, onRefresh);
	}
}
