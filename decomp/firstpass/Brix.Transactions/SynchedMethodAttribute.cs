using System;
using System.Collections.Generic;
using System.Reflection;

namespace Brix.Transactions;

public class SynchedMethodAttribute : Attribute
{
	public class MethodInfoComparer : EqualityComparer<MethodInfo>
	{
		public override bool Equals(MethodInfo left, MethodInfo right)
		{
			if (left.Equals(right))
			{
				return true;
			}
			if (left.GetHashCode() != right.GetHashCode())
			{
				return false;
			}
			if (left.DeclaringType != right.DeclaringType)
			{
				return false;
			}
			ParameterInfo[] parameters = left.GetParameters();
			ParameterInfo[] parameters2 = right.GetParameters();
			if (parameters.Length != parameters2.Length)
			{
				return false;
			}
			for (int i = 0; i < parameters.Length; i++)
			{
				if (parameters[i].ParameterType != parameters2[i].ParameterType)
				{
					return false;
				}
			}
			if (left.ReturnType != right.ReturnType)
			{
				return false;
			}
			return true;
		}

		public override int GetHashCode(MethodInfo t)
		{
			return t.GetHashCode();
		}
	}

	public static Dictionary<MethodInfo, Guid> MethodToKey = new Dictionary<MethodInfo, Guid>(new MethodInfoComparer());

	public static Dictionary<Guid, SimpleMethodWrapper> KeyToMethod = new Dictionary<Guid, SimpleMethodWrapper>();

	public static void Register(Guid methodId, MethodInfo methodInfo)
	{
		KeyToMethod[methodId] = new SimpleMethodWrapper(methodInfo);
		MethodToKey[methodInfo] = methodId;
	}
}
