using System.Collections.Generic;
using Brix.Engine;
using Brix.Utils.Logging;

namespace Brix.Pathfinding.Test;

public class TerrainPathNode : WalkablePathNode
{
	private static BrixLogger logger = new BrixLogger(typeof(TerrainPathNode).FullName);

	public static readonly TerrainPathNode EmptyTerrainNode = new TerrainPathNode();

	private static readonly PathBranch[] NoSiblings = new PathBranch[0];

	private static readonly PathBranch[] NullSiblings = new PathBranch[0];

	public PathBranch[] siblings;

	public XYZ position;

	public PathNode parentNode = PathNode.Empty;

	public int obstacleCount;

	public int accessCount;

	public int niveaux;

	public int weightToParent;

	public int weightFromParent;

	public static readonly XYZ[] neighbors = new XYZ[4]
	{
		XYZ.right,
		-XYZ.right,
		XYZ.forward,
		-XYZ.forward
	};

	private static readonly List<PathBranch> _branches = new List<PathBranch>();

	private static readonly int[][] NeighborLevelIndices = new int[8][]
	{
		new int[4] { 1, 2, 0, 3 },
		new int[4] { 0, 3, 1, 2 },
		new int[4] { 0, 1, 3, 2 },
		new int[4] { 3, 2, 0, 1 },
		new int[2] { 1, 3 },
		new int[2] { 2, 0 },
		new int[2] { 0, 2 },
		new int[2] { 3, 1 }
	};

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

	public override NodeType Type
	{
		get
		{
			if (position == XYZ.zero || siblings == NullSiblings)
			{
				return NodeType.Empty;
			}
			if (ObstacleCount > 0)
			{
				return NodeType.Mixed;
			}
			return NodeType.Full;
		}
	}

	private TerrainPathNode()
	{
		siblings = NullSiblings;
	}

	public TerrainPathNode(XYZ position, int niveaux)
	{
		this.position = position;
		this.niveaux = niveaux;
		siblings = NoSiblings;
	}

	public override float GetSurfaceHeightDelta(float dx, float dz)
	{
		return Voxel.ComputeSurfaceHeightFromNiveaux(niveaux, dx, dz);
	}

	public override void Clear()
	{
		ObstacleCount = 0;
		ParentNode = PathNode.Empty;
		siblings = NullSiblings;
	}

	public void Init(TestTerrainLod pathLod)
	{
		bool checkTrees = TestPathfinding.IsAlmostNearATree(position);
		_branches.Clear();
		for (int i = 0; i < neighbors.Length; i++)
		{
			AddNeighborBranches(position + neighbors[i], i, pathLod, _branches, checkTrees);
		}
		siblings = ((_branches.Count <= 0) ? NoSiblings : _branches.ToArray());
	}

	private void AddNeighborBranches(XYZ neighbor, int neighborIndex, TestTerrainLod pathLod, List<PathBranch> branches, bool checkTrees)
	{
		TerrainPathNode node = pathLod.GetPathNode(neighbor) as TerrainPathNode;
		if (!TryAddRegularNeighborBranch(node, neighborIndex, branches, checkTrees))
		{
			TerrainPathNode node2 = pathLod.GetPathNode(neighbor - XYZ.up) as TerrainPathNode;
			TerrainPathNode node3 = pathLod.GetPathNode(neighbor + XYZ.up) as TerrainPathNode;
			if (!(TryAddRegularNeighborBranch(node2, neighborIndex, branches, checkTrees) | TryAddRegularNeighborBranch(node3, neighborIndex, branches, checkTrees)))
			{
				TryAddBreakableNeighborBranch(node2, branches);
				TryAddBreakableNeighborBranch(node, branches);
				TryAddBreakableNeighborBranch(node3, branches);
			}
		}
	}

	private bool TryAddRegularNeighborBranch(TerrainPathNode node, int neighborIndex, List<PathBranch> branches, bool checkTrees)
	{
		if (node == null || node.IsEmpty)
		{
			return false;
		}
		XYZ representativePosition = node.RepresentativePosition;
		if (checkTrees && TestPathfinding.IsSemiWalkableNatureBlocker(representativePosition))
		{
			if (Voxel.IsOpenAndWalkable(representativePosition))
			{
				branches.Add(new PathBranch(node, PathBranch.BranchType.Forest, 10));
				return true;
			}
			return false;
		}
		if (Voxel.IsOpenAndWalkable(representativePosition))
		{
			int denivelation = GetDenivelation(position.y, niveaux, representativePosition.y, node.niveaux, neighborIndex);
			if (denivelation > -10 && denivelation < 10)
			{
				int num = 10;
				PathBranch.BranchType type;
				if (denivelation >= 0)
				{
					if (denivelation >= 4)
					{
						num += 160;
						type = PathBranch.BranchType.StepDown;
					}
					else
					{
						num += 160 * denivelation / 4;
						type = PathBranch.BranchType.Full;
					}
				}
				else if (denivelation <= -4)
				{
					num += 240;
					type = PathBranch.BranchType.Climbing;
				}
				else
				{
					num += 240 * -denivelation / 4;
					type = PathBranch.BranchType.Full;
				}
				branches.Add(new PathBranch(node, type, num));
				return true;
			}
		}
		return false;
	}

	private bool TryAddBreakableNeighborBranch(TerrainPathNode node, List<PathBranch> branches)
	{
		if (node == null || node.IsEmpty)
		{
			return false;
		}
		XYZ representativePosition = node.RepresentativePosition;
		if (!Voxel.IsEmpty(representativePosition - XYZ.up))
		{
			int penalty = 0;
			if (IsBreakable(representativePosition, ref penalty))
			{
				IsBreakable(representativePosition + XYZ.up, ref penalty);
				branches.Add(new PathBranch(node, PathBranch.BranchType.Breaking, penalty));
				return true;
			}
		}
		return false;
	}

	private bool IsBreakable(XYZ p, ref int penalty)
	{
		if (Voxel.IsBlock(p))
		{
			int num = (int)Voxel.GetBlockData(p).DamageReceiver.MaxHP;
			penalty += num * 70 / 100;
			return true;
		}
		return false;
	}

	private static int GetDenivelation(int yA, int nivA, int yB, int nivB, int neighborIndex)
	{
		int[] array = NeighborLevelIndices[neighborIndex];
		if (array.Length == 4)
		{
			int num = ((nivA >> (array[0] << 2)) & 3) + yA * 3;
			int num2 = ((nivA >> (array[1] << 2)) & 3) + yA * 3;
			int num3 = ((nivB >> (array[2] << 2)) & 3) + yB * 3;
			int num4 = ((nivB >> (array[3] << 2)) & 3) + yB * 3;
			int num5 = num - num3;
			int num6 = num2 - num4;
			return num5 + num6;
		}
		int num7 = ((nivA >> (array[0] << 2)) & 3) + yA * 3;
		int num8 = ((nivB >> (array[1] << 2)) & 3) + yB * 3;
		return (num7 - num8) * 2;
	}
}
