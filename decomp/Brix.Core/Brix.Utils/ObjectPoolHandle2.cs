using System;

namespace Brix.Utils;

public sealed class ObjectPoolHandle2<T> : IDisposable, IResetable where T : class, IResetable, new()
{
	private static readonly ObjectPool2<ObjectPoolHandle2<T>> Handles = ObjectPool2.CreatePool<ObjectPoolHandle2<T>>();

	private static readonly ObjectPool2<T> Pool = ObjectPool2.CreatePool<T>();

	private T _value;

	public T Value => _value;

	public bool IsAlive => _value != null;

	public static ObjectPoolHandle2<T> Request()
	{
		ObjectPoolHandle2<T> objectPoolHandle = Handles.Reserve();
		objectPoolHandle._value = Pool.Reserve();
		return objectPoolHandle;
	}

	public void Dispose()
	{
		if (IsAlive)
		{
			Pool.Release(_value);
			Handles.Release(this);
		}
	}

	public void ResetObject()
	{
		_value = null;
	}
}
