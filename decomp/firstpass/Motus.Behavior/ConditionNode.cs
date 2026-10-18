using Brix.Utils.Closures;
using UnityEngine;

namespace Motus.Behavior;

public static class ConditionNode
{
	internal class ConditionalNode : PooledNode<ConditionalNode>
	{
		internal bool stopsOnConditionSuccess;

		internal bool FailOnStop;

		internal FailException Exception;

		internal Node SubNode;

		private ICondition _condition;

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

		public override NodeRun Spawn()
		{
			return ConditionalNodeRun.Request(this);
		}

		public override NodeRun SpawnWithArg(object arg)
		{
			return Spawn();
		}

		protected override void Clear()
		{
			_condition = TrueCondition.Instance;
		}

		public static ConditionalNode Request(Node n, bool isStopCondition, bool failOnStop, FailException e)
		{
			ConditionalNode conditionalNode = PooledNode<ConditionalNode>.Request();
			conditionalNode.SubNode = n;
			conditionalNode.stopsOnConditionSuccess = isStopCondition;
			conditionalNode._condition = TrueCondition.Instance;
			conditionalNode.FailOnStop = failOnStop;
			conditionalNode.Exception = e;
			return conditionalNode;
		}
	}

	internal class ConditionalNodeRun : PooledNodeRun<ConditionalNodeRun>, IWithResult
	{
		private bool _stopsOnConditionSuccess;

		private bool _failOnStop;

		private FailException _exception;

		private ICondition _condition;

		private NodeRun _subNodeRun;

		private bool _hasResult;

		private object _result;

		public bool HasResult => _hasResult;

		public object Result => _result;

		public static ConditionalNodeRun Request(ConditionalNode n)
		{
			ConditionalNodeRun conditionalNodeRun = PooledNodeRun<ConditionalNodeRun>.Request(n.Label, n.Context, n.Finalizer.Delegate);
			conditionalNodeRun._subNodeRun = n.SubNode.Spawn();
			conditionalNodeRun._condition = n.Condition;
			conditionalNodeRun._exception = n.Exception;
			conditionalNodeRun._failOnStop = n.FailOnStop;
			conditionalNodeRun._stopsOnConditionSuccess = n.stopsOnConditionSuccess;
			return conditionalNodeRun;
		}

		protected override void Init()
		{
			_condition.Init();
			_subNodeRun.Start();
		}

		protected override void Tick()
		{
			if (_condition.Test() == _stopsOnConditionSuccess)
			{
				InterruptExecution();
			}
			else
			{
				TickSubNode();
			}
		}

		protected override void Clear()
		{
		}

		protected override void Terminate()
		{
			if (_subNodeRun != null)
			{
				_subNodeRun.Dispose();
				_subNodeRun = null;
			}
		}

		private void TickSubNode()
		{
			base.State = _subNodeRun.Run();
			if (base.State != NodeState.Running)
			{
				_hasResult = NodeRun.TryGetParam(_subNodeRun, ref _result);
				Exception = _subNodeRun.Exception;
			}
		}

		private void InterruptExecution()
		{
			if (_failOnStop)
			{
				base.State = NodeState.Fail;
				Exception = _exception;
			}
			else
			{
				base.State = NodeState.Done;
			}
		}
	}

	public struct ConditionalNodeBuilder : IBuilderBase, IConditionBuilder
	{
		private ConditionalNode _conditionalNode;

		ICondition IConditionBuilder.Condition
		{
			get
			{
				return _conditionalNode.Condition;
			}
			set
			{
				_conditionalNode.Condition = value;
			}
		}

		public StaticNode ToNode => _conditionalNode;

		public ConditionalNodeBuilder(Node n, bool isStopCondition, bool failOnStop, FailException e)
		{
			_conditionalNode = ConditionalNode.Request(n, isStopCondition, failOnStop, e);
			_conditionalNode.Condition = TrueCondition.Instance;
		}

		internal ConditionalNodeBuilder(ConditionalNode n)
		{
			_conditionalNode = n;
		}

		void IBuilderBase.SetFinalize(IClosure<IAction> a)
		{
			_conditionalNode.Finalizer = a;
		}

		void IBuilderBase.Label(string label, Object context)
		{
			_conditionalNode.Label = label;
			_conditionalNode.Context = context;
		}

		public static implicit operator StaticNode(ConditionalNodeBuilder m)
		{
			return m._conditionalNode;
		}

		public static implicit operator Node(ConditionalNodeBuilder m)
		{
			return m._conditionalNode;
		}
	}
}
