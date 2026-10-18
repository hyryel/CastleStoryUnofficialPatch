using Brix.External.Factories;
using Brix.Game.Components;
using Brix.Lifecycle.Pooling;
using UnityEngine;

namespace Brix.Input;

internal class UniversalGrabber : PickerComponent
{
	public GameObject HeldObject;

	public override void OnHoverAny(bool hovering)
	{
		if (HeldObject != null && HeldObject.activeSelf)
		{
			IFitter component = HeldObject.GetComponent<IFitter>();
			if (component != null)
			{
				component.FitAndOrient(_voxelRayHit);
			}
			else
			{
				HeldObject.transform.position = _voxelRayHit.WorldHitVector;
			}
		}
	}

	public override void OnFirstActionAny(bool actioning, ClickInfo clickInfo)
	{
		if (clickInfo.actionName == "cam_action1" && clickInfo.clickState == ClickState.down)
		{
			PlaceHeldObjects();
		}
		else if (clickInfo.actionName == "cam_action2")
		{
			ChangeFit();
		}
	}

	public override void OnEnterAny(bool hovering)
	{
		if (HeldObject != null)
		{
			HeldObject.SetActive(value: true);
		}
	}

	public override void OnLeaveAny(bool hovering)
	{
		if (HeldObject != null)
		{
			HeldObject.SetActive(value: false);
		}
	}

	public override void HoldKey(Factory.AssetKey key)
	{
		DestroyHeldObjects();
		ObjectPoolSingleton.Request(out var go, key, Vector3.zero, Quaternion.identity);
		HeldObject = go;
	}

	protected virtual void PlaceHeldObjects()
	{
		HeldObject = null;
	}

	protected virtual void ChangeFit()
	{
		if (HeldObject != null)
		{
			IFitter component = HeldObject.GetComponent<IFitter>();
			if (component != null)
			{
				component.ChangeFit();
			}
			else
			{
				HeldObject.transform.rotation *= Quaternion.Euler(0f, 90f, 0f);
			}
		}
	}

	public virtual void DestroyHeldObjects()
	{
		if (HeldObject != null)
		{
			Object.Destroy(HeldObject);
			HeldObject = null;
		}
	}

	public void DeactivateHeldObject()
	{
		if (HeldObject != null)
		{
			HeldObject.SetActive(value: false);
		}
	}

	public void ActivateHeldObject()
	{
		if (HeldObject != null)
		{
			HeldObject.SetActive(value: true);
		}
	}

	protected override void OnStandByChange()
	{
		UpdateHeldObjects();
	}

	protected override void OnActiveChange()
	{
		UpdateHeldObjects();
	}

	private void UpdateHeldObjects()
	{
		if (base.Active && !base.StandBy)
		{
			ActivateHeldObject();
		}
		else
		{
			DeactivateHeldObject();
		}
	}
}
