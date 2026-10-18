using System;
using Brix.External.Factories;
using Brix.Lifecycle.Pooling;
using Brix.Utils.Logging;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Brix.IO.Serialization;

public class PooledGameObjectFactory : IGameObjectFactory
{
	private static BrixLogger logger = new BrixLogger();

	public GameObject Spawn(object assetKey, JObject jObject)
	{
		try
		{
			ObjectPoolSingleton.Request(out var go, assetKey as Factory.AssetKey, active: false);
			return go;
		}
		catch (Exception ex)
		{
			logger.ErrorFormat(ex, "The GameObject pool request with key '{0}' failed. See inner exception for more details.", assetKey);
			throw ex;
		}
	}
}
