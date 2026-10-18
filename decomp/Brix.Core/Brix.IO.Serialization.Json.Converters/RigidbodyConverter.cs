using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Brix.IO.Serialization.Json.Converters;

public class RigidbodyConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(Rigidbody);
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		writer.WriteStartObject();
		Rigidbody rigidbody = (Rigidbody)value;
		writer.WritePropertyName("$type");
		writer.WriteValue(SerializationUtils.GetNewtonsoftTypeName(typeof(Rigidbody), serializer));
		if (rigidbody.angularVelocity != new Vector3(0f, 0f, 0f))
		{
			writer.WritePropertyName("angularVelocity");
			serializer.Serialize(writer, rigidbody.angularVelocity);
		}
		if (rigidbody.velocity != new Vector3(0f, 0f, 0f))
		{
			writer.WritePropertyName("velocity");
			serializer.Serialize(writer, rigidbody.velocity);
		}
		writer.WritePropertyName("isKinematic");
		serializer.Serialize(writer, rigidbody.isKinematic);
		writer.WritePropertyName("useGravity");
		serializer.Serialize(writer, rigidbody.useGravity);
		writer.WriteEndObject();
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		JObject jObject = JObject.Load(reader);
		Rigidbody rigidbody = (Rigidbody)existingValue;
		rigidbody.useGravity = serializer.Deserialize<bool>(jObject["useGravity"].CreateReader());
		rigidbody.isKinematic = serializer.Deserialize<bool>(jObject["isKinematic"].CreateReader());
		if (jObject["velocity"] != null)
		{
			rigidbody.velocity = serializer.Deserialize<Vector3>(jObject["velocity"].CreateReader());
		}
		if (jObject["angularVelocity"] != null)
		{
			rigidbody.angularVelocity = serializer.Deserialize<Vector3>(jObject["angularVelocity"].CreateReader());
		}
		return existingValue;
	}
}
