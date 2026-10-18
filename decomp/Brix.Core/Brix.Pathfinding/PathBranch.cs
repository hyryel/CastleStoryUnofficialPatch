using System;

namespace Brix.Pathfinding;

public struct PathBranch
{
	public class BranchType : IComparable<BranchType>
	{
		public const int EmptyBits = 0;

		public const int ForestBits = 1;

		public const int BreakingBits = 2;

		public const int LadderBits = 4;

		public const int ClimbingBits = 8;

		public const int StepDownBits = 16;

		public const int DiagonalBits = 32;

		public const int FullBits = int.MaxValue;

		public static readonly BranchType Empty = new BranchType("Empty", 0, 0);

		public static BranchType Forest = new BranchType("Forest", 1, 1);

		public static BranchType Breaking = new BranchType("Breaking", 2, 2);

		public static BranchType Ladder = new BranchType("Ladder", 4, 4);

		public static BranchType Climbing = new BranchType("Climbing", 8, int.MaxValue);

		public static BranchType StepDown = new BranchType("StepDown", 16, int.MaxValue);

		public static BranchType Diagonal = new BranchType("Diagonal", 32, int.MaxValue);

		public static readonly BranchType Full = new BranchType("Full", int.MaxValue, int.MaxValue);

		internal readonly string Name;

		internal readonly int Bits;

		internal readonly int LodBits;

		public BranchType(string name, int bits, int lodBits)
		{
			Name = name;
			Bits = bits;
			LodBits = bits;
		}

		public int CompareTo(BranchType other)
		{
			return Bits - other.Bits;
		}

		public override string ToString()
		{
			return Name;
		}
	}

	public static PathBranch Empty = new PathBranch(PathNode.Empty, BranchType.Empty);

	public static PathBranch[] None = new PathBranch[0];

	public PathNode node;

	public BranchType type;

	public int weight;

	public PathNode Node => node ?? PathNode.Empty;

	public BranchType Type => type ?? BranchType.Empty;

	public int Weight => weight;

	public bool IsEmpty => Type == BranchType.Empty;

	public bool IsMixed => Type != BranchType.Empty && Type != BranchType.Full;

	public bool IsFull => Type == BranchType.Full;

	public PathBranch(PathNode node, int weight = 0)
	{
		this.node = node;
		type = BranchType.Full;
		this.weight = weight;
	}

	public PathBranch(PathNode node, BranchType type, int weight = 0)
	{
		this.node = node;
		this.type = type;
		this.weight = weight;
	}
}
