using Brix.External.Factories;
using UnityEngine;

namespace Brix.Lifecycle.Pooling;

public class PoolConfiguration : MonoBehaviour
{
	public int InitialPoolSize;

	public int MaximumPoolSize;

	public int InstanceLimit;

	public bool KeepActive;

	public IGameObjectPool CreatePoolAt(Transform poolContainer, Factory.AssetKey key)
	{
		PoolMonitor orAddComponent = poolContainer.gameObject.GetOrAddComponent<PoolMonitor>();
		return orAddComponent.GameObjectPool = new GameObjectPool(key, poolContainer, this);
	}
}
