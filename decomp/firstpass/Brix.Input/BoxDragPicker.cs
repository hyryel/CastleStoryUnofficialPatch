using UnityEngine;

namespace Brix.Input;

internal class BoxDragPicker : PickerComponent
{
	private void TryHoverBoxDrag(GameObject hoveredGameObject)
	{
		BoxDrag component = hoveredGameObject.GetComponent<BoxDrag>();
		if ((bool)component)
		{
			component.OnBoxHover(_voxelRayHit.LastRay);
		}
	}

	private void TryHoverCircle(GameObject hoveredGameObject)
	{
		CircleZones component = hoveredGameObject.GetComponent<CircleZones>();
		if ((bool)component)
		{
			Vector3 worldHitVector = _voxelRayHit.WorldHitVector;
			component.OnHover(worldHitVector);
		}
	}

	private void TryHoverRectZone(GameObject hoveredGameObject)
	{
		RectZone component = hoveredGameObject.GetComponent<RectZone>();
		if ((bool)component)
		{
			Vector3 point = _voxelRayHit.WorldHitVector;
			if (Physics.Raycast(_voxelRayHit.LastRay, out var hitInfo, 1000f, 1024, QueryTriggerInteraction.Ignore))
			{
				point = hitInfo.point;
			}
			component.OnHover(point);
		}
	}

	public override void OnHoverGameObject(GameObject hoveredGameObject)
	{
		TryHoverBoxDrag(hoveredGameObject);
		TryHoverCircle(hoveredGameObject);
		TryHoverRectZone(hoveredGameObject);
	}

	private void TryHoverEndBoxDrag(GameObject hoveredGameObject)
	{
		BoxDrag component = hoveredGameObject.GetComponent<BoxDrag>();
		if ((bool)component)
		{
			component.OnBoxeHoverEnded();
		}
	}

	private void TryHoverEndCircle(GameObject hoveredGameObject)
	{
		CircleZones component = hoveredGameObject.GetComponent<CircleZones>();
		if ((bool)component)
		{
			component.OnHoverEnd();
		}
	}

	private void TryHoverEndRectZone(GameObject hoveredGameObject)
	{
		RectZone component = hoveredGameObject.GetComponent<RectZone>();
		if ((bool)component)
		{
			component.OnHoverEnded();
		}
	}

	public override void OnLeaveGameObject(GameObject hoveredGameObject)
	{
		TryHoverEndBoxDrag(hoveredGameObject);
		TryHoverEndCircle(hoveredGameObject);
		TryHoverEndRectZone(hoveredGameObject);
	}
}
