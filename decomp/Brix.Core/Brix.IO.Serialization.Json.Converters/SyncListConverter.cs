using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.IO.Serialization.Json.Converters;

public class SyncListConverter : BrixJsonConverter
{
	internal Action<SyncList<GameObject>, List<GameObject>> ListSetter = CreateSetter<SyncList<GameObject>, List<GameObject>>(typeof(SyncList<GameObject>).GetField("m_Objects", BindingFlags.Instance | BindingFlags.NonPublic));

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(SyncList<GameObject>);
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		RemoveEntryFromWriterSerializeStack(serializer);
		JsonContract jsonContract = serializer.ContractResolver.ResolveContract(typeof(List<GameObject>));
		SerializeList(serializer, writer, value, jsonContract, null, null, null);
		AddToWriterSerializeStack(serializer, value);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ListSetter(existingValue as SyncList<GameObject>, serializer.Deserialize<List<GameObject>>(reader));
		return existingValue;
	}

	private static Action<S, T> CreateSetter<S, T>(FieldInfo field)
	{
		string name = field.ReflectedType.FullName + ".set_" + field.Name;
		DynamicMethod dynamicMethod = new DynamicMethod(name, null, new Type[2]
		{
			typeof(S),
			typeof(T)
		}, restrictedSkipVisibility: true);
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		if (field.IsStatic)
		{
			iLGenerator.Emit(OpCodes.Ldarg_1);
			iLGenerator.Emit(OpCodes.Stsfld, field);
		}
		else
		{
			iLGenerator.Emit(OpCodes.Ldarg_0);
			iLGenerator.Emit(OpCodes.Ldarg_1);
			iLGenerator.Emit(OpCodes.Stfld, field);
		}
		iLGenerator.Emit(OpCodes.Ret);
		return (Action<S, T>)dynamicMethod.CreateDelegate(typeof(Action<S, T>));
	}
}
