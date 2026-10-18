using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.Util.Logging;

public static class UnityLogDuplicator
{
	private static BrixLogger logger = new BrixLogger("UnityEngine");

	private static UnityLogAppender _unityLogAppender;

	private static bool _subscribed;

	public static void SubscribeToUnityLogs()
	{
		if (!_subscribed)
		{
			Application.logMessageReceivedThreaded += HandleLog;
			_subscribed = true;
		}
	}

	public static void UnsubscribeToUnityLogs()
	{
		if (_subscribed)
		{
			_subscribed = false;
			Application.logMessageReceivedThreaded -= HandleLog;
		}
	}

	public static void Clear()
	{
		_unityLogAppender = null;
	}

	public static void SetMonitoredUnityLogAppender(UnityLogAppender appender)
	{
		_unityLogAppender = appender;
	}

	private static void HandleLog(string condition, string stacktrace, LogType type)
	{
		if (_unityLogAppender == null || !_unityLogAppender.AppendingEnabled)
		{
			switch (type)
			{
			case LogType.Exception:
				logger.ErrorFormat("{0}\n{1}", condition, stacktrace);
				break;
			case LogType.Error:
				logger.ErrorFormat("{0}\n{1}", condition, stacktrace);
				break;
			case LogType.Assert:
				logger.FatalFormat("{0}\n{1}", condition, stacktrace);
				break;
			case LogType.Warning:
				logger.Warn(condition);
				break;
			case LogType.Log:
				logger.Info(condition);
				break;
			}
		}
	}
}
