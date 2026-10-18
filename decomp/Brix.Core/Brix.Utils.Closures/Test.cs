using System;
using UnityEngine;

namespace Brix.Utils.Closures;

public class Test
{
	private static void Run(Action<int, string> f)
	{
		f(8, "height");
		Closure.AClosed<Closure.ABase<int, string>, int, string> f2 = f.Make().Close("width");
		f2.Invoke(5);
		f2.Invoke(6);
		f.Make().Close("depth").Invoke(8);
		Run2(f2);
	}

	private static void Run2<TAction>(TAction f) where TAction : IAction<int>
	{
		f.Invoke(9);
	}

	public static void TestFunc(int x, string s)
	{
		Debug.Log(s + ": " + x);
	}

	public static void Test1()
	{
		Run(TestFunc);
	}

	public static void Test2()
	{
		Var<int> t = new Var<int>();
		Action<int, string, Var<int>> f = delegate(int x, string s, Var<int> _sum)
		{
			_sum.Set(_sum.Get() + x + s.Length);
		};
		Closure.APooled<Closure.AClosed<Closure.ABase<int, string, Var<int>>, int, string, Var<int>>, int, string> aPooled = f.Make().Close(t).Pooled();
		Action<int, string> action = aPooled.Action;
		for (int num = 0; num < 1000; num++)
		{
			Run(action);
		}
		aPooled.Release();
	}

	public static void Test3()
	{
		Action<int, string> f = delegate(int x, string s)
		{
			TestFunc(x, s);
		};
		Action<int> f2 = delegate(int x)
		{
			TestFunc(x, "cheval");
		};
		IClosure<IAction<int>> closure = f.Make().Close("chien").Pooled2();
		IClosure<IAction<int>> closure2 = f2.Make().Pooled2();
		closure.Delegate.Invoke(1);
		closure2.Delegate.Invoke(2);
		closure2.Dispose();
		closure2 = closure;
		closure2.Delegate.Invoke(2);
		closure2.Dispose();
	}
}
