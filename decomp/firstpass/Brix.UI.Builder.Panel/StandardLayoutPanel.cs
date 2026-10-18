using System;
using Brix.External.Factories;
using UnityEngine;

namespace Brix.UI.Builder.Panel;

public class StandardLayoutPanel : BaseLayoutPanel<StandardLayoutPanelComponent>
{
	private readonly Factory.AssetKey _assetKey = Layouts.Standard;

	protected override Factory.AssetKey AssetKey => _assetKey;

	public StandardLayoutPanel(bool luaDriven = false)
		: base((Action<StandardLayoutPanelComponent>)null, (Action<StandardLayoutPanelComponent>)null, luaDriven)
	{
	}

	public StandardLayoutPanel(Action<StandardLayoutPanelComponent> onReset, Action<StandardLayoutPanelComponent> onRefresh)
		: base(onReset, onRefresh, luaDriven: false)
	{
	}

	public override RectTransform GetLayoutTrs()
	{
		return base.Instance._bin;
	}

	public static StandardLayoutPanel LuaNew()
	{
		return new StandardLayoutPanel(luaDriven: true);
	}

	public static StandardLayoutPanel New(Action<StandardLayoutPanelComponent> onReset = null, Action<StandardLayoutPanelComponent> onRefresh = null)
	{
		return new StandardLayoutPanel(onReset, onRefresh);
	}
}
