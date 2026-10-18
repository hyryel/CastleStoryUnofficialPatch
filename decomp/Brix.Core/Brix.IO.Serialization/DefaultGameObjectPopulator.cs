using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Brix.External.Factories;
using Brix.IO.Serialization.Json;
using Brix.IO.Serialization.Json.Converters;
using Brix.Utils.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Brix.IO.Serialization;

public class DefaultGameObjectPopulator : IGameObjectPopulator
{
	private static BrixLogger logger = new BrixLogger();

	public void Populate(string json, GameObject gameObject, JsonSerializer serializer)
	{
		Populate(JObject.Load(new JsonTextReader(new StringReader(json))), gameObject, serializer);
	}

	public void Populate(JObject jObject, GameObject gameObject, JsonSerializer serializer)
	{
		PopulateGameObjectName(jObject, gameObject);
		PopulateAllComponents(jObject, gameObject, serializer);
	}

	private void PopulateGameObjectName(JObject jObject, GameObject gameObject)
	{
		if (SerializationUtils.FieldExists("name", jObject) && gameObject.GetComponent<FactoryImprint>() == null)
		{
			gameObject.name = SerializationUtils.DeserializeField<string>("name", jObject);
		}
	}

	private void PopulateAllComponents(JObject jObject, GameObject gameObject, JsonSerializer serializer)
	{
		HashSet<Type> hashSet = new HashSet<Type>();
		if (!SerializationUtils.FieldExists("components", jObject))
		{
			return;
		}
		foreach (JObject item in jObject["components"].Cast<JObject>())
		{
			JObject jObject2 = item;
			if (SerializationUtils.FieldExists("$ref", jObject2))
			{
				continue;
			}
			Type type = DeserializeObjectType(jObject2, serializer);
			if (!hashSet.Add(type))
			{
				logger.Warn("GameObjectInternalPopulator.PopulateAllComponents() There was already a component of type '" + type.ToString() + "' deserialized. Second instance will be skipped");
				continue;
			}
			Component component = gameObject.GetComponent(type);
			if (component == null)
			{
				logger.Info("GameObjectPopulator.PopulateAllComponents(): No component '" + type.ToString() + "' found. Creating component for GO '" + gameObject.name + "'");
				component = gameObject.AddComponent(type);
			}
			try
			{
				PopulateComponent(serializer, component, jObject2);
			}
			catch (Exception innerException)
			{
				string message = "GameObjectPopulator.PopulateAllComponents(): Failed populating component of type '" + type.ToString() + "' for GO with the name '" + gameObject.name + "' and InstanceId#" + gameObject.GetInstanceID() + ". See inner exception for more details.";
				throw new JsonReaderException(message, innerException);
			}
		}
	}

	private Type DeserializeObjectType(JObject jsonComponent, JsonSerializer serializer)
	{
		string strTypeAndAssembly = SerializationUtils.DeserializeField<string>("$type", jsonComponent);
		return SerializationUtils.BindToType(strTypeAndAssembly, serializer);
	}

	public void PopulateComponent(JsonSerializer serializer, Component component, JToken jsonComponent)
	{
		JsonReader reader = jsonComponent.CreateReader();
		JsonConverter converter = serializer.ContractResolver.ResolveContract(component.GetType()).Converter;
		if (converter != null && converter.GetType() != typeof(MonoBehaviourConverter))
		{
			converter.ReadJson(reader, component.GetType(), component, serializer);
		}
		else
		{
			serializer.Populate(reader, component);
		}
	}
}
