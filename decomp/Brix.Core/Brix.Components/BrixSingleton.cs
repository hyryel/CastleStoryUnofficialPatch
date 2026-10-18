using Brix.Utils.Logging;

namespace Brix.Components;

public class BrixSingleton<T> : BrixComponent where T : BrixSingleton<T>
{
	private static BrixLogger logger = new BrixLogger();

	protected static T _instance;

	public static T Instance => _instance;

	protected override void Awake()
	{
		base.Awake();
		if (_instance != null)
		{
			logger.ErrorFormat("Singleton creation failed for type '{0}' on GameObject '{1}'. An instance already exist on GameObject '{2}'.", GetType().Name, base.name, _instance.name);
		}
		else
		{
			_instance = (T)this;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_instance != this)
		{
			logger.ErrorFormat("Singleton deletion failed for type '{0}' on GameObject '{1}'. Active instance is on GameObject '{2}'.", GetType().Name, base.name, _instance.name);
		}
		else
		{
			_instance = null;
		}
	}
}
