using System.Collections;
using System.Collections.Generic;

namespace Brix.Utils.Collections;

public class UnorderedBag<T> : ICollection<T>, IEnumerable<T>, IEnumerable
{
	public List<T> List = new List<T>();

	public int Count => List.Count;

	public bool IsReadOnly => false;

	public UnorderedListEnumerator<T> GetEnumerator()
	{
		return new UnorderedListEnumerator<T>(List);
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Add(T item)
	{
		List.Add(item);
	}

	public void Clear()
	{
		List.Clear();
	}

	public bool Contains(T item)
	{
		return List.Contains(item);
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		List.CopyTo(array, arrayIndex);
	}

	public bool Remove(T item)
	{
		return List.Remove(item);
	}
}
