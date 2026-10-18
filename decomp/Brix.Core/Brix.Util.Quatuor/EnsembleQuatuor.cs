using System;
using System.Collections.Generic;

namespace Brix.Util.Quatuor;

public class EnsembleQuatuor : HashSet<Quatuor>
{
	public int Has(int index)
	{
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				Quatuor current = enumerator.Current;
				if (current.b == index)
				{
					return current.d;
				}
			}
		}
		return 0;
	}

	public void RemoveLinksBy(int[] cases)
	{
		List<Quatuor> list = new List<Quatuor>();
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				Quatuor current = enumerator.Current;
				if (Array.BinarySearch(cases, current.d) >= 0)
				{
					list.Add(current);
				}
			}
		}
		foreach (Quatuor item in list)
		{
			Remove(item);
		}
	}

	public void RemoveLinksBy(int index)
	{
		List<Quatuor> list = new List<Quatuor>();
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				Quatuor current = enumerator.Current;
				if (current.d == index)
				{
					list.Add(current);
				}
			}
		}
		foreach (Quatuor item in list)
		{
			Remove(item);
		}
	}
}
