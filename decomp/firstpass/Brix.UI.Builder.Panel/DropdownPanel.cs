using System;
using Brix.External.Factories;
using Brix.UI.Builder.Menu;

namespace Brix.UI.Builder.Panel;

public class DropdownPanel : BaseWidgetPanel<DropdownPanelComponent, DropdownPanel.WidgetId>
{
	public enum BinId
	{
		Default
	}

	public enum WidgetId
	{
		LayoutElement,
		ArrowImage,
		ValueCheckImage,
		ValueImage,
		ValueLabel,
		ValueTemplateImage,
		ValueTemplateLabel,
		Dropdown
	}

	public enum Variant
	{
		Standard,
		StandardImage,
		StandardBig,
		StandardBigReskin,
		StandardShort
	}

	private Factory.AssetKey _assetKey = Buttons.Dropdown;

	protected override Factory.AssetKey AssetKey => _assetKey;

	public DropdownPanel(bool luaDriven = false)
		: base((Action<DropdownPanelComponent>)null, (Action<DropdownPanelComponent>)null, luaDriven)
	{
	}

	public DropdownPanel(Action<DropdownPanelComponent> onReset, Action<DropdownPanelComponent> onRefresh)
		: base(onReset, onRefresh, luaDriven: false)
	{
	}

	public override IWidgetComponent GetWidgetComponent(WidgetId widgetId)
	{
		return widgetId switch
		{
			WidgetId.LayoutElement => base.Instance._layoutElementWidget, 
			WidgetId.ArrowImage => base.Instance._arrowImageWidget, 
			WidgetId.ValueCheckImage => base.Instance._valueCheckImageWidget, 
			WidgetId.ValueImage => base.Instance._valueImageWidget, 
			WidgetId.ValueLabel => base.Instance._valueLabelWidget, 
			WidgetId.ValueTemplateImage => base.Instance._valueTemplateImageWidget, 
			WidgetId.ValueTemplateLabel => base.Instance._valueTemplateLabelWidget, 
			WidgetId.Dropdown => base.Instance._dropdownWidget, 
			_ => throw new NotImplementedException(), 
		};
	}

	public DropdownPanel As(Variant variant)
	{
		switch (variant)
		{
		case Variant.Standard:
			_assetKey = Buttons.Dropdown;
			break;
		case Variant.StandardImage:
			_assetKey = Buttons.DropdownImage;
			break;
		case Variant.StandardBig:
			_assetKey = Buttons.DropdownBig;
			break;
		case Variant.StandardBigReskin:
			_assetKey = Buttons.DropdownBigReskin;
			break;
		case Variant.StandardShort:
			_assetKey = Buttons.DropdownShort;
			break;
		default:
			throw new NotImplementedException();
		}
		return this;
	}

	public static DropdownPanel LuaNew()
	{
		return new DropdownPanel(luaDriven: true);
	}

	public static DropdownPanel New(Action<DropdownPanelComponent> onReset = null, Action<DropdownPanelComponent> onRefresh = null)
	{
		return new DropdownPanel(onReset, onRefresh);
	}
}
