using System;
using System.Collections;
using System.Collections.Generic;

namespace Brix.Utils.Collections;

public class UnorderedListEnumerator<T> : IEnumerator<T>, IDisposable, IEnumerator
{
	public List<T> List;

	private int i;

	public T Current => List[i];

	object IEnumerator.Current => Current;

	public UnorderedListEnumerator(List<T> list)
	{
		List = list;
		i = -1;
	}

	public bool MoveNext()
	{
		return ++i < List.Count;
	}

	public void Reset()
	{
		i = -1;
	}

	public void Dispose()
	{
	}

	public void RemoveCurrentNode()
	{
		List[i] = List.Pop();
		i--;
	}
}
