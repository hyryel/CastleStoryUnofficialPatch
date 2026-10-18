using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Game;
using Brix.Game.AI;
using Brix.Game.Components;
using Motus.Behavior;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using UnityEngine;

public abstract class VoxelGoalProvider : Behavior
{
	private Signal startFeeding = new Signal();

	private Signal doneRefreshing = new Signal();

	private Signal doneFeeding = new Signal();

	[SerializeField]
	private VoxelGoalSpawner goalSpawner;

	[SerializeField]
	private float AutoRefreshDelay = 15f;

	[SerializeField]
	private float AutoRefreshFrameBudget = 0.01f;

	private Dictionary<XYZ, SpawnOrder> spawnPerVoxel = new Dictionary<XYZ, SpawnOrder>();

	private IVoxelVolume voxels = default(VoxelBounds);

	private IEnumerator<XYZ> enumerator;

	private bool enumerationDone;

	public bool CanUpdate;

	private bool updateNow;

	private float timeAtLastRefresh;

	protected IReadOnlySignal StartFeeding => startFeeding;

	protected IReadOnlySignal DoneRefreshing => doneRefreshing;

	protected IReadOnlySignal DoneFeeding => doneFeeding;

	protected IReadOnlySignal PartialRefresh => goalSpawner.Waiting;

	protected override void Awake()
	{
		base.Awake();
		goalSpawner.WriteInto(spawnPerVoxel);
		goalSpawner.SetGoalCreator(CreateGoal);
	}

	protected override Node CreateTree()
	{
		return Node.Sequence.Do(Node.WaitForCondition((VoxelGoalProvider self) => self.ShouldRefresh(), this)).Do(Refresh()).Loop()
			.ToNode;
	}

	protected abstract SpawnOrder VoxelNeedsGoal(XYZ pos);

	protected abstract Goal CreateGoal(SpawnOrder order);

	public Slinq<Goal, SelectContext<Goal, KeyValuePair<XYZ, SpawnOrder>, DictionaryContext<XYZ, SpawnOrder>>> Goals()
	{
		return from kvp in spawnPerVoxel.Slinq()
			select kvp.Value.instance;
	}

	public int GoalCount()
	{
		return spawnPerVoxel.Count;
	}

	public Slinq<Goal, SelectContext<Goal, KeyValuePair<XYZ, SpawnOrder>, DictionaryContext<XYZ, SpawnOrder>>> All()
	{
		return from kvp in spawnPerVoxel.Slinq()
			select kvp.Value.instance;
	}

	public void GetAll(ICollection<Goal> goals)
	{
		All().AddTo(goals);
	}

	protected void StartUpdate()
	{
		updateNow = true;
	}

	protected VoxelGoalProvider SetVolume(IVoxelVolume v)
	{
		voxels = v;
		return this;
	}

	private bool ShouldRefresh()
	{
		if (!CanUpdate)
		{
			return false;
		}
		if (updateNow)
		{
			updateNow = false;
			return true;
		}
		if (Time.time - timeAtLastRefresh > AutoRefreshDelay)
		{
			return true;
		}
		return false;
	}

	private StaticNode Refresh()
	{
		return Node.Sequence.Do(delegate(VoxelGoalProvider self)
		{
			self.PrepareLoop();
		}, this).Do(delegate(VoxelGoalProvider self)
		{
			self.RemoveOutsideBounds();
		}, this).Do(FeedingLoop())
			.Do(delegate(VoxelGoalProvider self)
			{
				self.doneFeeding.Invoke();
			}, this)
			.Do(Node.WaitForCondition((VoxelGoalProvider self) => self.goalSpawner.Done() || self.updateNow, this))
			.Do(delegate(VoxelGoalProvider self)
			{
				self.OnSpawnerDone();
			}, this)
			.ToNode;
	}

	private void PrepareLoop()
	{
		startFeeding.Invoke();
		goalSpawner.ClearQueue();
		enumerator = voxels.GetEnumerator();
		enumerationDone = false;
	}

	private StaticNode FeedingLoop()
	{
		Node node = Node.Do(delegate(VoxelGoalProvider self)
		{
			self.FeedSpawnerSome(10);
		}, this);
		return Node.Loop(node).MaxTicksPerRun((long)((float)GameClock.TicksPerFrame * AutoRefreshFrameBudget)).While((VoxelGoalProvider self) => !self.enumerationDone, this)
			.Finally(delegate(VoxelGoalProvider self)
			{
				self.Reset();
			}, this)
			.ToNode;
	}

	private void OnSpawnerDone()
	{
		if (goalSpawner.Done())
		{
			timeAtLastRefresh = Time.time;
			doneRefreshing.Invoke();
		}
	}

	private void Reset()
	{
		enumerator.Dispose();
		enumerationDone = true;
	}

	private void RemoveOutsideBounds()
	{
		foreach (KeyValuePair<XYZ, SpawnOrder> item in spawnPerVoxel)
		{
			if (!voxels.Contains(item.Key) || !item.Value.IsValid())
			{
				goalSpawner.QueueRemove(item.Value);
			}
		}
	}

	private bool MoveAndCheck()
	{
		bool flag = enumerator.MoveNext();
		enumerationDone = !flag;
		return flag;
	}

	private void FeedSpawnerSome(int n)
	{
		int num = 0;
		while (MoveAndCheck())
		{
			XYZ current = enumerator.Current;
			SpawnOrder pos = VoxelNeedsGoal(current);
			SpawnOrder valueOrDefault = spawnPerVoxel.GetValueOrDefault(current);
			if (pos.key != valueOrDefault.key || pos.alwaysReplace)
			{
				goalSpawner.QueueRemove(valueOrDefault);
				if (pos.key != null)
				{
					goalSpawner.QueueAdd(pos);
				}
			}
			num++;
			if (num > n)
			{
				break;
			}
		}
	}

	protected static VoxelBounds DefaultBounds(Vector3 position)
	{
		XYZ xYZ = XYZ.FromVector3(position);
		return new VoxelBounds(xYZ + XYZ.one * 2, xYZ - XYZ.one * 2);
	}

	protected static VoxelBounds SearchBounds(Vector3 position)
	{
		XYZ xYZ = XYZ.FromVector3(position);
		XYZ xYZ2 = new XYZ(2, 20, 2);
		return new VoxelBounds(xYZ + xYZ2, xYZ - xYZ2);
	}
}
