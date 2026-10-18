using System;
using System.Collections.Generic;
using Brix.Components;
using Brix.Engine;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.AI.Nodes;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Brix.Game.Semantique;
using Brix.Lifecycle.Pooling;
using Brix.Pathfinding.Test;
using Brix.Utils;
using Brix.Utils.Closures;
using Brix.Utils.Factories;
using Motus.Behavior;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using UnityEngine;

namespace Brix.Game.AI.KnowledgeSpace;

public class Knowledge : MonoBehaviour
{
	[SerializeField]
	public KnowledgeUpdater knowledgeUpdater;

	[SerializeField]
	public KnowledgeExcluder knowledgeExcluder;

	[SerializeField]
	private KnowledgeReserver knowledgeReserver;

	private static Knowledge _instance;

	private static List<Factory.AssetKey> autoAdd = new List<Factory.AssetKey>
	{
		Brix.External.Factories.Test.Minitron,
		Brix.External.Factories.Test.Corruptron,
		Brix.External.Factories.Test.Biftron,
		Brix.External.Factories.Test.Bricktron
	};

	private static HashSet<Factory.AssetKey> storageObjects = new HashSet<Factory.AssetKey>
	{
		ObjetsDynamiques.Palette,
		ObjetsDynamiques.Toolrack
	};

	public static HashSet<Factory.AssetKey> InterestingResourceObject = new HashSet<Factory.AssetKey>
	{
		Blocks.Wood_DoublePlank_AA,
		Blocks.Stone_DoubleBrick,
		Blocks.Wood_DoubleLog_AA,
		ObjetsDynamiques.Palette,
		ObjetsDynamiques.Toolrack,
		ObjetsDynamiques.Furnace,
		ObjetsDynamiques.ChoppingBlock
	};

	private Dictionary<GameObject, List<KnowledgeRegion>> RegionsOfObjects => knowledgeUpdater.RegionsOf;

	public static Knowledge Instance => _instance;

	private void Awake()
	{
		if (_instance != null)
		{
			throw new Exception("There shall only be one");
		}
		_instance = this;
	}

	public IGameComponent ClosestToolFrom(Labor futureUser, Accessoire toolType, Faction faction)
	{
		Closure.FBase<Labor, IGameComponent, float> distanceFunc = KnowledgeUtility.GetBestToolDistance().Make();
		Closure.FClosed<Closure.FBase<IGameComponent, Labor, bool>, IGameComponent, Labor, bool> additionalCondition = Closure.Make<IGameComponent, Labor, bool>(_ClosestToolFrom_Test).Close(futureUser);
		return NearestValidObject(futureUser, faction, additionalCondition, distanceFunc, storageObjects);
	}

	private static bool _ClosestToolFrom_Test(IGameComponent gc, Labor futureUser)
	{
		return !futureUser.IsNullOrReleased() && futureUser.CanEquipRackContent(gc.GameObject);
	}

	public IGameComponent ClosestToolRackWithSpace(Labor futureUser, Accessoire toolType, Faction faction)
	{
		Closure.FBase<Labor, IGameComponent, float> distanceFunc = KnowledgeUtility.GetBestToolDistance().Make();
		Closure.FClosed<Closure.FBase<IGameComponent, Labor, bool>, IGameComponent, Labor, bool> additionalCondition = Closure.Make<IGameComponent, Labor, bool>(_CanStoreToolOnRack_Test).Close(futureUser);
		return NearestValidObject(futureUser, faction, additionalCondition, distanceFunc, storageObjects);
	}

	private static bool _CanStoreToolOnRack_Test(IGameComponent gc, Labor futureUser)
	{
		return !futureUser.IsNullOrReleased() && futureUser.CanStoreToolOnRack(gc.GameObject);
	}

	public bool KnowsAbout(GameObject go)
	{
		if (go.IsNullOrReleased())
		{
			return false;
		}
		return RegionsOf(go).Count > 0;
	}

	public Node SignalOutOfReach(GameObject A, GameObject B, VProperty accessProperty)
	{
		if (KnowsAbout(A) && KnowsAbout(B))
		{
			return knowledgeExcluder.ExcludeAndSpread(A, B, accessProperty);
		}
		return Node.Empty;
	}

	public Slinq<GameObject, ConcatContext<SelectSlinqContext<GameObject, HashSetSlinqContext<GameObject>, Factory.AssetKey, IListContext<Factory.AssetKey>>, GameObject, SelectContext<GameObject, KeyValuePair<IVolume, int>, PredicateContext<KeyValuePair<IVolume, int>, DictionaryContext<IVolume, int>>>>> GetAllInterestingGameObjects()
	{
		return (from kvp in TestPathfinding.Instance.AllParticipantVolumes.Slinq()
			where VolumeIsInteresting(kvp.Key)
			select kvp.Key.GameObject).Concat(autoAdd.Slinq().SelectMany((Factory.AssetKey k) => BrixSingleton<AutoList>.Instance.GetInstances(k).Slinq()));
	}

	public bool IsReserved(GameObject go)
	{
		return knowledgeReserver.IsReserved(go);
	}

	public void ReserveUntil(GameObject go, Signal until)
	{
		knowledgeReserver.ReserveUntil(go, until);
	}

	public void ReserveUntil<T>(GameObject go, Signal<T> until)
	{
		knowledgeReserver.ReserveUntil(go, until);
	}

	public void ResetExclusionsFor(GameObject go, float delay)
	{
		Neo.Relais(delegate
		{
			DelayedReset(go);
		}, delay, "Reset exclusions");
	}

	private void DelayedReset(GameObject go)
	{
		if (!Architecte.termine && !go.IsNullOrReleased())
		{
			knowledgeExcluder.ResetExclusionsFor(go);
		}
	}

	public void AllReacheableObjectsFrom(GameObject go, HashSet<IGameComponent> result)
	{
		foreach (KnowledgeRegion item in RegionsOf(go))
		{
			if (item == null)
			{
				continue;
			}
			foreach (KeyValuePair<IVolume, int> member in item.Members)
			{
				IGameComponent gameComponent = member.Key.AsComponent();
				if (!gameComponent.IsNullOrReleased() && !knowledgeExcluder.AreExclusive(go, gameComponent.GameObject, member.Value))
				{
					result.Add(gameComponent);
				}
			}
		}
	}

	public void ReacheableObjectsFromWithKey(IGameComponent source, HashSet<Factory.AssetKey> subset, HashSet<IGameComponent> result)
	{
		foreach (KnowledgeRegion item in RegionsOf(source.GameObject))
		{
			if (item == null)
			{
				continue;
			}
			foreach (Factory.AssetKey item2 in subset)
			{
				foreach (IVolume item3 in item.MembersWithKey(item2))
				{
					IGameComponent gameComponent = item3.AsComponent();
					if (!gameComponent.IsNullOrReleased() && !knowledgeExcluder.AreExclusive(source.GameObject, item3.GameObject, item.Members[item3]))
					{
						result.Add(gameComponent);
					}
				}
			}
		}
	}

	public void UniqueResourcesReacheableFrom(GameObject go, HashSet<Factory.AssetKey> result)
	{
		RegionsOf(go).Slinq().NotNull().ForEach(delegate(KnowledgeRegion r, HashSet<Factory.AssetKey> _result)
		{
			r.UniqueResources(_result);
		}, result);
	}

	public bool CanReach(IGameComponent source, IGameComponent destination, int accessIndicesMask = -1)
	{
		return ConnectedRegion(source, destination, accessIndicesMask) != null;
	}

	private KnowledgeRegion ConnectedRegion(IGameComponent source, IGameComponent destination, int accessIndicesMask = -1)
	{
		KnowledgeRegion result = null;
		foreach (KnowledgeRegion item in RegionsOf(source.GameObject))
		{
			IVolume volume = destination.volume;
			if (volume == null || !item.Members.TryGetValue(volume, out var value) || knowledgeExcluder.AreExclusive(source.GameObject, destination.GameObject, value & accessIndicesMask))
			{
				continue;
			}
			result = item;
			break;
		}
		return result;
	}

	public IGameComponent ClosestStorageWithRoomFor(Labor source, IGameComponent itemToStore, Faction faction)
	{
		Closure.FClosed<Closure.FBase<Labor, IGameComponent, IGameComponent, float>, Labor, IGameComponent, IGameComponent, float> distanceFunc = KnowledgeUtility.GetStorageDistance().Make().Close(itemToStore);
		Closure.FClosed<Closure.FBase<IGameComponent, IGameComponent, bool>, IGameComponent, IGameComponent, bool> additionalCondition = Closure.Make<IGameComponent, IGameComponent, bool>(_ClosestStorageWithRoomFor_Test).Close(itemToStore);
		return NearestValidObject(source, faction, additionalCondition, distanceFunc, storageObjects);
	}

	private static bool _ClosestStorageWithRoomFor_Test(IGameComponent gc, IGameComponent itemToStore)
	{
		return gc.WithRecepteur((Recepteur r, GameObject g) => r.Fonction == FonctionDeContenant.entrepot && r.HasRoomFor(g), _default: false, itemToStore.GameObject);
	}

	public IGameComponent ClosestObjectToPickup(Labor source, Ressource resource, Faction faction)
	{
		Closure.FBase<Labor, IGameComponent, float> distanceFunc = KnowledgeUtility.GetPickupDistance().Make();
		Closure.FClosed<Closure.FBase<IGameComponent, Ressource, bool>, IGameComponent, Ressource, bool> additionalCondition = Closure.Make<IGameComponent, Ressource, bool>(_ClosestObjectToPickup_Test).Close(resource);
		return NearestValidObject(source, faction, additionalCondition, distanceFunc, InterestingResourceObject);
	}

	private static bool _ClosestObjectToPickup_Test(IGameComponent gc, Ressource ressource)
	{
		return !gc.IsNullOrReleased() && ((Apparence.PrincipaleIs(gc, ressource) && !gc.ParentRecepteur) || (gc.recepteur != null && gc.recepteur.Carries(ressource) && NotInCraftingInventory(gc.recepteur.gameObject, ressource)));
	}

	private static bool NotInCraftingInventory(GameObject go, Ressource ressource)
	{
		CraftingStation component = go.GetComponent<CraftingStation>();
		return component == null || component.ProductionIs(ressource);
	}

	private IGameComponent NearestValidObject<TCondition, TDistance>(Labor source, Faction faction, TCondition additionalCondition, TDistance distanceFunc, HashSet<Factory.AssetKey> subset) where TCondition : struct, IFunc<IGameComponent, bool> where TDistance : struct, IFunc<Labor, IGameComponent, float>
	{
		HashSet<IGameComponent> hashSet = CollectionPool.RequestHashSet<IGameComponent>();
		ReacheableObjectsFromWithKey(source, subset, hashSet);
		IGameComponent result = ClosestMatchingObject(hashSet, source, faction, additionalCondition, distanceFunc);
		hashSet.Dispose();
		return result;
	}

	private IGameComponent ClosestMatchingObject<TCondition, TDistance>(HashSet<IGameComponent> knownObjects, Labor source, Faction faction, TCondition additionalCondition, TDistance distanceFunc) where TCondition : struct, IFunc<IGameComponent, bool> where TDistance : struct, IFunc<Labor, IGameComponent, float>
	{
		float num = float.MaxValue;
		IGameComponent result = null;
		foreach (IGameComponent knownObject in knownObjects)
		{
			if (Matches(knownObject, faction) && additionalCondition.Invoke(knownObject))
			{
				float num2 = distanceFunc.Invoke(source, knownObject);
				if (num2 < num)
				{
					num = num2;
					result = knownObject;
				}
			}
		}
		return result;
	}

	private bool Matches(IGameComponent gc, Faction faction)
	{
		return gc != null && Affiliation.IsSameOrNeutral(gc.faction, faction) && LocalFactionVision.ObjectIsVisibleForFaction(gc, faction);
	}

	private List<KnowledgeRegion> RegionsOf(GameObject go)
	{
		if (go.IsNullOrReleased())
		{
			return Empty.List<KnowledgeRegion>();
		}
		return RegionsOfObjects.GetValueOrDefault(go, Empty.List<KnowledgeRegion>());
	}

	public static bool VolumeIsInteresting(IVolume volume)
	{
		if (volume == null || (volume is Volume && (volume as Volume).IsNullOrReleased()))
		{
			return false;
		}
		return true;
	}

	public void PropagateBlueprintRegions(Blueprint from, IVolume to)
	{
		List<KnowledgeRegion> valueOrDefault = RegionsOfObjects.GetValueOrDefault(from.gameObject);
		if (valueOrDefault == null)
		{
			return;
		}
		int num = 0;
		VolumePropertyRequest cases = to.GetCases(VProperty.access);
		for (int i = 0; i < cases.Count; i++)
		{
			XYZ position = cases[i];
			if (from.volume.HasPropertyAt(VProperty.fantome, position))
			{
				num |= 1 << i;
			}
		}
		foreach (KnowledgeRegion item in valueOrDefault)
		{
			item.Add(to, num);
		}
		List<KnowledgeRegion> valueOrDefault2 = RegionsOfObjects.GetValueOrDefault(to.GameObject);
		if (valueOrDefault2 == null)
		{
			return;
		}
		foreach (KnowledgeRegion item2 in valueOrDefault)
		{
			if (!valueOrDefault2.Contains(item2))
			{
				valueOrDefault2.Add(item2);
			}
		}
	}
}
