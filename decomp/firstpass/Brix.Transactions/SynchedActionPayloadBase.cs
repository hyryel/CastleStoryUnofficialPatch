using System;
using Brix.IO.Serialization.Json.Converters;
using Newtonsoft.Json;

namespace Brix.Transactions;

public class SynchedActionPayloadBase : TransactionPayload
{
	protected class SynchedActionPayloadException : Exception
	{
		public SynchedActionPayloadException(SynchedActionPayloadBase p, Exception e)
			: base(string.Concat("Invocation failed for synchedMethod '", SynchedMethodAttribute.KeyToMethod[p._methodId], "', See inner exception for more details "), e)
		{
		}
	}

	protected Guid _methodId;

	[JsonConverter(typeof(TypeIdentifierConverter))]
	protected object _methodTarget;

	public SynchedActionPayloadBase(Guid methodId, object methodTarget)
	{
		_methodId = methodId;
		_methodTarget = methodTarget;
	}

	protected virtual void _Invoke()
	{
		SynchedMethodAttribute.KeyToMethod[_methodId].Invoke(_methodTarget);
	}

	public override void Invoke()
	{
		try
		{
			_Invoke();
		}
		catch (Exception e)
		{
			throw new SynchedActionPayloadException(this, e);
		}
	}

	public static TransactionPayload Create(Delegate de)
	{
		if (!SynchedMethodAttribute.MethodToKey.TryGetValue(de.Method, out var value))
		{
			return new UnsynchedActionPayload(de.Method, de.Target);
		}
		return new SynchedActionPayload(value, de.Target);
	}

	public static TransactionPayload Create<A>(Delegate de, A a)
	{
		if (!SynchedMethodAttribute.MethodToKey.TryGetValue(de.Method, out var value))
		{
			return new UnsynchedActionPayload<A>(de.Method, de.Target, a);
		}
		return new SynchedActionPayload<A>(value, de.Target, a);
	}

	public static TransactionPayload Create<A, B>(Delegate de, A a, B b)
	{
		if (!SynchedMethodAttribute.MethodToKey.TryGetValue(de.Method, out var value))
		{
			return new UnsynchedActionPayload<A, B>(de.Method, de.Target, a, b);
		}
		return new SynchedActionPayload<A, B>(value, de.Target, a, b);
	}

	public static TransactionPayload Create<A, B, C>(Delegate de, A a, B b, C c)
	{
		if (!SynchedMethodAttribute.MethodToKey.TryGetValue(de.Method, out var value))
		{
			return new UnsynchedActionPayload<A, B, C>(de.Method, de.Target, a, b, c);
		}
		return new SynchedActionPayload<A, B, C>(value, de.Target, a, b, c);
	}

	public static TransactionPayload Create<A, B, C, D>(Delegate de, A a, B b, C c, D d)
	{
		if (!SynchedMethodAttribute.MethodToKey.TryGetValue(de.Method, out var value))
		{
			return new UnsynchedActionPayload<A, B, C, D>(de.Method, de.Target, a, b, c, d);
		}
		return new SynchedActionPayload<A, B, C, D>(value, de.Target, a, b, c, d);
	}

	public static TransactionPayload Create<A, B, C, D, E>(Delegate de, A a, B b, C c, D d, E e)
	{
		if (!SynchedMethodAttribute.MethodToKey.TryGetValue(de.Method, out var value))
		{
			return new UnsynchedActionPayload<A, B, C, D, E>(de.Method, de.Target, a, b, c, d, e);
		}
		return new SynchedActionPayload<A, B, C, D, E>(value, de.Target, a, b, c, d, e);
	}

	public static TransactionPayload Create<A, B, C, D, E, F>(Delegate de, A a, B b, C c, D d, E e, F f)
	{
		if (!SynchedMethodAttribute.MethodToKey.TryGetValue(de.Method, out var value))
		{
			return new UnsynchedActionPayload<A, B, C, D, E, F>(de.Method, de.Target, a, b, c, d, e, f);
		}
		return new SynchedActionPayload<A, B, C, D, E, F>(value, de.Target, a, b, c, d, e, f);
	}
}
