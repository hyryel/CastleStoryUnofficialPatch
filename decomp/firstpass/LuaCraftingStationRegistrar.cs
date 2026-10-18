using Brix.Engine;
using Brix.Game.Components;
using Brix.Utils.Signals;

public class LuaCraftingStationRegistrar : GameComponent
{
	protected override void Start()
	{
		base.Start();
		GameSignals.LateConnect(UIGameObserver.IsReady, ConnectSignals).DisconnectWhen(base.OnReleaseOrDestroySignal);
	}

	private void ConnectSignals()
	{
		UIGameObserver.crafting.SelectionChanged.Connect(CraftingStation.lua_UpdateSelection).DisconnectWhen(base.OnReleaseOrDestroySignal);
	}
}
