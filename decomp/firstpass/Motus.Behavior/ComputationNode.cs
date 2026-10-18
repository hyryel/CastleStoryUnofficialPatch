using System;
using System.Collections.Generic;
using Brix.External.Signals;
using Brix.Utils.Closures;
using Smooth.Algebraics;
using UnityEngine;

namespace Motus.Behavior;

public abstract class ComputationNode : FuncNodeBuilder
{
	public class MissingResultException : FailException
	{
		public MissingResultException(string msg)
			: base(msg)
		{
		}
	}

	public class Node<TArgs, TReturn> : FuncNode.AbstractFuncNode<TArgs, IEnumerator<ComputationProgress<TReturn>>, Node<TArgs, TReturn>>
	{
		public override NodeRun Spawn()
		{
			return ComputationNodeRun<TArgs, TReturn>.Request(_function, _args, Label, Context, base.Finalizer.Delegate);
		}

		public override NodeRun SpawnWithArg(object arg)
		{
			return ComputationNodeRun<TArgs, TReturn>.Request(_function, _args, Label, Context, base.Finalizer.Delegate);
		}
	}

	public abstract class AbstractComputationNodeRun<TResult, TNode> : PooledNodeRun<TNode>, IWithResult where TNode : AbstractComputationNodeRun<TResult, TNode>
	{
		private Stack<IEnumerator<ComputationProgress<TResult>>> _fiberStack = new Stack<IEnumerator<ComputationProgress<TResult>>>();

		private ComputationProgress<TResult>.SignalWaitBase _blockingSignal;

		private int _delay;

		private NodeRun _subNodeRun;

		private Node _subNode;

		private TResult _result;

		public bool HasResult => base.State == NodeState.Done;

		public object Result => _result;

		protected abstract IEnumerator<ComputationProgress<TResult>> Invoke();

		protected override void Init()
		{
			_delay = 0;
			_blockingSignal = null;
			_fiberStack.Push(Invoke());
		}

		protected override void Tick()
		{
			if (IsDelayed())
			{
				return;
			}
			try
			{
				do
				{
					TickStack();
				}
				while (ProcessStackTick());
			}
			catch (FailException exception)
			{
				base.State = NodeState.Fail;
				Exception = exception;
			}
		}

		protected override void Terminate()
		{
			if (_subNodeRun != null)
			{
				DisposeSubNode();
			}
			while (_fiberStack.Count > 0)
			{
				_fiberStack.Pop().Dispose();
			}
		}

		private void TickStack()
		{
			if (_subNodeRun != null)
			{
				TickNode();
			}
			else
			{
				TickFiberStack();
			}
		}

		private void TickNode()
		{
			_subNodeRun.Run();
		}

		private void TickFiberStack()
		{
			while (_fiberStack.Count > 0 && !_fiberStack.Peek().MoveNext())
			{
				_fiberStack.Pop().Dispose();
			}
		}

		private bool ProcessStackTick()
		{
			bool result = false;
			if (_subNodeRun != null)
			{
				result = ProcessTickedNode();
			}
			else if (_fiberStack.Count > 0)
			{
				result = ProcessTickedComputationProgress();
			}
			else
			{
				base.State = NodeState.Fail;
				Exception = new MissingResultException("Computation nodes failed to compute value");
			}
			return result;
		}

		private bool ProcessTickedNode()
		{
			bool result = false;
			if (_subNodeRun.State == NodeState.Running)
			{
				return result;
			}
			result = true;
			if (_subNodeRun.State == NodeState.Fail)
			{
				result = false;
				base.State = NodeState.Fail;
				Exception = _subNodeRun.Exception;
			}
			DisposeSubNode();
			return result;
		}

		private bool ProcessTickedComputationProgress()
		{
			bool result = false;
			IEnumerator<ComputationProgress<TResult>> enumerator = _fiberStack.Peek();
			if (enumerator.Current is ComputationProgress<TResult>.SubComputation)
			{
				result = true;
				enumerator = (enumerator.Current as ComputationProgress<TResult>.SubComputation).SubFiber;
				_fiberStack.Push(enumerator);
			}
			else if (enumerator.Current is ComputationProgress<TResult>.Failed)
			{
				Exception = ((ComputationProgress<TResult>.Failed)enumerator.Current).Exception;
				base.State = NodeState.Fail;
			}
			else if (enumerator.Current is ComputationProgress<TResult>.SubNode)
			{
				result = true;
				_subNode = (enumerator.Current as ComputationProgress<TResult>.SubNode).Node;
				_subNodeRun = _subNode.Spawn();
			}
			else if (enumerator.Current is ComputationProgress<TResult>.Waiting)
			{
				_delay = (enumerator.Current as ComputationProgress<TResult>.Waiting).Delay + 1;
				if (!IsDelayed())
				{
					result = true;
				}
			}
			else if (enumerator.Current is ComputationProgress<TResult>.SignalWaitBase)
			{
				_blockingSignal = enumerator.Current as ComputationProgress<TResult>.SignalWaitBase;
			}
			else if (enumerator.Current is ComputationProgress<TResult>.Result)
			{
				_result = (enumerator.Current as ComputationProgress<TResult>.Result).Value;
				base.State = NodeState.Done;
			}
			return result;
		}

		private bool IsDelayed()
		{
			if (--_delay > 0 || (_blockingSignal != null && !_blockingSignal.Called))
			{
				return true;
			}
			_blockingSignal = null;
			return false;
		}

		private void DisposeSubNode()
		{
			_subNodeRun.Dispose();
			_subNode.Dispose();
			_subNodeRun = null;
			_subNode = null;
		}
	}

	public class ComputationNodeRun<TArgs, TReturn> : AbstractComputationNodeRun<TReturn, ComputationNodeRun<TArgs, TReturn>>
	{
		private Func<TArgs, IEnumerator<ComputationProgress<TReturn>>> _func;

		private TArgs _args;

		public static NodeRun Request(Func<TArgs, IEnumerator<ComputationProgress<TReturn>>> func, TArgs args, string label, UnityEngine.Object context, IAction finalize)
		{
			ComputationNodeRun<TArgs, TReturn> computationNodeRun = PooledNodeRun<ComputationNodeRun<TArgs, TReturn>>.Request(label, context, finalize);
			computationNodeRun._func = func;
			computationNodeRun._args = args;
			return computationNodeRun;
		}

		protected override void Clear()
		{
			_func = null;
			_args = default(TArgs);
		}

		protected override IEnumerator<ComputationProgress<TReturn>> Invoke()
		{
			return _func(_args);
		}
	}

	internal new static class Builder<TReturn, TNode>
	{
		public static StaticNode Make(Func<IEnumerator<ComputationProgress<TReturn>>> f)
		{
			Node<Func<IEnumerator<ComputationProgress<TReturn>>>, TReturn> node = PooledNode<Node<Func<IEnumerator<ComputationProgress<TReturn>>>, TReturn>>.Request();
			node._function = (Func<IEnumerator<ComputationProgress<TReturn>>> t) => t();
			node._args = f;
			node._canAssignToFirstParam = Catches;
			node._getWithArgs = (Func<IEnumerator<ComputationProgress<TReturn>>> t, object arg) => t;
			return node;
		}

		public static bool Catches(Type e)
		{
			return e == typeof(FailException) || e.IsSubclassOf(typeof(FailException));
		}
	}

	internal new static class Builder<T1, TReturn, TNode> where TNode : Node<T1, TReturn>, new()
	{
		public static StaticNode Make(Func<T1, IEnumerator<ComputationProgress<TReturn>>> f, T1 t1)
		{
			Node<T1, TReturn> node = PooledNode<Node<T1, TReturn>>.Request();
			node._function = f;
			node._args = t1;
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (T1 result, object arg) => result;
			return node;
		}

		public static StaticNode Make(Func<T1, IEnumerator<ComputationProgress<TReturn>>> f)
		{
			Node<T1, TReturn> node = PooledNode<Node<T1, TReturn>>.Request();
			node._function = f;
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

	internal new static class Builder<T1, T2, TReturn, TNode> where TNode : Node<Tuple<T1, T2, Func<T1, T2, IEnumerator<ComputationProgress<TReturn>>>>, TReturn>, new()
	{
		public static StaticNode Make(Func<T1, T2, IEnumerator<ComputationProgress<TReturn>>> f, T1 t1, T2 t2)
		{
			Node<Tuple<T1, T2, Func<T1, T2, IEnumerator<ComputationProgress<TReturn>>>>, TReturn> node = PooledNode<Node<Tuple<T1, T2, Func<T1, T2, IEnumerator<ComputationProgress<TReturn>>>>, TReturn>>.Request();
			node._function = (Tuple<T1, T2, Func<T1, T2, IEnumerator<ComputationProgress<TReturn>>>> tuple) => tuple._3(tuple._1, tuple._2);
			node._args = Tuple.Create(t1, t2, f);
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Tuple<T1, T2, Func<T1, T2, IEnumerator<ComputationProgress<TReturn>>>> result, object arg) => result;
			return node;
		}

		public static StaticNode Make(Func<T1, T2, IEnumerator<ComputationProgress<TReturn>>> f, T2 t2)
		{
			Node<Tuple<T1, T2, Func<T1, T2, IEnumerator<ComputationProgress<TReturn>>>>, TReturn> node = PooledNode<Node<Tuple<T1, T2, Func<T1, T2, IEnumerator<ComputationProgress<TReturn>>>>, TReturn>>.Request();
			node._function = (Tuple<T1, T2, Func<T1, T2, IEnumerator<ComputationProgress<TReturn>>>> tuple) => tuple._3(tuple._1, tuple._2);
			node._args = Tuple.Create(default(T1), t2, f);
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Tuple<T1, T2, Func<T1, T2, IEnumerator<ComputationProgress<TReturn>>>> tuple, object arg) => Tuple.Create((T1)arg, tuple._2, tuple._3);
			return node;
		}

		public static bool CanAssignToFirstParam(Type paramType)
		{
			return typeof(T1) == paramType || paramType.IsSubclassOf(typeof(T1));
		}
	}

	internal new static class Builder<T1, T2, T3, TReturn, TNode> where TNode : Node<Tuple<T1, T2, T3, Func<T1, T2, T3, IEnumerator<ComputationProgress<TReturn>>>>, TReturn>, new()
	{
		public static StaticNode Make(Func<T1, T2, T3, IEnumerator<ComputationProgress<TReturn>>> f, T1 t1, T2 t2, T3 t3)
		{
			Node<Tuple<T1, T2, T3, Func<T1, T2, T3, IEnumerator<ComputationProgress<TReturn>>>>, TReturn> node = PooledNode<Node<Tuple<T1, T2, T3, Func<T1, T2, T3, IEnumerator<ComputationProgress<TReturn>>>>, TReturn>>.Request();
			node._function = (Tuple<T1, T2, T3, Func<T1, T2, T3, IEnumerator<ComputationProgress<TReturn>>>> tuple) => tuple._4(tuple._1, tuple._2, tuple._3);
			node._args = Tuple.Create(t1, t2, t3, f);
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Tuple<T1, T2, T3, Func<T1, T2, T3, IEnumerator<ComputationProgress<TReturn>>>> result, object arg) => result;
			return node;
		}

		public static StaticNode Make(Func<T1, T2, T3, IEnumerator<ComputationProgress<TReturn>>> f, T2 t2, T3 t3)
		{
			Node<Tuple<T1, T2, T3, Func<T1, T2, T3, IEnumerator<ComputationProgress<TReturn>>>>, TReturn> node = PooledNode<Node<Tuple<T1, T2, T3, Func<T1, T2, T3, IEnumerator<ComputationProgress<TReturn>>>>, TReturn>>.Request();
			node._function = (Tuple<T1, T2, T3, Func<T1, T2, T3, IEnumerator<ComputationProgress<TReturn>>>> tuple) => tuple._4(tuple._1, tuple._2, tuple._3);
			node._args = Tuple.Create(default(T1), t2, t3, f);
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Tuple<T1, T2, T3, Func<T1, T2, T3, IEnumerator<ComputationProgress<TReturn>>>> tuple, object arg) => Tuple.Create((T1)arg, tuple._2, tuple._3, tuple._4);
			return node;
		}

		public static bool CanAssignToFirstParam(Type paramType)
		{
			return typeof(T1) == paramType || paramType.IsSubclassOf(typeof(T1));
		}
	}

	internal new static class Builder<T1, T2, T3, T4, TReturn, TNode> where TNode : Node<Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, IEnumerator<ComputationProgress<TReturn>>>>, TReturn>, new()
	{
		public static StaticNode Make(Func<T1, T2, T3, T4, IEnumerator<ComputationProgress<TReturn>>> f, T1 t1, T2 t2, T3 t3, T4 t4)
		{
			Node<Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, IEnumerator<ComputationProgress<TReturn>>>>, TReturn> node = PooledNode<Node<Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, IEnumerator<ComputationProgress<TReturn>>>>, TReturn>>.Request();
			node._function = (Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, IEnumerator<ComputationProgress<TReturn>>>> tuple) => tuple._5(tuple._1, tuple._2, tuple._3, tuple._4);
			node._args = Tuple.Create(t1, t2, t3, t4, f);
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, IEnumerator<ComputationProgress<TReturn>>>> result, object arg) => result;
			return node;
		}

		public static StaticNode Make(Func<T1, T2, T3, T4, IEnumerator<ComputationProgress<TReturn>>> f, T2 t2, T3 t3, T4 t4)
		{
			Node<Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, IEnumerator<ComputationProgress<TReturn>>>>, TReturn> node = PooledNode<Node<Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, IEnumerator<ComputationProgress<TReturn>>>>, TReturn>>.Request();
			node._function = (Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, IEnumerator<ComputationProgress<TReturn>>>> tuple) => tuple._5(tuple._1, tuple._2, tuple._3, tuple._4);
			node._args = Tuple.Create(default(T1), t2, t3, t4, f);
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, IEnumerator<ComputationProgress<TReturn>>>> tuple, object arg) => Tuple.Create((T1)arg, tuple._2, tuple._3, tuple._4, tuple._5);
			return node;
		}

		public static bool CanAssignToFirstParam(Type paramType)
		{
			return typeof(T1) == paramType || paramType.IsSubclassOf(typeof(T1));
		}
	}

	internal new static class Builder<T1, T2, T3, T4, T5, TReturn, TNode> where TNode : Node<Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, IEnumerator<ComputationProgress<TReturn>>>>, TReturn>, new()
	{
		public static StaticNode Make(Func<T1, T2, T3, T4, T5, IEnumerator<ComputationProgress<TReturn>>> f, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
		{
			Node<Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, IEnumerator<ComputationProgress<TReturn>>>>, TReturn> node = PooledNode<Node<Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, IEnumerator<ComputationProgress<TReturn>>>>, TReturn>>.Request();
			node._function = (Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, IEnumerator<ComputationProgress<TReturn>>>> tuple) => tuple._6(tuple._1, tuple._2, tuple._3, tuple._4, tuple._5);
			node._args = Tuple.Create(t1, t2, t3, t4, t5, f);
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, IEnumerator<ComputationProgress<TReturn>>>> result, object arg) => result;
			return node;
		}

		public static StaticNode Make(Func<T1, T2, T3, T4, T5, IEnumerator<ComputationProgress<TReturn>>> f, T2 t2, T3 t3, T4 t4, T5 t5)
		{
			Node<Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, IEnumerator<ComputationProgress<TReturn>>>>, TReturn> node = PooledNode<Node<Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, IEnumerator<ComputationProgress<TReturn>>>>, TReturn>>.Request();
			node._function = (Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, IEnumerator<ComputationProgress<TReturn>>>> tuple) => tuple._6(tuple._1, tuple._2, tuple._3, tuple._4, tuple._5);
			node._args = Tuple.Create(default(T1), t2, t3, t4, t5, f);
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, IEnumerator<ComputationProgress<TReturn>>>> tuple, object arg) => Tuple.Create((T1)arg, tuple._2, tuple._3, tuple._4, tuple._5, tuple._6);
			return node;
		}

		public static bool CanAssignToFirstParam(Type paramType)
		{
			return typeof(T1) == paramType || paramType.IsSubclassOf(typeof(T1));
		}
	}

	public static Node Make<TReturn>(Func<IEnumerator<ComputationProgress<TReturn>>> f)
	{
		return Builder<TReturn, Node<Func<IEnumerator<ComputationProgress<TReturn>>>, TReturn>>.Make(f);
	}

	public static Node Make<T1, TReturn>(Func<T1, IEnumerator<ComputationProgress<TReturn>>> f, T1 t1)
	{
		return Builder<T1, TReturn, Node<T1, TReturn>>.Make(f, t1);
	}

	public static Node Make<T1, TReturn>(Func<T1, IEnumerator<ComputationProgress<TReturn>>> f)
	{
		return Builder<T1, TReturn, Node<T1, TReturn>>.Make(f);
	}

	public static Node Make<T1, T2, TReturn>(Func<T1, T2, IEnumerator<ComputationProgress<TReturn>>> f, T1 t1, T2 t2)
	{
		return Builder<T1, T2, TReturn, Node<Tuple<T1, T2, Func<T1, T2, IEnumerator<ComputationProgress<TReturn>>>>, TReturn>>.Make(f, t1, t2);
	}

	public static Node Make<T1, T2, TReturn>(Func<T1, T2, IEnumerator<ComputationProgress<TReturn>>> f, T2 t2)
	{
		return Builder<T1, T2, TReturn, Node<Tuple<T1, T2, Func<T1, T2, IEnumerator<ComputationProgress<TReturn>>>>, TReturn>>.Make(f, t2);
	}

	public static Node Make<T1, T2, T3, TReturn>(Func<T1, T2, T3, IEnumerator<ComputationProgress<TReturn>>> a, T1 t1, T2 t2, T3 t3)
	{
		return Builder<T1, T2, T3, TReturn, Node<Tuple<T1, T2, T3, Func<T1, T2, T3, IEnumerator<ComputationProgress<TReturn>>>>, TReturn>>.Make(a, t1, t2, t3);
	}

	public static Node Make<T1, T2, T3, TReturn>(Func<T1, T2, T3, IEnumerator<ComputationProgress<TReturn>>> a, T2 t2, T3 t3)
	{
		return Builder<T1, T2, T3, TReturn, Node<Tuple<T1, T2, T3, Func<T1, T2, T3, IEnumerator<ComputationProgress<TReturn>>>>, TReturn>>.Make(a, t2, t3);
	}

	public static Node Make<T1, T2, T3, T4, TReturn>(Func<T1, T2, T3, T4, IEnumerator<ComputationProgress<TReturn>>> a, T1 t1, T2 t2, T3 t3, T4 t4)
	{
		return Builder<T1, T2, T3, T4, TReturn, Node<Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, IEnumerator<ComputationProgress<TReturn>>>>, TReturn>>.Make(a, t1, t2, t3, t4);
	}

	public static Node Make<T1, T2, T3, T4, TReturn>(Func<T1, T2, T3, T4, IEnumerator<ComputationProgress<TReturn>>> a, T2 t2, T3 t3, T4 t4)
	{
		return Builder<T1, T2, T3, T4, TReturn, Node<Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, IEnumerator<ComputationProgress<TReturn>>>>, TReturn>>.Make(a, t2, t3, t4);
	}

	public static Node Make<T1, T2, T3, T4, T5, TReturn>(Func<T1, T2, T3, T4, T5, IEnumerator<ComputationProgress<TReturn>>> a, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
	{
		return Builder<T1, T2, T3, T4, T5, TReturn, Node<Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, IEnumerator<ComputationProgress<TReturn>>>>, TReturn>>.Make(a, t1, t2, t3, t4, t5);
	}

	public static Node Make<T1, T2, T3, T4, T5, TReturn>(Func<T1, T2, T3, T4, T5, IEnumerator<ComputationProgress<TReturn>>> a, T2 t2, T3 t3, T4 t4, T5 t5)
	{
		return Builder<T1, T2, T3, T4, T5, TReturn, Node<Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, IEnumerator<ComputationProgress<TReturn>>>>, TReturn>>.Make(a, t2, t3, t4, t5);
	}
}
