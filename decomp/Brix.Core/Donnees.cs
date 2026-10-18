using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class Donnees
{
	public delegate int FKey<T>(T val);

	private static Dictionary<string, Type> name2type = new Dictionary<string, Type>();

	private static Dictionary<string, object> staticVars = new Dictionary<string, object>();

	private static Dictionary<string, object> memberVars = new Dictionary<string, object>();

	public static bool Augment(ref int x, int m)
	{
		if (x < m)
		{
			x = m;
			return true;
		}
		return false;
	}

	public static bool Augment(ref float x, float m)
	{
		if (x < m)
		{
			x = m;
			return true;
		}
		return false;
	}

	public static bool Augment(ref double x, double m)
	{
		if (x < m)
		{
			x = m;
			return true;
		}
		return false;
	}

	public static bool Diminish(ref int x, int m)
	{
		if (x > m)
		{
			x = m;
			return true;
		}
		return false;
	}

	public static bool Diminish(ref float x, float m)
	{
		if (x > m)
		{
			x = m;
			return true;
		}
		return false;
	}

	public static bool Diminish(ref double x, double m)
	{
		if (x > m)
		{
			x = m;
			return true;
		}
		return false;
	}

	public static bool AssignZ<T>(ref T x, T y)
	{
		if (x == null)
		{
			x = y;
			return true;
		}
		return false;
	}

	public static T Or<T>(T x, T y)
	{
		return (x != null) ? x : y;
	}

	public static T Or<T>(T x, Func<T> f)
	{
		return (x != null) ? x : f();
	}

	public static T Assign<T>(ref T x, T y)
	{
		x = y;
		return x;
	}

	public static T Lazy<T>(ref T x, Func<T> f)
	{
		if (x == null)
		{
			x = f();
		}
		return x;
	}

	public static void Swap<T>(ref T x, ref T y)
	{
		T val = x;
		x = y;
		y = val;
	}

	public static T OrNew<T>(ref T x) where T : new()
	{
		if (x == null)
		{
			x = new T();
			return x;
		}
		return x;
	}

	public static IEnumerable<Type> GetTypes(string className)
	{
		return from assembly in AppDomain.CurrentDomain.GetAssemblies()
			from type in assembly.GetTypes()
			where type.Name == className
			select type;
	}

	public static IEnumerable<Type> GetTypes(string className, Type baseType)
	{
		return from type in GetTypes(className)
			where baseType.IsAssignableFrom(type)
			select type;
	}

	public static Type GetType(string className)
	{
		return name2type.NewKey(className, (string s) => GetTypes(s).FirstOrDefault());
	}

	public static Type GetType(string className, Type baseType)
	{
		return GetTypes(className, baseType).FirstOrDefault();
	}

	public static Type GetType(string className, string namesp)
	{
		string fullname = namesp + "." + className;
		IEnumerable<Type> source = from t in GetTypes(className)
			where t.FullName == fullname
			select t;
		return source.FirstOrDefault();
	}

	public static void Log(object o)
	{
		UnityEngine.Debug.Log(o);
	}

	public static void LogWarning(object o)
	{
		UnityEngine.Debug.LogWarning(o);
	}

	public static void LogError(object o)
	{
		UnityEngine.Debug.LogError(o);
	}

	public static string IntToHex1(int i)
	{
		return ((char)((i > 9) ? (65 + (i - 10)) : (48 + i))).ToString() ?? "";
	}

	public static string IntToHex2(int i)
	{
		int i2 = (i >> 4) & 0xF;
		int i3 = i & 0xF;
		return IntToHex1(i2) + IntToHex1(i3);
	}

	public static int Hex1ToInt(char h)
	{
		return (h >= 'a') ? (h - 97 + 10) : ((h >= 'A') ? (h - 65 + 10) : (h - 48));
	}

	public static int Hex2ToInt(char h1, char h2)
	{
		int num = Hex1ToInt(h1);
		int num2 = Hex1ToInt(h2);
		return (num << 4) | num2;
	}

	public static string StringToText(string s)
	{
		string text = "";
		for (int i = 0; i < s.Length; i++)
		{
			char c = s[i];
			text = (((c < '0' || c > '9') && (c < 'a' || c > 'z') && (c < 'A' || c > 'Z')) ? (text + "%" + IntToHex2(c)) : (text + c));
		}
		return text;
	}

	public static string TextToString(string text)
	{
		string text2 = "";
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] == '%')
			{
				int num = Hex2ToInt(text[i + 1], text[i + 2]);
				text2 += (char)num;
				i += 2;
			}
			else
			{
				text2 += text[i];
			}
		}
		return text2;
	}

	public static long LongFrom4Short(short a, short b, short c, short d)
	{
		return (long)((ushort)a | ((ulong)(ushort)b << 16) | ((ulong)(ushort)c << 32) | ((ulong)(ushort)d << 48));
	}

	public static void LongTo4Short(long l, out short a, out short b, out short c, out short d)
	{
		a = (short)(ushort)l;
		b = (short)(ushort)(l >> 16);
		c = (short)(ushort)(l >> 32);
		d = (short)(ushort)(l >> 48);
	}

	public static void Sort<T>(T[] a, FKey<T> key)
	{
		int[] array = new int[a.Length];
		for (int i = 0; i < a.Length; i++)
		{
			array[i] = key(a[i]);
		}
		Array.Sort(array, a);
	}

	public static IEnumerator Union(IEnumerable a, IEnumerable b)
	{
		if (a != null)
		{
			foreach (object item in a)
			{
				yield return item;
			}
		}
		if (b == null)
		{
			yield break;
		}
		foreach (object item2 in b)
		{
			yield return item2;
		}
	}

	public static bool Vide(IEnumerable a)
	{
		return a == null || !a.GetEnumerator().MoveNext();
	}

	public static bool Equals(IEnumerable i, IEnumerable j)
	{
		if (i == null)
		{
			return j == null;
		}
		if (j == null)
		{
			return false;
		}
		IEnumerator enumerator = i.GetEnumerator();
		IEnumerator enumerator2 = j.GetEnumerator();
		do
		{
			if (!enumerator.MoveNext())
			{
				return !enumerator2.MoveNext();
			}
			if (!enumerator2.MoveNext())
			{
				return false;
			}
		}
		while (enumerator.Current == enumerator2.Current);
		return false;
	}

	public static IEnumerable<T> AsEnum<T>(IEnumerable i)
	{
		foreach (object o in i)
		{
			yield return (T)o;
		}
	}

	public static IEnumerable<B> Map<A, B>(IEnumerable<A> i, Func<A, B> f)
	{
		foreach (A a in i)
		{
			yield return f(a);
		}
	}

	public static IEnumerable<KeyValuePair<A, C>> MapKeyValue<A, B, C>(IEnumerable<KeyValuePair<A, B>> i, Func<B, C> f)
	{
		foreach (KeyValuePair<A, B> kv in i)
		{
			yield return new KeyValuePair<A, C>(kv.Key, f(kv.Value));
		}
	}

	public static K ArgMax<K, T>(IEnumerable<K> keys, Func<K, T> fValue) where T : IComparable<T>
	{
		IEnumerator<K> enumerator = keys.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			throw new KeyNotFoundException();
		}
		K current = enumerator.Current;
		T other = fValue(enumerator.Current);
		while (enumerator.MoveNext())
		{
			T val = fValue(enumerator.Current);
			if (val.CompareTo(other) > 0)
			{
				current = enumerator.Current;
				other = val;
			}
		}
		return current;
	}

	public static object StaticMessage(string className, string methodName, object[] args)
	{
		Type type = Type.GetType(className);
		MethodInfo method = type.GetMethod(methodName, BindingFlags.Static);
		return method.Invoke(null, args);
	}

	public static object StaticMessage(string className, string methodName, object arg)
	{
		object[] args = new object[1] { arg };
		return StaticMessage(className, methodName, args);
	}

	public static object StaticMessage(string className, string methodName)
	{
		return StaticMessage(className, methodName, null);
	}

	public static string PrintAll(IEnumerable list)
	{
		return PrintAll(list, ", ");
	}

	public static string PrintAll(IEnumerable list, string delimiter)
	{
		string text = "";
		foreach (object item in list)
		{
			text = text + ((text == "") ? "" : delimiter) + item.ToString();
		}
		return text;
	}

	public static IEnumerable Clone(IEnumerable a)
	{
		Classeur<object> classeur = new Classeur<object>();
		if (a != null)
		{
			foreach (object item in a)
			{
				classeur.Add(item);
			}
		}
		return classeur;
	}

	public static IEnumerable<T> Clone<T>(IEnumerable<T> a)
	{
		Classeur<T> classeur = new Classeur<T>();
		if (a != null)
		{
			foreach (T item in a)
			{
				classeur.Add(item);
			}
		}
		return classeur;
	}

	public static Dictionary<TKey, TValue> Dico<TKey, TValue>(Hashtable h)
	{
		Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();
		foreach (object key in h.Keys)
		{
			try
			{
				dictionary.Add((TKey)key, (TValue)h[key]);
			}
			catch (Exception o)
			{
				LogWarning(o);
			}
		}
		return dictionary;
	}

	public static Dictionary<TKey, TValue> Dico<TKey, TValue>(IEnumerable<TKey> keys, Func<TKey, TValue> map)
	{
		Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();
		foreach (TKey key in keys)
		{
			dictionary[key] = map(key);
		}
		return dictionary;
	}

	public static TValue NewDicoKey<TKey, TValue>(Dictionary<TKey, TValue> dico, TKey key) where TValue : new()
	{
		if (!dico.TryGetValue(key, out var value))
		{
			value = new TValue();
			dico.Add(key, value);
		}
		return value;
	}

	public static TValue NewDicoKey<TKey, TValue>(Dictionary<TKey, TValue> dico, TKey key, Func<TValue> makeEntry)
	{
		return NewDicoKey(dico, key, (TKey k) => makeEntry());
	}

	public static TValue NewDicoKey<TKey, TValue>(Dictionary<TKey, TValue> dico, TKey key, Func<TKey, TValue> makeEntry)
	{
		if (!dico.TryGetValue(key, out var value))
		{
			value = makeEntry(key);
			dico.Add(key, value);
		}
		return value;
	}

	public static TValue NewKey<TKey, TValue>(this Dictionary<TKey, TValue> dico, TKey key) where TValue : new()
	{
		return NewDicoKey(dico, key);
	}

	public static TValue NewKey<TKey, TValue>(this Dictionary<TKey, TValue> dico, TKey key, Func<TValue> makeEntry)
	{
		return NewDicoKey(dico, key, makeEntry);
	}

	public static TValue NewKey<TKey, TValue>(this Dictionary<TKey, TValue> dico, TKey key, Func<TKey, TValue> makeEntry)
	{
		return NewDicoKey(dico, key, makeEntry);
	}

	public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dico, TKey key)
	{
		TValue value;
		return dico.TryGetValue(key, out value) ? value : default(TValue);
	}

	public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dico, TKey key, TValue defaultValue)
	{
		TValue value;
		return dico.TryGetValue(key, out value) ? value : defaultValue;
	}

	public static TValue ModifyValue<TKey, TValue>(this Dictionary<TKey, TValue> dico, TKey key, Func<TValue, TValue> valueFunc)
	{
		TValue valueOrDefault = dico.GetValueOrDefault(key);
		return dico[key] = valueFunc(valueOrDefault);
	}

	public static IEnumerator<T> Apply<T>(this IEnumerator<T> enumerator, Action<T> action)
	{
		while (enumerator.MoveNext())
		{
			T t = enumerator.Current;
			action(t);
			yield return t;
		}
	}

	public static IEnumerable<T> Apply<T>(this IEnumerable<T> enumerator, Action<T> action)
	{
		foreach (T t in enumerator)
		{
			action(t);
			yield return t;
		}
	}

	private static void Noop<T>(T t)
	{
	}

	public static void ApplyNow<T>(this IEnumerator<T> enumerator, Action<T> action = null)
	{
		action = action ?? new Action<T>(Noop);
		while (enumerator.Apply(action).MoveNext())
		{
		}
	}

	public static void ApplyNow<T>(this IEnumerable<T> enumerator, Action<T> action = null)
	{
		action = action ?? new Action<T>(Noop);
		enumerator.Apply(action).Count();
	}

	public static T Pop<T>(this List<T> list)
	{
		int index = list.Count - 1;
		T result = list[index];
		list.RemoveAt(index);
		return result;
	}

	public static T[] GetInstanceArray<T>(int count) where T : new()
	{
		T[] array = new T[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = new T();
		}
		return array;
	}

	public static T[] GetInstanceArray<T>(int count, out T[] res) where T : new()
	{
		res = new T[count];
		for (int i = 0; i < count; i++)
		{
			res[i] = new T();
		}
		return res;
	}

	public static Var<T> StaticVar<T>(T init, int depth)
	{
		StackTrace stackTrace = new StackTrace(fNeedFileInfo: true);
		StackFrame frame = stackTrace.GetFrame(depth + 1);
		string text = frame.GetFileName() + ":" + frame.GetFileLineNumber() + ":" + frame.GetFileColumnNumber();
		if (staticVars.TryGetValue(text, out var value))
		{
			return value as Var<T>;
		}
		Log("Creating var at " + text);
		Var<T> var = new Var<T>(init);
		staticVars.Add(text, var);
		return var;
	}

	public static Var<T> StaticVar<T>()
	{
		return StaticVar(default(T), 1);
	}

	public static Var<T> MemberVar<T>(T init, object o, int depth)
	{
		StackTrace stackTrace = new StackTrace(fNeedFileInfo: true);
		StackFrame frame = stackTrace.GetFrame(depth + 1);
		string text = frame.GetFileName() + ":" + frame.GetFileLineNumber() + ":" + frame.GetFileColumnNumber();
		string key = text + ":" + o.GetHashCode();
		if (memberVars.TryGetValue(key, out var value))
		{
			return value as Var<T>;
		}
		Log(string.Concat("Creating var for ", o, " at ", text));
		Var<T> var = new Var<T>(init);
		memberVars.Add(key, var);
		return var;
	}
}
