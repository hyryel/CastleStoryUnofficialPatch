using System;
using System.Diagnostics;
using log4net;
using log4net.Core;

namespace Brix.Utils.Logging;

public class BrixLogger : ILog, ILoggerWrapper
{
	protected static bool _environmentConfigured;

	protected ILog ilog;

	public ILogger Logger => ilog.Logger;

	public bool IsDebugEnabled => ilog.IsDebugEnabled;

	public bool IsInfoEnabled => ilog.IsInfoEnabled;

	public bool IsWarnEnabled => ilog.IsWarnEnabled;

	public bool IsErrorEnabled => ilog.IsErrorEnabled;

	public bool IsFatalEnabled => ilog.IsFatalEnabled;

	public BrixLogger()
		: this(new StackFrame(1, fNeedFileInfo: true).GetMethod().DeclaringType.FullName)
	{
	}

	public BrixLogger(string name)
	{
		if (!_environmentConfigured)
		{
			_environmentConfigured = true;
			BrixLoggerConfigurator.Configure();
		}
		ilog = LogManager.GetLogger(name);
	}

	public void Debug(object message)
	{
		ilog.Debug(message);
	}

	public void Debug(Exception exception, object message)
	{
		ilog.Debug(message, exception);
	}

	void ILog.Debug(object message, Exception exception)
	{
		ilog.Debug(message, exception);
	}

	void ILog.DebugFormat(string format, object arg0)
	{
		DebugFormat(format, arg0);
	}

	void ILog.DebugFormat(string format, object arg0, object arg1)
	{
		ilog.DebugFormat(format, arg0, arg1);
	}

	void ILog.DebugFormat(string format, object arg0, object arg1, object arg2)
	{
		ilog.DebugFormat(format, arg0, arg1, arg2);
	}

	void ILog.DebugFormat(IFormatProvider provider, string format, params object[] args)
	{
		ilog.DebugFormat(provider, format, args);
	}

	void ILog.DebugFormat(string format, params object[] args)
	{
		ilog.DebugFormat(format, args);
	}

	public void DebugFormat<T0>(string format, T0 t0)
	{
		if (ilog.IsDebugEnabled)
		{
			ilog.Debug(string.Format(format, t0));
		}
	}

	public void DebugFormat<T0, T1>(string format, T0 t0, T1 t1)
	{
		if (ilog.IsDebugEnabled)
		{
			ilog.Debug(string.Format(format, t0, t1));
		}
	}

	public void DebugFormat<T0, T1, T2>(string format, T0 t0, T1 t1, T2 t2)
	{
		if (ilog.IsDebugEnabled)
		{
			ilog.Debug(string.Format(format, t0, t1, t2));
		}
	}

	public void DebugFormat<T0, T1, T2, T3>(string format, T0 t0, T1 t1, T2 t2, T3 t3)
	{
		if (ilog.IsDebugEnabled)
		{
			ilog.Debug(string.Format(format, t0, t1, t2, t3));
		}
	}

	public void DebugFormat<T0, T1, T2, T3, T4>(string format, T0 t0, T1 t1, T2 t2, T3 t3, T4 t4)
	{
		if (ilog.IsDebugEnabled)
		{
			ilog.Debug(string.Format(format, t0, t1, t2, t3, t4));
		}
	}

	public void DebugFormat<T0, T1, T2, T3, T4, T5>(string format, T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
	{
		if (ilog.IsDebugEnabled)
		{
			ilog.Debug(string.Format(format, t0, t1, t2, t3, t4, t5));
		}
	}

	public void DebugFormat<T0, T1, T2, T3, T4, T5, T6>(string format, T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6)
	{
		if (ilog.IsDebugEnabled)
		{
			ilog.Debug(string.Format(format, t0, t1, t2, t3, t4, t5, t6));
		}
	}

	public void DebugFormat<T0>(Exception e, string format, T0 t0)
	{
		if (ilog.IsDebugEnabled)
		{
			ilog.Debug(string.Format(format, t0), e);
		}
	}

	public void DebugFormat<T0, T1>(Exception e, string format, T0 t0, T1 t1)
	{
		if (ilog.IsDebugEnabled)
		{
			ilog.Debug(string.Format(format, t0, t1), e);
		}
	}

	public void DebugFormat<T0, T1, T2>(Exception e, string format, T0 t0, T1 t1, T2 t2)
	{
		if (ilog.IsDebugEnabled)
		{
			ilog.Debug(string.Format(format, t0, t1, t2), e);
		}
	}

	public void DebugFormat<T0, T1, T2, T3>(Exception e, string format, T0 t0, T1 t1, T2 t2, T3 t3)
	{
		if (ilog.IsDebugEnabled)
		{
			ilog.Debug(string.Format(format, t0, t1, t2, t3), e);
		}
	}

	public void DebugFormat<T0, T1, T2, T3, T4>(Exception e, string format, T0 t0, T1 t1, T2 t2, T3 t3, T4 t4)
	{
		if (ilog.IsDebugEnabled)
		{
			ilog.Debug(string.Format(format, t0, t1, t2, t3, t4), e);
		}
	}

	public void DebugFormat<T0, T1, T2, T3, T4, T5>(Exception e, string format, T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
	{
		if (ilog.IsDebugEnabled)
		{
			ilog.Debug(string.Format(format, t0, t1, t2, t3, t4, t5), e);
		}
	}

	public void DebugFormat<T0, T1, T2, T3, T4, T5, T6>(Exception e, string format, T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6)
	{
		if (ilog.IsDebugEnabled)
		{
			ilog.Debug(string.Format(format, t0, t1, t2, t3, t4, t5, t6), e);
		}
	}

	public void Info(object message)
	{
		ilog.Info(message);
	}

	private void Info(Exception exception, object message)
	{
		ilog.Info(message, exception);
	}

	void ILog.Info(object message, Exception exception)
	{
		ilog.Info(message, exception);
	}

	void ILog.InfoFormat(string format, params object[] args)
	{
		ilog.InfoFormat(format, args);
	}

	void ILog.InfoFormat(string format, object arg0)
	{
		ilog.InfoFormat(format, arg0);
	}

	void ILog.InfoFormat(string format, object arg0, object arg1)
	{
		ilog.InfoFormat(format, arg0, arg1);
	}

	void ILog.InfoFormat(string format, object arg0, object arg1, object arg2)
	{
		ilog.InfoFormat(format, arg0, arg1, arg2);
	}

	void ILog.InfoFormat(IFormatProvider provider, string format, params object[] args)
	{
		ilog.InfoFormat(provider, format, args);
	}

	public void InfoFormat<T0>(string format, T0 t0)
	{
		if (ilog.IsInfoEnabled)
		{
			ilog.Info(string.Format(format, t0));
		}
	}

	public void InfoFormat<T0, T1>(string format, T0 t0, T1 t1)
	{
		if (ilog.IsInfoEnabled)
		{
			ilog.Info(string.Format(format, t0, t1));
		}
	}

	public void InfoFormat<T0, T1, T2>(string format, T0 t0, T1 t1, T2 t2)
	{
		if (ilog.IsInfoEnabled)
		{
			ilog.Info(string.Format(format, t0, t1, t2));
		}
	}

	public void InfoFormat<T0, T1, T2, T3>(string format, T0 t0, T1 t1, T2 t2, T3 t3)
	{
		if (ilog.IsInfoEnabled)
		{
			ilog.Info(string.Format(format, t0, t1, t2, t3));
		}
	}

	public void InfoFormat<T0, T1, T2, T3, T4>(string format, T0 t0, T1 t1, T2 t2, T3 t3, T4 t4)
	{
		if (ilog.IsInfoEnabled)
		{
			ilog.Info(string.Format(format, t0, t1, t2, t3, t4));
		}
	}

	public void InfoFormat<T0, T1, T2, T3, T4, T5>(string format, T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
	{
		if (ilog.IsInfoEnabled)
		{
			ilog.Info(string.Format(format, t0, t1, t2, t3, t4, t5));
		}
	}

	public void InfoFormat<T0, T1, T2, T3, T4, T5, T6>(string format, T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6)
	{
		if (ilog.IsInfoEnabled)
		{
			ilog.Info(string.Format(format, t0, t1, t2, t3, t4, t5, t6));
		}
	}

	public void InfoFormat<T0>(Exception e, string format, T0 t0)
	{
		if (ilog.IsInfoEnabled)
		{
			ilog.Info(string.Format(format, t0), e);
		}
	}

	public void InfoFormat<T0, T1>(Exception e, string format, T0 t0, T1 t1)
	{
		if (ilog.IsInfoEnabled)
		{
			ilog.Info(string.Format(format, t0, t1), e);
		}
	}

	public void InfoFormat<T0, T1, T2>(Exception e, string format, T0 t0, T1 t1, T2 t2)
	{
		if (ilog.IsInfoEnabled)
		{
			ilog.Info(string.Format(format, t0, t1, t2), e);
		}
	}

	public void InfoFormat<T0, T1, T2, T3>(Exception e, string format, T0 t0, T1 t1, T2 t2, T3 t3)
	{
		if (ilog.IsInfoEnabled)
		{
			ilog.Info(string.Format(format, t0, t1, t2, t3), e);
		}
	}

	public void InfoFormat<T0, T1, T2, T3, T4>(Exception e, string format, T0 t0, T1 t1, T2 t2, T3 t3, T4 t4)
	{
		if (ilog.IsInfoEnabled)
		{
			ilog.Info(string.Format(format, t0, t1, t2, t3, t4), e);
		}
	}

	public void InfoFormat<T0, T1, T2, T3, T4, T5>(Exception e, string format, T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
	{
		if (ilog.IsInfoEnabled)
		{
			ilog.Info(string.Format(format, t0, t1, t2, t3, t4, t5), e);
		}
	}

	public void InfoFormat<T0, T1, T2, T3, T4, T5, T6>(Exception e, string format, T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6)
	{
		if (ilog.IsInfoEnabled)
		{
			ilog.Info(string.Format(format, t0, t1, t2, t3, t4, t5, t6), e);
		}
	}

	public void Warn(object message)
	{
		ilog.Warn(message);
	}

	public void Warn(Exception exception, object message)
	{
		ilog.Warn(message, exception);
	}

	void ILog.Warn(object message, Exception exception)
	{
		ilog.Warn(message, exception);
	}

	void ILog.WarnFormat(string format, params object[] args)
	{
		ilog.WarnFormat(format, args);
	}

	void ILog.WarnFormat(string format, object arg0)
	{
		ilog.WarnFormat(format, arg0);
	}

	void ILog.WarnFormat(string format, object arg0, object arg1)
	{
		ilog.WarnFormat(format, arg0, arg1);
	}

	void ILog.WarnFormat(string format, object arg0, object arg1, object arg2)
	{
		ilog.WarnFormat(format, arg0, arg1, arg2);
	}

	void ILog.WarnFormat(IFormatProvider provider, string format, params object[] args)
	{
		ilog.WarnFormat(provider, format, args);
	}

	public void WarnFormat<T0>(string format, T0 t0)
	{
		if (ilog.IsWarnEnabled)
		{
			ilog.Warn(string.Format(format, t0));
		}
	}

	public void WarnFormat<T0, T1>(string format, T0 t0, T1 t1)
	{
		if (ilog.IsWarnEnabled)
		{
			ilog.Warn(string.Format(format, t0, t1));
		}
	}

	public void WarnFormat<T0, T1, T2>(string format, T0 t0, T1 t1, T2 t2)
	{
		if (ilog.IsWarnEnabled)
		{
			ilog.Warn(string.Format(format, t0, t1, t2));
		}
	}

	public void WarnFormat<T0, T1, T2, T3>(string format, T0 t0, T1 t1, T2 t2, T3 t3)
	{
		if (ilog.IsWarnEnabled)
		{
			ilog.Warn(string.Format(format, t0, t1, t2, t3));
		}
	}

	public void WarnFormat<T0, T1, T2, T3, T4>(string format, T0 t0, T1 t1, T2 t2, T3 t3, T4 t4)
	{
		if (ilog.IsWarnEnabled)
		{
			ilog.Warn(string.Format(format, t0, t1, t2, t3, t4));
		}
	}

	public void WarnFormat<T0, T1, T2, T3, T4, T5>(string format, T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
	{
		if (ilog.IsWarnEnabled)
		{
			ilog.Warn(string.Format(format, t0, t1, t2, t3, t4, t5));
		}
	}

	public void WarnFormat<T0, T1, T2, T3, T4, T5, T6>(string format, T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6)
	{
		if (ilog.IsWarnEnabled)
		{
			ilog.Warn(string.Format(format, t0, t1, t2, t3, t4, t5, t6));
		}
	}

	public void WarnFormat<T0>(Exception e, string format, T0 t0)
	{
		if (ilog.IsWarnEnabled)
		{
			ilog.Warn(string.Format(format, t0), e);
		}
	}

	public void WarnFormat<T0, T1>(Exception e, string format, T0 t0, T1 t1)
	{
		if (ilog.IsWarnEnabled)
		{
			ilog.Warn(string.Format(format, t0, t1), e);
		}
	}

	public void WarnFormat<T0, T1, T2>(Exception e, string format, T0 t0, T1 t1, T2 t2)
	{
		if (ilog.IsWarnEnabled)
		{
			ilog.Warn(string.Format(format, t0, t1, t2), e);
		}
	}

	public void WarnFormat<T0, T1, T2, T3>(Exception e, string format, T0 t0, T1 t1, T2 t2, T3 t3)
	{
		if (ilog.IsWarnEnabled)
		{
			ilog.Warn(string.Format(format, t0, t1, t2, t3), e);
		}
	}

	public void WarnFormat<T0, T1, T2, T3, T4>(Exception e, string format, T0 t0, T1 t1, T2 t2, T3 t3, T4 t4)
	{
		if (ilog.IsWarnEnabled)
		{
			ilog.Warn(string.Format(format, t0, t1, t2, t3, t4), e);
		}
	}

	public void WarnFormat<T0, T1, T2, T3, T4, T5>(Exception e, string format, T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
	{
		if (ilog.IsWarnEnabled)
		{
			ilog.Warn(string.Format(format, t0, t1, t2, t3, t4, t5), e);
		}
	}

	public void WarnFormat<T0, T1, T2, T3, T4, T5, T6>(Exception e, string format, T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6)
	{
		if (ilog.IsWarnEnabled)
		{
			ilog.Warn(string.Format(format, t0, t1, t2, t3, t4, t5, t6), e);
		}
	}

	public void Error(object message)
	{
		ilog.Error(message);
	}

	public void Error(Exception exception, object message)
	{
		ilog.Error(message, exception);
	}

	void ILog.Error(object message, Exception exception)
	{
		ilog.Error(message, exception);
	}

	void ILog.ErrorFormat(string format, params object[] args)
	{
		ilog.ErrorFormat(format, args);
	}

	void ILog.ErrorFormat(string format, object arg0)
	{
		ilog.ErrorFormat(format, arg0);
	}

	void ILog.ErrorFormat(string format, object arg0, object arg1)
	{
		ilog.ErrorFormat(format, arg0, arg1);
	}

	void ILog.ErrorFormat(string format, object arg0, object arg1, object arg2)
	{
		ilog.ErrorFormat(format, arg0, arg1, arg2);
	}

	void ILog.ErrorFormat(IFormatProvider provider, string format, params object[] args)
	{
		ilog.ErrorFormat(provider, format, args);
	}

	public void ErrorFormat<T0>(string format, T0 t0)
	{
		if (ilog.IsErrorEnabled)
		{
			ilog.Error(string.Format(format, t0));
		}
	}

	public void ErrorFormat<T0, T1>(string format, T0 t0, T1 t1)
	{
		if (ilog.IsErrorEnabled)
		{
			ilog.Error(string.Format(format, t0, t1));
		}
	}

	public void ErrorFormat<T0, T1, T2>(string format, T0 t0, T1 t1, T2 t2)
	{
		if (ilog.IsErrorEnabled)
		{
			ilog.Error(string.Format(format, t0, t1, t2));
		}
	}

	public void ErrorFormat<T0, T1, T2, T3>(string format, T0 t0, T1 t1, T2 t2, T3 t3)
	{
		if (ilog.IsErrorEnabled)
		{
			ilog.Error(string.Format(format, t0, t1, t2, t3));
		}
	}

	public void ErrorFormat<T0, T1, T2, T3, T4>(string format, T0 t0, T1 t1, T2 t2, T3 t3, T4 t4)
	{
		if (ilog.IsErrorEnabled)
		{
			ilog.Error(string.Format(format, t0, t1, t2, t3, t4));
		}
	}

	public void ErrorFormat<T0, T1, T2, T3, T4, T5>(string format, T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
	{
		if (ilog.IsErrorEnabled)
		{
			ilog.Error(string.Format(format, t0, t1, t2, t3, t4, t5));
		}
	}

	public void ErrorFormat<T0, T1, T2, T3, T4, T5, T6>(string format, T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6)
	{
		if (ilog.IsErrorEnabled)
		{
			ilog.Error(string.Format(format, t0, t1, t2, t3, t4, t5, t6));
		}
	}

	public void ErrorFormat<T0>(Exception e, string format, T0 t0)
	{
		if (ilog.IsErrorEnabled)
		{
			ilog.Error(string.Format(format, t0), e);
		}
	}

	public void ErrorFormat<T0, T1>(Exception e, string format, T0 t0, T1 t1)
	{
		if (ilog.IsErrorEnabled)
		{
			ilog.Error(string.Format(format, t0, t1), e);
		}
	}

	public void ErrorFormat<T0, T1, T2>(Exception e, string format, T0 t0, T1 t1, T2 t2)
	{
		if (ilog.IsErrorEnabled)
		{
			ilog.Error(string.Format(format, t0, t1, t2), e);
		}
	}

	public void ErrorFormat<T0, T1, T2, T3>(Exception e, string format, T0 t0, T1 t1, T2 t2, T3 t3)
	{
		if (ilog.IsErrorEnabled)
		{
			ilog.Error(string.Format(format, t0, t1, t2, t3), e);
		}
	}

	public void ErrorFormat<T0, T1, T2, T3, T4>(Exception e, string format, T0 t0, T1 t1, T2 t2, T3 t3, T4 t4)
	{
		if (ilog.IsErrorEnabled)
		{
			ilog.Error(string.Format(format, t0, t1, t2, t3, t4), e);
		}
	}

	public void ErrorFormat<T0, T1, T2, T3, T4, T5>(Exception e, string format, T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
	{
		if (ilog.IsErrorEnabled)
		{
			ilog.Error(string.Format(format, t0, t1, t2, t3, t4, t5), e);
		}
	}

	public void ErrorFormat<T0, T1, T2, T3, T4, T5, T6>(Exception e, string format, T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6)
	{
		if (ilog.IsErrorEnabled)
		{
			ilog.Error(string.Format(format, t0, t1, t2, t3, t4, t5, t6), e);
		}
	}

	public void Fatal(object message)
	{
		ilog.Fatal(message);
	}

	public void Fatal(Exception exception, object message)
	{
		ilog.Fatal(message, exception);
	}

	void ILog.Fatal(object message, Exception exception)
	{
		ilog.Fatal(message, exception);
	}

	void ILog.FatalFormat(string format, params object[] args)
	{
		ilog.FatalFormat(format, args);
	}

	void ILog.FatalFormat(string format, object arg0)
	{
		ilog.FatalFormat(format, arg0);
	}

	void ILog.FatalFormat(string format, object arg0, object arg1)
	{
		ilog.FatalFormat(format, arg0, arg1);
	}

	void ILog.FatalFormat(string format, object arg0, object arg1, object arg2)
	{
		ilog.FatalFormat(format, arg0, arg1, arg2);
	}

	void ILog.FatalFormat(IFormatProvider provider, string format, params object[] args)
	{
		ilog.FatalFormat(provider, format, args);
	}

	public void FatalFormat<T0>(string format, T0 t0)
	{
		if (ilog.IsErrorEnabled)
		{
			ilog.Fatal(string.Format(format, t0));
		}
	}

	public void FatalFormat<T0, T1>(string format, T0 t0, T1 t1)
	{
		if (ilog.IsErrorEnabled)
		{
			ilog.Fatal(string.Format(format, t0, t1));
		}
	}

	public void FatalFormat<T0, T1, T2>(string format, T0 t0, T1 t1, T2 t2)
	{
		if (ilog.IsErrorEnabled)
		{
			ilog.Fatal(string.Format(format, t0, t1, t2));
		}
	}

	public void FatalFormat<T0, T1, T2, T3>(string format, T0 t0, T1 t1, T2 t2, T3 t3)
	{
		if (ilog.IsErrorEnabled)
		{
			ilog.Fatal(string.Format(format, t0, t1, t2, t3));
		}
	}

	public void FatalFormat<T0, T1, T2, T3, T4>(string format, T0 t0, T1 t1, T2 t2, T3 t3, T4 t4)
	{
		if (ilog.IsErrorEnabled)
		{
			ilog.Fatal(string.Format(format, t0, t1, t2, t3, t4));
		}
	}

	public void FatalFormat<T0, T1, T2, T3, T4, T5>(string format, T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
	{
		if (ilog.IsErrorEnabled)
		{
			ilog.Fatal(string.Format(format, t0, t1, t2, t3, t4, t5));
		}
	}

	public void FatalFormat<T0, T1, T2, T3, T4, T5, T6>(string format, T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6)
	{
		if (ilog.IsErrorEnabled)
		{
			ilog.Fatal(string.Format(format, t0, t1, t2, t3, t4, t5, t6));
		}
	}

	public void FatalFormat<T0>(Exception e, string format, T0 t0)
	{
		if (ilog.IsErrorEnabled)
		{
			ilog.Fatal(string.Format(format, t0), e);
		}
	}

	public void FatalFormat<T0, T1>(Exception e, string format, T0 t0, T1 t1)
	{
		if (ilog.IsErrorEnabled)
		{
			ilog.Fatal(string.Format(format, t0, t1), e);
		}
	}

	public void FatalFormat<T0, T1, T2>(Exception e, string format, T0 t0, T1 t1, T2 t2)
	{
		if (ilog.IsErrorEnabled)
		{
			ilog.Fatal(string.Format(format, t0, t1, t2), e);
		}
	}

	public void FatalFormat<T0, T1, T2, T3>(Exception e, string format, T0 t0, T1 t1, T2 t2, T3 t3)
	{
		if (ilog.IsErrorEnabled)
		{
			ilog.Fatal(string.Format(format, t0, t1, t2, t3), e);
		}
	}

	public void FatalFormat<T0, T1, T2, T3, T4>(Exception e, string format, T0 t0, T1 t1, T2 t2, T3 t3, T4 t4)
	{
		if (ilog.IsErrorEnabled)
		{
			ilog.Fatal(string.Format(format, t0, t1, t2, t3, t4), e);
		}
	}

	public void FatalFormat<T0, T1, T2, T3, T4, T5>(Exception e, string format, T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
	{
		if (ilog.IsErrorEnabled)
		{
			ilog.Fatal(string.Format(format, t0, t1, t2, t3, t4, t5), e);
		}
	}

	public void FatalFormat<T0, T1, T2, T3, T4, T5, T6>(Exception e, string format, T0 t0, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6)
	{
		if (ilog.IsErrorEnabled)
		{
			ilog.Fatal(string.Format(format, t0, t1, t2, t3, t4, t5, t6), e);
		}
	}
}
