using System;
using UnityEngine;

[Serializable]
public class ColorProp : Prop
{
	public delegate Color Get();

	public delegate void Set(Color value);

	public Get gets;

	public Set sets;

	public ColorProp(string name, Get get, Set set)
		: base(name)
	{
		gets = (Get)Delegate.Combine(gets, get);
		sets = (Set)Delegate.Combine(sets, set);
	}

	public override bool Matches(PropertyBlend blend)
	{
		return blend.GetOutputTypeDesc() == "Color";
	}
}
