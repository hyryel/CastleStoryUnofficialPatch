using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Brix.Engine;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.AI.Damage;
using UnityEngine;

namespace Brix.Game.Components.Volumes;

public class NullVolume : IVolume, IDisposable, IEnumerable<KeyValuePair<XYZ, VPropertyMask>>, IEnumerable
{
	private static NullVolume _instance = new NullVolume();

	private IEnumerator<KeyValuePair<XYZ, VPropertyMask>> emptyEnumerator = Enumerable.Empty<KeyValuePair<XYZ, VPropertyMask>>().GetEnumerator();

	private Signal _bidonSignal = new Signal();

	private Signal<GameObject> _bidonSignal2 = new Signal<GameObject>();

	public static NullVolume Get => _instance;

	public IPassThrough PassThrough => PassThroughObstacle.Null;

	public IDamageReceiver DamageReceiver => NullDamageReceiver.Get;

	public VolumePropertyRequest access => VolumePropertyRequest.Empty;

	public VolumePropertyRequest blueprint => VolumePropertyRequest.Empty;

	public XYZ Position => XYZ.zero;

	public int RotationIndex => 0;

	public Quaternion Rotation => Quaternion.identity;

	public VolumePropertyRequest obstacles => VolumePropertyRequest.Empty;

	public BaseVolume BaseVolume => new BaseVolume(null);

	public string Name => "Null volume";

	public GameObject GameObject => null;

	public IGameComponent GameComponent => null;

	public ValidationData VData { get; private set; }

	public Factory.AssetKey AssetKey => null;

	public Faction Faction
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public bool Registered => false;

	public Signal RegisteredSignal => _bidonSignal;

	public Signal UnregisteredSignal => _bidonSignal;

	public IReadOnlySignal<GameObject> OnReleaseOrDestroySignal => _bidonSignal2;

	private NullVolume()
	{
	}

	public VolumePropertyRequest GetCases(VProperty p)
	{
		return VolumePropertyRequest.Empty;
	}

	public VolumeMultiPropertyRequest GetCases(VPropertyMask mask)
	{
		return VolumeMultiPropertyRequest.Empty;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public IEnumerator<KeyValuePair<XYZ, VPropertyMask>> GetEnumerator()
	{
		return emptyEnumerator;
	}

	public bool HasPropertiesAt(VPropertyMask hp, XYZ position)
	{
		return false;
	}

	public bool HasAnyProperties(VPropertyMask p)
	{
		return false;
	}

	public bool HasAnyPropertiesAt(VPropertyMask hp, XYZ position)
	{
		return false;
	}

	public bool HasProperty(VProperty p)
	{
		return false;
	}

	public bool HasPropertyAt(VProperty p, XYZ position)
	{
		return false;
	}

	public bool HasPropertyAt(VProperty p, IEnumerable<XYZ> indices)
	{
		return false;
	}

	public bool HasPropertyInAny(VProperty p, IEnumerable<XYZ> indices)
	{
		return false;
	}

	public ValidationResult Validate()
	{
		return ValidationResult.Invalid;
	}

	public void Register()
	{
	}

	public void Unregister()
	{
	}

	public void Dispose()
	{
	}

	public T GetComponent<T>()
	{
		return default(T);
	}
}
