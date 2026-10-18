using System;
using System.Collections.Generic;
using Brix.External.Signals;

namespace FSM;

public class SignalState
{
	private interface ActionInterface
	{
	}

	private class BasicAction : ActionInterface
	{
		public Action Action;

		public void Execute()
		{
			Action();
		}
	}

	private class SignalAction : ActionInterface
	{
		public Signal Signal;

		public Action Action;

		public void Execute(Signal sig)
		{
			if (sig == Signal)
			{
				Action();
			}
		}
	}

	private class SignalAction<P1> : ActionInterface
	{
		public Signal<P1> Signal;

		public Action<P1> Action;

		public void Execute(Signal<P1> sig, P1 p1)
		{
			if (sig == Signal)
			{
				Action(p1);
			}
		}
	}

	private class SignalAction<P1, P2> : ActionInterface
	{
		public Signal<P1, P2> Signal;

		public Action<P1, P2> Action;

		public void Execute(Signal<P1, P2> sig, P1 p1, P2 p2)
		{
			if (sig == Signal)
			{
				Action(p1, p2);
			}
		}
	}

	private class SignalAction<P1, P2, P3> : ActionInterface
	{
		public Signal<P1, P2, P3> Signal;

		public Action<P1, P2, P3> Action;

		public void Execute(Signal<P1, P2, P3> sig, P1 p1, P2 p2, P3 p3)
		{
			if (sig == Signal)
			{
				Action(p1, p2, p3);
			}
		}
	}

	private class SignalAction<P1, P2, P3, P4> : ActionInterface
	{
		public Signal<P1, P2, P3, P4> Signal;

		public Action<P1, P2, P3, P4> Action;

		public void Execute(Signal<P1, P2, P3, P4> sig, P1 p1, P2 p2, P3 p3, P4 p4)
		{
			if (sig == Signal)
			{
				Action(p1, p2, p3, p4);
			}
		}
	}

	public SignalState Parent;

	public List<SignalHandler> Handlers;

	private List<ActionInterface> entry;

	private List<ActionInterface> exit;

	public string Name { get; private set; }

	public List<SignalState> Children { get; private set; }

	public List<SignalState> Regions { get; private set; }

	public List<SignalState> InitialStates
	{
		get
		{
			List<SignalState> list = new List<SignalState>();
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

	public SignalState(string name)
	{
		Regions = null;
		Children = null;
		Name = name;
	}

	public SignalState(string name, SignalState parent)
		: this(name)
	{
		parent?.AddSubState(this);
	}

	public SignalStateRun Spawn()
	{
		return new SignalStateRun(this);
	}

	public void Transit(Signal signal, SignalState target, Func<bool> guard = null, SignalHandler.TimingType timing = SignalHandler.TimingType.PreChildren, bool trap = false)
	{
		AddHandler(new SignalTransition(signal, target, guard, timing, trap));
	}

	public void Transit<P1>(Signal<P1> signal, SignalState target, Func<P1, bool> guard = null, SignalHandler.TimingType timing = SignalHandler.TimingType.PreChildren, bool trap = false)
	{
		AddHandler(new SignalTransition<P1>(signal, target, guard, timing, trap));
	}

	public void Transit<P1, P2>(Signal<P1, P2> signal, SignalState target, Func<P1, P2, bool> guard = null, SignalHandler.TimingType timing = SignalHandler.TimingType.PreChildren, bool trap = false)
	{
		AddHandler(new SignalTransition<P1, P2>(signal, target, guard, timing, trap));
	}

	public void Transit<P1, P2, P3>(Signal<P1, P2, P3> signal, SignalState target, Func<P1, P2, P3, bool> guard = null, SignalHandler.TimingType timing = SignalHandler.TimingType.PreChildren, bool trap = false)
	{
		AddHandler(new SignalTransition<P1, P2, P3>(signal, target, guard, timing, trap));
	}

	public void Transit<P1, P2, P3, P4>(Signal<P1, P2, P3, P4> signal, SignalState target, Func<P1, P2, P3, P4, bool> guard = null, SignalHandler.TimingType timing = SignalHandler.TimingType.PreChildren, bool trap = false)
	{
		AddHandler(new SignalTransition<P1, P2, P3, P4>(signal, target, guard, timing, trap));
	}

	public void React(Signal signal, Action reaction, SignalHandler.TimingType timing = SignalHandler.TimingType.PostChildren, bool trap = false)
	{
		AddHandler(new SignalReaction(signal, reaction, timing, trap));
	}

	public void React<P1>(Signal<P1> signal, Action<P1> reaction, SignalHandler.TimingType timing = SignalHandler.TimingType.PostChildren, bool trap = false)
	{
		AddHandler(new SignalReaction<P1>(signal, reaction, timing, trap));
	}

	public void React<P1, P2>(Signal<P1, P2> signal, Action<P1, P2> reaction, SignalHandler.TimingType timing = SignalHandler.TimingType.PostChildren, bool trap = false)
	{
		AddHandler(new SignalReaction<P1, P2>(signal, reaction, timing, trap));
	}

	public void React<P1, P2, P3>(Signal<P1, P2, P3> signal, Action<P1, P2, P3> reaction, SignalHandler.TimingType timing = SignalHandler.TimingType.PostChildren, bool trap = false)
	{
		AddHandler(new SignalReaction<P1, P2, P3>(signal, reaction, timing, trap));
	}

	public void React<P1, P2, P3, P4>(Signal<P1, P2, P3, P4> signal, Action<P1, P2, P3, P4> reaction, SignalHandler.TimingType timing = SignalHandler.TimingType.PostChildren, bool trap = false)
	{
		AddHandler(new SignalReaction<P1, P2, P3, P4>(signal, reaction, timing, trap));
	}

	private void AddHandler(SignalHandler handler)
	{
		Handlers = Handlers ?? new List<SignalHandler>();
		Handlers.Add(handler);
	}

	public void Entry(Action e)
	{
		entry = entry ?? new List<ActionInterface>();
		entry.Add(new BasicAction
		{
			Action = e
		});
	}

	public void Entry(Signal s, Action a)
	{
		entry = entry ?? new List<ActionInterface>();
		entry.Add(new SignalAction
		{
			Signal = s,
			Action = a
		});
	}

	public void Entry<P1>(Signal<P1> s, Action<P1> a)
	{
		entry = entry ?? new List<ActionInterface>();
		entry.Add(new SignalAction<P1>
		{
			Signal = s,
			Action = a
		});
	}

	public void Entry<P1, P2>(Signal<P1, P2> s, Action<P1, P2> a)
	{
		entry = entry ?? new List<ActionInterface>();
		entry.Add(new SignalAction<P1, P2>
		{
			Signal = s,
			Action = a
		});
	}

	public void Entry<P1, P2, P3>(Signal<P1, P2, P3> s, Action<P1, P2, P3> a)
	{
		entry = entry ?? new List<ActionInterface>();
		entry.Add(new SignalAction<P1, P2, P3>
		{
			Signal = s,
			Action = a
		});
	}

	public void Entry<P1, P2, P3, P4>(Signal<P1, P2, P3, P4> s, Action<P1, P2, P3, P4> a)
	{
		entry = entry ?? new List<ActionInterface>();
		entry.Add(new SignalAction<P1, P2, P3, P4>
		{
			Signal = s,
			Action = a
		});
	}

	public void Exit(Action e)
	{
		exit = exit ?? new List<ActionInterface>();
		exit.Add(new BasicAction
		{
			Action = e
		});
	}

	public void Exit(Signal s, Action a)
	{
		exit = exit ?? new List<ActionInterface>();
		exit.Add(new SignalAction
		{
			Signal = s,
			Action = a
		});
	}

	public void Exit<P1>(Signal<P1> s, Action<P1> a)
	{
		exit = exit ?? new List<ActionInterface>();
		exit.Add(new SignalAction<P1>
		{
			Signal = s,
			Action = a
		});
	}

	public void Exit<P1, P2>(Signal<P1, P2> s, Action<P1, P2> a)
	{
		exit = exit ?? new List<ActionInterface>();
		exit.Add(new SignalAction<P1, P2>
		{
			Signal = s,
			Action = a
		});
	}

	public void Exit<P1, P2, P3>(Signal<P1, P2, P3> s, Action<P1, P2, P3> a)
	{
		exit = exit ?? new List<ActionInterface>();
		exit.Add(new SignalAction<P1, P2, P3>
		{
			Signal = s,
			Action = a
		});
	}

	public void Exit<P1, P2, P3, P4>(Signal<P1, P2, P3, P4> s, Action<P1, P2, P3, P4> a)
	{
		exit = exit ?? new List<ActionInterface>();
		exit.Add(new SignalAction<P1, P2, P3, P4>
		{
			Signal = s,
			Action = a
		});
	}

	public void ExecuteEntry()
	{
		if (entry != null)
		{
			Execute(entry);
		}
	}

	public void ExecuteEntry(Signal signal)
	{
		if (entry != null)
		{
			Execute(entry, signal);
		}
	}

	public void ExecuteEntry<P1>(Signal<P1> signal, P1 p1)
	{
		if (entry != null)
		{
			Execute(entry, signal, p1);
		}
	}

	public void ExecuteEntry<P1, P2>(Signal<P1, P2> signal, P1 p1, P2 p2)
	{
		if (entry != null)
		{
			Execute(entry, signal, p1, p2);
		}
	}

	public void ExecuteEntry<P1, P2, P3>(Signal<P1, P2, P3> signal, P1 p1, P2 p2, P3 p3)
	{
		if (entry != null)
		{
			Execute(entry, signal, p1, p2, p3);
		}
	}

	public void ExecuteEntry<P1, P2, P3, P4>(Signal<P1, P2, P3, P4> signal, P1 p1, P2 p2, P3 p3, P4 p4)
	{
		if (entry != null)
		{
			Execute(entry, signal, p1, p2, p3, p4);
		}
	}

	public void ExecuteExit()
	{
		if (exit != null)
		{
			Execute(exit);
		}
	}

	public void ExecuteExit(Signal signal)
	{
		if (exit != null)
		{
			Execute(exit, signal);
		}
	}

	public void ExecuteExit<P1>(Signal<P1> signal, P1 p1)
	{
		if (exit != null)
		{
			Execute(exit, signal, p1);
		}
	}

	public void ExecuteExit<P1, P2>(Signal<P1, P2> signal, P1 p1, P2 p2)
	{
		if (exit != null)
		{
			Execute(exit, signal, p1, p2);
		}
	}

	public void ExecuteExit<P1, P2, P3>(Signal<P1, P2, P3> signal, P1 p1, P2 p2, P3 p3)
	{
		if (exit != null)
		{
			Execute(exit, signal, p1, p2, p3);
		}
	}

	public void ExecuteExit<P1, P2, P3, P4>(Signal<P1, P2, P3, P4> signal, P1 p1, P2 p2, P3 p3, P4 p4)
	{
		if (exit != null)
		{
			Execute(exit, signal, p1, p2, p3, p4);
		}
	}

	private void Execute(List<ActionInterface> actions)
	{
		foreach (ActionInterface action in actions)
		{
			if (action is BasicAction)
			{
				(action as BasicAction).Execute();
			}
		}
	}

	private void Execute(List<ActionInterface> actions, Signal signal)
	{
		foreach (ActionInterface action in actions)
		{
			if (action is BasicAction)
			{
				(action as BasicAction).Execute();
			}
			else if (action is SignalAction)
			{
				(action as SignalAction).Execute(signal);
			}
		}
	}

	private void Execute<P1>(List<ActionInterface> actions, Signal<P1> signal, P1 p1)
	{
		foreach (ActionInterface action in actions)
		{
			if (action is BasicAction)
			{
				(action as BasicAction).Execute();
			}
			else if (action is SignalAction<P1>)
			{
				(action as SignalAction<P1>).Execute(signal, p1);
			}
		}
	}

	private void Execute<P1, P2>(List<ActionInterface> actions, Signal<P1, P2> signal, P1 p1, P2 p2)
	{
		foreach (ActionInterface action in actions)
		{
			if (action is BasicAction)
			{
				(action as BasicAction).Execute();
			}
			else if (action is SignalAction<P1, P2>)
			{
				(action as SignalAction<P1, P2>).Execute(signal, p1, p2);
			}
		}
	}

	private void Execute<P1, P2, P3>(List<ActionInterface> actions, Signal<P1, P2, P3> signal, P1 p1, P2 p2, P3 p3)
	{
		foreach (ActionInterface action in actions)
		{
			if (action is BasicAction)
			{
				(action as BasicAction).Execute();
			}
			else if (action is SignalAction<P1, P2, P3>)
			{
				(action as SignalAction<P1, P2, P3>).Execute(signal, p1, p2, p3);
			}
		}
	}

	private void Execute<P1, P2, P3, P4>(List<ActionInterface> actions, Signal<P1, P2, P3, P4> signal, P1 p1, P2 p2, P3 p3, P4 p4)
	{
		foreach (ActionInterface action in actions)
		{
			if (action is BasicAction)
			{
				(action as BasicAction).Execute();
			}
			else if (action is SignalAction<P1, P2, P3, P4>)
			{
				(action as SignalAction<P1, P2, P3, P4>).Execute(signal, p1, p2, p3, p4);
			}
		}
	}

	public void AddSubState(SignalState substate)
	{
		if (Children == null)
		{
			Children = new List<SignalState>();
		}
		Children.Add(substate);
		substate.Parent = this;
	}

	public void AddInitialSubState(SignalState substate)
	{
		if (Children == null)
		{
			Children = new List<SignalState>();
		}
		if (Children.Contains(substate))
		{
			Children.Remove(substate);
		}
		Children.Insert(0, substate);
		substate.Parent = this;
	}

	public void AddRegion(SignalState region)
	{
		if (Regions == null)
		{
			Regions = new List<SignalState>();
		}
		Regions.Add(region);
		region.Parent = this;
	}

	public bool IsSiblingOf(SignalState s)
	{
		return s.Parent == Parent;
	}

	public void CollectAllHandlers(List<SignalHandler> handlers)
	{
		if (Handlers != null)
		{
			handlers.AddRange(Handlers);
		}
		if (Children != null)
		{
			foreach (SignalState child in Children)
			{
				child.CollectAllHandlers(handlers);
			}
		}
		if (Regions == null)
		{
			return;
		}
		foreach (SignalState region in Regions)
		{
			region.CollectAllHandlers(handlers);
		}
	}
}
