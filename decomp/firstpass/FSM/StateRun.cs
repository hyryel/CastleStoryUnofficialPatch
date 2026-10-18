using System;
using System.Collections.Generic;

namespace FSM;

public class StateRun
{
	private readonly List<StateRun> subruns = new List<StateRun>();

	private readonly List<object> messageBacklog = new List<object>();

	private State State;

	private bool isHandlingMessage;

	public StateRun(State state)
	{
		State = state;
	}

	public void Start(object message = null)
	{
		State.CallEntry(message);
		foreach (State initialState in State.InitialStates)
		{
			StateRun stateRun = new StateRun(initialState);
			subruns.Add(stateRun);
			stateRun.Start(message);
		}
	}

	public void Stop(object message = null)
	{
		foreach (StateRun subrun in subruns)
		{
			subrun.Stop(message);
		}
		subruns.Clear();
		State.CallExit(message);
	}

	public void Handle(object message)
	{
		if (isHandlingMessage)
		{
			messageBacklog.Add(message);
			return;
		}
		isHandlingMessage = true;
		if (TraverseMessage(message, out var trapped) != null)
		{
			throw new Exception(string.Concat("Handling of message \"", message, "\" provoked a transition that could no be completed, verify state machine integrity."));
		}
		isHandlingMessage = false;
		for (int i = 0; i < messageBacklog.Count; i++)
		{
			isHandlingMessage = true;
			TraverseMessage(messageBacklog[i], out trapped);
			isHandlingMessage = false;
			if (i == 100)
			{
				throw new Exception("StateRun Message backlog reached satefy limit, possible infinite loop detected.");
			}
		}
		messageBacklog.Clear();
	}

	[Obsolete("Not used anymore", false)]
	public void HandleMessage(object message)
	{
		Handle(message);
	}

	private Transition TraverseMessage(object message, out bool trapped)
	{
		Transition transition = CallHandlers(message, Handler.TimingType.PreChildren, out trapped);
		if (transition != null)
		{
			if (transition.Target.Parent != State)
			{
				return transition;
			}
			if (subruns.Count > 1)
			{
				throw new Exception("Transition to direct child is not supported for states with regions; add the transition to the parent region itself");
			}
			subruns[0].Stop(message);
			subruns[0].State = transition.Target;
			subruns[0].Start(message);
			return null;
		}
		if (trapped)
		{
			return null;
		}
		foreach (StateRun subrun in subruns)
		{
			Transition transition2 = subrun.TraverseMessage(message, out trapped);
			if (transition2 != null)
			{
				if (transition2.Target.Parent != State)
				{
					return transition2;
				}
				subrun.Stop(message);
				subrun.State = transition2.Target;
				subrun.Start(message);
			}
		}
		if (trapped)
		{
			return null;
		}
		Transition transition3 = CallHandlers(message, Handler.TimingType.PostChildren, out trapped);
		if (transition3 == null)
		{
			return null;
		}
		if (transition3.Target.Parent != State)
		{
			return transition3;
		}
		if (subruns.Count > 1)
		{
			throw new Exception("Transition to direct child is not supported for states with regions; add the transition to the parent region itself");
		}
		subruns[0].Stop(message);
		subruns[0].State = transition3.Target;
		subruns[0].Start(message);
		return null;
	}

	public string DebugCurrentStateTree(string indent = "-")
	{
		string text = indent + State.Name + "\n";
		foreach (StateRun subrun in subruns)
		{
			text += subrun.DebugCurrentStateTree(indent + "   ");
		}
		return text;
	}

	private Transition CallHandlers(object message, Handler.TimingType timing, out bool trapped)
	{
		trapped = false;
		Type type = message.GetType();
		foreach (Handler handler in State.Handlers)
		{
			if (handler.GetMessageType().IsAssignableFrom(type) && handler.Timing == timing)
			{
				Transition transition = handler.Handle(message, out trapped);
				if (transition != null)
				{
					return transition;
				}
				if (trapped)
				{
					break;
				}
			}
		}
		return null;
	}
}
