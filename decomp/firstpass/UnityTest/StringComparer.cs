using System;

namespace UnityTest;

public class StringComparer : ComparerBaseGeneric<string>
{
	public enum CompareType
	{
		Equal,
		NotEqual,
		Shorter,
		Longer
	}

	public CompareType compareType;

	public StringComparison comparisonType = StringComparison.Ordinal;

	public bool ignoreCase;

	protected override bool Compare(string a, string b)
	{
		if (ignoreCase)
		{
			a = a.ToLower();
			b = b.ToLower();
		}
		return compareType switch
		{
			CompareType.Equal => string.Compare(a, b, comparisonType) == 0, 
			CompareType.NotEqual => string.Compare(a, b, comparisonType) != 0, 
			CompareType.Longer => string.Compare(a, b, comparisonType) > 0, 
			CompareType.Shorter => string.Compare(a, b, comparisonType) < 0, 
			_ => throw new Exception(), 
		};
	}
}
