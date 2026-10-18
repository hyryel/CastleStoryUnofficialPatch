using System;
using System.Collections;
using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Game.AI.Damage;
using Brix.Network;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Components.Volumes;

public abstract class Volume : NetworkGameComponent, IVolume, IDisposable, IEnumerable<KeyValuePair<XYZ, VPropertyMask>>, IEnumerable
{
	public static Signal<IVolume> NewVolumeSignal = new Signal<IVolume>();

	protected FreeVolume _internalVolume;

	public virtual FreeVolume InternalVolume
	{
		get
		{
			_internalVolume.SetConfiguration(AssetKey, Position, RotationIndex, SubConfiguration);
			return _internalVolume;
		}
		set
		{
			_internalVolume = value;
		}
	}

	public Signal RegisteredSignal { get; private set; }

	public Signal UnregisteredSignal { get; private set; }

	public bool Registered { get; protected set; }

	public IGameComponent GameComponent => this;

	public IPassThrough PassThrough { get; private set; }

	public IDamageReceiver DamageReceiver => base.damageReceiver;

	public virtual ValidationData VData => GetComponent<ValidatorComponent>().VData;

	public virtual XYZ Position => base.voxel.position;

	public virtual int SubConfiguration => 0;

	public virtual Faction Faction
	{
		get
		{
			return faction;
		}
		set
		{
			faction = value;
		}
	}

	public virtual int RotationIndex => base.voxel.RotationIndex;

	public virtual Quaternion Rotation => base.transform.rotation;

	public virtual VolumePropertyRequest access => InternalVolume.access;

	public virtual VolumePropertyRequest blueprint => InternalVolume.blueprint;

	public virtual VolumePropertyRequest obstacles => InternalVolume.obstacles;

	public BaseVolume BaseVolume => _internalVolume.BaseVolume;

	public IEnumerator<KeyValuePair<XYZ, VPropertyMask>> GetEnumerator()
	{
		return InternalVolume.GetEnumerator(Position);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	[ContextMenu("Force Volume Registration")]
	public virtual void Register()
	{
		VolumeRegistrator.Instance.RegisterVolume(this);
		Registered = true;
		RegisteredSignal.Invoke();
		ApplyDeblocProperties();
	}

	[ContextMenu("Force Volume Unregistration")]
	public virtual void Unregister()
	{
		if (Registered)
		{
			VolumeRegistrator.Instance.UnregisterVolume(this);
			UnregisteredSignal.Invoke();
		}
		Registered = false;
	}

	public virtual bool HasProperty(VProperty p)
	{
		return InternalVolume.HasProperty(p);
	}

	public virtual bool HasProperties(VPropertyMask p)
	{
		return InternalVolume.HasProperties(p);
	}

	public virtual bool HasAnyProperties(VPropertyMask p)
	{
		return InternalVolume.HasAnyProperties(p);
	}

	public virtual bool HasPropertyAt(VProperty p, XYZ position)
	{
		return InternalVolume.HasPropertyAt(p, position);
	}

	public virtual bool HasPropertyAt(VProperty p, IEnumerable<XYZ> positions)
	{
		return InternalVolume.HasPropertyAt(p, positions);
	}

	public virtual bool HasPropertiesAt(VPropertyMask p, XYZ position)
	{
		return InternalVolume.HasPropertiesAt(p, position);
	}

	public virtual bool HasPropertiesAt(VPropertyMask p, IEnumerable<XYZ> positions)
	{
		return InternalVolume.HasPropertiesAt(p, positions);
	}

	public virtual bool HasAnyPropertiesAt(VPropertyMask p, XYZ position)
	{
		return InternalVolume.HasAnyPropertiesAt(p, position);
	}

	public virtual bool HasAnyPropertiesAt(VPropertyMask p, IEnumerable<XYZ> positions)
	{
		return InternalVolume.HasAnyPropertiesAt(p, positions);
	}

	public virtual bool HasPropertyInAny(VProperty p, IEnumerable<XYZ> positions)
	{
		return InternalVolume.HasPropertyInAny(p, positions);
	}

	public virtual bool HasPropertiesInAny(VPropertyMask p, IEnumerable<XYZ> positions)
	{
		return InternalVolume.HasPropertiesInAny(p, positions);
	}

	public virtual bool HasAnyPropertiesInAny(VPropertyMask p, IEnumerable<XYZ> positions)
	{
		return InternalVolume.HasAnyPropertiesInAny(p, positions);
	}

	public virtual VolumePropertyRequest GetCases(VProperty p)
	{
		return InternalVolume.GetCases(p);
	}

	public virtual VolumeMultiPropertyRequest GetCases(VPropertyMask mask)
	{
		return InternalVolume.GetCases(mask);
	}

	public virtual ValidationResult Validate()
	{
		ValidatorComponent component = GetComponent<ValidatorComponent>();
		if (component != null)
		{
			return component.Validate();
		}
		return ValidationResult.Valid;
	}

	public override void Initialize()
	{
		base.Initialize();
		RegisteredSignal = new Signal();
		UnregisteredSignal = new Signal();
		PassThrough = GetComponent<IPassThrough>() ?? PassThroughObstacle.Null;
	}

	public override void OnRequested()
	{
		base.OnRequested();
		NewVolumeSignal.Invoke(this);
		InternalVolume = FreeVolume.Make(AssetKey, Position, base.voxel.RotationIndex);
		ApplyDeblocProperties();
	}

	public void ApplyDeblocProperties()
	{
		if (NetworkServer.active)
		{
			GetCases(VProperty.debloc).Slinq().Select(Voxel.GetVoxel).ForEach(delegate(IVoxel bd)
			{
				bd.DeBlock();
			});
			GetCases(VProperty.deblocTerrain).Slinq().Select(Voxel.GetTerrainBlockData).ForEach(delegate(IVoxel bd)
			{
				bd.DeBlock();
			});
		}
	}

	public override void OnStartAnywhere()
	{
		base.OnStartAnywhere();
		InternalVolume.SetConfiguration(Position, RotationIndex, SubConfiguration);
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		Unregister();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		InternalVolume.Dispose();
	}

	public void Dispose()
	{
	}

	IReadOnlySignal<GameObject> IVolume.get_OnReleaseOrDestroySignal()
	{
		return base.OnReleaseOrDestroySignal;
	}

	string IVolume.get_Name()
	{
		return base.Name;
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		bool flag2 = default(bool);
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
	}
}
