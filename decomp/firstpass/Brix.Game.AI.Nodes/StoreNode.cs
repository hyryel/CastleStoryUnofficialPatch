using System;
using Brix.Game.AI.KnowledgeSpace;
using Brix.Game.Components;
using Motus.Behavior;
using UnityEngine;

namespace Brix.Game.AI.Nodes;

public static class StoreNode
{
	public static Node GoStoreIn(this Labor labor, GameComponent contenant)
	{
		return (StaticNode)Node.Sequence.Do((Func<Labor, GameComponent, Node>)NavigateAndCheck, labor, contenant).Do((Func<Labor, GameComponent, StaticNode>)StoreIn, labor, contenant).Label("StoreIn");
	}

	public static Node NavigateAndCheck(this Labor labor, GameComponent contenant)
	{
		if (labor.IsNullOrReleased() || contenant.IsNullOrReleased())
		{
			return Node.FailWork<Exceptions.NullTarget>();
		}
		return Node.Do((Labor aLabor, GameComponent aContenant) => aLabor.RunToExactly(Location.Of(aContenant)), labor, contenant).FailWhen(SharedWorkException<Exceptions.NullStorage>.Get).Condition(GameComponentExtender.IsNullOrReleased, contenant)
			.FailWhen(SharedWorkException<Exceptions.InventoryBecameEmpty>.Get)
			.Condition((Labor aLabor) => aLabor.recepteur.IsEmpty(), labor)
			.FailWhen(SharedWorkException<Exceptions.StorageBecameFull>.Get)
			.Condition((GameComponent aContenant) => aContenant.recepteur.Fonction == FonctionDeContenant.entrepot && aContenant.recepteur.IsFull(), contenant);
	}

	public static StaticNode StoreIn(this Labor labor, GameComponent contenant)
	{
		return Node.Sequence.Do(delegate(Labor aLabor, GameComponent aContenant)
		{
			aLabor.transaction.StoreEverything(aLabor.gameObject, aContenant.gameObject);
		}, labor, contenant).Do(Node.WaitFor.Ticks(2));
	}

	public static StaticNode GoStoreInBestStorage(this Labor labor)
	{
		Sequencer.Builder b = Node.Sequence.Do((Labor aLabor) => aLabor.BestRecepteurToStoreInventory(), labor).Process((GameObject aDestinationGo, Labor aLabor) => (!(aDestinationGo == null)) ? aLabor.GoStoreIn(aDestinationGo.GetComponent<GameComponent>()) : Node.FailWork<Exceptions.CouldNotFindStorageForInventory>(), labor);
		return b.Loop().Until((Labor aLabor) => aLabor.recepteur.IsEmpty(), labor).ToNode;
	}

	public static void SetTaskTarget(Labor labor, GameObject destination)
	{
		labor.CurrentTask.Target = destination;
	}

	public static StaticNode GoStoreInBestStorageOrDrop(this Labor labor, Location fallback)
	{
		return Node.Catcher.Do(labor.GoStoreInBestStorage()).Catch((Exceptions.CouldNotFindStorageForInventory e, Labor _labor, Location _fallback) => _labor.GoDropAtExactly(_fallback, canFail: false).AndFail(e), labor, fallback).Catch((Exceptions.PathUnfindable e, Labor _labor, Location _fallback) => _labor.GoDropAtExactly(_fallback, canFail: false).AndFail(e), labor, fallback);
	}

	public static StaticNode NestedGoStoreInBestStorageOrDrop(this Labor labor, int count = 0)
	{
		return Node.Selector.If((int _count) => _count >= 5, count).Do((Func<Labor, StaticNode>)DropNode.Drop, labor).If((int _count) => _count == 0, count)
			.Do((Func<Labor, int, StaticNode>)TryNestedGoStoreInBestStorageOrDrop, labor, count)
			.Else()
			.Do(delegate(Labor _labor)
			{
				Knowledge.Instance.ResetExclusionsFor(_labor.gameObject, 0.5f);
			}, labor)
			.Do(Node.WaitForMilliseconds(1000 * count))
			.Do((Func<Labor, int, StaticNode>)TryNestedGoStoreInBestStorageOrDrop, labor, count);
	}

	private static StaticNode TryNestedGoStoreInBestStorageOrDrop(this Labor labor, int count = 0)
	{
		return Node.Catcher.Do((Func<Labor, StaticNode>)GoStoreInBestStorage, labor).Catch((Exceptions.CouldNotFindStorageForInventory e, Labor _labor, int _count) => _labor.NestedGoStoreInBestStorageOrDrop(_count + 1).AndFail(e), labor, count);
	}
}
