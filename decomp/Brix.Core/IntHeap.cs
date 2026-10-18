using System.Collections.Generic;

public class IntHeap<T>
{
	private SortedList<int, T> k2v;

	private SortedList<T, int> v2k;

	public int Count => k2v.Count;

	public IntHeap()
	{
		k2v = new SortedList<int, T>();
		v2k = new SortedList<T, int>();
	}

	public void Push(int k, T x)
	{
		RemoveItem(x);
		k2v.Add(k, x);
		v2k.Add(x, k);
	}

	public T Pop()
	{
		T val = k2v.Values[Count - 1];
		k2v.RemoveAt(Count - 1);
		v2k.Remove(val);
		return val;
	}

	public int PeekMaxKey()
	{
		return k2v.Keys[Count - 1];
	}

	public T PeekMaxItem()
	{
		return k2v.Values[Count - 1];
	}

	public bool Contains(T x)
	{
		return v2k.ContainsKey(x);
	}

	public int KeyOf(T x)
	{
		int num = v2k.IndexOfKey(x);
		return (num >= 0) ? v2k.Values[num] : 0;
	}

	public T ItemOf(int k)
	{
		int num = k2v.IndexOfKey(k);
		if (num < 0)
		{
			throw null;
		}
		return k2v.Values[num];
	}

	public bool RemoveKey(int k)
	{
		int num = k2v.IndexOfKey(k);
		if (num < 0)
		{
			return false;
		}
		v2k.Remove(k2v.Values[num]);
		k2v.RemoveAt(num);
		return true;
	}

	public bool RemoveItem(T x)
	{
		int num = v2k.IndexOfKey(x);
		if (num < 0)
		{
			return false;
		}
		int key = v2k.Values[num];
		v2k.RemoveAt(num);
		num = k2v.IndexOfKey(key);
		k2v.RemoveAt(num);
		return false;
	}
}
