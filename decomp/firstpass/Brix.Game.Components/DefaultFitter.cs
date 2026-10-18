using Brix.Engine;
using Brix.Input;
using UnityEngine;

namespace Brix.Game.Components;

public class DefaultFitter : AbstractFitter
{
	[SerializeField]
	protected bool _baseOnTopOfGround;

	protected CompCache<VoxelTransform> _voxelTransform;

	protected virtual void Awake()
	{
		_voxelTransform = new CompCache<VoxelTransform>(base.gameObject);
	}

	public override void FitAndOrient(VoxelRaycastHit voxelHit, out Vector3 absolutePosition)
	{
		Vector3 vector = FindFitPosition(voxelHit);
		_voxelTransform.Get.PlacementOffset = GetPlacementOffset(voxelHit);
		base.gameObject.transform.rotation = _rotation;
		base.gameObject.transform.position = _voxelTransform.Get.GetOffsettedPosition(vector) + _voxelTransform.Get.PlacementOffset;
		absolutePosition = vector;
	}

	protected override Vector3 FindFitPosition(VoxelRaycastHit voxelHit)
	{
		if (IsFitInCrust(voxelHit))
		{
			if (_baseOnTopOfGround)
			{
				return Architecte.SurfaceRaycast(voxelHit.VoxelPosition.ToVector3()) + new Vector3(0f, 0.5f, 0f);
			}
			return voxelHit.VoxelPosition.ToVector3();
		}
		if (voxelHit.HitBlockInfo.IsNotNull && AbstractFitter._fitOnLogKeys.Contains(voxelHit.HitBlockInfo.Key))
		{
			return FitOnLog(voxelHit);
		}
		if (voxelHit.HitBlockInfo.IsNotNull && AbstractFitter._fitOnPlugKeys.Contains(voxelHit.HitBlockInfo.Key))
		{
			return FitOnPlug(voxelHit);
		}
		return voxelHit.VoxelNormal.ToVector3();
	}

	protected Vector3 GetPlacementOffset(VoxelRaycastHit voxelHit)
	{
		if (_baseOnTopOfGround && IsFitInCrust(voxelHit))
		{
			return new Vector3(0f, -0.5f, 0f);
		}
		return Vector3.zero;
	}

	public override void CloneInternals(IFitter original)
	{
		base.CloneInternals(original);
		DefaultFitter defaultFitter = original as DefaultFitter;
		if (defaultFitter != null)
		{
			_baseOnTopOfGround = defaultFitter._baseOnTopOfGround;
		}
	}

	public override IFitter CopyTo(GameObject go)
	{
		Object.Destroy((MonoBehaviour)go.GetInterface<IFitter>());
		DefaultFitter defaultFitter = go.AddComponent<DefaultFitter>();
		defaultFitter.CloneInternals(this);
		return defaultFitter;
	}
}
