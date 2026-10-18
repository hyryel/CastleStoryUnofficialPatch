using Brix.Engine;
using Brix.Engine.Nature;
using Brix.Game;
using Brix.Game.Components;
using Brix.Game.Components.CatapultSpace;
using UnityEngine;

namespace Brix.Input;

internal class BlueprintPMPicker : PickerComponent
{
	private LayerBundle unitLayer = new LayerBundle(22);

	private LayerBundle objectLayer = new LayerBundle(16);

	private LayerBundle terrainLayer = new LayerBundle(10, 17);

	public override object GetPieMenuTarget()
	{
		Blueprint blueprint = PickingUtility.FindComponentInParents<Blueprint>(_voxelRayHit.HitGameObject);
		StateRenderer component = blueprint.GetComponent<StateRenderer>();
		if (component.IsVisible)
		{
			return blueprint;
		}
		_utilityVRH.PlaceAtRaycast(Camera.main.ScreenPointToRay(PickingUtility.Player.controllers.Mouse.screenPosition), Picking.Instance.MaxDistance, unitLayer);
		if (_utilityVRH.HasHit)
		{
			return PickingUtility.FindComponentInParents<GameComponent>(_utilityVRH.HitGameObject, layerwise: false).gameObject;
		}
		_utilityVRH.PlaceAtRaycast(Camera.main.ScreenPointToRay(PickingUtility.Player.controllers.Mouse.screenPosition), Picking.Instance.MaxDistance, objectLayer);
		if (_utilityVRH.HasHit)
		{
			Catapult catapult = PickingUtility.FindComponentInParents<Catapult>(_utilityVRH.HitGameObject, layerwise: false);
			if (catapult != null)
			{
				return catapult.gameObject;
			}
			SignificantParent significantParent = PickingUtility.FindComponentInParents<SignificantParent>(_utilityVRH.HitGameObject, layerwise: false);
			if (significantParent != null)
			{
				return significantParent.Get;
			}
			GameComponent gameComponent = PickingUtility.FindComponentInParents<GameComponent>(_utilityVRH.HitGameObject, layerwise: false);
			if (gameComponent != null)
			{
				return gameComponent.gameObject;
			}
			return null;
		}
		_utilityVRH.PlaceAtRaycast(Camera.main.ScreenPointToRay(PickingUtility.Player.controllers.Mouse.screenPosition), Picking.Instance.MaxDistance, terrainLayer);
		if (_utilityVRH.HasHit)
		{
			if (_utilityVRH.HitLayer == UnityLayer.Terrain)
			{
				return _utilityVRH.VoxelPosition;
			}
			if (_utilityVRH.HitLayer == UnityLayer.Nature)
			{
				NatureData natureData = PickingUtility.FindHitNature(_utilityVRH);
				if (natureData.IsValid)
				{
					return natureData;
				}
				XYZ xYZ = PickingUtility.FindHitTree(_utilityVRH);
				if (xYZ != XYZ.zero)
				{
					return xYZ;
				}
			}
		}
		return null;
	}
}
