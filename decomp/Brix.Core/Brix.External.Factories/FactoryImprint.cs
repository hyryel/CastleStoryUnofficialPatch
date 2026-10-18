using System;
using Brix.Components;
using Brix.Lifecycle.Pooling;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.External.Factories;

public class FactoryImprint : BrixComponent, AssetKeyHolder, IdentityHolder
{
	public static bool _onApplicationQuit = false;

	private static BrixLogger logger = new BrixLogger();

	private bool _registered;

	[SerializeField]
	private Factory.AssetKey _assetKey = new Factory.AssetKey("", "");

	private string _id = null;

	public bool Registered
	{
		get
		{
			return _registered;
		}
		set
		{
			_registered = value;
		}
	}

	public float Timestamp { get; internal set; }

	public Factory.AssetKey AssetKey
	{
		get
		{
			return _assetKey;
		}
		set
		{
			_assetKey = value;
		}
	}

	public string Id
	{
		get
		{
			if (!_registered && string.IsNullOrEmpty(_id))
			{
				GenerateNewId();
			}
			return _id;
		}
		set
		{
			if (_registered && _id != value)
			{
				UnregisterId();
			}
			_id = value;
			RegisterId();
		}
	}

	public string PeekId()
	{
		return _id;
	}

	private void GenerateNewId()
	{
		_id = Guid.NewGuid().ToString();
		RegisterId();
	}

	protected override void Awake()
	{
		base.Awake();
		if (!string.IsNullOrEmpty(_id))
		{
			RegisterId();
		}
	}

	protected override void OnDestroy()
	{
		AutoList instance = BrixSingleton<AutoList>.Instance;
		IObjectPoolSingleton instance2 = ObjectPoolSingleton.Instance;
		if (instance != null && instance.RemoveFromAutoList(_assetKey, base.gameObject) && instance2 != null)
		{
			instance2.GetGameObjectPool(_assetKey)?.OnInstanceDestroyed();
			if (!_onApplicationQuit)
			{
				logger.Warn($"The GameObject '{base.name}' with AssetKey '{_assetKey}' had to be forcefully removed from the autolist. Is the GameObject being destroyed without going through the pool?");
			}
		}
	}

	private void OnApplicationQuit()
	{
		_onApplicationQuit = true;
	}

	private void RegisterId()
	{
		if (!_registered)
		{
			BrixSingleton<MainGameObjectRegistry>.Instance.Register(Id, base.gameObject);
			_registered = true;
		}
	}

	private void UnregisterId()
	{
		if (_id != null && !_onApplicationQuit)
		{
			BrixSingleton<MainGameObjectRegistry>.Instance.Unregister(_id);
			_registered = false;
		}
	}

	public override void OnRequested()
	{
		base.OnRequested();
		Timestamp = Time.time;
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		UnregisterId();
		_id = null;
	}
}
