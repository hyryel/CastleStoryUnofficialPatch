using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MultiSet<T> : IEnumerable<T>, IEnumerable
{
	private Dictionary<T, int> k2n;

	private int count;

	public int Count => count;

	public IEnumerable<T> Keys => k2n.Keys;

	public IEnumerable<int> Counts => k2n.Values;

	public MultiSet()
	{
		k2n = new Dictionary<T, int>();
		count = 0;
	}

	public int Add(T x)
	{
		int value = 0;
		k2n.TryGetValue(x, out value);
		value = (k2n[x] = value + 1);
		count++;
		return value;
	}

	public int Add(MultiSet<T> other)
	{
		int num = 0;
		foreach (KeyValuePair<T, int> item in other.k2n)
		{
			T key = item.Key;
			k2n.TryGetValue(key, out var value);
			value += item.Value;
			k2n[key] = value;
			num += value;
		}
		count += other.count;
		return num;
	}

	public int Remove(T x)
	{
		if (!k2n.TryGetValue(x, out var value))
		{
			return -1;
		}
		count--;
		if (value <= 1)
		{
			k2n.Remove(x);
			return 0;
		}
		return k2n[x] = value - 1;
	}

	public bool Contains(T x)
	{
		return k2n.ContainsKey(x);
	}

	public int CountKey(T x)
	{
		int value = 0;
		k2n.TryGetValue(x, out value);
		return value;
	}

	public T MostFrequentKey()
	{
		return Donnees.ArgMax(k2n, (KeyValuePair<T, int> k_n) => k_n.Value).Key;
	}

	public int HighestCount()
	{
		return k2n.Values.Max();
	}

	public IEnumerator<T> GetEnumerator()
	{
		foreach (KeyValuePair<T, int> key_n in k2n)
		{
			T key = key_n.Key;
			int n = key_n.Value;
			for (int i = 0; i < n; i++)
			{
				yield return key;
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return k2n.Keys.GetEnumerator();
	}
}
