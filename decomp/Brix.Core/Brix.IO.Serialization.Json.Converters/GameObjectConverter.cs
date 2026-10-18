using System;
using Brix.Utils.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Brix.IO.Serialization.Json.Converters;

public class GameObjectConverter : JsonConverter
{
	private static BrixLogger logger = new BrixLogger();

	public JsonWriterConverter WriterConverter { get; set; }

	public JsonReaderConverter ReaderConverter { get; set; }

	public IAssetKeyResolver AssetKeyResolver { get; set; }

	public IGameObjectFactory GameObjectFactory { get; set; }

	public IGameObjectPopulator Populator { get; set; }

	public GameObjectConverter()
	{
		WriterConverter = new JsonWriterConverter(this);
		ReaderConverter = new JsonReaderConverter(this);
		AssetKeyResolver = new AssetKeyResolver();
		GameObjectFactory = new PooledGameObjectFactory();
		Populator = new DefaultGameObjectPopulator();
	}

	public override bool CanConvert(Type objectType)
	{
		logger.Debug("GameObjectConverter.CanConvert(): " + objectType);
		return objectType == typeof(GameObject);
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		MonoBehaviour monoBehaviour = value as MonoBehaviour;
		GameObject gameObject = (value as GameObject) ?? monoBehaviour.gameObject;
		try
		{
			WriterConverter.Write(writer, gameObject, serializer, monoBehaviour);
		}
		catch (Exception innerException)
		{
			string message = "Serialization failed for GameObject with the name '" + gameObject.name + "'.\n See inner exception for more details";
			logger.Error(message);
			throw new JsonWriterException(message, innerException);
		}
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		JToken jToken = JToken.Load(reader);
		try
		{
			if (jToken.Type == JTokenType.Object)
			{
				return ReadJson(jToken as JObject, serializer);
			}
			return null;
		}
		catch (Exception ex)
		{
			string message = $"GameObject deserialization failed. See inner exception for more details. json:{jToken}'";
			logger.Error(message);
			throw ex;
		}
	}

	public object ReadJson(JObject jsonObject, JsonSerializer serializer)
	{
		return ReaderConverter.Read(jsonObject, serializer);
	}

	public void PushContext(JsonSerializer serializer, GameObject contextualGameObject)
	{
		serializer.BrixReferenceResolver().PushContext(contextualGameObject);
	}

	public GameObject PopContext(JsonSerializer serializer, GameObject gameObject)
	{
		GameObject gameObject2 = serializer.BrixReferenceResolver().PopContext();
		if (gameObject != gameObject2)
		{
			throw new JsonWriterException("The serialization context (GameObject context) has been corrupted. GameObject context expected was '" + gameObject.name + "' but was '" + gameObject2.name + "' instead.");
		}
		return gameObject2;
	}
}
