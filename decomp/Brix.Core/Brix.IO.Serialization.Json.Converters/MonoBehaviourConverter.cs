using System;
using Brix.Utils.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace Brix.IO.Serialization.Json.Converters;

public class MonoBehaviourConverter : BrixJsonConverter
{
	private static BrixLogger logger = new BrixLogger();

	private GameObjectConverter goConverter = new GameObjectConverter();

	public override bool CanConvert(Type objectType)
	{
		logger.Debug("GameObjectConverter.CanConvert(): " + objectType);
		return typeof(MonoBehaviour).IsAssignableFrom(objectType);
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		MonoBehaviour monoBehaviour = value as MonoBehaviour;
		if (monoBehaviour == null)
		{
			serializer.Serialize(writer, null);
			return;
		}
		RemoveEntryFromWriterSerializeStack(serializer);
		if (serializer.BrixReferenceResolver().PeekContext() != monoBehaviour.gameObject)
		{
			goConverter.WriteJson(writer, monoBehaviour, serializer);
		}
		else if (serializer.BrixReferenceResolver().componentReferenceResolver.IsReferenced(serializer, monoBehaviour))
		{
			SerializeAsReference(writer, serializer, monoBehaviour);
		}
		else
		{
			serializer.BrixReferenceResolver().componentReferenceResolver.GetReference(serializer, monoBehaviour);
			JsonContract jsonContract = serializer.ContractResolver.ResolveContract(value.GetType());
			SerializeObject(serializer, writer, value, (JsonObjectContract)jsonContract, null, null, null);
		}
		AddToWriterSerializeStack(serializer, value);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.Null)
		{
			return null;
		}
		JObject jObject = JObject.Load(reader);
		try
		{
			Component component;
			if (SerializationUtils.FieldExists("$component", jObject))
			{
				GameObject gameObject = goConverter.ReadJson(jObject, serializer) as GameObject;
				string strTypeAndAssembly = SerializationUtils.DeserializeField<string>("$component", jObject);
				component = gameObject.GetComponent(SerializationUtils.BindToType(strTypeAndAssembly, serializer));
			}
			else if (SerializationUtils.FieldExists("$ref", jObject))
			{
				string text = SerializationUtils.DeserializeField<string>("$ref", jObject);
				component = serializer.ReferenceResolver.ResolveReference(serializer, text) as Component;
				if (component == null)
				{
					logger.Warn("Could not resolve the component with reference '" + text + "'.");
				}
			}
			else
			{
				GameObject gameObject2 = serializer.BrixReferenceResolver().PeekContext();
				if (!(gameObject2 != null))
				{
					string message = string.Concat("MonoBehaviourConverter.ReadJson(): Trying to deserialize a component of type '", objectType, "' without a GameObject Context. Serialization will abort due to irrecuperable failure");
					throw new JsonReaderException(message);
				}
				component = gameObject2.GetComponent(objectType);
				if (component == null)
				{
					logger.Warn(string.Concat("MonoBehaviourConverter.ReadJson(): No matching component was found for component '", objectType, "'. Creating component for GO '", gameObject2.name, "' with InstanceId#", gameObject2.GetInstanceID()));
					component = gameObject2.AddComponent(objectType);
				}
				try
				{
					GameObjectPopulator.PopulateComponent(serializer, component, jObject);
				}
				catch (Exception innerException)
				{
					string message2 = string.Concat("MonoBehaviourConverter.ReadJson(): Failed populating component of type '", objectType, "' for GO with the name '", gameObject2.name, "' and InstanceId#", gameObject2.GetInstanceID(), ". See inner exception for more details.");
					throw new JsonReaderException(message2, innerException);
				}
			}
			return component;
		}
		catch (JsonReaderException ex)
		{
			throw ex;
		}
		catch (Exception innerException2)
		{
			string message3 = string.Concat("MonoBehaviourConverter.ReadJson(): Failed to deserialize a component of type '", objectType, "'. Serialization will abort due to irrecuperable failure. json=\n", jObject);
			throw new JsonReaderException(message3, innerException2);
		}
	}

	private void SerializeAsReference(JsonWriter writer, JsonSerializer serializer, MonoBehaviour monoBehaviour)
	{
		goConverter.WriteJson(writer, monoBehaviour, serializer);
	}
}
