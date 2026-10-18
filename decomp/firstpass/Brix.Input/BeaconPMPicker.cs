namespace Brix.Input;

internal class BeaconPMPicker : PickerComponent
{
	public override object GetPieMenuTarget()
	{
		return PickingUtility.GetLayerRootParent(_voxelRayHit.HitGameObject);
	}
}
