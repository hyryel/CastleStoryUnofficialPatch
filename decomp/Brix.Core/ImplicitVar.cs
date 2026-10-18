public class ImplicitVar<T> : AbstractVar<T>
{
	public delegate T FGet();

	public delegate void FSet(T x);

	private T _mem;

	public FGet _Get;

	public FSet _Set;

	public ImplicitVar()
	{
		_Get = () => _mem;
		_Set = delegate(T x)
		{
			_mem = x;
		};
	}

	public ImplicitVar(T y)
	{
		_mem = y;
		_Get = () => _mem;
		_Set = delegate(T x)
		{
			_mem = x;
		};
	}

	public ImplicitVar(FGet _get, FSet _set)
	{
		_Get = _get;
		_Set = _set;
	}

	public override T Get()
	{
		return _Get();
	}

	public override void Set(T x)
	{
		_Set(x);
	}

	public static implicit operator T(ImplicitVar<T> v)
	{
		return (v != null) ? v.data : default(T);
	}

	public static T operator +(ImplicitVar<T> x, T y)
	{
		return Operator<T>.Add(x, y);
	}

	public static T operator +(T x, ImplicitVar<T> y)
	{
		return Operator<T>.Add(x, y);
	}

	public static T operator +(ImplicitVar<T> x, ImplicitVar<T> y)
	{
		return Operator<T>.Add(x, y);
	}

	public static T operator -(ImplicitVar<T> x, T y)
	{
		return Operator<T>.Subtract(x, y);
	}

	public static T operator -(T x, ImplicitVar<T> y)
	{
		return Operator<T>.Subtract(x, y);
	}

	public static T operator -(ImplicitVar<T> x, ImplicitVar<T> y)
	{
		return Operator<T>.Subtract(x, y);
	}

	public static T operator *(ImplicitVar<T> x, T y)
	{
		return Operator<T>.Multiply(x, y);
	}

	public static T operator *(T x, ImplicitVar<T> y)
	{
		return Operator<T>.Multiply(x, y);
	}

	public static T operator *(ImplicitVar<T> x, ImplicitVar<T> y)
	{
		return Operator<T>.Multiply(x, y);
	}

	public static T operator /(ImplicitVar<T> x, T y)
	{
		return Operator<T>.Divide(x, y);
	}

	public static T operator /(T x, ImplicitVar<T> y)
	{
		return Operator<T>.Divide(x, y);
	}

	public static T operator /(ImplicitVar<T> x, ImplicitVar<T> y)
	{
		return Operator<T>.Divide(x, y);
	}
}
