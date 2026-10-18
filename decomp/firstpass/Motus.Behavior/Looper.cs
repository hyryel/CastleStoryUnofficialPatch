using System;
using System.Diagnostics;
using Brix.External.Signals;
using Brix.Utils.Closures;
using UnityEngine;

namespace Motus.Behavior;

public static class Looper
{
	public class LoopNode : PooledNode<LoopNode>
	{
		internal Node _loopedNode;

		internal ICondition _condition = TrueCondition.Instance;

		internal long _maxTicksPerRun;

		internal long _ticksBetweenRun;

		internal bool _loopsWhenTrue = true;

		public override NodeRun Spawn()
		{
			return LoopNodeRun.Request(this, _condition);
		}

		public override NodeRun SpawnWithArg(object arg)
		{
			return Spawn();
		}

		protected override void Clear()
		{
			if (_loopedNode != null)
			{
				_loopedNode.Dispose();
				_loopedNode = null;
			}
			_condition.Dispose();
			_condition = TrueCondition.Instance;
			_maxTicksPerRun = 0L;
			_ticksBetweenRun = 0L;
			_loopsWhenTrue = true;
		}

		public void SetUntilCondition(ICondition c)
		{
			SetCondition(c, loopsWhenTrue: false);
		}

		public void SetWhileCondition(ICondition c)
		{
			SetCondition(c, loopsWhenTrue: true);
		}

		private void SetCondition(ICondition c, bool loopsWhenTrue)
		{
			_condition.Dispose();
			_condition = c;
			_loopsWhenTrue = loopsWhenTrue;
		}

		public static LoopNode Request(Node looped)
		{
			LoopNode loopNode = PooledNode<LoopNode>.Request();
			loopNode._loopedNode = looped;
			return loopNode;
		}
	}

	public class LoopNodeRun : PooledNodeRun<LoopNodeRun>
	{
		private NodeRun _nodeRun;

		private ICondition _condition;

		private bool _loopsWhenTrue;

		private long _waitTicksLeft;

		private long _processingTicksLeft;

		private long _maxTicksPerRun;

		private long _ticksBetweenRuns;

		private Stopwatch _stopWatch = new Stopwatch();

		public static NodeRun Request(LoopNode node, ICondition loopCondition)
		{
			LoopNodeRun loopNodeRun = PooledNodeRun<LoopNodeRun>.Request(node.Label, node.Context, node.Finalizer.Delegate);
			loopNodeRun._nodeRun = node._loopedNode.Spawn();
			loopNodeRun._condition = loopCondition;
			loopNodeRun._loopsWhenTrue = node._loopsWhenTrue;
			loopNodeRun._maxTicksPerRun = node._maxTicksPerRun;
			loopNodeRun._ticksBetweenRuns = node._ticksBetweenRun;
			return loopNodeRun;
		}

		protected override void Init()
		{
			_nodeRun.Start();
			_processingTicksLeft = 0L;
			_waitTicksLeft = _ticksBetweenRuns;
			_condition.Init();
		}

		protected override void Tick()
		{
			CalculateAllowedTicks();
			if (HasProcessingTimeLeft() && !HasWaitingTimeLeft())
			{
				NodeState nodeState = TickNode();
				if (nodeState == NodeState.Fail && _nodeRun.Exception is LoopBreak)
				{
					base.State = NodeState.Done;
					return;
				}
				base.State = _nodeRun.State;
				Exception = _nodeRun.Exception;
			}
		}

		private NodeState TickNode()
		{
			StartTimeTracking();
			try
			{
				_nodeRun.Run();
				while (NodeRunIsDone(_nodeRun) && ContinueLooping())
				{
					_nodeRun.Halt();
					_nodeRun.Start();
					if (!HasProcessingTimeLeft())
					{
						break;
					}
					_nodeRun.Run();
				}
			}
			finally
			{
				UpdateProcessingTicksLeft();
				StopTimeTracking();
			}
			return _nodeRun.State;
		}

		protected override void Terminate()
		{
			if (_nodeRun != null)
			{
				_nodeRun.Halt();
			}
		}

		protected override void Clear()
		{
			if (_nodeRun != null)
			{
				_nodeRun.Dispose();
				_nodeRun = null;
			}
		}

		private void StartTimeTracking()
		{
			_stopWatch.Start();
		}

		private void StopTimeTracking()
		{
			_stopWatch.Reset();
		}

		private void UpdateProcessingTicksLeft()
		{
			_processingTicksLeft -= _stopWatch.ElapsedTicks;
		}

		private void CalculateAllowedTicks()
		{
			if (_maxTicksPerRun != 0)
			{
				_processingTicksLeft = Math.Min(_processingTicksLeft + _maxTicksPerRun, _maxTicksPerRun);
			}
			else
			{
				_processingTicksLeft = 0L;
			}
		}

		private static bool NodeRunIsDone(NodeRun nodeRun)
		{
			return nodeRun.State == NodeState.Done || nodeRun.Exception is LoopContinue;
		}

		private bool ContinueLooping()
		{
			return _condition.Test() == _loopsWhenTrue;
		}

		private bool HasProcessingTimeLeft()
		{
			return _processingTicksLeft >= _stopWatch.ElapsedTicks;
		}

		private bool HasWaitingTimeLeft()
		{
			bool result = _waitTicksLeft > 0;
			_waitTicksLeft = (_ticksBetweenRuns + _waitTicksLeft) % (_ticksBetweenRuns + 1);
			return result;
		}
	}

	public struct Builder : IBuilderBase
	{
		private static readonly Func<bool> _returnTrue = () => true;

		private LoopNode _loopNode;

		public StaticNode ToNode => _loopNode;

		public Builder(Node node)
		{
			_loopNode = LoopNode.Request(node);
		}

		public static implicit operator StaticNode(Builder m)
		{
			return m._loopNode;
		}

		public Builder WithDelay(long ticks)
		{
			_loopNode._ticksBetweenRun = ((ticks >= 0) ? ticks : 0);
			return this;
		}

		public Builder MaxTicksPerRun(long ticks)
		{
			_loopNode._maxTicksPerRun = ((ticks >= 0) ? ticks : 0);
			return this;
		}

		void IBuilderBase.SetFinalize(IClosure<IAction> a)
		{
			_loopNode.Finalizer = a;
		}

		void IBuilderBase.Label(string label, UnityEngine.Object context)
		{
			_loopNode.Label = label;
			_loopNode.Context = context;
		}

		private Builder While(IClosure<IFunc<bool>> f)
		{
			_loopNode.SetWhileCondition(PredicateCondition.Request(f));
			return this;
		}

		public Builder While(Func<bool> f)
		{
			return While(f.Make().Pooled2());
		}

		public Builder While<T1>(Func<T1, bool> f, T1 t1)
		{
			return While(f.Make().Close(t1).Pooled2());
		}

		public Builder While<T1, T2>(Func<T1, T2, bool> f, T1 t1, T2 t2)
		{
			return While(f.Make().Close(t2).Close(t1)
				.Pooled2());
		}

		public Builder While<T1, T2, T3>(Func<T1, T2, T3, bool> f, T1 t1, T2 t2, T3 t3)
		{
			return While(f.Make().Close(t3).Close(t2)
				.Close(t1)
				.Pooled2());
		}

		public Builder While<T1, T2, T3, T4>(Func<T1, T2, T3, T4, bool> f, T1 t1, T2 t2, T3 t3, T4 t4)
		{
			return While(f.Make().Close(t4).Close(t3)
				.Close(t2)
				.Close(t1)
				.Pooled2());
		}

		private Builder Until(IClosure<IFunc<bool>> f)
		{
			_loopNode.SetUntilCondition(PredicateCondition.Request(f));
			return this;
		}

		public Builder Until(Func<bool> f)
		{
			return Until(f.Make().Pooled2());
		}

		public Builder Until<T1>(Func<T1, bool> f, T1 t1)
		{
			return Until(f.Make().Close(t1).Pooled2());
		}

		public Builder Until<T1, T2>(Func<T1, T2, bool> f, T1 t1, T2 t2)
		{
			return Until(f.Make().Close(t2).Close(t1)
				.Pooled2());
		}

		public Builder Until<T1, T2, T3>(Func<T1, T2, T3, bool> f, T1 t1, T2 t2, T3 t3)
		{
			return Until(f.Make().Close(t3).Close(t2)
				.Close(t1)
				.Pooled2());
		}

		public Builder Until<T1, T2, T3, T4>(Func<T1, T2, T3, T4, bool> f, T1 t1, T2 t2, T3 t3, T4 t4)
		{
			return Until(f.Make().Close(t4).Close(t3)
				.Close(t2)
				.Close(t1)
				.Pooled2());
		}

		private Builder Until(IReadOnlySignal s, IClosure<IFunc<bool>> filter)
		{
			_loopNode.SetUntilCondition(SignalCondition.Request(s, filter));
			return this;
		}

		private Builder Until<T1>(IReadOnlySignal<T1> s, IClosure<IFunc<T1, bool>> filter)
		{
			_loopNode.SetUntilCondition(SignalCondition.Request(s, filter));
			return this;
		}

		private Builder Until<T1, T2>(IReadOnlySignal<T1, T2> s, IClosure<IFunc<T1, T2, bool>> filter)
		{
			_loopNode.SetUntilCondition(SignalCondition.Request(s, filter));
			return this;
		}

		private Builder Until<T1, T2, T3>(IReadOnlySignal<T1, T2, T3> s, IClosure<IFunc<T1, T2, T3, bool>> filter)
		{
			_loopNode.SetUntilCondition(SignalCondition.Request(s, filter));
			return this;
		}

		private Builder Until<T1, T2, T3, T4>(IReadOnlySignal<T1, T2, T3, T4> s, IClosure<IFunc<T1, T2, T3, T4, bool>> filter)
		{
			_loopNode.SetUntilCondition(SignalCondition.Request(s, filter));
			return this;
		}

		public Builder Until(IReadOnlySignal s)
		{
			return Until(s, _returnTrue.Make().Pooled2());
		}

		public Builder Until(IReadOnlySignal s, Func<bool> filter)
		{
			return Until(s, filter.Make().Pooled2());
		}

		public Builder Until<T1>(IReadOnlySignal s, Func<T1, bool> filter, T1 t1)
		{
			return Until(s, filter.Make().Close(t1).Pooled2());
		}

		public Builder Until<T1, T2>(IReadOnlySignal s, Func<T1, T2, bool> filter, T1 t1, T2 t2)
		{
			return Until(s, filter.Make().Close(t2).Close(t1)
				.Pooled2());
		}

		public Builder Until<T1, T2, T3>(IReadOnlySignal s, Func<T1, T2, T3, bool> filter, T1 t1, T2 t2, T3 t3)
		{
			return Until(s, filter.Make().Close(t3).Close(t2)
				.Close(t1)
				.Pooled2());
		}

		public Builder Until<T1, T2, T3, T4>(IReadOnlySignal s, Func<T1, T2, T3, T4, bool> filter, T1 t1, T2 t2, T3 t3, T4 t4)
		{
			return Until(s, filter.Make().Close(t4).Close(t3)
				.Close(t2)
				.Close(t1)
				.Pooled2());
		}

		public Builder Until<T1, T2, T3, T4, T5>(IReadOnlySignal s, Func<T1, T2, T3, T4, T5, bool> filter, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
		{
			return Until(s, filter.Make().Close(t5).Close(t4)
				.Close(t3)
				.Close(t2)
				.Close(t1)
				.Pooled2());
		}

		public Builder Until<T1>(IReadOnlySignal<T1> s)
		{
			return Until(s, _returnTrue.Make().Pad<T1>().Pooled2());
		}

		public Builder Until<T1>(IReadOnlySignal<T1> s, Func<bool> filter)
		{
			return Until(s, filter.Make().Pad<T1>().Pooled2());
		}

		public Builder Until<T1>(IReadOnlySignal<T1> s, Func<T1, bool> filter)
		{
			return Until(s, filter.Make().Pooled2());
		}

		public Builder Until<T1, T2>(IReadOnlySignal<T1> s, Func<T1, T2, bool> filter, T2 t2)
		{
			return Until(s, filter.Make().Close(t2).Pooled2());
		}

		public Builder Until<T1, T2, T3>(IReadOnlySignal<T1> s, Func<T1, T2, T3, bool> filter, T2 t2, T3 t3)
		{
			return Until(s, filter.Make().Close(t3).Close(t2)
				.Pooled2());
		}

		public Builder Until<T1, T2, T3, T4>(IReadOnlySignal<T1> s, Func<T1, T2, T3, T4, bool> filter, T2 t2, T3 t3, T4 t4)
		{
			return Until(s, filter.Make().Close(t4).Close(t3)
				.Close(t2)
				.Pooled2());
		}

		public Builder Until<T1, T2, T3, T4, T5>(IReadOnlySignal<T1> s, Func<T1, T2, T3, T4, T5, bool> filter, T2 t2, T3 t3, T4 t4, T5 t5)
		{
			return Until(s, filter.Make().Close(t5).Close(t4)
				.Close(t3)
				.Close(t2)
				.Pooled2());
		}

		public Builder Until<T1, T2>(IReadOnlySignal<T1, T2> s)
		{
			return Until(s, _returnTrue.Make().Pad<T1>().Pad<T2>()
				.Pooled2());
		}

		public Builder Until<T1, T2>(IReadOnlySignal<T1, T2> s, Func<bool> filter)
		{
			return Until(s, filter.Make().Pad<T1>().Pad<T2>()
				.Pooled2());
		}

		public Builder Until<T1, T2>(IReadOnlySignal<T1, T2> s, Func<T1, bool> filter)
		{
			return Until(s, filter.Make().Pad<T2>().Pooled2());
		}

		public Builder Until<T1, T2>(IReadOnlySignal<T1, T2> s, Func<T1, T2, bool> filter)
		{
			return Until(s, filter.Make().Pooled2());
		}

		public Builder Until<T1, T2, T3>(IReadOnlySignal<T1, T2> s, Func<T1, T2, T3, bool> filter, T3 t3)
		{
			return Until(s, filter.Make().Close(t3).Pooled2());
		}

		public Builder Until<T1, T2, T3, T4>(IReadOnlySignal<T1, T2> s, Func<T1, T2, T3, T4, bool> filter, T3 t3, T4 t4)
		{
			return Until(s, filter.Make().Close(t4).Close(t3)
				.Pooled2());
		}

		public Builder Until<T1, T2, T3, T4, T5>(IReadOnlySignal<T1, T2> s, Func<T1, T2, T3, T4, T5, bool> filter, T3 t3, T4 t4, T5 t5)
		{
			return Until(s, filter.Make().Close(t5).Close(t4)
				.Close(t3)
				.Pooled2());
		}

		public Builder Until<T1, T2, T3>(IReadOnlySignal<T1, T2, T3> s)
		{
			return Until(s, _returnTrue.Make().Pad<T1>().Pad<T2>()
				.Pad<T3>()
				.Pooled2());
		}

		public Builder Until<T1, T2, T3>(IReadOnlySignal<T1, T2, T3> s, Func<bool> filter)
		{
			return Until(s, filter.Make().Pad<T1>().Pad<T2>()
				.Pad<T3>()
				.Pooled2());
		}

		public Builder Until<T1, T2, T3>(IReadOnlySignal<T1, T2, T3> s, Func<T1, bool> filter)
		{
			return Until(s, filter.Make().Pad<T2>().Pad<T3>()
				.Pooled2());
		}

		public Builder Until<T1, T2, T3>(IReadOnlySignal<T1, T2, T3> s, Func<T1, T2, bool> filter)
		{
			return Until(s, filter.Make().Pad<T3>().Pooled2());
		}

		public Builder Until<T1, T2, T3>(IReadOnlySignal<T1, T2, T3> s, Func<T1, T2, T3, bool> filter)
		{
			return Until(s, filter.Make().Pooled2());
		}

		public Builder Until<T1, T2, T3, T4>(IReadOnlySignal<T1, T2, T3> s, Func<T1, T2, T3, T4, bool> filter, T4 t4)
		{
			return Until(s, filter.Make().Close(t4).Pooled2());
		}

		public Builder Until<T1, T2, T3, T4, T5>(IReadOnlySignal<T1, T2, T3> s, Func<T1, T2, T3, T4, T5, bool> filter, T4 t4, T5 t5)
		{
			return Until(s, filter.Make().Close(t5).Close(t4)
				.Pooled2());
		}

		public Builder Until<T1, T2, T3, T4>(IReadOnlySignal<T1, T2, T3, T4> s)
		{
			return Until(s, _returnTrue.Make().Pad<T1>().Pad<T2>()
				.Pad<T3>()
				.Pad<T4>()
				.Pooled2());
		}

		public Builder Until<T1, T2, T3, T4>(IReadOnlySignal<T1, T2, T3, T4> s, Func<bool> filter)
		{
			return Until(s, filter.Make().Pad<T1>().Pad<T2>()
				.Pad<T3>()
				.Pad<T4>()
				.Pooled2());
		}

		public Builder Until<T1, T2, T3, T4>(IReadOnlySignal<T1, T2, T3, T4> s, Func<T1, bool> filter)
		{
			return Until(s, filter.Make().Pad<T2>().Pad<T3>()
				.Pad<T4>()
				.Pooled2());
		}

		public Builder Until<T1, T2, T3, T4>(IReadOnlySignal<T1, T2, T3, T4> s, Func<T1, T2, bool> filter)
		{
			return Until(s, filter.Make().Pad<T3>().Pad<T4>()
				.Pooled2());
		}

		public Builder Until<T1, T2, T3, T4>(IReadOnlySignal<T1, T2, T3, T4> s, Func<T1, T2, T3, bool> filter)
		{
			return Until(s, filter.Make().Pad<T4>().Pooled2());
		}

		public Builder Until<T1, T2, T3, T4>(IReadOnlySignal<T1, T2, T3, T4> s, Func<T1, T2, T3, T4, bool> filter)
		{
			return Until(s, filter.Make().Pooled2());
		}

		public Builder Until<T1, T2, T3, T4, T5>(IReadOnlySignal<T1, T2, T3, T4> s, Func<T1, T2, T3, T4, T5, bool> filter, T5 t5)
		{
			return Until(s, filter.Make().Close(t5).Pooled2());
		}
	}
}
