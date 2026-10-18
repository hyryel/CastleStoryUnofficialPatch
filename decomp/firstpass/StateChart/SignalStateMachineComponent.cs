using Brix.Game.Components;
using Brix.Utils.Logging;
using FSM;
using UnityEngine.Networking;

namespace StateChart;

public class SignalStateMachineComponent : UpdatableNetworkGameComponent
{
	private static readonly UpdatableNetworkProfiler<SignalStateMachineComponent> UpdatableProfiler = new UpdatableNetworkProfiler<SignalStateMachineComponent>();

	private static BrixLogger logger = new BrixLogger(typeof(SignalStateMachineComponent).FullName);

	protected SignalStateRun StateRun;

	private SignalState graph;

	protected override UpdatableProfiler Updatable => UpdatableProfiler;

	protected override bool HasUpdate => NetworkServer.active;

	protected override bool HasFixedUpdate => NetworkServer.active;

	protected virtual SignalState CreateFSM()
	{
		return null;
	}

	protected virtual void InitClientAnimator()
	{
	}

	protected virtual void InitAnimator()
	{
	}

	public override void OnStartPureClient()
	{
		base.OnStartPureClient();
		InitClientAnimator();
	}

	protected override void Start()
	{
		base.Start();
		if (NetworkServer.active)
		{
			if (graph == null)
			{
				graph = CreateFSM();
			}
			if (graph != null)
			{
				StateRun = new SignalStateRun(graph);
				StateRun.Start();
			}
			else
			{
				logger.Error("invalid state graph : CreateFSM must return a valid state graph");
			}
		}
	}

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
