using Brix.Game;
using Brix.Game.Network;
using UnityEngine;

namespace Brix.Input;

public class MineProjectPlacementPicker : BeaconPlacementPicker
{
	protected override void NetworkCreate()
	{
		User.LocalUser.GetComponent<UNetGroupe>().CallCmdSpawnMineProject(_voxelRayHit.HitVector, Quaternion.identity, spawnSelected: true);
	}
}
