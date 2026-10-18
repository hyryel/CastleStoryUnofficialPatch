using System;
using Brix.External.Factories;
using Brix.UI.Builder.Menu;

namespace Brix.UI.Builder.Panel;

public class LuaInputFieldPanel : BaseWidgetPanel<LuaInputFieldPanelComponent, LuaInputFieldPanel.WidgetId>
{
	public enum WidgetId
	{
		LayoutElement,
		Image,
		InputField,
		Label,
		PlaceholderLabel,
		FrameAnimator
	}

	public enum Variant
	{
		Default
	}

	private Factory.AssetKey _assetKey = InputFields.LuaShort;

	protected override Factory.AssetKey AssetKey => _assetKey;

	public LuaInputFieldPanel(bool luaDriven = false)
		: base((Action<LuaInputFieldPanelComponent>)null, (Action<LuaInputFieldPanelComponent>)null, luaDriven)
	{
	}

	public LuaInputFieldPanel(Action<LuaInputFieldPanelComponent> onReset, Action<LuaInputFieldPanelComponent> onRefresh)
		: base(onReset, onRefresh, luaDriven: false)
	{
	}

	public override IWidgetComponent GetWidgetComponent(WidgetId widgetId)
	{
		return widgetId switch
		{
			WidgetId.LayoutElement => base.Instance._layoutElementWidget, 
			WidgetId.Image => base.Instance._imageWidget, 
			WidgetId.InputField => base.Instance._inputFieldWidget, 
			WidgetId.Label => base.Instance._labelWidget, 
			WidgetId.PlaceholderLabel => base.Instance._placeholderLabelWidget, 
			WidgetId.FrameAnimator => base.Instance._frameAnimatorWidget, 
			_ => throw new NotImplementedException(), 
		};
	}

	public LuaInputFieldPanel As(Variant variant)
	{
		if (variant == Variant.Default)
		{
			_assetKey = InputFields.LuaShort;
		}
		return this;
	}

	public static LuaInputFieldPanel LuaNew()
	{
		return new LuaInputFieldPanel(luaDriven: true);
	}

	public static LuaInputFieldPanel New(Action<LuaInputFieldPanelComponent> onReset = null, Action<LuaInputFieldPanelComponent> onRefresh = null)
	{
		return new LuaInputFieldPanel(onReset, onRefresh);
	}
}
