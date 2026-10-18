using System.Collections.Generic;
using Brix.Utils;
using Brix.Utils.Closures;
using UnityEngine;

namespace Motus.Behavior;

public static class Trier
{
	internal class TryNode : PooledNode<TryNode>
	{
		internal int DelayOnFailure;

		internal List<Node> _nodes = new List<Node>();

		public override NodeRun Spawn()
		{
			return TryNodeRun.Request(this);
		}

		public override NodeRun SpawnWithArg(object arg)
		{
			return Spawn();
		}

		protected override void Clear()
		{
			DelayOnFailure = 0;
			for (int num = _nodes.Count - 1; num >= 0; num--)
			{
				_nodes[num].Dispose();
			}
			_nodes.Clear();
		}
	}

	internal class TryNodeRun : PooledNodeRun<TryNodeRun>
	{
		private List<Node> _subNodes = new List<Node>();

		private NodeRun _nodeRun;

		private int _nodeIndex = -1;

		private int _delayRemaining;

		private int _delayAfterFail;

		public static TryNodeRun Request(TryNode n)
		{
			TryNodeRun tryNodeRun = PooledNodeRun<TryNodeRun>.Request(n.Label, n.Context, n.Finalizer.Delegate);
			tryNodeRun._subNodes = n._nodes;
			return tryNodeRun;
		}

		protected override void Init()
		{
			_nodeIndex = 0;
			_delayRemaining = 0;
			if (_subNodes.Count != 0)
			{
				_nodeRun = _subNodes[_nodeIndex].Spawn();
			}
			else
			{
				_nodeRun = EmptyNode._NodeRun;
			}
		}

		protected override void Tick()
		{
			if (!IsDelayed())
			{
				TickNodes();
				base.State = _nodeRun.State;
				Exception = _nodeRun.Exception;
			}
		}

		protected override void Terminate()
		{
			if (_nodeRun != null)
			{
				_nodeRun.Dispose();
				_nodeRun = null;
			}
		}

		protected override void Clear()
		{
			_subNodes = Empty.List<Node>();
			_delayAfterFail = 0;
		}

		private void TickNodes()
		{
			while (_nodeRun.Run() == NodeState.Fail)
			{
				bool flag = TrySpawnMatchingNode();
				_delayRemaining = _delayAfterFail;
				if (_delayAfterFail != 0 || !flag)
				{
					break;
				}
			}
		}

		private bool IsDelayed()
		{
			return --_delayRemaining > 0;
		}

		private bool TrySpawnMatchingNode()
		{
			if (++_nodeIndex < _subNodes.Count)
			{
				_nodeRun.Halt();
				_nodeRun = _subNodes[_nodeIndex].Spawn();
				return true;
			}
			return false;
		}
	}

	public struct Builder : IBuilderBase, ISequenceBuilder
	{
		private TryNode _tryNode;

		public StaticNode ToNode => _tryNode;

		public Builder Init()
		{
			_tryNode = PooledNode<TryNode>.Request();
			return this;
		}

		public Builder WaitOnFail(int delay)
		{
			_tryNode.DelayOnFailure = delay;
			return this;
		}

		void ISequenceBuilder.AddNode(Node node)
		{
			_tryNode._nodes.Add(node);
		}

		void IBuilderBase.SetFinalize(IClosure<IAction> a)
		{
			_tryNode.Finalizer = a;
		}

		void IBuilderBase.Label(string label, Object context)
		{
			Node node = ((_tryNode._nodes.Count <= 0) ? _tryNode : _tryNode._nodes[_tryNode._nodes.Count - 1]);
			node.Label = label;
			node.Context = context;
		}

		public static implicit operator StaticNode(Builder t)
		{
			return t._tryNode;
		}
	}
}
