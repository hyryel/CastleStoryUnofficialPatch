using System;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Brix.Utils;
using UnityEngine;
using log4net.Appender;
using log4net.Core;

namespace Brix.Util.Logging;

public class UnityLogAppender : AppenderSkeleton
{
	private struct LogEntry
	{
		public string level;

		public string message;

		public string filepath;

		public int lineNumber;

		public int column;
	}

	private static Action<string, string, int, int> LogPlayerBuildError;

	private static Action<LogType, string, UnityEngine.Object> Internal_Log;

	private bool _disabled = false;

	public bool AppendingEnabled => !_disabled;

	public UnityLogAppender()
	{
		if (Internal_Log == null)
		{
			BindDefaultLogger();
		}
		if (LogPlayerBuildError == null)
		{
			BindErrorLog();
		}
		UnityLogDuplicator.SetMonitoredUnityLogAppender(this);
	}

	private static void BindDefaultLogger()
	{
		Type type = RearViewMirror.GetType("DebugLogHandler");
		MethodInfo method = type.GetMethod("Internal_Log", BindingFlags.Static | BindingFlags.NonPublic);
		Internal_Log = Delegate.CreateDelegate(typeof(Action<LogType, string, UnityEngine.Object>), method) as Action<LogType, string, UnityEngine.Object>;
	}

	private void BindErrorLog()
	{
		MethodInfo method = typeof(Debug).GetMethod("LogPlayerBuildError", BindingFlags.Static | BindingFlags.NonPublic);
		if (method != null)
		{
			LogPlayerBuildError = Delegate.CreateDelegate(typeof(Action<string, string, int, int>), method) as Action<string, string, int, int>;
			return;
		}
		LogPlayerBuildError = delegate(string message, string filepath, int lineNumber, int column)
		{
			Internal_Log(LogType.Error, message + "\n\n\n\n\n\n\n", null);
		};
	}

	public void DisableAppending()
	{
		_disabled = true;
	}

	public void EnableAppending()
	{
		_disabled = false;
	}

	protected override void Append(LoggingEvent loggingEvent)
	{
		if (_disabled)
		{
			return;
		}
		try
		{
			LogEntry logentry = GenerateLogEntry(RenderLoggingEvent(loggingEvent), StackTraceUtility.ExtractStackTrace(), loggingEvent.Level.Name);
			PrintToUnityConsole(logentry);
		}
		catch (Exception message)
		{
			Debug.LogError(message);
		}
	}

	private LogEntry GenerateLogEntry(string message, string stacktrace, string level)
	{
		LogEntry result = default(LogEntry);
		StringBuilder stringBuilder = new StringBuilder(message).Append("\n");
		string[] array = stacktrace.Split('\n');
		int i;
		for (i = 0; i <= array.Length; i++)
		{
			Match match = Regex.Match(array[i], "\\(at\\s([^\\)]+)\\:([\\d]+)\\)");
			string value = match.Groups[0].Value;
			if ((!match.Success || !value.Contains("log4net\\")) && !value.Contains("UnityLogAppender.cs") && !value.Contains("Logger.cs"))
			{
				result.filepath = match.Groups[1].Value;
				int.TryParse(match.Groups[2].Value, out result.lineNumber);
				break;
			}
		}
		for (; i < array.Length; i++)
		{
			stringBuilder.Append(array[i]).Append("\n");
		}
		result.message = stringBuilder.ToString();
		result.level = level;
		return result;
	}

	private static void PrintToUnityConsole(LogEntry logentry)
	{
		switch (logentry.level)
		{
		case "INFO":
		case "DEBUG":
			Internal_Log(LogType.Log, logentry.message + "\n\n\n\n\n\n\n", null);
			break;
		case "WARN":
			Internal_Log(LogType.Warning, logentry.message + "\n\n\n\n\n\n\n", null);
			break;
		case "ERROR":
		case "FATAL":
			if (Threading.IsOnMainThread)
			{
				LogPlayerBuildError(logentry.message, logentry.filepath, logentry.lineNumber, logentry.column);
			}
			else
			{
				Internal_Log(LogType.Error, logentry.message + "\n\n\n\n\n\n\n", null);
			}
			break;
		}
	}
}
