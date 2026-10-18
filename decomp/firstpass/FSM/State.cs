using System;
using System.Collections.Generic;
using System.Linq;

namespace FSM;

public class State
{
	public List<Handler> Handlers = new List<Handler>();

	private List<MulticastDelegate> entry = new List<MulticastDelegate>();

	private List<MulticastDelegate> exit = new List<MulticastDelegate>();

	public State Parent;

	public string Name { get; set; }

	public List<State> Children { get; private set; }

	public List<State> Regions { get; private set; }

	public List<State> InitialStates
	{
		get
		{
			List<State> list = new List<State>();
			if (Children != null)
			{
				list.Add(Children[0]);
			}
			if (Regions != null)
			{
				list.AddRange(Regions);
			}
			return list;
		}
	}

	public State(string name)
	{
		Regions = null;
		Children = null;
		Name = name;
	}

	public State(string name, State parent)
	{
		Regions = null;
		Children = null;
		Name = name;
		parent?.AddSubState(this);
	}

	public void TransitTo<MESSAGETYPE>(State target)
	{
		TransitTo<MESSAGETYPE>(target, null, Handler.TimingType.PreChildren);
	}

	public void TransitTo<MESSAGETYPE>(State target, Func<MESSAGETYPE, bool> guard)
	{
		TransitTo(target, guard, Handler.TimingType.PreChildren);
	}

	public void TransitTo<MESSAGETYPE>(State target, Handler.TimingType timing)
	{
		TransitTo<MESSAGETYPE>(target, null, timing);
	}

	public void TransitTo<MESSAGETYPE>(State target, Func<MESSAGETYPE, bool> guard, Handler.TimingType timing)
	{
		Handlers.Add(new MessageTransition<MESSAGETYPE>(target, guard, timing, trap: false));
	}

	public void TransitTo<MESSAGETYPE>(State target, Func<MESSAGETYPE, bool> guard, Handler.TimingType timing, bool trap)
	{
		Handlers.Add(new MessageTransition<MESSAGETYPE>(target, guard, timing, trap));
	}

	public void React<MESSAGETYPE>(Action<MESSAGETYPE> reactioncode)
	{
		React(reactioncode, Handler.TimingType.PostChildren);
	}

	public void React<MESSAGETYPE>(Action<MESSAGETYPE> reactioncode, Handler.TimingType timing)
	{
		Handlers.Add(new MessageReaction<MESSAGETYPE>(reactioncode, timing, trap: false));
	}

	public void React<MESSAGETYPE>(Action<MESSAGETYPE> reactioncode, Handler.TimingType timing, bool trap)
	{
		Handlers.Add(new MessageReaction<MESSAGETYPE>(reactioncode, timing, trap));
	}

	public void Entry(Action e)
	{
		entry.Add(e);
	}

	public void Entry<T>(Action<T> e)
	{
		entry.Add(e);
	}

	public void Exit(Action e)
	{
		exit.Add(e);
	}

	public void Exit<T>(Action<T> e)
	{
		exit.Add(e);
	}

	public void CallEntry(object message)
	{
		Type type = message?.GetType();
		foreach (MulticastDelegate item in entry)
		{
			if (item is Action)
			{
				(item as Action)();
			}
			else if (type != null && item.GetType().IsGenericType && item.GetType().GetGenericArguments().Contains(type))
			{
				item.DynamicInvoke(message);
			}
		}
	}

	public void CallExit(object message)
	{
		Type type = message?.GetType();
		foreach (MulticastDelegate item in exit)
		{
			if (item is Action)
			{
				(item as Action)();
			}
			else if (type != null && item.GetType().IsGenericType && item.GetType().GetGenericArguments().Contains(type))
			{
				item.DynamicInvoke(message);
			}
		}
	}

	public void AddSubState(State substate)
	{
		if (Children == null)
		{
			Children = new List<State>();
		}
		Children.Add(substate);
		substate.Parent = this;
	}

	public void AddInitialSubState(State substate)
	{
		if (Children == null)
		{
			Children = new List<State>();
		}
		if (Children.Contains(substate))
		{
			Children.Remove(substate);
		}
		Children.Insert(0, substate);
		substate.Parent = this;
	}

	public void AddRegion(State region)
	{
		if (Regions == null)
		{
			Regions = new List<State>();
		}
		Regions.Add(region);
		region.Parent = this;
	}

	public bool IsSiblingOf(State s)
	{
		return s.Parent == Parent;
	}
}
