using System;
using System.IO;
using System.Linq;
using System.Threading;
using Brix.Util.Logging;
using UnityEngine;
using log4net;
using log4net.Config;
using log4net.Core;
using log4net.Repository;
using log4net.Repository.Hierarchy;

namespace Brix.Utils.Logging;

public static class BrixLoggerConfigurator
{
	private static string ApplicationName = "Castle Story";

	public static void Configure()
	{
		ConfigureEnvironment();
		ConfigureLog4Net();
	}

	public static string GetLogDirectory()
	{
		return GetLogDirectory(PlatformResolver.GetPlatform());
	}

	public static void StopLoggers()
	{
		LogManager.GetRepository().ResetConfiguration();
		UnityLogDuplicator.Clear();
	}

	public static void RestartLoggers()
	{
		StopLoggers();
		ConfigureLog4Net();
	}

	public static void SetAllLoggersToDebug()
	{
		ILoggerRepository[] allRepositories = LogManager.GetAllRepositories();
		ILoggerRepository[] array = allRepositories;
		foreach (ILoggerRepository loggerRepository in array)
		{
			loggerRepository.Threshold = loggerRepository.LevelMap["DEBUG"];
			Hierarchy hierarchy = (Hierarchy)loggerRepository;
			log4net.Core.ILogger[] currentLoggers = hierarchy.GetCurrentLoggers();
			log4net.Core.ILogger[] array2 = currentLoggers;
			foreach (log4net.Core.ILogger logger in array2)
			{
				((log4net.Repository.Hierarchy.Logger)logger).Level = hierarchy.LevelMap["DEBUG"];
			}
		}
		Hierarchy hierarchy2 = (Hierarchy)LogManager.GetRepository();
		log4net.Repository.Hierarchy.Logger root = hierarchy2.Root;
		root.Level = hierarchy2.LevelMap["DEBUG"];
	}

	public static void DisableUnityAppender()
	{
		if (LogManager.GetRepository() is Hierarchy hierarchy)
		{
			hierarchy.GetAppenders().OfType<UnityLogAppender>().FirstOrDefault()?.DisableAppending();
		}
	}

	public static void EnableUnityAppender()
	{
		if (LogManager.GetRepository() is Hierarchy hierarchy)
		{
			hierarchy.GetAppenders().OfType<UnityLogAppender>().FirstOrDefault()?.EnableAppending();
		}
	}

	internal static string GetLogDirectory(Platform type)
	{
		string text = Environment.GetEnvironmentVariable("HOME");
		if (string.IsNullOrEmpty(text))
		{
			text = Environment.GetEnvironmentVariable("TMP");
			if (string.IsNullOrEmpty(text))
			{
				text = ".";
			}
		}
		if (string.IsNullOrEmpty(ApplicationName))
		{
			ApplicationName = "Unity3D-Project";
		}
		return type switch
		{
			Platform.MacOS => text + "/Library/logs/" + ApplicationName, 
			Platform.WindowsXP => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Temp/" + ApplicationName + "/logs", 
			Platform.WindowsVista7 => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Temp/" + ApplicationName + "/logs", 
			_ => "/tmp/" + ApplicationName + "/logs", 
		};
	}

	private static bool ConfigureFromFile(string path = "Info/logging.xml")
	{
		try
		{
			FileInfo fileInfo = new FileInfo(path);
			if (!fileInfo.Exists)
			{
				Debug.LogWarning("Could not find logging XML configuration file.");
				return false;
			}
			XmlConfigurator.Configure(fileInfo);
			Debug.Log("Logging configured using file at '" + path + "'");
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to configure logging from file 'Info/logging.xml'. See inner exception for more details: " + ex.Message);
			return false;
		}
	}

	private static void ConfigureLog4Net()
	{
		if (!ConfigureFromFile())
		{
			FallbackBrixLoggerConfigurator.ConfigureLog4Net();
		}
	}

	private static void ConfigureEnvironment()
	{
		Platform platform = PlatformResolver.GetPlatform();
		Environment.SetEnvironmentVariable("LOGDIR", GetLogDirectory(platform));
		if (Thread.CurrentThread.Name == null)
		{
			Thread.CurrentThread.Name = "main";
		}
	}

	private static void PrintSystemInfo(ILog log)
	{
		if (Threading.IsOnMainThread)
		{
			log.Info("**********************************************************************************");
			log.Info("Logging system started");
			log.Info("Operating-System:  " + SystemInfo.operatingSystem);
			log.Info("     System Spec:  " + SystemInfo.processorType + " - #" + SystemInfo.processorCount + " (" + SystemInfo.systemMemorySize + ")");
			log.Info("     Device Spec:  " + SystemInfo.deviceName + " " + SystemInfo.deviceModel + " [" + SystemInfo.deviceUniqueIdentifier + "]");
			log.Info("     Graphic Spec: " + SystemInfo.graphicsDeviceID + ":" + SystemInfo.graphicsDeviceName + " - " + SystemInfo.graphicsDeviceVendorID + ":" + SystemInfo.graphicsDeviceVendor + " (" + SystemInfo.graphicsMemorySize + "MB, Shader Level " + SystemInfo.graphicsShaderLevel + ")");
		}
	}
}
