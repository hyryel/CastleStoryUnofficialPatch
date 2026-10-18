using System;
using Brix.External.Factories;
using Brix.Utils.Logging;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Brix.IO.Serialization;

public class DefaultGameObjectFactory : IGameObjectFactory
{
	private static BrixLogger logger = new BrixLogger();

	public GameObject Spawn(object assetKey, JObject jObject)
	{
		try
		{
			return Factory.Instantiate<Transform>(assetKey as Factory.AssetKey, active: false).gameObject;
		}
		catch (Exception ex)
		{
			logger.Error($"The AssetKey '{assetKey}' caused an error when trying to create a GO via the factory");
			throw ex;
		}
	}
}
