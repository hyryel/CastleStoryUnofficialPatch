using System;
using Brix.Engine;
using UnityEngine;

namespace Brix.Pathfinding;

public class PathEmptyNode : PathNode
{
	public override XYZ RepresentativePosition => XYZ.zero;

	public override XYZ LowResPosition => XYZ.zero;

	public override Vector3 CenterPosition => Vector3.zero;

	public override int Lod => 0;

	public override NodeType Type => NodeType.Empty;

	public override int ObstacleCount
	{
		get
		{
			return 0;
		}
		set
		{
		}
	}

	public override int Surface
	{
		get
		{
			return 0;
		}
		set
		{
		}
	}

	public override PathNode ParentNode
	{
		get
		{
			return PathNode.Empty;
		}
		set
		{
			throw new InvalidOperationException();
		}
	}
}
