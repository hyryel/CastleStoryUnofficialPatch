using Brix.Engine.Nature;
using Brix.External.Factories;
using Brix.Game;
using Brix.Game.Beacons;
using Brix.Game.Network;
using UnityEngine;

namespace Brix.Input;

public class HarvestProjectPlacementPicker : BeaconPlacementPicker
{
	private HarvestProjectPlacementOptions _options;

	public override void SetOptions(IPickerComponentOptions options)
	{
		_options = options as HarvestProjectPlacementOptions;
	}

	protected override void NetworkCreate()
	{
		if (_options == null)
		{
			_options = new HarvestProjectPlacementOptions(NatureType.everything);
		}
		User.LocalUser.GetComponent<UNetGroupe>().CallCmdSpawnHarvestProject(_voxelRayHit.HitVector, Quaternion.identity, spawnSelected: true, (int)_options.AcceptedTypes);
	}

	public override void SetIcon()
	{
		Factory.AssetKey harvestMeta = HarvestData.GetHarvestMeta(_options.AcceptedTypes);
		_beacon.Meta = Factory.Peek(harvestMeta).GetComponent<BeaconMeta>();
	}
}
