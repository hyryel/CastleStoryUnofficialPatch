using Brix.Engine;
using UnityEngine;

namespace Brix.Pathfinding;

public abstract class PathDirectNode : PathNode
{
	public override int Lod => 0;

	public override XYZ LowResPosition => RepresentativePosition;

	public override Vector3 CenterPosition => RepresentativePosition.ToVector3();

	public override int Surface
	{
		get
		{
			return 1;
		}
		set
		{
		}
	}
}
