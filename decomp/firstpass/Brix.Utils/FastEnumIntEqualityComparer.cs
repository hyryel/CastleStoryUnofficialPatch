using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Brix.Utils;

public class FastEnumIntEqualityComparer<TEnum> : IEqualityComparer<TEnum> where TEnum : struct
{
	private static class BoxAvoidance
	{
		public static readonly Func<TEnum, int> ToInt;

		static BoxAvoidance()
		{
			ParameterExpression parameterExpression = Expression.Parameter(typeof(TEnum), null);
			UnaryExpression body = Expression.ConvertChecked(parameterExpression, typeof(int));
			ToInt = Expression.Lambda<Func<TEnum, int>>(body, new ParameterExpression[1] { parameterExpression }).Compile();
		}
	}

	public static FastEnumIntEqualityComparer<TEnum> Instance { get; private set; }

	private FastEnumIntEqualityComparer()
	{
	}

	static FastEnumIntEqualityComparer()
	{
		Instance = new FastEnumIntEqualityComparer<TEnum>();
	}

	public bool Equals(TEnum firstEnum, TEnum secondEnum)
	{
		return BoxAvoidance.ToInt(firstEnum) == BoxAvoidance.ToInt(secondEnum);
	}

	public int GetHashCode(TEnum firstEnum)
	{
		return BoxAvoidance.ToInt(firstEnum);
	}
}
