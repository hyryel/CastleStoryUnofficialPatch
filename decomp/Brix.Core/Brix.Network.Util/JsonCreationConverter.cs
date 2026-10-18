using System;
using System.Collections;
using Brix.Utils.Logging;
using Newtonsoft.Json;

namespace Brix.Network.Util;

public abstract class JsonCreationConverter<T> : JsonConverter
{
	private static BrixLogger logger = new BrixLogger();

	protected abstract T CreateArray(JsonReader reader, JsonSerializer serializer);

	public override bool CanConvert(Type objectType)
	{
		return typeof(T).IsAssignableFrom(objectType);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.StartArray)
		{
			return CreateArray(reader, serializer);
		}
		return serializer.Deserialize<T>(reader);
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		logger.Info("JsonCreationConverter<T>.WriteJson() value: " + value);
		writer.WriteStartArray();
		foreach (object item in (IList)value)
		{
			serializer.Serialize(writer, item);
		}
		writer.WriteEndArray();
	}
}
