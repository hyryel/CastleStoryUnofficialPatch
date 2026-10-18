using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Brix.IO.Serialization;

public class GameObjectPopulator
{
	private static DefaultGameObjectPopulator populator = new DefaultGameObjectPopulator();

	public static void Populate(string json, GameObject gameObject)
	{
		Populate(JObject.Load(new JsonTextReader(new StringReader(json))), gameObject);
	}

	public static void Populate(JObject jObject, GameObject gameObject, string target = "save")
	{
		JsonSerializer serializer = JsonSerializer.Create(Io.GetLoadSerializerSettings(target));
		populator.Populate(jObject, gameObject, serializer);
	}

	public static void PopulateComponent(JsonSerializer serializer, Component component, JToken jsonComponent)
	{
		populator.PopulateComponent(serializer, component, jsonComponent);
	}
}
