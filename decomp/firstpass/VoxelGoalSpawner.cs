using System;
using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Game;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Lifecycle.Pooling;
using Motus.Behavior;

public class VoxelGoalSpawner : Behavior
{
	public Signal Waiting = new Signal();

	private Dictionary<XYZ, SpawnOrder> goals;

	private Func<SpawnOrder, Goal> GoalCreator;

	private Queue<SpawnOrder> toRemove = new Queue<SpawnOrder>();

	private Queue<SpawnOrder> toAdd = new Queue<SpawnOrder>();

	public void QueueAdd(SpawnOrder pos)
	{
		toAdd.Enqueue(pos);
	}

	public void QueueRemove(SpawnOrder goal)
	{
		toRemove.Enqueue(goal);
	}

	public void ClearQueue()
	{
		toRemove.Clear();
		toAdd.Clear();
	}

	public void SetGoalCreator(Func<SpawnOrder, Goal> f)
	{
		GoalCreator = f;
	}

	public void WriteInto(Dictionary<XYZ, SpawnOrder> g)
	{
		goals = g;
	}

	public bool Done()
	{
		return toRemove.Count == 0 && toAdd.Count == 0;
	}

	protected override Node CreateTree()
	{
		return Node.Sequence.Do(Node.WaitForCondition(ShouldUpdate)).Do(DeleteAndSpawn()).Loop()
			.ToNode;
	}

	private bool ShouldUpdate()
	{
		return !Done();
	}

	private StaticNode DeleteAndSpawn()
	{
		return Node.Sequence.Do(DeleteLoop()).Do(AddLoop()).ToNode;
	}

	private StaticNode DeleteLoop()
	{
		Node node = Node.Do(delegate(VoxelGoalSpawner self)
		{
			self.DeleteSome(10);
		}, this);
		return Node.Loop(node).MaxTicksPerRun(GameClock.TicksPerFrame / 10).While((VoxelGoalSpawner self) => self.toRemove.Count != 0, this)
			.ToNode;
	}

	private StaticNode AddLoop()
	{
		Node node = Node.Do(delegate(VoxelGoalSpawner self)
		{
			self.AddSome(10);
		}, this);
		return Node.Loop(node).MaxTicksPerRun(GameClock.TicksPerFrame / 10).While((VoxelGoalSpawner self) => self.toAdd.Count != 0, this)
			.ToNode;
	}

	private void DeleteSome(int n)
	{
		int num = 0;
		while (toRemove.Count > 0)
		{
			num++;
			SpawnOrder spawnOrder = toRemove.Dequeue();
			if (!spawnOrder.instance.IsNullOrReleased())
			{
				ObjectPoolSingleton.Release(spawnOrder.instance.gameObject);
			}
			goals.Remove(spawnOrder.pos);
			if (num > n)
			{
				break;
			}
		}
	}

	private void AddSome(int n)
	{
		int num = 0;
		while (toAdd.Count > 0)
		{
			num++;
			SpawnOrder spawnOrder = toAdd.Dequeue();
			spawnOrder.instance = GoalCreator(spawnOrder);
			goals[spawnOrder.pos] = spawnOrder;
			if (num > n)
			{
				break;
			}
		}
	}
}
