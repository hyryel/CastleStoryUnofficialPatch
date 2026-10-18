using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Brix.IO.Serialization.Json.Converters;

public abstract class BrixJsonConverter : JsonConverter
{
	private MethodInfo _serializeObjectMethodInfo;

	private MethodInfo _serializeListMethodInfo;

	private MethodInfo _serializeMultiArrayMethodInfo;

	protected FieldInfo _newtonsoftSerializerWriter;

	protected FieldInfo _newtonsoftSerializeStack;

	public BrixJsonConverter()
	{
		BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.NonPublic;
		Type type = typeof(JsonSerializer).Assembly.GetType("Newtonsoft.Json.Serialization.JsonSerializerInternalWriter");
		Type[] types = new Type[6]
		{
			typeof(JsonWriter),
			typeof(object),
			typeof(JsonObjectContract),
			typeof(JsonProperty),
			typeof(JsonContainerContract),
			typeof(JsonProperty)
		};
		_serializeObjectMethodInfo = type.GetMethod("SerializeObject", bindingAttr, null, types, null);
		Type[] types2 = new Type[6]
		{
			typeof(JsonWriter),
			typeof(IEnumerable),
			typeof(JsonArrayContract),
			typeof(JsonProperty),
			typeof(JsonContainerContract),
			typeof(JsonProperty)
		};
		_serializeListMethodInfo = type.GetMethod("SerializeList", bindingAttr, null, types2, null);
		Type[] types3 = new Type[6]
		{
			typeof(JsonWriter),
			typeof(Array),
			typeof(JsonArrayContract),
			typeof(JsonProperty),
			typeof(JsonContainerContract),
			typeof(JsonProperty)
		};
		_serializeMultiArrayMethodInfo = type.GetMethod("SerializeMultidimensionalArray", bindingAttr, null, types3, null);
		Type type2 = typeof(JsonSerializer).Assembly.GetType("Newtonsoft.Json.Serialization.JsonSerializerProxy");
		_newtonsoftSerializerWriter = type2.GetField("_serializerWriter", bindingAttr);
		_newtonsoftSerializeStack = type.GetField("_serializeStack", bindingAttr);
	}

	protected void AddToWriterSerializeStack(JsonSerializer serializer, object value)
	{
		object value2 = _newtonsoftSerializerWriter.GetValue(serializer);
		List<object> list = (List<object>)_newtonsoftSerializeStack.GetValue(value2);
		list.Add(value);
	}

	protected void RemoveEntryFromWriterSerializeStack(JsonSerializer serializer)
	{
		object value = _newtonsoftSerializerWriter.GetValue(serializer);
		List<object> list = (List<object>)_newtonsoftSerializeStack.GetValue(value);
		list.RemoveAt(list.Count - 1);
	}

	protected void SerializeObject(JsonSerializer serializer, params object[] parameters)
	{
		object value = _newtonsoftSerializerWriter.GetValue(serializer);
		_serializeObjectMethodInfo.Invoke(value, parameters);
	}

	protected void SerializeList(JsonSerializer serializer, params object[] parameters)
	{
		object value = _newtonsoftSerializerWriter.GetValue(serializer);
		_serializeListMethodInfo.Invoke(value, parameters);
	}

	protected void SerializeMultiArray(JsonSerializer serializer, params object[] parameters)
	{
		object value = _newtonsoftSerializerWriter.GetValue(serializer);
		_serializeMultiArrayMethodInfo.Invoke(value, parameters);
	}
}
