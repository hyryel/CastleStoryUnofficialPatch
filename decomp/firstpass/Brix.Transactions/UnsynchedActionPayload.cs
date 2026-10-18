using System;
using System.Reflection;

namespace Brix.Transactions;

public class UnsynchedActionPayload : SynchedActionPayload
{
	private MethodInfo _methodInfo;

	public UnsynchedActionPayload(MethodInfo methodInfo, object methodTarget)
		: base(Guid.NewGuid(), methodTarget)
	{
		_methodInfo = methodInfo;
	}

	protected override void _Invoke()
	{
		SynchedMethodAttribute.Register(_methodId, _methodInfo);
		base._Invoke();
	}
}
public class UnsynchedActionPayload<A> : SynchedActionPayload<A>
{
	private MethodInfo _methodInfo;

	public UnsynchedActionPayload(MethodInfo methodInfo, object methodTarget, A a)
		: base(Guid.NewGuid(), methodTarget, a)
	{
		_methodInfo = methodInfo;
	}

	protected override void _Invoke()
	{
		SynchedMethodAttribute.Register(_methodId, _methodInfo);
		base._Invoke();
	}
}
public class UnsynchedActionPayload<A, B> : SynchedActionPayload<A, B>
{
	private MethodInfo _methodInfo;

	public UnsynchedActionPayload(MethodInfo methodInfo, object methodTarget, A a, B b)
		: base(Guid.NewGuid(), methodTarget, a, b)
	{
		_methodInfo = methodInfo;
	}

	protected override void _Invoke()
	{
		SynchedMethodAttribute.Register(_methodId, _methodInfo);
		base._Invoke();
	}
}
public class UnsynchedActionPayload<A, B, C> : SynchedActionPayload<A, B, C>
{
	private MethodInfo _methodInfo;

	public UnsynchedActionPayload(MethodInfo methodInfo, object methodTarget, A a, B b, C c)
		: base(Guid.NewGuid(), methodTarget, a, b, c)
	{
		_methodInfo = methodInfo;
	}

	protected override void _Invoke()
	{
		SynchedMethodAttribute.Register(_methodId, _methodInfo);
		base._Invoke();
	}
}
public class UnsynchedActionPayload<A, B, C, D> : SynchedActionPayload<A, B, C, D>
{
	private MethodInfo _methodInfo;

	public UnsynchedActionPayload(MethodInfo methodInfo, object methodTarget, A a, B b, C c, D d)
		: base(Guid.NewGuid(), methodTarget, a, b, c, d)
	{
		_methodInfo = methodInfo;
	}

	protected override void _Invoke()
	{
		SynchedMethodAttribute.Register(_methodId, _methodInfo);
		base._Invoke();
	}
}
public class UnsynchedActionPayload<A, B, C, D, E> : SynchedActionPayload<A, B, C, D, E>
{
	private MethodInfo _methodInfo;

	public UnsynchedActionPayload(MethodInfo methodInfo, object methodTarget, A a, B b, C c, D d, E e)
		: base(Guid.NewGuid(), methodTarget, a, b, c, d, e)
	{
		_methodInfo = methodInfo;
	}

	protected override void _Invoke()
	{
		SynchedMethodAttribute.Register(_methodId, _methodInfo);
		base._Invoke();
	}
}
public class UnsynchedActionPayload<A, B, C, D, E, F> : SynchedActionPayload<A, B, C, D, E, F>
{
	private MethodInfo _methodInfo;

	public UnsynchedActionPayload(MethodInfo methodInfo, object methodTarget, A a, B b, C c, D d, E e, F f)
		: base(Guid.NewGuid(), methodTarget, a, b, c, d, e, f)
	{
		_methodInfo = methodInfo;
	}

	protected override void _Invoke()
	{
		SynchedMethodAttribute.Register(_methodId, _methodInfo);
		base._Invoke();
	}
}
