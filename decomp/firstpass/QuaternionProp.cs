using System;
using UnityEngine;

[Serializable]
public class QuaternionProp : Prop
{
	public delegate Quaternion Get();

	public delegate void Set(Quaternion value);

	public Get gets;

	public Set sets;

	public QuaternionProp(string name, Get get, Set set)
		: base(name)
	{
		gets = (Get)Delegate.Combine(gets, get);
		sets = (Set)Delegate.Combine(sets, set);
	}

	public override bool Matches(PropertyBlend blend)
	{
		return blend.GetOutputTypeDesc() == "Quaternion";
	}
}
