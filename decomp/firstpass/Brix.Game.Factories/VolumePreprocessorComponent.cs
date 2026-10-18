using System;
using System.Collections.Generic;
using System.Linq;
using Brix.Engine;
using Brix.Engine.Abstract;
using Brix.External.Factories;
using Brix.External.Factories.Templates;
using Brix.External.Signals;
using Brix.Game.Components.Volumes;
using Brix.Util;
using Brix.Utils;
using Brix.Utils.Logging;
using Smooth.Algebraics;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.Factories;

public class VolumePreprocessorComponent : MonoBehaviour, IFactoryPostProcessor
{
	private class PreProcessedVolumeDataContainer
	{
		private struct Profile : IComparable<Profile>, IEquatable<Profile>
		{
			private readonly Factory.AssetKey _assetKey;

			private readonly int _rotationIndex;

			public Profile(Factory.AssetKey assetKey, int rotationIndex)
			{
				_assetKey = assetKey;
				_rotationIndex = rotationIndex;
			}

			public override bool Equals(object obj)
			{
				if (object.ReferenceEquals(null, obj))
				{
					return false;
				}
				return obj is Profile && Equals((Profile)obj);
			}

			public override int GetHashCode()
			{
				return (((_assetKey != null) ? _assetKey.GetHashCode() : 0) * 397) ^ _rotationIndex;
			}

			public static bool operator ==(Profile left, Profile right)
			{
				return left.Equals(right);
			}

			public static bool operator !=(Profile left, Profile right)
			{
				return !left.Equals(right);
			}

			public int CompareTo(Profile other)
			{
				throw new NotImplementedException();
			}

			public bool Equals(Profile other)
			{
				return object.Equals(_assetKey, other._assetKey) && _rotationIndex == other._rotationIndex;
			}
		}

		private readonly Dictionary<Profile, List<Dictionary<XYZ, VPropertyMask>>> _vSettings = new Dictionary<Profile, List<Dictionary<XYZ, VPropertyMask>>>();

		private readonly Dictionary<Profile, List<List<XYZ>[]>> _indices = new Dictionary<Profile, List<List<XYZ>[]>>();

		private static Dictionary<XYZ, VPropertyMask> emptyIndexedVProperties = Empty.Dictionary<XYZ, VPropertyMask>();

		private static List<XYZ>[] emptyVPropertyIndices = new List<XYZ>[64];

		public bool TryGetIndexedVSettings(Factory.AssetKey assetKey, int rotationIndex, int configId, out Dictionary<XYZ, VPropertyMask> ivs)
		{
			if (_vSettings.TryGetValue(new Profile(assetKey, rotationIndex), out var value))
			{
				ivs = value[configId];
				return true;
			}
			ivs = null;
			return false;
		}

		public bool TryGetIndicesPerVProperty(Factory.AssetKey assetKey, int rotationIndex, int configId, out List<XYZ>[] vsi)
		{
			if (_indices.TryGetValue(new Profile(assetKey, rotationIndex), out var value))
			{
				vsi = value[configId];
				return true;
			}
			vsi = null;
			return false;
		}

		public Dictionary<XYZ, VPropertyMask> GetIndexedVSettings(Factory.AssetKey assetKey, int rotationIndex, int configId)
		{
			try
			{
				if (_vSettings.TryGetValue(new Profile(assetKey, rotationIndex), out var value))
				{
					return value[configId];
				}
				return emptyIndexedVProperties;
			}
			catch (KeyNotFoundException)
			{
				throw new KeyNotFoundException($"No Volume profile rotation found for AssetKey '{assetKey}', with rotation index {rotationIndex}, and configId {configId}");
			}
		}

		public List<XYZ>[] GetIndicesPerVProperty(Factory.AssetKey assetKey, int rotationIndex, int configId)
		{
			try
			{
				if (_indices.TryGetValue(new Profile(assetKey, rotationIndex), out var value))
				{
					return value[configId];
				}
				return emptyVPropertyIndices;
			}
			catch (KeyNotFoundException)
			{
				throw new KeyNotFoundException($"No Volume profile rotation found for AssetKey '{assetKey}', with rotation index {rotationIndex}, and configId {configId}");
			}
		}

		public int AddIndexedVSettings(Factory.AssetKey assetKey, int rotationIndex, Dictionary<XYZ, VPropertyMask> newSettings)
		{
			List<Dictionary<XYZ, VPropertyMask>> valueOrCreate = _vSettings.GetValueOrCreate(new Profile(assetKey, rotationIndex));
			valueOrCreate.Add(newSettings);
			return valueOrCreate.Count - 1;
		}

		public int AddIndicesPerVProperty(Factory.AssetKey assetKey, int rotationIndex, List<XYZ>[] vPropertyIndices)
		{
			List<List<XYZ>[]> valueOrCreate = _indices.GetValueOrCreate(new Profile(assetKey, rotationIndex));
			valueOrCreate.Add(vPropertyIndices);
			return valueOrCreate.Count - 1;
		}

		public int GetLatestConfigId(Factory.AssetKey key)
		{
			return _vSettings[new Profile(key, 0)].Count - 1;
		}
	}

	private static BrixLogger logger = new BrixLogger(typeof(VolumePreprocessorComponent).FullName);

	private PreProcessedVolumeDataContainer _preprocessedData = new PreProcessedVolumeDataContainer();

	protected Signal Destroyed = new Signal();

	private void Awake()
	{
		VolumePreprocessor._instance = this;
	}

	private void OnDestroy()
	{
		Destroyed.Invoke();
	}

	public void PostProcess(Factory f)
	{
		foreach (VolumeDataComponent item in from v in f.GetComponentsInChildren<VolumeDataComponent>(includeInactive: true)
			where v.enabled
			select v)
		{
			PreprocessNewVolume(item.GetInterface<FactoryTemplate>().AssetKey, item);
			IVolume component = item.gameObject.GetComponent<IVolume>();
			if (component == null)
			{
				MovableVolume movableVolume = item.gameObject.AddComponent<MovableVolume>();
				movableVolume.RegistryMode = item.RegistryMode;
			}
			UnityEngine.Object.Destroy(item);
			item.enabled = false;
		}
	}

	public int PreprocessNewVolume(Factory.AssetKey key, VolumeDataComponent source)
	{
		if (_preprocessedData.TryGetIndexedVSettings(key, 0, 0, out var _))
		{
			logger.WarnFormat("The preprocessing of Volume '{0}' has already been done...this means volumes have been processed out of order. This could potentially cause issues", key);
		}
		else
		{
			CopyRotatedVolumeData(key, source);
		}
		return 0;
	}

	public int PreprocessNewVolume(Factory.AssetKey key, Factory.AssetKey source, int configurationId)
	{
		if (_preprocessedData.TryGetIndexedVSettings(key, 0, 0, out var _))
		{
			logger.WarnFormat("The preprocessing of Volume '{0}' has already been done...this means volumes have been processed out of order. This could potentially cause issues", key);
		}
		else
		{
			PreprocessVolume(key, source, configurationId);
		}
		return 0;
	}

	public int PreprocessVolume(Factory.AssetKey key, Factory.AssetKey source, int configurationId)
	{
		int result = -1;
		if (!_preprocessedData.TryGetIndexedVSettings(source, 0, 0, out var _))
		{
			VolumeDataComponent component = Factory.Peek(source).GetComponent<VolumeDataComponent>();
			if (component != null && component.enabled)
			{
				logger.WarnFormat("The preprocessing of Volume '{0}' might be innaccurate because it depends on the yet unprocessed Volume '{1}'", key, source);
				result = CopyRotatedVolumeData(key, component);
			}
			else
			{
				logger.ErrorFormat("The preprocessing of Volume '{0}' could not be completed because it depends on '{1}' which has no VolumeDataComponent", key, source);
			}
		}
		else
		{
			result = CopyRotatedVolumeData(key, source, configurationId);
		}
		return result;
	}

	private int CopyRotatedVolumeData(Factory.AssetKey key, Factory.AssetKey source, int configurationId)
	{
		for (int num = PreprocessedRotations.DiscreteRotationForIndexes.Length - 1; num >= 0; num--)
		{
			Dictionary<XYZ, VPropertyMask> newSettings = _preprocessedData.GetIndexedVSettings(source, num, configurationId).ToIndexedVolumetricSettings();
			List<XYZ>[] vPropertyIndices = _preprocessedData.GetIndicesPerVProperty(source, num, configurationId).ToVolumetricSettingsIndices();
			_preprocessedData.AddIndexedVSettings(key, num, newSettings);
			_preprocessedData.AddIndicesPerVProperty(key, num, vPropertyIndices);
		}
		return _preprocessedData.GetLatestConfigId(key);
	}

	private int CopyRotatedVolumeData(Factory.AssetKey key, VolumeDataComponent source)
	{
		Quaternion[] discreteRotationForIndexes = PreprocessedRotations.DiscreteRotationForIndexes;
		for (int num = discreteRotationForIndexes.Length - 1; num >= 0; num--)
		{
			Dictionary<XYZ, VPropertyMask> dictionary = source.ToIndexedVolumetricSettings(discreteRotationForIndexes[num]);
			List<XYZ>[] vPropertyIndices = dictionary.ToVolumetricSettingsIndices();
			_preprocessedData.AddIndexedVSettings(key, num, dictionary);
			_preprocessedData.AddIndicesPerVProperty(key, num, vPropertyIndices);
		}
		return _preprocessedData.GetLatestConfigId(key);
	}

	public void RemovePropertiesFromVolume(VPropertyMask properties, Factory.AssetKey key, int configurationId)
	{
		for (int num = PreprocessedRotations.DiscreteRotationForIndexes.Length - 1; num >= 0; num--)
		{
			_preprocessedData.GetIndexedVSettings(key, num, configurationId).RemoveProperties(properties);
			_preprocessedData.GetIndicesPerVProperty(key, num, configurationId).RemoveProperties(properties);
		}
	}

	public void RemovePropertiesFromVolume(VProperty p, Factory.AssetKey key, int configurationId)
	{
		for (int num = PreprocessedRotations.DiscreteRotationForIndexes.Length - 1; num >= 0; num--)
		{
			_preprocessedData.GetIndexedVSettings(key, num, configurationId).RemoveProperties(p);
			_preprocessedData.GetIndicesPerVProperty(key, num, configurationId).RemoveProperties(p);
		}
	}

	public void KeepOnlySelectedPropertiesForVolume(VProperty toKeep, Factory.AssetKey key, int configurationId)
	{
		for (int num = PreprocessedRotations.DiscreteRotationForIndexes.Length - 1; num >= 0; num--)
		{
			_preprocessedData.GetIndexedVSettings(key, num, configurationId).KeepProperties(toKeep);
			_preprocessedData.GetIndicesPerVProperty(key, num, configurationId).KeepProperties(toKeep);
		}
	}

	public void KeepOnlySelectedPropertiesForVolume(VPropertyMask toKeep, Factory.AssetKey key, int configurationId)
	{
		for (int num = PreprocessedRotations.DiscreteRotationForIndexes.Length - 1; num >= 0; num--)
		{
			_preprocessedData.GetIndexedVSettings(key, num, configurationId).KeepProperties(toKeep);
			_preprocessedData.GetIndicesPerVProperty(key, num, configurationId).KeepProperties(toKeep);
		}
	}

	public void CopyPropertyIndicesToPropertyFromVolume(List<Tuple<VProperty, VProperty>> conversionRules, Factory.AssetKey key, int configurationId)
	{
		for (int num = PreprocessedRotations.DiscreteRotationForIndexes.Length - 1; num >= 0; num--)
		{
			Dictionary<XYZ, VPropertyMask> indexedVSettings = _preprocessedData.GetIndexedVSettings(key, num, configurationId);
			List<XYZ>[] indicesPerVProperty = _preprocessedData.GetIndicesPerVProperty(key, num, configurationId);
			foreach (Tuple<VProperty, VProperty> conversionRule in conversionRules)
			{
				indexedVSettings.CopyIndices(conversionRule._1, conversionRule._2);
				indicesPerVProperty.CopyIndices(conversionRule._1, conversionRule._2);
			}
			indicesPerVProperty.Slinq().NotNull().ForEach(delegate(List<XYZ> list)
			{
				list.Sort();
			});
		}
	}

	private static int RotateIndex(int index, Quaternion rotation)
	{
		Vector3 vecteur = rotation * Calcule.GetSignedVector(index);
		vecteur = Calcule.RoundVecteur(vecteur);
		return Brix.Engine.Abstract.Voxel.GetScalaire(vecteur);
	}

	public Dictionary<XYZ, VPropertyMask> GetIndexedVSettings(Factory.AssetKey assetKey, int rotationIndex, int configId)
	{
		return _preprocessedData.GetIndexedVSettings(assetKey, rotationIndex, configId);
	}

	public List<XYZ>[] GetIndicesPerVProperty(Factory.AssetKey assetKey, int rotationIndex, int configId)
	{
		return _preprocessedData.GetIndicesPerVProperty(assetKey, rotationIndex, configId);
	}
}
