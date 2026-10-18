using System;
using Brix.External.Signals;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.UI.GameObserver;

public static class SignalNullCheckExtender<T> where T : UnityEngine.Object
{
	public static class Utility<TA>
	{
		public static Action<Action<T, TA>, T, TA> DoNothingIfNull = delegate(Action<T, TA> f, T o, TA a)
		{
			if (SignalNullCheckExtender<T>.NullCheck((Delegate)f, o))
			{
				f(o, a);
			}
		};
	}

	private static BrixLogger logger = new BrixLogger(typeof(SignalNullCheckExtender<T>).FullName);

	private static bool NullCheck(Delegate func, T obj)
	{
		if ((object)func == null)
		{
			return false;
		}
		if (obj == null)
		{
			logger.ErrorFormat("Null value in UIGameObserver signal for {0}", typeof(T).ToString());
			return false;
		}
		return true;
	}
}
public static class SignalNullCheckExtender
{
	public static SignalConnection ConnectNullChecked<T, TA>(this IReadOnlySignal signal, Action<T, TA> func, T obj, TA a) where T : UnityEngine.Object
	{
		return signal.Connect(SignalNullCheckExtender<T>.Utility<TA>.DoNothingIfNull, func, obj, a);
	}
}
