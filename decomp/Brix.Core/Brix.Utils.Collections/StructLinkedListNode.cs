using System;

namespace Brix.Utils.Collections;

public struct StructLinkedListNode : IComparable<StructLinkedListNode>, IEquatable<StructLinkedListNode>
{
	internal static readonly StructLinkedListNode Empty = new StructLinkedListNode(int.MaxValue, 0);

	internal readonly int Index;

	internal readonly int BirthIndex;

	public StructLinkedListNode(int index, int birthIndex)
	{
		Index = index;
		BirthIndex = birthIndex;
	}

	public override int GetHashCode()
	{
		return (Index * 397) ^ BirthIndex;
	}

	public static bool operator ==(StructLinkedListNode left, StructLinkedListNode right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(StructLinkedListNode left, StructLinkedListNode right)
	{
		return !left.Equals(right);
	}

	public int CompareTo(StructLinkedListNode other)
	{
		int num = Index.CompareTo(other.Index);
		if (num == 0)
		{
			num = BirthIndex.CompareTo(other.BirthIndex);
		}
		return num;
	}

	public bool Equals(StructLinkedListNode other)
	{
		return Index.Equals(other.Index) && BirthIndex.Equals(other.BirthIndex);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		return obj is StructLinkedListNode && Equals((StructLinkedListNode)obj);
	}
}
