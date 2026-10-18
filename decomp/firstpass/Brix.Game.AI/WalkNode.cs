using System;
using Brix.Engine;
using Brix.Game.AI.Pathfinding;
using Brix.Pathfinding;
using Brix.Pathfinding.Test;
using Brix.Utils;
using Brix.Utils.Locomotion;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.Game.AI;

public class WalkNode : Poolable
{
	private static BrixLogger logger = new BrixLogger(typeof(WalkNode).FullName);

	private WalkablePathNode _pathNode;

	private PathBranch.BranchType _branchType = PathBranch.BranchType.Empty;

	private bool _hasSpecificOrientation;

	private Quaternion rotation;

	public WalkablePathNode PathNode => _pathNode;

	public XYZ Voxel => _pathNode.RepresentativePosition;

	public Vector3 SurfacePosition
	{
		get
		{
			Vector3 position = Position;
			position.y = GetSurfaceHeight(0f, 0f);
			return position;
		}
	}

	public bool HasSpecificOrientation => _hasSpecificOrientation;

	public Vector3 Position
	{
		get
		{
			return _pathNode.RepresentativePosition.ToVector3();
		}
		set
		{
			throw new InvalidOperationException();
		}
	}

	public Quaternion Orientation
	{
		get
		{
			return rotation;
		}
		set
		{
			rotation = value;
			_hasSpecificOrientation = true;
		}
	}

	public WalkNode()
	{
	}

	public WalkNode(WalkablePathNode pathNode)
	{
		_pathNode = pathNode;
		if (pathNode == null)
		{
			logger.Error("WalkableLocation: pathNode is null");
		}
		else if (!pathNode.IsEmpty)
		{
			_branchType = PathBranch.BranchType.Full;
		}
	}

	public WalkNode(PathBranch pathBranch)
	{
		WalkablePathNode walkablePathNode = (_pathNode = (WalkablePathNode)pathBranch.Node);
		if (walkablePathNode == null)
		{
			logger.Error("WalkNode: pathNode is null");
		}
		else if (!walkablePathNode.IsEmpty)
		{
			_branchType = pathBranch.Type;
		}
	}

	public WalkNode(WalkablePathNode pathNode, Quaternion rotation)
		: this(pathNode)
	{
		this.rotation = rotation;
		_hasSpecificOrientation = true;
	}

	public WalkNode(PathBranch pathBranch, Quaternion rotation)
		: this(pathBranch)
	{
		this.rotation = rotation;
		_hasSpecificOrientation = true;
	}

	public static WalkNode At(XYZ position)
	{
		return new WalkNode(PathNodeRegistry.At(position));
	}

	public WalkNode Toward(WalkablePathNode next)
	{
		Quaternion quaternion = Quaternion.LookRotation((next.RepresentativePosition - Voxel).ToVector3().Flatten(), Vector3.up);
		PathBranch.BranchType type = PathNodeRegistry.PermissionVers(Voxel, next.RepresentativePosition);
		return new WalkNode(new PathBranch(next, type), quaternion);
	}

	public void Reset(WalkNode other)
	{
		_pathNode = other._pathNode;
		_branchType = other._branchType;
		rotation = other.rotation;
		_hasSpecificOrientation = other._hasSpecificOrientation;
	}

	public void Reset(WalkablePathNode pathNode)
	{
		_pathNode = pathNode;
		if (pathNode == null)
		{
			logger.Error("WalkableLocation: pathNode is null");
		}
		else if (!pathNode.IsEmpty)
		{
			_branchType = PathBranch.BranchType.Full;
		}
		rotation = Quaternion.identity;
		_hasSpecificOrientation = false;
	}

	public void Reset(PathBranch pathBranch)
	{
		WalkablePathNode walkablePathNode = (_pathNode = (WalkablePathNode)pathBranch.Node);
		if (walkablePathNode == null)
		{
			logger.Error("WalkableLocation: pathNode is null");
		}
		else if (!walkablePathNode.IsEmpty)
		{
			_branchType = pathBranch.Type;
		}
		rotation = Quaternion.identity;
		_hasSpecificOrientation = false;
	}

	public void SetPermissions(PathBranch.BranchType branchType)
	{
		_branchType = branchType;
	}

	public void ClearPermissions()
	{
		_branchType = PathBranch.BranchType.Empty;
	}

	public float GetSurfaceHeight(Vector3 innerPosition)
	{
		return _pathNode.GetSurfaceHeight(innerPosition);
	}

	public float GetSurfaceHeight(float dx, float dz)
	{
		return _pathNode.GetSurfaceHeight(dx, dz);
	}

	public bool IsWalkable()
	{
		return _branchType.CompareTo(PathBranch.BranchType.Diagonal) >= 0 || _branchType == PathBranch.BranchType.Forest;
	}

	public bool MustClimbUp()
	{
		return _branchType == PathBranch.BranchType.Climbing;
	}

	public bool MustStepDown()
	{
		return _branchType == PathBranch.BranchType.StepDown;
	}

	public bool IsLadder()
	{
		return false;
	}

	public bool IsBlockBreaking()
	{
		return _branchType == PathBranch.BranchType.Breaking;
	}

	public bool IsForest()
	{
		return _branchType == PathBranch.BranchType.Forest;
	}

	public bool MustFail()
	{
		return _branchType.CompareTo(PathBranch.BranchType.Empty) <= 0 || _pathNode == null || _pathNode.IsEmpty;
	}

	public float GetAngleToPosition(Vector3 originPos, Quaternion originDir)
	{
		Vector3 normalized = (Position - originPos).normalized;
		Vector3 vector = originDir * Vector3.forward;
		vector.y = 0f;
		vector.Normalize();
		normalized.y = 0f;
		normalized.Normalize();
		return Vector3.Angle(vector, normalized);
	}

	public float GetAngleToOrientation(Quaternion origin)
	{
		return Vector3.Angle((origin * Vector3.forward).Flatten(), (Orientation * Vector3.forward).Flatten());
	}

	public bool IsToTheRight(Vector3 originPos, Quaternion originDir)
	{
		Vector3 normalized = (Position - originPos).normalized;
		Vector3 lhs = originDir * Vector3.right;
		lhs.y = 0f;
		lhs.Normalize();
		return Vector3.Dot(lhs, normalized) > 0f;
	}

	public float GetDirectionToPosition(Vector3 originPos, Quaternion originDir)
	{
		float num = GetAngleToPosition(originPos, originDir) / 180f;
		if (!IsToTheRight(originPos, originDir))
		{
			num = 0f - num;
		}
		return num;
	}

	public float GetDirectionToOrientation(Quaternion originDir)
	{
		float num = GetAngleToOrientation(originDir) / 180f;
		Quaternion a = Quaternion.LookRotation(originDir * Vector3.right);
		if (Quaternion.Angle(a, Orientation) > 90f)
		{
			num = 0f - num;
		}
		return num;
	}

	public float GetDirectionFromPositionToOrientation(Vector3 origin)
	{
		Quaternion a = Quaternion.LookRotation(Position - origin);
		float num = Quaternion.Angle(a, Orientation) / 180f;
		Vector3 normalized = (Position - origin).normalized;
		if (Vector3.Dot(normalized, Orientation * Vector3.right) > 0f)
		{
			num = 0f - num;
		}
		return num;
	}
}
