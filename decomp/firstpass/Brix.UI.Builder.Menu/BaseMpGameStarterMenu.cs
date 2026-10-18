using System;
using Brix.Assets;
using Brix.Network;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public abstract class BaseMpGameStarterMenu : BaseMapStarterMenu
{
	private static readonly Action<LobbyPlayer, BaseMpGameStarterMenu> OnPlayerJoinOrLeave = delegate(LobbyPlayer player, BaseMpGameStarterMenu menu)
	{
		menu.Refresh();
	};

	protected override bool LoadOnInit => true;

	protected override bool ReloadOnOpen => false;

	protected override bool LoadOnOpen => false;

	protected override bool AsModal => false;

	protected override bool IsReskin => true;

	protected override bool ThumbnailIsHeightmap => TypeContext == MenuTypeContext.New;

	protected override bool ThumbnailShowsBg => true;

	protected override bool IsLobby => true;

	protected override AssetPlayableContext PlayableContext => AssetPlayableContext.Multiplayer;

	protected override AssetQueryParameters QueryParameters => AssetQueryParameters.AllSources;

	protected override MenuPlayabilityContext PlayabilityContext => MenuPlayabilityContext.Multi;

	protected override MenuOperationContext OperationContext => MenuOperationContext.Load;

	protected override MenuGameContext GameContext => MenuGameContext.Normal;

	protected BaseMpGameStarterMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	protected override void OpenSelf()
	{
		base.OpenSelf();
		OnAfterSetMap();
	}

	protected override SignalHandle<MenuState> AddSignals()
	{
		return base.AddSignals().AddSignalConnection(MenuState.Opened, UNetManager.OnSetIsHostReady, BaseMenu.OnRefresh, this).AddSignalConnection(MenuState.Opened, UNetManager.ServerPlayerKicked, OnPlayerJoinOrLeave, this)
			.AddSignalConnection(MenuState.Opened, UNetManager.LobbyPlayerJoined, OnPlayerJoinOrLeave, this)
			.AddSignalConnection(MenuState.Opened, UNetManager.LobbyPlayerLeft, OnPlayerJoinOrLeave, this)
			.AddSignalConnection(MenuState.Opened, LobbyMenu.OnToggleMapLibrarySignal, BaseMenu.OnRefresh, this);
	}
}
