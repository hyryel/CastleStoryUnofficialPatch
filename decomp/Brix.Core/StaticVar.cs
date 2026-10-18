public class StaticVar<T> : AbstractVar<T>
{
	private Var<T> v;

	public StaticVar()
	{
		v = Donnees.StaticVar(default(T), 1);
	}

	public StaticVar(T x)
	{
		v = Donnees.StaticVar(x, 1);
	}

	public override T Get()
	{
		return v.Get();
	}

	public override void Set(T x)
	{
		v.Set(x);
	}

	private StaticVar(Var<T> v2)
	{
		v = v2;
	}

	public static implicit operator Var<T>(StaticVar<T> sv)
	{
		return sv?.v;
	}

	public static implicit operator StaticVar<T>(Var<T> v)
	{
		return new StaticVar<T>(v);
	}
}
