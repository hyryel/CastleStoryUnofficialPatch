using System;
using System.Collections.Generic;
using System.Reflection;
using Brix.IO.Serialization.Json.Converters;
using Newtonsoft.Json;
using UnityEngine;

namespace Brix.Transactions;

public class SimpleSpawnPostProcessor : SpawnPostProcessor
{
	public MethodInfo methodInfo;

	[JsonConverter(typeof(TypeIdentifierConverter))]
	public object methodTarget;

	[JsonConverter(typeof(TypeIdentifierConverter))]
	public object[] methodArgs;

	public SimpleSpawnPostProcessor(Delegate d, params object[] args)
	{
		methodInfo = d.Method;
		methodTarget = d.Target;
		methodArgs = args;
	}

	[JsonConstructor]
	private SimpleSpawnPostProcessor(MethodInfo d, object target, params object[] methodArgs)
	{
		methodInfo = d;
		methodTarget = target;
		this.methodArgs = methodArgs;
	}

	public override void OnSpawnPostProcess(GameObject instance)
	{
		List<object> list = new List<object>();
		list.Add(instance);
		list.AddRange(methodArgs);
		methodInfo.Invoke(methodTarget, list.ToArray());
	}

	public static SimpleSpawnPostProcessor Create(Action<GameObject> a)
	{
		return new SimpleSpawnPostProcessor(a);
	}

	public static SimpleSpawnPostProcessor Create<T1>(Action<GameObject, T1> a, T1 b)
	{
		return new SimpleSpawnPostProcessor(a, b);
	}

	public static SimpleSpawnPostProcessor Create<T1, T2>(Action<GameObject, T1, T2> a, T1 b, T2 c)
	{
		return new SimpleSpawnPostProcessor(a, b, c);
	}

	public static SimpleSpawnPostProcessor Create<T1, T2, T3>(Action<GameObject, T1, T2, T3> a, T1 b, T2 c, T3 d)
	{
		return new SimpleSpawnPostProcessor(a, b, c, d);
	}

	public static SimpleSpawnPostProcessor Create<T1>(Func<GameObject, T1> a, T1 b)
	{
		return new SimpleSpawnPostProcessor(a, b);
	}

	public static SimpleSpawnPostProcessor Create<T1, T2>(Func<GameObject, T1, T2> a, T1 b, T2 c)
	{
		return new SimpleSpawnPostProcessor(a, b, c);
	}

	public static SimpleSpawnPostProcessor Create<T1, T2, T3>(Func<GameObject, T1, T2, T3> a, T1 b, T2 c, T3 d)
	{
		return new SimpleSpawnPostProcessor(a, b, c, d);
	}

	public static SimpleSpawnPostProcessor Create<T1, T2, T3, T4>(Func<GameObject, T1, T2, T3, T4> a, T1 b, T2 c, T3 d, T4 e)
	{
		return new SimpleSpawnPostProcessor(a, b, c, d, e);
	}
}
