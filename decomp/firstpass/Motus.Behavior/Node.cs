using System;
using System.Collections;
using System.Collections.Generic;
using Brix.External.Signals;
using Brix.Game.AI;
using Brix.Utils;
using Brix.Utils.Closures;
using Smooth.Slinq;
using UnityEngine;

namespace Motus.Behavior;

public abstract class Node : IDisposable
{
	public class Enumerator : IEnumerator<object>, IDisposable, IEnumerator
	{
		private NodeRun nodeRun;

		private NodeState state;

		private bool disposedValue;

		object IEnumerator<object>.Current => Current;

		object IEnumerator.Current => Current;

		public NodeState NodeState => state;

		public object Current => null;

		public Enumerator(NodeRun run)
		{
			nodeRun = run;
			state = NodeState.New;
		}

		public bool MoveNext()
		{
			state = nodeRun.Run();
			if (state == NodeState.Running)
			{
				return true;
			}
			Dispose();
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			return MoveNext();
		}

		void IEnumerator.Reset()
		{
		}

		public void Dispose()
		{
			if (!disposedValue)
			{
				disposedValue = true;
				nodeRun.Dispose();
				nodeRun = Empty.Spawn();
			}
		}
	}

	private static class ForEachBuilder<T>
	{
		public static Func<IEnumerable<T>, IEnumerable<T>> ReturnEnumerable = (IEnumerable<T> items) => items;

		public static Action<List<T>, Func<IEnumerable<T>>> UnwrapEnumerable = delegate(List<T> list, Func<IEnumerable<T>> items)
		{
			items().Slinq().AddTo(list);
		};
	}

	private static class ForEachBuilder<P1, T>
	{
		public static Action<List<T>, Func<P1, IEnumerable<T>>, P1> UnwrapEnumerable = delegate(List<T> list, Func<P1, IEnumerable<T>> items, P1 p1)
		{
			items(p1).Slinq().AddTo(list);
		};
	}

	private static class ForEachBuilder<P1, P2, T>
	{
		public static Action<List<T>, Func<P1, P2, IEnumerable<T>>, P1, P2> UnwrapEnumerable = delegate(List<T> list, Func<P1, P2, IEnumerable<T>> items, P1 p1, P2 p2)
		{
			items(p1, p2).Slinq().AddTo(list);
		};
	}

	private static class ForEachBuilder<P1, P2, P3, T>
	{
		public static Action<List<T>, Func<P1, P2, P3, IEnumerable<T>>, P1, P2, P3> UnwrapEnumerable = delegate(List<T> list, Func<P1, P2, P3, IEnumerable<T>> items, P1 p1, P2 p2, P3 p3)
		{
			items(p1, p2, p3).Slinq().AddTo(list);
		};
	}

	public string Label = string.Empty;

	public UnityEngine.Object Context;

	public IClosure<IAction> _finalizer = Brix.Utils.Empty.Closure;

	private static readonly StaticNode BreakInstance = Share(Fail(new LoopBreak()));

	private static readonly StaticNode ContinueInstance = Share(Fail(new LoopContinue()));

	private static Func<bool> _returnTrue = () => true;

	public IClosure<IAction> Finalizer
	{
		get
		{
			return _finalizer;
		}
		set
		{
			_finalizer.Dispose();
			_finalizer = value;
		}
	}

	public static Catcher.Builder Catcher => default(Catcher.Builder).Init();

	public static StaticNode Empty => EmptyNode._Node;

	public static StaticNode Forever => ForeverNode._Node;

	public static ParallelSequencer.Builder Parallel => default(ParallelSequencer.Builder).Init();

	public static Brancher.Builder Selector => default(Brancher.Builder).Init();

	public static Sequencer.Builder Sequence => default(Sequencer.Builder).Init();

	public static TimedSequencer.Builder TimedSequence => default(TimedSequencer.Builder).Init();

	public static Trier.Builder Trier => default(Trier.Builder).Init();

	public static Waiter.WaitConditionBuilder WaitFor => new Waiter.WaitConditionBuilder(FalseCondition.Instance, FalseCondition.Instance);

	public static StaticNode Do(Action a)
	{
		return ActionNode.Make(a);
	}

	public static StaticNode Do<T1>(Action<T1> a, T1 t1)
	{
		return ActionNode.Make(a, t1);
	}

	public static StaticNode Do<T1, T2>(Action<T1, T2> a, T1 t1, T2 t2)
	{
		return ActionNode.Make(a, t1, t2);
	}

	public static StaticNode Do<T1, T2, T3>(Action<T1, T2, T3> a, T1 t1, T2 t2, T3 t3)
	{
		return ActionNode.Make(a, t1, t2, t3);
	}

	public static StaticNode Do<T1, T2, T3, T4>(Action<T1, T2, T3, T4> a, T1 t1, T2 t2, T3 t3, T4 t4)
	{
		return ActionNode.Make(a, t1, t2, t3, t4);
	}

	public virtual bool Catches(FailException e)
	{
		return e.GetType() == typeof(FailException) || e.GetType().IsSubclassOf(typeof(FailException));
	}

	public abstract void Dispose();

	public abstract NodeRun Spawn();

	public abstract NodeRun SpawnWithArg(object arg);

	public Enumerator ToEnumerator()
	{
		return new Enumerator(Spawn());
	}

	public static ConditionNode.ConditionalNodeBuilder ConditionNode(Node n, bool isStopCondition)
	{
		return new ConditionNode.ConditionalNodeBuilder(n, isStopCondition, failOnStop: false, null);
	}

	public static ConditionNode.ConditionalNodeBuilder ConditionNode(Node n, bool isStopCondition, FailException e)
	{
		return new ConditionNode.ConditionalNodeBuilder(n, isStopCondition, failOnStop: true, e);
	}

	private static ConditionNode.ConditionalNodeBuilder ConditionNode(Node n, bool isStopCondition, bool failOnStop, FailException e)
	{
		return new ConditionNode.ConditionalNodeBuilder(n, isStopCondition, failOnStop, e);
	}

	public static StaticNode Do(Func<Node> f)
	{
		return DynamicNode.Make(f);
	}

	public static StaticNode Do<T1>(Func<T1, Node> f, T1 t1)
	{
		return DynamicNode.Make(f, t1);
	}

	public static StaticNode Do<T1, T2>(Func<T1, T2, Node> f, T1 t1, T2 t2)
	{
		return DynamicNode.Make(f, t1, t2);
	}

	public static StaticNode Do<T1, T2, T3>(Func<T1, T2, T3, Node> f, T1 t1, T2 t2, T3 t3)
	{
		return DynamicNode.Make(f, t1, t2, t3);
	}

	public static StaticNode Do<T1, T2, T3, T4>(Func<T1, T2, T3, T4, Node> f, T1 t1, T2 t2, T3 t3, T4 t4)
	{
		return DynamicNode.Make(f, t1, t2, t3, t4);
	}

	public static StaticNode Do(Func<StaticNode> f)
	{
		return DynamicNode.Make(f);
	}

	public static StaticNode Do<T1>(Func<T1, StaticNode> f, T1 t1)
	{
		return DynamicNode.Make(f, t1);
	}

	public static StaticNode Do<T1, T2>(Func<T1, T2, StaticNode> f, T1 t1, T2 t2)
	{
		return DynamicNode.Make(f, t1, t2);
	}

	public static StaticNode Do<T1, T2, T3>(Func<T1, T2, T3, StaticNode> f, T1 t1, T2 t2, T3 t3)
	{
		return DynamicNode.Make(f, t1, t2, t3);
	}

	public static StaticNode Do<T1, T2, T3, T4>(Func<T1, T2, T3, T4, StaticNode> f, T1 t1, T2 t2, T3 t3, T4 t4)
	{
		return DynamicNode.Make(f, t1, t2, t3, t4);
	}

	public static StaticNode Fail<T>() where T : SharedException<T>, new()
	{
		return Fail(SharedException<T>.Get);
	}

	public static StaticNode FailWork<T>() where T : SharedWorkException<T>, new()
	{
		return Fail(SharedWorkException<T>.Get);
	}

	public static StaticNode FailPathTo<T>(Location location) where T : Exceptions.PathException, new()
	{
		return Fail(Exceptions.PathException.To<T>(location));
	}

	public static StaticNode Fail(FailException e)
	{
		return Failer.Make(e);
	}

	public static ForEacher.Builder<T> ForEach<T>(IEnumerable<T> items)
	{
		return ForEach(ForEachBuilder<T>.ReturnEnumerable, items);
	}

	public static ForEacher.Builder<T> ForEach<T>(Func<IEnumerable<T>> itemsRetriever)
	{
		return new ForEacher.Builder<T>(ForEachBuilder<T>.UnwrapEnumerable.Make().Close(itemsRetriever).Pooled2());
	}

	public static ForEacher.Builder<T> ForEach<P1, T>(Func<P1, IEnumerable<T>> itemsRetriever, P1 p1)
	{
		return new ForEacher.Builder<T>(ForEachBuilder<P1, T>.UnwrapEnumerable.Make().Close(p1).Close(itemsRetriever)
			.Pooled2());
	}

	public static ForEacher.Builder<T> ForEach<P1, P2, T>(Func<P1, P2, IEnumerable<T>> itemsRetriever, P1 p1, P2 p2)
	{
		return new ForEacher.Builder<T>(ForEachBuilder<P1, P2, T>.UnwrapEnumerable.Make().Close(p2).Close(p1)
			.Close(itemsRetriever)
			.Pooled2());
	}

	public static ForEacher.Builder<T> ForEach<P1, P2, P3, T>(Func<P1, P2, P3, IEnumerable<T>> itemsRetriever, P1 p1, P2 p2, P3 p3)
	{
		return new ForEacher.Builder<T>(ForEachBuilder<P1, P2, P3, T>.UnwrapEnumerable.Make().Close(p3).Close(p2)
			.Close(p1)
			.Close(itemsRetriever)
			.Pooled2());
	}

	public static ForEacher.Builder<T> ForEach<T>(Action<List<T>> itemsRetriever)
	{
		return new ForEacher.Builder<T>(itemsRetriever.Make().Pooled2());
	}

	public static ForEacher.Builder<T> ForEach<P1, T>(Action<List<T>, P1> itemsRetriever, P1 p1)
	{
		return new ForEacher.Builder<T>(itemsRetriever.Make().Close(p1).Pooled2());
	}

	public static ForEacher.Builder<T> ForEach<P1, P2, T>(Action<List<T>, P1, P2> itemsRetriever, P1 p1, P2 p2)
	{
		return new ForEacher.Builder<T>(itemsRetriever.Make().Close(p2).Close(p1)
			.Pooled2());
	}

	public static ForEacher.Builder<T> ForEach<P1, P2, P3, T>(Action<List<T>, P1, P2, P3> itemsRetriever, P1 p1, P2 p2, P3 p3)
	{
		return new ForEacher.Builder<T>(itemsRetriever.Make().Close(p3).Close(p2)
			.Close(p1)
			.Pooled2());
	}

	public static ForEacher.Builder<T> ForEach<P1, P2, P3, P4, T>(Action<List<T>, P1, P2, P3, P4> itemsRetriever, P1 p1, P2 p2, P3 p3, P4 p4)
	{
		return new ForEacher.Builder<T>(itemsRetriever.Make().Close(p4).Close(p3)
			.Close(p2)
			.Close(p1)
			.Pooled2());
	}

	public static StaticNode Do<TReturn>(Func<TReturn> a)
	{
		return FuncNode.Make(a);
	}

	public static StaticNode Do<T1, TReturn>(Func<T1, TReturn> a, T1 t1)
	{
		return FuncNode.Make(a, t1);
	}

	public static StaticNode Do<T1, T2, TReturn>(Func<T1, T2, TReturn> a, T1 t1, T2 t2)
	{
		return FuncNode.Make(a, t1, t2);
	}

	public static StaticNode Do<T1, T2, T3, TReturn>(Func<T1, T2, T3, TReturn> a, T1 t1, T2 t2, T3 t3)
	{
		return FuncNode.Make(a, t1, t2, t3);
	}

	public static StaticNode Do<T1, T2, T3, T4, TReturn>(Func<T1, T2, T3, T4, TReturn> a, T1 t1, T2 t2, T3 t3, T4 t4)
	{
		return FuncNode.Make(a, t1, t2, t3, t4);
	}

	public static StaticNode Do<T1, T2, T3, T4, T5, TReturn>(Func<T1, T2, T3, T4, T5, TReturn> a, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
	{
		return FuncNode.Make(a, t1, t2, t3, t4, t5);
	}

	public static Looper.Builder Loop(Node node)
	{
		return new Looper.Builder(node);
	}

	public static Looper.Builder Loop(Func<Node> e)
	{
		return Loop(DynamicNode.Make(e));
	}

	public static Looper.Builder Loop<P1>(Func<P1, Node> e, P1 p1)
	{
		return Loop(DynamicNode.Make(e, p1));
	}

	public static Looper.Builder Loop<P1, P2>(Func<P1, P2, Node> e, P1 p1, P2 p2)
	{
		return Loop(DynamicNode.Make(e, p1, p2));
	}

	public static Looper.Builder Loop<P1, P2, P3>(Func<P1, P2, P3, Node> e, P1 p1, P2 p2, P3 p3)
	{
		return Loop(DynamicNode.Make(e, p1, p2, p3));
	}

	public static Looper.Builder Loop<P1, P2, P3, P4>(Func<P1, P2, P3, P4, Node> e, P1 p1, P2 p2, P3 p3, P4 p4)
	{
		return Loop(DynamicNode.Make(e, p1, p2, p3, p4));
	}

	public static Looper.Builder Loop(Func<StaticNode> e)
	{
		return Loop(DynamicNode.Make(e));
	}

	public static Looper.Builder Loop<P1>(Func<P1, StaticNode> e, P1 p1)
	{
		return Loop(DynamicNode.Make(e, p1));
	}

	public static Looper.Builder Loop<P1, P2>(Func<P1, P2, StaticNode> e, P1 p1, P2 p2)
	{
		return Loop(DynamicNode.Make(e, p1, p2));
	}

	public static Looper.Builder Loop<P1, P2, P3>(Func<P1, P2, P3, StaticNode> e, P1 p1, P2 p2, P3 p3)
	{
		return Loop(DynamicNode.Make(e, p1, p2, p3));
	}

	public static Looper.Builder Loop<P1, P2, P3, P4>(Func<P1, P2, P3, P4, StaticNode> e, P1 p1, P2 p2, P3 p3, P4 p4)
	{
		return Loop(DynamicNode.Make(e, p1, p2, p3, p4));
	}

	public static StaticNode Break()
	{
		return BreakInstance;
	}

	public static StaticNode Continue()
	{
		return ContinueInstance;
	}

	public static Sharer.Builder Share(Node node)
	{
		return new Sharer.Builder(node);
	}

	public static Shielder.Builder Shield(Node node)
	{
		return new Shielder.Builder(node);
	}

	public static StaticNode WaitForTicks(int ticks)
	{
		return Waiter.Make(ticks);
	}

	public static StaticNode WaitForMilliseconds(int ms)
	{
		return Waiter.Make(TimeCondition.Request((float)ms / 1000f)).Also().ToNode;
	}

	private static StaticNode WaitForCondition(IClosure<IFunc<bool>> f)
	{
		return Waiter.Make(PredicateCondition.Request(f)).ToNode;
	}

	public static StaticNode WaitForCondition(Func<bool> f)
	{
		return WaitForCondition(f.Make().Pooled2());
	}

	public static StaticNode WaitForCondition<T1>(Func<T1, bool> f, T1 t1)
	{
		return WaitForCondition(f.Make().Close(t1).Pooled2());
	}

	public static StaticNode WaitForCondition<T1, T2>(Func<T1, T2, bool> f, T1 t1, T2 t2)
	{
		return WaitForCondition(f.Make().Close(t2).Close(t1)
			.Pooled2());
	}

	public static StaticNode WaitForCondition<T1, T2, T3>(Func<T1, T2, T3, bool> f, T1 t1, T2 t2, T3 t3)
	{
		return WaitForCondition(f.Make().Close(t3).Close(t2)
			.Close(t1)
			.Pooled2());
	}

	public static StaticNode WaitForCondition<T1, T2, T3, T4>(Func<T1, T2, T3, T4, bool> f, T1 t1, T2 t2, T3 t3, T4 t4)
	{
		return WaitForCondition(f.Make().Close(t4).Close(t3)
			.Close(t2)
			.Close(t1)
			.Pooled2());
	}

	private static StaticNode WaitForCondition(IReadOnlySignal s, IClosure<IFunc<bool>> filter)
	{
		return Waiter.Make(SignalCondition.Request(s, filter)).ToNode;
	}

	private static StaticNode WaitForCondition<T1>(IReadOnlySignal<T1> s, IClosure<IFunc<T1, bool>> filter)
	{
		return Waiter.Make(SignalCondition.Request(s, filter)).ToNode;
	}

	private static StaticNode WaitForCondition<T1, T2>(IReadOnlySignal<T1, T2> s, IClosure<IFunc<T1, T2, bool>> filter)
	{
		return Waiter.Make(SignalCondition.Request(s, filter)).ToNode;
	}

	private static StaticNode WaitForCondition<T1, T2, T3>(IReadOnlySignal<T1, T2, T3> s, IClosure<IFunc<T1, T2, T3, bool>> filter)
	{
		return Waiter.Make(SignalCondition.Request(s, filter)).ToNode;
	}

	private static StaticNode WaitForCondition<T1, T2, T3, T4>(IReadOnlySignal<T1, T2, T3, T4> s, IClosure<IFunc<T1, T2, T3, T4, bool>> filter)
	{
		return Waiter.Make(SignalCondition.Request(s, filter)).ToNode;
	}

	public static StaticNode WaitForCondition(IReadOnlySignal s)
	{
		return WaitForCondition(s, _returnTrue.Make().Pooled2());
	}

	public static StaticNode WaitForCondition(IReadOnlySignal s, Func<bool> filter)
	{
		return WaitForCondition(s, filter.Make().Pooled2());
	}

	public static StaticNode WaitForCondition<T1>(IReadOnlySignal s, Func<T1, bool> filter, T1 t1)
	{
		return WaitForCondition(s, filter.Make().Close(t1).Pooled2());
	}

	public static StaticNode WaitForCondition<T1, T2>(IReadOnlySignal s, Func<T1, T2, bool> filter, T1 t1, T2 t2)
	{
		return WaitForCondition(s, filter.Make().Close(t2).Close(t1)
			.Pooled2());
	}

	public static StaticNode WaitForCondition<T1, T2, T3>(IReadOnlySignal s, Func<T1, T2, T3, bool> filter, T1 t1, T2 t2, T3 t3)
	{
		return WaitForCondition(s, filter.Make().Close(t3).Close(t2)
			.Close(t1)
			.Pooled2());
	}

	public static StaticNode WaitForCondition<T1, T2, T3, T4>(IReadOnlySignal s, Func<T1, T2, T3, T4, bool> filter, T1 t1, T2 t2, T3 t3, T4 t4)
	{
		return WaitForCondition(s, filter.Make().Close(t4).Close(t3)
			.Close(t2)
			.Close(t1)
			.Pooled2());
	}

	public static StaticNode WaitForCondition<T1, T2, T3, T4, T5>(IReadOnlySignal s, Func<T1, T2, T3, T4, T5, bool> filter, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
	{
		return WaitForCondition(s, filter.Make().Close(t5).Close(t4)
			.Close(t3)
			.Close(t2)
			.Close(t1)
			.Pooled2());
	}

	public static StaticNode WaitForCondition<T1>(IReadOnlySignal<T1> s)
	{
		return WaitForCondition(s, _returnTrue.Make().Pad<T1>().Pooled2());
	}

	public static StaticNode WaitForCondition<T1>(IReadOnlySignal<T1> s, Func<bool> filter)
	{
		return WaitForCondition(s, filter.Make().Pad<T1>().Pooled2());
	}

	public static StaticNode WaitForCondition<T1>(IReadOnlySignal<T1> s, Func<T1, bool> filter)
	{
		return WaitForCondition(s, filter.Make().Pooled2());
	}

	public static StaticNode WaitForCondition<T1, T2>(IReadOnlySignal<T1> s, Func<T1, T2, bool> filter, T2 t2)
	{
		return WaitForCondition(s, filter.Make().Close(t2).Pooled2());
	}

	public static StaticNode WaitForCondition<T1, T2, T3>(IReadOnlySignal<T1> s, Func<T1, T2, T3, bool> filter, T2 t2, T3 t3)
	{
		return WaitForCondition(s, filter.Make().Close(t3).Close(t2)
			.Pooled2());
	}

	public static StaticNode WaitForCondition<T1, T2, T3, T4>(IReadOnlySignal<T1> s, Func<T1, T2, T3, T4, bool> filter, T2 t2, T3 t3, T4 t4)
	{
		return WaitForCondition(s, filter.Make().Close(t4).Close(t3)
			.Close(t2)
			.Pooled2());
	}

	public static StaticNode WaitForCondition<T1, T2, T3, T4, T5>(IReadOnlySignal<T1> s, Func<T1, T2, T3, T4, T5, bool> filter, T2 t2, T3 t3, T4 t4, T5 t5)
	{
		return WaitForCondition(s, filter.Make().Close(t5).Close(t4)
			.Close(t3)
			.Close(t2)
			.Pooled2());
	}

	public static StaticNode WaitForCondition<T1, T2>(IReadOnlySignal<T1, T2> s)
	{
		return WaitForCondition(s, _returnTrue.Make().Pad<T1>().Pad<T2>()
			.Pooled2());
	}

	public static StaticNode WaitForCondition<T1, T2>(IReadOnlySignal<T1, T2> s, Func<bool> filter)
	{
		return WaitForCondition(s, filter.Make().Pad<T1>().Pad<T2>()
			.Pooled2());
	}

	public static StaticNode WaitForCondition<T1, T2>(IReadOnlySignal<T1, T2> s, Func<T1, bool> filter)
	{
		return WaitForCondition(s, filter.Make().Pad<T2>().Pooled2());
	}

	public static StaticNode WaitForCondition<T1, T2>(IReadOnlySignal<T1, T2> s, Func<T1, T2, bool> filter)
	{
		return WaitForCondition(s, filter.Make().Pooled2());
	}

	public static StaticNode WaitForCondition<T1, T2, T3>(IReadOnlySignal<T1, T2> s, Func<T1, T2, T3, bool> filter, T3 t3)
	{
		return WaitForCondition(s, filter.Make().Close(t3).Pooled2());
	}

	public static StaticNode WaitForCondition<T1, T2, T3, T4>(IReadOnlySignal<T1, T2> s, Func<T1, T2, T3, T4, bool> filter, T3 t3, T4 t4)
	{
		return WaitForCondition(s, filter.Make().Close(t4).Close(t3)
			.Pooled2());
	}

	public static StaticNode WaitForCondition<T1, T2, T3, T4, T5>(IReadOnlySignal<T1, T2> s, Func<T1, T2, T3, T4, T5, bool> filter, T3 t3, T4 t4, T5 t5)
	{
		return WaitForCondition(s, filter.Make().Close(t5).Close(t4)
			.Close(t3)
			.Pooled2());
	}

	public static StaticNode WaitForCondition<T1, T2, T3>(IReadOnlySignal<T1, T2, T3> s)
	{
		return WaitForCondition(s, _returnTrue.Make().Pad<T1>().Pad<T2>()
			.Pad<T3>()
			.Pooled2());
	}

	public static StaticNode WaitForCondition<T1, T2, T3>(IReadOnlySignal<T1, T2, T3> s, Func<bool> filter)
	{
		return WaitForCondition(s, filter.Make().Pad<T1>().Pad<T2>()
			.Pad<T3>()
			.Pooled2());
	}

	public static StaticNode WaitForCondition<T1, T2, T3>(IReadOnlySignal<T1, T2, T3> s, Func<T1, bool> filter)
	{
		return WaitForCondition(s, filter.Make().Pad<T2>().Pad<T3>()
			.Pooled2());
	}

	public static StaticNode WaitForCondition<T1, T2, T3>(IReadOnlySignal<T1, T2, T3> s, Func<T1, T2, bool> filter)
	{
		return WaitForCondition(s, filter.Make().Pad<T3>().Pooled2());
	}

	public static StaticNode WaitForCondition<T1, T2, T3>(IReadOnlySignal<T1, T2, T3> s, Func<T1, T2, T3, bool> filter)
	{
		return WaitForCondition(s, filter.Make().Pooled2());
	}

	public static StaticNode WaitForCondition<T1, T2, T3, T4>(IReadOnlySignal<T1, T2, T3> s, Func<T1, T2, T3, T4, bool> filter, T4 t4)
	{
		return WaitForCondition(s, filter.Make().Close(t4).Pooled2());
	}

	public static StaticNode WaitForCondition<T1, T2, T3, T4, T5>(IReadOnlySignal<T1, T2, T3> s, Func<T1, T2, T3, T4, T5, bool> filter, T4 t4, T5 t5)
	{
		return WaitForCondition(s, filter.Make().Close(t5).Close(t4)
			.Pooled2());
	}

	public static StaticNode WaitForCondition<T1, T2, T3, T4>(IReadOnlySignal<T1, T2, T3, T4> s)
	{
		return WaitForCondition(s, _returnTrue.Make().Pad<T1>().Pad<T2>()
			.Pad<T3>()
			.Pad<T4>()
			.Pooled2());
	}

	public static StaticNode WaitForCondition<T1, T2, T3, T4>(IReadOnlySignal<T1, T2, T3, T4> s, Func<bool> filter)
	{
		return WaitForCondition(s, filter.Make().Pad<T1>().Pad<T2>()
			.Pad<T3>()
			.Pad<T4>()
			.Pooled2());
	}

	public static StaticNode WaitForCondition<T1, T2, T3, T4>(IReadOnlySignal<T1, T2, T3, T4> s, Func<T1, bool> filter)
	{
		return WaitForCondition(s, filter.Make().Pad<T2>().Pad<T3>()
			.Pad<T4>()
			.Pooled2());
	}

	public static StaticNode WaitForCondition<T1, T2, T3, T4>(IReadOnlySignal<T1, T2, T3, T4> s, Func<T1, T2, bool> filter)
	{
		return WaitForCondition(s, filter.Make().Pad<T3>().Pad<T4>()
			.Pooled2());
	}

	public static StaticNode WaitForCondition<T1, T2, T3, T4>(IReadOnlySignal<T1, T2, T3, T4> s, Func<T1, T2, T3, bool> filter)
	{
		return WaitForCondition(s, filter.Make().Pad<T4>().Pooled2());
	}

	public static StaticNode WaitForCondition<T1, T2, T3, T4>(IReadOnlySignal<T1, T2, T3, T4> s, Func<T1, T2, T3, T4, bool> filter)
	{
		return WaitForCondition(s, filter.Make().Pooled2());
	}

	public static StaticNode WaitForCondition<T1, T2, T3, T4, T5>(IReadOnlySignal<T1, T2, T3, T4> s, Func<T1, T2, T3, T4, T5, bool> filter, T5 t5)
	{
		return WaitForCondition(s, filter.Make().Close(t5).Pooled2());
	}
}
