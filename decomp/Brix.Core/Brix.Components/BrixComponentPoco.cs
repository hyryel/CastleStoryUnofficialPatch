using System;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Lifecycle.Pooling;
using Brix.Utils.Logging;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Components;

public class BrixComponentPoco : IBrixComponent, IPoolableComponent, IInitializable, IRequestable, IResetable
{
	private static BrixLogger logger;

	private static Action<NetworkIdentity> ResetNetworkIdentity;

	private GameObject _gameObject;

	private NetworkIdentity _networkIdentity;

	private string _name;

	private Signal<GameObject> _onReleaseOrDestroySignal = new Signal<GameObject>();

	private Signal _onReleaseSignal = new Signal();

	private Signal _onDestroySignal = new Signal();

	private bool _destroyed;

	private bool _released;

	private bool _requested;

	public bool Requested => _requested;

	public bool Released => _released;

	public bool Destroyed => _destroyed;

	BrixComponentPoco IBrixComponent.BrixComponentPoco => this;

	public bool Initialized => true;

	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			_gameObject.name = (_name = value);
		}
	}

	public IReadOnlySignal<GameObject> OnReleaseOrDestroySignal => _onReleaseOrDestroySignal;

	public IReadOnlySignal OnReleaseSignal => _onReleaseSignal;

	public IReadOnlySignal OnDestroySignal => _onDestroySignal;

	static BrixComponentPoco()
	{
		logger = new BrixLogger();
		MethodInfo method = typeof(NetworkIdentity).GetMethod("Reset", BindingFlags.Instance | BindingFlags.NonPublic);
		ParameterExpression parameterExpression = Expression.Parameter(typeof(NetworkIdentity), "IdentityToReset");
		MethodCallExpression body = Expression.Call(parameterExpression, method);
		ResetNetworkIdentity = Expression.Lambda<Action<NetworkIdentity>>(body, new ParameterExpression[1] { parameterExpression }).Compile();
	}

	public BrixComponentPoco(GameObject gameObject)
	{
		_gameObject = gameObject;
		_requested = false;
		_networkIdentity = gameObject.GetComponent<NetworkIdentity>();
		_onReleaseOrDestroySignal.MaximumConnections = 1000;
		_onReleaseSignal.MaximumConnections = 1000;
		_onDestroySignal.MaximumConnections = 1000;
	}

	public void OnDestroy()
	{
		if (!_destroyed)
		{
			if (Requested)
			{
				ForceResetComponent();
			}
			_destroyed = true;
			_onDestroySignal.SafeInvoke();
			_onReleaseOrDestroySignal.SafeInvoke(_gameObject);
		}
	}

	private IEnumerator DestroyFailSafeDetection()
	{
		yield return new WaitForEndOfFrame();
		if (_gameObject != null)
		{
			logger.ErrorFormat("SEMANTIC ERROR: GameObject {0} had one of its GameComponent/NetworkComponent Destroyed. Destroying them are not supported at the moment. Fallback attempted", _gameObject.name);
			_destroyed = false;
		}
	}

	public void Initialize()
	{
	}

	public void OnRequested()
	{
		_released = false;
		_requested = true;
	}

	public void ResetComponent()
	{
		if (_requested)
		{
			_released = true;
			_requested = false;
			_onReleaseSignal.SafeInvoke();
			_onReleaseOrDestroySignal.SafeInvoke(_gameObject);
			if (_networkIdentity != null && _networkIdentity.isServer)
			{
				NetworkServer.UnSpawn(_gameObject);
				ResetNetworkIdentity(_networkIdentity);
			}
		}
	}

	public void ForceResetComponent()
	{
		_gameObject.GetComponents<IPoolableComponent>().Slinq().ForEach(delegate(IPoolableComponent pc)
		{
			pc.ResetComponent();
		});
	}
}
