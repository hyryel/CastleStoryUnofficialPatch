using System;
using Brix.External.Factories;
using Brix.IO.Serialization.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Brix.IO.Serialization;

public class AssetKeyResolver : IAssetKeyResolver
{
	public string ResolveAssetKey(GameObject gameObject)
	{
		SerializationComponent component = gameObject.GetComponent<SerializationComponent>();
		if (component != null && !component.DoNotCreateOnDeserialized && !component.FactorylessGameObject)
		{
			return gameObject.GetInterface<AssetKeyHolder>().AssetKey.ToString();
		}
		return null;
	}

	public object ResolveAssetKey(JObject gameObject)
	{
		string text = SerializationUtils.DeserializeField<string>("$assetKey", gameObject).Trim();
		if (string.IsNullOrEmpty(text) || text.Length < 3 || text[0] == '.' || text[text.Length - 1] == '.')
		{
			throw new ArgumentException("The GameObject's AssetKey '" + text + "' is invalid in the following.");
		}
		return new Factory.AssetKey(text);
	}
}
