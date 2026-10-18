using System.Collections.Generic;
using Brix.Transactions;

namespace Brix.Game.Network;

public class NetworkLocator<T> : Locator<T>
{
	protected static Dictionary<Transactor, NetworkLocator<T>> transactor2locator = new Dictionary<Transactor, NetworkLocator<T>>();

	public Transactor Transactor;

	public Registry<T> Registry;

	private NetworkLocator(Transactor transactor, Registry<T> registry)
	{
		Transactor = transactor;
		Registry = registry;
	}

	public static NetworkLocator<T> GetOrCreateLocator<S>(Transactor transactor) where S : Registry<T>, new()
	{
		if (!transactor2locator.TryGetValue(transactor, out var value))
		{
			transactor2locator.Add(transactor, value = new NetworkLocator<T>(transactor, new S()));
		}
		return value;
	}

	public static NetworkLocator<T> GetOrCreateLocator<S>(Transactor transactor, out S registry) where S : Registry<T>, new()
	{
		if (transactor2locator.TryGetValue(transactor, out var value))
		{
			registry = (S)value.Registry;
		}
		else
		{
			transactor2locator.Add(transactor, value = new NetworkLocator<T>(transactor, registry = new S()));
		}
		return value;
	}

	public static NetworkLocator<T> RetrieveLocator(Transactor transactor)
	{
		return transactor2locator[transactor];
	}

	public NetworkHandle<T> CreateHandle(T t)
	{
		return new NetworkHandle<T>(this, t);
	}
}
