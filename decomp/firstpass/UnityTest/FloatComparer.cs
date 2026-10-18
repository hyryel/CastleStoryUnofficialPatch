using System;

namespace UnityTest;

public class FloatComparer : ComparerBaseGeneric<float>
{
	public enum CompareTypes
	{
		Equal,
		NotEqual,
		Greater,
		Less
	}

	public CompareTypes compareTypes;

	public double floatingPointError = 9.999999747378752E-05;

	protected override bool Compare(float a, float b)
	{
		return compareTypes switch
		{
			CompareTypes.Equal => (double)Math.Abs(a - b) < floatingPointError, 
			CompareTypes.NotEqual => (double)Math.Abs(a - b) > floatingPointError, 
			CompareTypes.Greater => a > b, 
			CompareTypes.Less => a < b, 
			_ => throw new Exception(), 
		};
	}

	public override int GetDepthOfSearch()
	{
		return 3;
	}
}
