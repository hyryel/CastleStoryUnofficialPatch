using Brix.Engine;
using Brix.Transactions;
using Newtonsoft.Json;
using UnityEngine;

namespace Brix.Game.AI;

public abstract class GoalStorage
{
	protected GameObject parent;

	protected GameObject gameObject;

	[SerializeField]
	[JsonProperty]
	protected IVoxelVolume bounds;

	protected Faction faction => Faction.DefaultAllied;

	protected Transactor DefaultTransactor => faction.GetComponent<Transactor>();

	public void SetParent(GameObject parent)
	{
		this.parent = parent;
	}

	public void SetGameObject(GameObject go)
	{
		gameObject = go;
	}

	public virtual void SetBounds(VoxelBounds bounds)
	{
		this.bounds = bounds;
	}

	public virtual IVoxelVolume GetBounds()
	{
		return bounds;
	}

	public abstract void AddGoal(Goal goal);

	public abstract void RemoveGoal(Goal goal);

	public abstract void RemoveOutdatedGoals();

	public abstract void FetchNewGoals();

	public abstract void RebuildDigGoalNodes();

	public void RefreshGoals()
	{
		RemoveOutdatedGoals();
		FetchNewGoals();
	}
}
