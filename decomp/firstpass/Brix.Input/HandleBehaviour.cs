using System;
using UnityEngine;

namespace Brix.Input;

public class HandleBehaviour
{
	private static readonly HandleBehaviour none = new HandleBehaviour();

	private static readonly TerrainHandleBehaviour terrain = new TerrainHandleBehaviour();

	private static readonly VerticalHandleBehaviour vertical = new VerticalHandleBehaviour();

	private static readonly HorizontalHandleBehaviour horizontal = new HorizontalHandleBehaviour();

	private static readonly BaseHandleBehaviour basic = new BaseHandleBehaviour();

	private static readonly ButtonHandleBehaviour button = new ButtonHandleBehaviour();

	private static readonly VerticalSliderHandleBehaviour verticalSlider = new VerticalSliderHandleBehaviour();

	private static readonly HorizontalPlaneBehaviour horizontalPlane = new HorizontalPlaneBehaviour();

	public virtual HandleType Type => HandleType.None;

	public virtual void HoverStart(DragHandle drag)
	{
	}

	public virtual void HoverEnd(DragHandle drag)
	{
	}

	public virtual void Pressed(DragHandle drag, ClickInfo click)
	{
	}

	public virtual void DragStarted(DragHandle handle)
	{
	}

	public virtual void Drag(DragHandle handle)
	{
	}

	public virtual void DragEnded(DragHandle handle)
	{
	}

	public virtual Vector3 GetDragDestination(DragHandle drag, Vector3 dragOrigin, Vector3 rayCollisionPoint)
	{
		return Vector3.zero;
	}

	public virtual Plane DragPlane(DragHandle drag)
	{
		return default(Plane);
	}

	public virtual void PickerBehaviour(DragHandle drag, HandleDragPicker handleDragPicker)
	{
	}

	public virtual void SetDefaultOrientation(DragHandle drag)
	{
	}

	public static HandleBehaviour OfType(HandleType behaviours)
	{
		return behaviours switch
		{
			HandleType.None => none, 
			HandleType.Base => basic, 
			HandleType.Terrain => terrain, 
			HandleType.Horizontal => horizontal, 
			HandleType.Vertical => vertical, 
			HandleType.Button => button, 
			HandleType.VerticalSlider => verticalSlider, 
			HandleType.HorizontalPlane => horizontalPlane, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
