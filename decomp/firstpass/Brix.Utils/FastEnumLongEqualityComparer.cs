using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Brix.Utils;

public class FastEnumLongEqualityComparer<TEnum> : IEqualityComparer<TEnum> where TEnum : struct
{
	private static class BoxAvoidance
	{
		public static readonly Func<TEnum, long> ToLong;

		static BoxAvoidance()
		{
			ParameterExpression parameterExpression = Expression.Parameter(typeof(TEnum), null);
			UnaryExpression body = Expression.ConvertChecked(parameterExpression, typeof(long));
			ToLong = Expression.Lambda<Func<TEnum, long>>(body, new ParameterExpression[1] { parameterExpression }).Compile();
		}
	}

	public static FastEnumLongEqualityComparer<TEnum> Instance { get; private set; }

	private FastEnumLongEqualityComparer()
	{
	}

	static FastEnumLongEqualityComparer()
	{
		Instance = new FastEnumLongEqualityComparer<TEnum>();
	}

	public bool Equals(TEnum firstEnum, TEnum secondEnum)
	{
		return BoxAvoidance.ToLong(firstEnum) == BoxAvoidance.ToLong(secondEnum);
	}

	public int GetHashCode(TEnum firstEnum)
	{
		return BoxAvoidance.ToLong(firstEnum).GetHashCode();
	}
}
