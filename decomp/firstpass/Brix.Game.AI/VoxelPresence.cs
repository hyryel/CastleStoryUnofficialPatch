using System;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Game.AI.Pathfinding;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Brix.Legacy;
using Brix.Pathfinding;
using Brix.Pathfinding.Test;
using Brix.Utils.Logging;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using UnityEngine;

namespace Brix.Game.AI;

[RequireComponent(typeof(Locomotion4))]
public class VoxelPresence : GameComponent
{
	private static BrixLogger logger = new BrixLogger(typeof(VoxelPresence).FullName);

	public static Signal<XYZ, VoxelPresence> VoxelBecameOccupied = new Signal<XYZ, VoxelPresence>();

	public static Signal<XYZ, VoxelPresence> VoxelBecameFree = new Signal<XYZ, VoxelPresence>();

	private Func<XYZ, bool> CollideWithEveryoneFunc;

	private Func<XYZ, bool> DontCollideIfAbleToWorkFunc;

	private Func<XYZ, bool> DontCollideIfEnemyFunc;

	public static bool DrawOccupiedVoxel = false;

	private Labor labor;

	private WalkablePathNode _currentPathNode;

	public Locomotion4 Locomotion;

	public bool OthersCanPassThroughMe => true;

	public XYZ CurrentVoxel => PositionOccupied;

	public WalkablePathNode CurrentPathNode
	{
		get
		{
			if (_currentPathNode == null || _currentPathNode.IsEmpty || _currentPathNode.RepresentativePosition != CurrentVoxel)
			{
				_currentPathNode = PathNodeRegistry.At(CurrentVoxel);
			}
			return _currentPathNode;
		}
	}

	public XYZ PositionEntering => Locomotion.PositionEntering;

	public XYZ PositionOccupied => Locomotion.PositionOccupied;

	public XYZ PositionNext => Locomotion.PositionNext;

	public XYZ PositionEnteringOrOccupied
	{
		get
		{
			XYZ xYZ = PositionEntering;
			if (xYZ == XYZ.zero)
			{
				xYZ = PositionOccupied;
			}
			return xYZ;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Locomotion = GetComponent<Locomotion4>();
		labor = GetComponent<Labor>();
		CollideWithEveryoneFunc = CollideWithEveryone;
		DontCollideIfAbleToWorkFunc = DontCollideIfAbleToWork;
		DontCollideIfEnemyFunc = DontCollideIfEnemy;
	}

	public bool CollideWithEveryone(XYZ position)
	{
		bool flag = VolumeRegistry.HasVolumesWithPropertyAt(position, VProperty.character, (IVolume p, VoxelPresence self) => self.PreventsMovement(p.GetComponent<VoxelPresence>(), testOthersPassThrough: false, passThroughEnemies: false), this);
		return !flag;
	}

	public bool DontCollideIfEnemy(XYZ position)
	{
		bool flag = VolumeRegistry.HasVolumesWithPropertyAt(position, VProperty.character, (IVolume p, VoxelPresence self) => self.PreventsMovement(p.GetComponent<VoxelPresence>(), testOthersPassThrough: false, passThroughEnemies: true), this);
		return !flag;
	}

	public bool DontCollideIfAbleToWork(XYZ position)
	{
		bool flag = VolumeRegistry.HasVolumesWithPropertyAt(position, VProperty.character, (IVolume p, VoxelPresence self) => self.PreventsMovement(p.GetComponent<VoxelPresence>(), testOthersPassThrough: true, passThroughEnemies: false), this);
		return !flag;
	}

	public Func<XYZ, bool> BuildPresenceTest(bool tryPassThrough = false, bool aggressive = false)
	{
		if (aggressive)
		{
			return DontCollideIfEnemyFunc;
		}
		if (tryPassThrough)
		{
			return DontCollideIfAbleToWorkFunc;
		}
		return CollideWithEveryoneFunc;
	}

	private bool PreventsMovement(VoxelPresence presence, bool testOthersPassThrough, bool passThroughEnemies)
	{
		if (presence == this || presence == null)
		{
			return false;
		}
		bool flag = (testOthersPassThrough && presence.OthersCanPassThroughMe && presence.affiliation.IsAllied(this)) || (passThroughEnemies && presence.affiliation.IsEnemy(this));
		return !flag;
	}

	public static bool HasPresenceAt(XYZ voxel)
	{
		return VolumeRegistry.HasVolumesWithPropertyAt(voxel, VProperty.character);
	}

	public static Slinq<VoxelPresence, SelectContext<VoxelPresence, IVolume, IListContext<IVolume>>> GetPresenceAt(XYZ voxel)
	{
		return VolumeRegistry.VolumesWithPropertyAt(voxel, VProperty.character).Slinq().Select(IVolumeExtender.GetComponent<VoxelPresence>);
	}

	private static bool IsWalkableVoxel(XYZ voxel)
	{
		return PathNodeRegistry.CheckPathNode(voxel);
	}

	private static bool IsWalkableVoxel(XYZ voxel, ref XYZ result)
	{
		if (!IsWalkableVoxel(voxel))
		{
			return false;
		}
		result = voxel;
		return true;
	}

	public static XYZ GetWalkableVoxel(XYZ voxel, out bool success)
	{
		success = GetWalkableVoxel(voxel, out XYZ result);
		return result;
	}

	public static bool GetWalkableVoxel(XYZ voxel, out XYZ result)
	{
		result = voxel;
		return IsWalkableVoxel(voxel, ref result) || IsWalkableVoxel(voxel + XYZ.up, ref result) || IsWalkableVoxel(voxel - XYZ.up, ref result);
	}

	public static XYZ GetWalkableVoxel(XYZ voxel)
	{
		if (!GetWalkableVoxel(voxel, out XYZ _))
		{
			logger.InfoFormat("VoxelPresence.GetWalkableVoxel({0}) failed. Returning original voxel.", voxel);
		}
		return voxel;
	}

	public VoxelPresence GetStackedPresence()
	{
		return GetPresenceAt(CurrentVoxel).Where((VoxelPresence other, VoxelPresence self) => other != self, this).FirstOrDefault();
	}

	public bool IsInForest()
	{
		return TreeRegistry.Instance.IsNearATree(CurrentVoxel) || (!PathNodeRegistry.GetAdjacentPositions(CurrentVoxel, PathBranch.BranchType.Climbing).Slinq().Any() && PathNodeRegistry.GetAdjacentPositions(CurrentVoxel, PathBranch.BranchType.Forest).Slinq().Any());
	}
}
