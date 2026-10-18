using System;
using Brix.External.Factories;
using Brix.UI.Builder.Menu;

namespace Brix.UI.Builder.Panel;

public class SliderInputPanel : BaseWidgetPanel<SliderInputPanelComponent, SliderInputPanel.WidgetId>
{
	public enum WidgetId
	{
		LayoutElement,
		InputField,
		Slider,
		Label,
		ValueSuffixLabel
	}

	private readonly Factory.AssetKey _assetKey = Sliders.StandardWithInput;

	protected override Factory.AssetKey AssetKey => _assetKey;

	public SliderInputPanel(bool luaDriven = false)
		: base((Action<SliderInputPanelComponent>)null, (Action<SliderInputPanelComponent>)null, luaDriven)
	{
	}

	public SliderInputPanel(Action<SliderInputPanelComponent> onReset, Action<SliderInputPanelComponent> onRefresh)
		: base(onReset, onRefresh, luaDriven: false)
	{
	}

	public override IWidgetComponent GetWidgetComponent(WidgetId widgetId)
	{
		return widgetId switch
		{
			WidgetId.LayoutElement => base.Instance._layoutElementWidget, 
			WidgetId.InputField => base.Instance._inputFieldWidget, 
			WidgetId.Slider => base.Instance._sliderWidget, 
			WidgetId.Label => base.Instance._keyTextWidget, 
			WidgetId.ValueSuffixLabel => base.Instance._valueSuffixTextWidget, 
			_ => throw new NotImplementedException(), 
		};
	}

	public static SliderInputPanel LuaNew()
	{
		return new SliderInputPanel(luaDriven: true);
	}

	public static SliderInputPanel New(Action<SliderInputPanelComponent> onReset = null, Action<SliderInputPanelComponent> onRefresh = null)
	{
		return new SliderInputPanel(onReset, onRefresh);
	}
}
