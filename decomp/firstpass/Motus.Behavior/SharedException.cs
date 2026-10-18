namespace Motus.Behavior;

public class SharedException<T> : FailException where T : FailException, new()
{
	private static T instance;

	public static T Get
	{
		get
		{
			if (instance == null)
			{
				instance = new T();
			}
			return instance;
		}
	}
}
