using System;
using UnityEngine;

namespace UnityTest;

public class ColliderComparer : ComparerBaseGeneric<Bounds>
{
	public enum CompareType
	{
		Intersects,
		DoesNotIntersect
	}

	public CompareType compareType;

	protected override bool Compare(Bounds a, Bounds b)
	{
		return compareType switch
		{
			CompareType.Intersects => a.Intersects(b), 
			CompareType.DoesNotIntersect => !a.Intersects(b), 
			_ => throw new Exception(), 
		};
	}
}
