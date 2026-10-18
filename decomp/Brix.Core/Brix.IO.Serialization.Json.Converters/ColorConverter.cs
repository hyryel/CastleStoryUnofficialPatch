using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Brix.IO.Serialization.Json.Converters;

public class ColorConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(Color);
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		Color color = (Color)value;
		writer.WriteStartObject();
		writer.WritePropertyName("$type");
		writer.WriteValue(SerializationUtils.GetNewtonsoftTypeName(typeof(Color), serializer));
		writer.WritePropertyName("a");
		serializer.Serialize(writer, color.a);
		writer.WritePropertyName("b");
		serializer.Serialize(writer, color.b);
		writer.WritePropertyName("g");
		serializer.Serialize(writer, color.g);
		writer.WritePropertyName("r");
		serializer.Serialize(writer, color.r);
		writer.WriteEndObject();
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		JObject jObject = JObject.Load(reader);
		Color color = (Color)existingValue;
		color.a = SerializationUtils.DeserializeField<float>("a", jObject);
		color.b = SerializationUtils.DeserializeField<float>("b", jObject);
		color.g = SerializationUtils.DeserializeField<float>("g", jObject);
		color.r = SerializationUtils.DeserializeField<float>("r", jObject);
		return color;
	}
}
