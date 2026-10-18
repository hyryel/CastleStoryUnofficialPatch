using System;

public class DividerAttribute : Attribute
{
	public float size;

	public DividerAttribute()
	{
	}

	public DividerAttribute(float size)
	{
		this.size = size;
	}
}
