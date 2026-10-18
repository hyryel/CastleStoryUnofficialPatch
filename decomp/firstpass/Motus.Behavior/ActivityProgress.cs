using System;
using System.Collections.Generic;
using Brix.External.Signals;

namespace Motus.Behavior;

public abstract class ActivityProgress
{
	public class WaitingDelay : ActivityProgress
	{
		public int Delay;

		internal WaitingDelay(int delay = 1)
		{
			Delay = delay;
		}
	}

	public class Failed : ActivityProgress
	{
		public FailException Exception;

		internal Failed(FailException exception = null)
		{
			Exception = exception ?? new ActivityFailException();
		}
	}

	public class SubActivity : ActivityProgress
	{
		public IEnumerator<ActivityProgress> SubFiber;

		internal SubActivity(IEnumerator<ActivityProgress> sub)
		{
			SubFiber = sub;
		}
	}

	public class SubNode : ActivityProgress
	{
		public Node Node;

		internal SubNode(Node node)
		{
			Node = node;
		}
	}

	public abstract class SignalWaitBase : ActivityProgress
	{
		public abstract bool Called { get; }
	}

	private class SignalWaitBase<TAction> : SignalWaitBase
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

	private static Dictionary<int, WaitingDelay> delays = new Dictionary<int, WaitingDelay>();

	private static readonly WaitingDelay WaitOne = new WaitingDelay();

	public static WaitingDelay Wait()
	{
		return WaitOne;
	}

	public static WaitingDelay Wait(int delay)
	{
		if (!delays.ContainsKey(delay))
		{
			delays.Add(delay, new WaitingDelay(delay));
		}
		return delays[delay];
	}

	public static Failed Fail(FailException e = null)
	{
		return new Failed(e);
	}

	public static Failed Fail(string failureMessage)
	{
		return Fail(new ActivityFailException(failureMessage));
	}

	public static SubActivity Sub(IEnumerator<ActivityProgress> sub)
	{
		return new SubActivity(sub);
	}

	public static SubActivity Sub(Func<IEnumerator<ActivityProgress>> sub)
	{
		return new SubActivity(sub());
	}

	public static SubNode Sub(Node node)
	{
		return new SubNode(node);
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
