using System;
using Brix.Utils;

public class Var<T> : AbstractVar<T>, IPoolable, IResetable, IDisposable
{
	private T _data;

	public Var()
	{
		_data = default(T);
	}

	public Var(T x)
	{
		_data = x;
	}

	public override T Get()
	{
		return _data;
	}

	public override void Set(T x)
	{
		_data = x;
	}

	public static Var<T> Request()
	{
		return ObjectPool<Var<T>>.Reserve();
	}

	public static Var<T> Request(T x)
	{
		Var<T> var = ObjectPool<Var<T>>.Reserve();
		var._data = x;
		return var;
	}

	public void ResetObject()
	{
		_data = default(T);
	}

	public void Dispose()
	{
		ObjectPool<Var<T>>.Release(this);
	}
}
