using System;
using Brix.External.Factories;
using Brix.UI.Builder.Menu;
using UnityEngine;

namespace Brix.UI.Builder.Panel;

public class SquareButtonPanel : BaseLayoutWidgetPanel<SquareButtonPanelComponent, SquareButtonPanel.WidgetId>
{
	public enum WidgetId
	{
		LayoutElement,
		Button,
		ImageLabel,
		HoverBgImage,
		HoverImage,
		HoverLabel,
		StateAnimator,
		FrameAnimator,
		Hotkey
	}

	public enum Variant
	{
		Default
	}

	private Factory.AssetKey _assetKey = Buttons.Square;

	protected override Factory.AssetKey AssetKey => _assetKey;

	public SquareButtonPanel(bool luaDriven = false)
		: base((Action<SquareButtonPanelComponent>)null, (Action<SquareButtonPanelComponent>)null, luaDriven)
	{
	}

	public SquareButtonPanel(Action<SquareButtonPanelComponent> onReset, Action<SquareButtonPanelComponent> onRefresh)
		: base(onReset, onRefresh, luaDriven: false)
	{
	}

	public override IWidgetComponent GetWidgetComponent(WidgetId widgetId)
	{
		return widgetId switch
		{
			WidgetId.Button => base.Instance._buttonWidget, 
			WidgetId.ImageLabel => base.Instance._spriteLabelWidget, 
			WidgetId.StateAnimator => base.Instance._stateAnimatorWidget, 
			WidgetId.FrameAnimator => base.Instance._frameAnimatorWidget, 
			WidgetId.Hotkey => base.Instance._hotkeyWidget, 
			WidgetId.HoverBgImage => base.Instance._hoverBgImageWidget, 
			WidgetId.HoverImage => base.Instance._hoverImageWidget, 
			WidgetId.HoverLabel => base.Instance._hoverLabelWidget, 
			WidgetId.LayoutElement => base.Instance._layoutElement, 
			_ => throw new NotImplementedException(), 
		};
	}

	public SquareButtonPanel As(Variant variant)
	{
		if (variant == Variant.Default)
		{
			_assetKey = Buttons.Square;
		}
		return this;
	}

	public override RectTransform GetLayoutTrs()
	{
		return base.Instance._bin;
	}

	public static SquareButtonPanel LuaNew()
	{
		return new SquareButtonPanel(luaDriven: true);
	}

	public static SquareButtonPanel New(Action<SquareButtonPanelComponent> onReset = null, Action<SquareButtonPanelComponent> onRefresh = null)
	{
		return new SquareButtonPanel(onReset, onRefresh);
	}
}
