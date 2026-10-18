using System;
using Brix.External.Factories;
using Brix.UI.Builder.Menu;
using UnityEngine;

namespace Brix.UI.Builder.Panel;

public class LuaScrollableLayoutPanel : BaseLayoutWidgetPanel<LuaScrollableLayoutPanelComponent, LuaScrollableLayoutPanel.WidgetId>
{
	public enum WidgetId
	{
		LayoutElement,
		Scrollable
	}

	public enum Variant
	{
		Default
	}

	private Factory.AssetKey _assetKey = Layouts.LuaScrollable;

	protected override Factory.AssetKey AssetKey => _assetKey;

	public LuaScrollableLayoutPanel(bool luaDriven = false)
		: base((Action<LuaScrollableLayoutPanelComponent>)null, (Action<LuaScrollableLayoutPanelComponent>)null, luaDriven)
	{
	}

	public LuaScrollableLayoutPanel(Action<LuaScrollableLayoutPanelComponent> onReset, Action<LuaScrollableLayoutPanelComponent> onRefresh)
		: base(onReset, onRefresh, luaDriven: false)
	{
	}

	public override IWidgetComponent GetWidgetComponent(WidgetId widgetId)
	{
		return widgetId switch
		{
			WidgetId.LayoutElement => base.Instance._layoutElementWidget, 
			WidgetId.Scrollable => base.Instance._scrollableWidget, 
			_ => throw new NotImplementedException(), 
		};
	}

	public LuaScrollableLayoutPanel As(Variant variant)
	{
		if (variant == Variant.Default)
		{
			_assetKey = Layouts.LuaScrollableRight;
			return this;
		}
		throw new NotImplementedException();
	}

	public static LuaScrollableLayoutPanel LuaNew()
	{
		return new LuaScrollableLayoutPanel(luaDriven: true);
	}

	public static LuaScrollableLayoutPanel New(Action<LuaScrollableLayoutPanelComponent> onReset = null, Action<LuaScrollableLayoutPanelComponent> onRefresh = null)
	{
		return new LuaScrollableLayoutPanel(onReset, onRefresh);
	}

	public override RectTransform GetLayoutTrs()
	{
		return base.Instance._bin;
	}
}
