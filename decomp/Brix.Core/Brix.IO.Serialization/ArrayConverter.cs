using System;
using System.Collections.Generic;
using Brix.IO.Serialization.Json;
using Brix.Network.Util;
using Brix.Utils.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Brix.IO.Serialization;

public class ArrayConverter : JsonCreationConverter<List<object>>
{
	private static BrixLogger logger = new BrixLogger();

	protected override List<object> CreateArray(JsonReader reader, JsonSerializer serializer)
	{
		List<object> list = new List<object>();
		JArray jArray = JArray.Load(reader);
		ConvertArrayValues(jArray, list, serializer);
		return list;
	}

	private void ConvertArrayValues(JArray jArray, List<object> array, JsonSerializer serializer)
	{
		foreach (JToken item in jArray)
		{
			if (item is JValue)
			{
				JValue jValue = item as JValue;
				try
				{
					if (jValue.Value is long || jValue.Value is long)
					{
						array.Add(Convert.ToInt32(jValue.Value));
					}
					else
					{
						array.Add(jValue.Value);
					}
				}
				catch (Exception message)
				{
					logger.Error(message);
				}
			}
			else if (item is JArray)
			{
				JArray jArray2 = item as JArray;
				List<object> list = new List<object>();
				ConvertArrayValues(jArray2, list, serializer);
				array.Add(list);
			}
			else
			{
				if (!(item is JObject))
				{
					continue;
				}
				JObject jObject = item as JObject;
				string text = jObject.Ref();
				Type type = DeserializeObjectType(jObject, serializer);
				if (text != null && type == null)
				{
					array.Add(serializer.ReferenceResolver.ResolveReference(serializer, text));
					continue;
				}
				logger.Info("Deserializing generic object (POCO) using objType: " + type);
				if (type != null)
				{
					array.Add(serializer.Deserialize(jObject.CreateReader(), type));
				}
				else
				{
					array.Add(serializer.Deserialize(jObject.CreateReader()));
				}
			}
		}
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
