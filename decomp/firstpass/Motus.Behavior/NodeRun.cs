using System;
using Brix.Utils;
using Brix.Utils.Closures;
using UnityEngine;

namespace Motus.Behavior;

public abstract class NodeRun : IDisposable
{
	public FailException Exception;

	protected string Label = string.Empty;

	protected UnityEngine.Object Context;

	protected IAction Finalizer = Empty.Closure.Delegate;

	public NodeState State { get; set; }

	public bool Finalized { get; private set; }

	protected NodeRun()
	{
		State = NodeState.New;
	}

	protected NodeRun(string label)
		: this()
	{
		Label = label;
	}

	protected NodeRun(string label, IAction finalizer)
		: this(label)
	{
		State = NodeState.New;
		Finalizer = finalizer;
	}

	protected virtual void Init()
	{
	}

	public void Start()
	{
		State = NodeState.Running;
		Finalized = false;
		Init();
	}

	protected virtual void Tick()
	{
	}

	public NodeState Run()
	{
		if (State == NodeState.Done || State == NodeState.Fail)
		{
			return State;
		}
		if (State == NodeState.New)
		{
			Start();
		}
		if (State == NodeState.Running)
		{
			ProfiledTick();
		}
		if (State != NodeState.Running)
		{
			Halt();
		}
		return State;
	}

	private void ProfiledTick()
	{
		Tick();
	}

	protected virtual void Terminate()
	{
	}

	public void Halt()
	{
		if (!Finalized)
		{
			Finalized = true;
			Terminate();
			Finalizer.Invoke();
		}
	}

	public void Restart()
	{
		Halt();
		Start();
	}

	public virtual void Dispose()
	{
	}

	protected static bool TryGetParam(NodeRun nodeRun, ref object result)
	{
		if (nodeRun is IWithResult { HasResult: not false } withResult)
		{
			result = withResult.Result;
			return withResult.HasResult;
		}
		return result != null;
	}
}
