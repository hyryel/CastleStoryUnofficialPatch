using Brix.Engine;
using UnityEngine;

namespace Brix.Game.Components.Volumes;

public class PassThroughObstacle : GameComponent, IPassThrough
{
	public static readonly NullPassThrough Null = new NullPassThrough();

	public virtual bool IsWalkable()
	{
		return true;
	}

	public virtual bool CanSearchThrough(GameObject character)
	{
		return CanPassThrough(character);
	}

	public virtual bool CanPassThrough(GameObject character)
	{
		return true;
	}

	public virtual int GetNiveauxAt(XYZ voxel)
	{
		return 0;
	}

	public virtual float GetWalkHeight(XYZ voxel, float dx, float dz)
	{
		return -0.5f;
	}
}
