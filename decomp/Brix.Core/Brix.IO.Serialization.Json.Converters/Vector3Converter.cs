using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Brix.IO.Serialization.Json.Converters;

public class Vector3Converter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(Vector3);
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		Vector3 vector = (Vector3)value;
		writer.WriteStartObject();
		writer.WritePropertyName("$type");
		writer.WriteValue(SerializationUtils.GetNewtonsoftTypeName(typeof(Vector3), serializer));
		writer.WritePropertyName("x");
		serializer.Serialize(writer, vector.x);
		writer.WritePropertyName("y");
		serializer.Serialize(writer, vector.y);
		writer.WritePropertyName("z");
		serializer.Serialize(writer, vector.z);
		writer.WriteEndObject();
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		JObject jObject = JObject.Load(reader);
		return new Vector3(SerializationUtils.DeserializeField<float>("x", jObject), SerializationUtils.DeserializeField<float>("y", jObject), SerializationUtils.DeserializeField<float>("z", jObject));
	}
}
