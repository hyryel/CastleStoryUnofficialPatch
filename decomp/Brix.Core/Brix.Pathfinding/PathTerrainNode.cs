using System;
using System.Linq;
using Brix.Engine;
using Brix.Pathfinding.Collections;

namespace Brix.Pathfinding;

public class PathTerrainNode : PathDirectNode
{
	public enum BranchType
	{
		None,
		Down,
		DownReverseClimbing,
		Same,
		SameClimbing,
		SameReverseClimbing,
		Up,
		UpClimbing
	}

	public static readonly XYZ[] neighbors = new XYZ[4]
	{
		XYZ.right,
		-XYZ.right,
		XYZ.forward,
		-XYZ.forward
	};

	private static readonly XYZ[] neighborsDown = neighbors.Select((XYZ p) => p - XYZ.up).ToArray();

	public BitField<BranchType> branches = default(BitField<BranchType>);

	public XYZ position;

	public PathNode parentNode = PathNode.Empty;

	public int obstacleCount;

	public int accessCount;

	public int weightToParent;

	public int weightFromParent;

	public override XYZ RepresentativePosition => position;

	public override int ObstacleCount
	{
		get
		{
			return obstacleCount;
		}
		set
		{
			int num = value - obstacleCount;
			obstacleCount += num;
			ParentNode.ObstacleCount += num;
		}
	}

	public override int AccessCount
	{
		get
		{
			return accessCount;
		}
		set
		{
			int num = value - accessCount;
			accessCount += num;
			ParentNode.AccessCount += num;
		}
	}

	public override PathNode ParentNode
	{
		get
		{
			return parentNode;
		}
		set
		{
			parentNode = value;
		}
	}

	public override int WeightToParent
	{
		get
		{
			return weightToParent;
		}
		set
		{
			weightToParent = value;
		}
	}

	public override int WeightFromParent
	{
		get
		{
			return weightFromParent;
		}
		set
		{
			weightFromParent = value;
		}
	}

	public void Init(XYZ p, PathTerrainLod pathLod)
	{
		position = p;
		branches.Clear();
		for (int i = 0; i < neighbors.Length; i++)
		{
			XYZ q = p + neighborsDown[i];
			BranchType branchType = GetBranchType(q, pathLod);
			branches.Add(i, branchType);
		}
	}

	public BranchType GetBranchType(XYZ q, PathTerrainLod pathLod)
	{
		PathNode pathNode = pathLod.GetPathNode(q);
		q.y++;
		PathNode pathNode2 = pathLod.GetPathNode(q);
		if (!pathNode.IsEmpty && pathNode2.IsEmpty)
		{
			return BranchType.Down;
		}
		pathNode = pathNode2;
		q.y++;
		pathNode2 = pathLod.GetPathNode(q);
		if (!pathNode.IsEmpty && pathNode2.IsEmpty)
		{
			return BranchType.Same;
		}
		pathNode = pathNode2;
		q.y++;
		pathNode2 = pathLod.GetPathNode(q);
		if (!pathNode.IsEmpty && pathNode2.IsEmpty)
		{
			return BranchType.Up;
		}
		return BranchType.None;
	}

	public XYZ GetRelativeNeighbor(BranchType type)
	{
		switch (type)
		{
		case BranchType.None:
			return XYZ.zero;
		case BranchType.Down:
		case BranchType.DownReverseClimbing:
			return -XYZ.up;
		case BranchType.Same:
		case BranchType.SameClimbing:
		case BranchType.SameReverseClimbing:
			return XYZ.zero;
		case BranchType.Up:
		case BranchType.UpClimbing:
			return XYZ.up;
		default:
			throw new ArgumentException("Invalid argument", "type");
		}
	}
}
