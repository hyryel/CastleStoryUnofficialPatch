using System;
using Brix.External.Factories;
using UnityEngine;

namespace Brix.UI.Builder.Panel;

public class ScrollableLayoutPanel : BaseLayoutPanel<ScrollableLayoutPanelComponent>
{
	private readonly Factory.AssetKey _assetKey = Layouts.Scrollable;

	protected override Factory.AssetKey AssetKey => _assetKey;

	public ScrollableLayoutPanel(bool luaDriven = false)
		: base((Action<ScrollableLayoutPanelComponent>)null, (Action<ScrollableLayoutPanelComponent>)null, luaDriven)
	{
	}

	public ScrollableLayoutPanel(Action<ScrollableLayoutPanelComponent> onReset, Action<ScrollableLayoutPanelComponent> onRefresh)
		: base(onReset, onRefresh, luaDriven: false)
	{
	}

	public static ScrollableLayoutPanel LuaNew()
	{
		return new ScrollableLayoutPanel(luaDriven: true);
	}

	public static ScrollableLayoutPanel New(Action<ScrollableLayoutPanelComponent> onReset = null, Action<ScrollableLayoutPanelComponent> onRefresh = null)
	{
		return new ScrollableLayoutPanel(onReset, onRefresh);
	}

	public override RectTransform GetLayoutTrs()
	{
		return base.Instance._bin;
	}
}
