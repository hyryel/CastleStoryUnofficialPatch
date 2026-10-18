using System;
using System.Reflection;

namespace Brix.Transactions;

public class UnsynchedFunctionPayload<T> : SynchedFunctionPayload<T>
{
	private MethodInfo _methodInfo;

	public UnsynchedFunctionPayload(MethodInfo methodInfo, object methodTarget)
		: base(Guid.NewGuid(), methodTarget)
	{
		_methodInfo = methodInfo;
	}

	protected override T _Invoke()
	{
		SynchedMethodAttribute.Register(_methodId, _methodInfo);
		return base._Invoke();
	}
}
public class UnsynchedFunctionPayload<A, T> : SynchedFunctionPayload<A, T>
{
	private MethodInfo _methodInfo;

	public UnsynchedFunctionPayload(MethodInfo methodInfo, object methodTarget, A a)
		: base(Guid.NewGuid(), methodTarget, a)
	{
		_methodInfo = methodInfo;
	}

	protected override T _Invoke()
	{
		SynchedMethodAttribute.Register(_methodId, _methodInfo);
		return base._Invoke();
	}
}
public class UnsynchedFunctionPayload<A, B, T> : SynchedFunctionPayload<A, B, T>
{
	private MethodInfo _methodInfo;

	public UnsynchedFunctionPayload(MethodInfo methodInfo, object methodTarget, A a, B b)
		: base(Guid.NewGuid(), methodTarget, a, b)
	{
		_methodInfo = methodInfo;
	}

	protected override T _Invoke()
	{
		SynchedMethodAttribute.Register(_methodId, _methodInfo);
		return base._Invoke();
	}
}
public class UnsynchedFunctionPayload<A, B, C, T> : SynchedFunctionPayload<A, B, C, T>
{
	private MethodInfo _methodInfo;

	public UnsynchedFunctionPayload(MethodInfo methodInfo, object methodTarget, A a, B b, C c)
		: base(Guid.NewGuid(), methodTarget, a, b, c)
	{
		_methodInfo = methodInfo;
	}

	protected override T _Invoke()
	{
		SynchedMethodAttribute.Register(_methodId, _methodInfo);
		return base._Invoke();
	}
}
public class UnsynchedFunctionPayload<A, B, C, D, T> : SynchedFunctionPayload<A, B, C, D, T>
{
	private MethodInfo _methodInfo;

	public UnsynchedFunctionPayload(MethodInfo methodInfo, object methodTarget, A a, B b, C c, D d)
		: base(Guid.NewGuid(), methodTarget, a, b, c, d)
	{
		_methodInfo = methodInfo;
	}

	protected override T _Invoke()
	{
		SynchedMethodAttribute.Register(_methodId, _methodInfo);
		return base._Invoke();
	}
}
public class UnsynchedFunctionPayload<A, B, C, D, E, T> : SynchedFunctionPayload<A, B, C, D, E, T>
{
	private MethodInfo _methodInfo;

	public UnsynchedFunctionPayload(MethodInfo methodInfo, object methodTarget, A a, B b, C c, D d, E e)
		: base(Guid.NewGuid(), methodTarget, a, b, c, d, e)
	{
		_methodInfo = methodInfo;
	}

	protected override T _Invoke()
	{
		SynchedMethodAttribute.Register(_methodId, _methodInfo);
		return base._Invoke();
	}
}
public class UnsynchedFunctionPayload<A, B, C, D, E, F, T> : SynchedFunctionPayload<A, B, C, D, E, F, T>
{
	private MethodInfo _methodInfo;

	public UnsynchedFunctionPayload(MethodInfo methodInfo, object methodTarget, A a, B b, C c, D d, E e, F f)
		: base(Guid.NewGuid(), methodTarget, a, b, c, d, e, f)
	{
		_methodInfo = methodInfo;
	}

	protected override T _Invoke()
	{
		SynchedMethodAttribute.Register(_methodId, _methodInfo);
		return base._Invoke();
	}
}
