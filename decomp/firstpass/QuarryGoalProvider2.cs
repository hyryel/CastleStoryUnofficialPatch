using System;
using System.Collections.Generic;
using Brix.Common.Collections;
using Brix.Engine;
using Brix.Engine.Nature;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Game.Network;
using Brix.Input;
using Brix.Lifecycle.Pooling;
using Brix.Utils.Signals;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using UnityEngine;

public class QuarryGoalProvider2 : VoxelGoalProvider, IGoalProvider
{
	public Signal GoalRefreshed = new Signal();

	public Signal Changed = new Signal();

	[SerializeField]
	private QuarryData data;

	[SerializeField]
	private StairsComputer stairComputer;

	[SerializeField]
	private Transform goalParent;

	public Dictionary<XYZ, SpawnOrder> stairs = new Dictionary<XYZ, SpawnOrder>();

	public bool Commited => data.Commited;

	public VoxelBounds Bounds => data.Bounds;

	public StairOrientation StairOrientation => data.StairOrientation;

	public GroundResource TypesToHarvest => data.TypesToHarvest;

	public int StairStartY => data.StairY;

	public void ToggleResource(GroundResource type)
	{
		data.Toggle(type);
	}

	public void SetResource(GroundResource type)
	{
		data.SetTypesToHarvest((int)type);
	}

	public void UniqueItemsGiven(ref HashSet<Factory.AssetKey> keys)
	{
		foreach (DigGoal item in DigGoals().NoLeak())
		{
			if (!item.IsNullOrReleased())
			{
				item.UniqueItems(ref keys);
			}
		}
	}

	public Slinq<DigGoal, NotNullContext<DigGoal, SelectContext<DigGoal, Goal, SelectContext<Goal, KeyValuePair<XYZ, SpawnOrder>, DictionaryContext<XYZ, SpawnOrder>>>>> DigGoals()
	{
		return (from g in Goals()
			select g as DigGoal).NotNull();
	}

	public void UniqueItemsNeeded(ref HashSet<Factory.AssetKey> keys)
	{
	}

	public override void Initialize()
	{
		base.Initialize();
		base.DoneRefreshing.Connect(GoalRefreshed.Invoke);
		data.NetworkUpdate.Connect(OnDataChange).DisconnectWhen(base.OnReleaseOrDestroySignal);
	}

	private void OnDataChange()
	{
		RefreshStairs();
		SetVolume(Bounds);
		StartUpdate();
		Changed.Invoke();
	}

	public void SetDefault(Vector3 position)
	{
		StairOrientation stairOrientation = StairOrientation.Default;
		VoxelBounds voxelBounds = VoxelGoalProvider.DefaultBounds(position);
		VoxelBounds bounds = VoxelGoalProvider.SearchBounds(position);
		XYZ min = voxelBounds.min;
		Architecte.TopAndBottomInBounds(bounds, out var _, out var bottom);
		min.y = bottom.y - 5;
		User.LocalUser.GetComponent<UNetGroupe>().CallCmdSetMine(base.gameObject, min.ToVector3(), voxelBounds.max.ToVector3(), stairOrientation.corner.ToVector3(), stairOrientation.direction.ToVector3(), commited: false);
	}

	public void Commit()
	{
		VoxelBounds bounds = Bounds;
		StairOrientation stairOrientation = StairOrientation;
		User.LocalUser.GetComponent<UNetGroupe>().CallCmdSetMine(base.gameObject, bounds.min.ToVector3(), bounds.max.ToVector3(), stairOrientation.corner.ToVector3(), stairOrientation.direction.ToVector3(), commited: true);
		Changed.Invoke();
	}

	protected override SpawnOrder VoxelNeedsGoal(XYZ pos)
	{
		if (IsStair(pos) || IsStair(pos + XYZ.up) || stairs.ContainsKey(pos + XYZ.up) || Voxel.IsEmpty(pos) || Voxel.IsInvincible(pos) || Voxel.IsBlock(pos) || NatureRegistry.HasNatureBlockerAt(pos) || NatureRegistry.HasNatureBlockerAt(pos + XYZ.up))
		{
			return SpawnOrder.None;
		}
		if (stairs.ContainsKey(pos))
		{
			return stairs[pos];
		}
		if (!IsNearSurface(pos))
		{
			return SpawnOrder.None;
		}
		return new SpawnOrder(pos, XYZ.zero, Test.DigGoal);
	}

	protected override Goal CreateGoal(SpawnOrder order)
	{
		if (order.key == Test.BuildStairsGoal)
		{
			return SpawnStairGoal(order.pos, order.direction);
		}
		if (order.key == Test.DigGoal)
		{
			return SpawnDigGoal(order.pos);
		}
		throw new ArgumentOutOfRangeException();
	}

	public void GetStairs(ref List<XYZ> positions, ref List<Faces> directions)
	{
		stairComputer.GetStairs(ref positions, ref directions);
	}

	public StairOrientation ValidStairFrom(VoxelBounds newBounds, StairOrientation orientation, out int y)
	{
		if (Commited)
		{
			y = StairStartY;
			return StairOrientation;
		}
		stairs.Clear();
		return stairComputer.CreateStairGoals(newBounds, orientation, out y, ref stairs);
	}

	private void RefreshStairs()
	{
		stairs.Clear();
		stairComputer.ExtendStairs(Bounds, StairOrientation, StairStartY, ref stairs);
	}

	private bool IsNearSurface(XYZ pos)
	{
		return Voxel.HasEmptyNeighbor(pos);
	}

	public static bool IsStair(XYZ pos)
	{
		bool result = false;
		IBlockData blockData = Voxel.GetBlockData(pos);
		if (blockData.BlockInfo.Key == Blocks.Mine_Staircase || blockData.BlockInfo.Key == Blocks.Mine_Staircase_Block)
		{
			result = true;
		}
		return result;
	}

	private BuildStairsGoal SpawnStairGoal(XYZ pos, XYZ direction)
	{
		ObjectPoolSingleton.RequestComponent<BuildStairsGoal>(out var t, Test.BuildStairsGoal, pos.ToVector3(), Quaternion.identity);
		t.transform.SetParent(goalParent);
		t.transform.forward = direction.ToVector3();
		return t;
	}

	private DigGoal SpawnDigGoal(XYZ pos)
	{
		ObjectPoolSingleton.RequestComponent<DigGoal>(out var t, Test.DigGoal, pos.ToVector3(), Quaternion.identity);
		t.transform.SetParent(goalParent);
		t.RebuildNode();
		t.ResourceSetting = data;
		return t;
	}

	public void RebuildDigGoalNodes()
	{
		foreach (DigGoal item in DigGoals().NoLeak())
		{
			item.RebuildNode();
		}
	}

	public bool HasNonStorable()
	{
		return false;
	}
}
