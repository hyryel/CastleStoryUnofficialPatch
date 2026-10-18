using System.Collections.Generic;
using Brix.External.Factories;
using Brix.Utils;
using Smooth.Slinq;

namespace Brix.Game.AI;

public abstract class StorableGoal : Goal
{
	public abstract void UniqueItems(ref HashSet<Factory.AssetKey> keys);

	protected bool CouldStore(Labor labor)
	{
		HashSet<Factory.AssetKey> keys = CollectionPool.RequestHashSet<Factory.AssetKey>();
		UniqueItems(ref keys);
		bool result = keys.Count == 0 || keys.Slinq().Any((Factory.AssetKey k, Labor _labor) => CanTakeAndStore(_labor, k), labor);
		keys.Dispose();
		return result;
	}

	private bool CanTakeAndStore(Labor labor, Factory.AssetKey assetKey)
	{
		return labor.recepteur.HasRoomFor(assetKey) && (bool)labor.BestRecepteurToStore(Factory.Peek(assetKey));
	}
}
