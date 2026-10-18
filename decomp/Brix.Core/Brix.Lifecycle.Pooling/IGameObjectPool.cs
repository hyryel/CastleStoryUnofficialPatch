using System;
using UnityEngine;

namespace Brix.Lifecycle.Pooling;

public interface IGameObjectPool
{
	int PoolSize { get; }

	int InstancesRequested { get; }

	int InitialPoolSize { get; }

	int PoolSizeLimit { get; }

	int InstanceLimit { get; }

	bool KeepActive { get; }

	bool SpawnsActive { get; }

	Transform DefaultOutputPath { get; set; }

	event Action GameObjectAddedToPool;

	event Action<GameObject> GameObjectRequestedFromPool;

	void ReleaseToPool(GameObject go);

	void OnInstanceDestroyed();

	GameObject CreatePoolObject();

	bool RequestFromPool(out GameObject returnedGO, Vector3 p, Quaternion r);

	bool RequestFromPool(out GameObject returnedGO, Vector3 p, Quaternion r, bool active);
}
