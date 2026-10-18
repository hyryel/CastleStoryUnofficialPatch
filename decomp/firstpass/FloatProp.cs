using System;

[Serializable]
public class FloatProp : Prop
{
	public delegate float Get();

	public delegate void Set(float value);

	public Get gets;

	public Set sets;

	public FloatProp(string name, Get get, Set set)
		: base(name)
	{
		gets = (Get)Delegate.Combine(gets, get);
		sets = (Set)Delegate.Combine(sets, set);
	}

	public override bool Matches(PropertyBlend blend)
	{
		return blend.GetOutputTypeDesc() == "Float";
	}
}
