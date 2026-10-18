using System;

namespace UnityTest;

public class IntComparer : ComparerBaseGeneric<int>
{
	public enum CompareType
	{
		Equal,
		NotEqual,
		Greater,
		GreaterOrEqual,
		Less,
		LessOrEqual
	}

	public CompareType compareType;

	protected override bool Compare(int a, int b)
	{
		return compareType switch
		{
			CompareType.Equal => a == b, 
			CompareType.NotEqual => a != b, 
			CompareType.Greater => a > b, 
			CompareType.GreaterOrEqual => a >= b, 
			CompareType.Less => a < b, 
			CompareType.LessOrEqual => a <= b, 
			_ => throw new Exception(), 
		};
	}
}
