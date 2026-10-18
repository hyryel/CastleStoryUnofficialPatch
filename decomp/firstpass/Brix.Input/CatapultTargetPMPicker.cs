namespace Brix.Input;

internal class CatapultTargetPMPicker : PickerComponent
{
	public override object GetPieMenuTarget()
	{
		return _voxelRayHit.HitGameObject;
	}
}
