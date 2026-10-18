using System;
using UnityEngine;

namespace UnityTest;

public class Vector4Comparer : VectorComparerBase<Vector4>
{
	public enum CompareType
	{
		MagnitudeEquals,
		MagnitudeNotEquals
	}

	public CompareType compareType;

	public double floatingPointError;

	protected override bool Compare(Vector4 a, Vector4 b)
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
