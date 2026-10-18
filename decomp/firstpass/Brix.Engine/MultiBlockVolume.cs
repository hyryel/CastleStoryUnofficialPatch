using System;
using System.Collections;
using System.Collections.Generic;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game;
using Brix.Game.AI.Damage;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Brix.Util;
using Brix.Utils;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Engine;

public class MultiBlockVolume : IVolume, IDisposable, IEnumerable<KeyValuePair<XYZ, VPropertyMask>>, IEnumerable
{
	private MultiBlockData _mbd;

	private List<IVolume> _volumes = new List<IVolume>();

	private BaseVolume _internalVolume = new BaseVolume(null);

	private VPropertyMask _vPropertiesMapped;

	private Signal _bidonSignal = new Signal();

	private Signal<GameObject> _bidonSignal2 = new Signal<GameObject>();

	public string Name => "Multiblock volume";

	public XYZ Position => _mbd.Position;

	public IPassThrough PassThrough => PassThroughObstacle.Null;

	public IDamageReceiver DamageReceiver => _mbd.DamageReceiver;

	public VolumePropertyRequest access => GetCases(VProperty.access);

	public VolumePropertyRequest blueprint => GetCases(VProperty.fantome);

	public VolumePropertyRequest obstacles => GetCases(VProperty.obstacle);

	public BaseVolume BaseVolume => _internalVolume;

	public IEnumerable<KeyValuePair<VProperty, VolumePropertyRequest>> VSettingIndices
	{
		get
		{
			throw new InvalidOperationException();
		}
	}

	public GameObject GameObject => null;

	public IGameComponent GameComponent => null;

	public ValidationData VData => ValidationData.None;

	public int RotationIndex => 0;

	public Quaternion Rotation => Quaternion.identity;

	public Faction Faction
	{
		get
		{
			return Faction.DefaultNeutral;
		}
		set
		{
		}
	}

	public Factory.AssetKey AssetKey
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public Dictionary<XYZ, VPropertyMask> IndexedVolumetricProperties { get; private set; }

	public List<XYZ>[] VolumetricPropertyIndices
	{
		get
		{
			return _internalVolume.VolumetricPropertyIndices;
		}
		set
		{
			_internalVolume.VolumetricPropertyIndices = value;
		}
	}

	public bool Registered => false;

	public Signal RegisteredSignal => _bidonSignal;

	public Signal UnregisteredSignal => _bidonSignal;

	public IReadOnlySignal<GameObject> OnReleaseOrDestroySignal => _bidonSignal2;

	public MultiBlockVolume(MultiBlockData mbd)
	{
		_mbd = mbd;
		VolumetricPropertyIndices = new List<XYZ>[64];
	}

	public void Dispose()
	{
	}

	private void GenerateMappingsFor(VPropertyMask vProperty)
	{
		VPropertyMask mask = ~_vPropertiesMapped & vProperty;
		foreach (int item in mask.Bits())
		{
			GenerateMappingFor((VProperty)item);
		}
		_internalVolume.GeneratePropertiesInUse();
	}

	private void GenerateMappingFor(VProperty vProperty)
	{
		List<XYZ> list = VolumetricPropertyIndices[(int)vProperty] ?? CollectionPool.RequestList<XYZ>();
		VolumetricPropertyIndices[(int)vProperty] = list;
		_volumes.Slinq().SelectMany((IVolume v, VProperty p) => v.GetCases(p).Slinq(), vProperty).Distinct()
			.AddTo(list);
		_vPropertiesMapped |= vProperty.ToMask();
	}

	public VolumePropertyRequest GetCases(VProperty p)
	{
		GenerateMappingsFor(p.ToMask());
		return _internalVolume.GetCases(p);
	}

	public VolumeMultiPropertyRequest GetCases(VPropertyMask mask)
	{
		GenerateMappingsFor(mask);
		return new VolumeMultiPropertyRequest(Position, mask, VolumetricPropertyIndices);
	}

	public bool HasProperty(VProperty p)
	{
		return _volumes.Slinq().Any((IVolume v, VProperty _p) => v.HasProperty(_p), p);
	}

	public bool HasPropertyAt(VProperty p, XYZ position)
	{
		foreach (IVolume volume in _volumes)
		{
			if (volume.HasPropertyAt(p, position))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAnyProperties(VPropertyMask props)
	{
		return props.Bits().Slinq().Any((int p, List<IVolume> _vols) => _vols.Slinq().Any((IVolume v, int _p) => v.HasProperty((VProperty)_p), p), _volumes);
	}

	public bool HasPropertyInAny(VProperty p, IEnumerable<XYZ> positions)
	{
		foreach (XYZ position in positions)
		{
			for (int i = 0; i < _volumes.Count; i++)
			{
				if (_volumes[i].HasPropertyAt(p, position))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasPropertyAt(VProperty p, IEnumerable<XYZ> positions)
	{
		for (int i = 0; i < _volumes.Count; i++)
		{
			if (_volumes[i].HasPropertyAt(p, positions))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasPropertiesAt(VPropertyMask p, XYZ position)
	{
		for (int i = 0; i < _volumes.Count; i++)
		{
			if (_volumes[i].HasPropertiesAt(p, position))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAnyPropertiesAt(VPropertyMask p, XYZ position)
	{
		for (int i = 0; i < _volumes.Count; i++)
		{
			if (_volumes[i].HasAnyPropertiesAt(p, position))
			{
				return true;
			}
		}
		return false;
	}

	public ValidationResult Validate()
	{
		return _volumes.Slinq().Aggregate((ValidationResult)0, (ValidationResult result, IVolume v) => v.Validate() | result);
	}

	public void Register()
	{
		throw new InvalidOperationException();
	}

	public void Unregister()
	{
		throw new InvalidOperationException();
	}

	public void Reset()
	{
		_vPropertiesMapped = ~VPropertyMask.All;
		LongBitsEnumerator enumerator = _vPropertiesMapped.Bits().GetEnumerator();
		while (enumerator.MoveNext())
		{
			int current = enumerator.Current;
			VolumetricPropertyIndices[current].Dispose();
			VolumetricPropertyIndices[current] = null;
		}
		_volumes.Clear();
		(from bd in _mbd.ChildBlocks.Slinq()
			select bd.Volume).AddTo(_volumes);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public IEnumerator<KeyValuePair<XYZ, VPropertyMask>> GetEnumerator()
	{
		throw new NotImplementedException();
	}

	public T GetComponent<T>()
	{
		return default(T);
	}
}
