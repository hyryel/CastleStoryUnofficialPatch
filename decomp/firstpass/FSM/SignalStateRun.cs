using System;
using System.Collections.Generic;
using Brix.External.Signals;

namespace FSM;

public class SignalStateRun
{
	private SignalState State;

	private SignalStateRun Parent;

	private List<SignalStateRun> SubRuns;

	private List<SignalConnection> Connections;

	private bool IsHandlingSignal;

	private Queue<Action> BackloggedSignals;

	public SignalStateRun(SignalState state, SignalStateRun parent = null)
	{
		State = state;
		Parent = parent;
	}

	public void Start()
	{
		if (Parent == null)
		{
			CreateConnections();
		}
		State.ExecuteEntry();
		List<SignalState>.Enumerator enumerator = State.InitialStates.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SignalState current = enumerator.Current;
			SubRuns = SubRuns ?? new List<SignalStateRun>();
			SignalStateRun signalStateRun = new SignalStateRun(current, this);
			SubRuns.Add(signalStateRun);
			signalStateRun.Start();
		}
	}

	public void Start(Signal sig)
	{
		if (Parent == null)
		{
			CreateConnections();
		}
		State.ExecuteEntry(sig);
		List<SignalState>.Enumerator enumerator = State.InitialStates.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SignalState current = enumerator.Current;
			SubRuns = SubRuns ?? new List<SignalStateRun>();
			SignalStateRun signalStateRun = new SignalStateRun(current, this);
			SubRuns.Add(signalStateRun);
			signalStateRun.Start(sig);
		}
	}

	public void Start<P1>(Signal<P1> sig, P1 p1)
	{
		if (Parent == null)
		{
			CreateConnections();
		}
		State.ExecuteEntry(sig, p1);
		List<SignalState>.Enumerator enumerator = State.InitialStates.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SignalState current = enumerator.Current;
			SubRuns = SubRuns ?? new List<SignalStateRun>();
			SignalStateRun signalStateRun = new SignalStateRun(current, this);
			SubRuns.Add(signalStateRun);
			signalStateRun.Start(sig, p1);
		}
	}

	public void Start<P1, P2>(Signal<P1, P2> sig, P1 p1, P2 p2)
	{
		if (Parent == null)
		{
			CreateConnections();
		}
		State.ExecuteEntry(sig, p1, p2);
		List<SignalState>.Enumerator enumerator = State.InitialStates.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SignalState current = enumerator.Current;
			SubRuns = SubRuns ?? new List<SignalStateRun>();
			SignalStateRun signalStateRun = new SignalStateRun(current, this);
			SubRuns.Add(signalStateRun);
			signalStateRun.Start(sig, p1, p2);
		}
	}

	public void Start<P1, P2, P3>(Signal<P1, P2, P3> sig, P1 p1, P2 p2, P3 p3)
	{
		if (Parent == null)
		{
			CreateConnections();
		}
		State.ExecuteEntry(sig, p1, p2, p3);
		List<SignalState>.Enumerator enumerator = State.InitialStates.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SignalState current = enumerator.Current;
			SubRuns = SubRuns ?? new List<SignalStateRun>();
			SignalStateRun signalStateRun = new SignalStateRun(current, this);
			SubRuns.Add(signalStateRun);
			signalStateRun.Start(sig, p1, p2, p3);
		}
	}

	public void Start<P1, P2, P3, P4>(Signal<P1, P2, P3, P4> sig, P1 p1, P2 p2, P3 p3, P4 p4)
	{
		if (Parent == null)
		{
			CreateConnections();
		}
		State.ExecuteEntry(sig, p1, p2, p3, p4);
		List<SignalState>.Enumerator enumerator = State.InitialStates.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SignalState current = enumerator.Current;
			SubRuns = SubRuns ?? new List<SignalStateRun>();
			SignalStateRun signalStateRun = new SignalStateRun(current, this);
			SubRuns.Add(signalStateRun);
			signalStateRun.Start(sig, p1, p2, p3, p4);
		}
	}

	public void Stop()
	{
		if (SubRuns != null)
		{
			List<SignalStateRun>.Enumerator enumerator = SubRuns.GetEnumerator();
			while (enumerator.MoveNext())
			{
				SignalStateRun current = enumerator.Current;
				current.Stop();
			}
		}
		State.ExecuteExit();
		if (Parent == null)
		{
			DestroyConnections();
		}
	}

	public void Stop(Signal sig)
	{
		if (SubRuns != null)
		{
			List<SignalStateRun>.Enumerator enumerator = SubRuns.GetEnumerator();
			while (enumerator.MoveNext())
			{
				SignalStateRun current = enumerator.Current;
				current.Stop(sig);
			}
			SubRuns.Clear();
		}
		State.ExecuteExit(sig);
		if (Parent == null)
		{
			DestroyConnections();
		}
	}

	public void Stop<P1>(Signal<P1> sig, P1 p1)
	{
		if (SubRuns != null)
		{
			List<SignalStateRun>.Enumerator enumerator = SubRuns.GetEnumerator();
			while (enumerator.MoveNext())
			{
				SignalStateRun current = enumerator.Current;
				current.Stop(sig, p1);
			}
			SubRuns.Clear();
		}
		State.ExecuteExit(sig, p1);
		if (Parent == null)
		{
			DestroyConnections();
		}
	}

	public void Stop<P1, P2>(Signal<P1, P2> sig, P1 p1, P2 p2)
	{
		if (SubRuns != null)
		{
			List<SignalStateRun>.Enumerator enumerator = SubRuns.GetEnumerator();
			while (enumerator.MoveNext())
			{
				SignalStateRun current = enumerator.Current;
				current.Stop(sig, p1, p2);
			}
			SubRuns.Clear();
		}
		State.ExecuteExit(sig, p1, p2);
		if (Parent == null)
		{
			DestroyConnections();
		}
	}

	public void Stop<P1, P2, P3>(Signal<P1, P2, P3> sig, P1 p1, P2 p2, P3 p3)
	{
		if (SubRuns != null)
		{
			List<SignalStateRun>.Enumerator enumerator = SubRuns.GetEnumerator();
			while (enumerator.MoveNext())
			{
				SignalStateRun current = enumerator.Current;
				current.Stop(sig, p1, p2, p3);
			}
			SubRuns.Clear();
		}
		State.ExecuteExit(sig, p1, p2, p3);
		if (Parent == null)
		{
			DestroyConnections();
		}
	}

	public void Stop<P1, P2, P3, P4>(Signal<P1, P2, P3, P4> sig, P1 p1, P2 p2, P3 p3, P4 p4)
	{
		if (SubRuns != null)
		{
			List<SignalStateRun>.Enumerator enumerator = SubRuns.GetEnumerator();
			while (enumerator.MoveNext())
			{
				SignalStateRun current = enumerator.Current;
				current.Stop(sig, p1, p2, p3, p4);
			}
			SubRuns.Clear();
		}
		State.ExecuteExit(sig, p1, p2, p3, p4);
		if (Parent == null)
		{
			DestroyConnections();
		}
	}

	private void CreateConnections()
	{
		if (Parent != null)
		{
			throw new Exception("Only root state should connect");
		}
		Connections = new List<SignalConnection>();
		List<SignalHandler> list = new List<SignalHandler>();
		State.CollectAllHandlers(list);
		List<SignalHandler>.Enumerator enumerator = list.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SignalHandler h = enumerator.Current;
			if (!Connections.Exists((SignalConnection c) => c.Signal == h.Signal))
			{
				Connections.Add(h.CreateConnection(this));
			}
		}
	}

	private void DestroyConnections()
	{
		List<SignalConnection>.Enumerator enumerator = Connections.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SignalConnection current = enumerator.Current;
			current.Disconnect();
		}
	}

	public void HandleSignal(Signal sig)
	{
		if (IsHandlingSignal)
		{
			BacklogSignal(sig);
			return;
		}
		IsHandlingSignal = true;
		bool trapped;
		SignalTransition signalTransition = TraverseSignal(sig, out trapped);
		if (signalTransition != null)
		{
			throw new Exception("Impossible transaction happened in state machine " + State.Name + ", verify graph integrity");
		}
		IsHandlingSignal = false;
		HandleBacklog();
	}

	private void HandleBacklog()
	{
		if (BackloggedSignals != null && BackloggedSignals.Count > 0)
		{
			Action action = BackloggedSignals.Dequeue();
			action();
		}
	}

	private void BacklogSignal(Signal sig)
	{
		BackloggedSignals = BackloggedSignals ?? new Queue<Action>();
		BackloggedSignals.Enqueue(delegate
		{
			HandleSignal(sig);
		});
	}

	private SignalTransition TraverseSignal(Signal sig, out bool trapped)
	{
		SignalTransition signalTransition = CallHandlers(sig, SignalHandler.TimingType.PreChildren, out trapped);
		if (signalTransition != null)
		{
			if (signalTransition.Target.Parent == State)
			{
				if (SubRuns.Count > 1)
				{
					throw new Exception("Transition to direct child is not supported for states with regions; add the transition to the parent region itself");
				}
				SubRuns[0].Stop(sig);
				SubRuns[0].State = signalTransition.Target;
				SubRuns[0].Start(sig);
				return null;
			}
			return signalTransition;
		}
		if (trapped)
		{
			return null;
		}
		if (SubRuns != null)
		{
			List<SignalStateRun>.Enumerator enumerator = SubRuns.GetEnumerator();
			while (enumerator.MoveNext())
			{
				SignalStateRun current = enumerator.Current;
				signalTransition = current.TraverseSignal(sig, out trapped);
				if (signalTransition != null)
				{
					if (signalTransition.Target.Parent != State)
					{
						return signalTransition;
					}
					current.Stop(sig);
					current.State = signalTransition.Target;
					current.Start(sig);
				}
			}
		}
		if (trapped)
		{
			return null;
		}
		signalTransition = CallHandlers(sig, SignalHandler.TimingType.PostChildren, out trapped);
		if (signalTransition != null)
		{
			if (signalTransition.Target.Parent == State)
			{
				if (SubRuns.Count > 1)
				{
					throw new Exception("Transition to direct child is not supported for states with regions; add the transition to the parent region itself");
				}
				SubRuns[0].Stop(sig);
				SubRuns[0].State = signalTransition.Target;
				SubRuns[0].Start(sig);
				return null;
			}
			return signalTransition;
		}
		return null;
	}

	private SignalTransition CallHandlers(Signal sig, SignalHandler.TimingType timing, out bool trapped)
	{
		if (State.Handlers == null)
		{
			trapped = false;
			return null;
		}
		List<SignalHandler>.Enumerator enumerator = State.Handlers.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SignalHandler current = enumerator.Current;
			if (current.Signal == sig && current.Timing == timing)
			{
				SignalTransition signalTransition = (current as SignalHandler0Param).Handle();
				trapped = current.Trap;
				if (signalTransition != null || trapped)
				{
					return signalTransition;
				}
			}
		}
		trapped = false;
		return null;
	}

	public void HandleSignal<P1>(Signal<P1> sig, P1 p1)
	{
		if (IsHandlingSignal)
		{
			BacklogSignal(sig, p1);
			return;
		}
		IsHandlingSignal = true;
		bool trapped;
		SignalTransition<P1> signalTransition = TraverseSignal(sig, p1, out trapped);
		if (signalTransition != null)
		{
			throw new Exception("Impossible transaction happened in state machine " + State.Name + ", verify graph integrity");
		}
		IsHandlingSignal = false;
		HandleBacklog();
	}

	private void BacklogSignal<P1>(Signal<P1> sig, P1 p1)
	{
		BackloggedSignals = BackloggedSignals ?? new Queue<Action>();
		BackloggedSignals.Enqueue(delegate
		{
			HandleSignal(sig, p1);
		});
	}

	private SignalTransition<P1> TraverseSignal<P1>(Signal<P1> sig, P1 p1, out bool trapped)
	{
		SignalTransition<P1> signalTransition = CallHandlers(sig, p1, SignalHandler.TimingType.PreChildren, out trapped);
		if (signalTransition != null)
		{
			if (signalTransition.Target.Parent == State)
			{
				if (SubRuns.Count > 1)
				{
					throw new Exception("Transition to direct child is not supported for states with regions; add the transition to the parent region itself");
				}
				SubRuns[0].Stop(sig, p1);
				SubRuns[0].State = signalTransition.Target;
				SubRuns[0].Start(sig, p1);
				return null;
			}
			return signalTransition;
		}
		if (trapped)
		{
			return null;
		}
		if (SubRuns != null)
		{
			List<SignalStateRun>.Enumerator enumerator = SubRuns.GetEnumerator();
			while (enumerator.MoveNext())
			{
				SignalStateRun current = enumerator.Current;
				signalTransition = current.TraverseSignal(sig, p1, out trapped);
				if (signalTransition != null)
				{
					if (signalTransition.Target.Parent != State)
					{
						return signalTransition;
					}
					current.Stop(sig, p1);
					current.State = signalTransition.Target;
					current.Start(sig, p1);
				}
			}
		}
		if (trapped)
		{
			return null;
		}
		signalTransition = CallHandlers(sig, p1, SignalHandler.TimingType.PostChildren, out trapped);
		if (signalTransition != null)
		{
			if (signalTransition.Target.Parent == State)
			{
				if (SubRuns.Count > 1)
				{
					throw new Exception("Transition to direct child is not supported for states with regions; add the transition to the parent region itself");
				}
				SubRuns[0].Stop(sig, p1);
				SubRuns[0].State = signalTransition.Target;
				SubRuns[0].Start(sig, p1);
				return null;
			}
			return signalTransition;
		}
		return null;
	}

	private SignalTransition<P1> CallHandlers<P1>(Signal<P1> sig, P1 p1, SignalHandler.TimingType timing, out bool trapped)
	{
		if (State.Handlers == null)
		{
			trapped = false;
			return null;
		}
		List<SignalHandler>.Enumerator enumerator = State.Handlers.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SignalHandler current = enumerator.Current;
			if (current.Signal == sig && current.Timing == timing)
			{
				SignalTransition<P1> signalTransition = (current as SignalHandler1Param<P1>).Handle(p1);
				trapped = current.Trap;
				if (signalTransition != null || trapped)
				{
					return signalTransition;
				}
			}
		}
		trapped = false;
		return null;
	}

	public void HandleSignal<P1, P2>(Signal<P1, P2> sig, P1 p1, P2 p2)
	{
		if (IsHandlingSignal)
		{
			BacklogSignal(sig, p1, p2);
			return;
		}
		IsHandlingSignal = true;
		bool trapped;
		SignalTransition<P1, P2> signalTransition = TraverseSignal(sig, p1, p2, out trapped);
		if (signalTransition != null)
		{
			throw new Exception("Impossible transaction happened in state machine " + State.Name + ", verify graph integrity");
		}
		IsHandlingSignal = false;
		HandleBacklog();
	}

	private void BacklogSignal<P1, P2>(Signal<P1, P2> sig, P1 p1, P2 p2)
	{
		BackloggedSignals = BackloggedSignals ?? new Queue<Action>();
		BackloggedSignals.Enqueue(delegate
		{
			HandleSignal(sig, p1, p2);
		});
	}

	private SignalTransition<P1, P2> TraverseSignal<P1, P2>(Signal<P1, P2> sig, P1 p1, P2 p2, out bool trapped)
	{
		SignalTransition<P1, P2> signalTransition = CallHandlers(sig, p1, p2, SignalHandler.TimingType.PreChildren, out trapped);
		if (signalTransition != null)
		{
			if (signalTransition.Target.Parent == State)
			{
				if (SubRuns.Count > 1)
				{
					throw new Exception("Transition to direct child is not supported for states with regions; add the transition to the parent region itself");
				}
				SubRuns[0].Stop(sig, p1, p2);
				SubRuns[0].State = signalTransition.Target;
				SubRuns[0].Start(sig, p1, p2);
				return null;
			}
			return signalTransition;
		}
		if (trapped)
		{
			return null;
		}
		if (SubRuns != null)
		{
			List<SignalStateRun>.Enumerator enumerator = SubRuns.GetEnumerator();
			while (enumerator.MoveNext())
			{
				SignalStateRun current = enumerator.Current;
				signalTransition = current.TraverseSignal(sig, p1, p2, out trapped);
				if (signalTransition != null)
				{
					if (signalTransition.Target.Parent != State)
					{
						return signalTransition;
					}
					current.Stop(sig, p1, p2);
					current.State = signalTransition.Target;
					current.Start(sig, p1, p2);
				}
			}
		}
		if (trapped)
		{
			return null;
		}
		signalTransition = CallHandlers(sig, p1, p2, SignalHandler.TimingType.PostChildren, out trapped);
		if (signalTransition != null)
		{
			if (signalTransition.Target.Parent == State)
			{
				if (SubRuns.Count > 1)
				{
					throw new Exception("Transition to direct child is not supported for states with regions; add the transition to the parent region itself");
				}
				SubRuns[0].Stop(sig, p1, p2);
				SubRuns[0].State = signalTransition.Target;
				SubRuns[0].Start(sig, p1, p2);
				return null;
			}
			return signalTransition;
		}
		return null;
	}

	private SignalTransition<P1, P2> CallHandlers<P1, P2>(Signal<P1, P2> sig, P1 p1, P2 p2, SignalHandler.TimingType timing, out bool trapped)
	{
		if (State.Handlers == null)
		{
			trapped = false;
			return null;
		}
		List<SignalHandler>.Enumerator enumerator = State.Handlers.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SignalHandler current = enumerator.Current;
			if (current.Signal == sig && current.Timing == timing)
			{
				SignalTransition<P1, P2> signalTransition = (current as SignalHandler2Params<P1, P2>).Handle(p1, p2);
				trapped = current.Trap;
				if (signalTransition != null || trapped)
				{
					return signalTransition;
				}
			}
		}
		trapped = false;
		return null;
	}

	public void HandleSignal<P1, P2, P3>(Signal<P1, P2, P3> sig, P1 p1, P2 p2, P3 p3)
	{
		if (IsHandlingSignal)
		{
			BacklogSignal(sig, p1, p2, p3);
			return;
		}
		IsHandlingSignal = true;
		bool trapped;
		SignalTransition<P1, P2, P3> signalTransition = TraverseSignal(sig, p1, p2, p3, out trapped);
		if (signalTransition != null)
		{
			throw new Exception("Impossible transaction happened in state machine " + State.Name + ", verify graph integrity");
		}
		IsHandlingSignal = false;
		HandleBacklog();
	}

	private void BacklogSignal<P1, P2, P3>(Signal<P1, P2, P3> sig, P1 p1, P2 p2, P3 p3)
	{
		BackloggedSignals = BackloggedSignals ?? new Queue<Action>();
		BackloggedSignals.Enqueue(delegate
		{
			HandleSignal(sig, p1, p2, p3);
		});
	}

	private SignalTransition<P1, P2, P3> TraverseSignal<P1, P2, P3>(Signal<P1, P2, P3> sig, P1 p1, P2 p2, P3 p3, out bool trapped)
	{
		SignalTransition<P1, P2, P3> signalTransition = CallHandlers(sig, p1, p2, p3, SignalHandler.TimingType.PreChildren, out trapped);
		if (signalTransition != null)
		{
			if (signalTransition.Target.Parent == State)
			{
				if (SubRuns.Count > 1)
				{
					throw new Exception("Transition to direct child is not supported for states with regions; add the transition to the parent region itself");
				}
				SubRuns[0].Stop(sig, p1, p2, p3);
				SubRuns[0].State = signalTransition.Target;
				SubRuns[0].Start(sig, p1, p2, p3);
				return null;
			}
			return signalTransition;
		}
		if (trapped)
		{
			return null;
		}
		if (SubRuns != null)
		{
			List<SignalStateRun>.Enumerator enumerator = SubRuns.GetEnumerator();
			while (enumerator.MoveNext())
			{
				SignalStateRun current = enumerator.Current;
				signalTransition = current.TraverseSignal(sig, p1, p2, p3, out trapped);
				if (signalTransition != null)
				{
					if (signalTransition.Target.Parent != State)
					{
						return signalTransition;
					}
					current.Stop(sig, p1, p2, p3);
					current.State = signalTransition.Target;
					current.Start(sig, p1, p2, p3);
				}
			}
		}
		if (trapped)
		{
			return null;
		}
		signalTransition = CallHandlers(sig, p1, p2, p3, SignalHandler.TimingType.PostChildren, out trapped);
		if (signalTransition != null)
		{
			if (signalTransition.Target.Parent == State)
			{
				if (SubRuns.Count > 1)
				{
					throw new Exception("Transition to direct child is not supported for states with regions; add the transition to the parent region itself");
				}
				SubRuns[0].Stop(sig, p1, p2, p3);
				SubRuns[0].State = signalTransition.Target;
				SubRuns[0].Start(sig, p1, p2, p3);
				return null;
			}
			return signalTransition;
		}
		return null;
	}

	private SignalTransition<P1, P2, P3> CallHandlers<P1, P2, P3>(Signal<P1, P2, P3> sig, P1 p1, P2 p2, P3 p3, SignalHandler.TimingType timing, out bool trapped)
	{
		if (State.Handlers == null)
		{
			trapped = false;
			return null;
		}
		List<SignalHandler>.Enumerator enumerator = State.Handlers.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SignalHandler current = enumerator.Current;
			if (current.Signal == sig && current.Timing == timing)
			{
				SignalTransition<P1, P2, P3> signalTransition = (current as SignalHandler3Params<P1, P2, P3>).Handle(p1, p2, p3);
				trapped = current.Trap;
				if (signalTransition != null || trapped)
				{
					return signalTransition;
				}
			}
		}
		trapped = false;
		return null;
	}

	public void HandleSignal<P1, P2, P3, P4>(Signal<P1, P2, P3, P4> sig, P1 p1, P2 p2, P3 p3, P4 p4)
	{
		if (IsHandlingSignal)
		{
			BacklogSignal(sig, p1, p2, p3, p4);
			return;
		}
		IsHandlingSignal = true;
		bool trapped;
		SignalTransition<P1, P2, P3, P4> signalTransition = TraverseSignal(sig, p1, p2, p3, p4, out trapped);
		if (signalTransition != null)
		{
			throw new Exception("Impossible transaction happened in state machine " + State.Name + ", verify graph integrity");
		}
		IsHandlingSignal = false;
		HandleBacklog();
	}

	private void BacklogSignal<P1, P2, P3, P4>(Signal<P1, P2, P3, P4> sig, P1 p1, P2 p2, P3 p3, P4 p4)
	{
		BackloggedSignals = BackloggedSignals ?? new Queue<Action>();
		BackloggedSignals.Enqueue(delegate
		{
			HandleSignal(sig, p1, p2, p3, p4);
		});
	}

	private SignalTransition<P1, P2, P3, P4> TraverseSignal<P1, P2, P3, P4>(Signal<P1, P2, P3, P4> sig, P1 p1, P2 p2, P3 p3, P4 p4, out bool trapped)
	{
		SignalTransition<P1, P2, P3, P4> signalTransition = CallHandlers(sig, p1, p2, p3, p4, SignalHandler.TimingType.PreChildren, out trapped);
		if (signalTransition != null)
		{
			if (signalTransition.Target.Parent == State)
			{
				if (SubRuns.Count > 1)
				{
					throw new Exception("Transition to direct child is not supported for states with regions; add the transition to the parent region itself");
				}
				SubRuns[0].Stop(sig, p1, p2, p3, p4);
				SubRuns[0].State = signalTransition.Target;
				SubRuns[0].Start(sig, p1, p2, p3, p4);
				return null;
			}
			return signalTransition;
		}
		if (trapped)
		{
			return null;
		}
		if (SubRuns != null)
		{
			List<SignalStateRun>.Enumerator enumerator = SubRuns.GetEnumerator();
			while (enumerator.MoveNext())
			{
				SignalStateRun current = enumerator.Current;
				signalTransition = current.TraverseSignal(sig, p1, p2, p3, p4, out trapped);
				if (signalTransition != null)
				{
					if (signalTransition.Target.Parent != State)
					{
						return signalTransition;
					}
					current.Stop(sig, p1, p2, p3, p4);
					current.State = signalTransition.Target;
					current.Start(sig, p1, p2, p3, p4);
				}
			}
		}
		if (trapped)
		{
			return null;
		}
		signalTransition = CallHandlers(sig, p1, p2, p3, p4, SignalHandler.TimingType.PostChildren, out trapped);
		if (signalTransition != null)
		{
			if (signalTransition.Target.Parent == State)
			{
				if (SubRuns.Count > 1)
				{
					throw new Exception("Transition to direct child is not supported for states with regions; add the transition to the parent region itself");
				}
				SubRuns[0].Stop(sig, p1, p2, p3, p4);
				SubRuns[0].State = signalTransition.Target;
				SubRuns[0].Start(sig, p1, p2, p3, p4);
				return null;
			}
			return signalTransition;
		}
		return null;
	}

	private SignalTransition<P1, P2, P3, P4> CallHandlers<P1, P2, P3, P4>(Signal<P1, P2, P3, P4> sig, P1 p1, P2 p2, P3 p3, P4 p4, SignalHandler.TimingType timing, out bool trapped)
	{
		if (State.Handlers == null)
		{
			trapped = false;
			return null;
		}
		List<SignalHandler>.Enumerator enumerator = State.Handlers.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SignalHandler current = enumerator.Current;
			if (current.Signal == sig && current.Timing == timing)
			{
				SignalTransition<P1, P2, P3, P4> signalTransition = (current as SignalHandler4Params<P1, P2, P3, P4>).Handle(p1, p2, p3, p4);
				trapped = current.Trap;
				if (signalTransition != null || trapped)
				{
					return signalTransition;
				}
			}
		}
		trapped = false;
		return null;
	}

	public string DebugCurrentStateTree(string indent = "-")
	{
		string text = indent + State.Name + "\n";
		if (SubRuns == null)
		{
			return text;
		}
		List<SignalStateRun>.Enumerator enumerator = SubRuns.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SignalStateRun current = enumerator.Current;
			text += current.DebugCurrentStateTree(indent + "   ");
		}
		return text;
	}
}
