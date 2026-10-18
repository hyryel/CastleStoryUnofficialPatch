using System;

[Serializable]
public abstract class Prop
{
	public string name;

	public Prop(string name)
	{
		this.name = name;
	}

	public abstract bool Matches(PropertyBlend blend);
}
