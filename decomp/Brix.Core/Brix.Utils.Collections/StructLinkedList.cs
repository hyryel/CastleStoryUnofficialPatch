using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Brix.Utils.Collections;

public class StructLinkedList<T>
{
	internal struct NodeData
	{
		public static NodeData Empty = new NodeData
		{
			Value = default(T),
			Prev = StructLinkedListNode.Empty,
			Next = StructLinkedListNode.Empty,
			BirthIndex = 0
		};

		public T Value;

		public StructLinkedListNode Prev;

		public StructLinkedListNode Next;

		public int BirthIndex;

		public int ReverseIndex;
	}

	public struct OrderedEnumerator : IEnumerator<T>, IDisposable, IEnumerator
	{
		private NodeData[] _data;

		private List<int> _dataIndices;

		private int i;

		object IEnumerator.Current => Current;

		public T Current { get; private set; }

		internal OrderedEnumerator(NodeData[] data, List<int> dataIndices, int firstIndex)
		{
			this = default(OrderedEnumerator);
			_data = data;
			_dataIndices = dataIndices;
			i = firstIndex;
			Current = default(T);
		}

		public bool MoveNext()
		{
			if (i == int.MaxValue)
			{
				return false;
			}
			int num = _dataIndices[i];
			Current = _data[num].Value;
			i = _data[num].Next.Index;
			return true;
		}

		public void Reset()
		{
		}

		public void Dispose()
		{
		}
	}

	public struct UnorderedEnumerator : IEnumerator<T>, IDisposable, IEnumerator
	{
		private StructLinkedList<T> _list;

		private int i;

		object IEnumerator.Current => Current;

		public T Current => _list._data[i].Value;

		internal UnorderedEnumerator(StructLinkedList<T> list)
		{
			_list = list;
			i = -1;
		}

		public bool MoveNext()
		{
			return ++i < _list._count;
		}

		public void Reset()
		{
		}

		public void Dispose()
		{
		}

		public void RemoveCurrentNode()
		{
			_list.Remove(_list._data[i].ReverseIndex);
			i--;
		}
	}

	public delegate void ValueMutator(ref T value);

	public delegate void ValueMutator<T0>(ref T value, T0 p0);

	public static class Test
	{
		public static void Test1()
		{
			StructLinkedList<int> structLinkedList = new StructLinkedList<int>();
			List<StructLinkedListNode> list = new List<StructLinkedListNode>();
			List<int> list2 = new List<int>();
			for (int i = 0; i < 100; i++)
			{
				int num = UnityEngine.Random.Range(0, 1000);
				list.Add(structLinkedList.AddLast(num));
				list2.Add(num);
			}
			for (int j = 0; j < 50; j++)
			{
				int index = UnityEngine.Random.Range(0, list.Count);
				structLinkedList.Remove(list[index]);
				list.RemoveAt(index);
				list2.RemoveAt(index);
			}
			for (int k = 0; k < 50; k++)
			{
				int num2 = UnityEngine.Random.Range(0, 1000);
				list.Add(structLinkedList.AddLast(num2));
				list2.Add(num2);
			}
			for (int l = 0; l < 50; l++)
			{
				int index2 = UnityEngine.Random.Range(0, list.Count);
				structLinkedList.Remove(list[index2]);
				list.RemoveAt(index2);
				list2.RemoveAt(index2);
			}
			if (list.Count != list2.Count)
			{
				Debug.LogWarning("Wrong count:" + list.Count + ", " + list2.Count);
				return;
			}
			for (int m = 0; m < list2.Count; m++)
			{
				if (structLinkedList.GetValue(list[m]) != list2[m])
				{
					Debug.LogWarning("Wrong item:" + m + ", " + structLinkedList.GetValue(list[m]) + ", " + list2[m]);
					return;
				}
			}
			if (structLinkedList._dataIndices.Count != 100)
			{
				Debug.LogWarning("Wrong internal array count:" + structLinkedList._dataIndices.Count);
			}
		}
	}

	private static readonly NodeData[] EmptyArray = new NodeData[0];

	private int _nextBirthIndex = 1;

	private List<int> _dataIndices = new List<int>();

	private List<int> _freeIndices = new List<int>();

	private NodeData[] _data = EmptyArray;

	private int _count;

	private StructLinkedListNode _first = StructLinkedListNode.Empty;

	private StructLinkedListNode _last = StructLinkedListNode.Empty;

	public int Count => _count;

	public StructLinkedListNode First => _first;

	public StructLinkedListNode Last => _last;

	public OrderedEnumerator GetOrderedEnumerator()
	{
		return new OrderedEnumerator(_data, _dataIndices, _first.Index);
	}

	public UnorderedEnumerator GetUnorderedEnumerator()
	{
		return new UnorderedEnumerator(this);
	}

	private StructLinkedListNode Alloc()
	{
		int birthIndex = _nextBirthIndex++;
		int num;
		if (_freeIndices.Count > 0)
		{
			num = _freeIndices.Pop();
			_dataIndices[num] = _count;
		}
		else
		{
			num = _dataIndices.Count;
			_dataIndices.Add(_count);
		}
		if (_count >= _data.Length)
		{
			Array.Resize(ref _data, Mathf.Max(1, _count * 2));
		}
		_data[_count] = NodeData.Empty;
		_data[_count].BirthIndex = birthIndex;
		_data[_count].ReverseIndex = num;
		_count++;
		return new StructLinkedListNode(num, birthIndex);
	}

	private void Release(int nodeIndex)
	{
		int num = _count - 1;
		int num2 = _dataIndices[nodeIndex];
		int reverseIndex = _data[num].ReverseIndex;
		_data[num2] = _data[num];
		_data[num] = NodeData.Empty;
		_dataIndices[reverseIndex] = num2;
		_freeIndices.Add(nodeIndex);
		_count--;
	}

	public StructLinkedListNode GetPrev(StructLinkedListNode node)
	{
		return GetPrev(node.Index);
	}

	public StructLinkedListNode GetNext(StructLinkedListNode node)
	{
		return GetNext(node.Index);
	}

	private StructLinkedListNode GetPrev(int nodeIndex)
	{
		return _data[_dataIndices[nodeIndex]].Prev;
	}

	private StructLinkedListNode GetNext(int nodeIndex)
	{
		return _data[_dataIndices[nodeIndex]].Next;
	}

	private void SetPrev(int nodeIndex, StructLinkedListNode prev)
	{
		_data[_dataIndices[nodeIndex]].Prev = prev;
	}

	private void SetNext(int nodeIndex, StructLinkedListNode next)
	{
		_data[_dataIndices[nodeIndex]].Next = next;
	}

	public bool IsActive(StructLinkedListNode node)
	{
		return IsActive(node.Index, node.BirthIndex);
	}

	public bool Equals(StructLinkedListNode a, StructLinkedListNode b)
	{
		return a.Index == b.Index;
	}

	private bool IsActive(int nodeIndex, int birthIndex)
	{
		if (nodeIndex >= _dataIndices.Count)
		{
			return false;
		}
		int num = _dataIndices[nodeIndex];
		if (num >= _count)
		{
			return false;
		}
		return birthIndex == _data[num].BirthIndex;
	}

	public T GetValue(StructLinkedListNode node)
	{
		return _data[_dataIndices[node.Index]].Value;
	}

	public void SetValue(StructLinkedListNode node, T value)
	{
		_data[_dataIndices[node.Index]].Value = value;
	}

	public void EditValue(StructLinkedListNode node, ValueMutator mutator)
	{
		mutator(ref _data[_dataIndices[node.Index]].Value);
	}

	public void EditValue<T0>(StructLinkedListNode node, ValueMutator<T0> mutator, T0 p0)
	{
		mutator(ref _data[_dataIndices[node.Index]].Value, p0);
	}

	public void Clear()
	{
		_dataIndices.Clear();
		_freeIndices.Clear();
		Array.Clear(_data, 0, _count);
		_data = EmptyArray;
		_count = 0;
		_first = (_last = StructLinkedListNode.Empty);
	}

	public StructLinkedListNode AddLast(T value)
	{
		StructLinkedListNode structLinkedListNode = AddLast();
		SetValue(structLinkedListNode, value);
		return structLinkedListNode;
	}

	private StructLinkedListNode AddLast()
	{
		StructLinkedListNode structLinkedListNode;
		if (Count != 0)
		{
			StructLinkedListNode last = _last;
			structLinkedListNode = (_last = Alloc());
			SetNext(last.Index, structLinkedListNode);
			SetPrev(_last.Index, last);
		}
		else
		{
			structLinkedListNode = (_first = (_last = Alloc()));
		}
		return structLinkedListNode;
	}

	public void Remove(StructLinkedListNode node)
	{
		Remove(node.Index);
	}

	private void Remove(int nodeIndex)
	{
		if (Count > 1)
		{
			StructLinkedListNode prev = GetPrev(nodeIndex);
			StructLinkedListNode next = GetNext(nodeIndex);
			if (IsActive(prev))
			{
				SetNext(prev.Index, next);
			}
			if (IsActive(next))
			{
				SetPrev(next.Index, prev);
			}
			if (_first.Index == nodeIndex)
			{
				_first = next;
			}
			if (_last.Index == nodeIndex)
			{
				_last = prev;
			}
		}
		else
		{
			_first = (_last = StructLinkedListNode.Empty);
		}
		Release(nodeIndex);
	}
}
