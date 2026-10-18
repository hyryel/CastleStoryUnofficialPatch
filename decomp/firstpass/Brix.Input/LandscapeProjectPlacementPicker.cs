using Brix.Game;
using Brix.Game.Network;
using UnityEngine;

namespace Brix.Input;

public class LandscapeProjectPlacementPicker : BeaconPlacementPicker
{
	protected override void NetworkCreate()
	{
		User.LocalUser.GetComponent<UNetGroupe>().CallCmdSpawnLandscapeProject(_voxelRayHit.HitVector, Quaternion.identity, spawnSelected: true);
	}
}
