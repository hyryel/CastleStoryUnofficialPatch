using Brix.Engine;
using Brix.Engine.Nature;
using Brix.Game;

namespace Brix.Input;

internal class TerrainPMPicker : PickerComponent
{
	public override object GetPieMenuTarget()
	{
		if (_voxelRayHit.HitLayer == UnityLayer.Terrain)
		{
			return _voxelRayHit.VoxelPosition;
		}
		if (_voxelRayHit.HitLayer == UnityLayer.Nature)
		{
			NatureData natureData = PickingUtility.FindHitNature(_voxelRayHit);
			if (natureData.IsValid)
			{
				return natureData;
			}
			XYZ xYZ = PickingUtility.FindHitTree(_voxelRayHit);
			if (xYZ != XYZ.zero)
			{
				return xYZ;
			}
		}
		return null;
	}
}
