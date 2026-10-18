using System;
using System.Collections;
using Brix.Engine;
using Brix.Engine.Abstract;
using Brix.External.Signals;
using Brix.Game;
using Brix.Utils.Listeners;

namespace Brix.Utils.Signals;

public static class SignalExtender
{
	private static class Utility<T1>
	{
		public static readonly Action<T1, SignalConnection> DisconnectWhen = delegate(T1 t1, SignalConnection connection)
		{
			connection.Disconnect();
		};

		public static readonly Action<T1, Action<T1>, Func<T1, bool>, SignalConnection> Connect = delegate(T1 t1, Action<T1> aAction, Func<T1, bool> aGuard, SignalConnection aConnection)
		{
			if (aGuard(t1))
			{
				aAction(t1);
				aConnection.Disconnect();
			}
		};
	}

	public static IEnumerator WaitForSignal(this Signal s, bool acceptPreviousOccurences = true)
	{
		if (acceptPreviousOccurences && s.InvokedCount > 0)
		{
			return Brix.Engine.Abstract.Neo.DoNothing();
		}
		return s._WaitForSignal();
	}

	private static IEnumerator _WaitForSignal(this Signal signal)
	{
		bool wait = true;
		signal.ConnectOnce(delegate
		{
			wait = false;
		});
		while (wait)
		{
			yield return GameClock.WaitForEndOfFrame;
		}
	}

	public static SignalConnection LateConnect(this Signal signal, Action action)
	{
		if (signal.InvokedCount > 0)
		{
			action();
		}
		return signal.Connect(action);
	}

	public static SignalConnection LateConnect<T1>(this Signal signal, Action<T1> action, T1 arg1)
	{
		if (signal.InvokedCount > 0)
		{
			action(arg1);
			return SignalConnection.Empty;
		}
		return signal.Connect(action, arg1);
	}

	public static SignalConnection LateConnectOnce(this Signal signal, Action action)
	{
		if (signal.InvokedCount > 0)
		{
			action();
			return SignalConnection.Empty;
		}
		return signal.ConnectOnce(action);
	}

	public static SignalConnection LateConnectOnce<T1>(this Signal signal, Action<T1> action, T1 arg1)
	{
		if (signal.InvokedCount > 0)
		{
			action(arg1);
			return SignalConnection.Empty;
		}
		return signal.ConnectOnce(action, arg1);
	}

	public static SignalConnection ConnectOnce(this ISignal signal, Action action, Func<bool> guard)
	{
		SignalConnection empty = SignalConnection.Empty;
		return signal.Connect(delegate(Action aAction, Func<bool> aGuard, SignalConnection aConnection)
		{
			if (aGuard())
			{
				aAction();
				aConnection.Disconnect();
			}
		}, action, guard, empty);
	}

	public static SignalConnection ConnectOnce<T1>(this ISignal<T1> signal, Action<T1> action, Func<T1, bool> guard)
	{
		SignalConnection empty = SignalConnection.Empty;
		return signal.Connect(Utility<T1>.Connect, action, guard, empty);
	}

	public static void DisconnectWhen(this SignalConnection connection, GameSignals.GameSignal signal)
	{
		GameSignals.Once(signal, delegate(SignalConnection c)
		{
			c.Disconnect();
		}, connection);
	}

	public static void DisconnectWhen<T1>(this SignalConnection connection, GameSignals.GameSignal<T1> signal)
	{
		GameSignals.Once(signal, Utility<T1>.DisconnectWhen, connection);
	}

	public static void DisconnectWhen(this SignalConnection connection, GameObjectListener listener)
	{
		listener.Signal.ConnectOnce(delegate(SignalConnection c)
		{
			c.Disconnect();
		}, connection);
	}

	public static void DisconnectWhen<T1>(this SignalConnection connection, GameObjectListener<T1> listener)
	{
		listener.Signal.ConnectOnce(Utility<T1>.DisconnectWhen, connection);
	}
}
