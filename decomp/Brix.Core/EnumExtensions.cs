using System;
using System.Linq.Expressions;

public static class EnumExtensions
{
	private static class EnumExtensionsInternal<TEnum> where TEnum : struct, IComparable, IConvertible, IFormattable
	{
		public static readonly Func<TEnum, TEnum, bool> HasFlagsDelegate = CreateHasFlagDelegate();

		private static Func<TEnum, TEnum, bool> CreateHasFlagDelegate()
		{
			if (!typeof(TEnum).IsEnum)
			{
				throw new ArgumentException($"{typeof(TEnum)} is not an Enum", typeof(EnumExtensionsInternal<>).GetGenericArguments()[0].Name);
			}
			ParameterExpression parameterExpression = Expression.Parameter(typeof(TEnum), typeof(TEnum).Name);
			ParameterExpression parameterExpression2 = Expression.Parameter(typeof(TEnum), typeof(TEnum).Name);
			Type type = ((Type.GetTypeCode(typeof(TEnum)) == TypeCode.UInt64) ? typeof(ulong) : typeof(long));
			Expression<Func<TEnum, TEnum, bool>> expression = Expression.Lambda<Func<TEnum, TEnum, bool>>(Expression.Equal(Expression.And(Expression.Convert(parameterExpression, type), Expression.Convert(parameterExpression2, type)), Expression.Convert(parameterExpression2, type)), new ParameterExpression[2] { parameterExpression, parameterExpression2 });
			return expression.Compile();
		}
	}

	public static bool HasFlag<TEnum>(this TEnum value, TEnum flag) where TEnum : struct, IComparable, IConvertible, IFormattable
	{
		return EnumExtensionsInternal<TEnum>.HasFlagsDelegate(value, flag);
	}
}
