using System;
using Brix.External.Signals;
using Brix.Transactions;
using Brix.Utils.Logging;

namespace Brix.Game.Network;

public class NetworkSignal : Signal
{
	private static BrixLogger logger = new BrixLogger(typeof(NetworkSignal).FullName);

	private NetworkHandle<NetworkSignal> _handle;

	private Transactor _transactor;

	public NetworkSignal(Transactor transactor)
	{
		_handle = transactor.CreateHandle(this);
		_transactor = transactor;
	}

	[SynchedMethod]
	private static void NetworkInvoke(NetworkHandle<NetworkSignal> handle)
	{
		handle.Value.BaseInvoke();
	}

	private void BaseInvoke()
	{
		base.Invoke();
	}

	public new TransactionMonitor Invoke()
	{
		try
		{
			if (_transactor.IsMine)
			{
				return _transactor.ExecuteSynchedMethod(NetworkInvoke, _handle);
			}
		}
		catch (Exception message)
		{
			logger.Error(message);
		}
		return null;
	}
}
public class NetworkSignal<P1> : Signal<P1>
{
	private static BrixLogger logger = new BrixLogger(typeof(NetworkSignal<P1>).FullName);

	private NetworkHandle<NetworkSignal<P1>> _handle;

	private Transactor _transactor;

	public NetworkSignal(Transactor transactor)
	{
		_transactor = transactor;
		_handle = transactor.CreateHandle(this);
	}

	[SynchedMethod]
	private static void NetworkInvoke(NetworkHandle<NetworkSignal<P1>> handle, P1 a)
	{
		handle.Value.BaseInvoke(a);
	}

	private void BaseInvoke(P1 a)
	{
		base.Invoke(a);
	}

	public new TransactionMonitor Invoke(P1 a)
	{
		try
		{
			if (_transactor.IsMine)
			{
				return _transactor.ExecuteSynchedMethod(NetworkInvoke, _handle, a);
			}
		}
		catch (Exception message)
		{
			logger.Error(message);
		}
		return null;
	}
}
public class NetworkSignal<P1, P2> : Signal<P1, P2>
{
	private static BrixLogger logger = new BrixLogger(typeof(NetworkSignal<P1, P2>).FullName);

	private NetworkHandle<NetworkSignal<P1, P2>> _handle;

	private Transactor _transactor;

	public NetworkSignal(Transactor transactor)
	{
		_transactor = transactor;
		_handle = transactor.CreateHandle(this);
	}

	[SynchedMethod]
	private static void NetworkInvoke(NetworkHandle<NetworkSignal<P1, P2>> handle, P1 a, P2 b)
	{
		handle.Value.BaseInvoke(a, b);
	}

	private void BaseInvoke(P1 a, P2 b)
	{
		base.Invoke(a, b);
	}

	public new TransactionMonitor Invoke(P1 a, P2 b)
	{
		try
		{
			if (_transactor.IsMine)
			{
				return _transactor.ExecuteSynchedMethod(NetworkInvoke, _handle, a, b);
			}
		}
		catch (Exception message)
		{
			logger.Error(message);
		}
		return null;
	}
}
public class NetworkSignal<P1, P2, P3> : Signal<P1, P2, P3>
{
	private static BrixLogger logger = new BrixLogger(typeof(NetworkSignal<P1, P2, P3>).FullName);

	private NetworkHandle<NetworkSignal<P1, P2, P3>> _handle;

	private Transactor _transactor;

	public NetworkSignal(Transactor transactor)
	{
		_transactor = transactor;
		_handle = transactor.CreateHandle(this);
	}

	[SynchedMethod]
	private static void NetworkInvoke(NetworkHandle<NetworkSignal<P1, P2, P3>> handle, P1 a, P2 b, P3 c)
	{
		handle.Value.BaseInvoke(a, b, c);
	}

	private void BaseInvoke(P1 a, P2 b, P3 c)
	{
		base.Invoke(a, b, c);
	}

	public new TransactionMonitor Invoke(P1 a, P2 b, P3 c)
	{
		try
		{
			if (_transactor.IsMine)
			{
				return _transactor.ExecuteSynchedMethod(NetworkInvoke, _handle, a, b, c);
			}
		}
		catch (Exception message)
		{
			logger.Error(message);
		}
		return null;
	}
}
public class NetworkSignal<P1, P2, P3, P4> : Signal<P1, P2, P3, P4>
{
	private static BrixLogger logger = new BrixLogger(typeof(NetworkSignal<P1, P2, P3, P4>).FullName);

	private NetworkHandle<NetworkSignal<P1, P2, P3, P4>> _handle;

	private Transactor _transactor;

	public NetworkSignal(Transactor transactor)
	{
		_transactor = transactor;
		_handle = transactor.CreateHandle(this);
	}

	[SynchedMethod]
	private static void NetworkInvoke(NetworkHandle<NetworkSignal<P1, P2, P3, P4>> handle, P1 a, P2 b, P3 c, P4 d)
	{
		handle.Value.BaseInvoke(a, b, c, d);
	}

	private void BaseInvoke(P1 a, P2 b, P3 c, P4 d)
	{
		base.Invoke(a, b, c, d);
	}

	public new TransactionMonitor Invoke(P1 a, P2 b, P3 c, P4 d)
	{
		try
		{
			if (_transactor.IsMine)
			{
				return _transactor.ExecuteSynchedMethod(NetworkInvoke, _handle, a, b, c, d);
			}
		}
		catch (Exception message)
		{
			logger.Error(message);
		}
		return null;
	}
}
