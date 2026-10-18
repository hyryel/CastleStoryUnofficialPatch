using System;

namespace FSM;

public class MessageTransition<MESSAGETYPE> : Transition
{
	public Func<MESSAGETYPE, bool> Guard;

	public MessageTransition(State target, Func<MESSAGETYPE, bool> guard, TimingType timing, bool trap)
	{
		Target = target;
		Guard = guard;
		Timing = timing;
		Trap = trap;
	}

	public override Transition Handle(object m, out bool trapped)
	{
		if (Guard == null || Guard((MESSAGETYPE)m))
		{
			trapped = Trap;
			return this;
		}
		trapped = false;
		return null;
	}

	public override Type GetMessageType()
	{
		return typeof(MESSAGETYPE);
	}
}
