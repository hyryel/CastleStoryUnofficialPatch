using System;
using System.Collections.Generic;
using System.Linq;
using Brix.Assets;
using Smooth.Slinq;

namespace ThirdParty.GameAnalytics;

public class GameAnalyticsProcesser
{
	private TimeFunction LastTimePlayed;

	private List<GameAnalyticsException> processingExceptions;

	public List<GameAnalyticsException> ProcessingExceptions
	{
		get
		{
			return processingExceptions;
		}
		set
		{
			processingExceptions = value;
		}
	}

	public GameAnalyticsProcesser(TimeFunction LastTimePlayed)
	{
		this.LastTimePlayed = LastTimePlayed;
	}

	public List<KeyValuePair<StoreProcessing, string>> GetProcessingSteps(GameAnalyticsStore store)
	{
		List<KeyValuePair<StoreProcessing, string>> list = new List<KeyValuePair<StoreProcessing, string>>();
		list.Add(new KeyValuePair<StoreProcessing, string>(() => UniqueCountEver(store), "UniqueCountEver"));
		list.Add(new KeyValuePair<StoreProcessing, string>(() => UniqueCountThisMonth(store), "UniqueCountThisMonth"));
		list.Add(new KeyValuePair<StoreProcessing, string>(() => UniqueCountToday(store), "UniqueCountToday"));
		list.Add(new KeyValuePair<StoreProcessing, string>(() => UniqueCountPerHour(store), "UniqueCountPerHour"));
		list.Add(new KeyValuePair<StoreProcessing, string>(() => HoursSinceLastPlay(store), "HoursSinceLastPlay"));
		list.Add(new KeyValuePair<StoreProcessing, string>(() => ClientSessionTime(store), "ClientSessionTime"));
		list.Add(new KeyValuePair<StoreProcessing, string>(() => PlaySessionTime(store), "PlaySessionTime"));
		list.Add(new KeyValuePair<StoreProcessing, string>(() => PlaySegmentTime(store), "PlaySegmentTime"));
		list.Add(new KeyValuePair<StoreProcessing, string>(() => TimeSpentInMenu(store), "TimeSpentInMenu"));
		list.Add(new KeyValuePair<StoreProcessing, string>(() => KeepEvents(store), "KeepEvents"));
		return list;
	}

	public GameAnalyticsStore ProcessStore(GameAnalyticsStore store)
	{
		GameAnalyticsStore result = new GameAnalyticsStore();
		processingExceptions = new List<GameAnalyticsException>();
		foreach (KeyValuePair<StoreProcessing, string> processingStep in GetProcessingSteps(store))
		{
			try
			{
				result += processingStep.Key();
			}
			catch (GameAnalyticsException ex)
			{
				ex.Data.Add("ProcessingStep", processingStep.Value);
				processingExceptions.Add(ex);
			}
		}
		return result;
	}

	public HashSet<GameAnalyticEvent.Types> GetEventToKeep()
	{
		HashSet<GameAnalyticEvent.Types> hashSet = new HashSet<GameAnalyticEvent.Types>();
		hashSet.Add(GameAnalyticEvent.Types.BricktronName);
		hashSet.Add(GameAnalyticEvent.Types.BlockPlaced);
		hashSet.Add(GameAnalyticEvent.Types.BriktronDied);
		hashSet.Add(GameAnalyticEvent.Types.PathfindingTotalCount);
		hashSet.Add(GameAnalyticEvent.Types.PathfindingSolvedCount);
		hashSet.Add(GameAnalyticEvent.Types.PathfindingUnsolvedCount);
		hashSet.Add(GameAnalyticEvent.Types.PathfindingInterruptedCount);
		hashSet.Add(GameAnalyticEvent.Types.PathfindingDirectCount);
		hashSet.Add(GameAnalyticEvent.Types.HardQuit);
		hashSet.Add(GameAnalyticEvent.Types.Round);
		hashSet.Add(GameAnalyticEvent.Types.SoftQuit);
		hashSet.Add(GameAnalyticEvent.Types.UnitDied);
		return hashSet;
	}

	public GameAnalyticsStore KeepEvents(GameAnalyticsStore store)
	{
		return store;
	}

	public GameAnalyticsStore GameModeLoaded(GameAnalyticsStore store)
	{
		GameAnalyticsStore result = new GameAnalyticsStore();
		GameAnalyticEvent gameAnalyticEvent = store.GetEvents().Slinq().FirstOrDefault((GameAnalyticEvent x) => x.type == GameAnalyticEvent.Types.gameModeStarted);
		if (gameAnalyticEvent != null)
		{
			string text = gameAnalyticEvent.context.name.Split(':').Slinq().First();
			string text2 = gameAnalyticEvent.context.name.Split(':').Slinq().Last();
			string text3 = "SaveFile";
			Asset_Gamemode asset_Gamemode = AssetDispensary.gamemodes.SendCodenameQuery(text);
			Asset_GamemodePreset asset_GamemodePreset = AssetDispensary.presets.SendCodenameQuery(text2);
			string text4 = (Legitimizer.GameModeIsLegit(text) ? asset_Gamemode.displayName : ("Custom" + asset_Gamemode.displayName));
			if (gameAnalyticEvent.context.value == 1f)
			{
				text4 += " Multiplayer";
			}
			if (text2 != string.Empty)
			{
				text3 = (Legitimizer.PresetIsLegit(text2) ? asset_GamemodePreset.displayName : ("Custom" + asset_GamemodePreset.displayName));
			}
			gameAnalyticEvent.context.name = text4 + ":" + text3;
		}
		return result;
	}

	public GameAnalyticsStore UniqueCountEver(GameAnalyticsStore store)
	{
		GameAnalyticsStore gameAnalyticsStore = new GameAnalyticsStore();
		GameAnalyticEvent gameAnalyticEvent = store.GetEvents().Slinq().FirstOrDefault((GameAnalyticEvent x) => x.type == GameAnalyticEvent.Types.startedPlaying);
		if (gameAnalyticEvent != null && LastTimePlayed() == DateTime.MaxValue)
		{
			gameAnalyticsStore.AddEvent(GameAnalyticEvent.Types.UniqueCountEver);
		}
		return gameAnalyticsStore;
	}

	public GameAnalyticsStore UniqueCountThisMonth(GameAnalyticsStore store)
	{
		GameAnalyticsStore gameAnalyticsStore = new GameAnalyticsStore();
		GameAnalyticEvent gameAnalyticEvent = store.GetEvents().Slinq().FirstOrDefault((GameAnalyticEvent x) => x.type == GameAnalyticEvent.Types.startedPlaying);
		if (gameAnalyticEvent != null)
		{
			if (LastTimePlayed().Month > gameAnalyticEvent.date.Month)
			{
				gameAnalyticsStore.AddEvent(GameAnalyticEvent.Types.UniqueCountThisMonth);
			}
			if (LastTimePlayed().Month == gameAnalyticEvent.date.Month)
			{
			}
			if (LastTimePlayed().Month < gameAnalyticEvent.date.Month)
			{
				gameAnalyticsStore.AddEvent(GameAnalyticEvent.Types.UniqueCountThisMonth);
			}
		}
		return gameAnalyticsStore;
	}

	public GameAnalyticsStore UniqueCountToday(GameAnalyticsStore store)
	{
		GameAnalyticsStore gameAnalyticsStore = new GameAnalyticsStore();
		GameAnalyticEvent gameAnalyticEvent = store.GetEvents().Slinq().FirstOrDefault((GameAnalyticEvent x) => x.type == GameAnalyticEvent.Types.startedPlaying);
		if (gameAnalyticEvent != null)
		{
			if (LastTimePlayed().Day > gameAnalyticEvent.date.Day)
			{
				gameAnalyticsStore.AddEvent(GameAnalyticEvent.Types.UniqueCountToday);
			}
			if (LastTimePlayed().Day == gameAnalyticEvent.date.Day)
			{
			}
			if (LastTimePlayed().Day < gameAnalyticEvent.date.Day)
			{
				gameAnalyticsStore.AddEvent(GameAnalyticEvent.Types.UniqueCountToday);
			}
		}
		return gameAnalyticsStore;
	}

	public GameAnalyticsStore UniqueCountPerHour(GameAnalyticsStore store)
	{
		GameAnalyticsStore gameAnalyticsStore = new GameAnalyticsStore();
		GameAnalyticsStore gameAnalyticsStore2 = PlaySessionTime(store);
		foreach (GameAnalyticEvent @event in gameAnalyticsStore2.GetEvents())
		{
			for (int i = @event.date.AddMinutes(-((int)@event.context.value * 60)).Hour; i <= @event.date.Hour; i++)
			{
				gameAnalyticsStore.AddEvent(GameAnalyticEvent.Types.UniqueCountPerHour, new GameAnalyticsContext(i, string.Empty));
			}
		}
		return gameAnalyticsStore;
	}

	public GameAnalyticsStore HoursSinceLastPlay(GameAnalyticsStore store)
	{
		GameAnalyticsStore gameAnalyticsStore = new GameAnalyticsStore();
		GameAnalyticEvent gameAnalyticEvent = store.GetEvents().Slinq().FirstOrDefault((GameAnalyticEvent x) => x.type == GameAnalyticEvent.Types.startedPlaying);
		DateTime dateTime = LastTimePlayed();
		if (gameAnalyticEvent != null && dateTime != DateTime.MaxValue)
		{
			float hours = HoursDiff(dateTime, gameAnalyticEvent.date);
			gameAnalyticsStore.AddEvent(GameAnalyticEvent.Types.HourSinceLastPlay, new GameAnalyticsContext(hours, string.Empty));
		}
		return gameAnalyticsStore;
	}

	private float HoursDiff(DateTime start, DateTime end)
	{
		TimeSpan timeSpan = end - start;
		return (float)timeSpan.Hours + (float)timeSpan.Minutes / 60f + (float)timeSpan.Seconds / 3600f;
	}

	private List<GameAnalyticEvent.Types> GetDelimiters(GameAnalyticsStore store, List<GameAnalyticEvent.Types> startDelimiters, List<GameAnalyticEvent.Types> midDelimiters, List<GameAnalyticEvent.Types> endDelimiters)
	{
		GameAnalyticEvent.Types item = startDelimiters.Find((GameAnalyticEvent.Types t) => (from e in store.GetEvents().Slinq()
			select e.type).Contains(t));
		GameAnalyticEvent.Types item2 = midDelimiters.Find((GameAnalyticEvent.Types t) => (from e in store.GetEvents().Slinq()
			select e.type).Contains(t));
		GameAnalyticEvent.Types item3 = endDelimiters.Find((GameAnalyticEvent.Types t) => (from e in store.GetEvents().Slinq()
			select e.type).Contains(t));
		List<GameAnalyticEvent.Types> list = new List<GameAnalyticEvent.Types>();
		list.Add(item);
		list.Add(item2);
		list.Add(item3);
		return list;
	}

	private GameAnalyticsStore SessionTime_Rework(GameAnalyticsStore store, GameAnalyticEvent.Types delimiterEventType, GameAnalyticEvent.Types midEventType, List<GameAnalyticEvent.Types> startDelimiters, List<GameAnalyticEvent.Types> midDelimiters, List<GameAnalyticEvent.Types> endDelimiters, int maxCount = -1)
	{
		GameAnalyticsStore gameAnalyticsStore = new GameAnalyticsStore();
		List<GameAnalyticEvent.Types> delimiters = GetDelimiters(store, startDelimiters, midDelimiters, endDelimiters);
		List<GameAnalyticEvent> list = store.GetEvents().ToList().FindAll((GameAnalyticEvent x) => delimiters.Contains(x.type));
		int num = 0;
		DateTime start = DateTime.MinValue;
		foreach (GameAnalyticEvent item in list)
		{
			if (startDelimiters.Contains(item.type))
			{
				if (num == 1)
				{
					throw new GameAnalyticsNonMatchingDelimitersException();
				}
				num++;
			}
			if (midDelimiters.Contains(item.type))
			{
				if (num != 1)
				{
					throw new GameAnalyticsNonMatchingDelimitersException();
				}
				if (midEventType != GameAnalyticEvent.Types.nullEvent)
				{
					gameAnalyticsStore.AddEvent(item.date, midEventType, new GameAnalyticsContext(HoursDiff(start, item.date), string.Empty));
				}
			}
			if (endDelimiters.Contains(item.type))
			{
				if (num != 1)
				{
					throw new GameAnalyticsNonMatchingDelimitersException();
				}
				num--;
				if (delimiterEventType != GameAnalyticEvent.Types.nullEvent)
				{
					gameAnalyticsStore.AddEvent(item.date, delimiterEventType, new GameAnalyticsContext(HoursDiff(start, item.date), string.Empty));
				}
				if (midEventType != GameAnalyticEvent.Types.nullEvent)
				{
					gameAnalyticsStore.AddEvent(item.date, midEventType, new GameAnalyticsContext(HoursDiff(start, item.date), string.Empty));
				}
			}
			start = item.date;
		}
		if (num == 1)
		{
			throw new GameAnalyticsNonMatchingDelimitersException();
		}
		return gameAnalyticsStore;
	}

	public GameAnalyticsStore ClientSessionTime(GameAnalyticsStore store)
	{
		List<GameAnalyticEvent.Types> list = new List<GameAnalyticEvent.Types>();
		list.Add(GameAnalyticEvent.Types.launchedClient);
		List<GameAnalyticEvent.Types> midDelimiters = new List<GameAnalyticEvent.Types>();
		List<GameAnalyticEvent.Types> list2 = new List<GameAnalyticEvent.Types>();
		list2.Add(GameAnalyticEvent.Types.SoftQuit);
		list2.Add(GameAnalyticEvent.Types.HardQuit);
		return SessionTime_Rework(store, GameAnalyticEvent.Types.ClientSessionTime, GameAnalyticEvent.Types.nullEvent, list, midDelimiters, list2);
	}

	public GameAnalyticsStore PlaySessionTime(GameAnalyticsStore store)
	{
		List<GameAnalyticEvent.Types> list = new List<GameAnalyticEvent.Types>();
		list.Add(GameAnalyticEvent.Types.startedPlaying);
		List<GameAnalyticEvent.Types> midDelimiters = new List<GameAnalyticEvent.Types>();
		List<GameAnalyticEvent.Types> list2 = new List<GameAnalyticEvent.Types>();
		list2.Add(GameAnalyticEvent.Types.backToMenu);
		list2.Add(GameAnalyticEvent.Types.SoftQuit);
		list2.Add(GameAnalyticEvent.Types.HardQuit);
		return SessionTime_Rework(store, GameAnalyticEvent.Types.PlaySessionTime, GameAnalyticEvent.Types.nullEvent, list, midDelimiters, list2);
	}

	public GameAnalyticsStore PlaySegmentTime(GameAnalyticsStore store)
	{
		List<GameAnalyticEvent.Types> list = new List<GameAnalyticEvent.Types>();
		list.Add(GameAnalyticEvent.Types.startedPlaying);
		List<GameAnalyticEvent.Types> list2 = new List<GameAnalyticEvent.Types>();
		list2.Add(GameAnalyticEvent.Types.reLoadGameCommand);
		List<GameAnalyticEvent.Types> list3 = new List<GameAnalyticEvent.Types>();
		list3.Add(GameAnalyticEvent.Types.backToMenu);
		list3.Add(GameAnalyticEvent.Types.SoftQuit);
		list3.Add(GameAnalyticEvent.Types.HardQuit);
		return SessionTime_Rework(store, GameAnalyticEvent.Types.nullEvent, GameAnalyticEvent.Types.PlaySegmentTime, list, list2, list3);
	}

	public GameAnalyticsStore SurvivalRoundsSurvived(GameAnalyticsStore store)
	{
		return store;
	}

	public GameAnalyticsStore TimeSpentInMenu(GameAnalyticsStore store)
	{
		GameAnalyticsStore gameAnalyticsStore = new GameAnalyticsStore();
		GameAnalyticsStore gameAnalyticsStore2 = ClientSessionTime(store);
		float num;
		if (store.GetEvents().Slinq().Any((GameAnalyticEvent e) => e.type == GameAnalyticEvent.Types.startedPlaying))
		{
			GameAnalyticsStore gameAnalyticsStore3 = PlaySessionTime(store);
			num = (from e in gameAnalyticsStore3.GetEvents().Slinq()
				where e.type == GameAnalyticEvent.Types.PlaySessionTime
				select e.context.value).Aggregate(0f, (float a, float b) => a + b);
		}
		else
		{
			num = 0f;
		}
		float num2 = (from e in gameAnalyticsStore2.GetEvents().Slinq()
			where e.type == GameAnalyticEvent.Types.ClientSessionTime
			select e.context.value).First();
		gameAnalyticsStore.AddEvent(GameAnalyticEvent.Types.TimeSpentInMenu, new GameAnalyticsContext(num2 - num, string.Empty));
		return gameAnalyticsStore;
	}
}
