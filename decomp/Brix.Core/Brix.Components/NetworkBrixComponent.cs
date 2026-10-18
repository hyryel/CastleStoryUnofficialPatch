using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Lifecycle.Pooling;
using Brix.Utils.Logging;
using Newtonsoft.Json;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Components;

[JsonObject(MemberSerialization.OptIn)]
public class NetworkBrixComponent : NetworkBehaviour, IBrixComponent, IPoolableComponent, IInitializable, IRequestable, IResetable
{
	private static BrixLogger logger = new BrixLogger();

	private bool _awoken;

	private bool _started;

	private bool _silentDestroyed;

	private BrixComponentPoco _bcPoco;

	public bool Initialized => _bcPoco != null;

	public bool Awoken => _awoken;

	public bool Started => _started;

	public BrixComponentPoco BrixComponentPoco => _bcPoco ?? (_bcPoco = new BrixComponentPoco(base.gameObject));

	public string Name
	{
		get
		{
			return _bcPoco.Name;
		}
		set
		{
			_bcPoco.Name = value;
		}
	}

	public IReadOnlySignal<GameObject> OnReleaseOrDestroySignal => _bcPoco.OnReleaseOrDestroySignal;

	public IReadOnlySignal OnReleaseSignal => _bcPoco.OnReleaseSignal;

	public IReadOnlySignal OnDestroySignal => _bcPoco.OnDestroySignal;

	public bool Requested => Initialized && _bcPoco.Requested;

	public bool Released => Initialized && _bcPoco.Released;

	public bool Destroyed => Initialized && _bcPoco.Destroyed;

	public void DestroySilently()
	{
		_silentDestroyed = true;
		Object.Destroy(this);
	}

	protected virtual void Awake()
	{
		if (!Initialized)
		{
			ForceInitialize();
		}
		_awoken = true;
	}

	protected virtual void Start()
	{
		if (!Requested)
		{
			ForceOnRequested();
		}
		_started = true;
	}

	protected virtual void OnDestroy()
	{
		if (!_silentDestroyed)
		{
			_bcPoco.OnDestroy();
		}
	}

	public virtual void Initialize()
	{
		_bcPoco = GetComponent<IBrixComponent>().BrixComponentPoco;
		_bcPoco.Initialize();
	}

	private void ForceInitialize()
	{
		IBrixComponent component = GetComponent<IBrixComponent>();
		if (!component.Initialized)
		{
			component.Initialize();
		}
		if (component as NetworkBrixComponent != this)
		{
			Initialize();
		}
	}

	public virtual void OnRequested()
	{
		_bcPoco.OnRequested();
	}

	private void ForceOnRequested()
	{
		GetComponents<IBrixComponent>().Slinq().ForEach(delegate(IBrixComponent bc)
		{
			bc.OnRequested();
		});
	}

	public virtual void ResetComponent()
	{
		if (_bcPoco == null)
		{
			logger.ErrorFormat("NetworkBrixComponent.ResetComponent: MonoBehaviour '{0}' of gameObject '{1}' is not initialized! Was the component added at runtime?", GetType(), base.name);
		}
		else
		{
			_bcPoco.ResetComponent();
		}
	}
}
