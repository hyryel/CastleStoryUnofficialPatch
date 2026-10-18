using System;

namespace ThirdParty.GameAnalytics;

public class GameAnalyticEvent
{
	public enum Types
	{
		dummyEvent,
		nullEvent,
		launchedClient,
		startedPlaying,
		reLoadGameCommand,
		backToMenu,
		gameModeStarted,
		GamemodePresetLoadGame,
		GamemodePresetNewGame,
		BricktronName,
		BlockPlaced,
		CorruptronDied,
		BricktronSpawned,
		Round,
		BriktronDied,
		PathfindingDirectCount,
		PathfindingSolvedCount,
		PathfindingUnsolvedCount,
		PathfindingInterruptedCount,
		PathfindingTotalCount,
		UnitDied,
		UniqueCountEver,
		UniqueCountThisMonth,
		UniqueCountToday,
		UniqueCountPerHour,
		HourSinceLastPlay,
		ClientSessionTime,
		PlaySessionTime,
		PlaySegmentTime,
		TimeSpentInMenu,
		SoftQuit,
		HardQuit,
		LoadError,
		QuitUnexpectedly,
		SaveUpgrade
	}

	public readonly DateTime date;

	public readonly Types type;

	public GameAnalyticsContext context;

	public GameAnalyticEvent(DateTime date, Types type, GameAnalyticsContext context = null)
	{
		this.date = date;
		this.type = type;
		this.context = context;
	}

	public override string ToString()
	{
		if (context == null)
		{
			return date.ToString() + " : " + type.ToString() + " : null";
		}
		return date.ToString() + " : " + type.ToString() + " : " + context.ToString();
	}
}
