using System;

public class Lazy<T> where T : class
{
	private T t;

	private Func<T> f;

	public T Value
	{
		get
		{
			if (t == null && f != null)
			{
				Func<T> func = f;
				f = null;
				t = func();
			}
			return t;
		}
		set
		{
			t = value;
		}
	}

	public Lazy(Func<T> f)
	{
		t = null;
		this.f = f;
	}
}
