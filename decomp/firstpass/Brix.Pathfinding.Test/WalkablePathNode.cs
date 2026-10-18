namespace Brix.Pathfinding.Test;

public abstract class WalkablePathNode : PathDirectNode
{
	public virtual float GetSurfaceHeightDelta(float dx, float dz)
	{
		return 0.5f;
	}

	public virtual void Clear()
	{
	}
}
