using System;
using Brix.External.Factories;
using Brix.UI.Builder.Menu;
using UnityEngine;

namespace Brix.UI.Builder.Panel;

public class PlayerEntryPanel : BaseLayoutWidgetPanel<PlayerEntryPanelComponent, PlayerEntryPanel.WidgetId>
{
	public enum WidgetId
	{

	}

	public enum Variant
	{
		Standard
	}

	private Factory.AssetKey _assetKey = Panels.PlayerEntry;

	protected override Factory.AssetKey AssetKey => _assetKey;

	public PlayerEntryPanel(bool luaDriven = false)
		: base((Action<PlayerEntryPanelComponent>)null, (Action<PlayerEntryPanelComponent>)null, luaDriven)
	{
	}

	public PlayerEntryPanel(Action<PlayerEntryPanelComponent> onReset, Action<PlayerEntryPanelComponent> onRefresh)
		: base(onReset, onRefresh, luaDriven: false)
	{
	}

	public override IWidgetComponent GetWidgetComponent(WidgetId widgetId)
	{
		throw new NotImplementedException();
	}

	public PlayerEntryPanel As(Variant variant)
	{
		if (variant == Variant.Standard)
		{
			_assetKey = Panels.PlayerEntry;
		}
		return this;
	}

	public override RectTransform GetLayoutTrs()
	{
		return base.Instance._playerBin;
	}

	public static PlayerEntryPanel LuaNew()
	{
		return new PlayerEntryPanel(luaDriven: true);
	}

	public static PlayerEntryPanel New(Action<PlayerEntryPanelComponent> onReset = null, Action<PlayerEntryPanelComponent> onRefresh = null)
	{
		return new PlayerEntryPanel(onReset, onRefresh);
	}
}
