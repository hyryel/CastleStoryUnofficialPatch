using System;
using System.Collections.Generic;
using Brix.External.Signals;
using Brix.Utils;

namespace FSM;

public class SignalStateMachine
{
	public class BuilderException : Exception
	{
		public BuilderException(string desc)
			: base(desc)
		{
		}
	}

	public class Builder
	{
		internal abstract class IHandlerBuilder
		{
			public Builder Owner;

			public string Target;

			public SignalHandler.TimingType Timing;

			public bool Trap;

			public abstract void Solve();
		}

		internal class HandlerBuilder : IHandlerBuilder
		{
			public Func<bool> Guard;

			public Action Reaction;

			public Signal Signal;

			public HandlerBuilder(Builder owner, Signal sig)
			{
				Owner = owner;
				Signal = sig;
			}

			public override void Solve()
			{
				if (Target == null && Reaction == null)
				{
					throw new BuilderException("called On with no Do or GoTo");
				}
				if (Target != null && Reaction != null)
				{
					throw new BuilderException("handler is both a transition and reaction, unsupported (yet)");
				}
				if (Target != null)
				{
					SolveTransition();
				}
				else
				{
					SolveReaction();
				}
			}

			private void SolveTransition()
			{
				SignalState signalState = Owner.nameMap[Target];
				if (signalState == null)
				{
					throw new BuilderException("invalid target, " + Target);
				}
				Owner.state.Transit(Signal, signalState, Guard, Timing, Trap);
			}

			private void SolveReaction()
			{
				Owner.state.React(Signal, Reaction, Timing, Trap);
			}
		}

		internal class HandlerBuilder<P1> : IHandlerBuilder
		{
			public Func<P1, bool> Guard;

			public Action<P1> Reaction;

			public Signal<P1> Signal;

			public HandlerBuilder(Builder owner, Signal<P1> sig)
			{
				Owner = owner;
				Signal = sig;
			}

			public override void Solve()
			{
				if (Target == null && Reaction == null)
				{
					throw new BuilderException("called On with no Do or GoTo");
				}
				if (Target != null && Reaction != null)
				{
					throw new BuilderException("handler is both a transition and reaction, unsupported (yet)");
				}
				if (Target != null)
				{
					SolveTransition();
				}
				else
				{
					SolveReaction();
				}
			}

			private void SolveTransition()
			{
				SignalState signalState = Owner.nameMap[Target];
				if (signalState == null)
				{
					throw new BuilderException("invalid target, " + Target);
				}
				Owner.state.Transit(Signal, signalState, Guard, Timing, Trap);
			}

			private void SolveReaction()
			{
				Owner.state.React(Signal, Reaction, Timing, Trap);
			}
		}

		internal class HandlerBuilder<P1, P2> : IHandlerBuilder
		{
			public Func<P1, P2, bool> Guard;

			public Action<P1, P2> Reaction;

			public Signal<P1, P2> Signal;

			public HandlerBuilder(Builder owner, Signal<P1, P2> sig)
			{
				Owner = owner;
				Signal = sig;
			}

			public override void Solve()
			{
				if (Target == null && Reaction == null)
				{
					throw new BuilderException("called On with no Do or GoTo");
				}
				if (Target != null && Reaction != null)
				{
					throw new BuilderException("handler is both a transition and reaction, unsupported (yet)");
				}
				if (Target != null)
				{
					SolveTransition();
				}
				else
				{
					SolveReaction();
				}
			}

			private void SolveTransition()
			{
				SignalState signalState = Owner.nameMap[Target];
				if (signalState == null)
				{
					throw new BuilderException("invalid target, " + Target);
				}
				Owner.state.Transit(Signal, signalState, Guard, Timing, Trap);
			}

			private void SolveReaction()
			{
				Owner.state.React(Signal, Reaction, Timing, Trap);
			}
		}

		internal class HandlerBuilder<P1, P2, P3> : IHandlerBuilder
		{
			public Func<P1, P2, P3, bool> Guard;

			public Action<P1, P2, P3> Reaction;

			public Signal<P1, P2, P3> Signal;

			public HandlerBuilder(Builder owner, Signal<P1, P2, P3> sig)
			{
				Owner = owner;
				Signal = sig;
			}

			public override void Solve()
			{
				if (Target == null && Reaction == null)
				{
					throw new BuilderException("called On with no Do or GoTo");
				}
				if (Target != null && Reaction != null)
				{
					throw new BuilderException("handler is both a transition and reaction, unsupported (yet)");
				}
				if (Target != null)
				{
					SolveTransition();
				}
				else
				{
					SolveReaction();
				}
			}

			private void SolveTransition()
			{
				SignalState signalState = Owner.nameMap[Target];
				if (signalState == null)
				{
					throw new BuilderException("invalid target, " + Target);
				}
				Owner.state.Transit(Signal, signalState, Guard, Timing, Trap);
			}

			private void SolveReaction()
			{
				Owner.state.React(Signal, Reaction, Timing, Trap);
			}
		}

		internal class HandlerBuilder<P1, P2, P3, P4> : IHandlerBuilder
		{
			public Func<P1, P2, P3, P4, bool> Guard;

			public Action<P1, P2, P3, P4> Reaction;

			public Signal<P1, P2, P3, P4> Signal;

			public HandlerBuilder(Builder owner, Signal<P1, P2, P3, P4> sig)
			{
				Owner = owner;
				Signal = sig;
			}

			public override void Solve()
			{
				if (Target == null && Reaction == null)
				{
					throw new BuilderException("called On with no Do or GoTo");
				}
				if (Target != null && Reaction != null)
				{
					throw new BuilderException("handler is both a transition and reaction, unsupported (yet)");
				}
				if (Target != null)
				{
					SolveTransition();
				}
				else
				{
					SolveReaction();
				}
			}

			private void SolveTransition()
			{
				SignalState signalState = Owner.nameMap[Target];
				if (signalState == null)
				{
					throw new BuilderException("invalid target, " + Target);
				}
				Owner.state.Transit(Signal, signalState, Guard, Timing, Trap);
			}

			private void SolveReaction()
			{
				Owner.state.React(Signal, Reaction, Timing, Trap);
			}
		}

		public Builder parent;

		public SignalState state;

		private List<IHandlerBuilder> pendingHandlers = new List<IHandlerBuilder>();

		private List<Builder> children = new List<Builder>();

		private Dictionary<string, SignalState> nameMap;

		private IHandlerBuilder currentHandler;

		public Builder(string name, Dictionary<string, SignalState> names)
		{
			state = new SignalState(name);
			nameMap = names;
			if (nameMap.ContainsKey(name))
			{
				throw new BuilderException("state has duplicate name, " + name + ", not supported in Builder");
			}
			names[name] = state;
		}

		public Builder Begin(string name)
		{
			Builder builder = new Builder(name, nameMap);
			children.Add(builder);
			builder.parent = this;
			state.AddSubState(builder.state);
			return builder;
		}

		public Builder BeginRegion(string name)
		{
			Builder builder = new Builder(name, nameMap);
			children.Add(builder);
			builder.parent = this;
			state.AddRegion(builder.state);
			return builder;
		}

		public Builder Initial()
		{
			if (parent == null)
			{
				throw new BuilderException("root state must be unique and is implicitly initial");
			}
			parent.state.AddInitialSubState(state);
			return this;
		}

		public Builder Entry(Action e)
		{
			state.Entry(e);
			return this;
		}

		public Builder Entry(Signal sig, Action e)
		{
			state.Entry(sig, e);
			return this;
		}

		public Builder Entry<P1>(Signal<P1> sig, Action<P1> e)
		{
			state.Entry(sig, e);
			return this;
		}

		public Builder Entry<P1, P2>(Signal<P1, P2> sig, Action<P1, P2> e)
		{
			state.Entry(sig, e);
			return this;
		}

		public Builder Entry<P1, P2, P3>(Signal<P1, P2, P3> sig, Action<P1, P2, P3> e)
		{
			state.Entry(sig, e);
			return this;
		}

		public Builder Entry<P1, P2, P3, P4>(Signal<P1, P2, P3, P4> sig, Action<P1, P2, P3, P4> e)
		{
			state.Entry(sig, e);
			return this;
		}

		public Builder Exit(Action e)
		{
			state.Exit(e);
			return this;
		}

		public Builder Exit(Signal sig, Action e)
		{
			state.Exit(sig, e);
			return this;
		}

		public Builder Exit<P1>(Signal<P1> sig, Action<P1> e)
		{
			state.Exit(sig, e);
			return this;
		}

		public Builder Exit<P1, P2>(Signal<P1, P2> sig, Action<P1, P2> e)
		{
			state.Exit(sig, e);
			return this;
		}

		public Builder Exit<P1, P2, P3>(Signal<P1, P2, P3> sig, Action<P1, P2, P3> e)
		{
			state.Exit(sig, e);
			return this;
		}

		public Builder Exit<P1, P2, P3, P4>(Signal<P1, P2, P3, P4> sig, Action<P1, P2, P3, P4> e)
		{
			state.Exit(sig, e);
			return this;
		}

		public Builder End()
		{
			if (parent == null)
			{
				SolveHandlers();
				return this;
			}
			return parent;
		}

		public Builder On(Signal sig)
		{
			currentHandler = new HandlerBuilder(this, sig);
			pendingHandlers.Add(currentHandler);
			return this;
		}

		public Builder On<P1>(Signal<P1> sig)
		{
			currentHandler = new HandlerBuilder<P1>(this, sig);
			pendingHandlers.Add(currentHandler);
			return this;
		}

		public Builder On<P1, P2>(Signal<P1, P2> sig)
		{
			currentHandler = new HandlerBuilder<P1, P2>(this, sig);
			pendingHandlers.Add(currentHandler);
			return this;
		}

		public Builder On<P1, P2, P3>(Signal<P1, P2, P3> sig)
		{
			currentHandler = new HandlerBuilder<P1, P2, P3>(this, sig);
			pendingHandlers.Add(currentHandler);
			return this;
		}

		public Builder On<P1, P2, P3, P4>(Signal<P1, P2, P3, P4> sig)
		{
			currentHandler = new HandlerBuilder<P1, P2, P3, P4>(this, sig);
			pendingHandlers.Add(currentHandler);
			return this;
		}

		public Builder PreChildren()
		{
			currentHandler.Timing = SignalHandler.TimingType.PreChildren;
			return this;
		}

		public Builder PostChildren()
		{
			currentHandler.Timing = SignalHandler.TimingType.PostChildren;
			return this;
		}

		public Builder Trap()
		{
			currentHandler.Trap = true;
			return this;
		}

		public Builder Do(Action reaction)
		{
			if (currentHandler == null)
			{
				throw new BuilderException("must call On before Do");
			}
			if (!(currentHandler is HandlerBuilder))
			{
				throw new BuilderException("incompatible reaction for signal while building state " + state.Name);
			}
			HandlerBuilder obj = currentHandler as HandlerBuilder;
			obj.Reaction = (Action)Delegate.Combine(obj.Reaction, reaction);
			return this;
		}

		public Builder Do<P1>(Action<P1> reaction)
		{
			if (currentHandler == null)
			{
				throw new BuilderException("must call On before Do");
			}
			if (!(currentHandler is HandlerBuilder<P1>))
			{
				throw new BuilderException("incompatible reaction for signal while building state " + state.Name);
			}
			HandlerBuilder<P1> obj = currentHandler as HandlerBuilder<P1>;
			obj.Reaction = (Action<P1>)Delegate.Combine(obj.Reaction, reaction.GetSafeInvoker());
			return this;
		}

		public Builder Do<P1, P2>(Action<P1, P2> reaction)
		{
			if (currentHandler == null)
			{
				throw new BuilderException("must call On before Do");
			}
			if (!(currentHandler is HandlerBuilder<P1, P2>))
			{
				throw new BuilderException("incompatible reaction for message type while bulding state " + state.Name);
			}
			HandlerBuilder<P1, P2> obj = currentHandler as HandlerBuilder<P1, P2>;
			obj.Reaction = (Action<P1, P2>)Delegate.Combine(obj.Reaction, reaction.GetSafeInvoker());
			return this;
		}

		public Builder Do<P1, P2, P3>(Action<P1, P2, P3> reaction)
		{
			if (currentHandler == null)
			{
				throw new BuilderException("must call On before Do");
			}
			if (!(currentHandler is HandlerBuilder<P1, P2, P3>))
			{
				throw new BuilderException("incompatible reaction for signal while building state " + state.Name);
			}
			HandlerBuilder<P1, P2, P3> obj = currentHandler as HandlerBuilder<P1, P2, P3>;
			obj.Reaction = (Action<P1, P2, P3>)Delegate.Combine(obj.Reaction, reaction.GetSafeInvoker());
			return this;
		}

		public Builder Do<P1, P2, P3, P4>(Action<P1, P2, P3, P4> reaction)
		{
			if (currentHandler == null)
			{
				throw new BuilderException("must call On before Do");
			}
			if (!(currentHandler is HandlerBuilder<P1, P2, P3, P4>))
			{
				throw new BuilderException("incompatible reaction for signal while building state " + state.Name);
			}
			HandlerBuilder<P1, P2, P3, P4> obj = currentHandler as HandlerBuilder<P1, P2, P3, P4>;
			obj.Reaction = (Action<P1, P2, P3, P4>)Delegate.Combine(obj.Reaction, reaction.GetSafeInvoker());
			return this;
		}

		public Builder Do<P1>(Action reaction)
		{
			Do((Action<P1>)delegate
			{
				reaction();
			});
			return this;
		}

		public Builder Do<P1, P2>(Action reaction)
		{
			Do((Action<P1, P2>)delegate
			{
				reaction();
			});
			return this;
		}

		public Builder Do<P1, P2, P3>(Action reaction)
		{
			Do((Action<P1, P2, P3>)delegate
			{
				reaction();
			});
			return this;
		}

		public Builder Do<P1, P2, P3, P4>(Action reaction)
		{
			Do((Action<P1, P2, P3, P4>)delegate
			{
				reaction();
			});
			return this;
		}

		public Builder GoTo(string target)
		{
			currentHandler.Target = target;
			return this;
		}

		public Builder If(Func<bool> guard)
		{
			if (currentHandler == null)
			{
				throw new BuilderException("must call GoTo before If");
			}
			if (!(currentHandler is HandlerBuilder))
			{
				throw new BuilderException("incompatible reaction for signal while building state " + state.Name);
			}
			HandlerBuilder obj = currentHandler as HandlerBuilder;
			obj.Guard = (Func<bool>)Delegate.Combine(obj.Guard, guard.GetSafeInvoker());
			return this;
		}

		public Builder If<P1>(Func<P1, bool> guard)
		{
			if (currentHandler == null)
			{
				throw new BuilderException("must call GoTo before If");
			}
			if (!(currentHandler is HandlerBuilder<P1>))
			{
				throw new BuilderException("incompatible reaction for signal while building state " + state.Name);
			}
			HandlerBuilder<P1> obj = currentHandler as HandlerBuilder<P1>;
			obj.Guard = (Func<P1, bool>)Delegate.Combine(obj.Guard, guard.GetSafeInvoker());
			return this;
		}

		public Builder If<P1, P2>(Func<P1, P2, bool> guard)
		{
			if (currentHandler == null)
			{
				throw new BuilderException("must call GoTo before If");
			}
			if (!(currentHandler is HandlerBuilder<P1, P2>))
			{
				throw new BuilderException("incompatible reaction for signal while building state " + state.Name);
			}
			HandlerBuilder<P1, P2> obj = currentHandler as HandlerBuilder<P1, P2>;
			obj.Guard = (Func<P1, P2, bool>)Delegate.Combine(obj.Guard, guard.GetSafeInvoker());
			return this;
		}

		public Builder If<P1, P2, P3>(Func<P1, P2, P3, bool> guard)
		{
			if (currentHandler == null)
			{
				throw new BuilderException("must call GoTo before If");
			}
			if (!(currentHandler is HandlerBuilder<P1, P2, P3>))
			{
				throw new BuilderException("incompatible reaction for signal while building state " + state.Name);
			}
			HandlerBuilder<P1, P2, P3> obj = currentHandler as HandlerBuilder<P1, P2, P3>;
			obj.Guard = (Func<P1, P2, P3, bool>)Delegate.Combine(obj.Guard, guard.GetSafeInvoker());
			return this;
		}

		public Builder If<P1, P2, P3, P4>(Func<P1, P2, P3, P4, bool> guard)
		{
			if (currentHandler == null)
			{
				throw new BuilderException("must call GoTo before If");
			}
			if (!(currentHandler is HandlerBuilder<P1, P2, P3, P4>))
			{
				throw new BuilderException("incompatible reaction for signal while building state " + state.Name);
			}
			HandlerBuilder<P1, P2, P3, P4> obj = currentHandler as HandlerBuilder<P1, P2, P3, P4>;
			obj.Guard = (Func<P1, P2, P3, P4, bool>)Delegate.Combine(obj.Guard, guard.GetSafeInvoker());
			return this;
		}

		public Builder If<P1>(Func<bool> guard)
		{
			If((P1 m) => guard());
			return this;
		}

		public Builder If<P1, P2>(Func<bool> guard)
		{
			If((P1 m, P2 n) => guard());
			return this;
		}

		public Builder If<P1, P2, P3>(Func<bool> guard)
		{
			If((P1 m, P2 n, P3 o) => guard());
			return this;
		}

		public Builder If<P1, P2, P3, P4>(Func<bool> guard)
		{
			If((P1 m, P2 n, P3 o, P4 p) => guard());
			return this;
		}

		public Builder Late()
		{
			if (currentHandler == null)
			{
				throw new BuilderException("must call GoTo before If");
			}
			currentHandler.Timing = SignalHandler.TimingType.PostChildren;
			return this;
		}

		public static implicit operator SignalState(Builder b)
		{
			if (b.parent != null)
			{
				throw new BuilderException("cannot extract State from non-closed declaration, make sure your Begins and Ends are balanced");
			}
			return b.state;
		}

		private void SolveHandlers()
		{
			foreach (IHandlerBuilder pendingHandler in pendingHandlers)
			{
				pendingHandler.Solve();
			}
			foreach (Builder child in children)
			{
				child.SolveHandlers();
			}
		}
	}

	public static Builder Begin(string name)
	{
		Builder builder = new Builder(name, new Dictionary<string, SignalState>());
		builder.parent = null;
		return builder;
	}
}
