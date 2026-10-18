using Brix.Game.Components;
using Brix.Game.Components.CatapultSpace;

namespace Brix.Input;

internal class ObjectPMPicker : PickerComponent
{
	public override object GetPieMenuTarget()
	{
		Catapult catapult = PickingUtility.FindComponentInParents<Catapult>(_voxelRayHit.HitGameObject, layerwise: false);
		if (catapult != null)
		{
			return catapult.gameObject;
		}
		SignificantParent significantParent = PickingUtility.FindComponentInParents<SignificantParent>(_voxelRayHit.HitGameObject, layerwise: false);
		if (significantParent != null)
		{
			return significantParent.Get;
		}
		GameComponent gameComponent = PickingUtility.FindComponentInParents<GameComponent>(_voxelRayHit.HitGameObject, layerwise: false);
		if (gameComponent != null)
		{
			return gameComponent.gameObject;
		}
		return null;
	}
}
