using System;
using Brix.External.Signals;

namespace FSM;

public class SignalReaction : SignalHandler0Param
{
	public Action Reaction;

	public SignalReaction(Signal sig, Action reaction, TimingType timing, bool trap)
		: base(sig, timing, trap)
	{
		Reaction = reaction;
	}

	public override SignalTransition Handle()
	{
		Reaction();
		return null;
	}
}
public class SignalReaction<P1> : SignalHandler1Param<P1>
{
	public Action<P1> Reaction;

	public SignalReaction(Signal<P1> sig, Action<P1> reaction, TimingType timing, bool trap)
		: base(sig, timing, trap)
	{
		Reaction = reaction;
	}

	public override SignalTransition<P1> Handle(P1 p1)
	{
		Reaction(p1);
		return null;
	}
}
public class SignalReaction<P1, P2> : SignalHandler2Params<P1, P2>
{
	public Action<P1, P2> Reaction;

	public SignalReaction(Signal<P1, P2> sig, Action<P1, P2> reaction, TimingType timing, bool trap)
		: base(sig, timing, trap)
	{
		Reaction = reaction;
	}

	public override SignalTransition<P1, P2> Handle(P1 p1, P2 p2)
	{
		Reaction(p1, p2);
		return null;
	}
}
public class SignalReaction<P1, P2, P3> : SignalHandler3Params<P1, P2, P3>
{
	public Action<P1, P2, P3> Reaction;

	public SignalReaction(Signal<P1, P2, P3> sig, Action<P1, P2, P3> reaction, TimingType timing, bool trap)
		: base(sig, timing, trap)
	{
		Reaction = reaction;
	}

	public override SignalTransition<P1, P2, P3> Handle(P1 p1, P2 p2, P3 p3)
	{
		Reaction(p1, p2, p3);
		return null;
	}
}
public class SignalReaction<P1, P2, P3, P4> : SignalHandler4Params<P1, P2, P3, P4>
{
	public Action<P1, P2, P3, P4> Reaction;

	public SignalReaction(Signal<P1, P2, P3, P4> sig, Action<P1, P2, P3, P4> reaction, TimingType timing, bool trap)
		: base(sig, timing, trap)
	{
		Reaction = reaction;
	}

	public override SignalTransition<P1, P2, P3, P4> Handle(P1 p1, P2 p2, P3 p3, P4 p4)
	{
		Reaction(p1, p2, p3, p4);
		return null;
	}
}
