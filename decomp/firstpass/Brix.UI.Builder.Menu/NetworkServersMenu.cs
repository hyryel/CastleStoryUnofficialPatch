using System;
using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Network;
using Brix.UI.Builder.Panel;
using Brix.Utils;
using Network;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Brix.UI.Builder.Menu;

public class NetworkServersMenu : BaseMenu
{
	public enum Source
	{
		Public,
		Lan
	}

	public static List<Source> SourceList = new List<Source>
	{
		Source.Public,
		Source.Lan
	};

	public static Dictionary<Source, string> SourceLabelMap = new Dictionary<Source, string>(FastEnumIntEqualityComparer<Source>.Instance)
	{
		{
			Source.Public,
			"Public"
		},
		{
			Source.Lan,
			"Lan"
		}
	};

	private readonly List<MatchList.MatchData> _servers = new List<MatchList.MatchData>();

	public static bool IsShowingFull = false;

	private static readonly Action<NetworkServersMenu> RefreshServers = delegate(NetworkServersMenu menu)
	{
		menu._servers.Clear();
		Dictionary<MatchAddress, MatchList.MatchData> matchList = UNetManager.instance.matchList.GetMatchList();
		foreach (KeyValuePair<MatchAddress, MatchList.MatchData> item in matchList)
		{
			if (!(item.Value.Version != Neo.SemanticVersion))
			{
				menu._servers.Add(item.Value);
			}
		}
		menu.Refresh();
	};

	public static GameSignals.GameSignal OnSelectServerSignal = new GameSignals.GameSignal();

	public static Signal RefreshSignal = new Signal();

	protected override bool ReloadOnOpen => true;

	public static Source SelectedSource => (LegacyMultiplayerJoinMenu.SelectedOperation == LegacyBaseMenu.Operation.LocalServers) ? Source.Lan : Source.Public;

	public static MatchList.MatchData SelectedMatchData
	{
		get
		{
			return Neo.matchData;
		}
		set
		{
			Neo.matchData = value;
			GameSignals.Invoke(OnSelectServerSignal);
			RefreshSignal.Invoke();
			NetworkServersActionsMenu.RefreshSignal.Invoke();
		}
	}

	public static bool CanJoin => SelectedMatchData.IsValid;

	private IPanel Library
	{
		get
		{
			Action<ScrollableLayoutPanelComponent> onReset = delegate(ScrollableLayoutPanelComponent panel)
			{
				panel._layoutElement.enabled = true;
				panel._layoutElement.flexibleHeight = 1f;
				panel._layoutElement.flexibleWidth = 1f;
			};
			Action<VerticalLayoutPanelComponent> onReset2 = delegate(VerticalLayoutPanelComponent panel)
			{
				panel._layoutGroup.spacing = 2f;
			};
			return ScrollableLayoutPanel.New(onReset).AddPanel(VerticalLayoutPanel.New(onReset2).As(VerticalLayoutPanel.Variant.Library).AddPanels(GetServerBuilders()));
		}
	}

	private NetworkServersMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static NetworkServersMenu Build(Func<RectTransform> binGetter)
	{
		return new NetworkServersMenu(binGetter);
	}

	protected override PanelHandle AddPanels()
	{
		return base.AddPanels().AddPanel(Library);
	}

	public PanelList GetBuilders()
	{
		PanelList panelList = PanelList.Request();
		PanelList panelList2 = PanelList.Request();
		panelList2.AddRange(GetServerBuilders());
		Action<VerticalLayoutPanelComponent> onReset = delegate(VerticalLayoutPanelComponent panel)
		{
			panel._fitter.enabled = false;
			panel._layoutGroup.childAlignment = TextAnchor.UpperLeft;
			panel._layoutGroup.childForceExpandWidth = true;
		};
		ILayoutPanel item = VerticalLayoutPanel.New(onReset).As(VerticalLayoutPanel.Variant.Library).AddPanels(panelList2);
		panelList.Add(item);
		return panelList;
	}

	private PanelList GetServerBuilders()
	{
		PanelList panelList = PanelList.Request();
		for (int i = 0; i < 40; i++)
		{
			int n = i;
			Action<ThumbnailPanelComponent> onRefresh = delegate(ThumbnailPanelComponent panel)
			{
				if (n >= _servers.Count)
				{
					panel.gameObject.SetActive(value: false);
				}
				else
				{
					MatchList.MatchData other = _servers[n];
					if (other.IsLanGame && SelectedSource == Source.Public)
					{
						panel.gameObject.SetActive(value: false);
					}
					else if (!other.IsLanGame && SelectedSource == Source.Lan)
					{
						panel.gameObject.SetActive(value: false);
					}
					else
					{
						panel.gameObject.SetActive(value: true);
						panel._label.text = other.MatchName;
						panel._subLabel.text = ((SelectedSource != Source.Public) ? string.Empty : string.Format("{0}: {1}/4", I2Helper.TryGet("##mainmenu_multiplayer_players"), other.PlayerCount.ToString()));
						panel._infoLabel.text = other.Address.ToString().Replace('|', ':');
						ColorBlock colors = panel._button.colors;
						if (SelectedMatchData.Equals(other))
						{
							colors.disabledColor = Colors.White;
							colors.normalColor = Colors.White;
							colors.highlightedColor = Colors.White;
							colors.pressedColor = Colors.White;
						}
						else
						{
							colors.disabledColor = Colors.Yellow;
							colors.normalColor = Colors.Yellow;
							colors.highlightedColor = Colors.LightYellow;
							colors.pressedColor = Colors.White;
						}
						panel._button.colors = colors;
					}
				}
			};
			Action<ThumbnailPanelComponent> onReset = delegate(ThumbnailPanelComponent panel)
			{
				panel._button.onClick.RemoveAllListeners();
				panel._button.onClick.AddListener(delegate
				{
					if (n < _servers.Count)
					{
						MatchList.MatchData matchData = _servers[n];
						if (!SelectedMatchData.Equals(matchData))
						{
							SelectedMatchData = matchData;
							RefreshSignal.Invoke();
						}
					}
				});
			};
			ThumbnailPanelComponent.BasePanel basePanel = ThumbnailPanelComponent.Build(onReset, onRefresh);
			basePanel.As(ThumbnailPanelComponent.Variant.ServerListItem);
			panelList.Add(basePanel);
		}
		return panelList;
	}

	public PanelList GetInfoBuilders(Func<string> getMessageValue)
	{
		PanelList panelList = PanelList.Request();
		Dictionary<string, NetworkBroadcastResult> broadcastsReceived = UNetManager.instance.networkDiscovery.broadcastsReceived;
		if (broadcastsReceived == null || broadcastsReceived.Keys.Count == 0)
		{
			Action<TitleTextPanelComponent> action = delegate(TitleTextPanelComponent panel)
			{
				panel._label.text = getMessageValue();
				panel._label.alignment = TextAnchor.MiddleCenter;
				panel._layoutElement.minHeight = panel._label.preferredHeight;
				panel._layoutElement.preferredHeight = panel._label.preferredHeight;
				panel._layoutElement.flexibleWidth = 1f;
				panel._layoutElement.flexibleHeight = 1f;
			};
			TitleTextPanelComponent.BasePanel item = TitleTextPanelComponent.Build(action, action).As(TitleTextPanelComponent.BasePanel.Variant.SidebarHelpText);
			panelList.Add(item);
		}
		return panelList;
	}

	protected override SignalHandle<MenuState> AddSignals()
	{
		return base.AddSignals().AddSignalConnection(MenuState.Opened, LegacyMultiplayerJoinMenu.OnSelectOperationSignal, RefreshServers, this).AddSignalConnection(MenuState.Opened, UNetManager.instance.matchList.MatchListChangedSignal, RefreshServers, this)
			.AddSignalConnection(MenuState.Opened, LegacyMultiplayerJoinMenu.OnRefreshTimerSignal, RefreshServers, this)
			.AddSignalConnection(MenuState.Opened, RefreshSignal, BaseMenu.OnRefresh, this);
	}

	protected override void OpenSelf()
	{
		UNetManager.instance.matchList.StartLobbyDiscovery();
		base.OpenSelf();
	}

	protected override void CloseSelf()
	{
		base.CloseSelf();
		SelectedMatchData = default(MatchList.MatchData);
	}
}
