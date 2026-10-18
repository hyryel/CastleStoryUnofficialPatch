using System.Collections;
using System.Collections.Generic;

namespace Brix.Util;

public struct LongBitsEnumerable : IEnumerable<int>, IEnumerable
{
	public long Bits;

	public LongBitsEnumerable(long bits)
	{
		Bits = bits;
	}

	public LongBitsEnumerator GetEnumerator()
	{
		return new LongBitsEnumerator(Bits);
	}

	IEnumerator<int> IEnumerable<int>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
