using System;
using System.Collections.Generic;

namespace Brix.Game.Components;

public class ListeSelection : HashSet<object>
{
	public Type firstType
	{
		get
		{
			if (premier != null)
			{
				return premier.GetType();
			}
			return null;
		}
	}

	public object premier
	{
		get
		{
			using (Enumerator enumerator = GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					return enumerator.Current;
				}
			}
			return null;
		}
	}

	public bool FirstType(Type t)
	{
		return t == firstType;
	}

	public T[] GetBuiltIn<T>()
	{
		T[] array = new T[base.Count];
		int num = 0;
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			object current = enumerator.Current;
			array[num] = (T)current;
			num++;
		}
		return array;
	}
}
