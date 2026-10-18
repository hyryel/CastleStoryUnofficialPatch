using Brix.Lifecycle.Pooling;
using UnityEngine;

namespace Brix.Transactions;

public class DestroyTransactionPayload : TransactionPayload
{
	public GameObject gameObject;

	public override void Invoke()
	{
		ObjectPoolSingleton.Release(gameObject);
	}
}
