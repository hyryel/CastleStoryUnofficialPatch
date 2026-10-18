using System;
using System.Reflection;
using Newtonsoft.Json;

namespace Brix.Transactions;

[JsonObject(MemberSerialization.Fields)]
public class SimpleMethodWrapper
{
	public MethodInfo methodInfo;

	public SimpleMethodWrapper(Delegate d)
	{
		methodInfo = d.Method;
	}

	[JsonConstructor]
	public SimpleMethodWrapper(MethodInfo d)
	{
		methodInfo = d;
	}

	public object Invoke(object methodTarget, params object[] methodArgs)
	{
		return methodInfo.Invoke(methodTarget, methodArgs);
	}
}
