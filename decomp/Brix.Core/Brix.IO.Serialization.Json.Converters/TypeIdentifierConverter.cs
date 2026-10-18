using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Brix.IO.Serialization.Json.Converters;

public class TypeIdentifierConverter : BrixJsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return false;
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		RemoveEntryFromWriterSerializeStack(serializer);
		serializer.Serialize(writer, value, typeof(object));
		AddToWriterSerializeStack(serializer, value);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		JToken jToken = JToken.Load(reader);
		if (jToken is JArray)
		{
			JArray jArray = jToken as JArray;
			object[] array = new object[jArray.Count];
			int num = 0;
			foreach (JToken item in jArray)
			{
				if (item is JArray)
				{
					array[num++] = serializer.Deserialize(item.CreateReader());
				}
				else
				{
					array[num++] = DeserializeNonArrayValue(serializer, item);
				}
			}
			return array;
		}
		return DeserializeNonArrayValue(serializer, jToken);
	}

	private object DeserializeNonArrayValue(JsonSerializer serializer, JToken jtoken)
	{
		if (jtoken is JValue)
		{
			JValue jValue = jtoken as JValue;
			if (jValue.Value is long || jValue.Value is long)
			{
				return Convert.ToInt32(jValue.Value);
			}
			return jValue.Value;
		}
		JObject jObject = jtoken as JObject;
		string text = jObject.Ref();
		Type type = DeserializeObjectType(jObject, serializer);
		object obj = null;
		return SerializationUtils.FieldExists("$component", jObject) ? serializer.Deserialize(jObject.CreateReader(), type) : ((text != null) ? serializer.ReferenceResolver.ResolveReference(serializer, text) : ((type == null) ? serializer.Deserialize(jObject.CreateReader()) : serializer.Deserialize(jObject.CreateReader(), type)));
	}

	private Type DeserializeObjectType(JObject json, JsonSerializer serializer)
	{
		string text = null;
		if (SerializationUtils.FieldExists("$type", json))
		{
			text = SerializationUtils.DeserializeField<string>("$type", json);
		}
		else if (SerializationUtils.FieldExists("$component", json))
		{
			text = SerializationUtils.DeserializeField<string>("$component", json);
		}
		return (text != null) ? SerializationUtils.BindToType(text) : null;
	}
}
