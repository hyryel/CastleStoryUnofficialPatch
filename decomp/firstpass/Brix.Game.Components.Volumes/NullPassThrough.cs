using Brix.Engine;
using UnityEngine;

namespace Brix.Game.Components.Volumes;

public class NullPassThrough : IPassThrough
{
	public bool IsWalkable()
	{
		return false;
	}

	public virtual bool CanSearchThrough(GameObject character)
	{
		return false;
	}

	public bool CanPassThrough(GameObject character)
	{
		return false;
	}

	public int GetNiveauxAt(XYZ voxel)
	{
		return 0;
	}

	public float GetWalkHeight(XYZ voxel, float dx, float dz)
	{
		return 0f;
	}
}
