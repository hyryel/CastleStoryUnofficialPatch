using System;
using Brix.External.Factories;
using Brix.UI.Builder.Menu;
using UnityEngine;

namespace Brix.UI.Builder.Panel;

public class VerticalLayoutPanel : BaseLayoutWidgetPanel<VerticalLayoutPanelComponent, VerticalLayoutPanel.WidgetId>
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

	private Factory.AssetKey _assetKey = Layouts.Vertical;

	protected override Factory.AssetKey AssetKey => _assetKey;

	public VerticalLayoutPanel(bool luaDriven = false)
		: base((Action<VerticalLayoutPanelComponent>)null, (Action<VerticalLayoutPanelComponent>)null, luaDriven)
	{
	}

	public VerticalLayoutPanel(Action<VerticalLayoutPanelComponent> onReset, Action<VerticalLayoutPanelComponent> onRefresh)
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

	public VerticalLayoutPanel As(Variant variant)
	{
		switch (variant)
		{
		case Variant.Standard:
			_assetKey = Layouts.Vertical;
			break;
		case Variant.Root:
			_assetKey = Layouts.VerticalRoot;
			break;
		case Variant.Library:
			_assetKey = Layouts.VerticalLibrary;
			break;
		}
		return this;
	}

	public override RectTransform GetLayoutTrs()
	{
		return base.Instance._bin;
	}

	public static VerticalLayoutPanel LuaNew()
	{
		return new VerticalLayoutPanel(luaDriven: true);
	}

	public static VerticalLayoutPanel New(Action<VerticalLayoutPanelComponent> onReset = null, Action<VerticalLayoutPanelComponent> onRefresh = null)
	{
		return new VerticalLayoutPanel(onReset, onRefresh);
	}
}
