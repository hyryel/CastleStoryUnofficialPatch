using UnityEngine;

namespace Brix.Transactions;

public abstract class SpawnPostProcessor
{
	public abstract void OnSpawnPostProcess(GameObject instance);
}
