using System;
using System.Collections.Generic;
using Brix.External.Factories;
using Brix.External.Signals;
using UnityEngine;

namespace Brix.Lifecycle.Pooling;

internal class NullObjectPoolSingleton : IObjectPoolSingleton
{
	private class NullGameObjectPool : IGameObjectPool
	{
		private static NullGameObjectPool instance = new NullGameObjectPool();

		public static NullGameObjectPool Instance => instance;

		public int PoolSize => 0;

		public int InstancesRequested => 0;

		public int InitialPoolSize => 0;

		public int PoolSizeLimit => 0;

		public int InstanceLimit => 0;

		public bool SpawnsActive { get; private set; }

		public bool KeepActive { get; private set; }

		public Transform DefaultOutputPath
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public event Action GameObjectAddedToPool;

		public event Action<GameObject> GameObjectRequestedFromPool;

		public void InitializePool()
		{
		}

		public GameObject CreatePoolObject()
		{
			return null;
		}

		public void ReleaseToPool(GameObject go)
		{
			UnityEngine.Object.Destroy(go);
		}

		public bool RequestFromPool(out GameObject gameObjectToReturn)
		{
			gameObjectToReturn = null;
			return false;
		}

		public void OnInstanceDestroyed()
		{
		}

		public bool RequestFromPool(out GameObject returnedGO, Vector3 p, Quaternion r)
		{
			returnedGO = null;
			return false;
		}

		public bool RequestFromPool(out GameObject returnedGO, Vector3 p, Quaternion r, bool active)
		{
			returnedGO = null;
			return false;
		}
	}

	private static NullObjectPoolSingleton instance = new NullObjectPoolSingleton();

	private Signal<Factory.AssetKey, GameObject> _gameObjectRequestedSignal = new Signal<Factory.AssetKey, GameObject>
	{
		MaximumConnections = 384
	};

	private Signal<Factory.AssetKey, GameObject> _gameObjectReleasedSignal = new Signal<Factory.AssetKey, GameObject>
	{
		MaximumConnections = 384
	};

	private Signal<Factory.AssetKey, GameObject> _gameObjectBeforeReleasedSignal = new Signal<Factory.AssetKey, GameObject>
	{
		MaximumConnections = 384
	};

	public static NullObjectPoolSingleton Instance => instance;

	public Signal<Factory.AssetKey, GameObject> GameObjectRequestedSignal => _gameObjectRequestedSignal;

	public Signal<Factory.AssetKey, GameObject> GameObjectReleasedSignal => _gameObjectReleasedSignal;

	public Signal<Factory.AssetKey, GameObject> GameObjectBeforeReleasedSignal => _gameObjectBeforeReleasedSignal;

	public Dictionary<Factory.AssetKey, IGameObjectPool> IndexedPools => new Dictionary<Factory.AssetKey, IGameObjectPool>();

	public void GeneratePools()
	{
	}

	public IGameObjectPool GetGameObjectPool(Factory.AssetKey key)
	{
		return NullGameObjectPool.Instance;
	}
}
