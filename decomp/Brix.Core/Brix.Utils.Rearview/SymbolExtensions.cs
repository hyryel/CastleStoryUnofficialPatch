using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Brix.Utils.Rearview;

public static class SymbolExtensions
{
	public static MethodInfo GetMethodInfo(Expression<Action> expression)
	{
		return GetMethodInfo((LambdaExpression)expression);
	}

	public static MethodInfo GetMethodInfo<T>(Expression<Action<T>> expression)
	{
		return GetMethodInfo((LambdaExpression)expression);
	}

	public static MethodInfo GetMethodInfo<T, TResult>(Expression<Func<T, TResult>> expression)
	{
		return GetMethodInfo((LambdaExpression)expression);
	}

	public static MethodInfo GetMethodInfo(LambdaExpression expression)
	{
		if (!(expression.Body is MethodCallExpression methodCallExpression))
		{
			throw new ArgumentException("Invalid Expression. Expression should consist of a Method call only.");
		}
		return methodCallExpression.Method;
	}
}
