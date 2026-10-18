using System;
using Brix.Components;
using Brix.External.Factories;
using Brix.External.Factories.Templates;
using Brix.IO.Serialization;
using Brix.Utils;
using UnityEngine;

namespace Brix.Lifecycle.Pooling;

public abstract class AbstractGameObjectPool : IGameObjectPool
{
	[SerializeField]
	protected Factory.AssetKey AssetKey;

	[SerializeField]
	protected Factory Factory;

	[SerializeField]
	protected int _instancesRequested;

	[SerializeField]
	protected int NextInstanceID;

	[SerializeField]
	protected FactoryTemplate Template;

	[SerializeField]
	private Transform _defaultOutputPath;

	[SerializeField]
	protected abstract Transform PoolContainer { get; }

	public bool SpawnsActive => !Template.SpawnInactive;

	public int InstancesRequested => _instancesRequested;

	public Transform DefaultOutputPath
	{
		get
		{
			return (_defaultOutputPath != null) ? _defaultOutputPath : (_defaultOutputPath = GameObjectResolver.FindOrCreateGameObject(Factory.InstantiatedObjectDefaultPath).transform);
		}
		set
		{
			_defaultOutputPath = value;
		}
	}

	public abstract int PoolSize { get; }

	public abstract int InitialPoolSize { get; }

	public abstract int PoolSizeLimit { get; }

	public abstract int InstanceLimit { get; }

	public abstract bool KeepActive { get; }

	public event Action GameObjectAddedToPool;

	public event Action<GameObject> GameObjectRequestedFromPool;

	public AbstractGameObjectPool(Factory.AssetKey key)
	{
		AssetKey = key;
		Factory = Factory.GetFactory(key.Factory);
		_defaultOutputPath = GameObjectResolver.FindOrCreateGameObject(Factory.InstantiatedObjectDefaultPath).transform;
		Template = Factory.Peek(key).GetComponent<FactoryTemplate>();
		GameObjectRequestedFromPool += OnGameObjectRequested;
	}

	public GameObject CreatePoolObject()
	{
		return CreatePoolObject(Vector3.zero, Quaternion.identity);
	}

	protected GameObject CreatePoolObject(Vector3 position, Quaternion rotation)
	{
		GameObject gameObject = Factory.Instantiate(AssetKey, position, rotation, active: false);
		SerializationComponent component = gameObject.GetComponent<SerializationComponent>();
		if (component != null)
		{
			component.Name = (component.DoNotCreateOnDeserialized ? AssetKey.Name : $"{AssetKey.Name} ({NextInstanceID++})");
		}
		else
		{
			gameObject.GetComponent<IBrixComponent>().Name = $"{AssetKey.Name} ({NextInstanceID++})";
		}
		gameObject.transform.SetParent(PoolContainer);
		if (this.GameObjectAddedToPool != null)
		{
			this.GameObjectAddedToPool();
		}
		return gameObject;
	}

	protected void OnGameObjectRequestedFromPool(GameObject go)
	{
		if (this.GameObjectRequestedFromPool != null)
		{
			this.GameObjectRequestedFromPool(go);
		}
	}

	protected void OnGameObjectAddedToPool()
	{
		if (this.GameObjectAddedToPool != null)
		{
			this.GameObjectAddedToPool();
		}
	}

	public bool RequestFromPool(out GameObject returnedGO, Vector3 p, Quaternion r)
	{
		return RequestFromPool(out returnedGO, p, r, SpawnsActive);
	}

	protected void OnGameObjectRequested(GameObject go)
	{
		IRequestable[] components = go.GetComponents<IRequestable>();
		for (int num = components.Length - 1; num >= 0; num--)
		{
			components[num].OnRequested();
		}
	}

	public void OnInstanceDestroyed()
	{
		_instancesRequested--;
	}

	public abstract void ReleaseToPool(GameObject go);

	public abstract bool RequestFromPool(out GameObject returnedGO, Vector3 p, Quaternion r, bool active);
}
