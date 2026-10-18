using System;
using Brix.Utils.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Brix.IO.Serialization.Json.Converters;

public class ActivityRunConverter : JsonConverter
{
	private static BrixLogger logger = new BrixLogger();

	public override bool CanConvert(Type objectType)
	{
		bool flag = false;
		if (flag)
		{
			logger.Debug(string.Concat("ActivityRunConverter.CanConvert(): ", objectType, " canConvert: ", flag.ToString()));
		}
		return flag;
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		logger.Warn("ActivityRunConverter.ReadJson() objectType: " + objectType);
		return null;
	}

	protected void WriteIdProperty(JsonWriter writer, object value, JsonSerializer serializer)
	{
		bool flag = false;
		if (value != null)
		{
			JsonContract jsonContract = serializer.ContractResolver.ResolveContract(value.GetType());
			flag = jsonContract.IsReference ?? ((serializer.PreserveReferencesHandling & PreserveReferencesHandling.Objects) == PreserveReferencesHandling.Objects);
		}
		if (flag)
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
