using Brix.Util.Logging;
using UnityEngine;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace Brix.Utils.Logging;

internal class FallbackBrixLoggerConfigurator
{
	public static void ConfigureLog4Net()
	{
		InitializeLog4Net();
		ConfigureAppenders();
	}

	private static void InitializeLog4Net()
	{
		Debug.Log("Initializing Log4Net");
		Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
		hierarchy.Shutdown();
		hierarchy.ResetConfiguration();
		hierarchy.Root.Level = Level.Warn;
	}

	private static void ConfigureAppenders()
	{
		PatternLayout patternLayout = new PatternLayout();
		patternLayout.ConversionPattern = "%-5level %logger - %message%n";
		patternLayout.ActivateOptions();
		UnityLogAppender unityLogAppender = new UnityLogAppender();
		unityLogAppender.Threshold = Level.Error;
		unityLogAppender.Layout = patternLayout;
		unityLogAppender.ActivateOptions();
		Platform platform = PlatformResolver.GetPlatform();
		if (platform == Platform.WebPlayer)
		{
			Debug.Log("Detected Web-Player, only using appended logger");
			BasicConfigurator.Configure(unityLogAppender);
			return;
		}
		PatternLayout patternLayout2 = new PatternLayout();
		patternLayout2.ConversionPattern = "%date [%thread] %-5level %logger - %message%n";
		patternLayout2.ActivateOptions();
		RollingFileAppender rollingFileAppender = new RollingFileAppender();
		rollingFileAppender.AppendToFile = true;
		rollingFileAppender.MaxFileSize = 2097152L;
		rollingFileAppender.MaxSizeRollBackups = 10;
		rollingFileAppender.RollingStyle = RollingFileAppender.RollingMode.Composite;
		rollingFileAppender.Threshold = Level.Warn;
		rollingFileAppender.File = BrixLoggerConfigurator.GetLogDirectory(platform) + "/unity.log";
		rollingFileAppender.Layout = patternLayout2;
		rollingFileAppender.PreserveLogFileNameExtension = true;
		rollingFileAppender.ImmediateFlush = true;
		rollingFileAppender.ActivateOptions();
		Debug.Log(string.Concat("Detected ", platform, ", using file Logging: ", rollingFileAppender.File));
		BasicConfigurator.Configure(rollingFileAppender, unityLogAppender);
	}
}
