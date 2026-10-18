using Brix.Game.Components;
using UnityEngine;

namespace Brix.Input;

public class HandleDragPicker : PickerComponent
{
	protected DragHandle _heldObject;

	protected Vector3 _dragOrigin;

	protected Plane _dragPlane;

	private ClickInfo _currentClickInfo;

	private LayerBundle terrainLayer = new LayerBundle(10);

	private void TryHoverHandle(GameObject hovered)
	{
		if (!hovered.IsNullOrReleased())
		{
			DragHandle component = hovered.GetComponent<DragHandle>();
			if (!(component == null) && (!_heldObject || !(_heldObject.gameObject != hovered) || !component.Showing))
			{
				component.HoverStart();
			}
		}
	}

	private void TryEndHoverHandle(GameObject hovered)
	{
		if (!hovered.IsNullOrReleased())
		{
			DragHandle component = hovered.GetComponent<DragHandle>();
			if (!(component == null) && (!_heldObject || !(_heldObject.gameObject != hovered)))
			{
				component.HoverEnd();
			}
		}
	}

	public override void OnEnterGameObject(GameObject hoveredGameObject)
	{
		TryHoverHandle(hoveredGameObject);
	}

	public override void OnHoverGameObject(GameObject hoveredGameObject)
	{
	}

	public override void OnLeaveGameObject(GameObject hoveredGameObject)
	{
		TryEndHoverHandle(hoveredGameObject);
	}

	public override void OnFirstActionGameObject(GameObject clickedGameObject, ClickInfo clickInfo)
	{
		if (clickInfo.actionName == "cam_action1" && clickInfo.clickState == ClickState.down)
		{
			_heldObject = clickedGameObject.GetComponent<DragHandle>();
			if (_heldObject != null && _heldObject.Showing)
			{
				_dragOrigin = _heldObject.transform.position;
				_dragPlane = _heldObject.DragPlane();
				_heldObject.DragStarted();
			}
		}
	}

	public override void OnActionAny(bool actioning, ClickInfo clickInfo)
	{
		_currentClickInfo = clickInfo;
		if (_heldObject != null && _heldObject.Showing)
		{
			_heldObject.PickerBehaviour(this);
		}
	}

	public override void OnLastActionAny(bool actioning, ClickInfo clickInfo)
	{
		if ((bool)_heldObject && _heldObject.Showing)
		{
			_heldObject.Pressed(clickInfo);
			_heldObject.DragEnded();
			_heldObject = null;
		}
	}

	protected override void OnStandByChange()
	{
		_heldObject = null;
	}

	protected override void OnActiveChange()
	{
		if ((bool)_heldObject)
		{
			if (_heldObject.Showing)
			{
				_heldObject.DragEnded();
				_heldObject.Showing = false;
			}
			_heldObject = null;
		}
	}

	public void DefaultDragBehavior()
	{
		if (_dragPlane.Raycast(_voxelRayHit.LastRay, out var enter))
		{
			Vector3 point = _voxelRayHit.LastRay.GetPoint(enter);
			_heldObject.Drag(_dragOrigin, point);
		}
	}

	public void TerrainDragBehavior()
	{
		_utilityVRH.PlaceAtRaycast(Camera.main.ScreenPointToRay(_currentClickInfo.mousePosition), Picking.Instance.MaxDistance, terrainLayer);
		if (_utilityVRH.HasHit)
		{
			_heldObject.Drag(_dragOrigin, _utilityVRH.WorldHitVector);
		}
	}
}
