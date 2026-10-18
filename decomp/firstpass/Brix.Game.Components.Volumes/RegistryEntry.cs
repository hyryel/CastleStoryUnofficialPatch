using System;
using System.Collections;
using System.Collections.Generic;
using Brix.Common;
using Brix.Util;
using Brix.Utils;
using Brix.Utils.Closures;

namespace Brix.Game.Components.Volumes;

public class RegistryEntry : PoolableObject<RegistryEntry>
{
	public struct Enumerator : IEnumerator<IVolume>, IDisposable, IEnumerator
	{
		private int _version;

		private RegistryEntry _entry;

		private LongBitsEnumerator _requestBitEnumerator;

		private int _propertyParticipantIndex;

		private List<IVolume> _propertyParticipants;

		object IEnumerator.Current => Current;

		public IVolume Current { get; set; }

		public Enumerator(RegistryEntry entry, VPropertyMask request)
		{
			this = default(Enumerator);
			_version = entry._version;
			_entry = entry;
			_requestBitEnumerator = request.Bits().GetEnumerator();
			_propertyParticipantIndex = 0;
			_propertyParticipants = Empty.List<IVolume>();
		}

		public bool MoveNext()
		{
			if (_version != _entry._version)
			{
				throw new InvalidOperationException("Volume Registry Entry has been changed during iteration. Version mismatch");
			}
			while (_propertyParticipantIndex >= _propertyParticipants.Count)
			{
				if (!_requestBitEnumerator.MoveNext())
				{
					return false;
				}
				_propertyParticipantIndex = 0;
				_propertyParticipants = _entry.PropertyParticipants[_requestBitEnumerator.Current] ?? Empty.List<IVolume>();
			}
			Current = _propertyParticipants[_propertyParticipantIndex++];
			return true;
		}

		public void Reset()
		{
		}

		public void Dispose()
		{
		}
	}

	public static RegistryEntry EmptyEntry = new RegistryEntry();

	private int _version;

	public List<IVolume>[] PropertyParticipants = new List<IVolume>[64];

	public VPropertyMask CurrentProperties;

	public bool Has(VProperty property)
	{
		return (CurrentProperties & property.ToMask()) != 0;
	}

	public bool HasAny(VPropertyMask properties)
	{
		return (CurrentProperties & properties) != 0;
	}

	public Enumerator GetVolumesWith(VPropertyMask mask)
	{
		return new Enumerator(this, mask & CurrentProperties);
	}

	public IVolume GetVolumeWith(VProperty p)
	{
		List<IVolume> list = PropertyParticipants[(int)p];
		return (list == null || list.Count <= 0) ? null : list[0];
	}

	public IVolume GetVolumeWith(VProperty p, Func<IVolume, bool> test)
	{
		List<IVolume> list = PropertyParticipants[(int)p];
		if (list != null)
		{
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				IVolume volume = list[i];
				if (test(volume))
				{
					return volume;
				}
			}
		}
		return null;
	}

	public IVolume GetVolumeWith<TArgs>(VProperty p, Func<IVolume, TArgs, bool> test, TArgs args)
	{
		List<IVolume> list = PropertyParticipants[(int)p];
		if (list != null)
		{
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				IVolume volume = list[i];
				if (test(volume, args))
				{
					return volume;
				}
			}
		}
		return null;
	}

	public IVolume GetVolumeWith(VPropertyMask properties)
	{
		LongBitsEnumerator enumerator = (properties & CurrentProperties).Bits().GetEnumerator();
		while (enumerator.MoveNext())
		{
			IVolume volumeWith = GetVolumeWith((VProperty)enumerator.Current);
			if (volumeWith != null)
			{
				enumerator.Dispose();
				return volumeWith;
			}
		}
		enumerator.Dispose();
		return null;
	}

	public IVolume GetVolumeWith(VPropertyMask properties, Func<IVolume, bool> test)
	{
		LongBitsEnumerator enumerator = (properties & CurrentProperties).Bits().GetEnumerator();
		while (enumerator.MoveNext())
		{
			IVolume volumeWith = GetVolumeWith((VProperty)enumerator.Current, test);
			if (volumeWith != null)
			{
				enumerator.Dispose();
				return volumeWith;
			}
		}
		enumerator.Dispose();
		return null;
	}

	public IVolume GetVolumeWith<TArgs>(VPropertyMask properties, Func<IVolume, TArgs, bool> test, TArgs args)
	{
		foreach (int item in (properties & CurrentProperties).Bits())
		{
			IVolume volumeWith = GetVolumeWith((VProperty)item, test, args);
			if (volumeWith != null)
			{
				return volumeWith;
			}
		}
		return null;
	}

	public bool HasVolumesWith(VProperty p, Func<IVolume, bool> test)
	{
		List<IVolume> list = PropertyParticipants[(int)p];
		if (list != null)
		{
			foreach (IVolume item in list)
			{
				if (test(item))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasVolumesWith<TArgs>(VProperty p, Func<IVolume, TArgs, bool> test, TArgs args)
	{
		List<IVolume> list = PropertyParticipants[(int)p];
		if (list != null)
		{
			foreach (IVolume item in list)
			{
				if (test(item, args))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasVolumesWith<TFunc>(VProperty p, TFunc test) where TFunc : IFunc<IVolume, bool>
	{
		List<IVolume> list = PropertyParticipants[(int)p];
		if (list != null)
		{
			foreach (IVolume item in list)
			{
				if (test.Invoke(item))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasVolumesWith(VPropertyMask properties, Func<IVolume, bool> test)
	{
		using (LongBitsEnumerator longBitsEnumerator = (properties & CurrentProperties).Bits().GetEnumerator())
		{
			while (longBitsEnumerator.MoveNext())
			{
				List<IVolume> list = PropertyParticipants[longBitsEnumerator.Current];
				if (list == null)
				{
					continue;
				}
				foreach (IVolume item in list)
				{
					if (test(item))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public bool HasVolumesWith<TArgs>(VPropertyMask properties, Func<IVolume, TArgs, bool> test, TArgs args)
	{
		using (LongBitsEnumerator longBitsEnumerator = (properties & CurrentProperties).Bits().GetEnumerator())
		{
			while (longBitsEnumerator.MoveNext())
			{
				List<IVolume> list = PropertyParticipants[longBitsEnumerator.Current];
				if (list == null)
				{
					continue;
				}
				foreach (IVolume item in list)
				{
					if (test(item, args))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public bool HasVolumesWith<TFunc>(VPropertyMask properties, TFunc test) where TFunc : IFunc<IVolume, bool>
	{
		using (LongBitsEnumerator longBitsEnumerator = (properties & CurrentProperties).Bits().GetEnumerator())
		{
			while (longBitsEnumerator.MoveNext())
			{
				List<IVolume> list = PropertyParticipants[longBitsEnumerator.Current];
				if (list == null)
				{
					continue;
				}
				foreach (IVolume item in list)
				{
					if (test.Invoke(item))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public List<IVolume> GetVolumesWith(VProperty p)
	{
		return PropertyParticipants[(int)p] ?? Empty.List<IVolume>();
	}

	public void AddVolumeEntry(IVolume volume, VPropertyMask properties)
	{
		using (LongBitsEnumerator longBitsEnumerator = properties.Bits().GetEnumerator())
		{
			while (longBitsEnumerator.MoveNext())
			{
				List<IVolume> list = PropertyParticipants[longBitsEnumerator.Current] ?? (PropertyParticipants[longBitsEnumerator.Current] = CollectionPool.RequestList(() => new List<IVolume>(8)));
				list.Add(volume);
			}
		}
		CurrentProperties |= properties;
		_version++;
	}

	public void RemoveVolumeEntry(IVolume volume, VPropertyMask properties)
	{
		using (LongBitsEnumerator longBitsEnumerator = properties.Bits().GetEnumerator())
		{
			while (longBitsEnumerator.MoveNext())
			{
				List<IVolume> list = PropertyParticipants[longBitsEnumerator.Current];
				list.Remove(volume);
				if (list.Count == 0)
				{
					list.Dispose();
					PropertyParticipants[longBitsEnumerator.Current] = null;
					CurrentProperties = CurrentProperties.Remove(longBitsEnumerator.Current);
				}
			}
		}
		_version++;
	}

	protected override void ResetObject()
	{
	}
}
