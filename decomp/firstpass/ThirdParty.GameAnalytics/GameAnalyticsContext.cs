namespace ThirdParty.GameAnalytics;

public class GameAnalyticsContext
{
	public float value;

	public string name;

	public GameAnalyticsContext(float hours = 0f, string name = "")
	{
		value = hours;
		this.name = name;
	}
}
