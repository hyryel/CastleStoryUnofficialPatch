public class DefaultDigGoalResourceSetting : IDigGoalResourceSetting
{
	private static DefaultDigGoalResourceSetting _instance;

	public static IDigGoalResourceSetting Get
	{
		get
		{
			if (_instance == null)
			{
				_instance = new DefaultDigGoalResourceSetting();
			}
			return _instance;
		}
	}

	public GroundResource TypesToHarvest => GroundResource.All;
}
