using Brix.External.Factories;
using Brix.Game;
using Brix.Game.AI;
using Brix.Game.Network;
using Brix.Legacy;
using UnityEngine;

namespace Brix.Input;

public class ProjectPlacementPicker : GrabberComponentV2
{
	public override void OnHoverTerrain(Vector3? hoveredVoxel)
	{
		if (_heldObject == null)
		{
			HoldKey(_masterKey, _voxelRayHit.HitVector, _rotation);
		}
		if (!_holdingClick)
		{
			TunnelController heldComponent = _heldObject.GetHeldComponent<TunnelController>();
			if (_heldObject.HeldObject.activeSelf && heldComponent != null && heldComponent.ReadyForPicking)
			{
				_heldObject.Position = _voxelRayHit.HitVector;
				_heldObject.Rotation = _rotation;
				heldComponent.ResetToCenter();
			}
		}
	}

	protected virtual void NetworkCreate()
	{
		User.LocalUser.GetComponent<UNetGroupe>().CallCmdSpawnGoupe(_masterKey.Factory, _masterKey.Name, _voxelRayHit.HitVector, Quaternion.identity, spawnSelected: true);
	}

	protected override void PlaceHeldObjects()
	{
		if (_masterKey != Balises.DefaultBalise && _masterKey != Balises.BaliseStartingPoint && _masterKey != Balises.PlayerFlagBeacon)
		{
			NetworkCreate();
		}
		else
		{
			GameObject gameObject = Factory.Instantiate(_masterKey, _voxelRayHit.HitVector, Quaternion.identity);
			BeaconPlacementPicker.BeaconPlaced.Invoke(gameObject.GetComponent<Beacon>());
		}
		Project heldComponent = _heldObject.GetHeldComponent<Project>();
		heldComponent.IsBeingPlaced = false;
		_heldObject.DestroyHeldObject();
		Picking.Instance.ClearPick();
		_heldObject = null;
	}

	public override void HoldKey(Factory.AssetKey key)
	{
		_masterKey = key;
	}

	public override void HoldKey(Factory.AssetKey key, Vector3 position, Quaternion rotation)
	{
		base.HoldKey(key, position, rotation);
		TunnelController heldComponent = _heldObject.GetHeldComponent<TunnelController>();
		if (heldComponent != null)
		{
			heldComponent.OnSetInPicker();
		}
		Project heldComponent2 = _heldObject.GetHeldComponent<Project>();
		heldComponent2.IsBeingPlaced = true;
	}

	protected override void OnActiveChange()
	{
		if (base.Active)
		{
			ActivateHeldObjects();
		}
		else
		{
			base.DestroyHeldObjects();
		}
	}

	public override void OnHoverAny(bool hovering)
	{
	}

	public override void OnEnterAny(bool hovering)
	{
	}

	public override void OnLeaveAny(bool hovering)
	{
	}

	protected override void ChangeFit()
	{
	}
}
