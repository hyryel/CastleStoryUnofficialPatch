using System;
using Brix.External.Factories;
using Brix.UI.Builder.Menu;
using UnityEngine;

namespace Brix.UI.Builder.Panel;

public class LobbyPanel : BaseLayoutWidgetPanel<LobbyPanelComponent, LobbyPanel.WidgetId>
{
	public enum WidgetId
	{

	}

	public enum Variant
	{
		Standard
	}

	private Factory.AssetKey _assetKey = Panels.Lobby;

	protected override Factory.AssetKey AssetKey => _assetKey;

	public LobbyPanel(bool luaDriven = false)
		: base((Action<LobbyPanelComponent>)null, (Action<LobbyPanelComponent>)null, luaDriven)
	{
	}

	public LobbyPanel(Action<LobbyPanelComponent> onReset, Action<LobbyPanelComponent> onRefresh)
		: base(onReset, onRefresh, luaDriven: false)
	{
	}

	public override IWidgetComponent GetWidgetComponent(WidgetId widgetId)
	{
		throw new NotImplementedException();
	}

	public LobbyPanel As(Variant variant)
	{
		if (variant == Variant.Standard)
		{
			_assetKey = Panels.Lobby;
		}
		return this;
	}

	public override RectTransform GetLayoutTrs()
	{
		throw new NotImplementedException();
	}

	public static LobbyPanel LuaNew()
	{
		return new LobbyPanel(luaDriven: true);
	}

	public static LobbyPanel New(Action<LobbyPanelComponent> onReset = null, Action<LobbyPanelComponent> onRefresh = null)
	{
		return new LobbyPanel(onReset, onRefresh);
	}
}
