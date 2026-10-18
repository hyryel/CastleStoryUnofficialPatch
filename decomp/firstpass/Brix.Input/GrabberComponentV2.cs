using System.Collections.Generic;
using Brix.Audio;
using Brix.External.Factories;
using Brix.Game.Components;
using Brix.Lifecycle.Pooling;
using Brix.Transactions;
using Brix.Utils;
using UnityEngine;

namespace Brix.Input;

public class GrabberComponentV2 : PickerComponent
{
	protected Slave _heldObject;

	protected IDragger _heldDragger;

	protected bool _holdingClick;

	protected Factory.AssetKey _masterKey;

	protected Quaternion _rotation = Quaternion.identity;

	protected SpawnTransactionMonitor<Blueprint> _lastMonitor;

	public override void OnHoverAny(bool hovering)
	{
		if (_heldObject == null)
		{
			HoldKey(_masterKey, _voxelRayHit.HitVector, _rotation);
		}
		if (!_holdingClick)
		{
			if (_heldDragger != null)
			{
				_heldDragger.ClearSlaves();
			}
			if (_heldObject.HeldObject.activeSelf)
			{
				_heldObject.Position = _voxelRayHit.HitVector;
				_heldObject.Rotation = _rotation;
			}
		}
	}

	public override void OnFirstActionAny(bool actioning, ClickInfo clickInfo)
	{
		if (clickInfo.actionName == "cam_action1" && clickInfo.clickState == ClickState.down && !PickingUtility.isInViewMode())
		{
			if (_heldDragger != null)
			{
				_heldDragger.SetDragStart(_voxelRayHit, this);
				_holdingClick = true;
			}
			else if (_lastMonitor == null || _lastMonitor.State != TransactionState.Running)
			{
				PlaceHeldObjects();
			}
		}
		else if (clickInfo.actionName == "cam_action2")
		{
			ChangeFit();
		}
	}

	public override void OnLastActionAny(bool actioning, ClickInfo clickInfo)
	{
		if (clickInfo.actionName == "cam_action1" && clickInfo.clickState == ClickState.up)
		{
			if (_holdingClick && (_lastMonitor == null || _lastMonitor.State != TransactionState.Running))
			{
				PlaceHeldObjects();
			}
			_holdingClick = false;
		}
	}

	public override void OnEnterAny(bool hovering)
	{
		ActivateHeldObjects();
	}

	public override void OnLeaveAny(bool hovering)
	{
		if (!_holdingClick)
		{
			DeactivateHeldObjects();
		}
	}

	public override void HoldKey(Factory.AssetKey key)
	{
		HoldKey(key, Vector3.zero, Quaternion.identity);
	}

	public virtual void HoldKey(Factory.AssetKey key, Vector3 position, Quaternion rotation)
	{
		_masterKey = key;
		DestroyHeldObjects();
		ObjectPoolSingleton.Request(out var go, _masterKey, position, rotation);
		SetHeldObject(go);
	}

	protected virtual void PlaceHeldObjects()
	{
		List<GameObject> list = new List<GameObject>();
		list.Add(_heldObject.ReleaseHeldObject());
		if (_heldDragger != null)
		{
			foreach (Slave slafe in _heldDragger.GetSlaves())
			{
				list.Add(slafe.ReleaseHeldObject());
			}
			_heldDragger.ClearSlaves();
			_heldDragger = null;
		}
		_heldObject = null;
		foreach (GameObject item in list)
		{
			ClasseMoi.Move(item);
			Blueprint component = item.GetComponent<Blueprint>();
			if (component != null)
			{
				component.State |= PlacementFlags.placed;
			}
		}
	}

	protected virtual void ChangeFit()
	{
		_rotation *= Quaternion.Euler(0f, 90f, 0f);
		_heldObject.Rotation = _rotation;
		if (_heldDragger != null)
		{
			foreach (Slave slafe in _heldDragger.GetSlaves())
			{
				slafe.Rotation = _rotation;
			}
		}
		SoundEngine.Play("Brick_Turn");
	}

	public virtual void SetHeldObject(GameObject go)
	{
		if (go != null)
		{
			_heldObject = new Slave(go, this);
			_heldDragger = _heldObject.GetHeldComponent<IDragger>();
		}
	}

	public virtual void DestroyHeldObjects()
	{
		if (_heldDragger != null)
		{
			_heldDragger.ClearSlaves();
			_heldDragger = null;
		}
		if (_heldObject != null)
		{
			_heldObject.DestroyHeldObject();
			_heldObject = null;
		}
	}

	public void DeactivateHeldObjects()
	{
		if (_heldDragger != null)
		{
			foreach (Slave slafe in _heldDragger.GetSlaves())
			{
				slafe.DeactivateHeldObject();
			}
		}
		if (_heldObject != null)
		{
			_heldObject.DeactivateHeldObject();
		}
		_holdingClick = false;
	}

	public void ActivateHeldObjects()
	{
		if (_heldDragger != null)
		{
			foreach (Slave slafe in _heldDragger.GetSlaves())
			{
				slafe.ActivateHeldObject();
			}
		}
		if (_heldObject != null)
		{
			_heldObject.ActivateHeldObject();
		}
	}

	public Factory.AssetKey GetKey()
	{
		return _masterKey;
	}

	public Slave GetSlave()
	{
		return _heldObject;
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
			ActivateHeldObjects();
		}
		else
		{
			DeactivateHeldObjects();
		}
	}
}
