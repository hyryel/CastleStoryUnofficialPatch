using System.Collections.Generic;
using System.Linq;

public class SparseArray<T> where T : class
{
	protected Marqueur check;

	protected Dictionary<int, T> elems;

	protected int countChecked;

	public int Length => check.Length;

	public IEnumerable<int> EmptyIndices => check.Libres();

	public IEnumerable<int> NonEmptyIndices => check.Presents();

	public IEnumerable<KeyValuePair<int, T>> AllocatedPairs
	{
		get
		{
			IEnumerable<KeyValuePair<int, T>> enumerable = elems;
			return enumerable ?? Enumerable.Empty<KeyValuePair<int, T>>();
		}
	}

	public IEnumerable<int> AllocatedIndices
	{
		get
		{
			IEnumerable<int> result;
			if (elems == null)
			{
				result = Enumerable.Empty<int>();
			}
			else
			{
				IEnumerable<int> keys = elems.Keys;
				result = keys;
			}
			return result;
		}
	}

	public IEnumerable<T> AllocatedElements
	{
		get
		{
			IEnumerable<T> result;
			if (elems == null)
			{
				result = Enumerable.Empty<T>();
			}
			else
			{
				IEnumerable<T> values = elems.Values;
				result = values;
			}
			return result;
		}
	}

	public int EmptyCount => Length - countChecked;

	public int NonEmptyCount => countChecked;

	public int AllocatedCount => (elems != null) ? elems.Count : 0;

	public bool Empty => countChecked == 0;

	public bool Full => countChecked == Length;

	public bool Partial => !Empty && !Full;

	public SparseArray(int nElems)
	{
		check = new Marqueur(nElems);
	}

	public void SetValue(int i, T value)
	{
		if (check.MarquerSiLibre(i))
		{
			countChecked++;
		}
		if (elems == null)
		{
			elems = new Dictionary<int, T>();
		}
		elems[i] = value;
	}

	public void SetToDefault(int i)
	{
		if (check.MarquerSiLibre(i))
		{
			countChecked++;
		}
		if (elems != null)
		{
			elems.Remove(i);
		}
	}

	public void ClearValue(int i)
	{
		if (check.EnleverSiPresent(i))
		{
			countChecked--;
		}
		if (elems != null)
		{
			elems.Remove(i);
		}
	}

	public T GetAllocatedValue(int i)
	{
		return elems[i];
	}

	public T GetValueOrDefault(int i)
	{
		return (elems == null) ? null : elems.GetValueOrDefault(i);
	}

	public T GetValueOrDefault(int i, T defaultValue)
	{
		return (elems == null) ? defaultValue : elems.GetValueOrDefault(i, defaultValue);
	}

	public void Clear()
	{
		check.Clear();
		elems = null;
		countChecked = 0;
	}

	public bool IsEmpty(int i)
	{
		return check.Libre(i);
	}

	public bool IsDefault(int i)
	{
		return check.Present(i);
	}

	public bool IsAllocated(int i)
	{
		return elems != null && elems.ContainsKey(i);
	}
}
