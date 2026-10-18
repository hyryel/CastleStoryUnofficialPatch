using Brix.Engine;
using Brix.Input;
using UnityEngine;

namespace Brix.Game.Components;

public class RopeBridgePlankFitter : AbstractFitter
{
	public override void FitAndOrient(VoxelRaycastHit voxelHit, out Vector3 absolutePosition)
	{
		RopeBridgeConfigurator ropeBridge = GetRopeBridge(voxelHit);
		if (ropeBridge == null)
		{
			base.gameObject.transform.rotation = _rotation;
			base.gameObject.transform.position = voxelHit.VoxelNormal.ToVector3();
		}
		else
		{
			ropeBridge.GetPlankTransform(voxelHit.VoxelNormal, out var position, out var rotation);
			base.gameObject.transform.rotation = rotation;
			base.gameObject.transform.position = position;
		}
		absolutePosition = base.gameObject.transform.position;
	}

	public override IFitter CopyTo(GameObject go)
	{
		Object.Destroy((MonoBehaviour)go.GetInterface<IFitter>());
		RopeBridgePlankFitter ropeBridgePlankFitter = go.AddComponent<RopeBridgePlankFitter>();
		ropeBridgePlankFitter.CloneInternals(this);
		return ropeBridgePlankFitter;
	}

	private RopeBridgeConfigurator GetRopeBridge(VoxelRaycastHit voxelHit)
	{
		XYZ voxelNormal = voxelHit.VoxelNormal;
		RopeBridgeConfigurator ropeBridgeFromPlankAt = RopeBridgeConfigurator.GetRopeBridgeFromPlankAt(voxelNormal);
		if (ropeBridgeFromPlankAt == null)
		{
			ropeBridgeFromPlankAt = RopeBridgeConfigurator.GetRopeBridgeFromPlankAt(voxelNormal + XYZ.up);
		}
		if (ropeBridgeFromPlankAt == null)
		{
			ropeBridgeFromPlankAt = RopeBridgeConfigurator.GetRopeBridgeFromPlankAt(voxelNormal - XYZ.up);
		}
		return ropeBridgeFromPlankAt;
	}
}
