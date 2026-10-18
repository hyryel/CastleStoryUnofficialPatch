using System;
using Brix.External.Factories;
using Brix.UI.Builder.Menu;

namespace Brix.UI.Builder.Panel;

public class LuaDropdownPanel : BaseWidgetPanel<LuaDropdownPanelComponent, LuaDropdownPanel.WidgetId>
{
	public enum BinId
	{
		Default
	}

	public enum WidgetId
	{
		LayoutElement,
		ValueLayoutElement,
		Image,
		ArrowImage,
		ValueCheckImage,
		ValueImage,
		ValueLabel,
		ValueTemplateImage,
		ValueTemplateLabel,
		ValueTemplateBgImage,
		ListBgImage,
		Dropdown
	}

	public enum Variant
	{
		Default
	}

	private Factory.AssetKey _assetKey = Buttons.Dropdown;

	protected override Factory.AssetKey AssetKey => _assetKey;

	public LuaDropdownPanel(bool luaDriven = false)
		: base((Action<LuaDropdownPanelComponent>)null, (Action<LuaDropdownPanelComponent>)null, luaDriven)
	{
	}

	public LuaDropdownPanel(Action<LuaDropdownPanelComponent> onReset, Action<LuaDropdownPanelComponent> onRefresh)
		: base(onReset, onRefresh, luaDriven: false)
	{
	}

	public override IWidgetComponent GetWidgetComponent(WidgetId widgetId)
	{
		return widgetId switch
		{
			WidgetId.LayoutElement => base.Instance._layoutElementWidget, 
			WidgetId.ValueLayoutElement => base.Instance._valueLayoutElementWidget, 
			WidgetId.Image => base.Instance._imageWidget, 
			WidgetId.ArrowImage => base.Instance._arrowImageWidget, 
			WidgetId.ValueCheckImage => base.Instance._valueCheckImageWidget, 
			WidgetId.ValueImage => base.Instance._valueImageWidget, 
			WidgetId.ValueLabel => base.Instance._valueLabelWidget, 
			WidgetId.ValueTemplateImage => base.Instance._valueTemplateImageWidget, 
			WidgetId.ValueTemplateLabel => base.Instance._valueTemplateLabelWidget, 
			WidgetId.ValueTemplateBgImage => base.Instance._valueTemplateBgImageWidget, 
			WidgetId.ListBgImage => base.Instance._listBgImageWidget, 
			WidgetId.Dropdown => base.Instance._dropdownWidget, 
			_ => throw new NotImplementedException(), 
		};
	}

	public LuaDropdownPanel As(Variant variant)
	{
		if (variant == Variant.Default)
		{
			_assetKey = Buttons.LuaDropdown;
			return this;
		}
		throw new NotImplementedException();
	}

	public static LuaDropdownPanel LuaNew()
	{
		return new LuaDropdownPanel(luaDriven: true);
	}

	public static LuaDropdownPanel New(Action<LuaDropdownPanelComponent> onReset = null, Action<LuaDropdownPanelComponent> onRefresh = null)
	{
		return new LuaDropdownPanel(onReset, onRefresh);
	}
}
