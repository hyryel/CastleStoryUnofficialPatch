using Brix.Game.Components;

namespace Brix.Input;

internal class UnitPMPicker : PickerComponent
{
	public override object GetPieMenuTarget()
	{
		return PickingUtility.FindComponentInParents<GameComponent>(_voxelRayHit.HitGameObject, layerwise: false).gameObject;
	}
}
