using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Brix.IO.Serialization.Json.Converters;

public class QuaternionConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(Quaternion);
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		Quaternion quaternion = (Quaternion)value;
		writer.WriteStartObject();
		writer.WritePropertyName("$type");
		writer.WriteValue(SerializationUtils.GetNewtonsoftTypeName(typeof(Vector3), serializer));
		writer.WritePropertyName("w");
		serializer.Serialize(writer, quaternion.w);
		writer.WritePropertyName("x");
		serializer.Serialize(writer, quaternion.x);
		writer.WritePropertyName("y");
		serializer.Serialize(writer, quaternion.y);
		writer.WritePropertyName("z");
		serializer.Serialize(writer, quaternion.z);
		writer.WriteEndObject();
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		JObject jObject = serializer.Deserialize<JObject>(reader);
		return new Quaternion(SerializationUtils.DeserializeField<float>("w", jObject), SerializationUtils.DeserializeField<float>("x", jObject), SerializationUtils.DeserializeField<float>("y", jObject), SerializationUtils.DeserializeField<float>("z", jObject));
	}
}
