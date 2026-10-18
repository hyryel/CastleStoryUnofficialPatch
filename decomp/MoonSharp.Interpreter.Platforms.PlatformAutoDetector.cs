using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MoonSharp.Interpreter.Interop;
using MoonSharp.Interpreter.Loaders;

namespace MoonSharp.Interpreter.Platforms;

public static class PlatformAutoDetector
{
	private static bool? m_IsRunningOnAOT;

	private static bool m_AutoDetectionsDone;

	public static bool IsRunningOnMono { get; private set; }

	public static bool IsRunningOnClr4 { get; private set; }

	public static bool IsRunningOnUnity { get; private set; }

	public static bool IsPortableFramework { get; private set; }

	public static bool IsUnityNative { get; private set; }

	public static bool IsUnityIL2CPP { get; private set; }

	public static bool IsRunningOnAOT
	{
		get
		{
			if (!m_IsRunningOnAOT.HasValue)
			{
				try
				{
					((Expression<Func<int>>)(() => 5)).Compile();
					m_IsRunningOnAOT = false;
				}
				catch (Exception)
				{
					m_IsRunningOnAOT = true;
				}
			}
			return m_IsRunningOnAOT.Value;
		}
	}

	private static void AutoDetectPlatformFlags()
	{
		if (!m_AutoDetectionsDone)
		{
			IsRunningOnUnity = (byte)(0u & (AppDomain.CurrentDomain.GetAssemblies().SelectMany((Assembly a) => a.SafeGetTypes()).Any((Type t) => t.FullName.StartsWith("UnityEngine.")) ? 1u : 0u)) != 0;
			IsRunningOnMono = Type.GetType("Mono.Runtime") != null;
			IsRunningOnClr4 = Type.GetType("System.Lazy`1") != null;
			m_AutoDetectionsDone = true;
		}
	}

	internal static IPlatformAccessor GetDefaultPlatform()
	{
		AutoDetectPlatformFlags();
		if (IsRunningOnUnity)
		{
			return new LimitedPlatformAccessor();
		}
		return new StandardPlatformAccessor();
	}

	internal static IScriptLoader GetDefaultScriptLoader()
	{
		AutoDetectPlatformFlags();
		_ = IsRunningOnUnity;
		return new FileSystemScriptLoader();
	}
}
You are not using the latest version of the tool, please update.
Latest version is '11.1.0.9782' (yours is '9.1.0.7988')
