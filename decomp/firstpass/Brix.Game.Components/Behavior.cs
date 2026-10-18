using System;
using Brix.Utils.Logging;
using Motus.Behavior;

namespace Brix.Game.Components;

public abstract class Behavior : UpdatableGameComponent<Behavior>
{
	private static BrixLogger logger = new BrixLogger(typeof(Behavior).FullName);

	private Node Tree;

	private NodeRun TreeRun;

	private string BehaviorName;

	protected virtual string CurrentTaskName => base.Name;

	protected override void Start()
	{
		base.Start();
		Tree = CreateTree();
		if (Tree == null)
		{
			throw new Exception("Behavior component must create a node");
		}
		TreeRun = Tree.Spawn();
		BehaviorName = $"Behavior: {GetType()}";
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (base.Started)
		{
			TreeRun.Halt();
		}
	}

	protected override void BrixFixedUpdate()
	{
		Tick();
	}

	protected override void OnFixedUpdateError(Exception e)
	{
		if (!ErrorHappened || !StopOnError)
		{
			logger.ErrorFormat(e, "Behavior error: {0} for {1} doing {2}", GetType(), base.Name, CurrentTaskName ?? "undefined task");
			ErrorHappened = true;
		}
		if (StopOnError && !this.IsNullOrReleased())
		{
			base.enabled = false;
		}
	}

	private void Tick()
	{
		try
		{
			if (this.IsNullOrReleased())
			{
				logger.ErrorFormat("The destroyed/released Behavior {0} is still being ticked by the GameClock", base.Name);
			}
			if (TreeRun.State != NodeState.Done)
			{
				TreeRun.Run();
			}
		}
		finally
		{
		}
	}

	protected abstract Node CreateTree();
}
