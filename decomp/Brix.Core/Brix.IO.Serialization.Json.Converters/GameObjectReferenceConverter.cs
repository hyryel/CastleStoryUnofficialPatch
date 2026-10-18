using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Brix.IO.Serialization.Json.Converters;

public class GameObjectReferenceConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(GameObject);
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		if (!serializer.ReferenceResolver.IsReferenced(serializer, value))
		{
			writer.WriteValue((UnityEngine.Object)null);
		}
		writer.WriteValue(serializer.ReferenceResolver.GetReference(serializer, value));
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		return reader.TokenType switch
		{
			JsonToken.StartObject => serializer.Deserialize(reader), 
			JsonToken.String => serializer.ReferenceResolver.ResolveReference(serializer, (string)reader.Value) as GameObject, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
