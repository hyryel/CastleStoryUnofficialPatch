namespace Brix.Util;

public static class CountBitsExtender
{
	public static int CountBits(this int x)
	{
		int num = 0;
		while (x != 0)
		{
			x &= x - 1;
			num++;
		}
		return num;
	}

	public static int CountBits(this long x)
	{
		int num = 0;
		while (x != 0)
		{
			x &= x - 1;
			num++;
		}
		return num;
	}

	public static BitsEnumerable Bits(this int x)
	{
		return new BitsEnumerable(x);
	}

	public static LongBitsEnumerable Bits(this long x)
	{
		return new LongBitsEnumerable(x);
	}
}
