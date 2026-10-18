using System.Collections.Generic;
using System.Linq;

namespace Brix.Pathfinding.Collections;

public class PriorityQueue<TKey, TValue>
{
	private SortedDictionary<TKey, Stack<TValue>> dico = new SortedDictionary<TKey, Stack<TValue>>();

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

	public TKey Priority
	{
		get
		{
			if (TryGetFirst(out var kv))
			{
				return kv.Key;
			}
			return default(TKey);
		}
	}

	public IEnumerable<TKey> Keys => from kv in dico
		from val in kv.Value
		select kv.Key;

	public IEnumerable<TValue> Values => from kv in dico
		from val in kv.Value
		select val;

	public void Enqueue(TKey key, TValue val)
	{
		if (dico.TryGetValue(key, out var value))
		{
			value.Push(val);
			return;
		}
		value = new Stack<TValue>();
		value.Push(val);
		dico.Add(key, value);
	}

	public TValue Dequeue()
	{
		if (TryGetFirst(out var kv))
		{
			Stack<TValue> value = kv.Value;
			TValue result = value.Pop();
			if (value.Count == 0)
			{
				dico.Remove(kv.Key);
			}
			return result;
		}
		return default(TValue);
	}

	public void Clear()
	{
		dico.Clear();
	}

	private bool TryGetFirst(out KeyValuePair<TKey, Stack<TValue>> kv)
	{
		SortedDictionary<TKey, Stack<TValue>>.Enumerator enumerator = dico.GetEnumerator();
		if (enumerator.MoveNext())
		{
			kv = enumerator.Current;
			return true;
		}
		kv = default(KeyValuePair<TKey, Stack<TValue>>);
		return false;
	}
}
