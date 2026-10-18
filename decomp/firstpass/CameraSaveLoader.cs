using System.IO;
using Brix.IO.Serialization;
using Brix.IO.Serialization.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public static class CameraSaveLoader
{
	public static void LoadCameraTransform(string fullJsonSave)
	{
		JArray jArray = JArray.Parse(File.ReadAllText(fullJsonSave));
		foreach (JToken item in jArray)
		{
			JObject jObject = item as JObject;
			if (SerializationUtils.FieldExists("name", jObject) && SerializationUtils.DeserializeField<string>("name", jObject) == "Camera")
			{
				GameObjectPopulator.Populate(jObject, Camera.main.gameObject);
				break;
			}
		}
	}
}
