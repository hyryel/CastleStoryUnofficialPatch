using System;
using System.Collections.Generic;
using Brix.Common.Collections;
using Brix.Utils;
using Brix.Utils.Closures;
using Smooth.Slinq;
using UnityEngine;

namespace Motus.Behavior;

public static class ParallelSequencer
{
	private class ParallelNode : PooledNode<ParallelNode>
	{
		internal List<Node> _nodes = new List<Node>();

		internal EndCriteria criteria;

		public override NodeRun Spawn()
		{
			return criteria switch
			{
				EndCriteria.WhenAllDone => ParallelNodeRun<AllDone>.Request(this), 
				EndCriteria.WhenAllDoneIgnoreFails => ParallelNodeRun<AllDoneIgnoreFails>.Request(this), 
				EndCriteria.WhenAnyDone => ParallelNodeRun<AnyDone>.Request(this), 
				EndCriteria.WhenAnyDoneIgnoreFails => ParallelNodeRun<AnyDoneIgnoreFails>.Request(this), 
				_ => ParallelNodeRun<AllDone>.Request(this), 
			};
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
			_nodes.Slinq().ForEach(delegate(Node node)
			{
				node.Dispose();
			});
			_nodes.Clear();
			criteria = EndCriteria.WhenAllDone;
		}
	}

	private abstract class ParallelNodeRun<T> : PooledNodeRun<T> where T : ParallelNodeRun<T>
	{
		protected List<Node> _subNodes = Empty.List<Node>();

		protected List<NodeRun> _subNodeRuns = new List<NodeRun>();

		public static ParallelNodeRun<T> Request(ParallelNode node)
		{
			T val = PooledNodeRun<T>.Request(node.Label, node.Context, node.Finalizer.Delegate);
			val._subNodes = node._nodes;
			return val;
		}

		protected override void Init()
		{
			if (_subNodes.Count == 0)
			{
				base.State = NodeState.Done;
			}
			for (int i = 0; i < _subNodes.Count; i++)
			{
				_subNodeRuns.Add(_subNodes[i].Spawn());
			}
		}

		protected override void Tick()
		{
			TickParallelNodes();
			if (_subNodeRuns.Count == 0)
			{
				base.State = NodeState.Done;
			}
		}

		protected abstract void TickParallelNodes();

		protected override void Clear()
		{
			_subNodes = Empty.List<Node>();
		}

		protected override void Terminate()
		{
			for (int num = _subNodeRuns.Count - 1; num >= 0; num--)
			{
				_subNodeRuns[num].Dispose();
			}
			_subNodeRuns.Clear();
		}
	}

	private class AllDone : ParallelNodeRun<AllDone>
	{
		protected override void TickParallelNodes()
		{
			for (int i = 0; i < _subNodeRuns.Count; i++)
			{
				NodeRun nodeRun = _subNodeRuns[i];
				if (nodeRun.Run() != NodeState.Running)
				{
					if (nodeRun.State == NodeState.Fail)
					{
						Exception = nodeRun.Exception;
						base.State = NodeState.Fail;
						break;
					}
					nodeRun.Dispose();
					_subNodeRuns.RemoveAt(i);
					i--;
				}
			}
		}
	}

	private class AnyDone : ParallelNodeRun<AnyDone>
	{
		protected override void TickParallelNodes()
		{
			for (int i = 0; i < _subNodeRuns.Count; i++)
			{
				NodeRun nodeRun = _subNodeRuns[i];
				if (nodeRun.Run() != NodeState.Running)
				{
					Exception = nodeRun.Exception;
					base.State = nodeRun.State;
					break;
				}
			}
		}
	}

	private class AllDoneIgnoreFails : ParallelNodeRun<AllDoneIgnoreFails>
	{
		protected override void TickParallelNodes()
		{
			for (int i = 0; i < _subNodeRuns.Count; i++)
			{
				NodeRun nodeRun = _subNodeRuns[i];
				if (nodeRun.Run() != NodeState.Running)
				{
					nodeRun.Dispose();
					_subNodeRuns.RemoveAt(i);
					i--;
				}
			}
		}
	}

	private class AnyDoneIgnoreFails : ParallelNodeRun<AnyDoneIgnoreFails>
	{
		protected override void TickParallelNodes()
		{
			for (int i = 0; i < _subNodeRuns.Count; i++)
			{
				NodeRun nodeRun = _subNodeRuns[i];
				if (nodeRun.Run() != NodeState.Running)
				{
					if (nodeRun.State != NodeState.Fail)
					{
						base.State = NodeState.Done;
						break;
					}
					nodeRun.Dispose();
					_subNodeRuns.RemoveAt(i);
					i--;
				}
			}
		}
	}

	public struct Builder : IBuilderBase, ISequenceBuilder
	{
		private ParallelNode _parallelNode;

		public StaticNode ToNode => _parallelNode;

		public Builder Init()
		{
			_parallelNode = PooledNode<ParallelNode>.Request();
			return this;
		}

		void IBuilderBase.SetFinalize(IClosure<IAction> a)
		{
			_parallelNode.Finalizer = a;
		}

		void IBuilderBase.Label(string label, UnityEngine.Object context)
		{
			Node node = ((_parallelNode._nodes.Count <= 0) ? _parallelNode : _parallelNode._nodes[_parallelNode._nodes.Count - 1]);
			node.Label = label;
			node.Context = context;
		}

		void ISequenceBuilder.AddNode(Node n)
		{
			_parallelNode.AddNode(n);
		}

		public Builder EndingCriteria(EndCriteria criteria)
		{
			_parallelNode.criteria = criteria;
			return this;
		}

		public Builder ForEachIn<T, TSlinq>(Slinq<T, TSlinq> all, Func<T, Node> nodeFunc)
		{
			foreach (T item in all.NoLeak())
			{
				_parallelNode.AddNode(DynamicNode.Make((Func<T, Node> _f, T _t) => _f(_t), nodeFunc, item));
			}
			return this;
		}

		public Builder ForEachIn<T>(IEnumerable<T> all, Func<T, Node> nodeFunc)
		{
			foreach (T item in all)
			{
				_parallelNode.AddNode(nodeFunc(item));
			}
			return this;
		}

		public static implicit operator StaticNode(Builder p)
		{
			return p._parallelNode;
		}
	}
}
