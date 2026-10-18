using System.Collections.Generic;
using Brix.Components;
using Brix.Engine;
using Brix.Engine.Blocks;
using Brix.Engine.Volumes;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.Storage;
using Brix.Legacy;
using Brix.Lifecycle.Pooling;
using Brix.Utils.Signals;
using UnityEngine;

namespace Brix.Game.AI;

public class RepairGoalProvider : VoxelGoalProvider, IGoalProvider
{
	public Signal Changed = new Signal();

	[SerializeField]
	private Transform goalParent;

	[SerializeField]
	private CompoundRecepteur compoundRecepteur;

	private Beacon beacon => GetComponent<Project>().Beacon;

	private float radius => GetComponent<Project>().Beacon.radius;

	public IVoxelVolume Bounds
	{
		get
		{
			VoxelCylinder voxelCylinder = new VoxelCylinder();
			voxelCylinder.center = beacon.position;
			voxelCylinder.extents = XYZ.one * (int)beacon.radius;
			return voxelCylinder;
		}
	}

	protected override void Start()
	{
		base.Start();
		beacon.PositionChange.End.Connect(delegate
		{
			OnDataChange();
		}).DisconnectWhen(base.OnReleaseSignal);
		beacon.RadiusChange.End.Connect(delegate
		{
			OnDataChange();
		}).DisconnectWhen(base.OnReleaseSignal);
		OnDataChange();
	}

	private void OnDataChange()
	{
		SetVolume(Bounds);
		StartUpdate();
		Changed.Invoke();
	}

	protected override SpawnOrder VoxelNeedsGoal(XYZ pos)
	{
		if (IsADamagedBrick(pos))
		{
			return new SpawnOrder(pos, XYZ.zero, Test.RepairGoal);
		}
		return SpawnOrder.None;
	}

	protected override Goal CreateGoal(SpawnOrder order)
	{
		ObjectPoolSingleton.RequestComponent<RepairGoal>(out var t, Test.RepairGoal, order.pos.ToVector3(), Quaternion.identity);
		t.transform.SetParent(goalParent.transform);
		return t;
	}

	private bool IsADamagedBrick(XYZ voxelIndex)
	{
		using IBlockData blockData = Voxel.GetBlockData(voxelIndex);
		return blockData.IsDamaged;
	}

	private IEnumerable<BlockData> GetAllDamagedBricksIn(Bounds bounds)
	{
		return BrixSingleton<BlockEngine>.Instance.GetDamagedBlocksIn(bounds);
	}

	public bool HasNonStorable()
	{
		return GoalCount() > 0;
	}

	public void UniqueItemsGiven(ref HashSet<Factory.AssetKey> keys)
	{
	}

	public void UniqueItemsNeeded(ref HashSet<Factory.AssetKey> keys)
	{
	}
}
