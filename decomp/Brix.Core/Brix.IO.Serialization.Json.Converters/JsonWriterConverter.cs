using System;
using System.Collections.Generic;
using System.Linq;
using Brix.Utils.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace Brix.IO.Serialization.Json.Converters;

public class JsonWriterConverter
{
	private static BrixLogger logger = new BrixLogger();

	private GameObjectConverter GOConverter;

	public JsonWriterConverter(GameObjectConverter goConverter)
	{
		GOConverter = goConverter;
	}

	public void Write(JsonWriter writer, GameObject gameObject, JsonSerializer serializer, MonoBehaviour component = null)
	{
		if (!IsSerializable(gameObject))
		{
			serializer.Serialize(writer, null);
			return;
		}
		writer.WriteStartObject();
		if (component != null)
		{
			WriteComponentProperty(writer, component.GetType(), serializer);
		}
		if (IsGameObjectAlreadySerialized(gameObject, serializer))
		{
			WriteRefProperty(writer, gameObject, serializer);
		}
		else
		{
			GOConverter.PushContext(serializer, gameObject);
			WriteJsonHeader(writer, gameObject, serializer);
			WriteJsonBody(writer, gameObject, serializer);
			GOConverter.PopContext(serializer, gameObject);
		}
		writer.WriteEndObject();
	}

	protected void WriteJsonHeader(JsonWriter writer, GameObject gameObject, JsonSerializer serializer)
	{
		WriteIdProperty(writer, gameObject, serializer);
		WriteTypeProperty(writer, typeof(GameObject), serializer);
		WriteAssetKeyProperty(writer, gameObject, serializer);
		WriteParentProperty(writer, gameObject, serializer);
		WritePathProperty(writer, gameObject, serializer);
	}

	protected void WriteJsonBody(JsonWriter writer, GameObject gameObject, JsonSerializer serializer)
	{
		WriteActiveSelfProperty(writer, gameObject, serializer);
		WriteSerializingProperties(writer, gameObject, serializer);
		WriteNameProperty(writer, gameObject, serializer);
		WriteComponentsProperty(writer, gameObject, serializer);
	}

	private void WriteSerializingProperties(JsonWriter writer, GameObject gameObject, JsonSerializer serializer)
	{
		SerializationComponent component = gameObject.GetComponent<SerializationComponent>();
		if (component == null || component.FactorylessGameObject)
		{
			writer.WritePropertyName("FactorylessGameObject");
			writer.WriteValue(value: true);
		}
		if (component != null && component.DoNotCreateOnDeserialized)
		{
			writer.WritePropertyName("DoNotCreateOnDeserialized");
			writer.WriteValue(value: true);
		}
	}

	protected void WriteIdProperty(JsonWriter writer, GameObject gameObject, JsonSerializer serializer)
	{
		JsonContract jsonContract = serializer.ContractResolver.ResolveContract(gameObject.GetType());
		if (jsonContract.IsReference ?? ((serializer.PreserveReferencesHandling & PreserveReferencesHandling.Objects) == PreserveReferencesHandling.Objects))
		{
			writer.WritePropertyName("$id");
			writer.WriteValue(serializer.ReferenceResolver.GetReference(serializer, gameObject));
		}
	}

	protected void WriteActiveSelfProperty(JsonWriter writer, GameObject gameObject, JsonSerializer serializer)
	{
		if (!gameObject.activeSelf)
		{
			writer.WritePropertyName("activeSelf");
			writer.WriteValue(gameObject.activeSelf);
		}
	}

	private void WriteNameProperty(JsonWriter writer, GameObject gameObject, JsonSerializer serializer)
	{
		writer.WritePropertyName("name");
		serializer.Serialize(writer, gameObject.name);
	}

	protected void WriteRefProperty(JsonWriter writer, GameObject gameObject, JsonSerializer serializer)
	{
		writer.WritePropertyName("$ref");
		writer.WriteValue(serializer.ReferenceResolver.GetReference(serializer, gameObject));
	}

	protected void WriteTypeProperty(JsonWriter writer, Type type, JsonSerializer serializer)
	{
		writer.WritePropertyName("$type");
		writer.WriteValue(SerializationUtils.GetNewtonsoftTypeName(type, serializer));
	}

	protected void WriteComponentProperty(JsonWriter writer, Type type, JsonSerializer serializer)
	{
		writer.WritePropertyName("$component");
		writer.WriteValue(SerializationUtils.GetNewtonsoftTypeName(type, serializer));
	}

	protected void WriteAssetKeyProperty(JsonWriter writer, GameObject gameObject, JsonSerializer serializer)
	{
		string text = GOConverter.AssetKeyResolver.ResolveAssetKey(gameObject);
		if (text != null)
		{
			writer.WritePropertyName("$assetKey");
			writer.WriteValue(text);
		}
	}

	private void WritePathProperty(JsonWriter writer, GameObject gameObject, JsonSerializer serializer)
	{
		writer.WritePropertyName("$path");
		GameObject obj = ((gameObject.transform.parent == null) ? null : gameObject.transform.parent.gameObject);
		writer.WriteValue(GetGameObjectPath(obj, serializer));
	}

	private void WriteParentProperty(JsonWriter writer, GameObject gameObject, JsonSerializer serializer)
	{
		GameObject nearestSerializableParent = GetNearestSerializableParent(gameObject, serializer);
		if (nearestSerializableParent != null)
		{
			writer.WritePropertyName("$parent");
			if (serializer.ReferenceResolver.IsReferenced(serializer, nearestSerializableParent))
			{
				serializer.Serialize(writer, serializer.ReferenceResolver.GetReference(serializer, nearestSerializableParent));
			}
			else
			{
				serializer.Serialize(writer, nearestSerializableParent);
			}
		}
	}

	private GameObject GetNearestSerializableParent(GameObject gameObject, JsonSerializer serializer)
	{
		SerializationComponent serializationComponent = null;
		Transform parent;
		while (true)
		{
			parent = gameObject.transform.parent;
			if (parent == null)
			{
				return null;
			}
			serializationComponent = parent.gameObject.GetComponent<SerializationComponent>();
			if (serializationComponent != null || serializer.ReferenceResolver.IsReferenced(null, parent.gameObject))
			{
				break;
			}
			gameObject = parent.gameObject;
		}
		return parent.gameObject;
	}

	private void WriteComponentsProperty(JsonWriter writer, GameObject gameObject, JsonSerializer serializer)
	{
		writer.WritePropertyName("components");
		IEnumerable<Component> value = from c in gameObject.GetComponents(typeof(Component)).NotNull()
			where HasASerializableField(c, serializer) || HasConverter(c.GetType(), serializer)
			select c;
		serializer.Serialize(writer, value);
	}

	private bool IsSerializable(GameObject gameObject)
	{
		if (gameObject == null)
		{
			return false;
		}
		Transform transform = gameObject.transform;
		do
		{
			SerializationComponent component = transform.GetComponent<SerializationComponent>();
			if (component != null && component.DoNotSerialize)
			{
				return false;
			}
			transform = transform.parent;
		}
		while (!(transform == null));
		return true;
	}

	private static bool IsGameObjectAlreadySerialized(GameObject gameObject, JsonSerializer serializer)
	{
		return serializer.ReferenceResolver.IsReferenced(serializer, gameObject);
	}

	private bool HasConverter(Type objectType, JsonSerializer serializer)
	{
		JsonContract jsonContract = serializer.ContractResolver.ResolveContract(objectType);
		return jsonContract != null && jsonContract.Converter != null && jsonContract.Converter.GetType() != typeof(MonoBehaviourConverter);
	}

	private bool HasASerializableField(Component component, JsonSerializer serializer)
	{
		bool result = serializer.ContractResolver.ResolveContract(component.GetType()) is JsonObjectContract jsonObjectContract && jsonObjectContract.Properties.Any((JsonProperty p) => p.IsPropertySerializable(component));
		logger.Debug(string.Concat("The component '", component.GetType(), "' does not contain any serializable field."));
		return result;
	}

	private string GetGameObjectPath(GameObject obj, JsonSerializer serializer)
	{
		if (obj == null)
		{
			return "/";
		}
		string text = "/" + GetPathNodeName(obj, serializer);
		while (obj.transform.parent != null)
		{
			obj = obj.transform.parent.gameObject;
			text = "/" + GetPathNodeName(obj, serializer) + text;
		}
		return text;
	}

	private string GetPathNodeName(GameObject go, JsonSerializer serializer)
	{
		if (serializer.ReferenceResolver.IsReferenced(serializer, go))
		{
			return serializer.BrixReferenceResolver().GetGameObjectID(go);
		}
		return go.name;
	}
}
