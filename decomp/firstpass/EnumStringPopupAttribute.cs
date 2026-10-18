using System;
using System.Collections.Generic;
using UnityEngine;

public class EnumStringPopupAttribute : PropertyAttribute
{
	public Type enumType;

	public Dictionary<int, string> stringMap;

	public EnumStringPopupAttribute(Type mapClassType)
	{
	}
}
