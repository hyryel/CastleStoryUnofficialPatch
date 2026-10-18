using Brix.Components;
using Brix.Engine;
using Brix.Input;
using Brix.Utils.Signals;
using Brix.Utils.Unity;
using Newtonsoft.Json;
using UnityEngine;

namespace Brix.Game.Components.Tunnel;

public class TunnelUnitUI : BrixComponent
{
	[DisplayRules(DisplayFlags.ReadOnlyAtRuntime)]
	[SerializeField]
	private TunnelUnit unit;

	[DisplayRules(DisplayFlags.ReadOnly | DisplayFlags.HideInEditMode)]
	[JsonProperty]
	public TunnelUI tunnelUI;

	[SerializeField]
	private VoxelFaces handles;

	private bool draggingLeft;

	private bool draggingForward;

	private bool draggingRight;

	private bool draggingBack;

	public override void OnRequested()
	{
		base.OnRequested();
		handles.Spawn();
		ConnectToSignals();
	}

	public void SetUI(TunnelUI ui)
	{
		tunnelUI = ui;
		tunnelUI.AddUnit(this);
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		if (tunnelUI != null && this != null)
		{
			tunnelUI.RemoveUnit(this);
		}
	}

	private void ConnectToSignals()
	{
		unit.Left.CanExpandAgain.Connect(EnableLeft).DisconnectWhen(base.OnReleaseOrDestroySignal);
		unit.Forward.CanExpandAgain.Connect(EnableForward).DisconnectWhen(base.OnReleaseOrDestroySignal);
		unit.Right.CanExpandAgain.Connect(EnableRight).DisconnectWhen(base.OnReleaseOrDestroySignal);
		unit.Back.CanExpandAgain.Connect(EnableBack).DisconnectWhen(base.OnReleaseOrDestroySignal);
		unit.Left.CanNoLongerExpand.Connect(DisableLeft).DisconnectWhen(base.OnReleaseOrDestroySignal);
		unit.Forward.CanNoLongerExpand.Connect(DisableForward).DisconnectWhen(base.OnReleaseOrDestroySignal);
		unit.Right.CanNoLongerExpand.Connect(DisableRight).DisconnectWhen(base.OnReleaseOrDestroySignal);
		unit.Back.CanNoLongerExpand.Connect(DisableBack).DisconnectWhen(base.OnReleaseOrDestroySignal);
		handles.Left.DragSignals.Connect(delegate
		{
			draggingLeft = true;
		}, DraggedLeft, null, base.OnReleaseOrDestroySignal);
		handles.Forward.DragSignals.Connect(delegate
		{
			draggingForward = true;
		}, DraggedForward, null, base.OnReleaseOrDestroySignal);
		handles.Right.DragSignals.Connect(delegate
		{
			draggingRight = true;
		}, DraggedRight, null, base.OnReleaseOrDestroySignal);
		handles.Back.DragSignals.Connect(delegate
		{
			draggingBack = true;
		}, DraggedBack, null, base.OnReleaseOrDestroySignal);
		handles.Up.ActionedSignal.Connect(OnDeleteButton).DisconnectWhen(base.OnReleaseOrDestroySignal);
	}

	private void OnDeleteButton(DragHandle handle, ClickInfo click)
	{
		if (Vector3.Distance(click.mousePosition, click.mousePositionDown) < 10f)
		{
			unit.tunnel.Remove(unit);
		}
	}

	private void DraggedLeft(DragHandle handle)
	{
		Vector3 vector = -Vector3.up + handles.Left.transform.right / 2f;
		XYZ pos = XYZ.FromVector3(handles.Left.transform.position + vector);
		Vector3 vector2 = base.transform.right + vector;
		XYZ xYZ = XYZ.FromVector3(handle.Destination + vector2);
		if (!unit.tunnel.IsOccupied(xYZ))
		{
			unit.tunnel.AddRange(pos, xYZ, unit.tunnel.GetExpansionType());
		}
	}

	private void DraggedForward(DragHandle handle)
	{
		Vector3 vector = -Vector3.up + handles.Forward.transform.right / 2f;
		XYZ pos = XYZ.FromVector3(handles.Forward.transform.position + vector);
		Vector3 vector2 = -base.transform.forward + vector;
		XYZ xYZ = XYZ.FromVector3(handle.Destination + vector2);
		if (!unit.tunnel.IsOccupied(xYZ))
		{
			unit.tunnel.AddRange(pos, xYZ, unit.tunnel.GetExpansionType());
		}
	}

	private void DraggedRight(DragHandle handle)
	{
		Vector3 vector = -Vector3.up + handles.Right.transform.right / 2f;
		XYZ pos = XYZ.FromVector3(handles.Right.transform.position + vector);
		Vector3 vector2 = -base.transform.right + vector;
		XYZ xYZ = XYZ.FromVector3(handle.Destination + vector2);
		if (!unit.tunnel.IsOccupied(xYZ))
		{
			unit.tunnel.AddRange(pos, xYZ, unit.tunnel.GetExpansionType());
		}
	}

	private void DraggedBack(DragHandle handle)
	{
		Vector3 vector = -Vector3.up + handles.Back.transform.right / 2f;
		XYZ pos = XYZ.FromVector3(handles.Back.transform.position + vector);
		Vector3 vector2 = base.transform.forward + vector;
		XYZ xYZ = XYZ.FromVector3(handle.Destination + vector2);
		if (!unit.tunnel.IsOccupied(xYZ))
		{
			unit.tunnel.AddRange(pos, xYZ, unit.tunnel.GetExpansionType());
		}
	}

	private void DisableLeft()
	{
		if (!draggingLeft)
		{
			handles.Left.Active = false;
			return;
		}
		draggingLeft = false;
		handles.Left.DragSignals.End.ConnectOnce(delegate
		{
			DisableLeft();
		});
	}

	private void DisableForward()
	{
		if (!draggingForward)
		{
			handles.Forward.Active = false;
			return;
		}
		draggingForward = false;
		handles.Forward.DragSignals.End.ConnectOnce(delegate
		{
			DisableForward();
		});
	}

	private void DisableRight()
	{
		if (!draggingRight)
		{
			handles.Right.Active = false;
			return;
		}
		draggingRight = false;
		handles.Right.DragSignals.End.ConnectOnce(delegate
		{
			DisableRight();
		});
	}

	private void DisableBack()
	{
		if (!draggingBack)
		{
			handles.Back.Active = false;
			return;
		}
		draggingBack = false;
		handles.Back.DragSignals.End.ConnectOnce(delegate
		{
			DisableBack();
		});
	}

	private void EnableBack()
	{
		handles.Back.Active = true;
	}

	private void EnableRight()
	{
		handles.Right.Active = true;
	}

	private void EnableForward()
	{
		handles.Forward.Active = true;
	}

	private void EnableLeft()
	{
		handles.Left.Active = true;
	}
}
