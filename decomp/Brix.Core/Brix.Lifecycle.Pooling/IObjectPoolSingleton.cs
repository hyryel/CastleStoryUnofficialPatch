using System.Collections.Generic;
using Brix.External.Factories;
using Brix.External.Signals;
using UnityEngine;

namespace Brix.Lifecycle.Pooling;

public interface IObjectPoolSingleton
{
	Signal<Factory.AssetKey, GameObject> GameObjectRequestedSignal { get; }

	Signal<Factory.AssetKey, GameObject> GameObjectReleasedSignal { get; }

	Signal<Factory.AssetKey, GameObject> GameObjectBeforeReleasedSignal { get; }

	Dictionary<Factory.AssetKey, IGameObjectPool> IndexedPools { get; }

	void GeneratePools();

	IGameObjectPool GetGameObjectPool(Factory.AssetKey key);
}
