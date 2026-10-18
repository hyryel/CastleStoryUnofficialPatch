using System;
using System.Collections;
using System.Collections.Generic;
using Brix.Assets;
using Brix.Engine;
using Brix.External.Factories;
using Brix.Game.AI;
using Brix.Game.Bricktron;
using Brix.Game.Components;
using Brix.Game.Network;
using Brix.Utils.Logging;
using Newtonsoft.Json.Linq;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Brix.Game;

public class StatsTracker : MonoBehaviour
{
	[Flags]
	private enum StatsStatus
	{
		Nothing = 0,
		Recording = 1,
		DebugMode = 2,
		Legit = 4,
		Verbose = 8
	}

	public static class Achievements
	{
		private static string[] ach_lightsout_list = new string[2] { "conquest_lightsout_660d9eb5-ab1a-49ee-8456-5e815682d8a0", "conquest_coop_lightsout_f5080be3-2d68-4c5c-8221-805eded708a1" };

		private static string[] ach_bricks_list = new string[24]
		{
			"PlacedBlock.Stone_Brick", "PlacedBlock.Stone_Brick_Round", "PlacedBlock.Stone_Column_Base", "PlacedBlock.Stone_Column_Middle", "PlacedBlock.Stone_Column_Top", "PlacedBlock.Stone_DoubleBrick", "PlacedBlock.Stone_DoubleBrick_RoundL", "PlacedBlock.Stone_DoubleBrick_RoundLR", "PlacedBlock.Stone_DoubleBrick_RoundR", "PlacedBlock.Stone_Arch",
			"PlacedBlock.Stone_Staircase", "PlacedBlock.Stone_BeamPlug_AllSides", "PlacedBlock.Stone_BeamPlug_Corner", "PlacedBlock.Stone_BeamPlug_TwoSides", "PlacedBlock.Stone_BeamPlug_OneSide", "PlacedBlock.Stone_BeamPlug_ThreeSides", "PlacedBlock.Stone_Brace1", "PlacedBlock.Stone_Brace2", "PlacedBlock.Stone_Merlon", "PlacedBlock.Stone_MerlonCorner",
			"PlacedBlock.Stone_Parapet1", "PlacedBlock.Stone_Parapet2", "PlacedBlock.Stone_Parapet3", "PlacedBlock.Clay_Brick"
		};

		private static void Unlock(string ach)
		{
			SteamUserStats.SetAchievement(ach);
			SteamUserStats.StoreStats();
		}

		private static void IncrementStat(string stat)
		{
			int pData = 0;
			SteamUserStats.GetStat(stat, out pData);
			SteamUserStats.SetStat(stat, pData + 1);
			SteamUserStats.StoreStats();
		}

		private static void SetStat(string stat, int num)
		{
			SteamUserStats.SetStat(stat, num);
			SteamUserStats.StoreStats();
		}

		public static void Check_DefeatABiftron()
		{
			if (SteamManager.Initialized && GetValue(_charactersKilled, Test.Biftron.ToString()) > 0)
			{
				Unlock("ACH_DEFEAT_A_BIFTRON");
			}
		}

		public static void Check_DefeatAWarlock()
		{
			if (SteamManager.Initialized && GetValue(_charactersKilled, "Test.Magitron") > 0)
			{
				Unlock("ACH_DEFEAT_A_WARLOCK");
			}
		}

		public static void Check_BricksPlaced(Factory.AssetKey key)
		{
			if (!SteamManager.Initialized)
			{
				return;
			}
			string text = key.ToString();
			for (int i = 0; i != ach_bricks_list.Length; i++)
			{
				if (ach_bricks_list[i] == text)
				{
					IncrementStat("STAT_BRICKS_PLACED");
					break;
				}
			}
		}

		public static void Check_AgentBouge()
		{
			if (SteamManager.Initialized && GetValue(_charactersSpawned, "Bougre") > 0)
			{
				Unlock("ACH_AGENT_BOUGRE");
			}
		}

		public static void Check_WavesAchs()
		{
			if (SteamManager.Initialized)
			{
				if (GetValue(_wavesSurvived, 15) > 0)
				{
					Unlock("ACH_SURVIVED_15_WAVES");
				}
				if (GetValue(_wavesSurvived, 30) > 0)
				{
					Unlock("ACH_SURVIVED_30_WAVES");
				}
				if (GetValue(_wavesSurvived, 60) > 0)
				{
					Unlock("ACH_SURVIVED_60_WAVES");
				}
			}
		}

		public static void Check_WinLightsOut()
		{
			if (SteamManager.Initialized && GetTotal(_presetWon, ach_lightsout_list) > 0)
			{
				Unlock("ACH_WON_LIGHTSOUT");
			}
		}
	}

	public static GameSignals.GameSignal<int> OnInvasionWave;

	protected static Dictionary<string, int> _charactersSpawned;

	protected static Dictionary<string, int> _charactersKilled;

	private static Dictionary<string, int> _gamemodesPlayed;

	private static Dictionary<string, int> _objectsBuilt;

	private static Dictionary<string, int> _presetPlayed;

	private static Dictionary<string, int> _presetLost;

	private static Dictionary<string, int> _presetWon;

	private static Dictionary<string, int> _mapPlayed;

	private static Dictionary<string, int> _gamesLost;

	private static Dictionary<string, int> _gamesWon;

	private static Dictionary<int, int> _wavesSurvived;

	private static Dictionary<string, int> _mapLost;

	private static Dictionary<string, int> _mapWon;

	public static GameSignals.GameSignal OnGameLost;

	public static GameSignals.GameSignal OnGameWon;

	private static readonly string _WAVES_SURVIVED;

	private static readonly string _PRESET_PLAYED;

	private static readonly string _CHARS_SPAWNED;

	private static readonly string _OBJECTS_BUILT;

	private static readonly string _SHARDS_CAPT;

	private static readonly string _GM_PLAYED;

	private static readonly string _CHARS_KILLED;

	private static readonly string _HOMES_CAPT;

	private static readonly string _PRESET_LOST;

	private static readonly string _MGS_SENT;

	private static readonly string _MAP_PLAYED;

	private static readonly string _GAMES_LOST;

	private static readonly string _PRESET_WON;

	private static readonly string _VOXEL_DUG;

	private static readonly string _GAMES_WON;

	private static readonly string _MAP_LOST;

	private static readonly string _SP_GAMES;

	private static readonly string _MP_GAMES;

	private static readonly string _MAP_WON;

	private static BrixLogger logger;

	private static int _highestWaveOfSession;

	private static int _shardsCaptured;

	private static int _homesCaptures;

	private static int _messagesSent;

	private static int _voxelDug;

	private static int _spGames;

	private static int _mpGames;

	private static StatsStatus _status;

	static StatsTracker()
	{
		OnInvasionWave = new GameSignals.GameSignal<int>();
		_charactersSpawned = new Dictionary<string, int>();
		_charactersKilled = new Dictionary<string, int>();
		_gamemodesPlayed = new Dictionary<string, int>();
		_objectsBuilt = new Dictionary<string, int>();
		_presetPlayed = new Dictionary<string, int>();
		_presetLost = new Dictionary<string, int>();
		_presetWon = new Dictionary<string, int>();
		_mapPlayed = new Dictionary<string, int>();
		_gamesLost = new Dictionary<string, int>();
		_gamesWon = new Dictionary<string, int>();
		_wavesSurvived = new Dictionary<int, int>();
		_mapLost = new Dictionary<string, int>();
		_mapWon = new Dictionary<string, int>();
		OnGameLost = new GameSignals.GameSignal();
		OnGameWon = new GameSignals.GameSignal();
		_WAVES_SURVIVED = "WavesSurvived";
		_PRESET_PLAYED = "PresetsPlayed";
		_CHARS_SPAWNED = "CharsSpawned";
		_OBJECTS_BUILT = "ObjectsBuilt";
		_SHARDS_CAPT = "ShardsCaptuded";
		_GM_PLAYED = "GamemodesPlayed";
		_CHARS_KILLED = "CharsKilled";
		_HOMES_CAPT = "HomesCaptuded";
		_PRESET_LOST = "PresetLost";
		_MGS_SENT = "MessagesSent";
		_MAP_PLAYED = "MapPlayed";
		_GAMES_LOST = "GamesLost";
		_PRESET_WON = "PresetWon";
		_VOXEL_DUG = "VoxelsDug";
		_GAMES_WON = "GamesWon";
		_MAP_LOST = "MapLost";
		_SP_GAMES = "SPGames";
		_MP_GAMES = "MPGames";
		_MAP_WON = "MapWon";
		logger = new BrixLogger(typeof(StatsTracker).FullName);
		_highestWaveOfSession = -1;
		_shardsCaptured = 0;
		_homesCaptures = 0;
		_messagesSent = 0;
		_voxelDug = 0;
		_spGames = 0;
		_mpGames = 0;
		_status = StatsStatus.Nothing;
		SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
	}

	public static void LoadStats(Hashtable table)
	{
		if (table != null)
		{
			if (table.ContainsKey(_CHARS_SPAWNED))
			{
				_charactersSpawned = ConvertFromJSONDict<string>(table[_CHARS_SPAWNED]);
			}
			if (table.ContainsKey(_CHARS_KILLED))
			{
				_charactersKilled = ConvertFromJSONDict<string>(table[_CHARS_KILLED]);
			}
			if (table.ContainsKey(_WAVES_SURVIVED))
			{
				_wavesSurvived = ConvertFromJSONDict<int>(table[_WAVES_SURVIVED]);
			}
			if (table.ContainsKey(_OBJECTS_BUILT))
			{
				_objectsBuilt = ConvertFromJSONDict<string>(table[_OBJECTS_BUILT]);
			}
			if (table.ContainsKey(_PRESET_PLAYED))
			{
				_presetPlayed = ConvertFromJSONDict<string>(table[_PRESET_PLAYED]);
			}
			if (table.ContainsKey(_GM_PLAYED))
			{
				_gamemodesPlayed = ConvertFromJSONDict<string>(table[_GM_PLAYED]);
			}
			if (table.ContainsKey(_PRESET_LOST))
			{
				_presetLost = ConvertFromJSONDict<string>(table[_PRESET_LOST]);
			}
			if (table.ContainsKey(_MAP_PLAYED))
			{
				_mapPlayed = ConvertFromJSONDict<string>(table[_MAP_PLAYED]);
			}
			if (table.ContainsKey(_GAMES_LOST))
			{
				_gamesLost = ConvertFromJSONDict<string>(table[_GAMES_LOST]);
			}
			if (table.ContainsKey(_PRESET_WON))
			{
				_presetWon = ConvertFromJSONDict<string>(table[_PRESET_WON]);
			}
			if (table.ContainsKey(_SHARDS_CAPT))
			{
				_shardsCaptured = ConvertFromJSONint(table[_SHARDS_CAPT]);
			}
			if (table.ContainsKey(_GAMES_WON))
			{
				_gamesWon = ConvertFromJSONDict<string>(table[_GAMES_WON]);
			}
			if (table.ContainsKey(_HOMES_CAPT))
			{
				_homesCaptures = ConvertFromJSONint(table[_HOMES_CAPT]);
			}
			if (table.ContainsKey(_MAP_LOST))
			{
				_mapLost = ConvertFromJSONDict<string>(table[_MAP_LOST]);
			}
			if (table.ContainsKey(_MAP_WON))
			{
				_mapWon = ConvertFromJSONDict<string>(table[_MAP_WON]);
			}
			if (table.ContainsKey(_MGS_SENT))
			{
				_messagesSent = ConvertFromJSONint(table[_MGS_SENT]);
			}
			if (table.ContainsKey(_VOXEL_DUG))
			{
				_voxelDug = ConvertFromJSONint(table[_VOXEL_DUG]);
			}
			if (table.ContainsKey(_SP_GAMES))
			{
				_spGames = ConvertFromJSONint(table[_SP_GAMES]);
			}
			if (table.ContainsKey(_MP_GAMES))
			{
				_mpGames = ConvertFromJSONint(table[_MP_GAMES]);
			}
		}
		_status |= StatsStatus.Legit;
	}

	public static Hashtable SaveStats()
	{
		if (!IsLegit())
		{
			return null;
		}
		Hashtable hashtable = new Hashtable();
		hashtable.Add(_CHARS_SPAWNED, _charactersSpawned);
		hashtable.Add(_CHARS_KILLED, _charactersKilled);
		hashtable.Add(_OBJECTS_BUILT, _objectsBuilt);
		hashtable.Add(_PRESET_PLAYED, _presetPlayed);
		hashtable.Add(_SHARDS_CAPT, _shardsCaptured);
		hashtable.Add(_GM_PLAYED, _gamemodesPlayed);
		hashtable.Add(_HOMES_CAPT, _homesCaptures);
		hashtable.Add(_PRESET_LOST, _presetLost);
		hashtable.Add(_MGS_SENT, _messagesSent);
		hashtable.Add(_MAP_PLAYED, _mapPlayed);
		hashtable.Add(_WAVES_SURVIVED, _wavesSurvived);
		hashtable.Add(_GAMES_LOST, _gamesLost);
		hashtable.Add(_PRESET_WON, _presetWon);
		hashtable.Add(_GAMES_WON, _gamesWon);
		hashtable.Add(_VOXEL_DUG, _voxelDug);
		hashtable.Add(_MAP_LOST, _mapLost);
		hashtable.Add(_SP_GAMES, _spGames);
		hashtable.Add(_MP_GAMES, _mpGames);
		hashtable.Add(_MAP_WON, _mapWon);
		return hashtable;
	}

	public static bool IsLegit()
	{
		return (_status & StatsStatus.Legit) != 0;
	}

	public static bool IsRecording()
	{
		return (_status & StatsStatus.Recording) != StatsStatus.Nothing && !IsDebugMode();
	}

	public static bool IsDebugMode()
	{
		if (GameParam.Map != null)
		{
			return GameParam.Map.isDebug;
		}
		return (_status & StatsStatus.DebugMode) != 0;
	}

	public static bool IsVerbose()
	{
		return (_status & StatsStatus.Verbose) != 0;
	}

	public static void StartRecording()
	{
		_status |= StatsStatus.Recording;
	}

	public static void StopRecording()
	{
		_status &= ~StatsStatus.Recording;
		_status &= ~StatsStatus.DebugMode;
	}

	public static void SetVerbose(bool verbose)
	{
		if (verbose)
		{
			_status |= StatsStatus.Verbose;
		}
		else
		{
			_status &= ~StatsStatus.Verbose;
		}
	}

	private static Dictionary<T, int> ConvertFromJSONDict<T>(object jsonObject)
	{
		if (jsonObject != null)
		{
			try
			{
				return ((JObject)jsonObject).ToObject<Dictionary<T, int>>();
			}
			catch (Exception ex)
			{
				logger.Warn("Stats: Couldn't convert dict\n" + ex.Message);
			}
		}
		return new Dictionary<T, int>();
	}

	private static int ConvertFromJSONint(object jsonObject)
	{
		if (jsonObject != null)
		{
			try
			{
				return (int)(long)jsonObject;
			}
			catch (Exception ex)
			{
				logger.Warn("Stats: Couldn't convert\n" + ex.Message);
			}
		}
		return 0;
	}

	private static int GetValue<Key>(Dictionary<Key, int> dict, Key key)
	{
		try
		{
			int value = 0;
			if (dict.TryGetValue(key, out value))
			{
				return value;
			}
		}
		catch (Exception ex)
		{
			logger.Warn("Stats: Couldn't get value\n" + ex.Message);
		}
		return 0;
	}

	private static int GetTotal<Key>(Dictionary<Key, int> dict, IList<Key> keys)
	{
		int num = 0;
		foreach (Key key in keys)
		{
			try
			{
				int value = 0;
				if (dict.TryGetValue(key, out value))
				{
					num += value;
				}
			}
			catch (Exception ex)
			{
				logger.Warn("Stats: Couldn't get value\n" + ex.Message);
			}
		}
		return num;
	}

	private static int Increment<Key>(Dictionary<Key, int> dict, Key key, string verboseTip)
	{
		try
		{
			if (!dict.ContainsKey(key))
			{
				dict[key] = 1;
				if (IsVerbose())
				{
					Debug.Log(string.Concat(verboseTip, key, " +now ", 1));
				}
				return 1;
			}
			int num = ++dict[key];
			if (IsVerbose())
			{
				Debug.Log(string.Concat(verboseTip, key, " +now ", num));
			}
			return num;
		}
		catch (Exception ex)
		{
			logger.Warn(string.Concat("Stats: Couldn't increment ", (!string.IsNullOrEmpty(verboseTip)) ? verboseTip : "key ", key, "\n", ex.Message));
		}
		return 0;
	}

	private static int Increment(ref int value, string verboseTip)
	{
		value++;
		if (IsVerbose())
		{
			Debug.Log(verboseTip + " +now " + value);
		}
		return value;
	}

	private static int Decrement<Key>(Dictionary<Key, int> dict, Key key, string verboseTip)
	{
		try
		{
			if (dict.ContainsKey(key))
			{
				int num = dict[key] - 1;
				if (num < 0)
				{
					num = 0;
				}
				dict[key] = num;
				if (IsVerbose())
				{
					Debug.Log(string.Concat(verboseTip, key, " -now ", num));
				}
				return num;
			}
		}
		catch (Exception ex)
		{
			logger.Warn(string.Concat("Stats: Couldn't decrement ", (!string.IsNullOrEmpty(verboseTip)) ? verboseTip : "key ", key, "\n", ex.Message));
		}
		return 0;
	}

	private static int Decrement(ref int value, string verboseTip)
	{
		value--;
		if (value < 0)
		{
			value = 0;
		}
		if (IsVerbose())
		{
			Debug.Log(verboseTip + " -now " + value);
		}
		return value;
	}

	public static void StartingGame(Asset_Map map, Asset_Gamemode gamemode, Asset_GamemodePreset preset, bool isMP)
	{
		if (IsRecording())
		{
			if (map != null)
			{
				Increment(_mapPlayed, map.codename, "mapPlayed ");
			}
			if (gamemode != null)
			{
				Increment(_gamemodesPlayed, gamemode.codename, "gamemodesPlayed ");
			}
			if (preset != null)
			{
				Increment(_presetPlayed, preset.codename, "presetPlayed ");
			}
			if (isMP)
			{
				Increment(ref _mpGames, "mpGames ");
			}
			else
			{
				Increment(ref _spGames, "spGames ");
			}
		}
	}

	public static void UnitSpawned(Labor unit)
	{
		if (IsRecording() && unit != null && unit.faction == User.LocalUser.faction)
		{
			Increment(_charactersSpawned, unit.GetComponent<Nom>().GetNom(), "unitSpawned");
			Achievements.Check_AgentBouge();
		}
	}

	public static void UnitDied(GameObject victim, GameObject murderer)
	{
		if (!IsRecording())
		{
			return;
		}
		try
		{
			Faction faction = victim.GetComponent<AffiliationComponent>().faction;
			if (!(murderer != null))
			{
				return;
			}
			AffiliationComponent component = murderer.GetComponent<AffiliationComponent>();
			if (component != null)
			{
				Faction faction2 = component.faction;
				if (faction2 == User.LocalUser.faction)
				{
					Increment(_charactersKilled, victim.GetComponent<FactoryImprint>().AssetKey.ToString(), "unitKilled");
					Achievements.Check_DefeatABiftron();
					Achievements.Check_DefeatAWarlock();
				}
			}
		}
		catch (Exception ex)
		{
			logger.Warn("Stats: Couldn't increment _charactersKilled\n" + ex.Message);
		}
	}

	public static void ObjectBuilt(Faction faction, Factory.AssetKey key)
	{
		if (IsRecording() && key != null && faction != null && faction == User.LocalUser.faction)
		{
			Increment(_objectsBuilt, key.ToString(), "objectsBuilt");
			Achievements.Check_BricksPlaced(key);
		}
	}

	public static void SentMessage()
	{
		if (IsRecording())
		{
			_messagesSent++;
			if (IsVerbose())
			{
				Debug.Log("_messagesSent now " + _messagesSent);
			}
		}
	}

	public static void DugVoxel(GameObject murderer)
	{
		if (!IsRecording() || !(murderer != null))
		{
			return;
		}
		try
		{
			AffiliationComponent component = murderer.GetComponent<AffiliationComponent>();
			if (component != null)
			{
				Faction faction = component.faction;
				if (faction == User.LocalUser.faction)
				{
					_voxelDug++;
				}
			}
		}
		catch (Exception ex)
		{
			logger.Warn("Stats: Couldn't increment _voxelDug\n" + ex.Message);
		}
	}

	public static void InvasionWave(int number)
	{
		if (IsRecording() && number > _highestWaveOfSession)
		{
			Increment(_wavesSurvived, number, "InvasionWave ");
			_highestWaveOfSession = number;
			Achievements.Check_WavesAchs();
		}
	}

	public static void GameWon()
	{
		if (IsRecording())
		{
			Increment(_gamesWon, GameParam.Gamemode.codename, "GamesWon ");
			Increment(_presetWon, GameParam.Preset.codename, "PresetWon ");
			Increment(_mapWon, GameParam.Map.codename, "MapWon ");
			Achievements.Check_WinLightsOut();
		}
	}

	public static void GameLost()
	{
		if (IsRecording())
		{
			Increment(_gamesLost, GameParam.Gamemode.codename, "GamesLost ");
			Increment(_presetLost, GameParam.Preset.codename, "PresetLost ");
			Increment(_mapLost, GameParam.Map.codename, "MapLost ");
		}
	}

	public static void GoingToGame()
	{
		try
		{
			_status &= ~StatsStatus.DebugMode;
			_highestWaveOfSession = -1;
			GameSignals.Connect(GameSignals.UnitDied, delegate(GameObject v, GameObject m)
			{
				UnitDied(v, m);
			});
			GameSignals.Connect(OnInvasionWave, delegate(int n)
			{
				InvasionWave(n);
			});
			GameSignals.Connect(OnGameWon, delegate
			{
				GameWon();
			});
			GameSignals.Connect(OnGameLost, delegate
			{
				GameLost();
			});
		}
		catch (Exception ex)
		{
			logger.Warn("Stats: Couldn't hook gamesignals\n" + ex.Message);
		}
	}

	private void Awake()
	{
		AssetDispensary.stats.Disk_ReadMeta();
	}

	private static void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
	{
		try
		{
			if (IsLegit())
			{
				AssetDispensary.stats.Disk_Overwrite();
			}
		}
		catch (Exception ex)
		{
			logger.Warn("Stats: Save on scene changed\n" + ex.Message);
		}
	}

	private void OnDestroy()
	{
	}

	private void OnApplicationQuit()
	{
		try
		{
			if (IsLegit())
			{
				AssetDispensary.stats.Disk_Overwrite();
			}
		}
		catch (Exception ex)
		{
			logger.Warn("Stats: Save on destroy\n" + ex.Message);
		}
		_status &= ~StatsStatus.Legit;
	}
}
