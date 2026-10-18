using System.Collections.Generic;
using Brix.Common.Collections;
using Brix.Engine;
using Brix.Engine.Nature;
using Brix.Engine.Trackers;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Game.Network;
using Brix.Lifecycle.Pooling;
using Brix.Utils.Signals;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using UnityEngine;

public class LandscapeGoalProvider : VoxelGoalProvider, IGoalProvider
{
	[SerializeField]
	private Transform goalParent;

	[SerializeField]
	private LandscapeData data;

	[SerializeField]
	private LandscapeProject project;

	private Signal drawingInfoReady = new Signal();

	private Signal updated = new Signal();

	private List<XYZ> toFill = new List<XYZ>(10000);

	private List<XYZ> toDig = new List<XYZ>(10000);

	public IReadOnlySignal DrawingInfoReady => drawingInfoReady;

	public IReadOnlySignal Updated => updated;

	public int Threshold => data.Threshold;

	public VoxelBounds Bounds => data.Bounds;

	public override void Initialize()
	{
		base.Initialize();
		data.NetworkUpdate.Connect(OnDataChange).DisconnectWhen(base.OnReleaseOrDestroySignal);
		base.DoneFeeding.Connect(drawingInfoReady.Invoke);
	}

	private void OnDataChange()
	{
		updated.Invoke();
		SetVolume(Bounds);
		base.StartFeeding.ConnectOnce(BeforeRefresh);
		base.DoneRefreshing.ConnectOnce(OnDoneRefreshing);
		StartUpdate();
	}

	public void SetDefault(Vector3 position)
	{
		Architecte.TopAndBottomInBounds(DefaultSearchBounds(position), out var top, out var bottom);
		VoxelBounds voxelBounds = VoxelGoalProvider.DefaultBounds(position);
		Vector3 min = new Vector3(voxelBounds.min.x, bottom.y, voxelBounds.min.z);
		Vector3 max = new Vector3(voxelBounds.max.x, top.y, voxelBounds.max.z);
		int threshold = (int)(min.y + max.y) / 2;
		User.LocalUser.GetComponent<UNetGroupe>().CallCmdSetLandscape(base.gameObject, min, max, threshold);
	}

	private VoxelBounds DefaultSearchBounds(Vector3 position)
	{
		VoxelBounds voxelBounds = VoxelGoalProvider.DefaultBounds(position);
		XYZ a = new XYZ(voxelBounds.min.x, voxelBounds.min.y - 20, voxelBounds.min.z);
		XYZ b = new XYZ(voxelBounds.max.x, voxelBounds.max.y + 20, voxelBounds.max.z);
		return new VoxelBounds(a, b);
	}

	private void BeforeRefresh()
	{
		toDig.Clear();
		toFill.Clear();
	}

	public void DrawingInfo(ref List<XYZ> _toFill, ref List<XYZ> _toDig)
	{
		toFill.Slinq().AddTo(_toFill);
		toDig.Slinq().AddTo(_toDig);
	}

	protected override SpawnOrder VoxelNeedsGoal(XYZ voxel)
	{
		if (!NeedsAGoal(voxel))
		{
			return SpawnOrder.None;
		}
		if (Threshold >= voxel.y)
		{
			if (NeedsFill(voxel))
			{
				toFill.Add(voxel);
				if (!NeedsFill(voxel - XYZ.up))
				{
					return new SpawnOrder(voxel, XYZ.zero, Test.LandscapeGoal);
				}
			}
		}
		else if (NeedsDig(voxel))
		{
			toDig.Add(voxel);
			if (Voxel.HasEmptyNeighbor(voxel))
			{
				return new SpawnOrder(voxel, XYZ.zero, Test.LandscapeGoal);
			}
		}
		return SpawnOrder.None;
	}

	protected override Goal CreateGoal(SpawnOrder order)
	{
		ObjectPoolSingleton.RequestComponent<LandscapeGoal>(out var t, Test.LandscapeGoal, order.pos.ToVector3(), Quaternion.identity);
		t.transform.SetParent(goalParent.transform);
		t.LinkProject(project);
		t.EvaluateTheshold(Threshold);
		return t;
	}

	private bool NeedsFill(XYZ p)
	{
		return Voxel.IsEmpty(p) || Voxel.IsCrust(p);
	}

	private bool NeedsDig(XYZ p)
	{
		return !Voxel.IsEmpty(p) && !Voxel.IsInvincible(p) && !Voxel.IsBlock(p);
	}

	private bool NeedsAGoal(XYZ p)
	{
		return !GeostaticityTracker.IsGeostatic(p) && !GeostaticityTracker.IsGeostatic(p + XYZ.up) && !NatureRegistry.HasNatureBlockerAt(p) && !NatureRegistry.HasNatureBlockerAt(p + XYZ.up);
	}

	private void ReevaluateAllThresholds()
	{
		foreach (LandscapeGoal item in Goals().NoLeak())
		{
			if (!item.IsNullOrReleased())
			{
				item.EvaluateTheshold(Threshold);
			}
		}
	}

	private void OnDoneRefreshing()
	{
		ReevaluateAllThresholds();
	}

	public bool HasNonStorable()
	{
		return toFill.Slinq().Any();
	}

	public bool HasStorable()
	{
		return toDig.Slinq().Any();
	}

	public Slinq<LandscapeGoal, SelectContext<LandscapeGoal, Goal, SelectContext<Goal, KeyValuePair<XYZ, SpawnOrder>, DictionaryContext<XYZ, SpawnOrder>>>> LandscapeGoals()
	{
		return from g in Goals()
			select g as LandscapeGoal;
	}

	public void UniqueItemsGiven(ref HashSet<Factory.AssetKey> keys)
	{
		foreach (LandscapeGoal item in LandscapeGoals().NoLeak())
		{
			if (!item.IsNullOrReleased())
			{
				item.UniqueItems(ref keys);
			}
		}
	}

	public void UniqueItemsNeeded(ref HashSet<Factory.AssetKey> keys)
	{
		if (HasNonStorable() && !HasStorable())
		{
			keys.Add(RawResources.RawStone);
		}
	}
}
