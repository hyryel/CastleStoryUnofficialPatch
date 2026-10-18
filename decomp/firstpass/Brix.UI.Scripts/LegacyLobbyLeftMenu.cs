using System;
using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Network;
using Brix.NewUI.UserSettings_;
using Brix.UI.Builder.Menu;
using Brix.UI.Icons;
using Brix.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Scripts;

public class LegacyLobbyLeftMenu : LegacyBaseMenu
{
	private enum ExpansionState
	{
		None,
		PlayerName,
		PlayerFaction,
		PlayerTeam
	}

	public Container TargetContainer;

	public Containee FactionPanelSource;

	public Containee TeamPanelSource;

	private static readonly IconKey CollapsedIconKey = IconKeys._UI_Big_Arrow_Down;

	private static readonly IconKey ExpandedIconKey = IconKeys._UI_Big_Arrow_Up;

	private static readonly Action<int, LegacyLobbyLeftMenu> OnItemClick = delegate(int idx, LegacyLobbyLeftMenu menu)
	{
		Container.Item item = menu.TargetContainer.items[idx];
		switch (item.containerPopulatorOperation)
		{
		case Operation.PlayerName:
			ToggleExpansionState(ExpansionState.PlayerName);
			break;
		case Operation.PlayerFaction:
		{
			int maxFactionCount = GameParam.MaxFactionCount;
			if (maxFactionCount > 0)
			{
				ToggleExpansionState(ExpansionState.PlayerFaction);
			}
			break;
		}
		case Operation.PlayerTeam:
		{
			int maxTeamCount = GameParam.MaxTeamCount;
			if (maxTeamCount > 0)
			{
				ToggleExpansionState(ExpansionState.PlayerTeam);
			}
			break;
		}
		case Operation.MultiplayerHost:
			UNetManager.instance.LocalPlayerName = ((!string.IsNullOrEmpty(Neo.multiplayerName)) ? Neo.multiplayerName : "Player");
			NetworkWaitingModalMenu.OpenAsHostSignal.Invoke();
			break;
		case Operation.LeaveLobby:
			NetworkWaitingQuitModalMenu.OpenSignal.Invoke();
			UNetManager.instance.ResetPersistantNetworkSceneObjects();
			UNetManager.instance.Disconnect();
			break;
		}
	};

	private static readonly Action<int, string, LegacyLobbyLeftMenu> OnItemText = delegate(int idx, string value, LegacyLobbyLeftMenu menu)
	{
		Container.Item item = menu.TargetContainer.items[idx];
		Operation containerPopulatorOperation = item.containerPopulatorOperation;
		if (containerPopulatorOperation == Operation.PlayerNameEdit)
		{
			PlayerName = value;
		}
	};

	private static readonly Action<LegacyLobbyLeftMenu> RefreshItems = delegate(LegacyLobbyLeftMenu menu)
	{
		foreach (Container.Item item2 in menu.TargetContainer.items)
		{
			if (!(item2.instance == null))
			{
				switch (item2.containerPopulatorOperation)
				{
				case Operation.PlayerName:
					item2.instance.spriteImage.sprite = ((!IsExpansionState(ExpansionState.PlayerName)) ? CollapsedIconKey.Get64() : ExpandedIconKey.Get64());
					break;
				case Operation.PlayerFaction:
					item2.instance.spriteImage.sprite = ((!IsExpansionState(ExpansionState.PlayerFaction)) ? CollapsedIconKey.Get64() : ExpandedIconKey.Get64());
					break;
				case Operation.PlayerTeam:
					item2.instance.spriteImage.sprite = ((!IsExpansionState(ExpansionState.PlayerTeam)) ? CollapsedIconKey.Get64() : ExpandedIconKey.Get64());
					break;
				case Operation.PlayerNameEdit:
				{
					bool activeSelf = item2.instance.gameObject.activeSelf;
					bool flag2 = IsExpansionState(ExpansionState.PlayerName);
					Container.ExecActivationAction(item2.instance, flag2);
					item2.instance.actualInputField.text = PlayerName;
					if (flag2 && !activeSelf)
					{
						item2.instance.actualInputField.Select();
					}
					break;
				}
				case Operation.PlayerFactionEdit:
				{
					bool flag3 = IsExpansionState(ExpansionState.PlayerFaction);
					Container.ExecActivationAction(item2.instance, flag3);
					if (menu._factionContainer == null)
					{
						ContaineeExtended containeeExtended2 = item2.instance as ContaineeExtended;
						containeeExtended2.ResetPlugCache();
						menu._factionContainer = containeeExtended2.GetPlugComponent<Container>("container");
					}
					menu.TryRebuildFactions();
					menu.RefreshFactions();
					break;
				}
				case Operation.PlayerTeamEdit:
				{
					bool flag = IsExpansionState(ExpansionState.PlayerTeam);
					Container.ExecActivationAction(item2.instance, flag);
					if (menu._teamContainer == null)
					{
						ContaineeExtended containeeExtended = item2.instance as ContaineeExtended;
						containeeExtended.ResetPlugCache();
						menu._teamContainer = containeeExtended.GetPlugComponent<Container>("container");
					}
					menu.TryRebuildTeams();
					menu.RefreshTeams();
					break;
				}
				}
			}
		}
	};

	private Container _factionContainer;

	private SignalConnection _sgOnClickFaction;

	private static readonly Action<int, LegacyLobbyLeftMenu> OnItemClick_Faction = delegate(int idx, LegacyLobbyLeftMenu menu)
	{
		menu.PlayerFaction = idx;
	};

	private Container _teamContainer;

	private SignalConnection _sgOnClickTeam;

	private static readonly Action<int, LegacyLobbyLeftMenu> OnClickTeam = delegate(int idx, LegacyLobbyLeftMenu menu)
	{
		menu.PlayerTeam = idx;
	};

	private static readonly Action<LegacyLobbyLeftMenu> OnSyncLimits = delegate(LegacyLobbyLeftMenu menu)
	{
		RefreshItems(menu);
	};

	private static readonly Action<LegacyLobbyLeftMenu> OnSetMapModeOrPreset = delegate(LegacyLobbyLeftMenu menu)
	{
		RefreshItems(menu);
	};

	public static GameSignals.GameSignal OnSetNameSignal = new GameSignals.GameSignal();

	public static GameSignals.GameSignal OnSetCurrentExpansionStateSignal = new GameSignals.GameSignal();

	private readonly List<SignalConnection> _sgs = new List<SignalConnection>();

	private static readonly Action<LobbyPlayer, LegacyLobbyLeftMenu> OnPlayerJoinOrLeave = delegate(LobbyPlayer p, LegacyLobbyLeftMenu menu)
	{
		RefreshItems(menu);
	};

	private const ExpansionState DefaultExpansionState = ExpansionState.None;

	private static ExpansionState _currentExpansionState;

	private static string PlayerName
	{
		get
		{
			return Neo.multiplayerName;
		}
		set
		{
			string multiplayerName = Neo.multiplayerName;
			string text = value;
			text = ((!string.IsNullOrEmpty(text)) ? text : I2Helper.TryGet("##mainmenu_lobby_unnamedplayer"));
			if (!(text == multiplayerName))
			{
				Neo.multiplayerName = text;
				UserSettingsReader.Save(UserSettingManager.GetSettingsFromGame());
				UNetManager.instance.LocalPlayerName = text;
				UNetManager.instance.LocalLobbyPlayer.CallCmdSetPlayerName(text);
				GameSignals.Invoke(OnSetNameSignal);
			}
		}
	}

	private int PlayerFaction
	{
		get
		{
			return (!(LobbyMenu.Player == null)) ? LobbyMenu.Player.factionNumber : 0;
		}
		set
		{
			int playerFaction = PlayerFaction;
			int num = ((!GameParam.IsFactionAssignable(value, playerFaction)) ? playerFaction : value);
			if (num != playerFaction)
			{
				if (CurrentExpansionState == ExpansionState.PlayerFaction)
				{
					ResetExpansionState();
				}
				LobbyMenu.Player.CallCmdSetFaction(num);
				RefreshFactions();
			}
		}
	}

	private int PlayerTeam
	{
		get
		{
			return (!(LobbyMenu.Player == null)) ? LobbyMenu.Player.TeamNumber : 0;
		}
		set
		{
			int playerTeam = PlayerTeam;
			int num = ((!GameParam.IsTeamAssignable(value, playerTeam)) ? playerTeam : value);
			if (num != playerTeam)
			{
				if (CurrentExpansionState == ExpansionState.PlayerTeam)
				{
					ResetExpansionState();
				}
				LobbyMenu.Player.CallCmdSetTeam(num);
				RefreshTeams();
			}
		}
	}

	private static ExpansionState CurrentExpansionState
	{
		get
		{
			return _currentExpansionState;
		}
		set
		{
			if (_currentExpansionState != value)
			{
				_currentExpansionState = value;
				GameSignals.Invoke(OnSetCurrentExpansionStateSignal);
			}
		}
	}

	private void OnEnable()
	{
		ResetExpansionState();
		Connect();
		RefreshItems(this);
	}

	private void OnDisable()
	{
		Disconnect();
	}

	private void TryRebuildFactions()
	{
		if (_factionContainer == null)
		{
			return;
		}
		GameParam.FactionInfo[] currentFactionInfos = GameParam.CurrentFactionInfos;
		if (_factionContainer.items.Count < currentFactionInfos.Length)
		{
			_sgOnClickFaction.Disconnect();
			_sgOnClickFaction = _factionContainer.OnItemClickSignal.Connect(OnItemClick_Faction, this);
			_factionContainer.Populate(isChain: true, purgeOnly: true);
			_factionContainer.items.Clear();
			GameParam.FactionInfo[] array = currentFactionInfos;
			foreach (GameParam.FactionInfo factionInfo in array)
			{
				Container.Item item = new Container.Item();
				item.original = FactionPanelSource;
				item.isDynamic = true;
				item.titleText = I2Helper.TryGet(factionInfo.Name).ToUpper();
				item.sprite = factionInfo.Icon;
				item.color = BaseMenu.Colors.White;
				item.keepSpriteImageEnabled = true;
				Container.Item item2 = item;
				_factionContainer.items.Add(item2);
			}
			_factionContainer.Populate(isChain: true);
		}
	}

	private void RefreshFactions()
	{
		if (_factionContainer == null || LobbyMenu.Player == null)
		{
			return;
		}
		int assignedFactionCount = GameParam.AssignedFactionCount;
		GameParam.FactionInfo[] currentFactionInfos = GameParam.CurrentFactionInfos;
		for (int i = 0; i < _factionContainer.items.Count; i++)
		{
			Container.Item item = _factionContainer.items[i];
			bool flag = i < currentFactionInfos.Length;
			Container.ExecActivationAction(item.instance, flag);
			if (!flag)
			{
				continue;
			}
			Sprite icon = currentFactionInfos[i].Icon;
			string text = I2Helper.TryGet(currentFactionInfos[i].Name).ToUpper();
			int dstGroup = i;
			int factionNumber = LobbyMenu.Player.factionNumber;
			if (!GameParam.IsFactionAssignable(dstGroup, factionNumber, assignedFactionCount))
			{
				text = $"<color=#{BaseMenu.HexCastleBrown}>{text}</color>";
			}
			item.sprite = icon;
			item.titleText = text;
			if (!(item.instance == null))
			{
				item.instance.spriteImage.sprite = icon;
				item.instance.title.text = text;
				bool flag2 = i == LobbyMenu.Player.factionNumber;
				ColorBlock colors = item.instance.button.colors;
				if (flag2)
				{
					colors.disabledColor = BaseMenu.Colors.White;
					colors.normalColor = BaseMenu.Colors.White;
					colors.highlightedColor = BaseMenu.Colors.White;
					colors.pressedColor = BaseMenu.Colors.White;
				}
				else
				{
					colors.disabledColor = BaseMenu.Colors.LightYellow;
					colors.normalColor = BaseMenu.Colors.LightYellow;
					colors.highlightedColor = BaseMenu.Colors.LighterYellow;
					colors.pressedColor = BaseMenu.Colors.White;
				}
				item.instance.button.colors = colors;
			}
		}
	}

	public static int GetValidatedTeam(int team)
	{
		if (team >= GameParam.CurrentTeamInfos.Length)
		{
			return 0;
		}
		LobbyPlayer player = LobbyMenu.Player;
		foreach (LobbyPlayer player2 in LobbyMenu.Players)
		{
			if (player2 == player || player2.factionNumber != player.factionNumber)
			{
				continue;
			}
			return player2.TeamNumber;
		}
		return team;
	}

	private void ValidateTeam()
	{
		PlayerTeam = GetValidatedTeam(LobbyMenu.Player.TeamNumber);
	}

	private void TryRebuildTeams()
	{
		if (_teamContainer == null)
		{
			return;
		}
		GameParam.TeamInfo[] currentTeamInfos = GameParam.CurrentTeamInfos;
		if (_teamContainer.items.Count < currentTeamInfos.Length)
		{
			_sgOnClickTeam.Disconnect();
			_sgOnClickTeam = _teamContainer.OnItemClickSignal.Connect(OnClickTeam, this);
			_teamContainer.Populate(isChain: true, purgeOnly: true);
			_teamContainer.items.Clear();
			GameParam.TeamInfo[] array = currentTeamInfos;
			foreach (GameParam.TeamInfo teamInfo in array)
			{
				Container.Item item = new Container.Item();
				item.original = TeamPanelSource;
				item.isDynamic = true;
				item.titleText = I2Helper.TryGet(teamInfo.Name).ToUpper();
				item.keepSpriteImageEnabled = true;
				Container.Item item2 = item;
				_teamContainer.items.Add(item2);
			}
			_teamContainer.Populate(isChain: true);
		}
	}

	private void RefreshTeams()
	{
		if (_teamContainer == null || LobbyMenu.Player == null)
		{
			return;
		}
		int assignedTeamCount = GameParam.AssignedTeamCount;
		GameParam.TeamInfo[] currentTeamInfos = GameParam.CurrentTeamInfos;
		for (int i = 0; i < _teamContainer.items.Count; i++)
		{
			Container.Item item = _teamContainer.items[i];
			bool flag = i < currentTeamInfos.Length;
			Container.ExecActivationAction(item.instance, flag);
			if (!flag)
			{
				continue;
			}
			string text = I2Helper.TryGet(currentTeamInfos[i].Name).ToUpper();
			int dstGroup = i;
			int teamNumber = LobbyMenu.Player.TeamNumber;
			if (!GameParam.IsTeamAssignable(dstGroup, teamNumber, assignedTeamCount))
			{
				text = $"<color=#{BaseMenu.HexCastleBrown}>{text}</color>";
			}
			item.titleText = text;
			if (!(item.instance == null))
			{
				item.instance.title.text = text;
				bool flag2 = i == LobbyMenu.Player.TeamNumber;
				ColorBlock colors = item.instance.button.colors;
				if (flag2)
				{
					colors.disabledColor = BaseMenu.Colors.White;
					colors.normalColor = BaseMenu.Colors.White;
					colors.highlightedColor = BaseMenu.Colors.White;
					colors.pressedColor = BaseMenu.Colors.White;
				}
				else
				{
					colors.disabledColor = BaseMenu.Colors.LightYellow;
					colors.normalColor = BaseMenu.Colors.LightYellow;
					colors.highlightedColor = BaseMenu.Colors.LighterYellow;
					colors.pressedColor = BaseMenu.Colors.White;
				}
				item.instance.button.colors = colors;
			}
		}
	}

	private void Connect()
	{
		if (TargetContainer != null)
		{
			_sgs.Add(TargetContainer.OnPopulateSignal.Connect(RefreshItems, this));
		}
		_sgs.Add(GameSignals.Connect(OnSetNameSignal, RefreshItems, this));
		_sgs.Add(GameSignals.Connect(OnSetCurrentExpansionStateSignal, RefreshItems, this));
		if (TargetContainer != null)
		{
			_sgs.Add(TargetContainer.OnItemClickSignal.Connect(OnItemClick, this));
		}
		if (TargetContainer != null)
		{
			_sgs.Add(TargetContainer.OnItemTextSignal.Connect(OnItemText, this));
		}
		if (TargetContainer != null)
		{
			_sgs.Add(TargetContainer.OnItemTextSignal.Connect(OnItemText, this));
		}
		_sgs.Add(GameSignals.Connect(UNetManager.LobbyPlayerJoined, OnPlayerJoinOrLeave, this));
		_sgs.Add(GameSignals.Connect(UNetManager.LobbyPlayerLeft, OnPlayerJoinOrLeave, this));
		_sgs.Add(GameSignals.Connect(UNetManager.PlayerListChanged, RefreshItems, this));
		_sgs.Add(GameSignals.Connect(GameParam.OnSyncLimitsSignal, OnSyncLimits, this));
		_sgs.Add(GameSignals.Connect(GameParam.OnSetMapSignal, OnSetMapModeOrPreset, this));
		_sgs.Add(GameSignals.Connect(GameParam.OnSetModeSignal, OnSetMapModeOrPreset, this));
		_sgs.Add(GameSignals.Connect(GameParam.OnSetPresetSignal, OnSetMapModeOrPreset, this));
	}

	private void Disconnect()
	{
		for (int i = 0; i < _sgs.Count; i++)
		{
			_sgs[i].Disconnect();
		}
		_sgs.Clear();
	}

	private static void ResetExpansionState()
	{
		CurrentExpansionState = ExpansionState.None;
	}

	private static void ToggleExpansionState(ExpansionState expansionState, bool onlyIfNone = false)
	{
		if (!onlyIfNone || CurrentExpansionState == ExpansionState.None)
		{
			if (CurrentExpansionState == expansionState)
			{
				ResetExpansionState();
			}
			else
			{
				CurrentExpansionState = expansionState;
			}
		}
	}

	private static bool IsExpansionState(ExpansionState expansionState)
	{
		return CurrentExpansionState == expansionState;
	}
}
