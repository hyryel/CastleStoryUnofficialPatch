using System;
using Brix.Utils.Closures;
using Smooth.Algebraics;
using UnityEngine;

namespace Motus.Behavior;

public abstract class ActionNode
{
	private class Node<TArgs> : PooledNode<Node<TArgs>>
	{
		internal Func<TArgs, object, TArgs> _getWithArgs;

		internal Action<TArgs> _action;

		internal Func<Type, bool> _canAssignToFirstParam;

		internal TArgs _args;

		public override NodeRun Spawn()
		{
			return ActionNodeRun<TArgs>.Request(_action, _args, Label, Context, base.Finalizer.Delegate);
		}

		public override NodeRun SpawnWithArg(object arg)
		{
			return ActionNodeRun<TArgs>.Request(_action, _getWithArgs(_args, arg), Label, Context, base.Finalizer.Delegate);
		}

		public override bool Catches(FailException e)
		{
			return _canAssignToFirstParam(e.GetType());
		}

		protected override void Clear()
		{
			_args = default(TArgs);
			_action = null;
			_getWithArgs = null;
			_canAssignToFirstParam = null;
		}
	}

	private abstract class AbstractActionRun<T> : PooledNodeRun<T> where T : AbstractActionRun<T>
	{
		protected abstract void Invoke();

		protected override void Tick()
		{
			try
			{
				Invoke();
				base.State = NodeState.Done;
			}
			catch (FailException exception)
			{
				Exception = exception;
				base.State = NodeState.Fail;
			}
		}
	}

	private class ActionNodeRun<TArgs> : AbstractActionRun<ActionNodeRun<TArgs>>
	{
		private Action<TArgs> _action;

		private TArgs _args;

		public static NodeRun Request(Action<TArgs> action, TArgs args, string label, UnityEngine.Object context, IAction finalize)
		{
			ActionNodeRun<TArgs> actionNodeRun = PooledNodeRun<ActionNodeRun<TArgs>>.Request(label, context, finalize);
			actionNodeRun._action = action;
			actionNodeRun._args = args;
			return actionNodeRun;
		}

		protected override void Invoke()
		{
			_action(_args);
		}

		protected override void Clear()
		{
			_action = null;
			_args = default(TArgs);
		}
	}

	private static class Builder
	{
		public static StaticNode Make(Action a)
		{
			Node<Action> node = PooledNode<Node<Action>>.Request();
			node._action = delegate(Action t)
			{
				t();
			};
			node._args = a;
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Action t, object arg) => t;
			return node;
		}

		public static bool CanAssignToFirstParam(Type e)
		{
			return e == typeof(FailException) || e.IsSubclassOf(typeof(FailException));
		}
	}

	private static class Builder<T1>
	{
		public static StaticNode Make(Action<T1> a, T1 t1)
		{
			Node<T1> node = PooledNode<Node<T1>>.Request();
			node._action = a;
			node._args = t1;
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (T1 result, object arg) => result;
			return node;
		}

		public static StaticNode Make(Action<T1> a)
		{
			Node<T1> node = PooledNode<Node<T1>>.Request();
			node._action = a;
			node._args = default(T1);
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (T1 t, object arg) => (T1)arg;
			return node;
		}

		public static bool CanAssignToFirstParam(Type exceptionType)
		{
			return typeof(T1) == exceptionType || exceptionType.IsSubclassOf(typeof(T1));
		}
	}

	private static class Builder<T1, T2>
	{
		public static StaticNode Make(Action<T1, T2> a, T1 t1, T2 t2)
		{
			Node<Tuple<T1, T2, Action<T1, T2>>> node = PooledNode<Node<Tuple<T1, T2, Action<T1, T2>>>>.Request();
			node._action = delegate(Tuple<T1, T2, Action<T1, T2>> tuple)
			{
				tuple._3(tuple._1, tuple._2);
			};
			node._args = Tuple.Create(t1, t2, a);
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Tuple<T1, T2, Action<T1, T2>> result, object arg) => result;
			return node;
		}

		public static StaticNode Make(Action<T1, T2> a, T2 t2)
		{
			Node<Tuple<T1, T2, Action<T1, T2>>> node = PooledNode<Node<Tuple<T1, T2, Action<T1, T2>>>>.Request();
			node._action = delegate(Tuple<T1, T2, Action<T1, T2>> tuple)
			{
				tuple._3(tuple._1, tuple._2);
			};
			node._args = Tuple.Create(default(T1), t2, a);
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Tuple<T1, T2, Action<T1, T2>> tuple, object arg) => Tuple.Create((T1)arg, tuple._2, tuple._3);
			return node;
		}

		public static bool CanAssignToFirstParam(Type exceptionType)
		{
			return typeof(T1) == exceptionType || exceptionType.IsSubclassOf(typeof(T1));
		}
	}

	private static class Builder<T1, T2, T3>
	{
		public static StaticNode Make(Action<T1, T2, T3> a, T1 t1, T2 t2, T3 t3)
		{
			Node<Tuple<T1, T2, T3, Action<T1, T2, T3>>> node = PooledNode<Node<Tuple<T1, T2, T3, Action<T1, T2, T3>>>>.Request();
			node._action = delegate(Tuple<T1, T2, T3, Action<T1, T2, T3>> tuple)
			{
				tuple._4(tuple._1, tuple._2, tuple._3);
			};
			node._args = Tuple.Create(t1, t2, t3, a);
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Tuple<T1, T2, T3, Action<T1, T2, T3>> result, object arg) => result;
			return node;
		}

		public static StaticNode Make(Action<T1, T2, T3> a, T2 t2, T3 t3)
		{
			Node<Tuple<T1, T2, T3, Action<T1, T2, T3>>> node = PooledNode<Node<Tuple<T1, T2, T3, Action<T1, T2, T3>>>>.Request();
			node._action = delegate(Tuple<T1, T2, T3, Action<T1, T2, T3>> tuple)
			{
				tuple._4(tuple._1, tuple._2, tuple._3);
			};
			node._args = Tuple.Create(default(T1), t2, t3, a);
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Tuple<T1, T2, T3, Action<T1, T2, T3>> tuple, object arg) => Tuple.Create((T1)arg, tuple._2, tuple._3, tuple._4);
			return node;
		}

		public static bool CanAssignToFirstParam(Type exceptionType)
		{
			return typeof(T1) == exceptionType || exceptionType.IsSubclassOf(typeof(T1));
		}
	}

	private static class Builder<T1, T2, T3, T4>
	{
		public static StaticNode Make(Action<T1, T2, T3, T4> a, T1 t1, T2 t2, T3 t3, T4 t4)
		{
			Node<Tuple<T1, T2, T3, T4, Action<T1, T2, T3, T4>>> node = PooledNode<Node<Tuple<T1, T2, T3, T4, Action<T1, T2, T3, T4>>>>.Request();
			node._action = delegate(Tuple<T1, T2, T3, T4, Action<T1, T2, T3, T4>> tuple)
			{
				tuple._5(tuple._1, tuple._2, tuple._3, tuple._4);
			};
			node._args = Tuple.Create(t1, t2, t3, t4, a);
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Tuple<T1, T2, T3, T4, Action<T1, T2, T3, T4>> result, object arg) => result;
			return node;
		}

		public static StaticNode Make(Action<T1, T2, T3, T4> a, T2 t2, T3 t3, T4 t4)
		{
			Node<Tuple<T1, T2, T3, T4, Action<T1, T2, T3, T4>>> node = PooledNode<Node<Tuple<T1, T2, T3, T4, Action<T1, T2, T3, T4>>>>.Request();
			node._action = delegate(Tuple<T1, T2, T3, T4, Action<T1, T2, T3, T4>> tuple)
			{
				tuple._5(tuple._1, tuple._2, tuple._3, tuple._4);
			};
			node._args = Tuple.Create(default(T1), t2, t3, t4, a);
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Tuple<T1, T2, T3, T4, Action<T1, T2, T3, T4>> tuple, object arg) => Tuple.Create((T1)arg, tuple._2, tuple._3, tuple._4, tuple._5);
			return node;
		}

		public static bool CanAssignToFirstParam(Type exceptionType)
		{
			return typeof(T1) == exceptionType || exceptionType.IsSubclassOf(typeof(T1));
		}
	}

	public static StaticNode Make(Action a)
	{
		return Builder.Make(a);
	}

	public static StaticNode Make<T1>(Action<T1> a, T1 t1)
	{
		return Builder<T1>.Make(a, t1);
	}

	public static StaticNode Make<T1, T2>(Action<T1, T2> a, T1 t1, T2 t2)
	{
		return Builder<T1, T2>.Make(a, t1, t2);
	}

	public static StaticNode Make<T1, T2, T3>(Action<T1, T2, T3> a, T1 t1, T2 t2, T3 t3)
	{
		return Builder<T1, T2, T3>.Make(a, t1, t2, t3);
	}

	public static StaticNode Make<T1, T2, T3, T4>(Action<T1, T2, T3, T4> a, T1 t1, T2 t2, T3 t3, T4 t4)
	{
		return Builder<T1, T2, T3, T4>.Make(a, t1, t2, t3, t4);
	}

	public static StaticNode Make<T1>(Action<T1> a)
	{
		return Builder<T1>.Make(a);
	}

	public static StaticNode Make<T1, T2>(Action<T1, T2> a, T2 t2)
	{
		return Builder<T1, T2>.Make(a, t2);
	}

	public static StaticNode Make<T1, T2, T3>(Action<T1, T2, T3> a, T2 t2, T3 t3)
	{
		return Builder<T1, T2, T3>.Make(a, t2, t3);
	}

	public static StaticNode Make<T1, T2, T3, T4>(Action<T1, T2, T3, T4> a, T2 t2, T3 t3, T4 t4)
	{
		return Builder<T1, T2, T3, T4>.Make(a, t2, t3, t4);
	}
}
