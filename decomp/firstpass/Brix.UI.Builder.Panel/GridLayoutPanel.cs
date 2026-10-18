using System;
using Brix.External.Factories;
using Brix.UI.Builder.Menu;
using UnityEngine;

namespace Brix.UI.Builder.Panel;

public class GridLayoutPanel : BaseLayoutWidgetPanel<GridLayoutPanelComponent, GridLayoutPanel.WidgetId>
{
	public enum WidgetId
	{
		LayoutElement,
		LayoutGroup,
		Image
	}

	private readonly Factory.AssetKey _assetKey = Layouts.Grid;

	protected override Factory.AssetKey AssetKey => _assetKey;

	public GridLayoutPanel(bool luaDriven = false)
		: base((Action<GridLayoutPanelComponent>)null, (Action<GridLayoutPanelComponent>)null, luaDriven)
	{
	}

	public GridLayoutPanel(Action<GridLayoutPanelComponent> onReset, Action<GridLayoutPanelComponent> onRefresh)
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

	public override RectTransform GetLayoutTrs()
	{
		return base.Instance._bin;
	}

	public static GridLayoutPanel LuaNew()
	{
		return new GridLayoutPanel(luaDriven: true);
	}

	public static GridLayoutPanel New(Action<GridLayoutPanelComponent> onReset = null, Action<GridLayoutPanelComponent> onRefresh = null)
	{
		return new GridLayoutPanel(onReset, onRefresh);
	}
}
