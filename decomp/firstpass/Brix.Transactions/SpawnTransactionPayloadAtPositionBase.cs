using Brix.External.Factories;
using Brix.Game;
using Brix.Lifecycle.Pooling;
using UnityEngine;

namespace Brix.Transactions;

public class SpawnTransactionPayloadAtPositionBase<TMonitor> : SpawnTransactionPayloadBase<TMonitor> where TMonitor : SpawnTransactionMonitor, new()
{
	private readonly Vector3 _position;

	private readonly Quaternion _rotation;

	public SpawnTransactionPayloadAtPositionBase(Factory.AssetKey assetKey, Faction faction, Vector3 position, Quaternion rotation, params SpawnPostProcessor[] postProcessors)
		: base(assetKey, faction, postProcessors)
	{
		_position = position;
		_rotation = rotation;
	}

	public override GameObject Spawn()
	{
		ObjectPoolSingleton.Request(out var go, _assetKey, _position, _rotation, active: false);
		InstantiatePostProcess(go);
		return go;
	}
}
