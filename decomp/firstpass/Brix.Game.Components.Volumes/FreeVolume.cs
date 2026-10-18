using System;
using System.Collections;
using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.AI.Damage;
using Brix.Game.Factories;
using Brix.Utils;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.Game.Components.Volumes;

public class FreeVolume : IVolume, IPoolable, IDisposable, IEnumerable<KeyValuePair<XYZ, VPropertyMask>>, IEnumerable, IResetable
{
	protected static BrixLogger logger = new BrixLogger(typeof(FreeVolume).FullName);

	private bool _disposed;

	private BaseVolume _baseVolume = new BaseVolume(null);

	private int _rotationIndex;

	private int _subConfiguration;

	private Signal<GameObject> _destroyedSignal;

	GameObject IVolume.GameObject => null;

	IGameComponent IVolume.GameComponent => null;

	public Factory.AssetKey AssetKey { get; private set; }

	public Faction Faction { get; set; }

	public string Name => "Free volume";

	public XYZ Position
	{
		get
		{
			return _baseVolume.Position;
		}
		set
		{
			_baseVolume.Position = value;
		}
	}

	public BaseVolume BaseVolume => _baseVolume;

	public int RotationIndex
	{
		get
		{
			return _rotationIndex;
		}
		set
		{
			if (_rotationIndex != value)
			{
				_rotationIndex = value;
				RegenerateMappings();
			}
		}
	}

	public Quaternion Rotation => PreprocessedRotations.DiscreteRotationForIndexes[_rotationIndex];

	public int SubConfiguration
	{
		get
		{
			return _subConfiguration;
		}
		set
		{
			if (_subConfiguration != value)
			{
				_subConfiguration = value;
				RegenerateMappings();
			}
		}
	}

	public IPassThrough PassThrough => PassThroughObstacle.Null;

	public IDamageReceiver DamageReceiver => NullDamageReceiver.Get;

	public VolumePropertyRequest access => GetCases(VProperty.access);

	public VolumePropertyRequest blueprint => GetCases(VProperty.fantome);

	public VolumePropertyRequest obstacles => GetCases(VProperty.obstacle);

	public ValidationData VData => GenerateValidationData();

	public Dictionary<XYZ, VPropertyMask> IndexedVolumetricProperties
	{
		get
		{
			return _baseVolume.IndexedVolumetricProperties;
		}
		set
		{
			_baseVolume.IndexedVolumetricProperties = value;
		}
	}

	public List<XYZ>[] VolumetricPropertyIndices
	{
		get
		{
			return _baseVolume.VolumetricPropertyIndices;
		}
		set
		{
			_baseVolume.VolumetricPropertyIndices = value;
		}
	}

	public bool Registered { get; private set; }

	public IReadOnlySignal<GameObject> OnReleaseOrDestroySignal => _destroyedSignal;

	public Signal RegisteredSignal { get; private set; }

	public Signal UnregisteredSignal { get; private set; }

	public FreeVolume()
	{
		RegisteredSignal = new Signal();
		UnregisteredSignal = new Signal();
		_destroyedSignal = new Signal<GameObject>();
	}

	public void SetConfiguration(XYZ position, int rotationIndex, int subConfig)
	{
		Position = position;
		if (rotationIndex != RotationIndex || subConfig != SubConfiguration)
		{
			_rotationIndex = rotationIndex;
			_subConfiguration = subConfig;
			RegenerateMappings();
		}
	}

	public void SetConfiguration(Factory.AssetKey key, XYZ position, int rotationIndex, int subConfig)
	{
		Position = position;
		if (rotationIndex != RotationIndex || subConfig != SubConfiguration || key != AssetKey)
		{
			AssetKey = key;
			_rotationIndex = rotationIndex;
			_subConfiguration = subConfig;
			RegenerateMappings();
		}
	}

	public static FreeVolume Make(Factory.AssetKey key, XYZ p)
	{
		return Make(key, p, 0);
	}

	public static FreeVolume Make(Factory.AssetKey assetKey, XYZ position, int rotationIndex, int subConfiguration = 0, Faction f = null)
	{
		FreeVolume freeVolume = ObjectPool<FreeVolume>.Reserve();
		freeVolume.AssetKey = assetKey;
		freeVolume.Position = position;
		freeVolume._rotationIndex = rotationIndex;
		freeVolume._subConfiguration = subConfiguration;
		freeVolume.Faction = ((!(f != null)) ? Faction.DefaultNeutral : f);
		freeVolume.RegenerateMappings();
		freeVolume._disposed = false;
		Volume.NewVolumeSignal.Invoke(freeVolume);
		return freeVolume;
	}

	private ValidationData GenerateValidationData()
	{
		GameObject gameObject = Factory.Peek(AssetKey);
		Blueprint component = gameObject.GetComponent<Blueprint>();
		ValidatorComponent component2 = gameObject.GetComponent<ValidatorComponent>();
		AbstractValidator validator = component2.Validator;
		int maxMissingSupport = component2.MaxMissingSupport;
		return new ValidationData(Position, RotationIndex, this, validator, maxMissingSupport, Faction, null, component);
	}

	private void RegenerateMappings()
	{
		_baseVolume.IndexedVolumetricProperties = VolumePreprocessor.GetIndexedVSettings(AssetKey, RotationIndex, _subConfiguration);
		_baseVolume.VolumetricPropertyIndices = VolumePreprocessor.GetVSettingsIndices(AssetKey, RotationIndex, _subConfiguration);
	}

	public VolumePropertyRequest GetCases(VProperty p)
	{
		return _baseVolume.GetCases(p);
	}

	public VolumeMultiPropertyRequest GetCases(VPropertyMask mask)
	{
		return _baseVolume.GetCases(mask);
	}

	public bool HasProperty(VProperty p)
	{
		return _baseVolume.HasProperty(p);
	}

	public bool HasProperties(VPropertyMask p)
	{
		return _baseVolume.HasProperties(p);
	}

	public bool HasAnyProperties(VPropertyMask p)
	{
		return _baseVolume.HasAnyProperties(p);
	}

	public bool HasPropertyAt(VProperty p, XYZ position)
	{
		return _baseVolume.HasPropertyAt(p, position);
	}

	public bool HasPropertyAt(VProperty p, IEnumerable<XYZ> positions)
	{
		return _baseVolume.HasPropertyAt(p, positions);
	}

	public bool HasPropertiesAt(VPropertyMask p, XYZ position)
	{
		return _baseVolume.HasPropertiesAt(p, position);
	}

	public bool HasPropertiesAt(VPropertyMask p, IEnumerable<XYZ> positions)
	{
		return _baseVolume.HasPropertiesAt(p, positions);
	}

	public bool HasAnyPropertiesAt(VPropertyMask p, XYZ position)
	{
		return _baseVolume.HasAnyPropertiesAt(p, position);
	}

	public bool HasAnyPropertiesAt(VPropertyMask p, IEnumerable<XYZ> positions)
	{
		return _baseVolume.HasAnyPropertiesAt(p, positions);
	}

	public bool HasPropertyInAny(VProperty p, IEnumerable<XYZ> positions)
	{
		return _baseVolume.HasPropertyInAny(p, positions);
	}

	public bool HasPropertiesInAny(VPropertyMask p, IEnumerable<XYZ> positions)
	{
		return _baseVolume.HasPropertiesInAny(p, positions);
	}

	public bool HasAnyPropertiesInAny(VPropertyMask p, IEnumerable<XYZ> indices)
	{
		return _baseVolume.HasAnyPropertiesInAny(p, indices);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public IEnumerator<KeyValuePair<XYZ, VPropertyMask>> GetEnumerator()
	{
		return _baseVolume.GetIndexedVProperiesEnumerator();
	}

	public IEnumerator<KeyValuePair<XYZ, VPropertyMask>> GetEnumerator(XYZ position)
	{
		return _baseVolume.GetIndexedVProperiesEnumerator(position);
	}

	public ValidationResult Validate()
	{
		ValidationData vData = GenerateValidationData();
		return vData.Validator.Validate(ref vData);
	}

	public void Register()
	{
		VolumeRegistrator.Instance.RegisterVolume(this);
		Registered = true;
		RegisteredSignal.Invoke();
	}

	public void Unregister()
	{
		if (Registered)
		{
			VolumeRegistrator.Instance.UnregisterVolume(this);
		}
		Registered = false;
		UnregisteredSignal.Invoke();
	}

	public void ResetObject()
	{
		if (_disposed)
		{
			logger.Error("Releasing already disposed free volume");
			return;
		}
		_disposed = true;
		_destroyedSignal.Clear();
		RegisteredSignal.Clear();
		UnregisteredSignal.Clear();
		AssetKey = null;
		Faction = null;
		_rotationIndex = 0;
		_subConfiguration = 0;
		_baseVolume.Reset();
		Registered = false;
	}

	public void Dispose()
	{
		_destroyedSignal.Invoke(null);
		ObjectPool<FreeVolume>.Release(this);
	}

	public T GetComponent<T>()
	{
		return default(T);
	}
}
