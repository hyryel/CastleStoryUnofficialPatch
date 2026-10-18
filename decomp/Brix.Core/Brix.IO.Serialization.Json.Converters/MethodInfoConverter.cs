using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Brix.IO.Serialization.Json.Converters;

public class MethodInfoConverter : JsonConverter
{
	private static Type methodInfoType = RearViewMirror.GetType("System.Reflection.MonoMethod");

	public override bool CanConvert(Type objectType)
	{
		return objectType.IsAssignableFrom(methodInfoType);
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		MethodInfo methodInfo = value as MethodInfo;
		writer.WriteStartObject();
		writer.WritePropertyName("$type");
		writer.WriteValue(SerializationUtils.GetNewtonsoftTypeName(methodInfo.GetType(), serializer));
		writer.WritePropertyName("reflectedType");
		writer.WriteValue(SerializationUtils.GetNewtonsoftTypeName(methodInfo.ReflectedType, serializer));
		writer.WritePropertyName("methodName");
		writer.WriteValue(methodInfo.Name);
		writer.WriteEndObject();
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		JObject jObject = JObject.Load(reader);
		if (!SerializationUtils.FieldExists("reflectedType", jObject))
		{
			throw new JsonReaderException($"MethodInfoConverter could not find the field 'reflectedType' in the following JObject : {jObject}");
		}
		string strTypeAndAssembly = SerializationUtils.DeserializeField<string>("reflectedType", jObject).Trim();
		Type type = SerializationUtils.BindToType(strTypeAndAssembly, serializer);
		if (!SerializationUtils.FieldExists("methodName", jObject))
		{
			throw new JsonReaderException($"MethodInfoConverter could not find the field 'methodName' in the following JObject : {jObject}");
		}
		string text = SerializationUtils.DeserializeField<string>("methodName", jObject).Trim();
		MethodInfo method = type.GetMethod(text, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		if (method == null)
		{
			throw new JsonReaderException($"Error: MethodInfoConverter could not resolve the MethodInfo for the type '{type}' for a method named '{text}'");
		}
		return method;
	}
}
