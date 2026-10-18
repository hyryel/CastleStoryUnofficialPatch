using System.Collections.Generic;

public class PriorityQueue<TKey, TValue>
{
	private SortedDictionary<TKey, Classeur<TValue>> dico;

	public bool Empty => dico.Count == 0;

	public int Count
	{
		get
		{
			int num = 0;
			foreach (TKey key in dico.Keys)
			{
				num += dico[key].Count;
			}
			return num;
		}
	}

	public TKey priority
	{
		get
		{
			if (TryGetFirst(out var key))
			{
				return key;
			}
			return default(TKey);
		}
	}

	public IEnumerator<TKey> Keys
	{
		get
		{
			foreach (TKey key in dico.Keys)
			{
				Classeur<TValue> vals = dico[key];
				for (int i = 0; i < vals.Count; i++)
				{
					yield return key;
				}
			}
		}
	}

	public IEnumerator<TValue> Values
	{
		get
		{
			foreach (TKey key in dico.Keys)
			{
				Classeur<TValue> vals = dico[key];
				for (int i = 0; i < vals.Count; i++)
				{
					yield return vals[i];
				}
			}
		}
	}

	public PriorityQueue()
	{
		dico = new SortedDictionary<TKey, Classeur<TValue>>();
	}

	public void Push(TKey key, TValue val)
	{
		if (dico.TryGetValue(key, out var value))
		{
			value.Add(val);
			return;
		}
		value = new Classeur<TValue>();
		value.Add(val);
		dico.Add(key, value);
	}

	private bool TryGetFirst(out TKey key)
	{
		IEnumerator<TKey> enumerator = dico.Keys.GetEnumerator();
		if (enumerator.MoveNext())
		{
			key = enumerator.Current;
			return true;
		}
		key = default(TKey);
		return false;
	}

	public TValue Pop()
	{
		if (TryGetFirst(out var key))
		{
			Classeur<TValue> classeur = dico[key];
			TValue result = classeur.Pop();
			if (classeur.Count == 0)
			{
				dico.Remove(key);
			}
			return result;
		}
		return default(TValue);
	}

	public void Clear()
	{
		dico.Clear();
	}
}
