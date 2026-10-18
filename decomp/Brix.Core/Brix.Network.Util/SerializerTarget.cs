using System;
using System.Globalization;
using System.Linq;

namespace Brix.Network.Util;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Interface, AllowMultiple = true)]
public sealed class SerializerTarget : Attribute
{
	public string[] Targets { get; private set; }

	public SerializerTarget(params string[] targets)
	{
		Targets = targets;
	}

	public bool IsTargetting(string target)
	{
		return Targets.Contains(target);
	}

	public static JsonFactory CreateJsonFactoryInstance(Type factoryType, string domainName)
	{
		try
		{
			return (JsonFactory)Activator.CreateInstance(factoryType, domainName);
		}
		catch (Exception innerException)
		{
			throw new Exception(string.Format("Error creating {0}", CultureInfo.InvariantCulture, factoryType), innerException);
		}
	}
}
