using System;
using Brix.External.Signals;

namespace FSM;

public class SignalTransition : SignalHandler0Param
{
	public SignalState Target;

	public Func<bool> Guard;

	public SignalTransition(Signal sig, SignalState target, Func<bool> guard, TimingType timing, bool trap)
		: base(sig, timing, trap)
	{
		Target = target;
		Guard = guard;
	}

	public override SignalTransition Handle()
	{
		if (Guard == null || Guard())
		{
			return this;
		}
		return null;
	}
}
public class SignalTransition<P1> : SignalHandler1Param<P1>
{
	public SignalState Target;

	public Func<P1, bool> Guard;

	public SignalTransition(Signal<P1> sig, SignalState target, Func<P1, bool> guard, TimingType timing, bool trap)
		: base(sig, timing, trap)
	{
		Target = target;
		Guard = guard;
	}

	public override SignalTransition<P1> Handle(P1 p1)
	{
		if (Guard == null || Guard(p1))
		{
			return this;
		}
		return null;
	}
}
public class SignalTransition<P1, P2> : SignalHandler2Params<P1, P2>
{
	public SignalState Target;

	public Func<P1, P2, bool> Guard;

	public SignalTransition(Signal<P1, P2> sig, SignalState target, Func<P1, P2, bool> guard, TimingType timing, bool trap)
		: base(sig, timing, trap)
	{
		Target = target;
		Guard = guard;
	}

	public override SignalTransition<P1, P2> Handle(P1 p1, P2 p2)
	{
		if (Guard == null || Guard(p1, p2))
		{
			return this;
		}
		return null;
	}
}
public class SignalTransition<P1, P2, P3> : SignalHandler3Params<P1, P2, P3>
{
	public SignalState Target;

	public Func<P1, P2, P3, bool> Guard;

	public SignalTransition(Signal<P1, P2, P3> sig, SignalState target, Func<P1, P2, P3, bool> guard, TimingType timing, bool trap)
		: base(sig, timing, trap)
	{
		Target = target;
		Guard = guard;
	}

	public override SignalTransition<P1, P2, P3> Handle(P1 p1, P2 p2, P3 p3)
	{
		if (Guard == null || Guard(p1, p2, p3))
		{
			return this;
		}
		return null;
	}
}
public class SignalTransition<P1, P2, P3, P4> : SignalHandler4Params<P1, P2, P3, P4>
{
	public SignalState Target;

	public Func<P1, P2, P3, P4, bool> Guard;

	public SignalTransition(Signal<P1, P2, P3, P4> sig, SignalState target, Func<P1, P2, P3, P4, bool> guard, TimingType timing, bool trap)
		: base(sig, timing, trap)
	{
		Target = target;
		Guard = guard;
	}

	public override SignalTransition<P1, P2, P3, P4> Handle(P1 p1, P2 p2, P3 p3, P4 p4)
	{
		if (Guard == null || Guard(p1, p2, p3, p4))
		{
			return this;
		}
		return null;
	}
}
