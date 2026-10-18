using System;
using System.Collections.Generic;
using System.Diagnostics;
using Brix.Utils;
using Brix.Utils.Closures;
using UnityEngine;

namespace Motus.Behavior;

public static class TimedSequencer
{
	internal class TimedSequenceNode : PooledNode<TimedSequenceNode>
	{
		internal List<Node> _nodes = new List<Node>();

		internal long _maxTicksPerRun = 4611686018427387903L;

		public override NodeRun Spawn()
		{
			return TimedSequenceNodeRun.Request(this);
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

	internal class TimedSequenceNodeRun : PooledNodeRun<TimedSequenceNodeRun>
	{
		private List<Node> _subNodes;

		private int _nodeIndex;

		private NodeRun _nodeRun;

		private bool _hasParam;

		private object _param;

		private long _maxTicksPerRun;

		private long _processingTicksLeft;

		private Stopwatch stopWatch = new Stopwatch();

		public static TimedSequenceNodeRun Request(TimedSequenceNode node)
		{
			TimedSequenceNodeRun timedSequenceNodeRun = PooledNodeRun<TimedSequenceNodeRun>.Request(node.Label, node.Context, node.Finalizer.Delegate);
			timedSequenceNodeRun._subNodes = node._nodes;
			timedSequenceNodeRun._maxTicksPerRun = node._maxTicksPerRun;
			return timedSequenceNodeRun;
		}

		protected override void Init()
		{
			_nodeIndex = 0;
			_processingTicksLeft = 0L;
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
			CalculateAllowedTicks();
			if (HasProcessingTimeLeft())
			{
				TickSequenceNodes();
				if (_nodeRun.State == NodeState.Done || _nodeRun.State == NodeState.Fail)
				{
					base.State = _nodeRun.State;
					Exception = _nodeRun.Exception;
				}
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
		}

		private void TickSequenceNodes()
		{
			StartTimeTracking();
			while (_nodeRun.Run() == NodeState.Done && ++_nodeIndex < _subNodes.Count)
			{
				_hasParam = NodeRun.TryGetParam(_nodeRun, ref _param);
				_nodeRun.Dispose();
				_nodeRun = Spawn(_subNodes[_nodeIndex], _hasParam, _param);
				if (!HasProcessingTimeLeft())
				{
					break;
				}
			}
			UpdateProcessingTicksLeft();
			StopTimeTracking();
		}

		private static NodeRun Spawn(Node node, bool hasParam, object param)
		{
			return (!hasParam) ? node.Spawn() : node.SpawnWithArg(param);
		}

		private void StartTimeTracking()
		{
			stopWatch.Start();
		}

		private void StopTimeTracking()
		{
			stopWatch.Reset();
		}

		private void UpdateProcessingTicksLeft()
		{
			_processingTicksLeft -= stopWatch.ElapsedTicks;
		}

		private void CalculateAllowedTicks()
		{
			_processingTicksLeft = Math.Min(_processingTicksLeft + _maxTicksPerRun, _maxTicksPerRun);
		}

		private bool HasProcessingTimeLeft()
		{
			return _processingTicksLeft > stopWatch.ElapsedTicks;
		}
	}

	public struct Builder : IBuilderBase, IParameteredSequenceBuilder, ISequenceBuilder
	{
		private TimedSequenceNode _sequence;

		public StaticNode ToNode => _sequence;

		public Builder Init()
		{
			_sequence = PooledNode<TimedSequenceNode>.Request();
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

		void IBuilderBase.Label(string label, UnityEngine.Object context)
		{
			Node node = ((_sequence._nodes.Count <= 0) ? _sequence : _sequence._nodes[_sequence._nodes.Count - 1]);
			node.Label = label;
			node.Context = context;
		}

		public Builder MaxTicksPerRun(long ticks)
		{
			_sequence._maxTicksPerRun = ticks;
			return this;
		}
	}
}
