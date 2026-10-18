using System;
using Brix.External.Factories;
using Brix.UI.Builder.Menu;
using UnityEngine;

namespace Brix.UI.Builder.Panel;

public class HorizontalLayoutPanel : BaseLayoutWidgetPanel<HorizontalLayoutPanelComponent, HorizontalLayoutPanel.WidgetId>
{
	public enum WidgetId
	{
		LayoutElement,
		LayoutGroup,
		Image
	}

	public enum Variant
	{
		Standard,
		Root,
		Library
	}

	private Factory.AssetKey _assetKey = Layouts.Horizontal;

	protected override Factory.AssetKey AssetKey => _assetKey;

	public HorizontalLayoutPanel(bool luaDriven = false)
		: base((Action<HorizontalLayoutPanelComponent>)null, (Action<HorizontalLayoutPanelComponent>)null, luaDriven)
	{
	}

	public HorizontalLayoutPanel(Action<HorizontalLayoutPanelComponent> onReset, Action<HorizontalLayoutPanelComponent> onRefresh)
		: base(onReset, onRefresh, luaDriven: false)
	{
	}

	public override IWidgetComponent GetWidgetComponent(WidgetId widgetId)
	{
		return widgetId switch
		{
			WidgetId.LayoutGroup => base.Instance._layoutGroupWidget, 
			WidgetId.LayoutElement => base.Instance._layoutElementWidget, 
			WidgetId.Image => base.Instance._imageWidget, 
			_ => throw new NotImplementedException(), 
		};
	}

	public HorizontalLayoutPanel As(Variant variant)
	{
		switch (variant)
		{
		case Variant.Standard:
			_assetKey = Layouts.Horizontal;
			break;
		case Variant.Root:
			_assetKey = Layouts.HorizontalRoot;
			break;
		}
		return this;
	}

	public override RectTransform GetLayoutTrs()
	{
		return base.Instance._bin;
	}

	public static HorizontalLayoutPanel LuaNew()
	{
		return new HorizontalLayoutPanel(luaDriven: true);
	}

	public static HorizontalLayoutPanel New(Action<HorizontalLayoutPanelComponent> onReset = null, Action<HorizontalLayoutPanelComponent> onRefresh = null)
	{
		return new HorizontalLayoutPanel(onReset, onRefresh);
	}
}
