using Brix.External.Signals;

namespace Brix.Utils.Signals;

public static class SignalConnectionExtender
{
	private static class Disconnector
	{
		public static SignalConnection DisconnectWhen(SignalConnection c, IConnectableOnceSignal s)
		{
			s.ConnectOnce(delegate(SignalConnection _c)
			{
				_c.Disconnect();
			}, c);
			return c;
		}
	}

	private static class Disconnector<T1>
	{
		public static SignalConnection DisconnectWhen(SignalConnection c, IConnectableOnceSignal<T1> s)
		{
			s.ConnectOnce(delegate(T1 a, SignalConnection _c)
			{
				_c.Disconnect();
			}, c);
			return c;
		}
	}

	private static class Disconnector<T1, T2>
	{
		public static SignalConnection DisconnectWhen(SignalConnection c, IConnectableOnceSignal<T1, T2> s)
		{
			s.ConnectOnce(delegate(T1 a, T2 b, SignalConnection _c)
			{
				_c.Disconnect();
			}, c);
			return c;
		}
	}

	public static SignalConnection DisconnectWhen(this SignalConnection c, IConnectableOnceSignal s)
	{
		return Disconnector.DisconnectWhen(c, s);
	}

	public static SignalConnection DisconnectWhen<T1>(this SignalConnection c, IConnectableOnceSignal<T1> s)
	{
		return Disconnector<T1>.DisconnectWhen(c, s);
	}

	public static SignalConnection DisconnectWhen<T1, T2>(this SignalConnection c, IConnectableOnceSignal<T1, T2> s)
	{
		return Disconnector<T1, T2>.DisconnectWhen(c, s);
	}
}
