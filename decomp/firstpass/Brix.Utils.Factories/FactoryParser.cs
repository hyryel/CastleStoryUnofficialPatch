using System;
using System.Collections.Generic;
using System.Reflection;
using Smooth.Slinq;

namespace Brix.Utils.Factories;

public static class FactoryParser
{
	private static List<MethodInfo> InvocationList;

	static FactoryParser()
	{
		InvocationList = new List<MethodInfo>();
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			Type[] types = assembly.GetTypes();
			foreach (Type type in types)
			{
				MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (MethodInfo methodInfo in methods)
				{
					object[] customAttributes = methodInfo.GetCustomAttributes(typeof(OnFactoriesLoaded), inherit: true);
					if (customAttributes.Length > 0)
					{
						InvocationList.Add(methodInfo);
					}
				}
			}
		}
	}

	public static void OnFactoriesLoaded()
	{
		InvocationList.Slinq().ForEach(delegate(MethodInfo mi)
		{
			mi.Invoke(null, null);
		});
	}
}
