using System.Reflection;
using Brix.Common.Collections;
using Brix.External.Factories;
using Brix.External.Factories.Templates;
using Brix.Lifecycle.Pooling;
using Brix.Utils.Logging;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Network;

public class FactoryRegistrator : MonoBehaviour
{
	private static BrixLogger logger = new BrixLogger(typeof(FactoryRegistrator).FullName);

	public void RegisterFactories()
	{
		Factory.GetFactories().Slinq().ForEach(delegate(Factory f)
		{
			RegisterFactory(f);
		});
	}

	public static void RegisterFactory(Factory f)
	{
		foreach (Factory.AssetKey item in (from t in f.Templates.Slinq()
			select t.AssetKey).NoLeak())
		{
			RegisterHandlerForTemplate(item);
		}
	}

	private static void RegisterHandlerForTemplate(Factory.AssetKey key)
	{
		SpawnDelegate spawnHandler = (Vector3 p, NetworkHash128 id) => ObjectPoolSingleton.Request_Unsafe(key, p, Quaternion.identity, active: false);
		UnSpawnDelegate unspawnHandler = delegate(GameObject go)
		{
			ObjectPoolSingleton.Release(go);
		};
		NetworkIdentity component = Factory.Peek(key).GetComponent<NetworkIdentity>();
		if ((bool)component && component.assetId.IsValid())
		{
			FieldInfo field = typeof(NetworkIdentity).GetField("m_AssetId", BindingFlags.Instance | BindingFlags.NonPublic);
			field.SetValue(Factory.Peek(key).GetComponent<NetworkIdentity>(), NetworkHash128.Parse("0"));
		}
		ClientScene.RegisterSpawnHandler(key.AssetId, spawnHandler, unspawnHandler);
		logger.DebugFormat("The AssetKey '{0}' was registered to UNET via the assetID '{1}'", key, key.AssetId.ToString());
	}
}
