using Brix.Engine.Nature;
using Brix.Game;
using Brix.Input;
using UnityEngine;

namespace Brix.UI.GameObserver;

public class BoulderHoverTracker : NatureHoverTracker
{
	protected override Vector3 GetExactNaturePosition(VoxelRaycastHit vrh)
	{
		if (vrh.HitLayer == UnityLayer.Nature)
		{
			NatureData natureData = PickingUtility.FindHitNature(vrh);
			if (natureData is BoulderData)
			{
				return natureData.RootPos.ToVector3();
			}
		}
		return _notHovering;
	}
}
