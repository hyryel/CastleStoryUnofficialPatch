using System;
using System.Collections.Generic;
using Brix.External.Signals;
using Brix.Network;
using Brix.UI.Builder.Panel;
using Brix.UI.Icons;
using Brix.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.UI.Builder.Menu;

public sealed class NetworkPlayersMenu : BaseMenu
{
	private static bool _showsPlayers = true;

	private static readonly Action<LobbyPlayer, NetworkPlayersMenu> OnPlayerJoin = delegate(LobbyPlayer player, NetworkPlayersMenu menu)
	{
		GameParam.RefreshFactionsAndTeams();
		menu.Refresh();
	};

	private static readonly Action<LobbyPlayer, NetworkPlayersMenu> OnPlayerLeave = delegate(LobbyPlayer player, NetworkPlayersMenu menu)
	{
		menu.Refresh();
	};

	private static readonly Action<LobbyPlayer, NetworkPlayersMenu> OnPlayerKick = delegate(LobbyPlayer player, NetworkPlayersMenu menu)
	{
		menu.Refresh();
	};

	private static readonly Action<NetworkPlayersMenu> OnShow = delegate(NetworkPlayersMenu menu)
	{
		_showsPlayers = true;
		menu.Refresh();
	};

	private static readonly Action<NetworkPlayersMenu> OnHide = delegate(NetworkPlayersMenu menu)
	{
		_showsPlayers = false;
		menu.Refresh();
	};

	public static Signal ShowSignal = new Signal();

	public static Signal HideSignal = new Signal();

	public static Signal CloseSignal = new Signal();

	public static Signal OpenSignal = new Signal();

	public static readonly Signal RefreshSignal = new Signal();

	private NetworkPlayersMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static NetworkPlayersMenu Build(Func<RectTransform> binGetter)
	{
		return new NetworkPlayersMenu(binGetter);
	}

	protected override void OpenSelf()
	{
		_showsPlayers = true;
		base.OpenSelf();
	}

	protected override PanelHandle AddPanels()
	{
		return base.AddPanels().AddPanels(GetPlayerBuilders());
	}

	private PanelList GetPlayerBuilders()
	{
		PanelList panelList = PanelList.Request();
		for (int i = 0; i < 4; i++)
		{
			panelList.Add(GetPlayerTeamPanel(i));
			for (int j = 0; j < 4; j++)
			{
				panelList.Add(GetPlayerPanel(i, j));
			}
		}
		return panelList;
	}

	private IPanel GetPlayerTeamPanel(int teamIndex)
	{
		Action<TitleTextPanelComponent> onRefresh = delegate(TitleTextPanelComponent panel)
		{
			GameParam.TeamInfo[] currentTeamInfos = GameParam.CurrentTeamInfos;
			List<LobbyPlayer> teamPlayers = GameParam.GetTeamPlayers(teamIndex, CollectionPool.RequestList<LobbyPlayer>());
			int count = teamPlayers.Count;
			teamPlayers.Dispose();
			if (count == 0)
			{
				panel.gameObject.SetActive(value: false);
			}
			else if (!_showsPlayers)
			{
				panel.gameObject.SetActive(value: false);
			}
			else
			{
				panel.gameObject.SetActive(value: true);
				GameParam.TeamInfo teamInfo = currentTeamInfos[teamIndex];
				panel._label.text = I2Helper.TryGet(teamInfo.Name).ToUpper();
			}
		};
		Action<TitleTextPanelComponent> onReset = delegate
		{
		};
		return TitleTextPanelComponent.Build(onReset, onRefresh).IgnoreWidgets();
	}

	private IPanel GetPlayerPanel(int teamIndex, int playerIndex)
	{
		Action<ThumbnailPanelComponent> onRefresh = delegate(ThumbnailPanelComponent panel)
		{
			if (!_showsPlayers)
			{
				panel.gameObject.SetActive(value: false);
			}
			else
			{
				List<LobbyPlayer> teamPlayers = GameParam.GetTeamPlayers(teamIndex, CollectionPool.RequestList<LobbyPlayer>());
				if (playerIndex >= teamPlayers.Count)
				{
					panel.gameObject.SetActive(value: false);
					teamPlayers.Dispose();
				}
				else
				{
					LobbyPlayer playerN = teamPlayers[playerIndex];
					if (playerN == null)
					{
						panel.gameObject.SetActive(value: false);
						teamPlayers.Dispose();
					}
					else
					{
						teamPlayers.Dispose();
						panel.gameObject.SetActive(value: true);
						panel._playerNameText.text = playerN.playerName;
						panel._playerIsReadyText.text = GetReadyStatus(playerN);
						GameParam.FactionInfo[] currentFactionInfos = GameParam.CurrentFactionInfos;
						if (playerN.factionNumber < currentFactionInfos.Length)
						{
							panel._factionImage.sprite = currentFactionInfos[playerN.factionNumber].Icon;
							panel._factionImage.color = Colors.White;
						}
						else
						{
							panel._factionImage.sprite = null;
							panel._factionImage.color = new Color(0f, 0f, 0f, 0f);
						}
						bool flag = LobbyMenu.Player.Equals(playerN);
						panel._bgImage.color = ((!flag) ? Colors.Yellow : Colors.White);
						panel._avatarBgImage.color = ((!flag) ? Colors.Yellow : Colors.White);
						panel._avatarRawImage.texture = ((!playerN.SteamID.IsValid()) ? null : playerN.GetSmallSteamAvatar());
						panel._avatarRawImage.gameObject.SetActive(panel._avatarRawImage.texture != null);
						panel._avatarGo.SetActive(playerN.SteamID.IsValid());
						panel._avatarButton.onClick.AddListener(delegate
						{
							if (playerN.SteamID.IsValid())
							{
								playerN.OpenSteamProfile();
							}
						});
						if (playerN.isHost)
						{
							panel._button.gameObject.SetActive(value: false);
							panel._pingBgImage.gameObject.SetActive(value: false);
							panel._pingImage.sprite = IconKeys._UI_Crown.Get64();
							panel._downloadGo.SetActive(value: false);
						}
						else
						{
							panel._button.gameObject.SetActive(NetworkServer.active && LobbyMenu.IsHost);
							panel._pingBgImage.gameObject.SetActive(value: true);
							panel._pingImage.sprite = GetPingSprite(playerN.ping);
							bool flag2 = IsPlayerDownloading(playerN);
							panel._downloadGo.SetActive(flag2);
							if (flag2)
							{
								panel._downloadImage.fillAmount = playerN.downloadProgression;
							}
						}
					}
				}
			}
		};
		Action<ThumbnailPanelComponent> onReset = delegate(ThumbnailPanelComponent panel)
		{
			panel._button.onClick.RemoveAllListeners();
			panel._button.onClick.AddListener(delegate
			{
				List<LobbyPlayer> teamPlayers = GameParam.GetTeamPlayers(teamIndex, CollectionPool.RequestList<LobbyPlayer>());
				if (playerIndex >= teamPlayers.Count)
				{
					teamPlayers.Dispose();
				}
				else
				{
					LobbyPlayer lobbyPlayer = teamPlayers[playerIndex];
					if (lobbyPlayer == null)
					{
						teamPlayers.Dispose();
					}
					else
					{
						teamPlayers.Dispose();
						if (LobbyMenu.IsHost && NetworkServer.active && !lobbyPlayer.isHost)
						{
							UNetManager.instance.KickPlayer(lobbyPlayer);
						}
					}
				}
			});
		};
		ThumbnailPanelComponent.BasePanel basePanel = ThumbnailPanelComponent.Build(onReset, onRefresh);
		basePanel.As(ThumbnailPanelComponent.Variant.PlayerListItem);
		return basePanel;
	}

	private static Sprite GetPingSprite(int ping)
	{
		if (ping < 10)
		{
			return IconKeys._UI_Signal5.Get64();
		}
		if (ping < 30)
		{
			return IconKeys._UI_Signal4.Get64();
		}
		if (ping < 50)
		{
			return IconKeys._UI_Signal3.Get64();
		}
		if (ping < 100)
		{
			return IconKeys._UI_Signal2.Get64();
		}
		return IconKeys._UI_Signal1.Get64();
	}

	private static string GetReadyStatus(LobbyPlayer player)
	{
		if (IsPlayerDownloading(player))
		{
			return I2Helper.TryGet("##mainmenu_lobby_downloading");
		}
		if (GameParam.Map == null)
		{
			return I2Helper.TryGet("##mainmenu_lobby_waitingforhost");
		}
		return I2Helper.TryGet((!LobbyMenu.GetIsReady(player)) ? "##mainmenu_lobby_notready" : "##mainmenu_lobby_ready");
	}

	public static bool IsPlayerDownloading(LobbyPlayer player)
	{
		return !player.downloadProgression.Equals(-1f);
	}

	protected override SignalHandle<MenuState> AddSignals()
	{
		return base.AddSignals().AddSignalConnection(MenuState.Loaded, OpenSignal, BaseMenu.OnOpen, this).AddSignalConnection(MenuState.Opened, CloseSignal, BaseMenu.OnClose, this)
			.AddSignalConnection(MenuState.Opened, RefreshSignal, BaseMenu.OnRefresh, this)
			.AddSignalConnection(MenuState.Opened, UNetManager.LobbyRefresh, BaseMenu.OnRefresh, this)
			.AddSignalConnection(MenuState.Opened, UNetManager.ServerPlayerKicked, OnPlayerKick, this)
			.AddSignalConnection(MenuState.Opened, UNetManager.PlayerListChanged, BaseMenu.OnRefresh, this)
			.AddSignalConnection(MenuState.Opened, UNetManager.LobbyPlayerJoined, OnPlayerJoin, this)
			.AddSignalConnection(MenuState.Opened, UNetManager.LobbyPlayerLeft, OnPlayerLeave, this)
			.AddSignalConnection(MenuState.Opened, UNetManager.OnSetIsHostReady, BaseMenu.OnRefresh, this)
			.AddSignalConnection(MenuState.Opened, UNetManager.OnPlayerDownloadStart, BaseMenu.OnRefresh, this)
			.AddSignalConnection(MenuState.Opened, UNetManager.OnPlayerDownloadEnd, BaseMenu.OnRefresh, this)
			.AddSignalConnection(MenuState.Opened, ShowSignal, OnShow, this)
			.AddSignalConnection(MenuState.Opened, HideSignal, OnHide, this);
	}
}
