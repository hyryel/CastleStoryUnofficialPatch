using FSM;
using UnityEngine;
using UnityEngine.Networking;

namespace StateChart;

public class StateMachineComponent : NetworkBehaviour
{
	protected StateRun StateRun;

	private State graph;

	private readonly DeltaTime _deltaTime = new DeltaTime(0f);

	protected virtual State CreateFSM()
	{
		return null;
	}

	protected virtual void Start()
	{
		if (graph == null)
		{
			graph = CreateFSM();
		}
		if (graph != null)
		{
			StateRun = new StateRun(graph);
			StateRun.Start();
		}
		else
		{
			Debug.LogError("invalid state graph : CreateFSM must return a valid state graph");
		}
	}

	protected virtual void OnDestroy()
	{
		StateRun.Stop();
		StateRun = null;
		graph = null;
	}

	public void HandleMessage(object m)
	{
		if (StateRun != null)
		{
			StateRun.Handle(m);
		}
	}

	protected virtual void Update()
	{
		_deltaTime.DeltaTime = Time.deltaTime;
		HandleMessage(_deltaTime);
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool result = default(bool);
		return result;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
	}
}
