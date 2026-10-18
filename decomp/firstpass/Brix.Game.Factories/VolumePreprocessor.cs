using System.Collections.Generic;
using System.Linq;
using Brix.Engine;
using Brix.External.Factories;
using Brix.Game.Components.Volumes;
using Brix.Utils;
using Smooth.Algebraics;
using UnityEngine;

namespace Brix.Game.Factories;

public class VolumePreprocessor
{
	public static VolumePreprocessorComponent _instance;

	public static Dictionary<XYZ, VPropertyMask> GetIndexedVSettings(Factory.AssetKey assetKey, int rotationIndex, int configId = 0)
	{
		return _instance.GetIndexedVSettings(assetKey, rotationIndex, configId);
	}

	public static Dictionary<XYZ, VPropertyMask> GetIndexedVSettings(Factory.AssetKey assetKey, Quaternion q, int configId = 0)
	{
		return _instance.GetIndexedVSettings(assetKey, PreprocessedRotations.RotationToDiscreteIndex(q), configId);
	}

	public static List<XYZ>[] GetVSettingsIndices(Factory.AssetKey assetKey, int rotationIndex, int configId = 0)
	{
		return _instance.GetIndicesPerVProperty(assetKey, rotationIndex, configId);
	}

	public static List<XYZ>[] GetVSettingsIndices(Factory.AssetKey assetKey, Quaternion q, int configId = 0)
	{
		return _instance.GetIndicesPerVProperty(assetKey, PreprocessedRotations.RotationToDiscreteIndex(q), configId);
	}

	public static IEnumerable<KeyValuePair<XYZ, VPropertyMask>> GloballyMappedIndexedVSettings(Factory.AssetKey assetKey, XYZ index, int rotationIndex)
	{
		return MapIndexedVSettings(index, GetIndexedVSettings(assetKey, rotationIndex));
	}

	public static IEnumerable<KeyValuePair<XYZ, VPropertyMask>> GloballyMappedIndexedVSettings(Factory.AssetKey assetKey, XYZ index, Quaternion rotation)
	{
		return MapIndexedVSettings(index, GetIndexedVSettings(assetKey, rotation));
	}

	private static IEnumerable<KeyValuePair<XYZ, VPropertyMask>> MapIndexedVSettings(XYZ index, Dictionary<XYZ, VPropertyMask> rotatedSettings)
	{
		using Dictionary<XYZ, VPropertyMask>.Enumerator e = rotatedSettings.GetEnumerator();
		KeyValuePair<XYZ, VPropertyMask> current = e.Current;
		yield return new KeyValuePair<XYZ, VPropertyMask>(current.Key + index, current.Value);
	}

	public static IEnumerable<KeyValuePair<VProperty, IEnumerable<XYZ>>> GloballyMappedIndicesPerVProperty(Factory.AssetKey assetKey, XYZ delta, int rotationIndex)
	{
		List<XYZ>[] rotatedIndicesPerVProperty = GetVSettingsIndices(assetKey, rotationIndex);
		for (int i = 0; i < rotatedIndicesPerVProperty.Length; i++)
		{
			VProperty vProperty = (VProperty)i;
			IEnumerable<XYZ> indices = rotatedIndicesPerVProperty[i].Select((XYZ position) => position + delta);
			yield return new KeyValuePair<VProperty, IEnumerable<XYZ>>(vProperty, indices);
		}
	}

	public static IEnumerable<KeyValuePair<VProperty, IEnumerable<XYZ>>> GloballyMappedIndicesPerVProperty(Factory.AssetKey assetKey, XYZ delta, Quaternion rotation)
	{
		List<XYZ>[] rotatedIndicesPerVProperty = GetVSettingsIndices(assetKey, rotation);
		for (int i = 0; i < rotatedIndicesPerVProperty.Length; i++)
		{
			VProperty vProperty = (VProperty)i;
			IEnumerable<XYZ> indices = rotatedIndicesPerVProperty[i].Select((XYZ position) => position + delta);
			yield return new KeyValuePair<VProperty, IEnumerable<XYZ>>(vProperty, indices);
		}
	}

	public static void CreatePreprocessedVolume(Factory.AssetKey key, VolumeDataComponent source)
	{
		_instance.PreprocessNewVolume(key, source);
	}

	public static void CreatePreprocessedVolume(Factory.AssetKey key, Factory.AssetKey source, int configurationId = 0)
	{
		_instance.PreprocessNewVolume(key, source, configurationId);
	}

	public static int CreatePreprocessedVolumeSubEntry(Factory.AssetKey key, Factory.AssetKey source, int configurationId = 0)
	{
		return _instance.PreprocessVolume(key, source, configurationId);
	}

	public static int CreatePreprocessedVolumeSubEntry(Factory.AssetKey key, int configurationId = 0)
	{
		return _instance.PreprocessVolume(key, key, configurationId);
	}

	public static void RemovePropertiesFromVolume(VPropertyMask excluded, Factory.AssetKey key, int configurationId = 0)
	{
		_instance.RemovePropertiesFromVolume(excluded, key, configurationId);
	}

	public static void RemovePropertiesFromVolume(VProperty excluded, Factory.AssetKey key, int configurationId = 0)
	{
		_instance.RemovePropertiesFromVolume(excluded, key, configurationId);
	}

	public static void KeepOnlySelectedPropertiesForVolume(VPropertyMask ToKeep, Factory.AssetKey key, int configurationId = 0)
	{
		_instance.KeepOnlySelectedPropertiesForVolume(ToKeep, key, configurationId);
	}

	public static void KeepOnlySelectedPropertiesForVolume(VProperty ToKeep, Factory.AssetKey key, int configurationId = 0)
	{
		_instance.KeepOnlySelectedPropertiesForVolume(ToKeep, key, configurationId);
	}

	public static void CopyPropertyIndicesToPropertyFromVolume(List<Tuple<VProperty, VProperty>> conversionRules, Factory.AssetKey key, int configurationId = 0)
	{
		_instance.CopyPropertyIndicesToPropertyFromVolume(conversionRules, key, configurationId);
	}
}
