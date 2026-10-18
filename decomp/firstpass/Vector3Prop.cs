using System;
using UnityEngine;

[Serializable]
public class Vector3Prop : Prop
{
	public delegate Vector3 Get();

	public delegate void Set(Vector3 value);

	public Get gets;

	public Set sets;

	public Vector3Prop(string name, Get get, Set set)
		: base(name)
	{
		gets = (Get)Delegate.Combine(gets, get);
		sets = (Set)Delegate.Combine(sets, set);
	}

	public override bool Matches(PropertyBlend blend)
	{
		return blend.GetOutputTypeDesc() == "Vector3";
	}
}
