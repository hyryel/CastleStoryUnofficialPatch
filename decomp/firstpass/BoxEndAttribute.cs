using System;

public class BoxEndAttribute : Attribute
{
	public enum Order
	{
		After,
		Before
	}

	public enum Target
	{
		Current,
		All
	}

	public Order order;

	public Target target;

	public BoxEndAttribute()
	{
	}

	public BoxEndAttribute(Order order)
	{
		target = Target.Current;
		this.order = order;
	}

	public BoxEndAttribute(Order order, Target target)
	{
		this.target = target;
		this.order = order;
	}
}
