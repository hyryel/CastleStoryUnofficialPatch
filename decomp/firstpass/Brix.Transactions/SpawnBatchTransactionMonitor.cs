using System.Linq;
using Brix.External.Signals;
using UnityEngine;

namespace Brix.Transactions;

public class SpawnBatchTransactionMonitor : TransactionMonitor<SpawnBatchTransactionReturnValue>
{
	public Signal<GameObject[]> OnBatchSpawned = new Signal<GameObject[]>();

	public GameObject[] GameObjects => base.ReturnValue.SpawnedInstances;

	public SpawnBatchTransactionMonitor()
	{
		OnSuccess.Connect(delegate(SpawnBatchTransactionReturnValue r)
		{
			OnBatchSpawned.Invoke(r.SpawnedInstances);
		});
	}
}
public class SpawnBatchTransactionMonitor<T> : SpawnBatchTransactionMonitor where T : Component
{
	public new Signal<T[]> OnBatchSpawned = new Signal<T[]>();

	public SpawnBatchTransactionMonitor()
	{
		base.OnBatchSpawned.Connect(delegate(GameObject[] gs)
		{
			OnBatchSpawned.Invoke(gs.Select((GameObject g) => g.GetComponent<T>()).ToArray());
		});
	}
}
