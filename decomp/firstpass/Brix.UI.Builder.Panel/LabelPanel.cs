using System;
using Brix.External.Factories;
using Brix.UI.Builder.Menu;

namespace Brix.UI.Builder.Panel;

public class LabelPanel : BaseWidgetPanel<LabelPanelComponent, LabelPanel.WidgetId>
{
	public enum WidgetId
	{
		LabelLayoutElement,
		Label
	}

	public enum Variant
	{
		Default
	}

	private Factory.AssetKey _assetKey = Texts.Default;

	protected override Factory.AssetKey AssetKey => _assetKey;

	public LabelPanel(bool luaDriven = false)
		: base((Action<LabelPanelComponent>)null, (Action<LabelPanelComponent>)null, luaDriven)
	{
	}

	public LabelPanel(Action<LabelPanelComponent> onReset, Action<LabelPanelComponent> onRefresh)
		: base(onReset, onRefresh, luaDriven: false)
	{
	}

	public override IWidgetComponent GetWidgetComponent(WidgetId widgetId)
	{
		return widgetId switch
		{
			WidgetId.LabelLayoutElement => base.Instance._labelLayoutElementWidget, 
			WidgetId.Label => base.Instance._labelWidget, 
			_ => throw new NotImplementedException(), 
		};
	}

	public LabelPanel As(Variant variant)
	{
		if (variant == Variant.Default)
		{
			_assetKey = Texts.Default;
		}
		return this;
	}

	public static LabelPanel LuaNew()
	{
		return new LabelPanel(luaDriven: true);
	}

	public static LabelPanel New(Action<LabelPanelComponent> onReset = null, Action<LabelPanelComponent> onRefresh = null)
	{
		return new LabelPanel(onReset, onRefresh);
	}
}
