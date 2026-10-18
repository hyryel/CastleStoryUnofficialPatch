using Brix.Game.AI;

namespace Motus.Behavior;

public class SharedWorkException<T> : Exceptions.WorkFailedException where T : Exceptions.WorkFailedException, new()
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
