using System;
using Brix.Engine;

namespace Brix.Pathfinding;

public class PathAStarQueue : PathPriorityQueue, IPathGoaledQueue, IPathQueue
{
	protected XYZ goalPosition;

	public XYZ GoalPosition
	{
		get
		{
			return goalPosition;
		}
		set
		{
			goalPosition = value;
		}
	}

	public PathAStarQueue(XYZ goalPosition)
	{
		this.goalPosition = goalPosition;
	}

	public virtual void Reset(XYZ goalPosition)
	{
		this.goalPosition = goalPosition;
		Clear();
	}

	private static int Magnitude(XYZ d)
	{
		return (int)Math.Sqrt(XYZ.SqrMagnitude(d));
	}

	public override int Evaluate(PathLink link)
	{
		PathNode node = link.head.Node;
		XYZ d = node.RepresentativePosition - GoalPosition;
		int num = Magnitude(d);
		int length = link.length;
		int num2 = num * 45 + length * 10 + link.weight * 2;
		int surface = node.Surface;
		_ = node.Lod;
		return num2 - (surface >> 0);
	}
}
