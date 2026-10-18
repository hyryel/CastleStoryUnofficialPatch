using System;
using System.Collections;
using System.Collections.Generic;
using Brix.Engine;
using Smooth.Slinq;
using Smooth.Slinq.Context;

namespace Brix.Game.Components.Volumes;

public struct VolumePropertyRequest : IList<XYZ>, ICollection<XYZ>, IEnumerable<XYZ>, IEnumerable
{
	public struct Enumerator : IEnumerator<XYZ>, IDisposable, IEnumerator
	{
		private VolumePropertyRequest _request;

		private List<XYZ>.Enumerator _enumerator;

		object IEnumerator.Current => Current;

		public XYZ Current => _enumerator.Current + _request._volumePosition;

		public Enumerator(VolumePropertyRequest request)
		{
			_enumerator = request._vPropertyIndices.GetEnumerator();
			_request = request;
		}

		public bool MoveNext()
		{
			return _enumerator.MoveNext();
		}

		public void Reset()
		{
			_enumerator = _request._vPropertyIndices.GetEnumerator();
		}

		public void Dispose()
		{
			_enumerator.Dispose();
		}
	}

	private static List<XYZ> empty = new List<XYZ>();

	private XYZ _volumePosition;

	private List<XYZ> _vPropertyIndices;

	private static List<XYZ> __empty = new List<XYZ>();

	public int Count => _vPropertyIndices.Count;

	public bool IsReadOnly => true;

	public XYZ this[int i]
	{
		get
		{
			return _vPropertyIndices[i] + _volumePosition;
		}
		set
		{
			throw new InvalidOperationException();
		}
	}

	public static VolumePropertyRequest Empty => new VolumePropertyRequest(XYZ.zero, __empty);

	public VolumePropertyRequest(XYZ position, List<XYZ> vPropertyIndices)
	{
		_volumePosition = position;
		_vPropertyIndices = vPropertyIndices ?? empty;
	}

	public Slinq<XYZ, SelectContext<XYZ, XYZ, IListContext<XYZ>, XYZ>> Slinq()
	{
		return IListContext<XYZ>.Slinq(_vPropertyIndices, 0, 1).Select((XYZ i, XYZ j) => i + j, _volumePosition);
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator<XYZ> IEnumerable<XYZ>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Add(XYZ i)
	{
		throw new InvalidOperationException();
	}

	public void Clear()
	{
		throw new InvalidOperationException();
	}

	public bool Contains(XYZ i)
	{
		return _vPropertyIndices.Contains(i);
	}

	public void CopyTo(XYZ[] array, int arrayIndex)
	{
		_vPropertyIndices.CopyTo(array, arrayIndex);
		for (int num = _vPropertyIndices.Count - 1; num >= 0; num--)
		{
			ref XYZ reference = ref array[num];
			reference = _vPropertyIndices[num] + _volumePosition;
		}
	}

	public bool Remove(XYZ item)
	{
		throw new InvalidOperationException();
	}

	public int IndexOf(XYZ item)
	{
		return _vPropertyIndices.IndexOf(item - _volumePosition);
	}

	public void Insert(int index, XYZ item)
	{
		throw new InvalidOperationException();
	}

	public void RemoveAt(int index)
	{
		throw new InvalidOperationException();
	}
}
