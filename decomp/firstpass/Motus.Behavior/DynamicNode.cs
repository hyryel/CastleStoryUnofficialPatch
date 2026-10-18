using System;
using Brix.External.Signals;
using Brix.Utils.Closures;
using Smooth.Algebraics;
using UnityEngine;

namespace Motus.Behavior;

public abstract class DynamicNode : FuncNodeBuilder
{
	public class Node<TArgs> : FuncNode.AbstractFuncNode<TArgs, Node, Node<TArgs>>
	{
		public override NodeRun Spawn()
		{
			return DynamicNodeRun<TArgs>.Request(_function, _args, Label, Context, base.Finalizer.Delegate);
		}

		public override NodeRun SpawnWithArg(object arg)
		{
			return DynamicNodeRun<TArgs>.Request(_function, _getWithArgs(_args, arg), Label, Context, base.Finalizer.Delegate);
		}
	}

	public abstract class AbstractDynamicNodeRun<T> : PooledNodeRun<T>, IWithResult where T : AbstractDynamicNodeRun<T>
	{
		protected NodeRun _nodeRun;

		private bool _hasResult;

		private object _result;

		public bool HasResult => _hasResult;

		public object Result => _result;

		protected abstract NodeRun SpawnDynamicNode();

		protected override void Init()
		{
			_nodeRun = SpawnDynamicNode();
			_nodeRun.Start();
		}

		protected override void Tick()
		{
			base.State = _nodeRun.Run();
			if (base.State != NodeState.Running)
			{
				_hasResult = NodeRun.TryGetParam(_nodeRun, ref _result);
				Exception = _nodeRun.Exception;
			}
		}

		protected override void Clear()
		{
			_hasResult = false;
			_result = null;
		}
	}

	public class DynamicNodeRun<TArgs> : AbstractDynamicNodeRun<DynamicNodeRun<TArgs>>
	{
		private Func<TArgs, Node> _func;

		private TArgs _args;

		private Node _spawnedDynamicNode;

		public static NodeRun Request(Func<TArgs, Node> func, TArgs args, string label, UnityEngine.Object context, IAction finalize)
		{
			DynamicNodeRun<TArgs> dynamicNodeRun = PooledNodeRun<DynamicNodeRun<TArgs>>.Request(label, context, finalize);
			dynamicNodeRun._func = func;
			dynamicNodeRun._args = args;
			return dynamicNodeRun;
		}

		protected override void Clear()
		{
			base.Clear();
			_func = null;
			_args = default(TArgs);
		}

		protected override void Terminate()
		{
			if (_spawnedDynamicNode != null)
			{
				_nodeRun.Dispose();
				_nodeRun = null;
				_spawnedDynamicNode.Dispose();
				_spawnedDynamicNode = null;
			}
		}

		protected override NodeRun SpawnDynamicNode()
		{
			_spawnedDynamicNode = _func(_args);
			return _spawnedDynamicNode.Spawn();
		}
	}

	private new static class Builder<TReturn, TNode>
	{
		public static StaticNode Make(Func<Node> f)
		{
			Node<Func<Node>> node = PooledNode<Node<Func<Node>>>.Request();
			node._function = (Func<Node> t) => t();
			node._args = f;
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Func<Node> t, object arg) => t;
			return node;
		}

		public static StaticNode Make(Func<StaticNode> f)
		{
			Func<Func<StaticNode>, Node> f2 = (Func<StaticNode> _f) => _f();
			Node<Func<StaticNode>> node = Builder<Func<StaticNode>, Node, Node<Func<StaticNode>>>.Make(f2, f) as Node<Func<StaticNode>>;
			node._canAssignToFirstParam = CanAssignToFirstParam;
			return node;
		}

		public static bool CanAssignToFirstParam(Type e)
		{
			return e == typeof(FailException) || e.IsSubclassOf(typeof(FailException));
		}
	}

	private new static class Builder<T1, TReturn, TNode>
	{
		public static StaticNode Make(Func<T1, Node> f, T1 t1)
		{
			Node<T1> node = PooledNode<Node<T1>>.Request();
			node._function = f;
			node._args = t1;
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (T1 result, object arg) => result;
			return node;
		}

		public static StaticNode Make(Func<T1, Node> f)
		{
			Node<T1> node = PooledNode<Node<T1>>.Request();
			node._function = f;
			node._args = default(T1);
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (T1 t, object arg) => (T1)arg;
			return node;
		}

		public static StaticNode Make(Func<T1, StaticNode> f, T1 t1)
		{
			Func<T1, Func<T1, StaticNode>, Node> f2 = (T1 _t1, Func<T1, StaticNode> _f) => _f(_t1);
			return Builder<T1, Func<T1, StaticNode>, Node, Node>.Make(f2, t1, f);
		}

		public static StaticNode Make(Func<T1, StaticNode> f)
		{
			Func<T1, Func<T1, StaticNode>, Node> f2 = (T1 _t1, Func<T1, StaticNode> _f) => _f(_t1);
			return Builder<T1, Func<T1, StaticNode>, Node, Node<Tuple<T1, Func<T1, StaticNode>, Func<T1, Func<T1, StaticNode>, Node>>>>.Make(f2, f);
		}

		public static bool CanAssignToFirstParam(Type paramType)
		{
			return typeof(T1) == paramType || paramType.IsSubclassOf(typeof(T1));
		}
	}

	private new static class Builder<T1, T2, TReturn, TNode>
	{
		public static StaticNode Make(Func<T1, T2, Node> f, T1 t1, T2 t2)
		{
			Node<Tuple<T1, T2, Func<T1, T2, Node>>> node = PooledNode<Node<Tuple<T1, T2, Func<T1, T2, Node>>>>.Request();
			node._function = (Tuple<T1, T2, Func<T1, T2, Node>> tuple) => tuple._3(tuple._1, tuple._2);
			node._args = Tuple.Create(t1, t2, f);
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Tuple<T1, T2, Func<T1, T2, Node>> result, object arg) => result;
			return node;
		}

		public static StaticNode Make(Func<T1, T2, Node> f, T2 t2)
		{
			Node<Tuple<T1, T2, Func<T1, T2, Node>>> node = PooledNode<Node<Tuple<T1, T2, Func<T1, T2, Node>>>>.Request();
			node._function = (Tuple<T1, T2, Func<T1, T2, Node>> tuple) => tuple._3(tuple._1, tuple._2);
			node._args = Tuple.Create(default(T1), t2, f);
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Tuple<T1, T2, Func<T1, T2, Node>> tuple, object arg) => Tuple.Create((T1)arg, tuple._2, tuple._3);
			return node;
		}

		public static StaticNode Make(Func<T1, T2, StaticNode> f, T1 t1, T2 t2)
		{
			Func<T1, T2, Func<T1, T2, StaticNode>, Node> f2 = (T1 _t1, T2 _t2, Func<T1, T2, StaticNode> _f) => _f(_t1, _t2);
			return Builder<T1, T2, Func<T1, T2, StaticNode>, Node, Node<Tuple<T1, T2, Func<T1, T2, StaticNode>, Func<T1, T2, Func<T1, T2, StaticNode>, Node>>>>.Make(f2, t1, t2, f);
		}

		public static StaticNode Make(Func<T1, T2, StaticNode> f, T2 t2)
		{
			Func<T1, T2, Func<T1, T2, StaticNode>, Node> f2 = (T1 _t1, T2 _t2, Func<T1, T2, StaticNode> _f) => _f(_t1, _t2);
			return Builder<T1, T2, Func<T1, T2, StaticNode>, Node, Node<Tuple<T1, T2, Func<T1, T2, StaticNode>, Func<T1, T2, Func<T1, T2, StaticNode>, Node>>>>.Make(f2, t2, f);
		}

		public static bool CanAssignToFirstParam(Type paramType)
		{
			return typeof(T1) == paramType || paramType.IsSubclassOf(typeof(T1));
		}
	}

	protected new static class Builder<T1, T2, T3, TReturn, TNode>
	{
		public static StaticNode Make(Func<T1, T2, T3, Node> f, T1 t1, T2 t2, T3 t3)
		{
			Node<Tuple<T1, T2, T3, Func<T1, T2, T3, Node>>> node = PooledNode<Node<Tuple<T1, T2, T3, Func<T1, T2, T3, Node>>>>.Request();
			node._function = (Tuple<T1, T2, T3, Func<T1, T2, T3, Node>> tuple) => tuple._4(tuple._1, tuple._2, tuple._3);
			node._args = Tuple.Create(t1, t2, t3, f);
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Tuple<T1, T2, T3, Func<T1, T2, T3, Node>> result, object arg) => result;
			return node;
		}

		public static StaticNode Make(Func<T1, T2, T3, Node> f, T2 t2, T3 t3)
		{
			Node<Tuple<T1, T2, T3, Func<T1, T2, T3, Node>>> node = PooledNode<Node<Tuple<T1, T2, T3, Func<T1, T2, T3, Node>>>>.Request();
			node._function = (Tuple<T1, T2, T3, Func<T1, T2, T3, Node>> tuple) => tuple._4(tuple._1, tuple._2, tuple._3);
			node._args = Tuple.Create(default(T1), t2, t3, f);
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Tuple<T1, T2, T3, Func<T1, T2, T3, Node>> tuple, object arg) => Tuple.Create((T1)arg, tuple._2, tuple._3, tuple._4);
			return node;
		}

		public static StaticNode Make(Func<T1, T2, T3, StaticNode> f, T1 t1, T2 t2, T3 t3)
		{
			Func<T1, T2, T3, Func<T1, T2, T3, StaticNode>, Node> f2 = (T1 _t1, T2 _t2, T3 _t3, Func<T1, T2, T3, StaticNode> _f) => _f(_t1, _t2, _t3);
			return Builder<T1, T2, T3, Func<T1, T2, T3, StaticNode>, Node, Node<Tuple<T1, T2, T3, Func<T1, T2, T3, StaticNode>, Func<T1, T2, T3, Func<T1, T2, T3, StaticNode>, Node>>>>.Make(f2, t1, t2, t3, f);
		}

		public static StaticNode Make(Func<T1, T2, T3, StaticNode> f, T2 t2, T3 t3)
		{
			Func<T1, T2, T3, Func<T1, T2, T3, StaticNode>, Node> f2 = (T1 _t1, T2 _t2, T3 _t3, Func<T1, T2, T3, StaticNode> _f) => _f(_t1, _t2, _t3);
			return Builder<T1, T2, T3, Func<T1, T2, T3, StaticNode>, Node, Node<Tuple<T1, T2, T3, Func<T1, T2, T3, StaticNode>, Func<T1, T2, T3, Func<T1, T2, T3, StaticNode>, Node>>>>.Make(f2, t2, t3, f);
		}

		public static bool CanAssignToFirstParam(Type paramType)
		{
			return typeof(T1) == paramType || paramType.IsSubclassOf(typeof(T1));
		}
	}

	protected new static class Builder<T1, T2, T3, T4, TReturn, TNode>
	{
		public static StaticNode Make(Func<T1, T2, T3, T4, Node> f, T1 t1, T2 t2, T3 t3, T4 t4)
		{
			Node<Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, Node>>> node = PooledNode<Node<Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, Node>>>>.Request();
			node._function = (Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, Node>> tuple) => tuple._5(tuple._1, tuple._2, tuple._3, tuple._4);
			node._args = Tuple.Create(t1, t2, t3, t4, f);
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, Node>> result, object arg) => result;
			return node;
		}

		public static StaticNode Make(Func<T1, T2, T3, T4, Node> f, T2 t2, T3 t3, T4 t4)
		{
			Node<Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, Node>>> node = PooledNode<Node<Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, Node>>>>.Request();
			node._function = (Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, Node>> tuple) => tuple._5(tuple._1, tuple._2, tuple._3, tuple._4);
			node._args = Tuple.Create(default(T1), t2, t3, t4, f);
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, Node>> tuple, object arg) => Tuple.Create((T1)arg, tuple._2, tuple._3, tuple._4, tuple._5);
			return node;
		}

		public static StaticNode Make(Func<T1, T2, T3, T4, StaticNode> f, T1 t1, T2 t2, T3 t3, T4 t4)
		{
			Func<T1, T2, T3, T4, Func<T1, T2, T3, T4, StaticNode>, Node> f2 = (T1 _t1, T2 _t2, T3 _t3, T4 _t4, Func<T1, T2, T3, T4, StaticNode> _f) => _f(_t1, _t2, _t3, _t4);
			return Builder<T1, T2, T3, T4, Func<T1, T2, T3, T4, StaticNode>, Node, Node<Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, StaticNode>, Func<T1, T2, T3, T4, Func<T1, T2, T3, T4, StaticNode>, Node>>>>.Make(f2, t1, t2, t3, t4, f);
		}

		public static StaticNode Make(Func<T1, T2, T3, T4, StaticNode> f, T2 t2, T3 t3, T4 t4)
		{
			Func<T1, T2, T3, T4, Func<T1, T2, T3, T4, StaticNode>, Node> f2 = (T1 _t1, T2 _t2, T3 _t3, T4 _t4, Func<T1, T2, T3, T4, StaticNode> _f) => _f(_t1, _t2, _t3, _t4);
			return Builder<T1, T2, T3, T4, Func<T1, T2, T3, T4, StaticNode>, Node, Node<Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, StaticNode>, Func<T1, T2, T3, T4, Func<T1, T2, T3, T4, StaticNode>, Node>>>>.Make(f2, t2, t3, t4, f);
		}

		public static bool CanAssignToFirstParam(Type paramType)
		{
			return typeof(T1) == paramType || paramType.IsSubclassOf(typeof(T1));
		}
	}

	protected new static class Builder<T1, T2, T3, T4, T5, TReturn, TNode>
	{
		public static StaticNode Make(Func<T1, T2, T3, T4, T5, Node> f, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
		{
			Node<Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, Node>>> node = PooledNode<Node<Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, Node>>>>.Request();
			node._function = (Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, Node>> tuple) => tuple._6(tuple._1, tuple._2, tuple._3, tuple._4, tuple._5);
			node._args = Tuple.Create(t1, t2, t3, t4, t5, f);
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, Node>> result, object arg) => result;
			return node;
		}

		public static StaticNode Make(Func<T1, T2, T3, T4, T5, Node> f, T2 t2, T3 t3, T4 t4, T5 t5)
		{
			Node<Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, Node>>> node = PooledNode<Node<Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, Node>>>>.Request();
			node._function = (Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, Node>> tuple) => tuple._6(tuple._1, tuple._2, tuple._3, tuple._4, tuple._5);
			node._args = Tuple.Create(default(T1), t2, t3, t4, t5, f);
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, Node>> tuple, object arg) => Tuple.Create((T1)arg, tuple._2, tuple._3, tuple._4, tuple._5, tuple._6);
			return node;
		}

		public static bool CanAssignToFirstParam(Type paramType)
		{
			return typeof(T1) == paramType || paramType.IsSubclassOf(typeof(T1));
		}
	}

	public static StaticNode Make(Func<Node> f)
	{
		return Builder<Node, Node<Func<Node>>>.Make(f);
	}

	public static StaticNode Make<T1>(Func<T1, Node> f, T1 t1)
	{
		return Builder<T1, Node, Node<T1>>.Make(f, t1);
	}

	public static StaticNode Make<T1>(Func<T1, Node> f)
	{
		return Builder<T1, Node, Node<T1>>.Make(f);
	}

	public static StaticNode Make<T1, T2>(Func<T1, T2, Node> f, T1 t1, T2 t2)
	{
		return Builder<T1, T2, Node, Node<Tuple<T1, T2, Func<T1, T2, Node>>>>.Make(f, t1, t2);
	}

	public static StaticNode Make<T1, T2>(Func<T1, T2, Node> f, T2 t2)
	{
		return Builder<T1, T2, Node, Node<Tuple<T1, T2, Func<T1, T2, Node>>>>.Make(f, t2);
	}

	public static StaticNode Make<T1, T2, T3>(Func<T1, T2, T3, Node> f, T1 t1, T2 t2, T3 t3)
	{
		return Builder<T1, T2, T3, Node, Node<Tuple<T1, T2, T3, Func<T1, T2, T3, Node>>>>.Make(f, t1, t2, t3);
	}

	public static StaticNode Make<T1, T2, T3>(Func<T1, T2, T3, Node> f, T2 t2, T3 t3)
	{
		return Builder<T1, T2, T3, Node, Node<Tuple<T1, T2, T3, Func<T1, T2, T3, Node>>>>.Make(f, t2, t3);
	}

	public static StaticNode Make<T1, T2, T3, T4>(Func<T1, T2, T3, T4, Node> f, T1 t1, T2 t2, T3 t3, T4 t4)
	{
		return Builder<T1, T2, T3, T4, Node, Node<Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, Node>>>>.Make(f, t1, t2, t3, t4);
	}

	public static StaticNode Make<T1, T2, T3, T4>(Func<T1, T2, T3, T4, Node> f, T2 t2, T3 t3, T4 t4)
	{
		return Builder<T1, T2, T3, T4, Node, Node<Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, Node>>>>.Make(f, t2, t3, t4);
	}

	public static StaticNode Make<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, Node> f, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
	{
		return Builder<T1, T2, T3, T4, T5, Node, Node<Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, Node>>>>.Make(f, t1, t2, t3, t4, t5);
	}

	public static StaticNode Make<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, Node> f, T2 t2, T3 t3, T4 t4, T5 t5)
	{
		return Builder<T1, T2, T3, T4, T5, Node, Node<Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, Node>>>>.Make(f, t2, t3, t4, t5);
	}

	public static StaticNode Make(Func<StaticNode> f)
	{
		return Builder<Node, Node<Func<Node>>>.Make(f);
	}

	public static StaticNode Make<T1>(Func<T1, StaticNode> f, T1 t1)
	{
		return Builder<T1, Node, Node<T1>>.Make(f, t1);
	}

	public static StaticNode Make<T1>(Func<T1, StaticNode> f)
	{
		return Builder<T1, Node, Node<T1>>.Make(f);
	}

	public static StaticNode Make<T1, T2>(Func<T1, T2, StaticNode> f, T1 t1, T2 t2)
	{
		return Builder<T1, T2, Node, Node<Tuple<T1, T2, Func<T1, T2, Node>>>>.Make(f, t1, t2);
	}

	public static StaticNode Make<T1, T2>(Func<T1, T2, StaticNode> f, T2 t2)
	{
		return Builder<T1, T2, Node, Node<Tuple<T1, T2, Func<T1, T2, Node>>>>.Make(f, t2);
	}

	public static StaticNode Make<T1, T2, T3>(Func<T1, T2, T3, StaticNode> f, T1 t1, T2 t2, T3 t3)
	{
		return Builder<T1, T2, T3, Node, Node<Tuple<T1, T2, T3, Func<T1, T2, T3, Node>>>>.Make(f, t1, t2, t3);
	}

	public static StaticNode Make<T1, T2, T3>(Func<T1, T2, T3, StaticNode> f, T2 t2, T3 t3)
	{
		return Builder<T1, T2, T3, Node, Node<Tuple<T1, T2, T3, Func<T1, T2, T3, Node>>>>.Make(f, t2, t3);
	}

	public static StaticNode Make<T1, T2, T3, T4>(Func<T1, T2, T3, T4, StaticNode> f, T1 t1, T2 t2, T3 t3, T4 t4)
	{
		return Builder<T1, T2, T3, T4, Node, Node<Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, Node>>>>.Make(f, t1, t2, t3, t4);
	}

	public static StaticNode Make<T1, T2, T3, T4>(Func<T1, T2, T3, T4, StaticNode> f, T2 t2, T3 t3, T4 t4)
	{
		return Builder<T1, T2, T3, T4, Node, Node<Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, Node>>>>.Make(f, t2, t3, t4);
	}
}
