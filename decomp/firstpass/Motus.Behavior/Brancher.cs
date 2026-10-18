using System;
using System.Collections;
using System.Collections.Generic;
using Brix.Common;
using Brix.External.Signals;
using Brix.Utils;
using Brix.Utils.Closures;
using UnityEngine;

namespace Motus.Behavior;

public static class Brancher
{
	public class BranchingNode : PooledNode<BranchingNode>
	{
		internal List<IBranch> _branches = new List<IBranch>();

		internal IBranch _elseBranch = EmptyBranch.Instance;

		internal IBranch _defaultBranch = EmptyBranch.Instance;

		public void AddBranch(Branch b)
		{
			_branches.Add(b);
		}

		public override NodeRun Spawn()
		{
			return BranchingNodeRun.Request(this);
		}

		public override NodeRun SpawnWithArg(object arg)
		{
			return Spawn();
		}

		protected override void Clear()
		{
			for (int num = _branches.Count - 1; num >= 0; num--)
			{
				_branches[num].Dispose();
			}
			_branches.Clear();
			_elseBranch.Dispose();
			_elseBranch = EmptyBranch.Instance;
			_defaultBranch.Dispose();
			_defaultBranch = EmptyBranch.Instance;
		}
	}

	public class BranchingNodeRun : PooledNodeRun<BranchingNodeRun>
	{
		private List<IBranch> _branches = Empty.List<IBranch>();

		private IBranch _elseBranch;

		private IBranch _defaultBranch;

		private IBranch _currentBranch;

		private NodeRun _currentNodeRun = EmptyNode._NodeRun;

		public static NodeRun Request(BranchingNode node)
		{
			BranchingNodeRun branchingNodeRun = PooledNodeRun<BranchingNodeRun>.Request(node.Label, node.Context, node.Finalizer.Delegate);
			branchingNodeRun._branches = node._branches;
			branchingNodeRun._elseBranch = node._elseBranch;
			branchingNodeRun._defaultBranch = node._defaultBranch;
			return branchingNodeRun;
		}

		protected override void Init()
		{
			_currentNodeRun = EmptyNode._NodeRun;
			_currentBranch = EmptyBranch.Instance;
			_branches.ForEach(delegate(IBranch b)
			{
				b.Init();
			});
		}

		protected override void Tick()
		{
			IBranch newBranch = SelectPrioritizedBranch();
			MoveToBranch(newBranch);
			TickBranchNode();
			Exception = _currentNodeRun.Exception;
		}

		protected override void Terminate()
		{
			_currentNodeRun.Dispose();
			_currentBranch = EmptyBranch.Instance;
		}

		protected override void Clear()
		{
			_branches = Empty.List<IBranch>();
			_elseBranch = EmptyBranch.Instance;
			_defaultBranch = null;
		}

		private IBranch SelectPrioritizedBranch()
		{
			bool flag = _currentBranch.BranchType == BranchType.Empty;
			for (int i = 0; i < _branches.Count; i++)
			{
				IBranch branch = _branches[i];
				if ((flag || branch.EvaluateEveryFrame) && branch.IsValid)
				{
					return branch;
				}
			}
			if (_defaultBranch != EmptyBranch.Instance && _currentBranch.BranchType != BranchType.If)
			{
				return _defaultBranch;
			}
			if (_currentBranch.BranchType == BranchType.While)
			{
				return EmptyBranch.Instance;
			}
			if (_currentBranch.BranchType == BranchType.Empty)
			{
				return _elseBranch;
			}
			return _currentBranch;
		}

		private void TickBranchNode()
		{
			base.State = _currentNodeRun.Run();
		}

		private void MoveToBranch(IBranch newBranch)
		{
			if (_currentBranch != newBranch)
			{
				_currentNodeRun.Dispose();
				_currentNodeRun = ((newBranch.Node != null) ? newBranch.Node.Spawn() : EmptyNode._NodeRun);
				_currentBranch = newBranch;
			}
		}
	}

	[Flags]
	public enum BranchType
	{
		If = 1,
		When = 2,
		While = 4,
		Default = 8,
		Else = 0x10,
		Empty = 0x20
	}

	public interface IBranch : IDisposable
	{
		Node Node { get; }

		BranchType BranchType { get; }

		bool EvaluateEveryFrame { get; }

		bool IsValid { get; }

		string Label { get; }

		UnityEngine.Object Context { get; }

		void Init();
	}

	public class Branch : PoolableObject<Branch>, IBranch, IDisposable
	{
		private const BranchType BranchEvaluatedEveryTime = BranchType.When | BranchType.While;

		protected ICondition _condition;

		public string Label;

		public UnityEngine.Object Context;

		protected Node _node;

		protected BranchType _branchType;

		string IBranch.Label => Label;

		UnityEngine.Object IBranch.Context => Context;

		public Node Node
		{
			get
			{
				return _node;
			}
			set
			{
				_node = value;
			}
		}

		public BranchType BranchType
		{
			get
			{
				return _branchType;
			}
			protected set
			{
				_branchType = value;
			}
		}

		public bool EvaluateEveryFrame => (_branchType & (BranchType.When | BranchType.While)) != 0;

		public bool IsValid => _condition.Test();

		public void Init()
		{
			_condition.Init();
		}

		public static Branch Request(BranchType bType, ICondition condition, Node node)
		{
			Branch branch = PoolableObject<Branch>.Request();
			branch._branchType = bType;
			branch._condition = condition;
			branch._node = node;
			return branch;
		}

		protected override void ResetObject()
		{
			_branchType = BranchType.Empty;
			_condition.Dispose();
			_condition = TrueCondition.Instance;
			_node.Dispose();
			_node = null;
			Label = null;
			Context = null;
		}

		public void Dispose()
		{
			ReleaseToPool();
		}
	}

	public class EmptyBranch : IBranch, IDisposable
	{
		private static readonly EmptyBranch _instance = new EmptyBranch();

		string IBranch.Label => null;

		UnityEngine.Object IBranch.Context => null;

		public static EmptyBranch Instance => _instance;

		public BranchType BranchType => BranchType.Empty;

		public bool EvaluateEveryFrame => true;

		public bool IsValid => true;

		public Node Node => EmptyNode._Node;

		private EmptyBranch()
		{
		}

		public void Dispose()
		{
		}

		public void Init()
		{
		}
	}

	public struct Builder : IBuilderBase, IParameteredSequenceBuilder, ISequenceBuilder
	{
		private static readonly Func<bool> _returnTrue = () => true;

		private BranchingNode _branchingNode;

		private Branch _currentBranch;

		public StaticNode ToNode => _branchingNode;

		public Builder Init()
		{
			_branchingNode = PooledNode<BranchingNode>.Request();
			return this;
		}

		[Obsolete]
		public Builder When(Func<IEnumerator> f)
		{
			return this;
		}

		private Builder When(IClosure<IFunc<bool>> f)
		{
			_currentBranch = Branch.Request(BranchType.When, PredicateCondition.Request(f), EmptyNode._Node);
			_branchingNode.AddBranch(_currentBranch);
			return this;
		}

		public Builder When(Func<bool> f)
		{
			return When(f.Make().Pooled2());
		}

		public Builder When<T1>(Func<T1, bool> f, T1 t1)
		{
			return When(f.Make().Close(t1).Pooled2());
		}

		public Builder When<T1, T2>(Func<T1, T2, bool> f, T1 t1, T2 t2)
		{
			return When(f.Make().Close(t2).Close(t1)
				.Pooled2());
		}

		public Builder When<T1, T2, T3>(Func<T1, T2, T3, bool> f, T1 t1, T2 t2, T3 t3)
		{
			return When(f.Make().Close(t3).Close(t2)
				.Close(t1)
				.Pooled2());
		}

		public Builder When<T1, T2, T3, T4>(Func<T1, T2, T3, T4, bool> f, T1 t1, T2 t2, T3 t3, T4 t4)
		{
			return When(f.Make().Close(t4).Close(t3)
				.Close(t2)
				.Close(t1)
				.Pooled2());
		}

		private Builder When(IReadOnlySignal s, IClosure<IFunc<bool>> filter)
		{
			_currentBranch = Branch.Request(BranchType.When, SignalCondition.Request(s, filter), EmptyNode._Node);
			_branchingNode.AddBranch(_currentBranch);
			return this;
		}

		private Builder When<T1>(IReadOnlySignal<T1> s, IClosure<IFunc<T1, bool>> filter)
		{
			_currentBranch = Branch.Request(BranchType.When, SignalCondition.Request(s, filter), EmptyNode._Node);
			_branchingNode.AddBranch(_currentBranch);
			return this;
		}

		private Builder When<T1, T2>(IReadOnlySignal<T1, T2> s, IClosure<IFunc<T1, T2, bool>> filter)
		{
			_currentBranch = Branch.Request(BranchType.When, SignalCondition.Request(s, filter), EmptyNode._Node);
			_branchingNode.AddBranch(_currentBranch);
			return this;
		}

		private Builder When<T1, T2, T3>(IReadOnlySignal<T1, T2, T3> s, IClosure<IFunc<T1, T2, T3, bool>> filter)
		{
			_currentBranch = Branch.Request(BranchType.When, SignalCondition.Request(s, filter), EmptyNode._Node);
			_branchingNode.AddBranch(_currentBranch);
			return this;
		}

		private Builder When<T1, T2, T3, T4>(IReadOnlySignal<T1, T2, T3, T4> s, IClosure<IFunc<T1, T2, T3, T4, bool>> filter)
		{
			_currentBranch = Branch.Request(BranchType.When, SignalCondition.Request(s, filter), EmptyNode._Node);
			_branchingNode.AddBranch(_currentBranch);
			return this;
		}

		public Builder When(IReadOnlySignal s)
		{
			return When(s, _returnTrue.Make().Pooled2());
		}

		public Builder When(IReadOnlySignal s, Func<bool> filter)
		{
			return When(s, filter.Make().Pooled2());
		}

		public Builder When<T1>(IReadOnlySignal s, Func<T1, bool> filter, T1 t1)
		{
			return When(s, filter.Make().Close(t1).Pooled2());
		}

		public Builder When<T1, T2>(IReadOnlySignal s, Func<T1, T2, bool> filter, T1 t1, T2 t2)
		{
			return When(s, filter.Make().Close(t2).Close(t1)
				.Pooled2());
		}

		public Builder When<T1, T2, T3>(IReadOnlySignal s, Func<T1, T2, T3, bool> filter, T1 t1, T2 t2, T3 t3)
		{
			return When(s, filter.Make().Close(t3).Close(t2)
				.Close(t1)
				.Pooled2());
		}

		public Builder When<T1, T2, T3, T4>(IReadOnlySignal s, Func<T1, T2, T3, T4, bool> filter, T1 t1, T2 t2, T3 t3, T4 t4)
		{
			return When(s, filter.Make().Close(t4).Close(t3)
				.Close(t2)
				.Close(t1)
				.Pooled2());
		}

		public Builder When<T1, T2, T3, T4, T5>(IReadOnlySignal s, Func<T1, T2, T3, T4, T5, bool> filter, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
		{
			return When(s, filter.Make().Close(t5).Close(t4)
				.Close(t3)
				.Close(t2)
				.Close(t1)
				.Pooled2());
		}

		public Builder When<T1>(IReadOnlySignal<T1> s)
		{
			return When(s, _returnTrue.Make().Pad<T1>().Pooled2());
		}

		public Builder When<T1>(IReadOnlySignal<T1> s, Func<bool> filter)
		{
			return When(s, filter.Make().Pad<T1>().Pooled2());
		}

		public Builder When<T1>(IReadOnlySignal<T1> s, Func<T1, bool> filter)
		{
			return When(s, filter.Make().Pooled2());
		}

		public Builder When<T1, T2>(IReadOnlySignal<T1> s, Func<T1, T2, bool> filter, T2 t2)
		{
			return When(s, filter.Make().Close(t2).Pooled2());
		}

		public Builder When<T1, T2, T3>(IReadOnlySignal<T1> s, Func<T1, T2, T3, bool> filter, T2 t2, T3 t3)
		{
			return When(s, filter.Make().Close(t3).Close(t2)
				.Pooled2());
		}

		public Builder When<T1, T2, T3, T4>(IReadOnlySignal<T1> s, Func<T1, T2, T3, T4, bool> filter, T2 t2, T3 t3, T4 t4)
		{
			return When(s, filter.Make().Close(t4).Close(t3)
				.Close(t2)
				.Pooled2());
		}

		public Builder When<T1, T2, T3, T4, T5>(IReadOnlySignal<T1> s, Func<T1, T2, T3, T4, T5, bool> filter, T2 t2, T3 t3, T4 t4, T5 t5)
		{
			return When(s, filter.Make().Close(t5).Close(t4)
				.Close(t3)
				.Close(t2)
				.Pooled2());
		}

		public Builder When<T1, T2>(IReadOnlySignal<T1, T2> s)
		{
			return When(s, _returnTrue.Make().Pad<T1>().Pad<T2>()
				.Pooled2());
		}

		public Builder When<T1, T2>(IReadOnlySignal<T1, T2> s, Func<bool> filter)
		{
			return When(s, filter.Make().Pad<T1>().Pad<T2>()
				.Pooled2());
		}

		public Builder When<T1, T2>(IReadOnlySignal<T1, T2> s, Func<T1, bool> filter)
		{
			return When(s, filter.Make().Pad<T2>().Pooled2());
		}

		public Builder When<T1, T2>(IReadOnlySignal<T1, T2> s, Func<T1, T2, bool> filter)
		{
			return When(s, filter.Make().Pooled2());
		}

		public Builder When<T1, T2, T3>(IReadOnlySignal<T1, T2> s, Func<T1, T2, T3, bool> filter, T3 t3)
		{
			return When(s, filter.Make().Close(t3).Pooled2());
		}

		public Builder When<T1, T2, T3, T4>(IReadOnlySignal<T1, T2> s, Func<T1, T2, T3, T4, bool> filter, T3 t3, T4 t4)
		{
			return When(s, filter.Make().Close(t4).Close(t3)
				.Pooled2());
		}

		public Builder When<T1, T2, T3, T4, T5>(IReadOnlySignal<T1, T2> s, Func<T1, T2, T3, T4, T5, bool> filter, T3 t3, T4 t4, T5 t5)
		{
			return When(s, filter.Make().Close(t5).Close(t4)
				.Close(t3)
				.Pooled2());
		}

		public Builder When<T1, T2, T3>(IReadOnlySignal<T1, T2, T3> s)
		{
			return When(s, _returnTrue.Make().Pad<T1>().Pad<T2>()
				.Pad<T3>()
				.Pooled2());
		}

		public Builder When<T1, T2, T3>(IReadOnlySignal<T1, T2, T3> s, Func<bool> filter)
		{
			return When(s, filter.Make().Pad<T1>().Pad<T2>()
				.Pad<T3>()
				.Pooled2());
		}

		public Builder When<T1, T2, T3>(IReadOnlySignal<T1, T2, T3> s, Func<T1, bool> filter)
		{
			return When(s, filter.Make().Pad<T2>().Pad<T3>()
				.Pooled2());
		}

		public Builder When<T1, T2, T3>(IReadOnlySignal<T1, T2, T3> s, Func<T1, T2, bool> filter)
		{
			return When(s, filter.Make().Pad<T3>().Pooled2());
		}

		public Builder When<T1, T2, T3>(IReadOnlySignal<T1, T2, T3> s, Func<T1, T2, T3, bool> filter)
		{
			return When(s, filter.Make().Pooled2());
		}

		public Builder When<T1, T2, T3, T4>(IReadOnlySignal<T1, T2, T3> s, Func<T1, T2, T3, T4, bool> filter, T4 t4)
		{
			return When(s, filter.Make().Close(t4).Pooled2());
		}

		public Builder When<T1, T2, T3, T4, T5>(IReadOnlySignal<T1, T2, T3> s, Func<T1, T2, T3, T4, T5, bool> filter, T4 t4, T5 t5)
		{
			return When(s, filter.Make().Close(t5).Close(t4)
				.Pooled2());
		}

		public Builder When<T1, T2, T3, T4>(IReadOnlySignal<T1, T2, T3, T4> s)
		{
			return When(s, _returnTrue.Make().Pad<T1>().Pad<T2>()
				.Pad<T3>()
				.Pad<T4>()
				.Pooled2());
		}

		public Builder When<T1, T2, T3, T4>(IReadOnlySignal<T1, T2, T3, T4> s, Func<bool> filter)
		{
			return When(s, filter.Make().Pad<T1>().Pad<T2>()
				.Pad<T3>()
				.Pad<T4>()
				.Pooled2());
		}

		public Builder When<T1, T2, T3, T4>(IReadOnlySignal<T1, T2, T3, T4> s, Func<T1, bool> filter)
		{
			return When(s, filter.Make().Pad<T2>().Pad<T3>()
				.Pad<T4>()
				.Pooled2());
		}

		public Builder When<T1, T2, T3, T4>(IReadOnlySignal<T1, T2, T3, T4> s, Func<T1, T2, bool> filter)
		{
			return When(s, filter.Make().Pad<T3>().Pad<T4>()
				.Pooled2());
		}

		public Builder When<T1, T2, T3, T4>(IReadOnlySignal<T1, T2, T3, T4> s, Func<T1, T2, T3, bool> filter)
		{
			return When(s, filter.Make().Pad<T4>().Pooled2());
		}

		public Builder When<T1, T2, T3, T4>(IReadOnlySignal<T1, T2, T3, T4> s, Func<T1, T2, T3, T4, bool> filter)
		{
			return When(s, filter.Make().Pooled2());
		}

		public Builder When<T1, T2, T3, T4, T5>(IReadOnlySignal<T1, T2, T3, T4> s, Func<T1, T2, T3, T4, T5, bool> filter, T5 t5)
		{
			return When(s, filter.Make().Close(t5).Pooled2());
		}

		private Builder While(IClosure<IFunc<bool>> f)
		{
			_currentBranch = Branch.Request(BranchType.While, PredicateCondition.Request(f), EmptyNode._Node);
			_branchingNode.AddBranch(_currentBranch);
			return this;
		}

		[Obsolete]
		public Builder While(Func<IEnumerator> f)
		{
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

		private Builder If(IClosure<IFunc<bool>> f)
		{
			_currentBranch = Branch.Request(BranchType.If, PredicateCondition.Request(f), EmptyNode._Node);
			_branchingNode.AddBranch(_currentBranch);
			return this;
		}

		[Obsolete]
		public Builder If(Func<IEnumerator> f)
		{
			return this;
		}

		public Builder If(Func<bool> f)
		{
			return If(f.Make().Pooled2());
		}

		public Builder If<T1>(Func<T1, bool> f, T1 t1)
		{
			return If(f.Make().Close(t1).Pooled2());
		}

		public Builder If<T1, T2>(Func<T1, T2, bool> f, T1 t1, T2 t2)
		{
			return If(f.Make().Close(t2).Close(t1)
				.Pooled2());
		}

		public Builder If<T1, T2, T3>(Func<T1, T2, T3, bool> f, T1 t1, T2 t2, T3 t3)
		{
			return If(f.Make().Close(t3).Close(t2)
				.Close(t1)
				.Pooled2());
		}

		public Builder If<T1, T2, T3, T4>(Func<T1, T2, T3, T4, bool> f, T1 t1, T2 t2, T3 t3, T4 t4)
		{
			return If(f.Make().Close(t4).Close(t3)
				.Close(t2)
				.Close(t1)
				.Pooled2());
		}

		public Builder If<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, bool> f, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
		{
			return If(f.Make().Close(t5).Close(t4)
				.Close(t3)
				.Close(t2)
				.Close(t1)
				.Pooled2());
		}

		public Builder Else()
		{
			_currentBranch = Branch.Request(BranchType.Else, TrueCondition.Instance, EmptyNode._Node);
			_branchingNode._elseBranch = _currentBranch;
			return this;
		}

		public Builder Default()
		{
			_currentBranch = Branch.Request(BranchType.Default, TrueCondition.Instance, EmptyNode._Node);
			_branchingNode._defaultBranch = _currentBranch;
			return this;
		}

		void ISequenceBuilder.AddNode(Node n)
		{
			Node oldNode = _currentBranch.Node;
			AppendNode(n, ref oldNode);
			_currentBranch.Node = oldNode;
		}

		private static void AppendNode(Node newNode, ref Node oldNode)
		{
			if (oldNode == Node.Empty)
			{
				oldNode = newNode;
				return;
			}
			if (oldNode is Sequencer.SequenceNode)
			{
				(oldNode as Sequencer.SequenceNode).AddNode(newNode);
				return;
			}
			Sequencer.SequenceNode sequenceNode = new Sequencer.SequenceNode();
			sequenceNode.AddNode(oldNode);
			sequenceNode.AddNode(newNode);
			oldNode = sequenceNode;
		}

		void IBuilderBase.SetFinalize(IClosure<IAction> a)
		{
			if (_currentBranch.Node is EmptyNode.Node)
			{
				throw new Exception("Finally must have a node...");
			}
			_branchingNode.Finalizer = a;
		}

		public static implicit operator StaticNode(Builder m)
		{
			return m._branchingNode;
		}

		void IBuilderBase.Label(string label, UnityEngine.Object context)
		{
			Node node;
			if (_currentBranch == null)
			{
				node = _branchingNode;
			}
			else
			{
				if (_currentBranch.Node is EmptyNode.Node)
				{
					_currentBranch.Label = label;
					_currentBranch.Context = context;
					return;
				}
				node = ((!(_currentBranch.Node is Sequencer.SequenceNode sequenceNode)) ? _currentBranch.Node : sequenceNode._nodes[sequenceNode._nodes.Count - 1]);
			}
			node.Label = label;
			node.Context = context;
		}
	}
}
