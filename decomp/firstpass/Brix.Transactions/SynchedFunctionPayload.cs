using System;
using Brix.IO.Serialization.Json.Converters;
using Newtonsoft.Json;

namespace Brix.Transactions;

public class SynchedFunctionPayload<T> : TransactionPayload<GenericReturnValue<T>>
{
	protected class SynchedFunctionPayloadException : Exception
	{
		public SynchedFunctionPayloadException(SynchedFunctionPayload<T> p, Exception e)
			: base(string.Concat("Invocation failed for synchedMethod '", SynchedMethodAttribute.KeyToMethod[p._methodId], "', See inner exception for more details "), e)
		{
		}
	}

	protected Guid _methodId;

	[JsonConverter(typeof(TypeIdentifierConverter))]
	protected object _methodTarget;

	public SynchedFunctionPayload(Guid methodId, object methodTarget)
	{
		_methodId = methodId;
		_methodTarget = methodTarget;
	}

	protected virtual T _Invoke()
	{
		return (T)SynchedMethodAttribute.KeyToMethod[_methodId].Invoke(_methodTarget);
	}

	public override void Invoke(GenericReturnValue<T> returnValue)
	{
		try
		{
			returnValue.Result = _Invoke();
		}
		catch (Exception e)
		{
			throw new SynchedFunctionPayloadException(this, e);
		}
	}

	public static TransactionPayload<GenericReturnValue<T>> Create(Delegate de)
	{
		if (!SynchedMethodAttribute.MethodToKey.TryGetValue(de.Method, out var value))
		{
			return new UnsynchedFunctionPayload<T>(de.Method, de.Target);
		}
		return new SynchedFunctionPayload<T>(value, de.Target);
	}

	public static TransactionPayload<GenericReturnValue<T>> Create<A>(Delegate de, A a)
	{
		if (!SynchedMethodAttribute.MethodToKey.TryGetValue(de.Method, out var value))
		{
			return new UnsynchedFunctionPayload<A, T>(de.Method, de.Target, a);
		}
		return new SynchedFunctionPayload<A, T>(value, de.Target, a);
	}

	public static TransactionPayload<GenericReturnValue<T>> Create<A, B>(Delegate de, A a, B b)
	{
		if (!SynchedMethodAttribute.MethodToKey.TryGetValue(de.Method, out var value))
		{
			return new UnsynchedFunctionPayload<A, B, T>(de.Method, de.Target, a, b);
		}
		return new SynchedFunctionPayload<A, B, T>(value, de.Target, a, b);
	}

	public static TransactionPayload<GenericReturnValue<T>> Create<A, B, C>(Delegate de, A a, B b, C c)
	{
		if (!SynchedMethodAttribute.MethodToKey.TryGetValue(de.Method, out var value))
		{
			return new UnsynchedFunctionPayload<A, B, C, T>(de.Method, de.Target, a, b, c);
		}
		return new SynchedFunctionPayload<A, B, C, T>(value, de.Target, a, b, c);
	}

	public static TransactionPayload<GenericReturnValue<T>> Create<A, B, C, D>(Delegate de, A a, B b, C c, D d)
	{
		if (!SynchedMethodAttribute.MethodToKey.TryGetValue(de.Method, out var value))
		{
			return new UnsynchedFunctionPayload<A, B, C, D, T>(de.Method, de.Target, a, b, c, d);
		}
		return new SynchedFunctionPayload<A, B, C, D, T>(value, de.Target, a, b, c, d);
	}

	public static TransactionPayload<GenericReturnValue<T>> Create<A, B, C, D, E>(Delegate de, A a, B b, C c, D d, E e)
	{
		if (!SynchedMethodAttribute.MethodToKey.TryGetValue(de.Method, out var value))
		{
			return new UnsynchedFunctionPayload<A, B, C, D, E, T>(de.Method, de.Target, a, b, c, d, e);
		}
		return new SynchedFunctionPayload<A, B, C, D, E, T>(value, de.Target, a, b, c, d, e);
	}

	public static TransactionPayload<GenericReturnValue<T>> Create<A, B, C, D, E, F>(Delegate de, A a, B b, C c, D d, E e, F f)
	{
		if (!SynchedMethodAttribute.MethodToKey.TryGetValue(de.Method, out var value))
		{
			return new UnsynchedFunctionPayload<A, B, C, D, E, F, T>(de.Method, de.Target, a, b, c, d, e, f);
		}
		return new SynchedFunctionPayload<A, B, C, D, E, F, T>(value, de.Target, a, b, c, d, e, f);
	}
}
public class SynchedFunctionPayload<A, T> : SynchedFunctionPayload<T>
{
	protected A a;

	public SynchedFunctionPayload(Guid methodId, object methodTarget, A a)
		: base(methodId, methodTarget)
	{
		this.a = a;
	}

	protected override T _Invoke()
	{
		return (T)SynchedMethodAttribute.KeyToMethod[_methodId].Invoke(_methodTarget, a);
	}
}
public class SynchedFunctionPayload<A, B, T> : SynchedFunctionPayload<A, T>
{
	protected B b;

	public SynchedFunctionPayload(Guid methodId, object methodTarget, A a, B b)
		: base(methodId, methodTarget, a)
	{
		this.b = b;
	}

	protected override T _Invoke()
	{
		return (T)SynchedMethodAttribute.KeyToMethod[_methodId].Invoke(_methodTarget, a);
	}
}
public class SynchedFunctionPayload<A, B, C, T> : SynchedFunctionPayload<A, B, T>
{
	protected C c;

	public SynchedFunctionPayload(Guid methodId, object methodTarget, A a, B b, C c)
		: base(methodId, methodTarget, a, b)
	{
		this.c = c;
	}

	protected override T _Invoke()
	{
		return (T)SynchedMethodAttribute.KeyToMethod[_methodId].Invoke(_methodTarget, a);
	}
}
public class SynchedFunctionPayload<A, B, C, D, T> : SynchedFunctionPayload<A, B, C, T>
{
	protected D d;

	public SynchedFunctionPayload(Guid methodId, object methodTarget, A a, B b, C c, D d)
		: base(methodId, methodTarget, a, b, c)
	{
		this.d = d;
	}

	protected override T _Invoke()
	{
		return (T)SynchedMethodAttribute.KeyToMethod[_methodId].Invoke(_methodTarget, a);
	}
}
public class SynchedFunctionPayload<A, B, C, D, E, T> : SynchedFunctionPayload<A, B, C, D, T>
{
	protected E e;

	public SynchedFunctionPayload(Guid methodId, object methodTarget, A a, B b, C c, D d, E e)
		: base(methodId, methodTarget, a, b, c, d)
	{
		this.e = e;
	}

	protected override T _Invoke()
	{
		return (T)SynchedMethodAttribute.KeyToMethod[_methodId].Invoke(_methodTarget, a);
	}
}
public class SynchedFunctionPayload<A, B, C, D, E, F, T> : SynchedFunctionPayload<A, B, C, D, E, T>
{
	protected F f;

	public SynchedFunctionPayload(Guid methodId, object methodTarget, A a, B b, C c, D d, E e, F f)
		: base(methodId, methodTarget, a, b, c, d, e)
	{
		this.f = f;
	}

	protected override T _Invoke()
	{
		return (T)SynchedMethodAttribute.KeyToMethod[_methodId].Invoke(_methodTarget, a);
	}
}
