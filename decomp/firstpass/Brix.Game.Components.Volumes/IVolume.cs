using System;
using System.Collections;
using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.AI.Damage;
using UnityEngine;

namespace Brix.Game.Components.Volumes;

public interface IVolume : IDisposable, IEnumerable<KeyValuePair<XYZ, VPropertyMask>>, IEnumerable
{
	Factory.AssetKey AssetKey { get; }

	XYZ Position { get; }

	int RotationIndex { get; }

	Quaternion Rotation { get; }

	GameObject GameObject { get; }

	IGameComponent GameComponent { get; }

	IPassThrough PassThrough { get; }

	IDamageReceiver DamageReceiver { get; }

	BaseVolume BaseVolume { get; }

	VolumePropertyRequest access { get; }

	VolumePropertyRequest blueprint { get; }

	VolumePropertyRequest obstacles { get; }

	bool Registered { get; }

	Signal RegisteredSignal { get; }

	Signal UnregisteredSignal { get; }

	IReadOnlySignal<GameObject> OnReleaseOrDestroySignal { get; }

	ValidationData VData { get; }

	Faction Faction { get; set; }

	string Name { get; }

	VolumePropertyRequest GetCases(VProperty p);

	VolumeMultiPropertyRequest GetCases(VPropertyMask mask);

	ValidationResult Validate();

	bool HasProperty(VProperty p);

	bool HasPropertyAt(VProperty p, XYZ position);

	bool HasAnyProperties(VPropertyMask p);

	bool HasPropertyInAny(VProperty p, IEnumerable<XYZ> positions);

	bool HasPropertyAt(VProperty p, IEnumerable<XYZ> positions);

	bool HasPropertiesAt(VPropertyMask hp, XYZ position);

	bool HasAnyPropertiesAt(VPropertyMask hp, XYZ position);

	void Register();

	void Unregister();
}
