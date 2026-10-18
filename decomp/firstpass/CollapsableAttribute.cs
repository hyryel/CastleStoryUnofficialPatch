using System;

public class CollapsableAttribute : Attribute
{
	public bool flag;

	public CollapsableAttribute()
	{
	}

	public CollapsableAttribute(bool initFlag)
	{
		flag = initFlag;
	}
}
