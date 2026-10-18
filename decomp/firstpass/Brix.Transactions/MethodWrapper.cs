using System;
using System.Reflection;
using Brix.IO.Serialization.Json.Converters;
using Newtonsoft.Json;

namespace Brix.Transactions;

public class MethodWrapper : SimpleMethodWrapper
{
	[JsonConverter(typeof(TypeIdentifierConverter))]
	public object methodTarget;

	[JsonConverter(typeof(TypeIdentifierConverter))]
	public object[] methodArgs;

	public MethodWrapper(Delegate d, object[] args)
		: base(d)
	{
		methodTarget = d.Target;
		methodArgs = args;
	}

	[JsonConstructor]
	private MethodWrapper(MethodInfo d, object target, params object[] methodArgs)
		: base(d)
	{
		methodTarget = target;
		this.methodArgs = methodArgs;
	}

	public object Invoke()
	{
		return Invoke(methodTarget, methodArgs);
	}

	private static MethodWrapper _Create(Delegate d, params object[] args)
	{
		return new MethodWrapper(d, args);
	}

	public static MethodWrapper Create(Action a)
	{
		return _Create(a);
	}

	public static MethodWrapper Create<T1>(Action<T1> a, T1 b)
	{
		return _Create(a, b);
	}

	public static MethodWrapper Create<T1, T2>(Action<T1, T2> a, T1 b, T2 c)
	{
		return _Create(a, b, c);
	}

	public static MethodWrapper Create<T1, T2, T3>(Action<T1, T2, T3> a, T1 b, T2 c, T3 d)
	{
		return _Create(a, b, c, d);
	}

	public static MethodWrapper Create<T1, T2, T3, T4>(Action<T1, T2, T3, T4> a, T1 b, T2 c, T3 d, T4 e)
	{
		return _Create(a, b, c, d, e);
	}

	public static MethodWrapper Create<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> a, T1 b, T2 c, T3 d, T4 e, T5 f)
	{
		return _Create(a, b, c, d, e, f);
	}

	public static MethodWrapper Create<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> a, T1 b, T2 c, T3 d, T4 e, T5 f, T6 g)
	{
		return _Create(a, b, c, d, e, f, g);
	}

	public static MethodWrapper Create<T1>(Func<T1> a, T1 b)
	{
		return _Create(a, b);
	}

	public static MethodWrapper Create<T1, T2>(Func<T1, T2> a, T1 b, T2 c)
	{
		return _Create(a, b, c);
	}

	public static MethodWrapper Create<T1, T2, T3>(Func<T1, T2, T3> a, T1 b, T2 c, T3 d)
	{
		return _Create(a, b, c, d);
	}

	public static MethodWrapper Create<T1, T2, T3, T4>(Func<T1, T2, T3, T4> a, T1 b, T2 c, T3 d, T4 e)
	{
		return _Create(a, b, c, d, e);
	}

	public static MethodWrapper Create<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5> a, T1 b, T2 c, T3 d, T4 e, T5 f)
	{
		return _Create(a, b, c, d, e, f);
	}

	public static MethodWrapper Create<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6> a, T1 b, T2 c, T3 d, T4 e, T5 f, T6 g)
	{
		return _Create(a, b, c, d, e, f, g);
	}
}
