using Brix.External.Factories.Templates;
using Brix.Lifecycle.Pooling;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.External.Factories;

public class FactoryRegistrator : MonoBehaviour
{
	public static void RegisterAllFactories()
	{
		Factory.GetFactories().Slinq().ForEach(delegate(Factory f)
		{
			RegisterFactory(f);
		});
	}

	public void RegisterFactories()
	{
		Factory.GetFactories().Slinq().ForEach(delegate(Factory f)
		{
			RegisterFactory(f);
		});
	}

	public static void RegisterFactory(Factory f)
	{
		foreach (FactoryTemplate template in f.Templates)
		{
			FactoryTemplate t = template;
			SpawnDelegate spawnHandler = (Vector3 p, NetworkHash128 id) => ObjectPoolSingleton.Request_Unsafe(t.AssetKey, p, Quaternion.identity, active: false);
			UnSpawnDelegate unspawnHandler = delegate(GameObject go)
			{
				ObjectPoolSingleton.Release(go);
			};
			ClientScene.RegisterSpawnHandler(t.AssetKey.AssetId, spawnHandler, unspawnHandler);
		}
	}
}
