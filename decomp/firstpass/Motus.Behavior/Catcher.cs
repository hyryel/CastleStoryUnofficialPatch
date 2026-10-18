using System;
using System.Collections.Generic;
using Brix.Utils;
using Brix.Utils.Closures;
using Smooth.Algebraics;
using UnityEngine;

namespace Motus.Behavior;

public static class Catcher
{
	public class CatcherNode : PooledNode<CatcherNode>
	{
		internal List<Node> _nodes = new List<Node>();

		internal List<Tuple<Type, Node>> _catchers = new List<Tuple<Type, Node>>();

		internal int _waitAfterFailure;

		public void AddNode(Node node)
		{
			_nodes.Add(node);
		}

		public void AddCatchNode<TException>(Node node) where TException : FailException
		{
			_catchers.Add(Tuple.Create(typeof(TException), node));
		}

		public override NodeRun Spawn()
		{
			return CatcherNodeRun.Request(this);
		}

		public override NodeRun SpawnWithArg(object arg)
		{
			return Spawn();
		}

		public void SetLabel(string label)
		{
			if (_nodes.Count > 0)
			{
				_nodes[_nodes.Count - 1].Label = label;
			}
			else
			{
				Label = label;
			}
		}

		public void SetCatchLabel(string label)
		{
			_catchers[_catchers.Count - 1]._2.Label = label;
		}

		protected override void Clear()
		{
			for (int num = _nodes.Count - 1; num >= 0; num--)
			{
				_nodes[num].Dispose();
			}
			_nodes.Clear();
			for (int num2 = _catchers.Count - 1; num2 >= 0; num2--)
			{
				_catchers[num2]._2.Dispose();
			}
			_catchers.Clear();
			_waitAfterFailure = 0;
		}
	}

	public class CatcherNodeRun : PooledNodeRun<CatcherNodeRun>
	{
		private List<Node> _subNodes = new List<Node>();

		private List<Tuple<Type, Node>> _catchers = new List<Tuple<Type, Node>>();

		private bool _hasParam;

		private object _param;

		private int _nodeIndex = -1;

		private int _catchIndex = -1;

		internal int _waitAfterFailure;

		private int _ticksLeftWaiting;

		private NodeRun _nodeRun;

		public FailException CurrentFailException { get; private set; }

		public static CatcherNodeRun Request(CatcherNode node)
		{
			CatcherNodeRun catcherNodeRun = PooledNodeRun<CatcherNodeRun>.Request(node.Label, node.Context, node.Finalizer.Delegate);
			catcherNodeRun._subNodes = node._nodes;
			catcherNodeRun._catchers = node._catchers;
			catcherNodeRun._waitAfterFailure = node._waitAfterFailure;
			return catcherNodeRun;
		}

		protected override void Init()
		{
			_param = null;
			_hasParam = false;
			_nodeIndex = 0;
			_catchIndex = -1;
			_ticksLeftWaiting = 0;
			CurrentFailException = null;
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
			if (CurrentFailException == null)
			{
				TickSequenceNodes();
			}
			if (CurrentFailException != null)
			{
				TickCatchNodes();
			}
			if (!IsDelayed())
			{
				Exception = _nodeRun.Exception;
				base.State = _nodeRun.State;
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
			_catchers = Empty.List<Tuple<Type, Node>>();
			_waitAfterFailure = 0;
			_param = null;
			_hasParam = false;
		}

		private void TickSequenceNodes()
		{
			while (_nodeRun.Run() == NodeState.Done && _nodeIndex + 1 < _subNodes.Count)
			{
				_hasParam = NodeRun.TryGetParam(_nodeRun, ref _param);
				_nodeRun.Dispose();
				_nodeRun = Spawn(_subNodes[++_nodeIndex], _hasParam, _param);
			}
			if (_nodeRun.Exception != null)
			{
				CurrentFailException = _nodeRun.Exception;
				TrySpawnMatchingCatchNode(ref _nodeRun, CurrentFailException);
			}
		}

		private void TickCatchNodes()
		{
			while (_nodeRun.Run() == NodeState.Done)
			{
				_nodeRun.Halt();
				if (!TrySpawnMatchingCatchNode(ref _nodeRun, CurrentFailException))
				{
					return;
				}
			}
			if (_waitAfterFailure != 0)
			{
				_ticksLeftWaiting = _waitAfterFailure + 1;
			}
		}

		private bool IsDelayed()
		{
			return --_ticksLeftWaiting > 0;
		}

		private static NodeRun Spawn(Node node, bool hasParam, object param)
		{
			return (!hasParam) ? node.Spawn() : node.SpawnWithArg(param);
		}

		private bool TrySpawnMatchingCatchNode(ref NodeRun nodeRun, Exception e)
		{
			while (++_catchIndex < _catchers.Count)
			{
				if (_catchers[_catchIndex]._1.IsInstanceOfType(e))
				{
					nodeRun.Dispose();
					nodeRun = _catchers[_catchIndex]._2.SpawnWithArg(e);
					return true;
				}
			}
			return false;
		}
	}

	public struct Builder : IBuilderBase, IParameteredSequenceBuilder, ISequenceBuilder
	{
		private class Resolver<BUILDER, TException> where BUILDER : IBuilderBase where TException : FailException
		{
			public static Builder _Catch(Func<BUILDER> f, Builder t)
			{
				Node node = DynamicNode.Make((Func<BUILDER> func) => func().ToNode, f);
				t.AddCatchNode<TException>(node);
				return t;
			}

			public static Builder _Catch(Func<TException, BUILDER> f, Builder t)
			{
				Node node = DynamicNode.Make((TException _e, Func<TException, BUILDER> func) => func(_e).ToNode, f);
				t.AddCatchNode<TException>(node);
				return t;
			}
		}

		private class Resolver<BUILDER, TException, P1> where BUILDER : IBuilderBase where TException : FailException
		{
			public static Builder _Catch(Func<TException, P1, BUILDER> f, P1 p1, Builder t)
			{
				StaticNode node = DynamicNode.Make((TException _e, Func<TException, P1, BUILDER> func, P1 _p1) => func(_e, _p1).ToNode, f, p1);
				t.AddCatchNode<TException>(node);
				return t;
			}
		}

		private class Resolver<BUILDER, TException, P1, P2> where BUILDER : IBuilderBase where TException : FailException
		{
			public static Builder _Catch(Func<TException, P1, P2, BUILDER> f, P1 p1, P2 p2, Builder t)
			{
				StaticNode node = DynamicNode.Make((TException _e, Func<TException, P1, P2, BUILDER> func, P1 _p1, P2 _p2) => func(_e, _p1, _p2).ToNode, f, p1, p2);
				t.AddCatchNode<TException>(node);
				return t;
			}
		}

		private class Resolver<BUILDER, TException, P1, P2, P3> where BUILDER : IBuilderBase where TException : FailException
		{
			public static Builder _Catch(Func<TException, P1, P2, P3, BUILDER> f, P1 p1, P2 p2, P3 p3, Builder t)
			{
				StaticNode node = DynamicNode.Make((TException _e, Func<TException, P1, P2, P3, BUILDER> func, P1 _p1, P2 _p2, P3 _p3) => func(_e, _p1, _p2, _p3).ToNode, f, p1, p2, p3);
				t.AddCatchNode<TException>(node);
				return t;
			}
		}

		private CatcherNode _catcherNode;

		private bool addedCatchNode;

		public StaticNode ToNode => _catcherNode;

		public Builder Init()
		{
			_catcherNode = PooledNode<CatcherNode>.Request();
			return this;
		}

		public Builder WaitAfterFailure(int wait)
		{
			_catcherNode._waitAfterFailure = wait;
			return this;
		}

		void ISequenceBuilder.AddNode(Node node)
		{
			addedCatchNode = false;
			_catcherNode.AddNode(node);
		}

		private void AddCatchNode<TException>(Node node) where TException : FailException
		{
			addedCatchNode = true;
			_catcherNode.AddCatchNode<TException>(node);
		}

		void IBuilderBase.SetFinalize(IClosure<IAction> a)
		{
			_catcherNode.Finalizer = a;
		}

		void IBuilderBase.Label(string label, UnityEngine.Object context)
		{
			Node node = ((!addedCatchNode) ? ((_catcherNode._nodes.Count <= 0) ? _catcherNode : _catcherNode._nodes[_catcherNode._nodes.Count - 1]) : _catcherNode._catchers[_catcherNode._catchers.Count - 1]._2);
			node.Label = label;
			node.Context = context;
		}

		public static implicit operator StaticNode(Builder t)
		{
			return t._catcherNode;
		}

		public Builder Catch<TException>(Action a) where TException : FailException
		{
			AddCatchNode<TException>(ActionNode.Make(a));
			return this;
		}

		public Builder Catch<TException>(Action<TException> a) where TException : FailException
		{
			AddCatchNode<TException>(ActionNode.Make(a));
			return this;
		}

		public Builder Catch<TException, P1>(Action<TException, P1> a, P1 p1) where TException : FailException
		{
			AddCatchNode<TException>(ActionNode.Make(a, p1));
			return this;
		}

		public Builder Catch<TException, P1, P2>(Action<TException, P1, P2> a, P1 p1, P2 p2) where TException : FailException
		{
			AddCatchNode<TException>(ActionNode.Make(a, p1, p2));
			return this;
		}

		public Builder Catch<TException, P1, P2, P3>(Action<TException, P1, P2, P3> a, P1 p1, P2 p2, P3 p3) where TException : FailException
		{
			AddCatchNode<TException>(ActionNode.Make(a, p1, p2, p3));
			return this;
		}

		public Builder Catch<TException>(Func<IEnumerator<ActivityProgress>> f) where TException : FailException
		{
			AddCatchNode<TException>(ActivityNode.Make(f));
			return this;
		}

		public Builder Catch<TException>(Func<TException, IEnumerator<ActivityProgress>> f) where TException : FailException
		{
			AddCatchNode<TException>(ActivityNode.Make(f));
			return this;
		}

		public Builder Catch<TException, P1>(Func<TException, P1, IEnumerator<ActivityProgress>> f, P1 p1) where TException : FailException
		{
			AddCatchNode<TException>(ActivityNode.Make(f, p1));
			return this;
		}

		public Builder Catch<TException, P1, P2>(Func<TException, P1, P2, IEnumerator<ActivityProgress>> f, P1 p1, P2 p2) where TException : FailException
		{
			AddCatchNode<TException>(ActivityNode.Make(f, p1, p2));
			return this;
		}

		public Builder Catch<TException, P1, P2, P3>(Func<TException, P1, P2, P3, IEnumerator<ActivityProgress>> f, P1 p1, P2 p2, P3 p3) where TException : FailException
		{
			AddCatchNode<TException>(ActivityNode.Make(f, p1, p2, p3));
			return this;
		}

		public Builder Catch<TException>(Node n) where TException : FailException
		{
			AddCatchNode<TException>(n);
			return this;
		}

		public Builder Catch<TException>(Func<Node> e) where TException : FailException
		{
			AddCatchNode<TException>(DynamicNode.Make(e));
			return this;
		}

		public Builder Catch<TException>(Func<TException, Node> e) where TException : FailException
		{
			AddCatchNode<TException>(DynamicNode.Make(e));
			return this;
		}

		public Builder Catch<TException, P1>(Func<TException, P1, Node> e, P1 p1) where TException : FailException
		{
			AddCatchNode<TException>(DynamicNode.Make(e, p1));
			return this;
		}

		public Builder Catch<TException, P1, P2>(Func<TException, P1, P2, Node> e, P1 p1, P2 p2) where TException : FailException
		{
			AddCatchNode<TException>(DynamicNode.Make(e, p1, p2));
			return this;
		}

		public Builder Catch<TException, P1, P2, P3>(Func<TException, P1, P2, P3, Node> e, P1 p1, P2 p2, P3 p3) where TException : FailException
		{
			AddCatchNode<TException>(DynamicNode.Make(e, p1, p2, p3));
			return this;
		}

		public Builder Catch<TException>(Func<Sequencer.Builder> f) where TException : FailException
		{
			return Resolver<Sequencer.Builder, TException>._Catch(f, this);
		}

		public Builder Catch<TException>(Func<ParallelSequencer.Builder> f) where TException : FailException
		{
			return Resolver<ParallelSequencer.Builder, TException>._Catch(f, this);
		}

		public Builder Catch<TException>(Func<Brancher.Builder> f) where TException : FailException
		{
			return Resolver<Brancher.Builder, TException>._Catch(f, this);
		}

		public Builder Catch<TException>(Func<Trier.Builder> f) where TException : FailException
		{
			return Resolver<Trier.Builder, TException>._Catch(f, this);
		}

		public Builder Catch<TException>(Func<TException, Sequencer.Builder> f) where TException : FailException
		{
			return Resolver<Sequencer.Builder, TException>._Catch(f, this);
		}

		public Builder Catch<TException>(Func<TException, ParallelSequencer.Builder> f) where TException : FailException
		{
			return Resolver<ParallelSequencer.Builder, TException>._Catch(f, this);
		}

		public Builder Catch<TException>(Func<TException, Brancher.Builder> f) where TException : FailException
		{
			return Resolver<Brancher.Builder, TException>._Catch(f, this);
		}

		public Builder Catch<TException>(Func<TException, Trier.Builder> f) where TException : FailException
		{
			return Resolver<Trier.Builder, TException>._Catch(f, this);
		}

		public Builder Catch<TException, P1>(Func<TException, P1, Sequencer.Builder> e, P1 p1) where TException : FailException
		{
			return Resolver<Sequencer.Builder, TException, P1>._Catch(e, p1, this);
		}

		public Builder Catch<TException, P1>(Func<TException, P1, ParallelSequencer.Builder> e, P1 p1) where TException : FailException
		{
			return Resolver<ParallelSequencer.Builder, TException, P1>._Catch(e, p1, this);
		}

		public Builder Catch<TException, P1>(Func<TException, P1, Brancher.Builder> e, P1 p1) where TException : FailException
		{
			return Resolver<Brancher.Builder, TException, P1>._Catch(e, p1, this);
		}

		public Builder Catch<TException, P1>(Func<TException, P1, Trier.Builder> e, P1 p1) where TException : FailException
		{
			return Resolver<Trier.Builder, TException, P1>._Catch(e, p1, this);
		}

		public Builder Catch<TException, P1, P2>(Func<TException, P1, P2, Sequencer.Builder> e, P1 p1, P2 p2) where TException : FailException
		{
			return Resolver<Sequencer.Builder, TException, P1, P2>._Catch(e, p1, p2, this);
		}

		public Builder Catch<TException, P1, P2>(Func<TException, P1, P2, ParallelSequencer.Builder> e, P1 p1, P2 p2) where TException : FailException
		{
			return Resolver<ParallelSequencer.Builder, TException, P1, P2>._Catch(e, p1, p2, this);
		}

		public Builder Catch<TException, P1, P2>(Func<TException, P1, P2, Brancher.Builder> e, P1 p1, P2 p2) where TException : FailException
		{
			return Resolver<Brancher.Builder, TException, P1, P2>._Catch(e, p1, p2, this);
		}

		public Builder Catch<TException, P1, P2>(Func<TException, P1, P2, Trier.Builder> e, P1 p1, P2 p2) where TException : FailException
		{
			return Resolver<Trier.Builder, TException, P1, P2>._Catch(e, p1, p2, this);
		}

		public Builder Catch<TException, P1, P2, P3>(Func<TException, P1, P2, P3, Sequencer.Builder> e, P1 p1, P2 p2, P3 p3) where TException : FailException
		{
			return Resolver<Sequencer.Builder, TException, P1, P2, P3>._Catch(e, p1, p2, p3, this);
		}

		public Builder Catch<TException, P1, P2, P3>(Func<TException, P1, P2, P3, ParallelSequencer.Builder> e, P1 p1, P2 p2, P3 p3) where TException : FailException
		{
			return Resolver<ParallelSequencer.Builder, TException, P1, P2, P3>._Catch(e, p1, p2, p3, this);
		}

		public Builder Catch<TException, P1, P2, P3>(Func<TException, P1, P2, P3, Brancher.Builder> e, P1 p1, P2 p2, P3 p3) where TException : FailException
		{
			return Resolver<Brancher.Builder, TException, P1, P2, P3>._Catch(e, p1, p2, p3, this);
		}

		public Builder Catch<TException, P1, P2, P3>(Func<TException, P1, P2, P3, Trier.Builder> e, P1 p1, P2 p2, P3 p3) where TException : FailException
		{
			return Resolver<Trier.Builder, TException, P1, P2, P3>._Catch(e, p1, p2, p3, this);
		}
	}
}
