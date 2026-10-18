using System;
using System.Collections;
using System.Collections.Generic;

namespace Brix.Utils.Collections;

public class RemovableListEnumerator<T> : IEnumerator<T>, IDisposable, IEnumerator
{
	public List<T> List;

	private int i;

	private int j;

	public T Current => List[i];

	object IEnumerator.Current => Current;

	public RemovableListEnumerator(List<T> list)
	{
		List = list;
		i = -1;
		j = 0;
	}

	public bool MoveNext()
	{
		i++;
		if (j != 0)
		{
			List[i - j] = List[i];
		}
		return i < List.Count;
	}

	public void Reset()
	{
		if (j != 0)
		{
			List.RemoveRange(i - j, j);
			j = 0;
		}
		i = -1;
	}

	public void Dispose()
	{
		Reset();
	}

	public void RemoveCurrentNode()
	{
		j++;
	}
}
