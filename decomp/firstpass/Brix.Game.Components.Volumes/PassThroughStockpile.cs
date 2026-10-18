using Brix.Engine;
using UnityEngine;

namespace Brix.Game.Components.Volumes;

public class PassThroughStockpile : PassThroughObstacle
{
	public override bool IsWalkable()
	{
		return base.recepteur.StoredItems.Count == 0;
	}

	public override bool CanPassThrough(GameObject character)
	{
		return base.recepteur.StoredItems.Count == 0;
	}

	public override int GetNiveauxAt(XYZ voxel)
	{
		return 4369;
	}
}
