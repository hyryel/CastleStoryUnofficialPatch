using System;

namespace FSM;

public class MessageReaction<MESSAGETYPE> : Handler
{
	public Action<MESSAGETYPE> ReactionCode;

	public MessageReaction(Action<MESSAGETYPE> reactioncode, TimingType timing, bool trap)
	{
		ReactionCode = reactioncode;
		Timing = timing;
		Trap = trap;
	}

	public override Transition Handle(object m, out bool trapped)
	{
		ReactionCode((MESSAGETYPE)m);
		trapped = Trap;
		return null;
	}

	public override Type GetMessageType()
	{
		return typeof(MESSAGETYPE);
	}
}
