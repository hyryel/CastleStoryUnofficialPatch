using System;
using Brix.External.Factories;
using Newtonsoft.Json;

namespace Brix.IO.Serialization.Json.Converters;

public class AssetKeyConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(Factory.AssetKey);
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		Factory.AssetKey assetKey = (Factory.AssetKey)value;
		writer.WriteValue(assetKey.ToString());
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		return new Factory.AssetKey((string)reader.Value);
	}
}
