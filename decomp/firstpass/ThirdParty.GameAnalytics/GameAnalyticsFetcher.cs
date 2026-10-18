using System;
using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.AI;
using Brix.Game.Names;
using Brix.Utils.Signals;
using Smooth.Slinq;
using UnityEngine;

namespace ThirdParty.GameAnalytics;

public class GameAnalyticsFetcher
{
	private IGameAnalyticsStore store;

	private HashSet<Factory.AssetKey> characters;

	protected Signal Disconnected = new Signal();

	public GameAnalyticsFetcher(IGameAnalyticsStore store)
	{
		this.store = store;
		InitCharacterSet();
		ConnectToSignals();
	}

	private void InitCharacterSet()
	{
		characters = new HashSet<Factory.AssetKey>();
		characters.Add(Test.Bricktron);
		characters.Add(Test.Biftron);
	}

	public void ReconnectToSignals()
	{
		Disconnected.Invoke();
		ConnectToSignals();
	}

	private void ConnectToSignals()
	{
		GameSignals.Connect(GameSignals.LevelLoaded, OnLoadLevel).DisconnectWhen(Disconnected);
		Factory.GameObjectCreatedSignal.Connect(OnGameObjectCreated).DisconnectWhen(Disconnected);
		GameSignals.Connect(GameSignals.LeaveGame, OnLeaveGame).DisconnectWhen(Disconnected);
		GameSignals.Connect(GameSignals.ReLoadGameCommand, OnReLoadGameCommand).DisconnectWhen(Disconnected);
		GameSignals.Connect(GameSignals.BlockPlaced, OnBlockPlaced).DisconnectWhen(Disconnected);
		GameSignals.Connect(GameSignals.QuitThroughMenu, OnQuitThroughMenu).DisconnectWhen(Disconnected);
		GameSignals.Connect(GameSignals.LoadError, OnLoadError).DisconnectWhen(Disconnected);
		GameSignals.Connect(GameSignals.UnitDied, OnUnitDied).DisconnectWhen(Disconnected);
		GameSignals.Connect(GameSignals.PathfindingRequest, OnPathfindingRequest).DisconnectWhen(Disconnected);
		GameSignals.Connect(GameSignals.PathfindingSolvedRequest, OnPathfindingSolvedRequest).DisconnectWhen(Disconnected);
		GameSignals.Connect(GameSignals.PathfindingUnsolvedRequest, OnPathfindingUnsolvedRequest).DisconnectWhen(Disconnected);
		GameSignals.Connect(GameSignals.PathfindingDirectRequest, OnPathfindingDirectRequest).DisconnectWhen(Disconnected);
		GameSignals.Connect(GameSignals.PathfindingInterruptedRequest, OnPathfindingInterruptedRequest).DisconnectWhen(Disconnected);
		GameSignals.Connect(GameSignals.GamemodeLoaded, OnGameModeStarted).DisconnectWhen(Disconnected);
		NameDispenserGO.GetDispenserGO(NameDispenserGO.Dispensers.Bricktrons).GetSignal().Connect(OnNewBricktronName)
			.DisconnectWhen(Disconnected);
	}

	private void OnNewBricktronName(string name)
	{
		store.AddEvent(GameAnalyticEvent.Types.BricktronName, new GameAnalyticsContext(0f, name));
	}

	private void OnQuitThroughMenu()
	{
		store.AddEvent(GameAnalyticEvent.Types.SoftQuit);
	}

	private void OnUnitDied(GameObject victim, GameObject killer)
	{
		string text = ((!(victim != null) || !(victim.GetComponent<Occupation>() != null)) ? "Unknown" : Enum.GetName(typeof(Occupation.Type), victim.GetComponent<Occupation>().CurrentOccupation));
		string text2 = ((!(killer != null) || !(killer.GetComponent<Occupation>() != null)) ? "Unknown" : Enum.GetName(typeof(Occupation.Type), killer.GetComponent<Occupation>().CurrentOccupation));
		store.AddEvent(GameAnalyticEvent.Types.UnitDied, new GameAnalyticsContext(0f, text + ":" + text2));
	}

	private void OnPathfindingRequest()
	{
		store.AddEvent(GameAnalyticEvent.Types.PathfindingTotalCount);
	}

	private void OnPathfindingDirectRequest()
	{
		store.AddEvent(GameAnalyticEvent.Types.PathfindingTotalCount);
	}

	private void OnPathfindingSolvedRequest()
	{
		store.AddEvent(GameAnalyticEvent.Types.PathfindingTotalCount);
	}

	private void OnPathfindingUnsolvedRequest()
	{
		store.AddEvent(GameAnalyticEvent.Types.PathfindingTotalCount);
	}

	private void OnPathfindingInterruptedRequest()
	{
		store.AddEvent(GameAnalyticEvent.Types.PathfindingTotalCount);
	}

	private void OnRound()
	{
		store.AddEvent(GameAnalyticEvent.Types.Round);
	}

	private void OnBlockPlaced<T>(T data)
	{
		store.AddEvent(GameAnalyticEvent.Types.BlockPlaced);
	}

	private void OnLoadError()
	{
		store.AddEvent(GameAnalyticEvent.Types.LoadError);
	}

	private void OnLoadLevel()
	{
		if (store.GetEvents().Slinq().Last()
			.type != GameAnalyticEvent.Types.reLoadGameCommand)
		{
			store.AddEvent(GameAnalyticEvent.Types.startedPlaying);
		}
	}

	private void OnLeaveGame()
	{
		store.AddEvent(GameAnalyticEvent.Types.backToMenu);
	}

	private void OnReLoadGameCommand()
	{
		store.AddEvent(GameAnalyticEvent.Types.reLoadGameCommand);
	}

	private void OnGameModeStarted()
	{
		if (GameParam.Map.IsNewGame())
		{
			store.AddEvent(GameAnalyticEvent.Types.GamemodePresetNewGame, new GameAnalyticsContext(Convert.ToSingle(Neo.IsMultiplayer), GameParam.Gamemode.codename + ":" + GameParam.Preset.codename));
		}
		else
		{
			store.AddEvent(GameAnalyticEvent.Types.GamemodePresetLoadGame, new GameAnalyticsContext(Convert.ToSingle(Neo.IsMultiplayer), GameParam.Gamemode.codename + ":" + GameParam.Preset.codename));
		}
	}

	private void OnCharacterCreated(GameObject go)
	{
		store.AddEvent(GameAnalyticEvent.Types.BricktronSpawned);
	}

	private void OnGameObjectCreated(Factory.AssetKey assetKey, GameObject go)
	{
		if (characters.Contains(assetKey))
		{
			OnCharacterCreated(go);
		}
	}
}
