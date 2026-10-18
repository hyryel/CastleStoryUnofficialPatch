using System.Collections.Generic;
using System.Text;
using Brix.External.Factories;
using Brix.External.Factories.Templates;
using Brix.External.Signals;
using Brix.Utils;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.Lifecycle.Pooling;

[RequireComponent(typeof(AutoList))]
[ExecuteInEditMode]
public class ObjectPoolSingleton : MonoBehaviour, IObjectPoolSingleton
{
	private static BrixLogger logger = new BrixLogger();

	public bool DisablePooling = false;

	private static IObjectPoolSingleton _instance;

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

	private Dictionary<Factory.AssetKey, IGameObjectPool> indexedPools = new Dictionary<Factory.AssetKey, IGameObjectPool>();

	private static List<GameObject> _releaseStack = new List<GameObject>();

	public static IObjectPoolSingleton Instance => _instance;

	public Signal<Factory.AssetKey, GameObject> GameObjectRequestedSignal => _gameObjectRequestedSignal;

	public Signal<Factory.AssetKey, GameObject> GameObjectReleasedSignal => _gameObjectReleasedSignal;

	public Signal<Factory.AssetKey, GameObject> GameObjectBeforeReleasedSignal => _gameObjectBeforeReleasedSignal;

	public Dictionary<Factory.AssetKey, IGameObjectPool> IndexedPools => indexedPools;

	internal void Awake()
	{
		if (_instance is ObjectPoolSingleton)
		{
			logger.ErrorFormat("Singleton creation failed for type '{0}' on GameObject '{1}'. An instance already exists on GameObject '{2}'.", GetType().Name, base.name, ((ObjectPoolSingleton)_instance).name);
		}
		else
		{
			_instance = this;
		}
	}

	private void OnDestroy()
	{
		GameObjectRequestedSignal.Clear();
		GameObjectReleasedSignal.Clear();
		if (_instance is ObjectPoolSingleton && this != _instance as ObjectPoolSingleton)
		{
			logger.ErrorFormat("Singleton deletion failed for type '{0}' on GameObject '{1}'. Active instance exists on GameObject '{2}'.", GetType().Name, base.name, ((ObjectPoolSingleton)_instance).name);
		}
		else
		{
			_instance = null;
		}
		_instance = NullObjectPoolSingleton.Instance;
	}

	public static GameObject Request_Unsafe(Factory.AssetKey key)
	{
		return Request_Unsafe(key, Vector3.zero, Quaternion.identity);
	}

	public static GameObject Request_Unsafe(Factory.AssetKey key, bool active)
	{
		return Request_Unsafe(key, Vector3.zero, Quaternion.identity, active);
	}

	public static GameObject Request_Unsafe(Factory.AssetKey key, Vector3 position, Quaternion rotation)
	{
		Request(out var go, key, position, rotation);
		return go;
	}

	public static GameObject Request_Unsafe(Factory.AssetKey key, Vector3 position, Quaternion rotation, bool active)
	{
		Request(out var go, key, position, rotation, active);
		return go;
	}

	public static bool Request(out GameObject go, Factory.AssetKey key)
	{
		return Request(out go, key, Vector3.zero, Quaternion.identity);
	}

	public static bool Request(out GameObject go, Factory.AssetKey key, bool active)
	{
		return Request(out go, key, Vector3.zero, Quaternion.identity, active);
	}

	public static bool Request(out GameObject go, Factory.AssetKey key, Vector3 position, Quaternion rotation)
	{
		go = null;
		bool result = false;
		if (_instance.IndexedPools.TryGetValue(key, out var value))
		{
			if (value.RequestFromPool(out go, position, rotation))
			{
				_instance.GameObjectRequestedSignal.Invoke(key, go);
				result = true;
			}
		}
		else
		{
			logger.ErrorFormat("No GameObject Pool were found for AssetKey {0}", key);
		}
		return result;
	}

	public static bool Request(out GameObject go, Factory.AssetKey key, Vector3 position, Quaternion rotation, bool active)
	{
		go = null;
		bool result = false;
		if (_instance.IndexedPools.TryGetValue(key, out var value))
		{
			if (value.RequestFromPool(out go, position, rotation, active))
			{
				_instance.GameObjectRequestedSignal.Invoke(key, go);
				result = true;
			}
		}
		else
		{
			logger.ErrorFormat("No GameObject Pool were found for AssetKey {0}", key);
		}
		return result;
	}

	public static bool RequestComponent<T>(out T t, Factory.AssetKey key, bool active = true) where T : Component
	{
		return RequestComponent<T>(out t, key, Vector3.zero, Quaternion.identity, active);
	}

	public static bool RequestComponent<T>(out T t, Factory.AssetKey key, Vector3 position, Quaternion rotation, bool active = true) where T : Component
	{
		t = null;
		if (!Request(out var go, key, position, rotation, active))
		{
			return false;
		}
		t = go.GetComponent<T>();
		if (t != null)
		{
			return true;
		}
		Release(go);
		logger.ErrorFormat("GameObject Pool Request failed for assetKey '{0}'. Requested component '{1}' could not be found.", key, typeof(T).Name);
		return false;
	}

	public static T RequestComponent_Unsafe<T>(Factory.AssetKey key, bool active = true) where T : Component
	{
		return RequestComponent_Unsafe<T>(key, Vector3.zero, Quaternion.identity, active);
	}

	public static T RequestComponent_Unsafe<T>(Factory.AssetKey key, Vector3 position, Quaternion rotation, bool active = true) where T : Component
	{
		RequestComponent<T>(out var t, key, position, rotation, active);
		return t;
	}

	public static void Release(GameObject go)
	{
		FactoryImprint factoryImprint = ((go == null) ? null : go.GetComponent<FactoryImprint>());
		if (factoryImprint == null)
		{
			Object.Destroy(go);
		}
		else
		{
			Release(factoryImprint);
		}
	}

	public static void Release(FactoryImprint imprint)
	{
		GameObject gameObject = imprint.gameObject;
		if (imprint.Released)
		{
			logger.ErrorFormat("Gameobject {0} with AssetKey {1} was already released. GameObject release aborted!", imprint.name, imprint.AssetKey);
			return;
		}
		if (!AddToReleaseStack(gameObject))
		{
			logger.ErrorFormat("Gameobject {0} with AssetKey {1} seems stuck in a release loop. GameObject release aborted!", imprint.name, imprint.AssetKey);
			return;
		}
		try
		{
			Instance.GameObjectBeforeReleasedSignal.Invoke(imprint.AssetKey, gameObject);
			ResetGameObject(imprint.gameObject);
			Instance.GameObjectReleasedSignal.Invoke(imprint.AssetKey, gameObject);
			if (!_instance.IndexedPools.TryGetValue(imprint.AssetKey, out var value))
			{
				Object.Destroy(gameObject);
			}
			else
			{
				value.ReleaseToPool(gameObject);
			}
		}
		finally
		{
			PopFromReleaseStack();
		}
	}

	private static bool AddToReleaseStack(GameObject releasedGO)
	{
		if (_releaseStack.Contains(releasedGO))
		{
			return false;
		}
		_releaseStack.Add(releasedGO);
		return true;
	}

	private static void PopFromReleaseStack()
	{
		_releaseStack.Pop();
	}

	public void GeneratePools()
	{
		foreach (Factory factory in Factory.GetFactories())
		{
			CloneTemplate[] componentsInChildren = factory.GetComponentsInChildren<CloneTemplate>(includeInactive: true);
			foreach (CloneTemplate cloneTemplate in componentsInChildren)
			{
				Factory.AssetKey assetKey = cloneTemplate.AssetKey;
				PoolConfiguration component = cloneTemplate.GetComponent<PoolConfiguration>();
				Transform poolContainer = GeneratePoolGameObjectContainer(assetKey);
				IGameObjectPool value;
				if (component == null || DisablePooling)
				{
					value = CreateDummyObjectPool(poolContainer, assetKey);
				}
				else
				{
					value = component.CreatePoolAt(poolContainer, assetKey);
					Object.Destroy(component);
				}
				IndexedPools.Add(assetKey, value);
			}
		}
	}

	private IGameObjectPool CreateDummyObjectPool(Transform poolContainer, Factory.AssetKey key)
	{
		PoolMonitor orAddComponent = poolContainer.gameObject.GetOrAddComponent<PoolMonitor>();
		return orAddComponent.GameObjectPool = new DummyGameObjectPool(key, poolContainer);
	}

	private Transform GeneratePoolGameObjectContainer(Factory.AssetKey assetKey)
	{
		StringBuilder stringBuilder = new StringBuilder(base.gameObject.FullPath());
		return GameObjectResolver.FindOrCreateGameObject(stringBuilder.Append('/').Append(assetKey.Factory).Append('/')
			.Append(assetKey.Name)
			.ToString());
	}

	internal void AddPoolForKey(Factory.AssetKey key)
	{
		Transform poolContainer = GeneratePoolGameObjectContainer(key);
		IGameObjectPool value = CreateDummyObjectPool(poolContainer, key);
		IndexedPools.Add(key, value);
	}

	public IGameObjectPool GetGameObjectPool(Factory.AssetKey key)
	{
		return IndexedPools.GetValueOrDefault(key);
	}

	private static void ResetGameObject(GameObject go)
	{
		IResetable[] components = go.GetComponents<IResetable>();
		for (int num = components.Length - 1; num >= 0; num--)
		{
			components[num].ResetComponent();
		}
		ResetTransform(go);
	}

	private static void ResetTransform(GameObject go)
	{
		Transform transform = go.transform;
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
	}
}
