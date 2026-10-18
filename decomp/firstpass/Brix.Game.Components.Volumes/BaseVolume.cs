using System;
using System.Collections;
using System.Collections.Generic;
using Brix.Common;
using Brix.Engine;
using Brix.Util;
using Brix.Utils;

namespace Brix.Game.Components.Volumes;

public struct BaseVolume
{
	public class VPropertyIndicesEnumerator : PoolableObject<VPropertyIndicesEnumerator>, IEnumerator<KeyValuePair<VProperty, VolumePropertyRequest>>, IDisposable, IEnumerator
	{
		public LongBitsEnumerator enumerator;

		object IEnumerator.Current => Current;

		public XYZ Position { get; set; }

		public List<XYZ>[] VolumetricPropertyIndices { get; set; }

		public KeyValuePair<VProperty, VolumePropertyRequest> Current
		{
			get
			{
				int current = enumerator.Current;
				return new KeyValuePair<VProperty, VolumePropertyRequest>((VProperty)current, new VolumePropertyRequest(Position, VolumetricPropertyIndices[current]));
			}
		}

		public static VPropertyIndicesEnumerator Request(BaseVolume v)
		{
			return Request(v.Position, v);
		}

		public static VPropertyIndicesEnumerator Request(XYZ position, BaseVolume v)
		{
			VPropertyIndicesEnumerator vPropertyIndicesEnumerator = PoolableObject<VPropertyIndicesEnumerator>.Request();
			vPropertyIndicesEnumerator.Position = position;
			vPropertyIndicesEnumerator.VolumetricPropertyIndices = v.VolumetricPropertyIndices;
			vPropertyIndicesEnumerator.enumerator = v.PropertiesInUse.Bits().GetEnumerator();
			return vPropertyIndicesEnumerator;
		}

		protected override void ResetObject()
		{
			Position = XYZ.zero;
			VolumetricPropertyIndices = null;
			enumerator = default(LongBitsEnumerator);
		}

		public void Dispose()
		{
			enumerator.Dispose();
			ReleaseToPool();
		}

		public bool MoveNext()
		{
			return enumerator.MoveNext();
		}

		public void Reset()
		{
			enumerator.Reset();
		}
	}

	public class IndexedVProperiesEnumerator : PoolableObject<IndexedVProperiesEnumerator>, IEnumerator<KeyValuePair<XYZ, VPropertyMask>>, IDisposable, IEnumerator
	{
		public Dictionary<XYZ, VPropertyMask>.Enumerator enumerator;

		object IEnumerator.Current => Current;

		public XYZ Position { get; set; }

		public KeyValuePair<XYZ, VPropertyMask> Current
		{
			get
			{
				KeyValuePair<XYZ, VPropertyMask> current = enumerator.Current;
				return new KeyValuePair<XYZ, VPropertyMask>(current.Key + Position, current.Value);
			}
		}

		public static IndexedVProperiesEnumerator Request(BaseVolume v)
		{
			return Request(v.Position, v);
		}

		public static IndexedVProperiesEnumerator Request(XYZ position, BaseVolume v)
		{
			IndexedVProperiesEnumerator indexedVProperiesEnumerator = PoolableObject<IndexedVProperiesEnumerator>.Request();
			indexedVProperiesEnumerator.Position = position;
			indexedVProperiesEnumerator.enumerator = v.IndexedVolumetricProperties.GetEnumerator();
			return indexedVProperiesEnumerator;
		}

		protected override void ResetObject()
		{
			Position = XYZ.zero;
			enumerator = default(Dictionary<XYZ, VPropertyMask>.Enumerator);
		}

		public void Dispose()
		{
			enumerator.Dispose();
			ReleaseToPool();
		}

		public bool MoveNext()
		{
			return enumerator.MoveNext();
		}

		public void Reset()
		{
		}
	}

	private static List<XYZ>[] EMPTY_ARRAY = new List<XYZ>[64];

	private XYZ _position;

	private Dictionary<XYZ, VPropertyMask> _indexedVolumetricProperties;

	public List<XYZ>[] _volumetricPropertyIndices;

	private VPropertyMask _propertiesInUse;

	public XYZ Position
	{
		get
		{
			return _position;
		}
		set
		{
			_position = value;
		}
	}

	public Dictionary<XYZ, VPropertyMask> IndexedVolumetricProperties
	{
		get
		{
			return _indexedVolumetricProperties;
		}
		set
		{
			_indexedVolumetricProperties = value;
		}
	}

	public List<XYZ>[] VolumetricPropertyIndices
	{
		get
		{
			return _volumetricPropertyIndices;
		}
		set
		{
			_volumetricPropertyIndices = value;
			GeneratePropertiesInUse();
		}
	}

	public VPropertyMask PropertiesInUse
	{
		get
		{
			return _propertiesInUse;
		}
		private set
		{
			_propertiesInUse = value;
		}
	}

	public BaseVolume(object o)
	{
		_position = XYZ.zero;
		_propertiesInUse = ~VPropertyMask.All;
		_volumetricPropertyIndices = EMPTY_ARRAY;
		_indexedVolumetricProperties = Empty.Dictionary<XYZ, VPropertyMask>();
	}

	public VoxelBounds ComputeBounds()
	{
		return new VoxelBounds(IndexedVolumetricProperties.Keys);
	}

	public void GeneratePropertiesInUse()
	{
		long num = 0L;
		for (int i = 0; i < 64; i++)
		{
			if (_volumetricPropertyIndices[i] != null)
			{
				num |= 1L << i;
			}
		}
		PropertiesInUse = (VPropertyMask)num;
	}

	public void Reset()
	{
		Position = XYZ.zero;
		IndexedVolumetricProperties = Empty.Dictionary<XYZ, VPropertyMask>();
		_volumetricPropertyIndices = EMPTY_ARRAY;
		PropertiesInUse = ~VPropertyMask.All;
	}

	public VolumePropertyRequest GetCases(VProperty p)
	{
		return new VolumePropertyRequest(Position, VolumetricPropertyIndices[(int)p]);
	}

	public VolumeMultiPropertyRequest GetCases(VPropertyMask hp)
	{
		return new VolumeMultiPropertyRequest(Position, hp, VolumetricPropertyIndices);
	}

	public bool HasProperty(VProperty p)
	{
		return VolumetricPropertyIndices[(int)p] != null;
	}

	public bool HasProperties(VPropertyMask p)
	{
		return p.Bits().Slinq().All((int i, List<XYZ>[] vpi) => vpi[i] != null, VolumetricPropertyIndices);
	}

	public bool HasAnyProperties(VPropertyMask p)
	{
		return p.Bits().Slinq().Any((int i, List<XYZ>[] vpi) => vpi[i] != null, VolumetricPropertyIndices);
	}

	public bool HasPropertyAt(VProperty p, XYZ position)
	{
		VPropertyMask value;
		return IndexedVolumetricProperties.TryGetValue(position - Position, out value) && value.HasFlag(p.ToMask());
	}

	public bool HasPropertyAt(VProperty p, IEnumerable<XYZ> indices)
	{
		if (VolumetricPropertyIndices[(int)p] != null)
		{
			List<XYZ> list = VolumetricPropertyIndices[(int)p];
			foreach (XYZ index in indices)
			{
				if (!list.Contains(index - Position))
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public bool HasPropertiesAt(VPropertyMask p, XYZ position)
	{
		VPropertyMask value;
		return IndexedVolumetricProperties.TryGetValue(position - Position, out value) && (value & p) == p;
	}

	public bool HasPropertiesAt(VPropertyMask p, IEnumerable<XYZ> indices)
	{
		foreach (XYZ index in indices)
		{
			if (!HasPropertiesAt(p, index))
			{
				return false;
			}
		}
		return true;
	}

	public bool HasAnyPropertiesAt(VPropertyMask p, XYZ position)
	{
		VPropertyMask value;
		return IndexedVolumetricProperties.TryGetValue(position - Position, out value) && (value & p) != 0;
	}

	public bool HasAnyPropertiesAt(VPropertyMask p, IEnumerable<XYZ> indices)
	{
		foreach (XYZ index in indices)
		{
			if (HasAnyPropertiesAt(p, index))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasPropertyInAny(VProperty p, IEnumerable<XYZ> indices)
	{
		List<XYZ> list = VolumetricPropertyIndices[(int)p];
		if (list != null)
		{
			foreach (XYZ index in indices)
			{
				if (list.Contains(index - Position))
				{
					return true;
				}
			}
			return false;
		}
		return false;
	}

	public bool HasPropertiesInAny(VPropertyMask p, IEnumerable<XYZ> indices)
	{
		foreach (XYZ index in indices)
		{
			if (HasPropertiesAt(p, index))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAnyPropertiesInAny(VPropertyMask p, IEnumerable<XYZ> indices)
	{
		foreach (XYZ index in indices)
		{
			if (HasAnyPropertiesAt(p, index))
			{
				return true;
			}
		}
		return false;
	}

	public IEnumerator<KeyValuePair<VProperty, VolumePropertyRequest>> GetVPropertyIndicesEnumerator()
	{
		return VPropertyIndicesEnumerator.Request(this);
	}

	public IEnumerator<KeyValuePair<VProperty, VolumePropertyRequest>> GetVPropertyIndicesEnumerator(XYZ position)
	{
		return VPropertyIndicesEnumerator.Request(position, this);
	}

	public IEnumerator<KeyValuePair<XYZ, VPropertyMask>> GetIndexedVProperiesEnumerator()
	{
		return IndexedVProperiesEnumerator.Request(this);
	}

	public IEnumerator<KeyValuePair<XYZ, VPropertyMask>> GetIndexedVProperiesEnumerator(XYZ position)
	{
		return IndexedVProperiesEnumerator.Request(position, this);
	}
}
