using UnityEngine;

namespace Brix.Transactions;

public class SpawnBatchTransactionPayload<T> : SpawnBatchTransactionPayloadBase<SpawnBatchTransactionMonitor<T>> where T : Component
{
	public SpawnBatchTransactionPayload(params TransactionPayload<SpawnTransactionReturnValue>[] payloads)
		: base(payloads)
	{
	}
}
