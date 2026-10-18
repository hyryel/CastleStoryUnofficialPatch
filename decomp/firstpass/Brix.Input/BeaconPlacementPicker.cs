using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game;
using Brix.Game.Beacons;
using Brix.Game.Network;
using Brix.IO.Serialization;
using Brix.Legacy;
using Brix.Lifecycle.Pooling;
using UnityEngine;

namespace Brix.Input;

public class BeaconPlacementPicker : PickerComponent
{
	public static Signal<Beacon> BeaconPlaced = new Signal<Beacon>();

	public static Signal<Beacon> BeaconCreated = new Signal<Beacon>();

	protected Beacon _beacon;

	private Factory.AssetKey _assetKey;

	[SerializeField]
	private Factory.AssetKey VisualsAssetKey;

	private void OnDestroy()
	{
		if (_beacon != null)
		{
			ObjectPoolSingleton.Release(_beacon.gameObject);
		}
		BeaconPlaced.Clear();
		BeaconCreated.Clear();
	}

	protected virtual void NetworkCreate()
	{
		User.LocalUser.GetComponent<UNetGroupe>().CallCmdSpawnGoupe(_assetKey.Factory, _assetKey.Name, _voxelRayHit.HitVector, Quaternion.identity, spawnSelected: true);
	}

	public override void OnLastActionTerrain(Vector3? clickedVoxel, ClickInfo clickInfo)
	{
		if (_beacon != null && clickInfo.actionName == "cam_action1" && _voxelRayHit.Normal == Vector3.up)
		{
			if (_assetKey != Balises.DefaultBalise && _assetKey != Balises.BaliseStartingPoint && _assetKey != Balises.PlayerFlagBeacon)
			{
				NetworkCreate();
			}
			else
			{
				GameObject gameObject = Factory.Instantiate(_assetKey, _voxelRayHit.HitVector, Quaternion.identity);
				BeaconPlaced.Invoke(gameObject.GetComponent<Beacon>());
			}
			ObjectPoolSingleton.Release(_beacon.gameObject);
			Picking.Instance.ClearPick();
		}
	}

	public override void OnHoverTerrain(Vector3? hoveredVoxel)
	{
		if (_beacon != null)
		{
			if (_voxelRayHit.Normal == Vector3.up)
			{
				ActivateBeacon();
				_beacon.GetComponent<Beacon>().position = _voxelRayHit.VoxelPosition;
			}
			else
			{
				DeactivateBeacon();
			}
		}
	}

	public override void OnLeaveTerrain(Vector3? hoveredVoxel)
	{
		if (_beacon != null)
		{
			DeactivateBeacon();
		}
	}

	private void CreateBeacon()
	{
		ObjectPoolSingleton.RequestComponent<Beacon>(out _beacon, Balises.DefaultBalise);
		SerializationComponent component = _beacon.GetComponent<SerializationComponent>();
		if ((bool)component)
		{
			component.DoNotSerialize = true;
		}
		SetIcon();
		BeaconCreated.Invoke(_beacon);
	}

	private void ActivateBeacon()
	{
		_beacon.Showing = true;
	}

	private void DeactivateBeacon()
	{
		_beacon.Showing = false;
	}

	public override void HoldKey(Factory.AssetKey assetKey)
	{
		if (assetKey != _assetKey || _beacon == null)
		{
			_assetKey = assetKey;
			if (_beacon != null)
			{
				ObjectPoolSingleton.Release(_beacon.gameObject);
			}
			if (_assetKey != null)
			{
				CreateBeacon();
			}
		}
	}

	protected override void OnActiveChange()
	{
		if (!base.Active && _beacon != null)
		{
			ObjectPoolSingleton.Release(_beacon.gameObject);
		}
	}

	protected override void OnStandByChange()
	{
		if (base.StandBy && _beacon != null)
		{
			_beacon.transform.position = Vector3.zero;
		}
	}

	public virtual void SetIcon()
	{
		BeaconDriver component = Factory.Peek(_assetKey).GetComponent<BeaconDriver>();
		if ((bool)component)
		{
			_beacon.Meta = component.beaconMeta;
		}
		else
		{
			_beacon.Meta = _beacon.GetOrAddComponent<BeaconMeta>();
		}
	}
}
