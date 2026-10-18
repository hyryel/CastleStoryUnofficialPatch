#define DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class Index<T>
{
	private Classeur<T> a;

	private Dictionary<T, int> dico;

	private Dictionary<T, T> extra;

	public T this[int i]
	{
		get
		{
			return a[i];
		}
		set
		{
			dico.Remove(value);
			a[i] = value;
			dico.Add(value, i);
		}
	}

	public int this[T t]
	{
		get
		{
			if (dico.TryGetValue(t, out var value))
			{
				return value;
			}
			value = a.Count;
			dico.Add(t, value);
			a.Add(t);
			return value;
		}
	}

	public int Count => a.Count;

	public IEnumerable<int> Keys => Enumerable.Range(0, a.Count);

	public IEnumerable<T> Values => a;

	public Index()
	{
		a = new Classeur<T>();
		dico = new Dictionary<T, int>();
	}

	public Index(IEnumerable<T> values)
	{
		a = new Classeur<T>();
		dico = new Dictionary<T, int>();
		foreach (T value in values)
		{
			Add(value);
		}
	}

	public static implicit operator T[](Index<T> index)
	{
		return index.a.ToBuiltin();
	}

	public static explicit operator Index<T>(T[] a)
	{
		return new Index<T>(a);
	}

	public Index<T> Modulo<S>(Func<T, S> f)
	{
		Index<T> index = new Index<T>();
		Dictionary<S, T> dictionary = new Dictionary<S, T>();
		foreach (T item in a)
		{
			S key = f(item);
			if (!dictionary.TryGetValue(key, out var value))
			{
				dictionary.Add(key, item);
				index.Add(item);
			}
			else
			{
				index.AddExtra(value, item);
			}
		}
		return index;
	}

	public int GetKey(T wanted)
	{
		if (dico.TryGetValue(wanted, out var value))
		{
			return value;
		}
		if (extra != null && extra.TryGetValue(wanted, out var value2))
		{
			return dico[value2];
		}
		throw new KeyNotFoundException(string.Concat("Key=", wanted, " (i=", value, ")"));
	}

	public T GetValue(int i)
	{
		return a[i];
	}

	public int TranslateKeyFrom(Index<T> other, int i)
	{
		return GetKey(other.a[i]);
	}

	public IEnumerable<int> TranslateKeysFrom(Index<T> other, IEnumerable<int> indices)
	{
		return indices.Select((int i) => TranslateKeyFrom(other, i));
	}

	public void AddExtra(T canonicalRepresentation, T alternateRepresentation)
	{
		if (extra == null)
		{
			extra = new Dictionary<T, T>();
		}
		extra[alternateRepresentation] = canonicalRepresentation;
	}

	public T Match(T t)
	{
		if (dico.TryGetValue(t, out var value))
		{
			try
			{
				return a[value];
			}
			catch (Exception o)
			{
				Donnees.LogWarning(o);
			}
		}
		return default(T);
	}

	public void Sort()
	{
		a.Sort();
		for (int i = 0; i < Count; i++)
		{
			dico[a[i]] = i;
		}
	}

	public int Add(T t)
	{
		try
		{
			int count = a.Count;
			dico.Add(t, count);
			a.Add(t);
			return count;
		}
		catch (Exception)
		{
			return -1;
		}
	}

	public int Remove(T t)
	{
		try
		{
			int num = dico[t];
			dico.Remove(t);
			a[num] = a.Pop();
			dico[a[num]] = num;
			return num;
		}
		catch (Exception)
		{
			return -1;
		}
	}

	public T RemoveAt(int i)
	{
		T val = a[i];
		dico.Remove(val);
		a[i] = a.Pop();
		dico[a[i]] = i;
		return val;
	}

	public bool Update(T t)
	{
		int num = Remove(t);
		Add(t);
		return num >= 0;
	}

	public bool Contains(T t)
	{
		return dico.ContainsKey(t);
	}

	public T[] ToBuiltin()
	{
		return a.ToBuiltin();
	}

	public T[] ToBuiltin(Type someT)
	{
		Debug.Assert(someT is T, "Index<" + typeof(T).Name + ">.ToBuiltin(" + someT.Name + ") ?!");
		return ToBuiltin();
	}

	public override string ToString()
	{
		return "Index<" + typeof(T).Name + ">{ Keys=[" + Donnees.PrintAll(Keys) + "], Values=[" + Donnees.PrintAll(Values) + "] }";
	}

	public void Seal()
	{
		if (dico != null)
		{
			a.ToBuiltin();
			dico = null;
		}
	}

	public void UnSeal()
	{
		if (dico == null)
		{
			dico = new Dictionary<T, int>();
			for (int i = 0; i < Count; i++)
			{
				dico.Add(a[i], i);
			}
		}
	}
}
