using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Brix.IO.Serialization;

public interface IGameObjectPopulator
{
	void Populate(string json, GameObject gameObject, JsonSerializer serializer);

	void Populate(JObject jObject, GameObject gameObject, JsonSerializer serializer);

	void PopulateComponent(JsonSerializer serializer, Component component, JToken jsonComponent);
}
