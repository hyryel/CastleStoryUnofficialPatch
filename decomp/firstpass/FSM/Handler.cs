using System;

namespace FSM;

public abstract class Handler
{
	public enum TimingType
	{
		PreChildren,
		PostChildren
	}

	public TimingType Timing;

	public bool Trap;

	public abstract Transition Handle(object m, out bool trapped);

	public abstract Type GetMessageType();
}
