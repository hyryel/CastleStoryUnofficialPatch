using System;
using Brix.External.Factories;
using Brix.Game.AI.Damage;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Brix.Utils;
using UnityEngine;

namespace Brix.Engine;

public class NullBlockData : IBlockData, IVoxel, IPoolable, IResetable, IDisposable
{
	private static NullBlockData __instance = new NullBlockData();

	IVoxel IVoxel.RootOrSelf => this;

	public bool IsNull => true;

	public bool IsTerrain => false;

	public bool IsMultiBlock => false;

	public XYZ Position => XYZ.zero;

	public Quaternion Rotation => Quaternion.identity;

	public Quaternion CanonicalRotation => Quaternion.identity;

	public IDamageReceiver DamageReceiver => NullDamageReceiver.Get;

	public ValidationData ValidationData => ValidationData.None;

	public IVolume Volume => NullVolume.Get;

	public IBlockData RootOrSelf => this;

	public Vector3 AbsolutePivotPosition => Vector3.zero;

	public BlockInfo BlockInfo => NullBlockInfo.Instance;

	public MegaVoxel MegaVoxel => null;

	public int Niveaux => 0;

	public bool IsDamaged => false;

	public bool IsInvincible => false;

	public float Damage
	{
		get
		{
			return 0f;
		}
		set
		{
		}
	}

	public bool IsCrust => false;

	private NullBlockData()
	{
	}

	public static NullBlockData Make()
	{
		return __instance;
	}

	public bool IsOfType(Factory.AssetKey assetKey)
	{
		return false;
	}

	public bool IsOfType(int blockInfoTypeId)
	{
		return false;
	}

	public void ResetObject()
	{
	}

	public void Dispose()
	{
	}

	public void ReleaseToPool()
	{
	}

	public void DeBlockClient(int id)
	{
	}

	public bool MatchesGameObject(GameObject go)
	{
		return false;
	}

	public bool RemoveBlockWithMatchingRoot(BlockData rootBD)
	{
		return false;
	}

	public void DestroyBlock(bool withParticles, bool withGarnotte)
	{
	}

	public void DeBlock(int blockId)
	{
	}

	public void RedrawVoxel(MaterialCombineDispatcher materialCombineDispatcher)
	{
	}

	public void Physicalize()
	{
	}
}
