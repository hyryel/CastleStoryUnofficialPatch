using System;

namespace Brix.UI.Builder.Panel;

public class LobbyPanelComponent : BasePanelComponent
{
	public static LobbyPanel Build(Action<LobbyPanelComponent> onReset = null, Action<LobbyPanelComponent> onRefresh = null)
	{
		return new LobbyPanel(onReset, onRefresh);
	}
}
