using System;
using System.Linq.Expressions;

public class Operator<T>
{
	private delegate BinaryExpression FOp(ParameterExpression paramA, ParameterExpression paramB);

	private static T Op(T a, T b, FOp op)
	{
		ParameterExpression parameterExpression = Expression.Parameter(typeof(T), "a");
		ParameterExpression parameterExpression2 = Expression.Parameter(typeof(T), "b");
		BinaryExpression body = op(parameterExpression, parameterExpression2);
		Func<T, T, T> func = Expression.Lambda<Func<T, T, T>>(body, new ParameterExpression[2] { parameterExpression, parameterExpression2 }).Compile();
		return func(a, b);
	}

	public static T Add(T a, T b)
	{
		return Op(a, b, Expression.Add);
	}

	public static T Subtract(T a, T b)
	{
		return Op(a, b, Expression.Subtract);
	}

	public static T Multiply(T a, T b)
	{
		return Op(a, b, Expression.Multiply);
	}

	public static T Divide(T a, T b)
	{
		return Op(a, b, Expression.Divide);
	}
}
