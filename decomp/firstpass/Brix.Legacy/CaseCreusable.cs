using System;
using Brix.Game.AI;

namespace Brix.Legacy;

public class CaseCreusable : IComparable<CaseCreusable>
{
	public int index;

	public int operation;

	public int shotgun;

	public Goal Goal;

	public static CaseCreusable neutre = new CaseCreusable(0, 0);

	public CaseCreusable(int index, int operation)
	{
		this.index = index;
		this.operation = operation;
	}

	public int CompareTo(CaseCreusable other)
	{
		return index.CompareTo(other.index);
	}

	public int CompareTo(int other)
	{
		return index.CompareTo(other);
	}

	public int GetHashcode()
	{
		return index;
	}
}
