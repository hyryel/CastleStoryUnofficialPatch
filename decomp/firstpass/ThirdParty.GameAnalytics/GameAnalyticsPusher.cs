using System.Collections.Generic;
using System.IO;

namespace ThirdParty.GameAnalytics;

public class GameAnalyticsPusher
{
	public List<string> APICallsMade;

	public List<string> PushToServer(GameAnalyticsStore store)
	{
		APICallsMade = new List<string>();
		foreach (GameAnalyticEvent @event in store.GetEvents())
		{
			PushEvent(@event);
		}
		return APICallsMade;
	}

	private void APICall(string symbol)
	{
		GA.API.Design.NewEvent(symbol);
		APICallsMade.Add(symbol);
		File.AppendAllText("Info/test_result.txt", symbol + "\n");
	}

	private void APICall(string symbol, float value)
	{
		GA.API.Design.NewEvent(symbol, value);
		APICallsMade.Add(symbol + "<value>" + value);
		File.AppendAllText("Info/test_result.txt", symbol + "<value>" + value + "\n");
	}

	private void PushEvent(GameAnalyticEvent GAevent)
	{
		if (GAevent.type == GameAnalyticEvent.Types.GamemodePresetNewGame)
		{
			APICall("Saurolytics:GamemodePresetNewGame:" + GAevent.context.name);
		}
		if (GAevent.type == GameAnalyticEvent.Types.GamemodePresetLoadGame)
		{
			APICall("Saurolytics:GamemodePresetLoadGame:" + GAevent.context.name);
		}
		if (GAevent.type == GameAnalyticEvent.Types.UnitDied)
		{
			APICall("Saurolytics:UnitDied:" + GAevent.context.name);
		}
		if (GAevent.type == GameAnalyticEvent.Types.QuitUnexpectedly)
		{
			APICall("Saurolytics:QuitUnexpectedly");
		}
		if (GAevent.type == GameAnalyticEvent.Types.LoadError)
		{
			APICall("Saurolytics:LoadError");
		}
		if (GAevent.type == GameAnalyticEvent.Types.SoftQuit)
		{
			APICall("Saurolytics:SoftQuit");
		}
		if (GAevent.type == GameAnalyticEvent.Types.HardQuit)
		{
			APICall("Saurolytics:HardQuit");
		}
		if (GAevent.type == GameAnalyticEvent.Types.Round)
		{
			APICall("Saurolytics:RoundPlayed", (int)GAevent.context.value);
		}
		if (GAevent.type == GameAnalyticEvent.Types.BlockPlaced)
		{
			APICall("Saurolytics:BlockPlaced", (int)GAevent.context.value);
		}
		if (GAevent.type == GameAnalyticEvent.Types.BricktronSpawned)
		{
			APICall("Saurolytics:BricktronSpawned", (int)GAevent.context.value);
		}
		if (GAevent.type == GameAnalyticEvent.Types.BriktronDied)
		{
			APICall("Saurolytics:BriktronDied", (int)GAevent.context.value);
		}
		if (GAevent.type == GameAnalyticEvent.Types.PathfindingTotalCount)
		{
			APICall("Saurolytics:Pathfinding:Total", (int)GAevent.context.value);
		}
		if (GAevent.type == GameAnalyticEvent.Types.PathfindingDirectCount)
		{
			APICall("Saurolytics:Pathfinding:Direct", (int)GAevent.context.value);
		}
		if (GAevent.type == GameAnalyticEvent.Types.PathfindingSolvedCount)
		{
			APICall("Saurolytics:Pathfinding:Solved", (int)GAevent.context.value);
		}
		if (GAevent.type == GameAnalyticEvent.Types.PathfindingUnsolvedCount)
		{
			APICall("Saurolytics:Pathfinding:Unsolved", (int)GAevent.context.value);
		}
		if (GAevent.type == GameAnalyticEvent.Types.PathfindingInterruptedCount)
		{
			APICall("Saurolytics:Pathfinding:Interrupted", (int)GAevent.context.value);
		}
		if (GAevent.type == GameAnalyticEvent.Types.startedPlaying)
		{
			APICall("Saurolytics:StartedPlaying");
		}
		if (GAevent.type == GameAnalyticEvent.Types.UniqueCountEver)
		{
			APICall("Saurolytics:UniqueCountEver");
		}
		if (GAevent.type == GameAnalyticEvent.Types.UniqueCountThisMonth)
		{
			APICall("Saurolytics:UniqueCountThisMonth");
		}
		if (GAevent.type == GameAnalyticEvent.Types.UniqueCountToday)
		{
			APICall("Saurolytics:UniqueCountToday");
		}
		if (GAevent.type == GameAnalyticEvent.Types.UniqueCountPerHour)
		{
			APICall("Saurolytics:UniqueCountPerHour:", GAevent.context.value);
		}
		if (GAevent.type == GameAnalyticEvent.Types.HourSinceLastPlay)
		{
			APICall("Saurolytics:HoursSinceLastPlay", GAevent.context.value);
		}
		if (GAevent.type == GameAnalyticEvent.Types.ClientSessionTime)
		{
			APICall("Saurolytics:ClientSessionTime", GAevent.context.value);
		}
		if (GAevent.type == GameAnalyticEvent.Types.PlaySessionTime)
		{
			APICall("Saurolytics:PlaySessionTime", GAevent.context.value);
		}
		if (GAevent.type == GameAnalyticEvent.Types.PlaySegmentTime)
		{
			APICall("Saurolytics:PlaySegmentTime", GAevent.context.value);
		}
		if (GAevent.type == GameAnalyticEvent.Types.TimeSpentInMenu)
		{
			APICall("Saurolytics:TimeSpentInMenu", GAevent.context.value);
		}
		if (GAevent.type == GameAnalyticEvent.Types.BricktronName)
		{
			APICall("Saurolytics:BricktronName:" + GAevent.context.name);
		}
		if (GAevent.type == GameAnalyticEvent.Types.gameModeStarted)
		{
			APICall("Saurolytics:GameModeLoaded:" + GAevent.context.name);
		}
	}
}
