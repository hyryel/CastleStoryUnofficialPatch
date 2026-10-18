using UnityEngine;

namespace Brix.Transactions;

public class SpawnBatchTransactionPayloadBase<TMonitor> : TransactionPayload<SpawnBatchTransactionReturnValue, TMonitor> where TMonitor : SpawnBatchTransactionMonitor, new()
{
	private TransactionPayload<SpawnTransactionReturnValue>[] _payloads;

	public SpawnBatchTransactionPayloadBase(params TransactionPayload<SpawnTransactionReturnValue>[] payloads)
	{
		_payloads = payloads;
	}

	public override void Invoke(SpawnBatchTransactionReturnValue returnValue)
	{
		returnValue.SpawnedInstances = new GameObject[_payloads.Length];
		for (int i = 0; i < _payloads.Length; i++)
		{
			SpawnTransactionReturnValue spawnTransactionReturnValue = new SpawnTransactionReturnValue();
			_payloads[i].Invoke(spawnTransactionReturnValue);
			returnValue.SpawnedInstances[i] = spawnTransactionReturnValue.SpawnedInstance;
		}
	}
}
