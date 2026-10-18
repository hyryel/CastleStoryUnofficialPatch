using System.Collections.Generic;
using Brix.Common;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Utils;
using Brix.Utils.Logging;

namespace Brix.Game.Components.Volumes;

public class VolumeRegistrator
{
	private static readonly BrixLogger logger = new BrixLogger(typeof(VolumeRegistrator).FullName);

	public static readonly VolumeRegistrator Instance = new VolumeRegistrator();

	public readonly Signal<BaseVolume> VolumeDataAddedSignal = new Signal<BaseVolume>();

	public readonly Signal<BaseVolume> VolumeDataRemovedSignal = new Signal<BaseVolume>();

	private readonly VolumeRegistryData _data = VolumeRegistry._data;

	public Signal<IVolume> RegisterVolumeSignal = new Signal<IVolume>();

	public Signal<IVolume> UnregisterVolumeSignal = new Signal<IVolume>();

	public VolumeRegistryData Data => _data;

	private VolumeRegistrator()
	{
	}

	public void RegisterVolume(IVolume volume)
	{
		UnregisterVolume_Internal(volume);
		AddVolumetricProperties(volume);
		RegisterVolumeSignal.Invoke(volume);
	}

	public void UnregisterVolume(IVolume v)
	{
		UnregisterVolume_Internal(v);
	}

	public void Reset()
	{
		_data.Reset();
	}

	private void AddVolumetricProperties(IVolume volume)
	{
		IEnumerator<KeyValuePair<XYZ, VPropertyMask>> enumerator = volume.GetEnumerator();
		while (enumerator.MoveNext())
		{
			XYZ key = enumerator.Current.Key;
			VPropertyMask value = enumerator.Current.Value;
			BuildVolumeRegistryEntryAt(volume, key, value);
		}
		enumerator.Dispose();
		_data.RegisteredVolumesData[volume] = volume.BaseVolume;
		VolumeDataAddedSignal.Invoke(volume.BaseVolume);
	}

	private static Dictionary<XYZ, VPropertyMask> DictionaryFactory()
	{
		return CollectionPool.RequestDictionary<XYZ, VPropertyMask>();
	}

	private void UnregisterVolume_Internal(IVolume v)
	{
		if (_data.RegisteredVolumesData.TryGetValue(v, out var value))
		{
			_data.RegisteredVolumesData.Remove(v);
			UnregisterVolumeSignal.Invoke(v);
			VolumeDataRemovedSignal.Invoke(value);
			IEnumerator<KeyValuePair<XYZ, VPropertyMask>> indexedVProperiesEnumerator = value.GetIndexedVProperiesEnumerator();
			while (indexedVProperiesEnumerator.MoveNext())
			{
				XYZ key = indexedVProperiesEnumerator.Current.Key;
				VPropertyMask value2 = indexedVProperiesEnumerator.Current.Value;
				RemoveVolumeRegistryEntryAt(v, key, value2);
			}
			indexedVProperiesEnumerator.Dispose();
		}
	}

	private void RemoveVolumeRegistryEntryAt(IVolume v, XYZ position, VPropertyMask properties)
	{
		if (!_data.IndexedEntries.TryGetValue(position, out var value))
		{
			logger.WarnFormat("Unregistration corrupted. A registry entry is missing at position '{0}' for object '{1}'.", position, v.GameObject.name);
			return;
		}
		value.RemoveVolumeEntry(v, properties);
		if (value.CurrentProperties == ~VPropertyMask.All)
		{
			value.ReleaseToPool();
			_data.IndexedEntries.Remove(position);
		}
	}

	private void BuildVolumeRegistryEntryAt(IVolume volume, XYZ position, VPropertyMask mask)
	{
		RegistryEntry valueOrCreate = _data.IndexedEntries.GetValueOrCreate(position, PoolableObject<RegistryEntry>.Request);
		valueOrCreate.AddVolumeEntry(volume, mask);
	}
}
