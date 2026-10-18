using System.Collections.Generic;
using Brix.Utils;
using Brix.Utils.Closures;
using UnityEngine;

namespace Motus.Behavior;

public static class Sequencer
{
	internal class SequenceNode : PooledNode<SequenceNode>
	{
		internal List<Node> _nodes = new List<Node>();

		public override NodeRun Spawn()
		{
			return SequenceNodeRun.Request(this);
		}

		public override NodeRun SpawnWithArg(object arg)
		{
			return Spawn();
		}

		public void AddNode(Node node)
		{
			_nodes.Add(node);
		}

		protected override void Clear()
		{
			for (int num = _nodes.Count - 1; num >= 0; num--)
			{
				_nodes[num].Dispose();
			}
			_nodes.Clear();
		}
	}

	internal class SequenceNodeRun : PooledNodeRun<SequenceNodeRun>
	{
		private List<Node> _subNodes;

		private int _nodeIndex;

		private NodeRun _nodeRun;

		private bool _hasParam;

		private object _param;

		public static SequenceNodeRun Request(SequenceNode node)
		{
			SequenceNodeRun sequenceNodeRun = PooledNodeRun<SequenceNodeRun>.Request(node.Label, node.Context, node.Finalizer.Delegate);
			sequenceNodeRun._subNodes = node._nodes;
			return sequenceNodeRun;
		}

		protected override void Init()
		{
			_nodeIndex = 0;
			_param = null;
			_hasParam = false;
			if (_subNodes.Count != 0)
			{
				_nodeRun = Spawn(_subNodes[0], hasParam: false, null);
			}
			else
			{
				_nodeRun = EmptyNode._NodeRun;
			}
		}

		protected override void Tick()
		{
			TickSequenceNodes();
			base.State = _nodeRun.State;
			Exception = _nodeRun.Exception;
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
		}

		private void TickSequenceNodes()
		{
			while (_nodeRun.Run() == NodeState.Done && ++_nodeIndex < _subNodes.Count)
			{
				_hasParam = NodeRun.TryGetParam(_nodeRun, ref _param);
				_nodeRun.Dispose();
				_nodeRun = Spawn(_subNodes[_nodeIndex], _hasParam, _param);
			}
		}

		private static NodeRun Spawn(Node node, bool hasParam, object param)
		{
			return (!hasParam) ? node.Spawn() : node.SpawnWithArg(param);
		}
	}

	public struct Builder : IBuilderBase, IParameteredSequenceBuilder, ISequenceBuilder
	{
		private SequenceNode _sequence;

		public StaticNode ToNode => _sequence;

		public Builder Init()
		{
			_sequence = PooledNode<SequenceNode>.Request();
			return this;
		}

		void ISequenceBuilder.AddNode(Node n)
		{
			_sequence.AddNode(n);
		}

		void IBuilderBase.SetFinalize(IClosure<IAction> a)
		{
			_sequence.Finalizer = a;
		}

		public static implicit operator StaticNode(Builder m)
		{
			return m._sequence;
		}

		void IBuilderBase.Label(string label, Object context)
		{
			Node node = ((_sequence._nodes.Count <= 0) ? _sequence : _sequence._nodes[_sequence._nodes.Count - 1]);
			node.Label = label;
			node.Context = context;
		}
	}
}
