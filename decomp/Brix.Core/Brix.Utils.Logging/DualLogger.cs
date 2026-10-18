using System;
using log4net;
using log4net.Core;

namespace Brix.Utils.Logging;

public class DualLogger : ILog, ILoggerWrapper
{
	private ILog log1;

	private ILog log2;

	public ILogger Logger => log1.Logger;

	public bool IsDebugEnabled => log1.IsDebugEnabled || log2.IsDebugEnabled;

	public bool IsInfoEnabled => log1.IsInfoEnabled || log2.IsInfoEnabled;

	public bool IsWarnEnabled => log1.IsWarnEnabled || log2.IsWarnEnabled;

	public bool IsErrorEnabled => log1.IsErrorEnabled || log2.IsErrorEnabled;

	public bool IsFatalEnabled => log1.IsFatalEnabled || log2.IsFatalEnabled;

	public DualLogger(ILog log1, ILog log2)
	{
		this.log1 = log1;
		this.log2 = log2;
	}

	public void Debug(object message)
	{
		log1.Debug(message);
		log2.Debug(message);
	}

	public void Debug(object message, Exception exception)
	{
		log1.Debug(message, exception);
		log2.Debug(message, exception);
	}

	public void DebugFormat(string format, params object[] args)
	{
		log1.DebugFormat(format, args);
		log2.DebugFormat(format, args);
	}

	public void DebugFormat(string format, object arg0)
	{
		log1.DebugFormat(format, arg0);
		log2.DebugFormat(format, arg0);
	}

	public void DebugFormat(string format, object arg0, object arg1)
	{
		log1.DebugFormat(format, arg0, arg1);
		log2.DebugFormat(format, arg0, arg1);
	}

	public void DebugFormat(string format, object arg0, object arg1, object arg2)
	{
		log1.DebugFormat(format, arg0, arg1, arg2);
		log2.DebugFormat(format, arg0, arg1, arg2);
	}

	public void DebugFormat(IFormatProvider provider, string format, params object[] args)
	{
		log1.DebugFormat(provider, format, args);
		log2.DebugFormat(provider, format, args);
	}

	public void Info(object message)
	{
		log1.Info(message);
		log2.Info(message);
	}

	public void Info(object message, Exception exception)
	{
		log1.Info(message, exception);
		log2.Info(message, exception);
	}

	public void InfoFormat(string format, params object[] args)
	{
		log1.InfoFormat(format, args);
		log2.InfoFormat(format, args);
	}

	public void InfoFormat(string format, object arg0)
	{
		log1.InfoFormat(format, arg0);
		log2.InfoFormat(format, arg0);
	}

	public void InfoFormat(string format, object arg0, object arg1)
	{
		log1.InfoFormat(format, arg0, arg1);
		log2.InfoFormat(format, arg0, arg1);
	}

	public void InfoFormat(string format, object arg0, object arg1, object arg2)
	{
		log1.InfoFormat(format, arg0, arg1, arg2);
		log2.InfoFormat(format, arg0, arg1, arg2);
	}

	public void InfoFormat(IFormatProvider provider, string format, params object[] args)
	{
		log1.InfoFormat(provider, format, args);
		log2.InfoFormat(provider, format, args);
	}

	public void Warn(object message)
	{
		log1.Warn(message);
		log2.Warn(message);
	}

	public void Warn(object message, Exception exception)
	{
		log1.Warn(message, exception);
		log2.Warn(message, exception);
	}

	public void WarnFormat(string format, params object[] args)
	{
		log1.WarnFormat(format, args);
		log2.WarnFormat(format, args);
	}

	public void WarnFormat(string format, object arg0)
	{
		log1.WarnFormat(format, arg0);
		log2.WarnFormat(format, arg0);
	}

	public void WarnFormat(string format, object arg0, object arg1)
	{
		log1.WarnFormat(format, arg0, arg1);
		log2.WarnFormat(format, arg0, arg1);
	}

	public void WarnFormat(string format, object arg0, object arg1, object arg2)
	{
		log1.WarnFormat(format, arg0, arg1, arg2);
		log2.WarnFormat(format, arg0, arg1, arg2);
	}

	public void WarnFormat(IFormatProvider provider, string format, params object[] args)
	{
		log1.WarnFormat(provider, format, args);
		log2.WarnFormat(provider, format, args);
	}

	public void Error(object message)
	{
		log1.Error(message);
		log2.Error(message);
	}

	public void Error(object message, Exception exception)
	{
		log1.Error(message, exception);
		log2.Error(message, exception);
	}

	public void ErrorFormat(string format, params object[] args)
	{
		log1.ErrorFormat(format, args);
		log2.ErrorFormat(format, args);
	}

	public void ErrorFormat(string format, object arg0)
	{
		log1.ErrorFormat(format, arg0);
		log2.ErrorFormat(format, arg0);
	}

	public void ErrorFormat(string format, object arg0, object arg1)
	{
		log1.ErrorFormat(format, arg0, arg1);
		log2.ErrorFormat(format, arg0, arg1);
	}

	public void ErrorFormat(string format, object arg0, object arg1, object arg2)
	{
		log1.ErrorFormat(format, arg0, arg1, arg2);
		log2.ErrorFormat(format, arg0, arg1, arg2);
	}

	public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
	{
		log1.ErrorFormat(provider, format, args);
		log2.ErrorFormat(provider, format, args);
	}

	public void Fatal(object message)
	{
		log1.Fatal(message);
		log2.Fatal(message);
	}

	public void Fatal(object message, Exception exception)
	{
		log1.Fatal(message, exception);
		log2.Fatal(message, exception);
	}

	public void FatalFormat(string format, params object[] args)
	{
		log1.FatalFormat(format, args);
		log2.FatalFormat(format, args);
	}

	public void FatalFormat(string format, object arg0)
	{
		log1.FatalFormat(format, arg0);
		log2.FatalFormat(format, arg0);
	}

	public void FatalFormat(string format, object arg0, object arg1)
	{
		log1.FatalFormat(format, arg0, arg1);
		log2.FatalFormat(format, arg0, arg1);
	}

	public void FatalFormat(string format, object arg0, object arg1, object arg2)
	{
		log1.FatalFormat(format, arg0, arg1, arg2);
		log2.FatalFormat(format, arg0, arg1, arg2);
	}

	public void FatalFormat(IFormatProvider provider, string format, params object[] args)
	{
		log1.FatalFormat(provider, format, args);
		log2.FatalFormat(provider, format, args);
	}
}
