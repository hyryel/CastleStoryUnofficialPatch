using System;
using Brix.Utils.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Brix.IO.Serialization.Json.Converters;

public class BehaviorConverter : JsonConverter
{
	private static BrixLogger logger = new BrixLogger();

	public override bool CanConvert(Type objectType)
	{
		bool flag = false;
		if (flag)
		{
			logger.Debug(string.Concat("BehaviorConverter.CanConvert(): ", objectType, " canConvert: ", flag.ToString()));
		}
		return flag;
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		return null;
	}

	protected void WriteIdProperty(JsonWriter writer, object value, JsonSerializer serializer)
	{
		JsonContract jsonContract = serializer.ContractResolver.ResolveContract(value.GetType());
		if (jsonContract.IsReference ?? ((serializer.PreserveReferencesHandling & PreserveReferencesHandling.Objects) == PreserveReferencesHandling.Objects))
		{
			writer.WritePropertyName("$id");
			writer.WriteValue(serializer.ReferenceResolver.GetReference(serializer, value));
		}
	}

	protected void WriteTypeProperty(JsonWriter writer, Type type, JsonSerializer serializer)
	{
		writer.WritePropertyName("$type");
		writer.WriteValue(SerializationUtils.GetNewtonsoftTypeName(type, serializer));
	}
}
