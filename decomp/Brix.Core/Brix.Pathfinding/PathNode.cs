using Brix.Engine;
using UnityEngine;

namespace Brix.Pathfinding;

public abstract class PathNode
{
	public enum NodeType
	{
		Empty,
		Mixed,
		Full
	}

	public static PathEmptyNode Empty = new PathEmptyNode();

	public static PathNode[] None = new PathNode[0];

	public bool IsEmpty => Type == NodeType.Empty;

	public bool IsMixed => Type == NodeType.Mixed;

	public bool IsFull => Type == NodeType.Full;

	public abstract XYZ RepresentativePosition { get; }

	public abstract XYZ LowResPosition { get; }

	public abstract Vector3 CenterPosition { get; }

	public abstract int Lod { get; }

	public abstract int ObstacleCount { get; set; }

	public virtual int AccessCount
	{
		get
		{
			return 0;
		}
		set
		{
		}
	}

	public abstract int Surface { get; set; }

	public abstract PathNode ParentNode { get; set; }

	public virtual int WeightToParent
	{
		get
		{
			return 0;
		}
		set
		{
		}
	}

	public virtual int WeightFromParent
	{
		get
		{
			return 0;
		}
		set
		{
		}
	}

	public virtual NodeType Type => (ObstacleCount > 0) ? NodeType.Mixed : NodeType.Full;
}
