public struct IntMask
{
	private int mask;

	public bool Empty => mask == 0;

	public IntMask(int i)
	{
		mask = i;
	}

	public static implicit operator IntMask(int i)
	{
		return new IntMask(i);
	}

	public static implicit operator int(IntMask i)
	{
		return i.mask;
	}

	public bool Intersects(IntMask other)
	{
		return (mask & other.mask) != 0;
	}

	public bool Contains(IntMask other)
	{
		return (~mask & other.mask) == 0;
	}

	public bool Matches(IntMask other)
	{
		return mask == other.mask;
	}

	public void Add(IntMask other)
	{
		mask |= other.mask;
	}

	public void Subtract(IntMask other)
	{
		mask &= ~other.mask;
	}
}
