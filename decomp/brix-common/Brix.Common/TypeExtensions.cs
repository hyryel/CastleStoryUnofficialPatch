using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Brix.Common;

public static class TypeExtensions
{
	private class TypeToIgnore
	{
	}

	private static class InstanceCreationFactory<TArg1, TArg2, TArg3>
	{
		private static readonly Dictionary<Type, Func<TArg1, TArg2, TArg3, object>> _instanceCreationMethods = new Dictionary<Type, Func<TArg1, TArg2, TArg3, object>>();

		public static T CreateInstanceOf<T>(TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			return (T)CreateInstanceOf(typeof(T), arg1, arg2, arg3);
		}

		public static object CreateInstanceOf(Type type, TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			CacheInstanceCreationMethodIfRequired(type);
			return _instanceCreationMethods[type](arg1, arg2, arg3);
		}

		private static void CacheInstanceCreationMethodIfRequired(Type type)
		{
			if (!_instanceCreationMethods.ContainsKey(type))
			{
				Type[] source = new Type[3]
				{
					typeof(TArg1),
					typeof(TArg2),
					typeof(TArg3)
				};
				Type[] array = source.Where((Type t) => t != typeof(TypeToIgnore)).ToArray();
				ConstructorInfo constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, CallingConventions.HasThis, array, new ParameterModifier[0]);
				ParameterExpression[] array2 = new ParameterExpression[3]
				{
					Expression.Parameter(typeof(TArg1), "param1"),
					Expression.Parameter(typeof(TArg2), "param2"),
					Expression.Parameter(typeof(TArg3), "param3")
				};
				ParameterExpression[] arguments = array2.Take(array.Length).ToArray();
				NewExpression body = Expression.New(constructor, arguments);
				Func<TArg1, TArg2, TArg3, object> value = Expression.Lambda<Func<TArg1, TArg2, TArg3, object>>(body, array2).Compile();
				_instanceCreationMethods[type] = value;
			}
		}
	}

	public static object MakeInstance(this Type type)
	{
		return InstanceCreationFactory<TypeToIgnore, TypeToIgnore, TypeToIgnore>.CreateInstanceOf(type, null, null, null);
	}

	public static object MakeInstance<TArg>(this Type type, TArg argument)
	{
		return InstanceCreationFactory<TArg, TypeToIgnore, TypeToIgnore>.CreateInstanceOf(type, argument, null, null);
	}

	public static object MakeInstance<TArg1, TArg2>(this Type type, TArg1 argument1, TArg2 argument2)
	{
		return InstanceCreationFactory<TArg1, TArg2, TypeToIgnore>.CreateInstanceOf(type, argument1, argument2, null);
	}

	public static object MakeInstance<TArg1, TArg2, TArg3>(this Type type, TArg1 argument1, TArg2 argument2, TArg3 argument3)
	{
		return InstanceCreationFactory<TArg1, TArg2, TArg3>.CreateInstanceOf(type, argument1, argument2, argument3);
	}

	public static T MakeInstance<T>()
	{
		return InstanceCreationFactory<TypeToIgnore, TypeToIgnore, TypeToIgnore>.CreateInstanceOf<T>(null, null, null);
	}

	public static T MakeInstance<T, TArg>(TArg argument)
	{
		return InstanceCreationFactory<TArg, TypeToIgnore, TypeToIgnore>.CreateInstanceOf<T>(argument, null, null);
	}

	public static T MakeInstance<T, TArg1, TArg2>(TArg1 argument1, TArg2 argument2)
	{
		return InstanceCreationFactory<TArg1, TArg2, TypeToIgnore>.CreateInstanceOf<T>(argument1, argument2, null);
	}

	public static T MakeInstance<T, TArg1, TArg2, TArg3>(TArg1 argument1, TArg2 argument2, TArg3 argument3)
	{
		return InstanceCreationFactory<TArg1, TArg2, TArg3>.CreateInstanceOf<T>(argument1, argument2, argument3);
	}
}
