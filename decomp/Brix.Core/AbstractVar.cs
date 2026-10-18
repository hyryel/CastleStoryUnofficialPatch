public abstract class AbstractVar<T>
{
	public T data
	{
		get
		{
			return Get();
		}
		set
		{
			Set(value);
		}
	}

	public abstract T Get();

	public abstract void Set(T x);

	public override string ToString()
	{
		return "(" + GetType().Name + ")" + data;
	}
}
