using Brix.External.Factories;
using Brix.Game;
using UnityEngine;

namespace Brix.Transactions;

public class SpawnTransactionPayloadAtPosition : SpawnTransactionPayloadAtPositionBase<SpawnTransactionMonitor>
{
	public SpawnTransactionPayloadAtPosition(Factory.AssetKey assetKey, Faction faction, Vector3 position, Quaternion rotation, params SpawnPostProcessor[] postProcessors)
		: base(assetKey, faction, position, rotation, postProcessors)
	{
	}
}
