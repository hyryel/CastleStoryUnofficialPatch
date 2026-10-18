using System;

namespace Brix.Transactions;

public class SynchedActionPayload : SynchedActionPayloadBase
{
	public SynchedActionPayload(Guid methodId, object methodTarget)
		: base(methodId, methodTarget)
	{
	}

	protected override void _Invoke()
	{
		SynchedMethodAttribute.KeyToMethod[_methodId].Invoke(_methodTarget);
	}
}
public class SynchedActionPayload<A> : SynchedActionPayload
{
	protected A a;

	public SynchedActionPayload(Guid methodId, object methodTarget, A a)
		: base(methodId, methodTarget)
	{
		this.a = a;
	}

	protected override void _Invoke()
	{
		SynchedMethodAttribute.KeyToMethod[_methodId].Invoke(_methodTarget, a);
	}
}
public class SynchedActionPayload<A, B> : SynchedActionPayload<A>
{
	protected B b;

	public SynchedActionPayload(Guid methodId, object methodTarget, A a, B b)
		: base(methodId, methodTarget, a)
	{
		this.b = b;
	}

	protected override void _Invoke()
	{
		SynchedMethodAttribute.KeyToMethod[_methodId].Invoke(_methodTarget, a, b);
	}
}
public class SynchedActionPayload<A, B, C> : SynchedActionPayload<A, B>
{
	protected C c;

	public SynchedActionPayload(Guid methodId, object methodTarget, A a, B b, C c)
		: base(methodId, methodTarget, a, b)
	{
		this.c = c;
	}

	protected override void _Invoke()
	{
		SynchedMethodAttribute.KeyToMethod[_methodId].Invoke(_methodTarget, a, b, c);
	}
}
public class SynchedActionPayload<A, B, C, D> : SynchedActionPayload<A, B, C>
{
	protected D d;

	public SynchedActionPayload(Guid methodId, object methodTarget, A a, B b, C c, D d)
		: base(methodId, methodTarget, a, b, c)
	{
		this.d = d;
	}

	protected override void _Invoke()
	{
		SynchedMethodAttribute.KeyToMethod[_methodId].Invoke(_methodTarget, a, b, c, d);
	}
}
public class SynchedActionPayload<A, B, C, D, E> : SynchedActionPayload<A, B, C, D>
{
	protected E e;

	public SynchedActionPayload(Guid methodId, object methodTarget, A a, B b, C c, D d, E e)
		: base(methodId, methodTarget, a, b, c, d)
	{
		this.e = e;
	}

	protected override void _Invoke()
	{
		SynchedMethodAttribute.KeyToMethod[_methodId].Invoke(_methodTarget, a, b, c, d, e);
	}
}
public class SynchedActionPayload<A, B, C, D, E, F> : SynchedActionPayload<A, B, C, D, E>
{
	protected F f;

	public SynchedActionPayload(Guid methodId, object methodTarget, A a, B b, C c, D d, E e, F f)
		: base(methodId, methodTarget, a, b, c, d, e)
	{
		this.f = f;
	}

	protected override void _Invoke()
	{
		SynchedMethodAttribute.KeyToMethod[_methodId].Invoke(_methodTarget, a, b, c, d, e, f);
	}
}
