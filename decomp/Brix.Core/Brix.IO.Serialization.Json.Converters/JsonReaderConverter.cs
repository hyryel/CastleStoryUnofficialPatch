using System;
using Brix.External.Factories;
using Brix.Utils;
using Brix.Utils.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.IO.Serialization.Json.Converters;

public class JsonReaderConverter
{
	private static BrixLogger logger = new BrixLogger();

	private GameObjectConverter GOConverter;

	public JsonReaderConverter(GameObjectConverter goConverter)
	{
		GOConverter = goConverter;
	}

	public object Read(JObject jObject, JsonSerializer serializer)
	{
		GameObject gameObject;
		if (SerializationUtils.FieldExists("$ref", jObject))
		{
			string text = SerializationUtils.DeserializeField<string>("$ref", jObject);
			gameObject = serializer.ReferenceResolver.ResolveReference(serializer, text) as GameObject;
			if (gameObject == null)
			{
				logger.Warn("Could not resolve the GameObject with reference '" + text + "'.");
			}
		}
		else
		{
			gameObject = DeserializeGameObject(jObject, serializer);
		}
		return gameObject;
	}

	private GameObject DeserializeGameObject(JObject jObject, JsonSerializer serializer)
	{
		logger.Debug("JsonReaderConverter.DeserializeGameObject() jObject: " + jObject);
		GameObject gameObject = null;
		if (SerializationUtils.TryDeserializeField<bool>("DoNotCreateOnDeserialized", jObject))
		{
			string path = GetScenePath(jObject) + GetName(jObject);
			gameObject = ResolveGameObjectInScene(path, jObject, serializer);
			if (gameObject.GetComponent<IdentityHolder>() == null)
			{
				gameObject.AddComponent<FactorylessImprint>();
			}
		}
		else
		{
			if (SerializationUtils.TryDeserializeField<bool>("FactorylessGameObject", jObject))
			{
				string parentPath = GetScenePath(jObject) + GetName(jObject);
				gameObject = GameObjectResolver.FindGameObjectInScene(parentPath);
			}
			gameObject = gameObject ?? CreateGameObject(jObject, serializer);
		}
		RegisterGameObject(jObject, serializer, gameObject);
		DeserializeParent(jObject, serializer);
		GOConverter.PushContext(serializer, gameObject);
		GOConverter.Populator.Populate(jObject, gameObject, serializer);
		GOConverter.PopContext(serializer, gameObject);
		ParentGameObject(jObject, serializer, gameObject);
		SetActiveSelf(jObject, gameObject);
		SpawnIfNetworkObject(gameObject);
		return gameObject;
	}

	private void SpawnIfNetworkObject(GameObject gameObject)
	{
		NetworkIdentity component = gameObject.GetComponent<NetworkIdentity>();
		if (component != null && component.netId.IsEmpty())
		{
			FactoryImprint component2 = gameObject.GetComponent<FactoryImprint>();
			if ((bool)component2)
			{
				NetworkServer.Spawn(gameObject, component2.AssetKey.AssetId);
			}
		}
	}

	private GameObject DeserializeParent(JObject jObject, JsonSerializer serializer)
	{
		JToken jToken = jObject["$parent"];
		if (jToken == null)
		{
			return null;
		}
		if (jToken.Type == JTokenType.Object)
		{
			return serializer.Deserialize<GameObject>(jToken.CreateReader());
		}
		return serializer.ReferenceResolver.ResolveReference(serializer, (string)jToken) as GameObject;
	}

	private void ParentGameObject(JObject jObject, JsonSerializer serializer, GameObject gameObject)
	{
		string scenePath = GetScenePath(jObject);
		if ("/".Equals(scenePath))
		{
			gameObject.transform.SetParent(null);
			return;
		}
		GameObject gameObject2 = ResolveGameObjectInScene(scenePath, jObject, serializer);
		bool worldPositionStays = gameObject.transform.position != Vector3.zero;
		gameObject.transform.SetParent(gameObject2.transform, worldPositionStays);
	}

	private static GameObject ResolveGameObjectInScene(string path, JObject jObject, JsonSerializer serializer)
	{
		GameObject gameObject = TryResolveGameObjectInScene(path, serializer);
		if (gameObject != null)
		{
			return gameObject;
		}
		throw new JsonReaderException($"The GameObject '{path}' could not be found in the scene. Make sure the path is valid and exists in the scene before trying to load the following JSon. JSON:\n{jObject}");
	}

	private static GameObject TryResolveGameObjectInScene(string path, JsonSerializer serializer)
	{
		return GameObjectResolver.ResolveGameObjectInScene(path, serializer.BrixReferenceResolver());
	}

	private GameObject CreateGameObject(JObject jObject, JsonSerializer serializer)
	{
		GameObject gameObject = null;
		if (SerializationUtils.TryDeserializeField<bool>("FactorylessGameObject", jObject))
		{
			gameObject = new GameObject();
			gameObject.SetActive(value: false);
			gameObject.AddComponent<SerializationComponent>().FactorylessGameObject = true;
			gameObject.AddComponent<FactorylessImprint>();
			return gameObject;
		}
		return GOConverter.GameObjectFactory.Spawn(GOConverter.AssetKeyResolver.ResolveAssetKey(jObject), jObject);
	}

	private void SetActiveSelf(JObject jObject, GameObject gameObject)
	{
		bool flag = true;
		if (SerializationUtils.FieldExists("activeSelf", jObject))
		{
			flag = SerializationUtils.DeserializeField<bool>("activeSelf", jObject);
		}
		if (!flag)
		{
			gameObject.SetActive(value: true);
			gameObject.SetActive(value: false);
		}
		gameObject.SetActive(flag);
	}

	private void RegisterGameObject(JObject jObject, JsonSerializer serializer, GameObject gameObject)
	{
		IdentityHolder component = gameObject.GetComponent<IdentityHolder>();
		if (!SerializationUtils.FieldExists("$id", jObject) || component == null || component.Registered)
		{
			return;
		}
		string text = SerializationUtils.DeserializeField<string>("$id", jObject);
		try
		{
			serializer.ReferenceResolver.AddReference(serializer, text, gameObject);
			component.Id = text.Substring(3);
			component.Registered = true;
		}
		catch (Exception ex)
		{
			logger.Error($"The referenceId '{text}' caused an error when trying register it via the reference resolver.");
			throw ex;
		}
	}

	private string GetName(JObject jObject)
	{
		return SerializationUtils.DeserializeField<string>("name", jObject).Trim();
	}

	private string GetScenePath(JObject jObject)
	{
		if (SerializationUtils.FieldExists("$path", jObject))
		{
			string text = SerializationUtils.DeserializeField<string>("$path", jObject).Trim();
			if (!text.EndsWith("/"))
			{
				text += "/";
			}
			return text;
		}
		logger.Warn("The $path field was not found for GameObject with the name '" + GetName(jObject) + "'. The object will be considered at the root of the scene graph");
		return "/";
	}
}
