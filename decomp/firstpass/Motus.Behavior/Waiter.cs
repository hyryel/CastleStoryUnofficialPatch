using Brix.Utils.Closures;
using UnityEngine;

namespace Motus.Behavior;

public static class Waiter
{
	internal class TickWaitNode : PooledNode<TickWaitNode>
	{
		internal int _delay;

		public override NodeRun Spawn()
		{
			return TickWaitNodeRun.Request(this);
		}

		public override NodeRun SpawnWithArg(object arg)
		{
			return Spawn();
		}

		protected override void Clear()
		{
			_delay = 0;
		}

		public static TickWaitNode Request(int delay)
		{
			TickWaitNode tickWaitNode = PooledNode<TickWaitNode>.Request();
			tickWaitNode._delay = delay;
			return tickWaitNode;
		}
	}

	internal class TickWaitNodeRun : PooledNodeRun<TickWaitNodeRun>
	{
		private int _delay;

		private int _ticksLeftWaiting;

		public static TickWaitNodeRun Request(TickWaitNode n)
		{
			TickWaitNodeRun tickWaitNodeRun = PooledNodeRun<TickWaitNodeRun>.Request(n.Label, n.Context, n.Finalizer.Delegate);
			tickWaitNodeRun._delay = n._delay;
			return tickWaitNodeRun;
		}

		protected override void Init()
		{
			_ticksLeftWaiting = _delay;
		}

		protected override void Tick()
		{
			if (!IsDelayed())
			{
				base.State = NodeState.Done;
			}
		}

		protected override void Clear()
		{
			_delay = 0;
		}

		private bool IsDelayed()
		{
			return _ticksLeftWaiting-- > 0;
		}
	}

	internal class WaitNodeWithCondition : PooledNode<WaitNodeWithCondition>
	{
		private ICondition _condition;

		private ICondition _failCondition;

		internal ICondition Condition
		{
			get
			{
				return _condition;
			}
			set
			{
				_condition = value;
			}
		}

		internal ICondition FailCondition
		{
			get
			{
				return _failCondition;
			}
			set
			{
				_failCondition = value;
			}
		}

		public override NodeRun Spawn()
		{
			return WaitNodeRunWithCondition.Request(this, _condition);
		}

		public override NodeRun SpawnWithArg(object arg)
		{
			return Spawn();
		}

		protected override void Clear()
		{
			_condition.Dispose();
		}

		public static WaitNodeWithCondition Request(ICondition condition, ICondition failCondition)
		{
			WaitNodeWithCondition waitNodeWithCondition = PooledNode<WaitNodeWithCondition>.Request();
			waitNodeWithCondition._condition = condition;
			waitNodeWithCondition._failCondition = failCondition;
			return waitNodeWithCondition;
		}
	}

	internal class WaitNodeRunWithCondition : PooledNodeRun<WaitNodeRunWithCondition>
	{
		private ICondition _condition;

		public static WaitNodeRunWithCondition Request(Node n, ICondition condition)
		{
			WaitNodeRunWithCondition waitNodeRunWithCondition = PooledNodeRun<WaitNodeRunWithCondition>.Request(n.Label, n.Context, n.Finalizer.Delegate);
			waitNodeRunWithCondition._condition = condition;
			return waitNodeRunWithCondition;
		}

		protected override void Init()
		{
			_condition.Init();
		}

		protected override void Tick()
		{
			if (_condition.Test())
			{
				base.State = NodeState.Done;
			}
		}

		protected override void Clear()
		{
		}
	}

	public struct WaitConditionBuilder : IBuilderBase, IConditionBuilder
	{
		internal WaitNodeWithCondition _waitNode;

		ICondition IConditionBuilder.Condition
		{
			get
			{
				return _waitNode.Condition;
			}
			set
			{
				_waitNode.Condition = value;
			}
		}

		public StaticNode ToNode => _waitNode;

		public WaitConditionBuilder(ICondition waitC, ICondition failC)
		{
			_waitNode = PooledNode<WaitNodeWithCondition>.Request();
			_waitNode.Condition = waitC;
			_waitNode.FailCondition = failC;
		}

		internal WaitConditionBuilder(WaitNodeWithCondition n)
		{
			_waitNode = n;
		}

		void IBuilderBase.SetFinalize(IClosure<IAction> a)
		{
			_waitNode.Finalizer = a;
		}

		void IBuilderBase.Label(string label, Object context)
		{
			_waitNode.Label = label;
			_waitNode.Context = context;
		}

		public static implicit operator StaticNode(WaitConditionBuilder m)
		{
			return m._waitNode;
		}

		public static implicit operator Node(WaitConditionBuilder m)
		{
			return m._waitNode;
		}
	}

	internal static TickWaitNode Make(int ticks)
	{
		return TickWaitNode.Request(ticks);
	}

	internal static ConditionAppenderBuilder<WaitConditionBuilder> Make(ICondition condition)
	{
		return new ConditionAppenderBuilder<WaitConditionBuilder>(new WaitConditionBuilder(WaitNodeWithCondition.Request(condition, FalseCondition.Instance)));
	}
}
