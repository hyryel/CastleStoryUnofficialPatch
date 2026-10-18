using System;
using Brix.External.Factories;
using Brix.Game.AI.Damage;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Brix.Utils;
using UnityEngine;

namespace Brix.Engine;

public interface IVoxel : IPoolable, IResetable, IDisposable
{
	XYZ Position { get; }

	Vector3 AbsolutePivotPosition { get; }

	Quaternion Rotation { get; }

	Quaternion CanonicalRotation { get; }

	BlockInfo BlockInfo { get; }

	MegaVoxel MegaVoxel { get; }

	IVoxel RootOrSelf { get; }

	int Niveaux { get; }

	bool IsDamaged { get; }

	float Damage { get; set; }

	IDamageReceiver DamageReceiver { get; }

	ValidationData ValidationData { get; }

	IVolume Volume { get; }

	bool IsCrust { get; }

	bool IsNull { get; }

	bool IsTerrain { get; }

	bool IsInvincible { get; }

	bool IsMultiBlock { get; }

	void DestroyBlock(bool withParticles, bool withGarnotte);

	void DeBlock(int blockId = -1);

	bool IsOfType(Factory.AssetKey assetKey);

	bool IsOfType(int blockInfoTypeId);

	void Physicalize();

	void ReleaseToPool();

	void DeBlockClient(int id);
}
