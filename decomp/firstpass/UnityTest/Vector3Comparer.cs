using System;
using UnityEngine;

namespace UnityTest;

public class Vector3Comparer : VectorComparerBase<Vector3>
{
	public enum CompareType
	{
		MagnitudeEquals,
		MagnitudeNotEquals
	}

	public CompareType compareType;

	public double floatingPointError = 9.999999747378752E-05;

	protected override bool Compare(Vector3 a, Vector3 b)
	{
		return compareType switch
		{
			CompareType.MagnitudeEquals => AreVectorMagnitudeEqual(a.magnitude, b.magnitude, floatingPointError), 
			CompareType.MagnitudeNotEquals => !AreVectorMagnitudeEqual(a.magnitude, b.magnitude, floatingPointError), 
			_ => throw new Exception(), 
		};
	}
}
