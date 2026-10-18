using System;
using System.Reflection;

namespace Brix.Compile;

public class AbstractContext
{
	private Assembly[] assemblies = null;

	private string[] namespaces = null;

	private string[] defines = null;

	public Assembly[] Assemblies
	{
		get
		{
			if (assemblies == null)
			{
				assemblies = AppDomain.CurrentDomain.GetAssemblies();
			}
			return assemblies;
		}
		set
		{
			assemblies = value;
		}
	}

	public string[] Namespaces
	{
		get
		{
			if (namespaces == null)
			{
				namespaces = new string[5] { "UnityEngine", "System.Collections", "System.Linq", "System", "System.Reflection" };
			}
			return namespaces;
		}
		set
		{
			namespaces = value;
		}
	}

	public string[] Defines
	{
		get
		{
			if (defines == null)
			{
				defines = new string[0];
			}
			return defines;
		}
		set
		{
			defines = value;
		}
	}

	public virtual void Copy(AbstractContext other)
	{
		assemblies = other.assemblies;
		namespaces = other.namespaces;
		defines = other.defines;
	}
}
