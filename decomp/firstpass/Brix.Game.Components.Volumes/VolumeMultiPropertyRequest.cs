using System;
using System.Collections;
using System.Collections.Generic;
using Brix.Engine;
using Brix.Util;
using Brix.Utils;
using Smooth.Slinq;

namespace Brix.Game.Components.Volumes;

public struct VolumeMultiPropertyRequest : IEnumerable<XYZ>, IEnumerable
{
	public struct Enumerator : IEnumerator<XYZ>, IDisposable, IEnumerator
	{
		private VolumeMultiPropertyRequest _request;

		private List<List<XYZ>> _lists;

		private List<int> _indices;

		private XYZ _current;

		private static Comparison<List<XYZ>> _compare = (List<XYZ> a, List<XYZ> b) => b[0].CompareTo(a[0]);

		object IEnumerator.Current => Current;

		public XYZ Current => _current + _request._volumePosition;

		public Enumerator(ref VolumeMultiPropertyRequest request)
		{
			_current = new XYZ(int.MinValue, int.MinValue, int.MinValue);
			_request = request;
			_lists = CollectionPool.RequestList<List<XYZ>>();
			_indices = CollectionPool.RequestList<int>();
			InitPropIndicesList();
		}

		public bool MoveNext()
		{
			XYZ xYZ = new XYZ(int.MaxValue, int.MaxValue, int.MaxValue);
			for (int num = _lists.Count - 1; num >= 0; num--)
			{
				List<XYZ> list = _lists[num];
				int index = _indices[num];
				XYZ xYZ2 = list[index];
				if (xYZ2.CompareTo(_current) > 0)
				{
					if (xYZ2.CompareTo(xYZ) <= 0)
					{
						xYZ = xYZ2;
					}
				}
				else
				{
					index = ++_indices[num];
					if (index < list.Count)
					{
						xYZ2 = list[index];
						if (xYZ2.CompareTo(_current) > 0 && xYZ2.CompareTo(xYZ) <= 0)
						{
							xYZ = xYZ2;
						}
					}
					else
					{
						_lists.RemoveAt(num);
					}
				}
			}
			_current = xYZ;
			return _lists.Count > 0;
		}

		public void Reset()
		{
			_current = new XYZ(int.MaxValue, int.MaxValue, int.MaxValue);
			_indices.Clear();
			_lists.Clear();
			InitPropIndicesList();
		}

		public void Dispose()
		{
			_lists.Dispose();
			_indices.Dispose();
		}

		private void InitPropIndicesList()
		{
			using (LongBitsEnumerator longBitsEnumerator = _request._properties.Bits().GetEnumerator())
			{
				while (longBitsEnumerator.MoveNext())
				{
					List<XYZ> list = _request._volumetricSettingIndices[longBitsEnumerator.Current];
					if (list != null)
					{
						_lists.Add(list);
					}
				}
			}
			_lists.Sort(_compare);
			int num = _lists.Count;
			while (--num >= 0)
			{
				_indices.Add(0);
			}
		}
	}

	private XYZ _volumePosition;

	private VPropertyMask _properties;

	private List<XYZ>[] _volumetricSettingIndices;

	private static List<XYZ>[] __emptyVPropertyIndices = new List<XYZ>[64];

	public static VolumeMultiPropertyRequest Empty => new VolumeMultiPropertyRequest(XYZ.zero, ~VPropertyMask.All, __emptyVPropertyIndices);

	public VolumeMultiPropertyRequest(XYZ index, VPropertyMask properties, List<XYZ>[] volumetricSettingIndices)
	{
		_volumePosition = index;
		_properties = properties;
		_volumetricSettingIndices = volumetricSettingIndices;
	}

	public Slinq<XYZ, VolumeMultiPropertyRequestSlinqContext> Slinq()
	{
		return VolumeMultiPropertyRequestSlinqContext.Slinq(this);
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(ref this);
	}

	IEnumerator<XYZ> IEnumerable<XYZ>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
