using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters;
using Brix.Assets;
using Brix.Engine;
using Brix.Engine.Environment;
using Brix.External.Factories;
using Brix.Game;
using Brix.Network;
using Brix.UI.Builder.Menu;
using Brix.UI.Icons;
using Brix.Utils;
using Brix.Utils.Collections;
using Brix.Utils.Logging;
using Newtonsoft.Json;
using Smooth.Slinq;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

public class GameParam : NetworkBehaviour
{
	public struct GameParameters
	{
		public string MapCodeName;

		public string ModeCodeName;

		public string PresetCodeName;

		public long AssetLastWriteTime;

		public int MinTeams;

		public int MaxTeams;

		public int MinPlayersPerTeam;

		public int MaxPlayersPerTeam;

		public int MinFactions;

		public int MaxFactions;

		public int MinPlayersPerFaction;

		public int MaxPlayersPerFactions;

		public bool AllowSharingTeam;

		public bool AllowSharingFaction;

		public GameParameters(string mapCodeName, string modeCodeName, string presetCodename, long assetLastWriteTime, int minTeams, int maxTeams, int minPlayersPerTeam, int maxPlayersPerTeam, int minFactions, int maxfactions, int maxPlayerPerFactions, int minPlayersPerFactions, bool allowSharingTeam, bool allowSharingFaction)
		{
			MapCodeName = mapCodeName;
			ModeCodeName = modeCodeName;
			PresetCodeName = presetCodename;
			AssetLastWriteTime = assetLastWriteTime;
			MinTeams = minTeams;
			MaxTeams = maxTeams;
			MinPlayersPerTeam = minPlayersPerTeam;
			MaxPlayersPerTeam = maxPlayersPerTeam;
			MinFactions = minFactions;
			MaxFactions = maxfactions;
			MaxPlayersPerFactions = maxPlayerPerFactions;
			MinPlayersPerFaction = minPlayersPerFactions;
			AllowSharingTeam = allowSharingTeam;
			AllowSharingFaction = allowSharingFaction;
		}
	}

	public delegate void PlayLoadingAnimationDelegate();

	public class FactionInfo
	{
		public Sprite Icon;

		public string Name;

		public FactionInfo(Sprite icon, string name)
		{
			Icon = icon;
			Name = name;
		}
	}

	public class TeamInfo
	{
		public Sprite Icon;

		public string Name;

		public TeamInfo(Sprite icon, string name)
		{
			Icon = icon;
			Name = name;
		}
	}

	[SyncVar]
	public bool HostLoadedAllFactions;

	[SyncVar]
	public bool HostLoaded;

	[SyncVar]
	public bool HostReady;

	[SyncVar(hook = "ParametersChanged")]
	public GameParameters Parameters;

	[SyncVar]
	public bool _isInitialized;

	[SyncVar]
	public string mapChecksum;

	private static BrixLogger logger;

	[SerializeField]
	private bool _waitingForAnim;

	public static GameSignals.GameSignal OnSetMapSignal;

	private Asset_Map _map;

	public static GameSignals.GameSignal OnSetModeSignal;

	private Asset_Gamemode _gamemode;

	public static GameSignals.GameSignal OnSetPresetSignal;

	private Asset_GamemodePreset _preset;

	public static GameSignals.GameSignal OnSetSelectedMapSignal;

	private Asset_Map _selectedMap;

	public static GameSignals.GameSignal OnSetSelectedModeSignal;

	private Asset_Gamemode _selectedGamemode;

	public static GameSignals.GameSignal OnSetSelectedPresetSignal;

	private Asset_GamemodePreset _selectedPreset;

	public static GameSignals.GameSignal OnSyncLimitsSignal;

	private static readonly Action OnAfterPlayLoadingAnimation;

	private static FactionInfo[] _currentFactionInfos;

	public static readonly TeamInfo[] CurrentTeamInfos;

	private static int kEventEventPlayLoadingAnimation;

	public static int MinTeamCount
	{
		get
		{
			int minTeams = instance.Parameters.MinTeams;
			minTeams = ((minTeams <= 0) ? 1 : minTeams);
			return (minTeams <= MaxTeamCount) ? minTeams : MaxTeamCount;
		}
	}

	public static int MinFactionCount
	{
		get
		{
			int minFactions = instance.Parameters.MinFactions;
			minFactions = ((minFactions <= 0) ? 1 : minFactions);
			return (minFactions <= MaxFactionCount) ? minFactions : MaxFactionCount;
		}
	}

	public static int MaxTeamCount
	{
		get
		{
			int maxTeams = instance.Parameters.MaxTeams;
			maxTeams = ((maxTeams != -1) ? maxTeams : CurrentTeamInfos.Length);
			return (maxTeams <= CurrentTeamInfos.Length) ? maxTeams : CurrentTeamInfos.Length;
		}
	}

	public static int MaxFactionCount
	{
		get
		{
			int maxFactions = instance.Parameters.MaxFactions;
			maxFactions = ((maxFactions != -1) ? maxFactions : CurrentFactionInfos.Length);
			return (maxFactions <= CurrentFactionInfos.Length) ? maxFactions : CurrentFactionInfos.Length;
		}
	}

	public static Asset_Map Map
	{
		get
		{
			if (instance == null)
			{
				return null;
			}
			return instance._map;
		}
		set
		{
			if (NetworkServer.active && value != null)
			{
				instance.NetworkmapChecksum = Legitimizer.GenerateOneHashForAllFilesIn(value.path).hash;
			}
			if (instance._map != value)
			{
				instance._map = value;
				logger.InfoFormat("<b>MAP</b>\n{0}", (instance._map != null) ? (instance._map.displayName + " (" + instance._map.customName + ")") : "<null>");
				if (value != null)
				{
					instance.Network_isInitialized = true;
				}
				GameSignals.Invoke(OnSetMapSignal);
			}
		}
	}

	public static Asset_Gamemode Gamemode
	{
		get
		{
			if (instance == null)
			{
				return null;
			}
			if (instance._gamemode == null && Map != null)
			{
				instance._gamemode = Map.GetGamemodes().Slinq().FirstOrDefault();
			}
			return instance._gamemode;
		}
		set
		{
			instance._gamemode = value;
			logger.InfoFormat("<b>GAMEMODE</b>\n{0}", (instance._gamemode != null) ? instance._gamemode.displayName : "<null>");
			GameSignals.Invoke(OnSetModeSignal);
		}
	}

	public static Asset_GamemodePreset Preset
	{
		get
		{
			if (instance == null)
			{
				return null;
			}
			if (instance._preset == null && Gamemode != null)
			{
				instance._preset = Gamemode.GetDefaultPreset();
			}
			return instance._preset;
		}
		set
		{
			instance._preset = value;
			logger.InfoFormat("<b>PRESET</b>\n{0}", (instance._preset != null) ? instance._preset.displayName : "<null>");
			UpdateParameters();
			RefreshFactionsAndTeams();
			if (NetworkServer.active)
			{
				if (value != null)
				{
					instance.Parameters.PresetCodeName = value.codename;
				}
				else
				{
					instance.Parameters.PresetCodeName = string.Empty;
				}
			}
			GameSignals.Invoke(OnSetPresetSignal);
		}
	}

	public static Asset_Map SelectedMap
	{
		get
		{
			if (instance == null)
			{
				return null;
			}
			return instance._selectedMap;
		}
		set
		{
			if (instance._selectedMap != value)
			{
				instance._selectedMap = value;
				logger.InfoFormat("<b>MAP (selected)</b>\n{0}", (instance._selectedMap != null) ? (instance._selectedMap.displayName + " (" + instance._selectedMap.customName + ")") : "<null>");
				GameSignals.Invoke(OnSetSelectedMapSignal);
			}
		}
	}

	public static Asset_Gamemode SelectedGamemode
	{
		get
		{
			if (instance == null)
			{
				return null;
			}
			if (instance._selectedGamemode == null && Map != null)
			{
				SelectedGamemode = Map.GetGamemodes().Slinq().FirstOrDefault();
			}
			return instance._selectedGamemode;
		}
		set
		{
			if (instance._selectedGamemode != value)
			{
				instance._selectedGamemode = value;
				logger.InfoFormat("<b>GAMEMODE (selected)</b>\n{0}", (instance._selectedGamemode != null) ? instance._selectedGamemode.displayName : "<null>");
				GameSignals.Invoke(OnSetSelectedModeSignal);
			}
		}
	}

	public static Asset_GamemodePreset SelectedPreset
	{
		get
		{
			if (instance == null)
			{
				return null;
			}
			if (instance._selectedPreset == null && SelectedGamemode != null)
			{
				SelectedPreset = SelectedGamemode.GetDefaultPreset();
			}
			return instance._selectedPreset;
		}
		set
		{
			if (instance._selectedPreset != value)
			{
				instance._selectedPreset = value;
				logger.InfoFormat("<b>PRESET (selected)</b>\n{0}", (instance._selectedPreset != null) ? instance._selectedPreset.displayName : "<null>");
				GameSignals.Invoke(OnSetSelectedPresetSignal);
			}
		}
	}

	public static GameParam instance
	{
		get
		{
			if (UNetManager.instance != null)
			{
				return UNetManager.instance.gameParam;
			}
			return null;
		}
	}

	public static FactionInfo[] CurrentFactionInfos
	{
		get
		{
			return _currentFactionInfos;
		}
		set
		{
			_currentFactionInfos = value;
		}
	}

	public static int AssignedFactionCount => UNetManager.instance.Lobby_AssignedFaction().Count;

	public static int AssignedTeamCount => UNetManager.instance.Lobby_AssignedTeam().Count;

	public bool NetworkHostLoadedAllFactions
	{
		get
		{
			return HostLoadedAllFactions;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref HostLoadedAllFactions, 1u);
		}
	}

	public bool NetworkHostLoaded
	{
		get
		{
			return HostLoaded;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref HostLoaded, 2u);
		}
	}

	public bool NetworkHostReady
	{
		get
		{
			return HostReady;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref HostReady, 4u);
		}
	}

	public GameParameters NetworkParameters
	{
		get
		{
			return Parameters;
		}
		[param: In]
		set
		{
			ref GameParameters parameters = ref Parameters;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				ParametersChanged(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref parameters, 8u);
		}
	}

	public bool Network_isInitialized
	{
		get
		{
			return _isInitialized;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref _isInitialized, 16u);
		}
	}

	public string NetworkmapChecksum
	{
		get
		{
			return mapChecksum;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref mapChecksum, 32u);
		}
	}

	[SyncEvent]
	public event PlayLoadingAnimationDelegate EventPlayLoadingAnimation;

	public void ParametersChanged(GameParameters parameters)
	{
		NetworkParameters = parameters;
		if (!NetworkServer.active)
		{
			ReadAndApplyNetworkParams();
		}
	}

	public static Biome BiomeToLoad()
	{
		BiomeIndex component = Factory.Peek(Brix.External.Factories.Environment.BiomeIndex).GetComponent<BiomeIndex>();
		return component.Biomes.Slinq().First((Biome b) => b.Name == Map.biomeName);
	}

	public static string GetBiomeName()
	{
		return Map.biomeName;
	}

	public static BiomeType GetBiomeType()
	{
		if (Map.biomeName == "Green")
		{
			return BiomeType.Green;
		}
		if (Map.biomeName == "Desert")
		{
			return BiomeType.Desert;
		}
		if (Map.biomeName == "Winter")
		{
			return BiomeType.Winter;
		}
		return BiomeType.Green;
	}

	public override void OnStartClient()
	{
		ReadAndApplyNetworkParams();
		EventPlayLoadingAnimation += OnPlayLoadingAnimation;
	}

	public void OnDisable()
	{
		Clear();
	}

	public void ReadAndApplyNetworkParams()
	{
		if (NetworkServer.active)
		{
			return;
		}
		if (!string.IsNullOrEmpty(Parameters.MapCodeName))
		{
			Asset_Map asset_Map = null;
			asset_Map = AssetDispensary.saves.SendQuery(AssetQueryParameters.AllSources | AssetQueryParameters.RefreshCache, AssetPlayableContext.Any).Slinq().Where((Asset_Map s, GameParam self) => s.codename == self.Parameters.MapCodeName && s.ModifiedTime == instance.Parameters.AssetLastWriteTime, this)
				.FirstOrDefault();
			if (asset_Map == null)
			{
				asset_Map = AssetDispensary.maps.SendQuery(AssetQueryParameters.AllSources | AssetQueryParameters.RefreshCache, AssetPlayableContext.Any).Slinq().Where((Asset_Map s, GameParam self) => s.codename == self.Parameters.MapCodeName && s.ModifiedTime == instance.Parameters.AssetLastWriteTime, this)
					.FirstOrDefault();
			}
			if (asset_Map != null)
			{
				Map = asset_Map;
				UNetManager.instance.RequestMapDownload(value: false);
			}
			else
			{
				UNetManager.instance.RequestMapDownload(value: true);
				logger.InfoFormat("Could not load map, requesting download for map {0}.", Parameters.MapCodeName);
			}
		}
		if (!string.IsNullOrEmpty(Parameters.ModeCodeName))
		{
			Gamemode = AssetDispensary.gamemodes.SendCodenameQuery(Parameters.ModeCodeName, AssetQueryParameters.AllSources | AssetQueryParameters.RefreshCache, AssetPlayableContext.Multiplayer);
		}
		if (!string.IsNullOrEmpty(Parameters.PresetCodeName))
		{
			Preset = AssetDispensary.presets.SendCodenameQuery(Parameters.PresetCodeName, AssetQueryParameters.AllSources | AssetQueryParameters.RefreshCache, AssetPlayableContext.Multiplayer);
		}
	}

	public static void UpdateParameters()
	{
		if (NetworkServer.active && Preset != null)
		{
			int result = -1;
			int result2 = -1;
			int num = -1;
			int result3 = -1;
			int result4 = -1;
			int result5 = -1;
			int result6 = -1;
			int result7 = -1;
			bool allowSharingTeam = true;
			bool allowSharingFaction = true;
			bool requiresCorruptrons = false;
			if (Preset.settings.ContainsKey("MP_MinTeams"))
			{
				int.TryParse(Preset.settings["MP_MinTeams"].ToString(), out result);
			}
			if (Preset.settings.ContainsKey("MP_MaxTeams"))
			{
				int.TryParse(Preset.settings["MP_MaxTeams"].ToString(), out result2);
			}
			if (Preset.settings.ContainsKey("MP_MinFactions"))
			{
				int.TryParse(Preset.settings["MP_MinFactions"].ToString(), out result3);
			}
			if (Preset.settings.ContainsKey("MP_MinPlayersPerTeam"))
			{
				int.TryParse(Preset.settings["MP_MinPlayersPerTeam"].ToString(), out result4);
			}
			if (Preset.settings.ContainsKey("MP_MaxPlayersPerTeam"))
			{
				int.TryParse(Preset.settings["MP_MaxPlayersPerTeam"].ToString(), out result5);
			}
			if (Preset.settings.ContainsKey("MP_MinPlayersPerFaction"))
			{
				int.TryParse(Preset.settings["MP_MinPlayersPerFaction"].ToString(), out result6);
			}
			if (Preset.settings.ContainsKey("MP_MaxPlayersPerFaction"))
			{
				int.TryParse(Preset.settings["MP_MaxPlayersPerFaction"].ToString(), out result7);
			}
			if (Preset.settings.ContainsKey("MP_AllowSharingTeam"))
			{
				allowSharingTeam = (bool)Preset.settings["MP_AllowSharingTeam"];
			}
			if (Preset.settings.ContainsKey("MP_AllowSharingFaction"))
			{
				allowSharingFaction = (bool)Preset.settings["MP_AllowSharingFaction"];
			}
			if (Preset.settings.ContainsKey("MP_RequiresCorruptrons"))
			{
				requiresCorruptrons = (bool)Preset.settings["MP_RequiresCorruptrons"];
			}
			num = instance.GetMaxFactions(requiresCorruptrons);
			instance.NetworkParameters = new GameParameters(instance._map.codename, instance._gamemode.codename, instance._preset.codename, instance._map.ModifiedTime, result, result2, result4, result5, result3, num, result7, result6, allowSharingTeam, allowSharingFaction);
		}
	}

	public static void RefreshFactionsAndTeams()
	{
		RefreshFactions();
		RefreshTeams();
		GameSignals.Invoke(OnSyncLimitsSignal);
	}

	private int GetMaxFactions(bool requiresCorruptrons)
	{
		int result = 0;
		if (Preset.settings.ContainsKey("MP_MaxFactions"))
		{
			int.TryParse(Preset.settings["MP_MaxFactions"].ToString(), out result);
		}
		int num = Map.GetMarkerGroupCount("Player Starting Point") - (requiresCorruptrons ? 1 : 0);
		return (result <= num && result > 0) ? result : num;
	}

	public static void StartGame(Action onLoadingScreenAnimEndAction = null)
	{
		if (onLoadingScreenAnimEndAction != null)
		{
			GameSignals.Once(GameSignals.LoadingScreenAnimEnd, onLoadingScreenAnimEndAction);
		}
		GameSignals.Connect(GameSignals.LoadingScreenAnimStart, delegate(GameParam aInstance)
		{
			aInstance.OnLoadingScreenAnimStart();
		}, instance);
		GameSignals.Connect(GameSignals.LoadingScreenAnimEnd, delegate(GameParam aInstance)
		{
			aInstance.OnLoadingScreenAnimEnd();
		}, instance);
		GameSignals.Invoke(GameSignals.LoadingGame, GameSignals.loadingContext.menu);
		if (!instance._waitingForAnim)
		{
			instance.StartLocalOnly();
		}
	}

	public void StartLocalOnly()
	{
		UNetManager.instance.StartHostLocalOnly();
	}

	public void OnLoadingScreenAnimStart()
	{
		_waitingForAnim = true;
	}

	public void OnLoadingScreenAnimEnd()
	{
		StartLocalOnly();
	}

	public void LoadDebug()
	{
		JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
		jsonSerializerSettings.TypeNameHandling = TypeNameHandling.Objects;
		jsonSerializerSettings.TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple;
		jsonSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
		Hashtable hashtable = JsonConvert.DeserializeObject<Hashtable>(File.ReadAllText("Info/DebugLaunchSettings.txt"), jsonSerializerSettings);
		Map = AssetDispensary.maps.SendCodenameQuery((string)hashtable["__MAPCODENAME"]);
		Gamemode = AssetDispensary.gamemodes.SendCodenameQuery((string)hashtable["__GAMEMODECODENAME"]);
		Preset = AssetDispensary.presets.SendCodenameQuery((string)hashtable["__PRESETCODENAME"]);
		UNetManager.instance.LoadDebug();
	}

	public void Clear()
	{
		Map = null;
		Gamemode = null;
		Preset = null;
		NetworkParameters = default(GameParameters);
		NetworkmapChecksum = string.Empty;
		NetworkHostLoaded = false;
		NetworkHostLoadedAllFactions = false;
		this.EventPlayLoadingAnimation = null;
	}

	public void BeginLoadingScreen()
	{
		CallEventPlayLoadingAnimation();
	}

	private void OnPlayLoadingAnimation()
	{
		if (UNetManager.instance.LocalLobbyPlayer.isHost)
		{
			GameSignals.Once(GameSignals.LoadingScreenAnimEnd, OnAfterPlayLoadingAnimation);
		}
		GameSignals.Invoke(GameSignals.LoadingGame, GameSignals.loadingContext.menu);
	}

	public static List<LobbyPlayer> GetFactionPlayers(int faction, List<LobbyPlayer> factionPlayers)
	{
		return UNetManager.instance.LobbyPlayerList.Slinq().Where((LobbyPlayer player, int aFaction) => player.factionNumber == aFaction, faction).AddTo(factionPlayers);
	}

	private static void RefreshFactions()
	{
		if (Map == null)
		{
			CurrentFactionInfos = new FactionInfo[0];
			return;
		}
		if (Map.IsNewGame())
		{
			if (Gamemode == null)
			{
				return;
			}
			List<Asset_Gamemode.FactionPrototype> list = (from f in Gamemode.Disk_ReadFactionPrototypes().Slinq()
				where !f.isAI && !f.isDefault && !f.isSpectator
				select f).ToPooledList();
			CurrentFactionInfos = new FactionInfo[list.Count];
			for (int num = 0; num < list.Count; num++)
			{
				Sprite sprite = list[num].flagIcon;
				if (sprite == null)
				{
					sprite = IconKeys._UI_FlagSmall.Get64();
				}
				CurrentFactionInfos[num] = new FactionInfo(sprite, list[num].name);
			}
			list.Dispose();
		}
		else
		{
			Faction.FactionMeta[] array = (from f in Map.Disk_ReadFactionMeta().Slinq()
				where f.assigned
				select f).ToArray();
			int num2 = 0;
			CurrentFactionInfos = new FactionInfo[array.Length];
			Faction.FactionMeta[] array2 = array;
			for (int num3 = 0; num3 < array2.Length; num3++)
			{
				Faction.FactionMeta factionMeta = array2[num3];
				CurrentFactionInfos[num2] = new FactionInfo(Faction.IconKeyMap[factionMeta.name].Get64(), factionMeta.ownerName);
				num2++;
			}
		}
		if (LobbyMenu.IsHost && !AreFactionsValid(AssignedFactionCount))
		{
			FixFactions();
		}
	}

	private static bool AreGroupsValid(int minGroupCount, int maxGroupCount, int minGroupPlayersCount, int maxGroupPlayersCount, int groupCount, int assignedGroupCount, Func<int, List<LobbyPlayer>, List<LobbyPlayer>> getGroupPlayers, Func<LobbyPlayer, int> getPlayerGroup)
	{
		if (assignedGroupCount < minGroupCount && minGroupCount != -1)
		{
			return false;
		}
		if (assignedGroupCount > maxGroupCount && maxGroupCount != -1)
		{
			return false;
		}
		for (int i = 0; i < groupCount; i++)
		{
			List<LobbyPlayer> list = getGroupPlayers(i, CollectionPool.RequestList<LobbyPlayer>());
			int count = list.Count;
			list.Dispose();
			if (count < minGroupPlayersCount && minGroupPlayersCount != -1)
			{
				return false;
			}
			if (count > maxGroupPlayersCount && maxGroupPlayersCount != -1)
			{
				return false;
			}
		}
		List<LobbyPlayer> players = GetPlayers(CollectionPool.RequestList<LobbyPlayer>());
		foreach (LobbyPlayer item in players)
		{
			int num = getPlayerGroup(item);
			if (num < 0 || num >= groupCount)
			{
				players.Dispose();
				return false;
			}
		}
		players.Dispose();
		return true;
	}

	private static bool AreFactionsValid(int assignedGroupCount = -1)
	{
		int minFactionCount = MinFactionCount;
		int maxFactionCount = MaxFactionCount;
		int minPlayersPerFaction = instance.Parameters.MinPlayersPerFaction;
		int maxPlayersPerFactions = instance.Parameters.MaxPlayersPerFactions;
		int groupCount = CurrentFactionInfos.Length;
		assignedGroupCount = ((assignedGroupCount == -1) ? AssignedFactionCount : assignedGroupCount);
		Func<int, List<LobbyPlayer>, List<LobbyPlayer>> getGroupPlayers = GetFactionPlayers;
		Func<LobbyPlayer, int> getPlayerGroup = (LobbyPlayer player) => player.factionNumber;
		return AreGroupsValid(minFactionCount, maxFactionCount, minPlayersPerFaction, maxPlayersPerFactions, groupCount, assignedGroupCount, getGroupPlayers, getPlayerGroup);
	}

	private static bool AreTeamsValid(int assignedGroupCount = -1)
	{
		if (IsLegacySavedGame())
		{
			return true;
		}
		int minTeamCount = MinTeamCount;
		int maxTeamCount = MaxTeamCount;
		int minPlayersPerTeam = instance.Parameters.MinPlayersPerTeam;
		int maxPlayersPerTeam = instance.Parameters.MaxPlayersPerTeam;
		int groupCount = CurrentTeamInfos.Length;
		assignedGroupCount = ((assignedGroupCount == -1) ? AssignedTeamCount : assignedGroupCount);
		Func<int, List<LobbyPlayer>, List<LobbyPlayer>> getGroupPlayers = GetTeamPlayers;
		Func<LobbyPlayer, int> getPlayerGroup = (LobbyPlayer player) => player.TeamNumber;
		return AreGroupsValid(minTeamCount, maxTeamCount, minPlayersPerTeam, maxPlayersPerTeam, groupCount, assignedGroupCount, getGroupPlayers, getPlayerGroup);
	}

	private static bool IsGroupAssignable(int dstGroup, int srcGroup, int minGroupCount, int maxGroupCount, int minGroupPlayersCount, int maxGroupPlayersCount, Func<int, List<LobbyPlayer>, List<LobbyPlayer>> getGroupPlayers, int assignedGroupCount)
	{
		if (dstGroup == srcGroup)
		{
			return true;
		}
		List<LobbyPlayer> list = getGroupPlayers(srcGroup, CollectionPool.RequestList<LobbyPlayer>());
		List<LobbyPlayer> list2 = getGroupPlayers(dstGroup, CollectionPool.RequestList<LobbyPlayer>());
		int count = list.Count;
		int count2 = list2.Count;
		list.Dispose();
		list2.Dispose();
		bool flag = count2 == 0;
		bool flag2 = count == 1;
		if (flag && flag2)
		{
			return true;
		}
		if (flag && maxGroupCount != -1 && assignedGroupCount + 1 > maxGroupCount)
		{
			return false;
		}
		if (flag2 && minGroupCount != -1 && assignedGroupCount - 1 < minGroupCount)
		{
			return false;
		}
		return true;
	}

	public static bool IsFactionAssignable(int dstGroup, int srcGroup, int assignedGroupCount = -1)
	{
		int minFactionCount = MinFactionCount;
		int maxFactionCount = MaxFactionCount;
		int minPlayersPerFaction = instance.Parameters.MinPlayersPerFaction;
		int maxPlayersPerFactions = instance.Parameters.MaxPlayersPerFactions;
		assignedGroupCount = ((assignedGroupCount == -1) ? AssignedFactionCount : assignedGroupCount);
		Func<int, List<LobbyPlayer>, List<LobbyPlayer>> getGroupPlayers = GetFactionPlayers;
		return IsGroupAssignable(dstGroup, srcGroup, minFactionCount, maxFactionCount, minPlayersPerFaction, maxPlayersPerFactions, getGroupPlayers, assignedGroupCount);
	}

	public static bool IsTeamAssignable(int dstGroup, int srcGroup, int assignedGroupCount = -1)
	{
		if (IsLegacySavedGame())
		{
			return true;
		}
		int minTeamCount = MinTeamCount;
		int maxTeamCount = MaxTeamCount;
		int minPlayersPerTeam = instance.Parameters.MinPlayersPerTeam;
		int maxPlayersPerTeam = instance.Parameters.MaxPlayersPerTeam;
		assignedGroupCount = ((assignedGroupCount == -1) ? AssignedTeamCount : assignedGroupCount);
		Func<int, List<LobbyPlayer>, List<LobbyPlayer>> getGroupPlayers = GetTeamPlayers;
		return IsGroupAssignable(dstGroup, srcGroup, minTeamCount, maxTeamCount, minPlayersPerTeam, maxPlayersPerTeam, getGroupPlayers, assignedGroupCount);
	}

	private static void FixGroup(int minGroupCount, int maxGroupCount, int minGroupPlayerCount, int maxGroupPlayerCount, int groupCount, Func<int, List<LobbyPlayer>, List<LobbyPlayer>> getGroupPlayers, Func<LobbyPlayer, int> getPlayerGroup, Action<LobbyPlayer, int> setPlayerGroup)
	{
		Dictionary<int, List<LobbyPlayer>> dictionary = CollectionPool.RequestDictionary<int, List<LobbyPlayer>>();
		Dictionary<int, List<int>> dictionary2 = CollectionPool.RequestDictionary<int, List<int>>();
		List<int> list = CollectionPool.RequestList<int>();
		List<int> list2 = CollectionPool.RequestList<int>();
		List<int> list3 = CollectionPool.RequestList<int>();
		List<int> list4 = CollectionPool.RequestList<int>();
		List<LobbyPlayer> players = GetPlayers(CollectionPool.RequestList<LobbyPlayer>());
		for (int i = 0; i < groupCount; i++)
		{
			List<LobbyPlayer> list5 = getGroupPlayers(i, CollectionPool.RequestList<LobbyPlayer>());
			dictionary.Add(i, list5);
			dictionary2.GetValueOrCreate(list5.Count, CollectionPool.RequestList<int>).Add(i);
			list.Add(i);
		}
		foreach (LobbyPlayer item2 in players)
		{
			int num = getPlayerGroup(item2);
			if (num < 0 || num >= groupCount)
			{
				dictionary[0].Add(item2);
			}
		}
		(from kvp in dictionary2.Slinq()
			select kvp.Key).AddTo(list2);
		list2.Sort();
		list2.Reverse();
		for (int num2 = 0; num2 < list2.Count; num2++)
		{
			int num3 = list2[num2];
			if (num3 == 0)
			{
				continue;
			}
			foreach (int item3 in dictionary2[num3])
			{
				if (list3.Count < maxGroupCount && maxGroupCount != -1)
				{
					if (!list3.Contains(item3))
					{
						list3.Add(item3);
					}
					if (list.Contains(item3))
					{
						list.Remove(item3);
					}
				}
				else
				{
					if (!list4.Contains(item3))
					{
						list4.Add(item3);
					}
					if (list.Contains(item3))
					{
						list.Remove(item3);
					}
				}
			}
		}
		while (list3.Count < minGroupCount && minGroupCount != -1)
		{
			if (list.Count > 0)
			{
				int item = list[0];
				if (!list3.Contains(item))
				{
					list3.Add(item);
				}
				if (list.Contains(item))
				{
					list.Remove(item);
				}
				continue;
			}
			logger.ErrorFormat("Group range makes no sense? [{0}, {1}]", minGroupCount, maxGroupCount);
			break;
		}
		dictionary2.Slinq().ForEach(delegate(KeyValuePair<int, List<int>> keyValuePair)
		{
			keyValuePair.Value.Dispose();
		});
		dictionary2.Dispose();
		list2.Dispose();
		list.Dispose();
		players.Dispose();
		List<int> list6 = list4.ToPooledList();
		List<int> list7 = list3.ToPooledList();
		foreach (int item4 in list6)
		{
			if (!dictionary.TryGetValue(item4, out var value))
			{
				continue;
			}
			List<LobbyPlayer> list8 = value.ToPooledList();
			foreach (LobbyPlayer item5 in list8)
			{
				foreach (int item6 in list7)
				{
					if (item6 == item4 || !dictionary.TryGetValue(item6, out var value2))
					{
						continue;
					}
					int count = value2.Count;
					if (count + 1 <= maxGroupPlayerCount || maxGroupPlayerCount == -1)
					{
						if (value.Contains(item5))
						{
							value.Remove(item5);
						}
						if (!value2.Contains(item5))
						{
							value2.Add(item5);
						}
						if (list4.Contains(item4))
						{
							list4.Remove(item4);
						}
						if (!list3.Contains(item6))
						{
							list3.Add(item6);
						}
					}
				}
			}
			list8.Dispose();
		}
		list6.Dispose();
		list7.Dispose();
		List<int> list9 = list3.ToPooledList();
		List<int> list10 = list3.ToPooledList();
		foreach (int item7 in list9)
		{
			if (!dictionary.TryGetValue(item7, out var value3))
			{
				continue;
			}
			List<LobbyPlayer> list11 = value3.ToPooledList();
			foreach (LobbyPlayer item8 in list11)
			{
				foreach (int item9 in list10)
				{
					if (item9 == item7 || !dictionary.TryGetValue(item9, out var value4))
					{
						continue;
					}
					int count2 = value3.Count;
					if (count2 - 1 < minGroupPlayerCount && minGroupPlayerCount != -1)
					{
						continue;
					}
					int count3 = value4.Count;
					if (count3 + 1 <= maxGroupPlayerCount || maxGroupPlayerCount == -1)
					{
						if (value3.Contains(item8))
						{
							value3.Remove(item8);
						}
						if (!value4.Contains(item8))
						{
							value4.Add(item8);
						}
					}
				}
			}
			list11.Dispose();
		}
		list9.Dispose();
		list10.Dispose();
		for (int num4 = 0; num4 < groupCount; num4++)
		{
			List<LobbyPlayer> list12 = dictionary[num4];
			foreach (LobbyPlayer item10 in list12)
			{
				if (getPlayerGroup(item10) != num4)
				{
					setPlayerGroup(item10, num4);
				}
			}
		}
		dictionary.Slinq().ForEach(delegate(KeyValuePair<int, List<LobbyPlayer>> keyValuePair)
		{
			keyValuePair.Value.Dispose();
		});
		dictionary.Dispose();
		list3.Dispose();
		list4.Dispose();
	}

	private static void FixFactions()
	{
		int minFactionCount = MinFactionCount;
		int maxFactionCount = MaxFactionCount;
		int minPlayersPerFaction = instance.Parameters.MinPlayersPerFaction;
		int maxPlayersPerFactions = instance.Parameters.MaxPlayersPerFactions;
		int groupCount = CurrentFactionInfos.Length;
		Func<int, List<LobbyPlayer>, List<LobbyPlayer>> getGroupPlayers = GetFactionPlayers;
		Func<LobbyPlayer, int> getPlayerGroup = (LobbyPlayer player) => player.factionNumber;
		Action<LobbyPlayer, int> setPlayerGroup = delegate(LobbyPlayer player, int group)
		{
			player.NetworkfactionNumber = group;
		};
		FixGroup(minFactionCount, maxFactionCount, minPlayersPerFaction, maxPlayersPerFactions, groupCount, getGroupPlayers, getPlayerGroup, setPlayerGroup);
	}

	private static void FixTeams()
	{
		int minTeamCount = MinTeamCount;
		int maxTeamCount = MaxTeamCount;
		int minPlayersPerTeam = instance.Parameters.MinPlayersPerTeam;
		int maxPlayersPerTeam = instance.Parameters.MaxPlayersPerTeam;
		int groupCount = CurrentTeamInfos.Length;
		Func<int, List<LobbyPlayer>, List<LobbyPlayer>> getGroupPlayers = GetTeamPlayers;
		Func<LobbyPlayer, int> getPlayerGroup = (LobbyPlayer player) => player.TeamNumber;
		Action<LobbyPlayer, int> setPlayerGroup = delegate(LobbyPlayer player, int group)
		{
			player.NetworkTeamNumber = group;
		};
		FixGroup(minTeamCount, maxTeamCount, minPlayersPerTeam, maxPlayersPerTeam, groupCount, getGroupPlayers, getPlayerGroup, setPlayerGroup);
	}

	public static List<LobbyPlayer> GetTeamPlayers(int team, List<LobbyPlayer> teamPlayers)
	{
		return UNetManager.instance.LobbyPlayerList.Slinq().Where((LobbyPlayer player, int aTeam) => player.TeamNumber == aTeam, team).AddTo(teamPlayers);
	}

	private static void RefreshTeams()
	{
		if (Map != null && LobbyMenu.IsHost)
		{
			int assignedTeamCount = AssignedTeamCount;
			if (!AreTeamsValid(assignedTeamCount))
			{
				FixTeams();
			}
		}
	}

	public static List<LobbyPlayer> GetPlayers(List<LobbyPlayer> players)
	{
		return UNetManager.instance.LobbyPlayerList.Slinq().AddTo(players);
	}

	private static bool IsLegacySavedGame()
	{
		if (Map != null && !Map.IsNewGame())
		{
			Asset_GamemodePreset lastPlayedGamemodePreset = Map.GetLastPlayedGamemodePreset();
			if (lastPlayedGamemodePreset == null)
			{
				return true;
			}
		}
		return false;
	}

	static GameParam()
	{
		logger = new BrixLogger(typeof(GameParam).FullName);
		OnSetMapSignal = new GameSignals.GameSignal();
		OnSetModeSignal = new GameSignals.GameSignal();
		OnSetPresetSignal = new GameSignals.GameSignal();
		OnSetSelectedMapSignal = new GameSignals.GameSignal();
		OnSetSelectedModeSignal = new GameSignals.GameSignal();
		OnSetSelectedPresetSignal = new GameSignals.GameSignal();
		OnSyncLimitsSignal = new GameSignals.GameSignal();
		OnAfterPlayLoadingAnimation = delegate
		{
			UNetManager.instance.LocalLobbyPlayer.SendReadyToBeginMessage();
		};
		_currentFactionInfos = new FactionInfo[0];
		CurrentTeamInfos = new TeamInfo[4]
		{
			new TeamInfo(null, "Team A"),
			new TeamInfo(null, "Team B"),
			new TeamInfo(null, "Team C"),
			new TeamInfo(null, "Team D")
		};
		kEventEventPlayLoadingAnimation = -676045385;
		NetworkBehaviour.RegisterEventDelegate(typeof(GameParam), kEventEventPlayLoadingAnimation, InvokeSyncEventEventPlayLoadingAnimation);
		NetworkCRC.RegisterBehaviour("GameParam", 0);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeSyncEventEventPlayLoadingAnimation(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Event EventPlayLoadingAnimation called on server.");
		}
		else if (((GameParam)obj).EventPlayLoadingAnimation != null)
		{
			((GameParam)obj).EventPlayLoadingAnimation();
		}
	}

	public void CallEventPlayLoadingAnimation()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Event EventPlayLoadingAnimation called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)7);
		networkWriter.WritePackedUInt32((uint)kEventEventPlayLoadingAnimation);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendEventInternal(networkWriter, 0, "EventPlayLoadingAnimation");
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(HostLoadedAllFactions);
			writer.Write(HostLoaded);
			writer.Write(HostReady);
			GeneratedNetworkCode._WriteGameParameters_GameParam(writer, Parameters);
			writer.Write(_isInitialized);
			writer.Write(mapChecksum);
			return true;
		}
		bool flag = false;
		if ((base.syncVarDirtyBits & 1) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(HostLoadedAllFactions);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(HostLoaded);
		}
		if ((base.syncVarDirtyBits & 4) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(HostReady);
		}
		if ((base.syncVarDirtyBits & 8) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			GeneratedNetworkCode._WriteGameParameters_GameParam(writer, Parameters);
		}
		if ((base.syncVarDirtyBits & 0x10) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(_isInitialized);
		}
		if ((base.syncVarDirtyBits & 0x20) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(mapChecksum);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			HostLoadedAllFactions = reader.ReadBoolean();
			HostLoaded = reader.ReadBoolean();
			HostReady = reader.ReadBoolean();
			Parameters = GeneratedNetworkCode._ReadGameParameters_GameParam(reader);
			_isInitialized = reader.ReadBoolean();
			mapChecksum = reader.ReadString();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			HostLoadedAllFactions = reader.ReadBoolean();
		}
		if ((num & 2) != 0)
		{
			HostLoaded = reader.ReadBoolean();
		}
		if ((num & 4) != 0)
		{
			HostReady = reader.ReadBoolean();
		}
		if ((num & 8) != 0)
		{
			ParametersChanged(GeneratedNetworkCode._ReadGameParameters_GameParam(reader));
		}
		if ((num & 0x10) != 0)
		{
			_isInitialized = reader.ReadBoolean();
		}
		if ((num & 0x20) != 0)
		{
			mapChecksum = reader.ReadString();
		}
	}
}
