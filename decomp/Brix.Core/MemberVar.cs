public class MemberVar<T> : AbstractVar<T>
{
	private Var<T> v;

	public MemberVar(object o)
	{
		v = Donnees.MemberVar(default(T), o, 1);
	}

	public MemberVar(object o, T x)
	{
		v = Donnees.MemberVar(x, o, 1);
	}

	public override T Get()
	{
		return v.Get();
	}

	public override void Set(T x)
	{
		v.Set(x);
	}

	private MemberVar(Var<T> v2)
	{
		v = v2;
	}

	public static implicit operator Var<T>(MemberVar<T> sv)
	{
		return sv?.v;
	}

	public static implicit operator MemberVar<T>(Var<T> v)
	{
		return new MemberVar<T>(v);
	}
}
