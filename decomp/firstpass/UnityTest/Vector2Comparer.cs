using System;
using UnityEngine;

namespace UnityTest;

public class Vector2Comparer : VectorComparerBase<Vector2>
{
	public enum CompareType
	{
		MagnitudeEquals,
		MagnitudeNotEquals
	}

	public CompareType compareType;

	public float floatingPointError = 0.0001f;

	protected override bool Compare(Vector2 a, Vector2 b)
	{
		return compareType switch
		{
			CompareType.MagnitudeEquals => AreVectorMagnitudeEqual(a.magnitude, b.magnitude, floatingPointError), 
			CompareType.MagnitudeNotEquals => !AreVectorMagnitudeEqual(a.magnitude, b.magnitude, floatingPointError), 
			_ => throw new Exception(), 
		};
	}

	public override int GetDepthOfSearch()
	{
		return 3;
	}
}
