using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Brix.Utils;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GenericEnumerable<T, TEnumerator> : IEnumerable<T>, IEnumerable where TEnumerator : struct, IEnumerator<T>
{
	public TEnumerator GetEnumerator()
	{
		return new TEnumerator();
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
