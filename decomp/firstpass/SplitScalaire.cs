public struct SplitScalaire
{
	public short x;

	public short y;

	public short z;

	public SplitScalaire(int i)
	{
		x = (short)i;
		y = (short)i;
		z = (short)i;
	}

	public SplitScalaire(int x, int y, int z)
	{
		this.x = (short)x;
		this.y = (short)y;
		this.z = (short)z;
	}

	public static implicit operator SplitScalaire(int i)
	{
		return new SplitScalaire(i);
	}

	public static implicit operator int(SplitScalaire s)
	{
		return s.x;
	}

	public static SplitScalaire operator +(SplitScalaire i, SplitScalaire j)
	{
		return new SplitScalaire(i.x + j.x, i.y + j.y, i.z + j.z);
	}

	public void PlusEgal(SplitScalaire i)
	{
		x += i.x;
		y += i.y;
		z += i.z;
	}
}
