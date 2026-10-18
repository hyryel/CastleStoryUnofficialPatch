namespace Brix.ThirdParty.GA;

public static class GameAnalytics
{
	public static void NewEvent(string eventName)
	{
		global::GA.API.Design.NewEvent(eventName);
	}

	public static void NewEvent(string eventName, float eventValue)
	{
		global::GA.API.Design.NewEvent(eventName, eventValue);
	}
}
