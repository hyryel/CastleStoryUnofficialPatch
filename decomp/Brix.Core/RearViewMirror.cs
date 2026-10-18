using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

public class RearViewMirror
{
	private static Dictionary<string, Type> cache = new Dictionary<string, Type>();

	private static List<string> _usualAssemblies = new List<string> { ", Assembly-UnityScript-firstpass, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", ", Assembly-UnityScript, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", ", Brix.Core, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", ", UnityEngine, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", ", Newtonsoft.Json, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null", ", mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" };

	private static void LogError(string message)
	{
		string text = StackTraceUtility.ExtractStackTrace();
		string[] array = text.Split('\n');
		foreach (string text2 in array)
		{
			Match match = Regex.Match(text2, "\\(at\\s([^\\:]+)\\:([\\d]+)\\)");
			if (match.Success && !match.Groups[0].Value.Contains("RearViewMirror.cs"))
			{
				MethodInfo method = typeof(Debug).GetMethod("LogPlayerBuildError", BindingFlags.Static | BindingFlags.NonPublic);
				method.Invoke(null, new object[4]
				{
					message + "\n" + text,
					match.Groups[1].Value,
					Convert.ToInt32(match.Groups[2].Value),
					0
				});
				return;
			}
			text = text.Replace(text2 + "\n", "");
		}
		Debug.LogError("[Report to JokieW - Bad RVM log] " + message);
	}

	private static BindingFlags GetFlags(object instance)
	{
		if (instance == null)
		{
			return BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
		}
		return BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
	}

	public static Type GetType(string name, string assembly)
	{
		return Type.GetType(name + ", " + assembly + ", Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
	}

	public static Type GetType(string name)
	{
		if (cache.ContainsKey(name))
		{
			return cache[name];
		}
		Type type = null;
		type = Type.GetType(name);
		if (type == null)
		{
			foreach (string usualAssembly in _usualAssemblies)
			{
				type = Type.GetType(name + usualAssembly);
				if (type != null)
				{
					break;
				}
			}
		}
		if (type == null)
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				Type[] types = assembly.GetTypes();
				foreach (Type type2 in types)
				{
					if (type2.Name == name)
					{
						Debug.LogWarning("RearViewMirror: Had to look through every assembly for [" + type2.AssemblyQualifiedName + "]");
						cache.Add(name, type2);
						return type2;
					}
				}
			}
			LogError("RearViewMirror: Couldn't find [" + name + "]");
		}
		if (type != null)
		{
			cache.Add(name, type);
		}
		return type;
	}

	public static Type GetGenericType(string name, params string[] generics)
	{
		return GetGenericType(GetType(name), generics);
	}

	public static Type GetGenericType(Type name, params string[] generics)
	{
		Type[] array;
		if (generics != null)
		{
			array = new Type[generics.Length];
			for (int i = 0; i != generics.Length; i++)
			{
				array[i] = GetType(generics[i]);
			}
		}
		else
		{
			array = new Type[0];
		}
		return name.MakeGenericType(array);
	}

	public static MethodInfo GetGenericMethod(Type classType, string name, Type[] generics)
	{
		MethodInfo[] methods = classType.GetMethods();
		foreach (MethodInfo methodInfo in methods)
		{
			if (methodInfo.IsGenericMethodDefinition && methodInfo.Name == name)
			{
				Type[] genericArguments = methodInfo.GetGenericArguments();
				if (genericArguments.Length == generics.Length)
				{
					return methodInfo.MakeGenericMethod(generics);
				}
			}
		}
		return null;
	}

	public static MethodInfo GetGenericMethod(Type classType, string name, Type[] generics, object[] parameters)
	{
		MethodInfo[] methods = classType.GetMethods();
		foreach (MethodInfo methodInfo in methods)
		{
			if (!methodInfo.IsGenericMethodDefinition || !(methodInfo.Name == name))
			{
				continue;
			}
			Type[] genericArguments = methodInfo.GetGenericArguments();
			if (genericArguments.Length != generics.Length)
			{
				continue;
			}
			ParameterInfo[] parameters2 = methodInfo.GetParameters();
			if (parameters2.Length != parameters.Length)
			{
				continue;
			}
			for (int j = 0; j <= parameters2.Length; j++)
			{
				if (j == parameters2.Length)
				{
					return methodInfo.MakeGenericMethod(generics);
				}
				Type parameterType = parameters2[j].ParameterType;
				Type[] derivedTypesAndInterfaces = GetDerivedTypesAndInterfaces(parameters[j].GetType());
				bool flag = false;
				Type[] array = derivedTypesAndInterfaces;
				foreach (Type type in array)
				{
					if (parameterType == type)
					{
						flag = true;
					}
					else
					{
						int num = Array.IndexOf(generics, type);
						if (num >= 0)
						{
							if (parameterType == genericArguments[num])
							{
								flag = true;
							}
						}
						else if (parameterType.IsGenericType && type.IsGenericType && parameterType.GetGenericTypeDefinition() == type.GetGenericTypeDefinition())
						{
							flag = true;
						}
					}
					if (flag)
					{
						break;
					}
				}
				if (!flag)
				{
					break;
				}
			}
		}
		return null;
	}

	private static Type[] GetDerivedTypesAndInterfaces(Type type)
	{
		List<Type> list = new List<Type>(type.GetInterfaces());
		while (type != null)
		{
			list.Add(type);
			type = type.BaseType;
		}
		return list.ToArray();
	}

	public static Type[] GetTypes(object[] objects)
	{
		if (objects != null)
		{
			Type[] array = new Type[objects.Length];
			for (int i = 0; i != objects.Length; i++)
			{
				if (objects[i] == null)
				{
					return null;
				}
				array[i] = objects[i].GetType();
			}
			return array;
		}
		return new Type[0];
	}

	public static object CallCtor(string typeName, params object[] parameters)
	{
		return CallCtor(GetType(typeName), parameters);
	}

	public static object CallCtor(Type type, params object[] parameters)
	{
		if (type != null)
		{
			ConstructorInfo constructor = type.GetConstructor(GetTypes(parameters));
			return constructor.Invoke(parameters);
		}
		return null;
	}

	public static object CallGenericMethod(string typeName, object instance, string name, Type[] genericTypes, params object[] parameters)
	{
		return CallGenericMethod(GetType(typeName), instance, name, genericTypes, parameters);
	}

	public static object CallGenericMethod(Type type, object instance, string name, Type[] genericTypes, params object[] parameters)
	{
		if (type != null)
		{
			MethodInfo genericMethod = GetGenericMethod(type, name, genericTypes, parameters);
			if (genericMethod != null)
			{
				return genericMethod.Invoke(instance, parameters);
			}
			LogError("RearViewMirror:CallGenericMethod can't find " + ((instance == null) ? "STATIC" : "INSTANCE") + " METHOD " + type.Name + ".[" + name + "]'" + genericTypes.Length);
		}
		return null;
	}

	public static object CallMethod(string typeName, object instance, string name, params object[] parameters)
	{
		return CallMethod(GetType(typeName), instance, name, parameters);
	}

	public static object CallMethod(Type type, object instance, string name, params object[] parameters)
	{
		if (type != null)
		{
			Type[] types = GetTypes(parameters);
			MethodInfo methodInfo = ((types != null) ? type.GetMethod(name, GetFlags(instance), null, GetTypes(parameters), null) : type.GetMethod(name, GetFlags(instance)));
			if (methodInfo != null)
			{
				return methodInfo.Invoke(instance, parameters);
			}
			LogError("RearViewMirror:CallMethod can't find " + ((instance == null) ? "STATIC" : "INSTANCE") + " METHOD " + type.Name + ".[" + name + "]");
		}
		return null;
	}

	public static object GetField(string typeName, object instance, string name)
	{
		return GetField(GetType(typeName), instance, name);
	}

	public static object GetField(Type type, object instance, string name)
	{
		FieldInfo field = type.GetField(name, GetFlags(instance));
		if (field != null)
		{
			return field.GetValue(instance);
		}
		LogError("RearViewMirror:GetField Can't find " + ((instance == null) ? "STATIC" : "INSTANCE") + " FIELD " + type.Name + ".[" + name + "]");
		return null;
	}

	public static object GetFieldWithAttribute<T>(Type type, object instance, Func<T, bool> predicate, out bool Successful)
	{
		FieldInfo[] fields = type.GetFields(GetFlags(instance));
		foreach (FieldInfo fieldInfo in fields)
		{
			object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(T), inherit: false);
			if (customAttributes.Length == 0)
			{
				continue;
			}
			object[] array = customAttributes;
			for (int j = 0; j < array.Length; j++)
			{
				T arg = (T)array[j];
				if (predicate(arg))
				{
					Successful = true;
					return fieldInfo.GetValue(instance);
				}
			}
		}
		Successful = false;
		return null;
	}

	public static object[] GetArray(Type type, object instance, string name)
	{
		return (object[])GetField(type, instance, name);
	}

	public static object[] GetArray(string typeName, object instance, string name)
	{
		return (object[])GetField(GetType(typeName), instance, name);
	}

	public static int GetInt(Type type, object instance, string name)
	{
		return (int)GetField(type, instance, name);
	}

	public static int GetInt(string typeName, object instance, string name)
	{
		return (int)GetField(GetType(typeName), instance, name);
	}

	public static float GetFloat(Type type, object instance, string name)
	{
		return (float)GetField(type, instance, name);
	}

	public static float GetFloat(string typeName, object instance, string name)
	{
		return (float)GetField(GetType(typeName), instance, name);
	}

	public static string GetString(Type type, object instance, string name)
	{
		return (string)GetField(type, instance, name);
	}

	public static string GetString(string typeName, object instance, string name)
	{
		return (string)GetField(GetType(typeName), instance, name);
	}

	public static bool GetBool(Type type, object instance, string name)
	{
		return (bool)GetField(type, instance, name);
	}

	public static bool GetBool(string typeName, object instance, string name)
	{
		return (bool)GetField(GetType(typeName), instance, name);
	}

	public static void SetFieldAtIndex(string typeName, object instance, string name, object value, int index)
	{
		SetFieldAtIndex(GetType(typeName), instance, name, value, index);
	}

	public static void SetFieldAtIndex(Type type, object instance, string name, object value, int index)
	{
		LogError("RearViewMirror:SetFieldAtIndex : Dynamic typing not supported. Aborting.");
	}

	public static void SetField(string typeName, object instance, string name, object value)
	{
		SetField(GetType(typeName), instance, name, value);
	}

	public static void SetField(Type type, object instance, string name, object value)
	{
		FieldInfo field = type.GetField(name, GetFlags(instance));
		if (field != null)
		{
			field.SetValue(instance, value);
			return;
		}
		LogError("RearViewMirror:SetField Can't find " + ((instance == null) ? "STATIC" : "INSTANCE") + " FIELD " + type.Name + ".[" + name + "]");
	}

	public static void SetFieldWithAttribute<T>(Type type, object instance, Func<T, bool> predicate, out bool Successful, object value)
	{
		FieldInfo[] fields = type.GetFields(GetFlags(instance));
		foreach (FieldInfo fieldInfo in fields)
		{
			object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(T), inherit: false);
			if (customAttributes.Length == 0)
			{
				continue;
			}
			object[] array = customAttributes;
			for (int j = 0; j < array.Length; j++)
			{
				T arg = (T)array[j];
				if (predicate(arg))
				{
					Successful = true;
					fieldInfo.SetValue(instance, Convert.ChangeType(value, fieldInfo.FieldType));
					return;
				}
			}
		}
		Successful = false;
	}

	public static object GetProperty(string typeName, object instance, string name)
	{
		return GetProperty(GetType(typeName), instance, name);
	}

	public static object GetProperty(Type type, object instance, string name)
	{
		PropertyInfo property = type.GetProperty(name, GetFlags(instance));
		if (property != null)
		{
			if (property.CanRead)
			{
				return property.GetValue(instance, null);
			}
			LogError("RearViewMirror:GetProperty Can't read " + ((instance == null) ? "STATIC" : "INSTANCE") + " PROPERTY " + type.Name + ".[" + name + "]");
		}
		else
		{
			LogError("RearViewMirror:GetProperty Can't find " + ((instance == null) ? "STATIC" : "INSTANCE") + " PROPERTY " + type.Name + ".[" + name + "]");
		}
		return null;
	}

	public static void SetProperty(string typeName, object instance, string name, object value)
	{
		SetProperty(GetType(typeName), instance, name, value);
	}

	public static void SetProperty(Type type, object instance, string name, object value)
	{
		PropertyInfo property = type.GetProperty(name, GetFlags(instance));
		if (property != null)
		{
			if (property.CanWrite)
			{
				property.SetValue(instance, value, null);
				return;
			}
			LogError("RearViewMirror:SetProperty Can't write " + ((instance == null) ? "STATIC" : "INSTANCE") + " PROPERTY " + type.Name + ".[" + name + "]");
		}
		else
		{
			LogError("RearViewMirror:SetProperty Can't find " + ((instance == null) ? "STATIC" : "INSTANCE") + " PROPERTY " + type.Name + ".[" + name + "]");
		}
	}
}
