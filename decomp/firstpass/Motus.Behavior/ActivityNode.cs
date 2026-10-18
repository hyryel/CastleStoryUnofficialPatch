using System;
using System.Collections.Generic;
using Brix.External.Signals;
using Brix.Utils.Closures;
using Smooth.Algebraics;
using UnityEngine;

namespace Motus.Behavior;

public abstract class ActivityNode : FuncNodeBuilder
{
	public class Node<TArgs> : FuncNode.AbstractFuncNode<TArgs, IEnumerator<ActivityProgress>, Node<TArgs>>
	{
		public override NodeRun Spawn()
		{
			return ActivityNodeRun<TArgs>.Request(_function, _args, Label, Context, base.Finalizer.Delegate);
		}

		public override NodeRun SpawnWithArg(object arg)
		{
			return ActivityNodeRun<TArgs>.Request(_function, _getWithArgs(_args, arg), Label, Context, base.Finalizer.Delegate);
		}
	}

	public abstract class AbstractActivityNodeRun<T> : PooledNodeRun<T> where T : AbstractActivityNodeRun<T>
	{
		private Stack<IEnumerator<ActivityProgress>> _fiberStack = new Stack<IEnumerator<ActivityProgress>>();

		private ActivityProgress.SignalWaitBase _blockingSignal;

		private int _delay;

		private Node _subNode;

		private NodeRun _subNodeRun;

		protected abstract IEnumerator<ActivityProgress> Invoke();

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
			while (_fiberStack.Count != 0)
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
				result = ProcessTickedActivityProgress();
			}
			else
			{
				base.State = NodeState.Done;
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
			if (_subNodeRun.State == NodeState.Fail)
			{
				result = false;
				base.State = NodeState.Fail;
				Exception = _subNodeRun.Exception;
			}
			else
			{
				result = true;
			}
			DisposeSubNode();
			return result;
		}

		private bool ProcessTickedActivityProgress()
		{
			bool result = false;
			IEnumerator<ActivityProgress> enumerator = _fiberStack.Peek();
			if (enumerator.Current is ActivityProgress.SubActivity)
			{
				result = true;
				enumerator = (enumerator.Current as ActivityProgress.SubActivity).SubFiber;
				_fiberStack.Push(enumerator);
			}
			else if (enumerator.Current is ActivityProgress.Failed)
			{
				Exception = ((ActivityProgress.Failed)enumerator.Current).Exception;
				base.State = NodeState.Fail;
			}
			else if (enumerator.Current is ActivityProgress.SubNode)
			{
				result = true;
				_subNode = (enumerator.Current as ActivityProgress.SubNode).Node;
				_subNodeRun = _subNode.Spawn();
			}
			else if (enumerator.Current is ActivityProgress.WaitingDelay)
			{
				_delay = (enumerator.Current as ActivityProgress.WaitingDelay).Delay + 1;
				if (!IsDelayed())
				{
					result = true;
				}
			}
			else if (enumerator.Current is ActivityProgress.SignalWaitBase)
			{
				_blockingSignal = enumerator.Current as ActivityProgress.SignalWaitBase;
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

	public class ActivityNodeRun<TArgs> : AbstractActivityNodeRun<ActivityNodeRun<TArgs>>
	{
		private Func<TArgs, IEnumerator<ActivityProgress>> _func;

		private TArgs _args;

		public static NodeRun Request(Func<TArgs, IEnumerator<ActivityProgress>> func, TArgs args, string label, UnityEngine.Object context, IAction finalize)
		{
			ActivityNodeRun<TArgs> activityNodeRun = PooledNodeRun<ActivityNodeRun<TArgs>>.Request(label, context, finalize);
			activityNodeRun._func = func;
			activityNodeRun._args = args;
			return activityNodeRun;
		}

		protected override void Clear()
		{
			_func = null;
			_args = default(TArgs);
		}

		protected override IEnumerator<ActivityProgress> Invoke()
		{
			return _func(_args);
		}
	}

	protected new static class Builder<TReturn, TNode>
	{
		public static StaticNode Make(Func<IEnumerator<ActivityProgress>> f)
		{
			Node<Func<IEnumerator<ActivityProgress>>> node = PooledNode<Node<Func<IEnumerator<ActivityProgress>>>>.Request();
			node._function = (Func<IEnumerator<ActivityProgress>> t) => t();
			node._args = f;
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Func<IEnumerator<ActivityProgress>> t, object arg) => t;
			return node;
		}

		public static bool CanAssignToFirstParam(Type e)
		{
			return e == typeof(FailException) || e.IsSubclassOf(typeof(FailException));
		}
	}

	protected new static class Builder<T1, TReturn, TNode> where TNode : Node<T1>, new()
	{
		public static StaticNode Make(Func<T1, IEnumerator<ActivityProgress>> f, T1 t1)
		{
			Node<T1> node = PooledNode<Node<T1>>.Request();
			node._function = f;
			node._args = t1;
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (T1 result, object arg) => result;
			return node;
		}

		public static StaticNode Make(Func<T1, IEnumerator<ActivityProgress>> f)
		{
			Node<T1> node = PooledNode<Node<T1>>.Request();
			node._function = f;
			node._args = default(T1);
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (T1 t, object arg) => (T1)arg;
			return node;
		}

		public static bool CanAssignToFirstParam(Type paramType)
		{
			return typeof(T1) == paramType || paramType.IsSubclassOf(typeof(T1));
		}
	}

	protected new static class Builder<T1, T2, TReturn, TNode> where TNode : Node<Tuple<T1, T2, Func<T1, T2, IEnumerator<ActivityProgress>>>>, new()
	{
		public static StaticNode Make(Func<T1, T2, IEnumerator<ActivityProgress>> f, T1 t1, T2 t2)
		{
			Node<Tuple<T1, T2, Func<T1, T2, IEnumerator<ActivityProgress>>>> node = PooledNode<Node<Tuple<T1, T2, Func<T1, T2, IEnumerator<ActivityProgress>>>>>.Request();
			node._function = (Tuple<T1, T2, Func<T1, T2, IEnumerator<ActivityProgress>>> tuple) => tuple._3(tuple._1, tuple._2);
			node._args = Tuple.Create(t1, t2, f);
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Tuple<T1, T2, Func<T1, T2, IEnumerator<ActivityProgress>>> result, object arg) => result;
			return node;
		}

		public static StaticNode Make(Func<T1, T2, IEnumerator<ActivityProgress>> f, T2 t2)
		{
			Node<Tuple<T1, T2, Func<T1, T2, IEnumerator<ActivityProgress>>>> node = PooledNode<Node<Tuple<T1, T2, Func<T1, T2, IEnumerator<ActivityProgress>>>>>.Request();
			node._function = (Tuple<T1, T2, Func<T1, T2, IEnumerator<ActivityProgress>>> tuple) => tuple._3(tuple._1, tuple._2);
			node._args = Tuple.Create(default(T1), t2, f);
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Tuple<T1, T2, Func<T1, T2, IEnumerator<ActivityProgress>>> tuple, object arg) => Tuple.Create((T1)arg, tuple._2, tuple._3);
			return node;
		}

		public static bool CanAssignToFirstParam(Type paramType)
		{
			return typeof(T1) == paramType || paramType.IsSubclassOf(typeof(T1));
		}
	}

	protected new static class Builder<T1, T2, T3, TReturn, TNode> where TNode : Node<Tuple<T1, T2, T3, Func<T1, T2, T3, IEnumerator<ActivityProgress>>>>, new()
	{
		public static StaticNode Make(Func<T1, T2, T3, IEnumerator<ActivityProgress>> f, T1 t1, T2 t2, T3 t3)
		{
			Node<Tuple<T1, T2, T3, Func<T1, T2, T3, IEnumerator<ActivityProgress>>>> node = PooledNode<Node<Tuple<T1, T2, T3, Func<T1, T2, T3, IEnumerator<ActivityProgress>>>>>.Request();
			node._function = (Tuple<T1, T2, T3, Func<T1, T2, T3, IEnumerator<ActivityProgress>>> tuple) => tuple._4(tuple._1, tuple._2, tuple._3);
			node._args = Tuple.Create(t1, t2, t3, f);
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Tuple<T1, T2, T3, Func<T1, T2, T3, IEnumerator<ActivityProgress>>> result, object arg) => result;
			return node;
		}

		public static StaticNode Make(Func<T1, T2, T3, IEnumerator<ActivityProgress>> f, T2 t2, T3 t3)
		{
			Node<Tuple<T1, T2, T3, Func<T1, T2, T3, IEnumerator<ActivityProgress>>>> node = PooledNode<Node<Tuple<T1, T2, T3, Func<T1, T2, T3, IEnumerator<ActivityProgress>>>>>.Request();
			node._function = (Tuple<T1, T2, T3, Func<T1, T2, T3, IEnumerator<ActivityProgress>>> tuple) => tuple._4(tuple._1, tuple._2, tuple._3);
			node._args = Tuple.Create(default(T1), t2, t3, f);
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Tuple<T1, T2, T3, Func<T1, T2, T3, IEnumerator<ActivityProgress>>> tuple, object arg) => Tuple.Create((T1)arg, tuple._2, tuple._3, tuple._4);
			return node;
		}

		public static bool CanAssignToFirstParam(Type paramType)
		{
			return typeof(T1) == paramType || paramType.IsSubclassOf(typeof(T1));
		}
	}

	protected new static class Builder<T1, T2, T3, T4, TReturn, TNode> where TNode : Node<Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, IEnumerator<ActivityProgress>>>>, new()
	{
		public static StaticNode Make(Func<T1, T2, T3, T4, IEnumerator<ActivityProgress>> f, T1 t1, T2 t2, T3 t3, T4 t4)
		{
			Node<Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, IEnumerator<ActivityProgress>>>> node = PooledNode<Node<Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, IEnumerator<ActivityProgress>>>>>.Request();
			node._function = (Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, IEnumerator<ActivityProgress>>> tuple) => tuple._5(tuple._1, tuple._2, tuple._3, tuple._4);
			node._args = Tuple.Create(t1, t2, t3, t4, f);
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, IEnumerator<ActivityProgress>>> result, object arg) => result;
			return node;
		}

		public static StaticNode Make(Func<T1, T2, T3, T4, IEnumerator<ActivityProgress>> f, T2 t2, T3 t3, T4 t4)
		{
			Node<Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, IEnumerator<ActivityProgress>>>> node = PooledNode<Node<Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, IEnumerator<ActivityProgress>>>>>.Request();
			node._function = (Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, IEnumerator<ActivityProgress>>> tuple) => tuple._5(tuple._1, tuple._2, tuple._3, tuple._4);
			node._args = Tuple.Create(default(T1), t2, t3, t4, f);
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, IEnumerator<ActivityProgress>>> tuple, object arg) => Tuple.Create((T1)arg, tuple._2, tuple._3, tuple._4, tuple._5);
			return node;
		}

		public static bool CanAssignToFirstParam(Type paramType)
		{
			return typeof(T1) == paramType || paramType.IsSubclassOf(typeof(T1));
		}
	}

	protected new static class Builder<T1, T2, T3, T4, T5, TReturn, TNode> where TNode : Node<Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, IEnumerator<ActivityProgress>>>>, new()
	{
		public static StaticNode Make(Func<T1, T2, T3, T4, T5, IEnumerator<ActivityProgress>> f, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
		{
			Node<Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, IEnumerator<ActivityProgress>>>> node = PooledNode<Node<Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, IEnumerator<ActivityProgress>>>>>.Request();
			node._function = (Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, IEnumerator<ActivityProgress>>> tuple) => tuple._6(tuple._1, tuple._2, tuple._3, tuple._4, tuple._5);
			node._args = Tuple.Create(t1, t2, t3, t4, t5, f);
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, IEnumerator<ActivityProgress>>> result, object arg) => result;
			return node;
		}

		public static StaticNode Make(Func<T1, T2, T3, T4, T5, IEnumerator<ActivityProgress>> f, T2 t2, T3 t3, T4 t4, T5 t5)
		{
			Node<Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, IEnumerator<ActivityProgress>>>> node = PooledNode<Node<Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, IEnumerator<ActivityProgress>>>>>.Request();
			node._function = (Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, IEnumerator<ActivityProgress>>> tuple) => tuple._6(tuple._1, tuple._2, tuple._3, tuple._4, tuple._5);
			node._args = Tuple.Create(default(T1), t2, t3, t4, t5, f);
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, IEnumerator<ActivityProgress>>> tuple, object arg) => Tuple.Create((T1)arg, tuple._2, tuple._3, tuple._4, tuple._5, tuple._6);
			return node;
		}

		public static bool CanAssignToFirstParam(Type paramType)
		{
			return typeof(T1) == paramType || paramType.IsSubclassOf(typeof(T1));
		}
	}

	public static StaticNode Make(Func<IEnumerator<ActivityProgress>> f)
	{
		return Builder<IEnumerator<ActivityProgress>, Node<Func<IEnumerator<ActivityProgress>>>>.Make(f);
	}

	public static StaticNode Make<T1>(Func<T1, IEnumerator<ActivityProgress>> f, T1 t1)
	{
		return Builder<T1, IEnumerator<ActivityProgress>, Node<T1>>.Make(f, t1);
	}

	public static StaticNode Make<T1>(Func<T1, IEnumerator<ActivityProgress>> f)
	{
		return Builder<T1, IEnumerator<ActivityProgress>, Node<T1>>.Make(f);
	}

	public static StaticNode Make<T1, T2>(Func<T1, T2, IEnumerator<ActivityProgress>> f, T1 t1, T2 t2)
	{
		return Builder<T1, T2, IEnumerator<ActivityProgress>, Node<Tuple<T1, T2, Func<T1, T2, IEnumerator<ActivityProgress>>>>>.Make(f, t1, t2);
	}

	public static StaticNode Make<T1, T2>(Func<T1, T2, IEnumerator<ActivityProgress>> f, T2 t2)
	{
		return Builder<T1, T2, IEnumerator<ActivityProgress>, Node<Tuple<T1, T2, Func<T1, T2, IEnumerator<ActivityProgress>>>>>.Make(f, t2);
	}

	public static StaticNode Make<T1, T2, T3>(Func<T1, T2, T3, IEnumerator<ActivityProgress>> f, T1 t1, T2 t2, T3 t3)
	{
		return Builder<T1, T2, T3, IEnumerator<ActivityProgress>, Node<Tuple<T1, T2, T3, Func<T1, T2, T3, IEnumerator<ActivityProgress>>>>>.Make(f, t1, t2, t3);
	}

	public static StaticNode Make<T1, T2, T3>(Func<T1, T2, T3, IEnumerator<ActivityProgress>> f, T2 t2, T3 t3)
	{
		return Builder<T1, T2, T3, IEnumerator<ActivityProgress>, Node<Tuple<T1, T2, T3, Func<T1, T2, T3, IEnumerator<ActivityProgress>>>>>.Make(f, t2, t3);
	}

	public static StaticNode Make<T1, T2, T3, T4>(Func<T1, T2, T3, T4, IEnumerator<ActivityProgress>> f, T1 t1, T2 t2, T3 t3, T4 t4)
	{
		return Builder<T1, T2, T3, T4, IEnumerator<ActivityProgress>, Node<Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, IEnumerator<ActivityProgress>>>>>.Make(f, t1, t2, t3, t4);
	}

	public static StaticNode Make<T1, T2, T3, T4>(Func<T1, T2, T3, T4, IEnumerator<ActivityProgress>> f, T2 t2, T3 t3, T4 t4)
	{
		return Builder<T1, T2, T3, T4, IEnumerator<ActivityProgress>, Node<Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, IEnumerator<ActivityProgress>>>>>.Make(f, t2, t3, t4);
	}

	public static StaticNode Make<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, IEnumerator<ActivityProgress>> f, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
	{
		return Builder<T1, T2, T3, T4, T5, IEnumerator<ActivityProgress>, Node<Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, IEnumerator<ActivityProgress>>>>>.Make(f, t1, t2, t3, t4, t5);
	}

	public static StaticNode Make<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, IEnumerator<ActivityProgress>> f, T2 t2, T3 t3, T4 t4, T5 t5)
	{
		return Builder<T1, T2, T3, T4, T5, IEnumerator<ActivityProgress>, Node<Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, IEnumerator<ActivityProgress>>>>>.Make(f, t2, t3, t4, t5);
	}
}
