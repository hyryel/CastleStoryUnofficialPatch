using System;
using System.Collections.Generic;
using System.Linq;
using Brix.Utils.Collections;
using Smooth.Slinq;

namespace Brix.Utils;

internal static class ArrayExtensions
{
	public static T[] TakeSome<T>(this T[] array, int start, int nb)
	{
		T[] array2 = new T[nb];
		for (int i = start; i < start + nb; i++)
		{
			array2[i] = array[i];
		}
		return array2;
	}

	public static T[] ToArray<T, TSlinq>(this Slinq<T, TSlinq> slinq)
	{
		List<T> list = slinq.ToPooledList();
		T[] result = list.ToArray();
		list.Dispose();
		return result;
	}

	public static void AddToList<T>(this T[] array, ref List<T> list)
	{
		for (int i = 0; i < array.Length; i++)
		{
			list.Add(array[i]);
		}
	}

	public static T[] Add<T>(this Array array, T item)
	{
		Type elementType = array.GetType().GetElementType();
		if (elementType == item.GetType())
		{
			List<T> list = array.Cast<T>().ToList();
			list.Add(item);
			return list.ToArray();
		}
		throw new InvalidCastException();
	}

	public static T[] AddRange<T>(this Array array, params T[] items)
	{
		Type elementType = array.GetType().GetElementType();
		if (elementType == items.GetType().GetElementType())
		{
			Array array2 = Array.CreateInstance(elementType, array.Length + items.Length);
			Array.Copy(array, array2, array.Length);
			Array.Copy(items, 0, array2, array.Length, items.Length);
			return (T[])array2;
		}
		throw new InvalidCastException();
	}

	public static T[] InsertAtBeginning<T>(this Array array, T item)
	{
		Type elementType = array.GetType().GetElementType();
		if (elementType == item.GetType())
		{
			Array array2 = Array.CreateInstance(elementType, array.Length + 1);
			Array.Copy(array, array2, array.Length);
			array2.SetValue(item, 0);
			return (T[])array2;
		}
		throw new InvalidCastException();
	}

	public static bool IsNullOrEmpty<T>(this T[] array)
	{
		return array == null || array.Length == 0;
	}
}
