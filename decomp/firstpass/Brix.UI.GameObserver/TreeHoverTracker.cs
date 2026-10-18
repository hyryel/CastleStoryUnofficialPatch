using Brix.Game;
using Brix.Input;
using UnityEngine;

namespace Brix.UI.GameObserver;

public class TreeHoverTracker : NatureHoverTracker
{
	protected override Vector3 GetExactNaturePosition(VoxelRaycastHit vrh)
	{
		if (vrh.HitLayer == UnityLayer.Nature)
		{
			return PickingUtility.FindHitTree(vrh).ToVector3();
		}
		return _notHovering;
	}
}
