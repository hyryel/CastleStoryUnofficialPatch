using System;
using Brix.External.Signals;
using Brix.Utils.Closures;
using Smooth.Algebraics;
using UnityEngine;

namespace Motus.Behavior;

public abstract class FuncNode : FuncNodeBuilder
{
	public abstract class AbstractFuncNode<TArgs, TReturn, TNode> : PooledNode<TNode> where TNode : AbstractFuncNode<TArgs, TReturn, TNode>
	{
		internal Func<TArgs, object, TArgs> _getWithArgs;

		internal Func<TArgs, TReturn> _function;

		internal Func<Type, bool> _canAssignToFirstParam;

		internal TArgs _args;

		public override bool Catches(FailException e)
		{
			return _canAssignToFirstParam(e.GetType());
		}

		protected override void Clear()
		{
			_getWithArgs = null;
			_function = null;
			_canAssignToFirstParam = null;
			_args = default(TArgs);
		}
	}

	public class Node<TArgs, TReturn> : AbstractFuncNode<TArgs, TReturn, Node<TArgs, TReturn>>
	{
		public override NodeRun Spawn()
		{
			return FuncNodeRun<TArgs, TReturn>.Request(_function, _args, Label, Context, base.Finalizer.Delegate);
		}

		public override NodeRun SpawnWithArg(object arg)
		{
			return FuncNodeRun<TArgs, TReturn>.Request(_function, _getWithArgs(_args, arg), Label, Context, base.Finalizer.Delegate);
		}
	}

	private abstract class AbstractFuncNodeRun<T> : PooledNodeRun<T>, IWithResult where T : AbstractFuncNodeRun<T>
	{
		public abstract bool HasResult { get; }

		public abstract object Result { get; }

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
				Halt();
				base.State = NodeState.Fail;
			}
		}
	}

	private class FuncNodeRun<TArgs, TReturn> : AbstractFuncNodeRun<FuncNodeRun<TArgs, TReturn>>
	{
		private Func<TArgs, TReturn> _func;

		private TArgs _args;

		private bool _invoked;

		private TReturn returnValue;

		public override bool HasResult => _invoked;

		public override object Result => returnValue;

		public static NodeRun Request(Func<TArgs, TReturn> func, TArgs args, string label, UnityEngine.Object context, IAction finalize)
		{
			FuncNodeRun<TArgs, TReturn> funcNodeRun = PooledNodeRun<FuncNodeRun<TArgs, TReturn>>.Request(label, context, finalize);
			funcNodeRun._func = func;
			funcNodeRun._args = args;
			return funcNodeRun;
		}

		protected override void Init()
		{
			_invoked = false;
			returnValue = default(TReturn);
		}

		protected override void Invoke()
		{
			_invoked = true;
			returnValue = _func(_args);
		}

		protected override void Clear()
		{
			_func = null;
			_args = default(TArgs);
		}
	}

	public static StaticNode Make<TReturn>(Func<TReturn> f)
	{
		return Builder<TReturn, Node<Func<TReturn>, TReturn>>.Make(f);
	}

	public static StaticNode Make<T1, TReturn>(Func<T1, TReturn> f, T1 t1)
	{
		return Builder<T1, TReturn, Node<T1, TReturn>>.Make(f, t1);
	}

	public static StaticNode Make<T1, TReturn>(Func<T1, TReturn> f)
	{
		return Builder<T1, TReturn, Node<T1, TReturn>>.Make(f);
	}

	public static StaticNode Make<T1, T2, TReturn>(Func<T1, T2, TReturn> f, T1 t1, T2 t2)
	{
		return Builder<T1, T2, TReturn, Node<Tuple<T1, T2, Func<T1, T2, TReturn>>, TReturn>>.Make(f, t1, t2);
	}

	public static StaticNode Make<T1, T2, TReturn>(Func<T1, T2, TReturn> f, T2 t2)
	{
		return Builder<T1, T2, TReturn, Node<Tuple<T1, T2, Func<T1, T2, TReturn>>, TReturn>>.Make(f, t2);
	}

	public static StaticNode Make<T1, T2, T3, TReturn>(Func<T1, T2, T3, TReturn> a, T1 t1, T2 t2, T3 t3)
	{
		return Builder<T1, T2, T3, TReturn, Node<Tuple<T1, T2, T3, Func<T1, T2, T3, TReturn>>, TReturn>>.Make(a, t1, t2, t3);
	}

	public static StaticNode Make<T1, T2, T3, TReturn>(Func<T1, T2, T3, TReturn> a, T2 t2, T3 t3)
	{
		return Builder<T1, T2, T3, TReturn, Node<Tuple<T1, T2, T3, Func<T1, T2, T3, TReturn>>, TReturn>>.Make(a, t2, t3);
	}

	public static StaticNode Make<T1, T2, T3, T4, TReturn>(Func<T1, T2, T3, T4, TReturn> a, T1 t1, T2 t2, T3 t3, T4 t4)
	{
		return Builder<T1, T2, T3, T4, TReturn, Node<Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, TReturn>>, TReturn>>.Make(a, t1, t2, t3, t4);
	}

	public static StaticNode Make<T1, T2, T3, T4, TReturn>(Func<T1, T2, T3, T4, TReturn> a, T2 t2, T3 t3, T4 t4)
	{
		return Builder<T1, T2, T3, T4, TReturn, Node<Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, TReturn>>, TReturn>>.Make(a, t2, t3, t4);
	}

	public static StaticNode Make<T1, T2, T3, T4, T5, TReturn>(Func<T1, T2, T3, T4, T5, TReturn> a, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
	{
		return Builder<T1, T2, T3, T4, T5, TReturn, Node<Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, TReturn>>, TReturn>>.Make(a, t1, t2, t3, t4, t5);
	}

	public static StaticNode Make<T1, T2, T3, T4, T5, TReturn>(Func<T1, T2, T3, T4, T5, TReturn> a, T2 t2, T3 t3, T4 t4, T5 t5)
	{
		return Builder<T1, T2, T3, T4, T5, TReturn, Node<Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, TReturn>>, TReturn>>.Make(a, t2, t3, t4, t5);
	}
}
