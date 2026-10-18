using Brix.Engine;
using Brix.Game.Components.Volumes;
using Brix.Input;
using UnityEngine;

namespace Brix.Game.Components;

public class PlankFitter : AbstractFitter
{
	protected CompCache<VoxelTransform> _voxelTransform;

	private void Awake()
	{
		_voxelTransform = new CompCache<VoxelTransform>(base.gameObject);
	}

	protected override Vector3 FindFitPosition(VoxelRaycastHit voxelHit)
	{
		if (IsFitInCrust(voxelHit) || HasFreeSpotOnBeam(voxelHit.VoxelPosition))
		{
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

	protected bool HasFreeSpotOnBeam(XYZ position)
	{
		return VolumeRegistry.HasVolumesWithPropertyAt(position, VProperty.beamSupport) && !VolumeRegistry.HasVolumesWithPropertyAtExcluding(position, VProperty.plankSupport, GetComponent<IVolume>());
	}

	public override IFitter CopyTo(GameObject go)
	{
		Object.Destroy((MonoBehaviour)go.GetInterface<IFitter>());
		PlankFitter plankFitter = go.AddComponent<PlankFitter>();
		plankFitter.CloneInternals(this);
		return plankFitter;
	}
}
