using System;
using System.Collections.Generic;
using System.Reflection;
using Smooth.Algebraics;

namespace Brix.Utils;

public static class TypeResolver
{
	private static Dictionary<Tuple<string, string>, Type> cachedTypes = new Dictionary<Tuple<string, string>, Type>();

	private static Dictionary<string, Type> hashToTypes = new Dictionary<string, Type>();

	private static Dictionary<Type, string> cachedHashTypes = new Dictionary<Type, string>();

	public static Type GetType(string assembly, string fullTypeName)
	{
		Tuple<string, string> key = Tuple.Create(assembly, fullTypeName);
		if (!cachedTypes.TryGetValue(key, out var value))
		{
			value = (cachedTypes[key] = RearViewMirror.GetType(fullTypeName, assembly));
		}
		return value;
	}

	public static Type GetType(string hash)
	{
		if (!hashToTypes.TryGetValue(hash, out var value))
		{
			string[] array = hash.Split(';');
			value = GetType(array[0], array[1]);
			if (value != null)
			{
				hashToTypes[hash] = value;
			}
		}
		return value;
	}

	public static string GetHash(Type tp)
	{
		if (!cachedHashTypes.TryGetValue(tp, out var value))
		{
			value = (cachedHashTypes[tp] = $"{tp.Assembly.GetName().Name};{tp.FullName}");
		}
		return value;
	}

	public static bool IsType(Type tp, Type assignableType)
	{
		return assignableType.IsAssignableFrom(tp);
	}

	public static bool IsType(Type tp, params Type[] assignableTypes)
	{
		foreach (Type type in assignableTypes)
		{
			if (type.IsAssignableFrom(tp))
			{
				return true;
			}
		}
		return false;
	}

	public static IEnumerable<Type> GetTypes(Func<Type, bool> predicate)
	{
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assemb in assemblies)
		{
			Type[] types = assemb.GetTypes();
			foreach (Type tp in types)
			{
				if (predicate(tp))
				{
					yield return tp;
				}
			}
		}
	}
}
