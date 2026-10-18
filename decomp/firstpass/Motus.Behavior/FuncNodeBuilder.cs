using System;
using Brix.External.Signals;
using Smooth.Algebraics;

namespace Motus.Behavior;

public abstract class FuncNodeBuilder
{
	protected static class Builder<TReturn, TNode>
	{
		public static StaticNode Make(Func<TReturn> f)
		{
			FuncNode.Node<Func<TReturn>, TReturn> node = PooledNode<FuncNode.Node<Func<TReturn>, TReturn>>.Request();
			node._function = (Func<TReturn> t) => t();
			node._args = f;
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Func<TReturn> t, object arg) => t;
			return node;
		}

		public static bool CanAssignToFirstParam(Type e)
		{
			return e == typeof(FailException) || e.IsSubclassOf(typeof(FailException));
		}
	}

	protected static class Builder<T1, TReturn, TNode> where TNode : FuncNode.Node<T1, TReturn>, new()
	{
		public static StaticNode Make(Func<T1, TReturn> f, T1 t1)
		{
			FuncNode.Node<T1, TReturn> node = PooledNode<FuncNode.Node<T1, TReturn>>.Request();
			node._function = f;
			node._args = t1;
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (T1 result, object arg) => result;
			return node;
		}

		public static StaticNode Make(Func<T1, TReturn> f)
		{
			FuncNode.Node<T1, TReturn> node = PooledNode<FuncNode.Node<T1, TReturn>>.Request();
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

	protected static class Builder<T1, T2, TReturn, TNode> where TNode : FuncNode.Node<Tuple<T1, T2, Func<T1, T2, TReturn>>, TReturn>, new()
	{
		public static StaticNode Make(Func<T1, T2, TReturn> f, T1 t1, T2 t2)
		{
			FuncNode.Node<Tuple<T1, T2, Func<T1, T2, TReturn>>, TReturn> node = PooledNode<FuncNode.Node<Tuple<T1, T2, Func<T1, T2, TReturn>>, TReturn>>.Request();
			node._function = (Tuple<T1, T2, Func<T1, T2, TReturn>> tuple) => tuple._3(tuple._1, tuple._2);
			node._args = Tuple.Create(t1, t2, f);
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Tuple<T1, T2, Func<T1, T2, TReturn>> result, object arg) => result;
			return node;
		}

		public static StaticNode Make(Func<T1, T2, TReturn> f, T2 t2)
		{
			FuncNode.Node<Tuple<T1, T2, Func<T1, T2, TReturn>>, TReturn> node = PooledNode<FuncNode.Node<Tuple<T1, T2, Func<T1, T2, TReturn>>, TReturn>>.Request();
			node._function = (Tuple<T1, T2, Func<T1, T2, TReturn>> tuple) => tuple._3(tuple._1, tuple._2);
			node._args = Tuple.Create(default(T1), t2, f);
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Tuple<T1, T2, Func<T1, T2, TReturn>> tuple, object arg) => Tuple.Create((T1)arg, tuple._2, tuple._3);
			return node;
		}

		public static bool CanAssignToFirstParam(Type exceptionType)
		{
			return typeof(T1) == exceptionType || exceptionType.IsSubclassOf(typeof(T1));
		}
	}

	protected static class Builder<T1, T2, T3, TReturn, TNode> where TNode : FuncNode.Node<Tuple<T1, T2, T3, Func<T1, T2, T3, TReturn>>, TReturn>, new()
	{
		public static StaticNode Make(Func<T1, T2, T3, TReturn> f, T1 t1, T2 t2, T3 t3)
		{
			FuncNode.Node<Tuple<T1, T2, T3, Func<T1, T2, T3, TReturn>>, TReturn> node = PooledNode<FuncNode.Node<Tuple<T1, T2, T3, Func<T1, T2, T3, TReturn>>, TReturn>>.Request();
			node._function = (Tuple<T1, T2, T3, Func<T1, T2, T3, TReturn>> tuple) => tuple._4(tuple._1, tuple._2, tuple._3);
			node._args = Tuple.Create(t1, t2, t3, f);
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Tuple<T1, T2, T3, Func<T1, T2, T3, TReturn>> result, object arg) => result;
			return node;
		}

		public static StaticNode Make(Func<T1, T2, T3, TReturn> f, T2 t2, T3 t3)
		{
			FuncNode.Node<Tuple<T1, T2, T3, Func<T1, T2, T3, TReturn>>, TReturn> node = PooledNode<FuncNode.Node<Tuple<T1, T2, T3, Func<T1, T2, T3, TReturn>>, TReturn>>.Request();
			node._function = (Tuple<T1, T2, T3, Func<T1, T2, T3, TReturn>> tuple) => tuple._4(tuple._1, tuple._2, tuple._3);
			node._args = Tuple.Create(default(T1), t2, t3, f);
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Tuple<T1, T2, T3, Func<T1, T2, T3, TReturn>> tuple, object arg) => Tuple.Create((T1)arg, tuple._2, tuple._3, tuple._4);
			return node;
		}

		public static bool CanAssignToFirstParam(Type exceptionType)
		{
			return typeof(T1) == exceptionType || exceptionType.IsSubclassOf(typeof(T1));
		}
	}

	protected static class Builder<T1, T2, T3, T4, TReturn, TNode> where TNode : FuncNode.Node<Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, TReturn>>, TReturn>, new()
	{
		public static StaticNode Make(Func<T1, T2, T3, T4, TReturn> f, T1 t1, T2 t2, T3 t3, T4 t4)
		{
			FuncNode.Node<Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, TReturn>>, TReturn> node = PooledNode<FuncNode.Node<Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, TReturn>>, TReturn>>.Request();
			node._function = (Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, TReturn>> tuple) => tuple._5(tuple._1, tuple._2, tuple._3, tuple._4);
			node._args = Tuple.Create(t1, t2, t3, t4, f);
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, TReturn>> result, object arg) => result;
			return node;
		}

		public static StaticNode Make(Func<T1, T2, T3, T4, TReturn> f, T2 t2, T3 t3, T4 t4)
		{
			FuncNode.Node<Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, TReturn>>, TReturn> node = PooledNode<FuncNode.Node<Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, TReturn>>, TReturn>>.Request();
			node._function = (Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, TReturn>> tuple) => tuple._5(tuple._1, tuple._2, tuple._3, tuple._4);
			node._args = Tuple.Create(default(T1), t2, t3, t4, f);
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Tuple<T1, T2, T3, T4, Func<T1, T2, T3, T4, TReturn>> tuple, object arg) => Tuple.Create((T1)arg, tuple._2, tuple._3, tuple._4, tuple._5);
			return node;
		}

		public static bool CanAssignToFirstParam(Type exceptionType)
		{
			return typeof(T1) == exceptionType || exceptionType.IsSubclassOf(typeof(T1));
		}
	}

	protected static class Builder<T1, T2, T3, T4, T5, TReturn, TNode> where TNode : FuncNode.Node<Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, TReturn>>, TReturn>, new()
	{
		public static StaticNode Make(Func<T1, T2, T3, T4, T5, TReturn> f, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
		{
			FuncNode.Node<Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, TReturn>>, TReturn> node = PooledNode<FuncNode.Node<Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, TReturn>>, TReturn>>.Request();
			node._function = (Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, TReturn>> tuple) => tuple._6(tuple._1, tuple._2, tuple._3, tuple._4, tuple._5);
			node._args = Tuple.Create(t1, t2, t3, t4, t5, f);
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, TReturn>> result, object arg) => result;
			return node;
		}

		public static StaticNode Make(Func<T1, T2, T3, T4, T5, TReturn> f, T2 t2, T3 t3, T4 t4, T5 t5)
		{
			FuncNode.Node<Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, TReturn>>, TReturn> node = PooledNode<FuncNode.Node<Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, TReturn>>, TReturn>>.Request();
			node._function = (Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, TReturn>> tuple) => tuple._6(tuple._1, tuple._2, tuple._3, tuple._4, tuple._5);
			node._args = Tuple.Create(default(T1), t2, t3, t4, t5, f);
			node._canAssignToFirstParam = CanAssignToFirstParam;
			node._getWithArgs = (Tuple<T1, T2, T3, T4, T5, Func<T1, T2, T3, T4, T5, TReturn>> tuple, object arg) => Tuple.Create((T1)arg, tuple._2, tuple._3, tuple._4, tuple._5, tuple._6);
			return node;
		}

		public static bool CanAssignToFirstParam(Type exceptionType)
		{
			return typeof(T1) == exceptionType || exceptionType.IsSubclassOf(typeof(T1));
		}
	}
}
