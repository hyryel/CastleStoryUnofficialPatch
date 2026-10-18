using System;
using System.Collections.Generic;

namespace Brix.Utils.Collections;

public class StructSet<T> where T : class, new()
{
	public struct Node : IComparable<Node>, IEquatable<Node>
	{
		internal static readonly Node Empty = new Node(int.MaxValue, 0);

		internal readonly int Index;

		internal readonly int BirthIndex;

		public Node(int index, int birthIndex)
		{
			Index = index;
			BirthIndex = birthIndex;
		}

		public override int GetHashCode()
		{
			return (Index * 397) ^ BirthIndex;
		}

		public static bool operator ==(Node left, Node right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Node left, Node right)
		{
			return !left.Equals(right);
		}

		public int CompareTo(Node other)
		{
			int num = Index.CompareTo(other.Index);
			if (num == 0)
			{
				num = BirthIndex.CompareTo(other.BirthIndex);
			}
			return num;
		}

		public bool Equals(Node other)
		{
			return Index.Equals(other.Index) && BirthIndex.Equals(other.BirthIndex);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			return obj is Node && Equals((Node)obj);
		}
	}

	public delegate void ValueMutator(ref T value);

	public delegate void ValueMutator<T0>(ref T value, T0 p0);

	private int _nextBirthIndex = 1;

	private List<int> _dataIndices = new List<int>();

	private List<int> _birthIndices = new List<int>();

	private List<int> _reverseIndices = new List<int>();

	private T[] _values = new T[1];

	private int _count = 0;

	public int Count => _count;

	public Node Alloc()
	{
		int num = _nextBirthIndex++;
		int count = _dataIndices.Count;
		_dataIndices.Add(_count);
		_birthIndices.Add(num);
		_reverseIndices.Add(count);
		if (_count >= _values.Length)
		{
			Array.Resize(ref _values, _count * 2);
		}
		if (_values[_count] == null)
		{
			_values[_count] = new T();
		}
		_count++;
		return new Node(count, num);
	}

	public void Release(Node node)
	{
		int num = _dataIndices[node.Index];
		int value = _birthIndices.Pop();
		int num2 = _reverseIndices.Pop();
		Donnees.Swap(ref _values[num], ref _values[_count - 1]);
		_birthIndices[num] = value;
		_reverseIndices[num] = num2;
		_dataIndices[num2] = num;
		_count--;
		if (_count * 3 < _values.Length * 2)
		{
			Array.Resize(ref _values, _count + 1);
		}
	}

	public bool IsActive(Node node)
	{
		if (node.Index >= _dataIndices.Count)
		{
			return false;
		}
		int num = _dataIndices[node.Index];
		if (num >= _count)
		{
			return false;
		}
		return node.BirthIndex == _birthIndices[num];
	}

	public bool Equals(Node a, Node b)
	{
		return a.Index == b.Index;
	}

	public T GetValue(Node node)
	{
		return _values[_dataIndices[node.Index]];
	}

	public void SetValue(Node node, T value)
	{
		_values[_dataIndices[node.Index]] = value;
	}

	public void EditValue(Node node, ValueMutator mutator)
	{
		mutator(ref _values[_dataIndices[node.Index]]);
	}

	public void EditValue<T0>(Node node, ValueMutator<T0> mutator, T0 p0)
	{
		mutator(ref _values[_dataIndices[node.Index]], p0);
	}

	public void Clear()
	{
		_count = 0;
		_dataIndices.Clear();
		_birthIndices.Clear();
		_reverseIndices.Clear();
		Array.Resize(ref _values, 1);
		Array.Clear(_values, 0, _values.Length);
	}
}
