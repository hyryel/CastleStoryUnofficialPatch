using Brix.Pathfinding.Test;
using UnityEngine;

namespace Brix.Game.AI;

public static class WalkablePathNodeExtender
{
	public static float GetSurfaceHeight(this WalkablePathNode pathNode, Vector3 innerPosition)
	{
		Vector3 vector = innerPosition - pathNode.CenterPosition;
		return pathNode.GetSurfaceHeight(vector.x, vector.z);
	}

	public static float GetSurfaceHeight(this WalkablePathNode pathNode, float dx, float dz)
	{
		return (float)pathNode.RepresentativePosition.y + pathNode.GetSurfaceHeightDelta(dx, dz);
	}
}
