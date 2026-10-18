using System;

namespace Brix.Util.Paire;

public class Paire : IComparable<Paire>
{
	public int a;

	public int b;

	public Paire(int A, int B)
	{
		a = A;
		b = B;
	}

	public int CompareTo(Paire other)
	{
		int num = a.CompareTo(other.a);
		if (num != 0)
		{
			return num;
		}
		return b.CompareTo(other.b);
	}

	public void Set(int a, int b)
	{
		this.a = a;
		this.b = b;
	}
}
