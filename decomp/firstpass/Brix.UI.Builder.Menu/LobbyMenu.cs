using System;
using System.Collections.Generic;
using Brix.Assets;
using Brix.External.Signals;
using Brix.Network;
using Brix.UI.Builder.Panel;
using Brix.Utils;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public sealed class LobbyMenu : BaseMenu
{
	private static readonly Action<PanelList, BaseMenu> GetPanelsAction = delegate(PanelList panels, BaseMenu menu)
	{
		LobbyPanel item = LobbyPanelComponent.Build();
		panels.Add(item);
	};

	private BaseMenu _titleMenu;

	private static bool _isMapLibrary;

	public static Signal OpenSignal = new Signal();

	public static Signal CloseSignal = new Signal();

	public static Signal RefreshSignal = new Signal();

	public static Signal OnToggleMapLibrarySignal = new Signal();

	private static readonly Action<LobbyPlayer, LobbyMenu> OnServerPlayerKicked = delegate(LobbyPlayer player, LobbyMenu menu)
	{
		LobbyPlayer player2 = Player;
		if (!(player != player2))
		{
			Leave();
			NetworkMessageModalMenu.Open2Signal.Invoke(I2Helper.TryGet("str_network_failure_playerkicked_title"), I2Helper.TryGet("str_network_failure_content"));
		}
	};

	private static readonly Action OnServerStopped = delegate
	{
		Leave();
		NetworkMessageModalMenu.Open2Signal.Invoke(I2Helper.TryGet("str_network_failure_serverstopped_title"), I2Helper.TryGet("str_network_failure_content"));
	};

	private static readonly Action<LobbyPlayer> OnPlayerJoined = delegate
	{
		SetEveryoneNotReady();
	};

	private static readonly Action<LobbyPlayer> OnPlayerLeft = delegate
	{
		SetEveryoneNotReady();
	};

	private static readonly Action OnSetMap = delegate
	{
		SetEveryoneNotReady();
	};

	private static readonly Action OnSetMode = delegate
	{
		SetEveryoneNotReady();
	};

	private static readonly Action OnSetPreset = delegate
	{
		SetEveryoneNotReady();
	};

	private static readonly Action OnSetSelectedMap = delegate
	{
		SetEveryoneNotReady();
	};

	private static readonly Action OnSelectOperation = delegate
	{
		TrySetSelectedAsLoaded();
	};

	private static readonly Action<LobbyMenu> OnSetIsHostReady = delegate(LobbyMenu menu)
	{
		menu.Refresh();
	};

	private static readonly Action<LobbyMenu> OnUpdate = delegate(LobbyMenu menu)
	{
		menu.Refresh();
	};

	public static List<LobbyPlayer> Players => UNetManager.instance.LobbyPlayerList;

	public static LobbyPlayer Player => UNetManager.instance.LocalLobbyPlayer;

	public static bool IsDownloading
	{
		get
		{
			if (Player == null)
			{
				return false;
			}
			return !(-1f).Equals(Player.downloadProgression);
		}
	}

	public static bool IsAnyPlayerDownloading
	{
		get
		{
			foreach (LobbyPlayer player in Players)
			{
				if (player == null || (-1f).Equals(player.downloadProgression))
				{
					continue;
				}
				return true;
			}
			return false;
		}
	}

	public static bool IsAnyClientNotReady
	{
		get
		{
			foreach (LobbyPlayer player in Players)
			{
				if (player == null || player.isHost || player.readyToBegin)
				{
					continue;
				}
				return true;
			}
			return false;
		}
	}

	public static bool CanSetReady
	{
		get
		{
			if (GameParam.Map == null)
			{
				return false;
			}
			if (GameParam.Gamemode == null)
			{
				return false;
			}
			if (GameParam.Preset == null)
			{
				return false;
			}
			if (Player == null)
			{
				return false;
			}
			return UNetManager.instance.CanLobbyPlayerSetReady(Player);
		}
	}

	public static bool IsHost => UNetManager.instance != null && UNetManager.instance.mode == UNetManager.Mode.Host;

	public static bool IsMapLibrary
	{
		get
		{
			return _isMapLibrary;
		}
		private set
		{
			if (_isMapLibrary != value)
			{
				_isMapLibrary = value;
				OnToggleMapLibrary();
			}
		}
	}

	protected override bool ReloadOnOpen => true;

	private LobbyMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static LobbyMenu Build(Func<RectTransform> binGetter)
	{
		return new LobbyMenu(binGetter);
	}

	protected override MenuHandle AddMenus()
	{
		return base.AddMenus().AddMenu(_titleMenu);
	}

	protected override PanelHandle AddPanels()
	{
		return base.AddPanels().AddPanelsNoLeak(GetPanelsAction, this);
	}

	public PanelList GetHeaderPanels()
	{
		PanelList panelList = PanelList.Request();
		Action<HorizontalLayoutPanelComponent> onReset = delegate(HorizontalLayoutPanelComponent panel)
		{
			RectOffset padding = panel._layoutGroup.padding;
			padding.bottom = 0;
			padding.top = 0;
			padding.left = 0;
			padding.right = 0;
			panel._layoutGroup.padding = padding;
			panel._layoutGroup.spacing = 0f;
			panel._image.enabled = false;
			panel._layoutElement.enabled = true;
			panel._layoutElement.preferredWidth = 270f;
		};
		HorizontalLayoutPanel panelN = HorizontalLayoutPanel.New(onReset);
		_titleMenu = SidebarTitleMenu.Build(() => panelN.GetLayoutTrs(), "Lobby", hasCloseButton: false);
		panelList.Add(panelN);
		return panelList;
	}

	public static void StartGame()
	{
		if (Player.isHost)
		{
			Player.SendReadyToBeginMessage();
		}
	}

	public static bool GetIsReady(LobbyPlayer player = null)
	{
		player = player ?? Player;
		if (player == null)
		{
			return false;
		}
		if (player.isHost)
		{
			return player.IsHostReady;
		}
		return player.readyToBegin;
	}

	public static void SetIsReady(bool value, LobbyPlayer player = null)
	{
		player = player ?? Player;
		if (player == null)
		{
			return;
		}
		if (player.isHost)
		{
			player.NetworkIsHostReady = value;
			UNetManager.instance.AbortUploads();
			if (value)
			{
				UNetManager.instance.SendMapToClients();
			}
			RefreshSignal.Invoke();
			NetworkPlayersMenu.RefreshSignal.Invoke();
		}
		else
		{
			player.NetworkIsHostReady = false;
			if (value)
			{
				player.SendReadyToBeginMessage();
			}
			else
			{
				player.SendNotReadyToBeginMessage();
			}
		}
	}

	public static bool CanHostPressStart()
	{
		return (from player in Players.Slinq()
			where player.readyToBegin
			select player).Count() >= Players.Count - 1;
	}

	private static void Leave()
	{
		UNetManager.instance.ResetPersistantNetworkSceneObjects();
		UNetManager.instance.Disconnect();
		CloseLobby();
	}

	private static void CloseLobby()
	{
		NetworkWaitingModalMenu.CloseSignal.Invoke();
		NetworkCustomServerModalMenu.CloseSignal.Invoke();
		CloseSignal.Invoke();
	}

	public static void ToggleMapLibrary()
	{
		IsMapLibrary = !IsMapLibrary;
	}

	private static void OnToggleMapLibrary()
	{
		if (IsMapLibrary)
		{
			NetworkPlayersMenu.HideSignal.Invoke();
			NetworkChatMenu.HideSignal.Invoke();
		}
		else
		{
			NetworkPlayersMenu.ShowSignal.Invoke();
			NetworkChatMenu.ShowSignal.Invoke();
		}
		TrySetSelectedAsLoaded();
		OnToggleMapLibrarySignal.Invoke();
	}

	public static void CloseMapLibrary()
	{
		IsMapLibrary = false;
	}

	private static void TrySetSelectedAsLoaded()
	{
		if (IsHost && !IsMapLibrary)
		{
			BaseMapLibraryMenu.SetSelectedAsLoaded();
		}
	}

	private static void SetEveryoneNotReady()
	{
		if (IsHost)
		{
			SetIsReady(value: false);
			UNetManager.instance.SetAllClientsAsNotReady();
		}
	}

	protected override void OpenSelf()
	{
		base.OpenSelf();
		if (Player != null && Player.isHost)
		{
			Player.NetworkIsHostReady = false;
			List<Asset_Map> list = AssetDispensary.maps.SendQuery(AssetQueryParameters.Official, AssetPlayableContext.Multiplayer);
			if (list.Count > 0)
			{
				list = (from x in list.Slinq()
					orderby x.Rating descending
					select x).ToList();
				Asset_Map map = list[0];
				foreach (Asset_Map item in list)
				{
					if (item.IsCurrentVersion())
					{
						map = item;
						break;
					}
				}
				BaseMapLibraryMenu.SetSelectedMap(map, AssetPlayableContext.Multiplayer, BaseAssetMenu.MenuGameContext.Normal, BaseAssetMenu.MenuTypeContext.New);
				BaseMapLibraryMenu.SetSelectedAsLoaded();
			}
		}
		PersistentCoroutineRunner.Relais(delegate
		{
			IsMapLibrary = false;
		});
	}

	protected override void CloseSelf()
	{
		base.CloseSelf();
		NetworkPlayersMenu.CloseSignal.Invoke();
		NetworkChatMenu.CloseSignal.Invoke();
	}

	protected override SignalHandle<MenuState> AddSignals()
	{
		return base.AddSignals().AddSignalConnection(MenuState.Loaded, OpenSignal, BaseMenu.OnOpen, this).AddSignalConnection(MenuState.Opened, CloseSignal, BaseMenu.OnClose, this)
			.AddSignalConnection(MenuState.Opened, RefreshSignal, BaseMenu.OnRefresh, this)
			.AddSignalConnection(MenuState.Opened, UNetManager.ServerPlayerKicked, OnServerPlayerKicked, this)
			.AddSignalConnection(MenuState.Opened, UNetManager.ServerStopped, OnServerStopped)
			.AddSignalConnection(MenuState.Opened, PersistentCoroutineRunner.UpdateSignal, OnUpdate, this)
			.AddSignalConnection(MenuState.Opened, UNetManager.LobbyPlayerJoined, OnPlayerJoined)
			.AddSignalConnection(MenuState.Opened, UNetManager.LobbyPlayerLeft, OnPlayerLeft)
			.AddSignalConnection(MenuState.Opened, UNetManager.OnSetIsHostReady, OnSetIsHostReady, this)
			.AddSignalConnection(MenuState.Opened, GameParam.OnSetMapSignal, OnSetMap)
			.AddSignalConnection(MenuState.Opened, GameParam.OnSetModeSignal, OnSetMode)
			.AddSignalConnection(MenuState.Opened, GameParam.OnSetPresetSignal, OnSetPreset)
			.AddSignalConnection(MenuState.Opened, GameParam.OnSetSelectedMapSignal, OnSetSelectedMap)
			.AddSignalConnection(MenuState.Opened, LegacyLeftLobbyMenu.OnSelectOperationSignal, OnSelectOperation);
	}
}
