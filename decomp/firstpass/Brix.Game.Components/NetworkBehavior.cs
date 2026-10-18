using System;
using Brix.Utils.Logging;
using Motus.Behavior;
using UnityEngine.Networking;

namespace Brix.Game.Components;

public abstract class NetworkBehavior : UpdatableNetworkGameComponent
{
	private static readonly UpdatableNetworkProfiler<NetworkBehavior> UpdatableProfiler = new UpdatableNetworkProfiler<NetworkBehavior>();

	private static BrixLogger logger = new BrixLogger(typeof(NetworkBehavior).FullName);

	public Node Tree;

	public NodeRun TreeRun;

	private string DefaultTaskName;

	protected bool StopOnError = true;

	private bool ErrorHappened;

	protected override UpdatableProfiler Updatable => UpdatableProfiler;

	protected virtual string CurrentTaskName => DefaultTaskName;

	protected override void Start()
	{
		base.Start();
		Tree = CreateTree();
		if (Tree == null)
		{
			throw new Exception("Behavior component must create a node");
		}
		TreeRun = Tree.Spawn();
		DefaultTaskName = $"Behavior: {GetType()}";
	}

	protected override void BrixFixedUpdate()
	{
		Tick();
	}

	private void Tick()
	{
		if (this.IsNullOrReleased())
		{
			logger.ErrorFormat("The destroyed/released GameObject {0} is still being ticked by the GameClock", base.Name);
		}
		if (ErrorHappened && StopOnError)
		{
			return;
		}
		try
		{
			if (TreeRun.State != NodeState.Done)
			{
				TreeRun.Run();
			}
		}
		catch (Exception e)
		{
			logger.ErrorFormat(e, "NetworkBehavior error: {0} for {1} doing {2}", GetType(), base.Name, CurrentTaskName ?? "undefined task");
			ErrorHappened = true;
		}
	}

	protected abstract Node CreateTree();

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		bool flag2 = default(bool);
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
	}
}
