using System;
using Brix.Utils.Logging;

namespace Brix.Utils;

public static class ActionExtender
{
	private static BrixLogger logger = new BrixLogger(typeof(ActionExtender).FullName);

	public static Action GetSafeInvoker(this Action action)
	{
		return delegate
		{
			try
			{
				action();
			}
			catch (Exception e)
			{
				try
				{
					logger.ErrorFormat(e, "SafeInvoke: method={0}, target={1}", action.Method, action.Target);
				}
				catch (Exception message)
				{
					logger.Error(message);
				}
			}
		};
	}

	public static Action<T1> GetSafeInvoker<T1>(this Action<T1> action)
	{
		return delegate(T1 a)
		{
			try
			{
				action(a);
			}
			catch (Exception e)
			{
				try
				{
					logger.ErrorFormat(e, "SafeInvoke: method='{0}', target='{1}' a='{2}'", action.Method, action.Target, a);
				}
				catch (Exception message)
				{
					logger.Error(message);
				}
			}
		};
	}

	public static Action<T1, T2> GetSafeInvoker<T1, T2>(this Action<T1, T2> action)
	{
		return delegate(T1 a, T2 b)
		{
			try
			{
				action(a, b);
			}
			catch (Exception e)
			{
				try
				{
					logger.ErrorFormat(e, "SafeInvoke: method='{0}', target='{1}' a='{2}' b='{3}'", action.Method, action.Target, a, b);
				}
				catch (Exception message)
				{
					logger.Error(message);
				}
			}
		};
	}

	public static Action<T1, T2, T3> GetSafeInvoker<T1, T2, T3>(this Action<T1, T2, T3> action)
	{
		return delegate(T1 a, T2 b, T3 c)
		{
			try
			{
				action(a, b, c);
			}
			catch (Exception e)
			{
				try
				{
					logger.ErrorFormat(e, "SafeInvoke: method='{0}', target='{1}' a='{2}' b='{3}' c='{4}'", action.Method, action.Target, a, b, c);
				}
				catch (Exception message)
				{
					logger.Error(message);
				}
			}
		};
	}

	public static Action<T1, T2, T3, T4> GetSafeInvoker<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> action)
	{
		return delegate(T1 a, T2 b, T3 c, T4 d)
		{
			try
			{
				action(a, b, c, d);
			}
			catch (Exception e)
			{
				try
				{
					logger.ErrorFormat(e, "SafeInvoke: method='{0}', target='{1}' a='{2}' b='{3}' c='{4}' d='{5}'", action.Method, action.Target, a, b, c, d);
				}
				catch (Exception message)
				{
					logger.Error(message);
				}
			}
		};
	}

	public static Func<T1> GetSafeInvoker<T1>(this Func<T1> func)
	{
		return delegate
		{
			try
			{
				return func();
			}
			catch (Exception e)
			{
				try
				{
					logger.ErrorFormat(e, "SafeInvoke: method={0}, target={1}", func.Method, func.Target);
				}
				catch (Exception message)
				{
					logger.Error(message);
				}
			}
			return default(T1);
		};
	}

	public static Func<T1, T2> GetSafeInvoker<T1, T2>(this Func<T1, T2> func)
	{
		return delegate(T1 a)
		{
			try
			{
				return func(a);
			}
			catch (Exception e)
			{
				try
				{
					logger.ErrorFormat(e, "SafeInvoke: method='{0}', target='{1}' a='{2}'", func.Method, func.Target, a);
				}
				catch (Exception message)
				{
					logger.Error(message);
				}
			}
			return default(T2);
		};
	}

	public static Func<T1, T2, T3> GetSafeInvoker<T1, T2, T3>(this Func<T1, T2, T3> func)
	{
		return delegate(T1 a, T2 b)
		{
			try
			{
				return func(a, b);
			}
			catch (Exception e)
			{
				try
				{
					logger.ErrorFormat(e, "SafeInvoke: method='{0}', target='{1}' a='{2}' b='{3}'", func.Method, func.Target, a, b);
				}
				catch (Exception message)
				{
					logger.Error(message);
				}
			}
			return default(T3);
		};
	}

	public static Func<T1, T2, T3, T4> GetSafeInvoker<T1, T2, T3, T4>(this Func<T1, T2, T3, T4> func)
	{
		return delegate(T1 a, T2 b, T3 c)
		{
			try
			{
				return func(a, b, c);
			}
			catch (Exception e)
			{
				try
				{
					logger.ErrorFormat(e, "SafeInvoke: method='{0}', target='{1}' a='{2}' b='{3}' c='{4}'", func.Method, func.Target, a, b, c);
				}
				catch (Exception message)
				{
					logger.Error(message);
				}
			}
			return default(T4);
		};
	}

	public static Func<T1, T2, T3, T4, T5> GetSafeInvoker<T1, T2, T3, T4, T5>(this Func<T1, T2, T3, T4, T5> func)
	{
		return delegate(T1 a, T2 b, T3 c, T4 d)
		{
			try
			{
				return func(a, b, c, d);
			}
			catch (Exception e)
			{
				try
				{
					logger.ErrorFormat(e, "SafeInvoke: method='{0}', target='{1}' a='{2}' b='{3}' c='{4}' d='{5}'", func.Method, func.Target, a, b, c, d);
				}
				catch (Exception message)
				{
					logger.Error(message);
				}
			}
			return default(T5);
		};
	}
}
