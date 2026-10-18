using System.Collections;
using System.Collections.Generic;

namespace Brix.Util;

public struct BitsEnumerable : IEnumerable<int>, IEnumerable
{
	public int Bits;

	public BitsEnumerable(int bits)
	{
		Bits = bits;
	}

	public BitsEnumerator GetEnumerator()
	{
		return new BitsEnumerator(Bits);
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
