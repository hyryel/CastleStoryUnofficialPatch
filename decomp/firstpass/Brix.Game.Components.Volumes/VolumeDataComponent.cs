using System.Collections;
using System.Collections.Generic;
using Brix.Engine;
using UnityEngine;

namespace Brix.Game.Components.Volumes;

[RequireComponent(typeof(BaseVoxelTransform))]
[ExecuteInEditMode]
public class VolumeDataComponent : GameComponent, IEnumerable<KeyValuePair<XYZ, VPropertyMask>>, IEnumerable
{
	[EnumFlag]
	[SerializeField]
	private VolumeRegistryMode _registryMode = VolumeRegistryMode.OnStart | VolumeRegistryMode.OnMove;

	[HideInInspector]
	[SerializeField]
	private IndexedVolumetricProperties _indexedVolumetricProperties = new IndexedVolumetricProperties();

	public VolumeRegistryMode RegistryMode => _registryMode;

	public Dictionary<XYZ, long> IndexedVolumetricProperties => _indexedVolumetricProperties;

	protected override void Awake()
	{
		base.Awake();
		_indexedVolumetricProperties = _indexedVolumetricProperties ?? new IndexedVolumetricProperties();
	}

	public VPropertyMask GetVolumetricSettingsAt(XYZ localPosition)
	{
		return (VPropertyMask)IndexedVolumetricProperties.GetValueOrDefault(localPosition);
	}

	public void RemoveVPropertiesAt(XYZ localPosition, VPropertyMask settings)
	{
		long num = IndexedVolumetricProperties[localPosition] & (long)(~settings);
		if (num != 0)
		{
			IndexedVolumetricProperties[localPosition] = num;
		}
		else
		{
			IndexedVolumetricProperties.Remove(localPosition);
		}
	}

	public void AddVPropertiesAt(XYZ localPosition, VPropertyMask settings)
	{
		IndexedVolumetricProperties[localPosition] = IndexedVolumetricProperties.GetValueOrDefault(localPosition) | (long)settings;
	}

	public IEnumerator<KeyValuePair<XYZ, VPropertyMask>> GetEnumerator()
	{
		foreach (KeyValuePair<XYZ, long> kvp in IndexedVolumetricProperties)
		{
			yield return new KeyValuePair<XYZ, VPropertyMask>(kvp.Key, (VPropertyMask)kvp.Value);
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
