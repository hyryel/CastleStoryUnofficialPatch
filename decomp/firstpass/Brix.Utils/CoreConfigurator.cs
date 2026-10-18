using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Brix.Util.Logging;
using Brix.Utils.Logging;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Utils;

public class CoreConfigurator : MonoBehaviour
{
	private static BrixLogger logger = new BrixLogger(typeof(CoreConfigurator).FullName);

	private static List<string> _usualAssemblyNames = new List<string> { "Assembly-CSharp-firstpass, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", "Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", "Brix.Core, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", "Brix.Common, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null" };

	public static bool IsCurrentThreadUnity => Threading.IsOnMainThread;

	private void Awake()
	{
		Threading.MainThread = Thread.CurrentThread;
		UnityLogDuplicator.SubscribeToUnityLogs();
		Thread thread = new Thread((ThreadStart)delegate
		{
			ForceAllBrixLoggersToInitiate();
		});
		thread.Start();
	}

	public static void ForceAllBrixLoggersToInitiate()
	{
		foreach (string usualAssemblyName in _usualAssemblyNames)
		{
			Assembly assembly = Assembly.Load(usualAssemblyName);
			if (assembly == null)
			{
				continue;
			}
			Type[] types = assembly.GetTypes();
			foreach (Type type in types)
			{
				FieldInfo fieldInfo = (from fi in type.GetFields().Slinq()
					where fi.FieldType == typeof(BrixLogger)
					select fi).FirstOrDefault();
				if (fieldInfo != null)
				{
					if (!fieldInfo.IsStatic)
					{
						logger.WarnFormat("Class/Struct '{0}' has a non-static BrixLogger with field '{1}'. Considering their instantiation cost, they are recommended in a non-static usage.", type, fieldInfo.Name);
					}
					RuntimeHelpers.RunClassConstructor(type.TypeHandle);
				}
			}
		}
	}
}
