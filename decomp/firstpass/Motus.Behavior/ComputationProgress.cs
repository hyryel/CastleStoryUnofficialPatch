using System;
using System.Collections.Generic;
using Brix.External.Signals;

namespace Motus.Behavior;

public abstract class ComputationProgress<TResult>
{
	public class Waiting : ComputationProgress<TResult>
	{
		public int Delay = 1;

		public Waiting(int delay = 1)
		{
			Delay = delay;
		}
	}

	public class Failed : ComputationProgress<TResult>
	{
		public FailException Exception;

		public Failed(FailException exception = null)
		{
			Exception = exception ?? new ComputationFailException();
		}
	}

	public class SubComputation : ComputationProgress<TResult>
	{
		public IEnumerator<ComputationProgress<TResult>> SubFiber;

		public SubComputation(IEnumerator<ComputationProgress<TResult>> sub)
		{
			SubFiber = sub;
		}
	}

	public class SubNode : ComputationProgress<TResult>
	{
		public Node Node;

		public SubNode(Node node)
		{
			Node = node;
		}
	}

	public class Result : ComputationProgress<TResult>
	{
		public TResult Value;

		public Result(TResult p)
		{
			Value = p;
		}
	}

	public abstract class SignalWaitBase : ComputationProgress<TResult>
	{
		public abstract bool Called { get; }
	}

	public class SignalWaitBase<TAction> : SignalWaitBase
	{
		protected IConnectableSignalBase<TAction> Signal;

		protected int Count;

		public override bool Called => Count < Signal.InvokedCount;

		public SignalWaitBase(IConnectableSignalBase<TAction> sig)
		{
			Signal = sig;
			Count = Signal.InvokedCount;
		}
	}

	private static readonly Waiting DefaultWaiting = new Waiting();

	public static Waiting Wait()
	{
		return DefaultWaiting;
	}

	public static Waiting Wait(int delay)
	{
		return new Waiting(delay);
	}

	public static Failed Fail(FailException e = null)
	{
		return new Failed(e);
	}

	public static SubNode Sub(Node node)
	{
		return new SubNode(node);
	}

	public static Result Return(TResult values)
	{
		return new Result(values);
	}

	public static SubComputation Return(IEnumerator<ComputationProgress<TResult>> sub)
	{
		return new SubComputation(sub);
	}

	public static SignalWaitBase Wait(Signal s)
	{
		return new SignalWaitBase<Action>(s);
	}

	public static SignalWaitBase Wait<P1>(Signal<P1> s)
	{
		return new SignalWaitBase<Action<P1>>(s);
	}

	public static SignalWaitBase Wait<P1, P2>(Signal<P1, P2> s)
	{
		return new SignalWaitBase<Action<P1, P2>>(s);
	}

	public static SignalWaitBase Wait<P1, P2, P3>(Signal<P1, P2, P3> s)
	{
		return new SignalWaitBase<Action<P1, P2, P3>>(s);
	}

	public static SignalWaitBase Wait<P1, P2, P3, P4>(Signal<P1, P2, P3, P4> s)
	{
		return new SignalWaitBase<Action<P1, P2, P3, P4>>(s);
	}
}
