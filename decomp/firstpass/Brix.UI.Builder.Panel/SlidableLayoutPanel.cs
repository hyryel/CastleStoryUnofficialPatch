using System;
using Brix.External.Factories;
using Brix.UI.Builder.Menu;
using UnityEngine;

namespace Brix.UI.Builder.Panel;

public class SlidableLayoutPanel : BaseLayoutWidgetPanel<SlidableLayoutPanelComponent, SlidableLayoutPanel.WidgetId>
{
	public enum WidgetId
	{
		LayoutElement,
		Slidable,
		SlidableLayoutElement,
		SlidableLayoutGroup,
		SlidableImage
	}

	public enum Variant
	{
		Top,
		Bottom,
		Left,
		Right
	}

	private Factory.AssetKey _assetKey = Layouts.SlidableTop;

	protected override Factory.AssetKey AssetKey => _assetKey;

	public SlidableLayoutPanel(bool luaDriven = false)
		: base((Action<SlidableLayoutPanelComponent>)null, (Action<SlidableLayoutPanelComponent>)null, luaDriven)
	{
	}

	public SlidableLayoutPanel(Action<SlidableLayoutPanelComponent> onReset, Action<SlidableLayoutPanelComponent> onRefresh)
		: base(onReset, onRefresh, luaDriven: false)
	{
	}

	public override IWidgetComponent GetWidgetComponent(WidgetId widgetId)
	{
		return widgetId switch
		{
			WidgetId.LayoutElement => base.Instance._layoutElementWidget, 
			WidgetId.Slidable => base.Instance._slidableWidget, 
			WidgetId.SlidableLayoutElement => base.Instance._slidableLayoutElementWidget, 
			WidgetId.SlidableLayoutGroup => base.Instance._slidableLayoutGroupWidget, 
			WidgetId.SlidableImage => base.Instance._slidableImageWidget, 
			_ => throw new NotImplementedException(), 
		};
	}

	public SlidableLayoutPanel As(Variant variant)
	{
		switch (variant)
		{
		case Variant.Top:
			_assetKey = Layouts.SlidableTop;
			break;
		case Variant.Bottom:
			_assetKey = Layouts.SlidableBottom;
			break;
		case Variant.Left:
			_assetKey = Layouts.SlidableLeft;
			break;
		case Variant.Right:
			_assetKey = Layouts.SlidableRight;
			break;
		}
		return this;
	}

	public override RectTransform GetLayoutTrs()
	{
		return base.Instance._socket;
	}

	public static SlidableLayoutPanel LuaNew()
	{
		return new SlidableLayoutPanel(luaDriven: true);
	}

	public static SlidableLayoutPanel New(Action<SlidableLayoutPanelComponent> onReset = null, Action<SlidableLayoutPanelComponent> onRefresh = null)
	{
		return new SlidableLayoutPanel(onReset, onRefresh);
	}
}
