using System;
using System.Collections;
using System.Collections.Generic;
using Brix.Assets;
using Brix.Audio;
using Brix.Components;
using Brix.Engine.Abstract;
using Brix.Engine.Blocks;
using Brix.Engine.Nature;
using Brix.Engine.Trackers;
using Brix.External.Factories;
using Brix.Game;
using Brix.IO.Serialization;
using Brix.Input;
using Brix.Legacy;
using Brix.Lua;
using Brix.Network;
using Brix.NewUI.LoadingScreen;
using Brix.Pathfinding.Test;
using Brix.UI.Builder.Menu.Component;
using Brix.Utils;
using MoonSharp.Interpreter;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Engine;

public class GameLoader
{
	private Asset_Map _mapToLoad;

	private Asset_Gamemode _gamemodeToLoad;

	private Asset_GamemodePreset _presetToLoad;

	private XYZ originOffset = XYZ.zero;

	private const int TerrainInitPerFrame = 10000;

	private const int TreeLoadPerFrame = 10000;

	public static GameLoader _instance;

	public int terrainInitSteps;

	public int pathfindingSteps;

	public int engineSteps;

	public int vegetationInitSteps;

	private int loadingProgress = -1;

	private int expectedLoadingProgress;

	private LoadingScreen loadingScreen;

	private int EstimateLoadingSteps()
	{
		int num = 0;
		if (_mapToLoad.version >= 6)
		{
			int num2 = Voxel.Engine.EstimateVisibleVoxelsInMap(_mapToLoad.path);
			num += (terrainInitSteps = num2 / 10000 * 2);
			num += (pathfindingSteps = TestPathfinding.Instance.EstimateLoadingSteps(num2));
			num += (engineSteps = num2 / 196 / 20);
		}
		_mapToLoad.Disk_EstimateTrees(out var treeCount, out var _);
		return num + (vegetationInitSteps = treeCount / 10000);
	}

	private IEnumerator LoadingStepsCheckpoint(int expectedStepIncrement)
	{
		if (loadingProgress < 0)
		{
			loadingProgress = 0;
			GameSignals.Connect(GameSignals.LoadingProgress, delegate
			{
				loadingProgress++;
			});
			GameSignals.Connect(GameSignals.LoadingProgressLeap, delegate(int leap)
			{
				loadingProgress += leap;
			});
		}
		while (loadingProgress < expectedLoadingProgress)
		{
			GameSignals.Invoke(t: Mathf.Min(100, expectedLoadingProgress - loadingProgress), gs: GameSignals.LoadingProgressLeap);
			yield return GameClock.WaitForEndOfFrame;
		}
		expectedLoadingProgress += expectedStepIncrement;
	}

	public static IEnumerator LoadNewGameFromGameParams(Action<Exception> onError)
	{
		GameLoader gameLoader = new GameLoader();
		gameLoader._mapToLoad = GameParam.Map;
		gameLoader._gamemodeToLoad = GameParam.Gamemode;
		gameLoader._presetToLoad = GameParam.Preset;
		IEnumerator enumerator = (_instance = gameLoader).Load();
		enumerator = Brix.Engine.Abstract.Neo.EnumAll(enumerator);
		return Brix.Engine.Abstract.Neo.TryEnum(enumerator, onError);
	}

	private IEnumerator Load()
	{
		yield return GameClock.WaitForEndOfFrame;
		if (!Neo.IsMultiplayer)
		{
			StatsTracker.GoingToGame();
		}
		GameSignals.Invoke(GameSignals.LoadingStarted);
		GameSignals.Invoke(GameSignals.LoadingGame, GameSignals.loadingContext.ingame);
		GameSignals.Invoke(GameSignals.MaxLoadingProgress, EstimateLoadingSteps());
		yield return BuildStep();
		yield return AwakeStep();
		yield return StartStep();
		StatsTracker.StartingGame(_mapToLoad, _gamemodeToLoad, _presetToLoad, Neo.IsMultiplayer);
	}

	private IEnumerator BuildStep()
	{
		loadingScreen = GameObject.Find("LoadingScreen").GetComponent<LoadingScreen>();
		while (!NetworkServer.active && !GameParam.instance._isInitialized)
		{
			loadingScreen.SetLoadingTitle("##loading_title_waitinghost");
			yield return new WaitForSeconds(1f);
		}
		loadingScreen.SetLoadingTitle("##loading");
		if (UNetManager.instance.LocalNetworkClient != null && Neo.IsMultiplayer)
		{
			ClientScene.Ready(UNetManager.instance.LocalNetworkClient.connection);
		}
		while (!UNetManager.instance.debugLoader && NetworkServer.active && !UNetManager.instance.UserList.TrueForAll((User user) => user.connectionToClient.isReady))
		{
			loadingScreen.SetLoadingTitle("##loading_title_waitingplayers");
			yield return new WaitForSeconds(1f);
		}
		yield return GameClock.WaitForEndOfFrame;
		while (User.LocalUser == null)
		{
			loadingScreen.SetLoadingTitle("##loading_title_waitinghost");
			yield return new WaitForSeconds(1f);
		}
		yield return GameClock.WaitForEndOfFrame;
		loadingScreen.SetLoadingTitle("##loading_title_step1");
		StartLua();
		if (NetworkServer.active)
		{
			SimpleHostScript.Run();
		}
		while (!NetworkServer.active && !GameParam.instance.HostLoadedAllFactions)
		{
			loadingScreen.SetLoadingTitle("##loading_title_waitinghost");
			yield return new WaitForSeconds(1f);
		}
		if (!NetworkServer.active)
		{
			SimpleHostScript.PopulateFactionsLocally();
		}
		while (NetworkServer.active && Faction.GetAllFactions().Count > 0 && !UNetManager.instance.UserList.TrueForAll((User user) => user.FactionReady))
		{
			loadingScreen.SetLoadingTitle("##loading_title_waitingplayers");
			yield return new WaitForSeconds(1f);
		}
		if (_mapToLoad.IsNewGame())
		{
			SimpleHostScript.AllFactionsFirstReady.Invoke();
		}
		GameSignals.Invoke(GameSignals.BeforeTerrainLoaded);
		yield return GameClock.WaitForEndOfFrame;
		yield return LoadingStepsCheckpoint(engineSteps);
		loadingScreen.SetLoadingTitle("##loading_title_step2");
		while (!NetworkServer.active && !VoxelNoiseConfigurator.IsInitialized())
		{
			yield return new WaitForSeconds(1f);
		}
		InitNoise();
		yield return LoadBasicTerrain();
		yield return LoadingStepsCheckpoint(terrainInitSteps);
		yield return InitTerrainGravity();
		yield return LoadingStepsCheckpoint(vegetationInitSteps);
		loadingScreen.SetLoadingTitle("##loading_title_step3");
		yield return LoadNature();
		GameSignals.Invoke(GameSignals.TerrainLoaded);
		User.LocalUser.CallCmdSetTerrainLoaded();
		yield return BuildTerrainMeshes();
		while (NetworkServer.active && !UNetManager.instance.UserList.TrueForAll((User user) => user.TerrainLoaded))
		{
			loadingScreen.SetLoadingTitle("##loading_title_waitingplayers");
			yield return new WaitForSeconds(1f);
		}
		loadingScreen.SetLoadingTitle("##loading_title_step4");
		if (NetworkServer.active)
		{
			yield return LoadGameObjects(originOffset);
		}
		GameSignals.Invoke(GameSignals.GameObjectsLoaded);
		while (!GameParam.instance.HostLoaded && !NetworkServer.active)
		{
			loadingScreen.SetLoadingTitle("##loading_title_waitinghost");
			yield return new WaitForSeconds(1f);
		}
		yield return LoadGridGuide();
		GameSignals.Invoke(GameSignals.LevelLoaded);
		if (_mapToLoad.IsNewGame())
		{
			GameSignals.Invoke(GameSignals.LevelFirstLoaded);
		}
		else
		{
			GameSignals.Invoke(GameSignals.LevelSaveLoaded);
		}
		yield return GameClock.WaitForEndOfFrame;
		yield return new WaitForFixedUpdate();
	}

	private void InitNoise()
	{
		if (NetworkServer.active)
		{
			VoxelNoiseConfigurator voxelNoiseConfigurator = Factory.Instantiate<VoxelNoiseConfigurator>(Brix.External.Factories.Noise.NOISE_CONFIG);
			Hashtable hashtable = GameParam.Map.Disk_ReadNoise();
			if (hashtable != null && hashtable.ContainsKey("__GLOBALSEED"))
			{
				VoxelNoiseConfigurator.Instance.NoiseConfigList = (List<VoxelNoiseConfigurator.NoiseConfig>)hashtable["__NOISETYPES"];
				VoxelNoiseConfigurator.SetGlobalSeed((int)(long)hashtable["__GLOBALSEED"]);
			}
			NetworkServer.Spawn(voxelNoiseConfigurator.gameObject, Brix.External.Factories.Noise.NOISE_CONFIG.AssetId);
		}
		ReadPresetDeepResourcesSettings();
	}

	private void ReadPresetDeepResourcesSettings()
	{
		GameParam.Preset.settings.TryGetValue("blueCrystalDeeplayer", out var value);
		GameParam.Preset.settings.TryGetValue("orangeCrystalDeeplayer", out var value2);
		GameParam.Preset.settings.TryGetValue("ironDeeplayer", out var value3);
		if (value != null)
		{
			VoxelNoiseConfigurator.GetNoiseConfigFromVoxelTerrainType(VoxelTerrainType.BlueCrystal).Enabled = (bool)value;
		}
		if (value2 != null)
		{
			VoxelNoiseConfigurator.GetNoiseConfigFromVoxelTerrainType(VoxelTerrainType.OrangeCrystal).Enabled = (bool)value2;
		}
		if (value3 != null)
		{
			VoxelNoiseConfigurator.GetNoiseConfigFromVoxelTerrainType(VoxelTerrainType.Iron).Enabled = (bool)value3;
		}
	}

	private IEnumerator AwakeStep()
	{
		yield return LoadingStepsCheckpoint(pathfindingSteps);
		loadingScreen.SetLoadingTitle("##loading_title_step5");
		yield return WaitForVoxelEngine();
		yield return InitPathfinding();
		GameSignals.Invoke(GameSignals.TerrainReady);
		yield return GameClock.WaitForEndOfFrame;
	}

	private IEnumerator StartStep()
	{
		GameSignals.Invoke(GameSignals.LoadUI);
		while (LuaMenuComponent.CoroutineCount != 0)
		{
			yield return new WaitForEndOfFrame();
		}
		if (!Neo.IsMultiplayer)
		{
			StatsTracker.StartRecording();
		}
		LuaLoader.Load("gameplayTest.lua");
		if (_mapToLoad.IsNewGame())
		{
			GameSignals.Invoke(GameSignals.LoadGamemode);
			GameSignals.Invoke(GameSignals.LevelFirstReady);
		}
		else
		{
			GameSignals.Invoke(GameSignals.LevelSaveReady);
		}
		GameSignals.Invoke(GameSignals.GamemodeLoaded);
		yield return GameClock.WaitForEndOfFrame;
		yield return LoadingStepsCheckpoint(0);
		if (NetworkServer.active)
		{
			GameParam.instance.NetworkHostLoaded = true;
		}
		if (!NetworkServer.active)
		{
			GameSignals.Invoke(GameSignals.LevelReady);
		}
		User.LocalUser.CallCmdSetLevelReady();
		while (!GameParam.instance.HostReady && !NetworkServer.active)
		{
			loadingScreen.SetLoadingTitle("##loading_title_waitinghost");
			yield return new WaitForSeconds(1f);
		}
		while (NetworkServer.active && Faction.GetAllFactions().Count > 0 && !UNetManager.instance.UserList.TrueForAll((User user) => user.LevelReady))
		{
			loadingScreen.SetLoadingTitle("##loading_title_waitingplayers");
			yield return new WaitForSeconds(1f);
		}
		if (NetworkServer.active)
		{
			GameSignals.Invoke(GameSignals.LevelReady);
			GameParam.instance.NetworkHostReady = true;
		}
		SoundEngine.GameIsReady = true;
		GameSignals.Invoke(GameSignals.SoundEngineReady);
	}

	private IEnumerator LoadBasicTerrain()
	{
		VoxelEngine.Load(_mapToLoad.path);
		yield return WaitForVoxelEngine();
	}

	private IEnumerator LoadGameObjects(XYZ offset)
	{
		if (NetworkServer.active)
		{
			yield return LoadPlacedBlockData();
			GameObjectSerializer gameManager = UnityEngine.Object.FindObjectOfType<GameObjectSerializer>();
			string jsonGOs = _mapToLoad.Disk_ReadGameObjects();
			if (jsonGOs == null)
			{
				yield break;
			}
			List<GameObject> objs = gameManager.LoadState("save", jsonGOs);
			if (objs == null)
			{
				yield break;
			}
			Vector3 translation = offset.ToVector3();
			foreach (GameObject item in objs)
			{
				if (item != null)
				{
					item.transform.position += translation;
					GameSignals.Invoke(GameSignals.GameObjectLoaded, item);
				}
			}
		}
		yield return GameClock.WaitForEndOfFrame;
	}

	private IEnumerator BuildTerrainMeshes()
	{
		VoxelEngine.StartRendering();
		yield return WaitForVoxelEngine();
	}

	private IEnumerator WaitForVoxelEngine()
	{
		int stepsLeft = engineSteps;
		while (VoxelEngine.Working)
		{
			yield return GameClock.WaitForEndOfFrame;
			if (stepsLeft > 0)
			{
				GameSignals.Invoke(GameSignals.LoadingProgress);
				stepsLeft--;
			}
		}
		if (stepsLeft > 0)
		{
			GameSignals.Invoke(GameSignals.LoadingProgressLeap, stepsLeft);
			stepsLeft = 0;
		}
		engineSteps = stepsLeft;
	}

	private void StartLua()
	{
		LuaLoader.LoadUserKeyBindingLuaFile();
		foreach (Asset_Mod item in AssetDispensary.mods.SendQuery())
		{
			Script modScript = null;
			item.Disk_LoadMod(out modScript);
			if (modScript != null)
			{
				GameSignals.Connect(UNetManager.LateDisconnected, delegate
				{
					Script.Kill(ref modScript);
				});
			}
		}
		if (_gamemodeToLoad == null)
		{
			return;
		}
		Script gamemodeScript = null;
		_gamemodeToLoad.Disk_LoadGamemode(out gamemodeScript, _presetToLoad);
		if (gamemodeScript != null)
		{
			GameSignals.Connect(UNetManager.LateDisconnected, delegate
			{
				Script.Kill(ref gamemodeScript);
			});
		}
	}

	private static IEnumerator InitTerrainGravity()
	{
		IEnumerable<XYZ> visibleVoxels = Voxel.Engine.GetNaturalBottomVoxelPositionsFromLayers();
		int steps = 0;
		foreach (XYZ p in visibleVoxels)
		{
			steps++;
			if (steps == 10000)
			{
				GameSignals.Invoke(GameSignals.LoadingProgress);
				yield return GameClock.WaitForEndOfFrame;
				steps = 0;
			}
			GeostaticityTracker.SetGeostatique(p);
		}
	}

	private static IEnumerator InitPathfinding()
	{
		TestPathfinding.Instance.autoInit = false;
		if (Architecte.isPathfindingEnabled)
		{
			yield return TestPathfinding.Instance.Read_Coroutine();
		}
		else
		{
			TestPathfinding.Instance.pathfinding.lodCount = 1;
		}
	}

	private IEnumerator LoadNature()
	{
		yield return LoadTrees();
		LoadPlants();
		LoadBoulders();
		LoadIrons();
		LoadPyrites();
	}

	private void LoadPlants()
	{
		BrixSingleton<PlantRegistry>.Instance.SerializedData = _mapToLoad.Disk_ReadPlants();
	}

	private void LoadBoulders()
	{
		BoulderRegistry.BuildBouldersFromSerialized(_mapToLoad.Disk_ReadBoulders());
	}

	private void LoadIrons()
	{
		IronRegistry.BuildMineralsFromSerialized(_mapToLoad.Disk_ReadIrons());
	}

	private void LoadPyrites()
	{
		PyriteRegistry.BuildMineralsFromSerialized(_mapToLoad.Disk_ReadPyrites());
	}

	private IEnumerator LoadGridGuide()
	{
		Picking.Instance.WorldEditorGrid.SetGuide(_mapToLoad.Disk_ReadWorldEditorGuide());
		yield return GameClock.WaitForEndOfFrame;
	}

	private IEnumerator LoadTrees()
	{
		Asset_Map.TreeInfo[] trees = _mapToLoad.Disk_ReadTrees();
		SortTreesByMegaVoxel(trees);
		int steps = 0;
		Asset_Map.TreeInfo[] array = trees;
		foreach (Asset_Map.TreeInfo treeInfo in array)
		{
			steps++;
			if (steps == 10000)
			{
				GameSignals.Invoke(GameSignals.LoadingProgress);
				yield return GameClock.WaitForEndOfFrame;
				steps = 0;
			}
			XYZ position = new XYZ(treeInfo.x, treeInfo.y, treeInfo.z);
			int tree = TreeRegistry.GetPseudoRandomArbreInfo(position.ToScalaire());
			tree = (tree & 0xF) | (Mathf.Clamp(treeInfo.s, 0, 15) << 4);
			TreeRegistry.Instance.Add(position, tree);
		}
	}

	private void SortTreesByMegaVoxel(Asset_Map.TreeInfo[] trees)
	{
		int[] keys = (from treeInfo in trees.Slinq()
			select Brix.Engine.Abstract.Voxel.GetGScalaire(treeInfo.x, treeInfo.y, treeInfo.z)).ToArray();
		Array.Sort(keys, trees);
	}

	private IEnumerator LoadPlacedBlockData()
	{
		yield return BrixSingleton<BlockEngine>.Instance.LoadBlockData(_mapToLoad.Disk_ReadPlacedBlocks);
	}
}
You are not using the latest version of the tool, please update.
Latest version is '11.1.0.9782' (yours is '9.1.0.7988')
