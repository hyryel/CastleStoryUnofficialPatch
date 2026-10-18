using Brix.Engine;
using Brix.External.Factories;
using Brix.Game.Components.Volumes;
using UnityEngine;

namespace Brix.Game.Components;

public class BarrelDOValidator : DefaultDOValidator
{
	private static VPropertyMask _supportSettings = VPropertyMask.stoneSupport | VPropertyMask.logSupport | VPropertyMask.plankSupport;

	protected override VPropertyMask SupportSettings => _supportSettings;

	protected override bool HasGroundSupportAt(ref ValidationData vData, XYZ i)
	{
		using IVoxel voxel = Voxel.GetTerrainBlockData(i);
		XYZ position = vData.Volume.Position;
		GameObject gameObject = Factory.Peek(vData.Volume.AssetKey);
		Vector3 offsettedPosition = gameObject.GetComponent<BaseVoxelTransform>().GetOffsettedPosition(vData.Volume.Position.ToVector3(), vData.Volume.Rotation);
		return !voxel.IsNull && (!voxel.IsCrust || (float)position.y < offsettedPosition.y);
	}

	public override void SetUnstable(ref ValidationData vData)
	{
		vData.Volume.Unregister();
		PropagateRevalidation(ref vData);
	}

	public override AbstractValidator GetBPValidatorCounterpart()
	{
		return SingletonSpawner.Get<BarrelBPValidator>();
	}
}
