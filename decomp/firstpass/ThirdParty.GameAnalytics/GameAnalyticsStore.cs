using System;
using System.Collections.Generic;
using System.Linq;
using Brix.External.Factories;
using Newtonsoft.Json;
using Smooth.Slinq;

namespace ThirdParty.GameAnalytics;

[JsonObject(MemberSerialization.OptIn)]
public class GameAnalyticsStore : IGameAnalyticsStore
{
	private HashSet<Factory.AssetKey> characters;

	private List<GameAnalyticEvent> _events;

	[JsonProperty]
	private List<GameAnalyticEvent> events
	{
		get
		{
			return _events.ToList();
		}
		set
		{
			_events = value;
		}
	}

	public GameAnalyticsStore()
	{
		_events = new List<GameAnalyticEvent>();
	}

	public GameAnalyticsStore(List<GameAnalyticEvent> events)
	{
		_events = events;
	}

	public IEnumerable<GameAnalyticEvent> GetEvents()
	{
		return _events;
	}

	public int Count()
	{
		return _events.Count;
	}

	public static GameAnalyticsStore operator +(GameAnalyticsStore a, GameAnalyticsStore b)
	{
		return new GameAnalyticsStore(a._events.Slinq().Concat(b._events.Slinq()).ToList());
	}

	public void AddEvent(GameAnalyticEvent.Types type, GameAnalyticsContext context = null)
	{
		AddEvent(DateTime.Now, type, context);
	}

	public void AddEvent(DateTime time, GameAnalyticEvent.Types type, GameAnalyticsContext context = null)
	{
		switch (type)
		{
		case GameAnalyticEvent.Types.BlockPlaced:
		case GameAnalyticEvent.Types.BricktronSpawned:
		case GameAnalyticEvent.Types.Round:
		case GameAnalyticEvent.Types.BriktronDied:
		case GameAnalyticEvent.Types.PathfindingDirectCount:
		case GameAnalyticEvent.Types.PathfindingSolvedCount:
		case GameAnalyticEvent.Types.PathfindingUnsolvedCount:
		case GameAnalyticEvent.Types.PathfindingInterruptedCount:
		case GameAnalyticEvent.Types.PathfindingTotalCount:
			AddOrIncrease(time, type);
			break;
		default:
			Add(time, type, context);
			break;
		}
	}

	private bool IncreaseContextValue(GameAnalyticEvent.Types type)
	{
		for (int i = 0; i < _events.Count; i++)
		{
			if (_events[i].type == type)
			{
				_events[i].context.value += 1f;
				return true;
			}
		}
		return false;
	}

	private void AddOrIncrease(DateTime time, GameAnalyticEvent.Types type)
	{
		if (!IncreaseContextValue(type))
		{
			_events.Add(new GameAnalyticEvent(time, type, new GameAnalyticsContext(1f, string.Empty)));
		}
	}

	private void Add(DateTime time, GameAnalyticEvent.Types type, GameAnalyticsContext context = null)
	{
		_events.Add(new GameAnalyticEvent(time, type, context));
	}
}
