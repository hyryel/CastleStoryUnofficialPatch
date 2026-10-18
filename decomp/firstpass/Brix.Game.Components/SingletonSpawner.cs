using System;
using System.Collections.Generic;
using Brix.Common;

namespace Brix.Game.Components;

public static class SingletonSpawner
{
	private static Dictionary<Type, object> singletons = new Dictionary<Type, object>();

	public static T Get<T>()
	{
		return (T)Get(typeof(T));
	}

	public static object Get(Type type)
	{
		if (!singletons.TryGetValue(type, out var value))
		{
			value = (singletons[type] = type.MakeInstance());
		}
		return value;
	}
}
