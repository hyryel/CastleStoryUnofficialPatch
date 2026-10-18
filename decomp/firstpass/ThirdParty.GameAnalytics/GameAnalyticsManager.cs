using System;
using System.Collections.Generic;
using Smooth.Slinq;
using UnityEngine;

namespace ThirdParty.GameAnalytics;

public class GameAnalyticsManager
{
	private GameAnalyticsStore store;

	private GameAnalyticsSaver saver;

	private GameAnalyticsProcesser processer;

	private GameAnalyticsPusher pusher;

	private GameAnalyticsFetcher fetcher;

	public List<string> previousUnpushedStoreAPICalls;

	public GameAnalyticsManager(string dataPath, string timeStampPath)
	{
		store = new GameAnalyticsStore();
		saver = new GameAnalyticsSaver(dataPath, timeStampPath);
		fetcher = new GameAnalyticsFetcher(store);
		processer = new GameAnalyticsProcesser(saver.LastTimePlayed);
		pusher = new GameAnalyticsPusher();
		store.AddEvent(GameAnalyticEvent.Types.launchedClient);
		previousUnpushedStoreAPICalls = PushPreviousUnpushedStore();
		Debug.Log("GameAnalytics booted");
	}

	public List<GameAnalyticsException> GetProcessiongExceptions()
	{
		return processer.ProcessingExceptions;
	}

	public List<string> GetProcessiongExceptionsStrings()
	{
		return (from x in processer.ProcessingExceptions.Slinq()
			select x.Data["ProcessingStep"] as string).ToList();
	}

	public List<string> PushPreviousUnpushedStore()
	{
		GameAnalyticsStore gameAnalyticsStore = saver.Load();
		if (gameAnalyticsStore != null && gameAnalyticsStore.Count() != 0)
		{
			gameAnalyticsStore = CloseUnprocessedStoreWithHardQuit(gameAnalyticsStore);
			saver.DeleteStoreData();
			return pusher.PushToServer(ProcessStore(gameAnalyticsStore));
		}
		return new List<string>();
	}

	private GameAnalyticsStore CloseUnprocessedStoreWithHardQuit(GameAnalyticsStore store)
	{
		store.AddEvent(store.GetEvents().Slinq().Last()
			.date, GameAnalyticEvent.Types.HardQuit);
			return store;
		}

		public void ReconnectToSignals()
		{
			fetcher.ReconnectToSignals();
		}

		public List<string> GetEventsStrings()
		{
			return (from e in store.GetEvents().Slinq()
				select e.ToString()).ToList();
		}

		public List<string> PushToServer()
		{
			List<string> result = pusher.PushToServer(ProcessStore(store));
			saver.SaveTimeStamp(DateTime.Now);
			saver.DeleteStoreData();
			store = new GameAnalyticsStore();
			return result;
		}

		public void SetTimeStamp(DateTime moment)
		{
			saver.SaveTimeStamp(moment);
		}

		public void SaveStore()
		{
			saver.Save(store);
		}

		private GameAnalyticsStore ProcessStore(GameAnalyticsStore _store)
		{
			return processer.ProcessStore(_store);
		}
	}
