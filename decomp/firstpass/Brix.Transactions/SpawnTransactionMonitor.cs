using Brix.External.Signals;
using UnityEngine;

namespace Brix.Transactions;

public class SpawnTransactionMonitor : TransactionMonitor<SpawnTransactionReturnValue>
{
	public Signal<GameObject> OnSpawned = new Signal<GameObject>();

	public GameObject GameObject => base.ReturnValue.SpawnedInstance;

	public SpawnTransactionMonitor()
	{
		OnSuccess.Connect(delegate(SpawnTransactionReturnValue r)
		{
			OnSpawned.Invoke(r.SpawnedInstance);
		});
	}
}
public class SpawnTransactionMonitor<T> : SpawnTransactionMonitor where T : Component
{
	public new Signal<T> OnSpawned = new Signal<T>();

	public SpawnTransactionMonitor()
	{
		base.OnSpawned.Connect(delegate(GameObject g)
		{
			OnSpawned.Invoke(g.GetComponent<T>());
		});
	}
}
