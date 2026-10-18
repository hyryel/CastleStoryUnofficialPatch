using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Factories;
using Brix.Input;
using Brix.Util;
using UnityEngine;

namespace Brix.Game.Components;

public abstract class AbstractFitter : MonoBehaviour, IFitter
{
	[SerializeField]
	protected bool _lockRotation;

	[SerializeField]
	protected bool _oneRotationPerAxis;

	protected Quaternion _rotation = Quaternion.identity;

	protected static Quaternion _rotation90 = Quaternion.Euler(0f, 90f, 0f);

	protected static HashSet<Factory.AssetKey> _fitOnLogKeys = new HashSet<Factory.AssetKey> { Blocks.Wood_SingleLog };

	private static Vector3[] _voxelSides = new Vector3[4]
	{
		Vector3.forward,
		Vector3.right,
		Vector3.back,
		Vector3.left
	};

	protected static HashSet<Factory.AssetKey> _fitOnPlugKeys = new HashSet<Factory.AssetKey>
	{
		Blocks.Stone_BeamPlug_AllSides,
		Blocks.Stone_BeamPlug_Corner,
		Blocks.Stone_BeamPlug_OneSide,
		Blocks.Stone_BeamPlug_TwoSides
	};

	public void FitAndOrient(VoxelRaycastHit voxelHit)
	{
		FitAndOrient(voxelHit, out var _);
	}

	public virtual void FitAndOrient(VoxelRaycastHit voxelHit, out Vector3 absolutePosition)
	{
		Vector3 vector = FindFitPosition(voxelHit);
		base.gameObject.transform.rotation = _rotation;
		base.gameObject.transform.position = vector;
		absolutePosition = vector;
	}

	protected virtual Vector3 FindFitPosition(VoxelRaycastHit voxelHit)
	{
		if (IsFitInCrust(voxelHit))
		{
			return voxelHit.VoxelPosition.ToVector3();
		}
		if (voxelHit.HitBlockInfo.IsNotNull && _fitOnLogKeys.Contains(voxelHit.HitBlockInfo.Key))
		{
			return FitOnLog(voxelHit);
		}
		if (voxelHit.HitBlockInfo.IsNotNull && _fitOnPlugKeys.Contains(voxelHit.HitBlockInfo.Key))
		{
			return FitOnPlug(voxelHit);
		}
		return voxelHit.VoxelNormal.ToVector3();
	}

	protected bool IsFitInCrust(VoxelRaycastHit voxelHit)
	{
		return Voxel.IsCrust(voxelHit.VoxelPosition) && Voxel.GetCubeFantome(voxelHit.VoxelPosition) == null;
	}

	public virtual void ChangeFit()
	{
		if (_lockRotation)
		{
			return;
		}
		if (_oneRotationPerAxis)
		{
			if (_rotation == Quaternion.identity)
			{
				_rotation = _rotation90;
			}
			else
			{
				_rotation = Quaternion.identity;
			}
		}
		else
		{
			_rotation *= Quaternion.Euler(0f, 90f, 0f);
		}
	}

	public virtual void FitForDrag(VoxelRaycastHit voxelHit)
	{
		FitForDrag(voxelHit, out var _);
	}

	public virtual void FitForDrag(VoxelRaycastHit voxelHit, out Vector3 absolutePosition)
	{
		FitAndOrient(voxelHit, out absolutePosition);
	}

	public virtual void CloneInternals(IFitter original)
	{
		AbstractFitter abstractFitter = original as AbstractFitter;
		_lockRotation = abstractFitter._lockRotation;
		_oneRotationPerAxis = abstractFitter._oneRotationPerAxis;
		if (abstractFitter != null && !_lockRotation)
		{
			if (_oneRotationPerAxis)
			{
				_rotation = Quaternion.LookRotation(Calcule.AbsVecteur(abstractFitter._rotation * Vector3.forward), abstractFitter._rotation * Vector3.up);
			}
			else
			{
				_rotation = abstractFitter._rotation;
			}
		}
	}

	public virtual IFitter CopyTo(GameObject go)
	{
		Object.Destroy((MonoBehaviour)go.GetInterface<IFitter>());
		DefaultFitter defaultFitter = go.AddComponent<DefaultFitter>();
		defaultFitter.CloneInternals(this);
		return defaultFitter;
	}

	protected virtual Vector3 FitOnLog(VoxelRaycastHit voxelHit)
	{
		if (!voxelHit.IsRaycastResult)
		{
			return voxelHit.HitVector;
		}
		XYZ voxelNormal = voxelHit.VoxelNormal;
		if (voxelNormal == voxelHit.VoxelPosition + XYZ.up || voxelNormal == voxelHit.VoxelPosition - XYZ.up)
		{
			return voxelNormal.ToVector3();
		}
		int num = 0;
		for (int i = 1; i < _voxelSides.Length; i++)
		{
			Vector3 vector = voxelHit.HitVector + _voxelSides[i];
			Vector3 vector2 = voxelHit.HitVector + _voxelSides[num];
			if ((vector2 - voxelHit.WorldHitVector).magnitude > (vector - voxelHit.WorldHitVector).magnitude)
			{
				num = i;
			}
		}
		return voxelHit.HitVector + _voxelSides[num];
	}

	protected virtual Vector3 FitOnPlug(VoxelRaycastHit voxelHit)
	{
		if (!voxelHit.IsRaycastResult)
		{
			return voxelHit.HitVector;
		}
		if ((double)voxelHit.LocalHitVector.y > 0.49)
		{
			return voxelHit.VoxelNormal.ToVector3();
		}
		int num = 0;
		for (int i = 1; i < _voxelSides.Length; i++)
		{
			Vector3 vector = voxelHit.HitVector + _voxelSides[i];
			Vector3 vector2 = voxelHit.HitVector + _voxelSides[num];
			if ((vector2 - voxelHit.WorldHitVector).magnitude > (vector - voxelHit.WorldHitVector).magnitude)
			{
				num = i;
			}
		}
		return voxelHit.HitVector + _voxelSides[num];
	}
}
