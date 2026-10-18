using System;
using System.Collections.Generic;
using Brix.Common.Collections;
using Brix.Engine;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Brix.Game.Interfaces;
using Brix.Game.Network;
using Brix.Legacy;
using Brix.Lifecycle.Pooling;
using Brix.Lua;
using Brix.Transactions;
using Brix.Utils;
using Brix.Utils.Collections;
using Brix.Utils.Factories;
using Brix.Utils.Logging;
using MoonSharp.Interpreter;
using Newtonsoft.Json;
using Smooth.Algebraics;
using Smooth.Collections;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace Brix.Game.Semantique;

public class Recepteur : GameComponent, IWithResourceExclusions, IResourceTracker, IRecepteur, IObjectTracker
{
	[SerializeField]
	private LuaDriver Ld;

	private Ressource previousResource;

	private static BrixLogger logger = new BrixLogger(typeof(Recepteur).FullName);

	private Signal _changed = new Signal();

	private Signal neverCalledSignal = new Signal();

	[LuaDrivable("hk_OnNewRecepteur")]
	private static readonly LuaHook<LuaDriver> lua_OnNewRecepteur = new LuaHook<LuaDriver>();

	[LuaDrivable("hk_OnRecepteurInventoryChanged")]
	private static readonly LuaHook<LuaDriver> lua_OnRecepteurInventoryChanged = new LuaHook<LuaDriver>();

	[LuaDrivable("hk_OnRecepteurDestroyed")]
	private static readonly LuaHook<LuaDriver> lua_OnRecepteurDestroyed = new LuaHook<LuaDriver>();

	private readonly Description _selfDescription = new Description();

	private readonly Description _contentDescription = new Description();

	[SerializeField]
	[HideInInspector]
	[FormerlySerializedAs("description")]
	private Description _baseCapacity = new Description();

	private readonly Description _baseCapacityWithExclusions = new Description();

	private readonly Description _currentCapacity = new Description();

	[JsonProperty]
	[SerializeField]
	[FormerlySerializedAs("exclusions")]
	[HideInInspector]
	private DicoAdjectif _exclusions = new DicoAdjectif();

	[SerializeField]
	[FormerlySerializedAs("AcceptMultipleResouceType")]
	private bool _acceptMultipleResouceType;

	[SerializeField]
	[FormerlySerializedAs("fonction")]
	private FonctionDeContenant _fonction;

	private ToolRack _rack;

	[SerializeField]
	private RecepteurState state;

	private Signal<List<GameObject>> _onItemsAddedSignal = new Signal<List<GameObject>>();

	public Signal<GameObject> OnReceive = new Signal<GameObject>();

	public Signal InventoryChanged = new Signal();

	public Signal<IDescriptor> OnResourceAdded = new Signal<IDescriptor>
	{
		MaximumConnections = 1000
	};

	public Signal<IDescriptor> OnResourceRemoved = new Signal<IDescriptor>
	{
		MaximumConnections = 1000
	};

	public Signal<Ressource> ExclusionChanged = new Signal<Ressource>();

	public Signal<Ressource> OnExclusionAdded = new Signal<Ressource>();

	public Signal<Ressource> OnExclusionRemoved = new Signal<Ressource>();

	[SerializeField]
	[FormerlySerializedAs("container")]
	private Transform _container;

	private bool _excludeAll;

	private Ressource _exclusiveResource;

	[SerializeField]
	private bool storeOnly;

	public bool TakeOnly;

	public bool ShowInStorageManagement = true;

	private HashSet<IDescriptor> _storedItems = new HashSet<IDescriptor>();

	private readonly HashSet<Factory.AssetKey> _assetKeySet = new HashSet<Factory.AssetKey>();

	private Transactor transactor;

	private Profile _profile;

	private Profile _profilePerso;

	public static HashSet<Adjectif> BagStuff = new HashSet<Adjectif>
	{
		Adjectif.stones,
		Adjectif.orangeCrystal,
		Adjectif.blueCrystal,
		Adjectif.iron
	};

	private SyncListString NetworkExclusionList;

	public Ressource _queuedAddedExclusion;

	public Ressource _queuedRemovedExclusion;

	private HashSet<GameObject> toScatter = new HashSet<GameObject>();

	private bool _inventoryChanged;

	public IReadOnlySignal Changed => _changed;

	public Signal ConvertedResources
	{
		get
		{
			ResourceConverter component = GetComponent<ResourceConverter>();
			if (component == null)
			{
				return neverCalledSignal;
			}
			return component.ConvertedResources;
		}
	}

	public Description SelfDescription => _selfDescription;

	public Description ContentDescription => _contentDescription;

	public Description BaseCapacity => _baseCapacity;

	public Description BaseCapacityWithExclusions => _baseCapacityWithExclusions;

	public Description CurrentCapacity => _currentCapacity;

	public DicoAdjectif Exclusions => _exclusions;

	public bool AcceptMultipleResourceType
	{
		get
		{
			return _acceptMultipleResouceType;
		}
		set
		{
			if (value != _acceptMultipleResouceType)
			{
				_acceptMultipleResouceType = value;
				RegenerateDescriptionsWithExclusions();
			}
		}
	}

	public FonctionDeContenant Fonction => _fonction;

	public IReadOnlySignal<List<GameObject>> ItemsAddedSignal => _onItemsAddedSignal;

	public Transform Container => _container;

	public bool ExcludeAll
	{
		get
		{
			return (!(state != null)) ? _excludeAll : state.ExcludeAll;
		}
		set
		{
			if ((bool)state)
			{
				state.NetworkExcludeAll = value;
			}
			else
			{
				_excludeAll = value;
			}
		}
	}

	public Ressource ExclusiveRessource
	{
		get
		{
			return (!(state != null)) ? _exclusiveResource : state.ExclusiveResource;
		}
		set
		{
			if ((bool)state)
			{
				state.ExclusiveResource = value;
			}
			else
			{
				_exclusiveResource = value;
			}
		}
	}

	public HashSet<IDescriptor> StoredItems => _storedItems;

	public bool ExcludingAll => ExcludeAll;

	public Slinq<GameObject, SelectContext<GameObject, IDescriptor, HashSetSlinqContext<IDescriptor>>> StoredGameObjectsSlinq => from d in _storedItems.Slinq()
		select d.GameObject;

	public ProfileReceveur ProfileReceveur
	{
		get
		{
			if (IsEmpty())
			{
				return null;
			}
			List<GameObject> list = StoredGameObjectsSlinq.ToPooledList();
			ProfileReceveur result = new ProfileReceveur(list);
			list.Dispose();
			return result;
		}
	}

	public static void RegisterClass(Script script)
	{
		Table table = new Table(script);
		table["hk_OnNewRecepteur"] = lua_OnNewRecepteur;
		table["hk_OnRecepteurInventoryChanged"] = lua_OnRecepteurInventoryChanged;
		table["hk_OnRecepteurDestroyed"] = lua_OnRecepteurDestroyed;
		script.Globals["Recepteur"] = table;
	}

	private static void OnBeforeReleased(Factory.AssetKey assetKey, GameObject go)
	{
		if (assetKey == ObjetsDynamiques.Palette)
		{
			Recepteur component = go.GetComponent<Recepteur>();
			LuaHookBase.InvokeFromCLR(lua_OnRecepteurDestroyed, component.Ld);
		}
	}

	public void UniqueStoredItemsKeys(HashSet<Factory.AssetKey> results)
	{
		(from i in StoredItems.Slinq()
			select i.AssetKey).AddTo(results);
	}

	public void RandomUniqueStoredItems(ICollection<IDescriptor> results)
	{
		List<IDescriptor> list = CollectionPool.RequestList<IDescriptor>();
		list.AddRange(StoredItems);
		list.Shuffle();
		UniqueItems(list, results);
		list.Dispose();
	}

	private void UniqueItems(List<IDescriptor> items, ICollection<IDescriptor> results)
	{
		_assetKeySet.Clear();
		foreach (IDescriptor item in items)
		{
			Factory.AssetKey assetKey = item.AssetKey;
			if (_assetKeySet.Add(assetKey))
			{
				results.Add(item);
			}
		}
	}

	public bool ContainsSomeOf(Factory.AssetKey key)
	{
		return StoredItems.Slinq().Any((IDescriptor d, Factory.AssetKey aKey) => d.AssetKey == aKey, key);
	}

	public bool ContainsSomeOf(HashSet<Factory.AssetKey> keys)
	{
		return StoredItems.Slinq().Any((IDescriptor d, HashSet<Factory.AssetKey> aKeys) => aKeys.Contains(d.AssetKey), keys);
	}

	public void Add(GameComponent item)
	{
		AddObject(item.gameObject);
	}

	public void Add(IDescriptor descriptor)
	{
		AddObject(descriptor.GameObject);
	}

	public virtual void AddObject(GameObject item)
	{
		AddObject(item, keepWorldPosition: true);
	}

	public void AddObject(GameObject item, bool keepWorldPosition)
	{
		if (item.transform.parent == _container)
		{
			logger.Warn($"The GameObject '{item.name}' was re-added to the recepteur '{base.gameObject.name}' while already in it.");
			return;
		}
		if (item.GetComponent<IDescriptor>() == null)
		{
			throw new ArgumentException(string.Format("The GameObject '{0}' couldn't be added to recepteur '{1}'. \n'{0}' has no Component implementing an IDescriptor.", item.name, base.gameObject.name));
		}
		Add_Unsafe(item, keepWorldPosition);
	}

	private void Add_Unsafe(GameObject item, bool keepWorldPosition)
	{
		ToolParentTracker component = item.GetComponent<ToolParentTracker>();
		if ((bool)component)
		{
			component.SetTrackerType(TrackingTypes.ressource);
		}
		item.transform.SetParent(_container, keepWorldPosition);
	}

	public bool HasComplexRecipe()
	{
		if (base.recepteur.BaseCapacity.Adjectifs.Skip(1).Any())
		{
			return true;
		}
		if (base.recepteur.BaseCapacity.Adjectifs.Any((Adjectif a) => !a.GetKey().GetInfo().baseType))
		{
			return true;
		}
		if (base.recepteur.BaseCapacity.Adjectifs.Any((Adjectif a) => a.quantifiable.valeur > 1))
		{
			return true;
		}
		return false;
	}

	public void Wipe()
	{
		List<GameObject> list = (from d in _storedItems.Slinq()
			select d.GameObject).ToPooledList();
		list.Slinq().ForEach(delegate(GameObject go)
		{
			go.SafeNetworkDestroy();
		});
		_storedItems.Clear();
	}

	public void WipeAndFill(Ressource r)
	{
		Wipe();
		int count = _baseCapacity.Value(r);
		Factory.AssetKey assetKey = Description.RepresentativeOf(r);
		(from key in Slinqable.Repeat(assetKey, count)
			select ObjectPoolSingleton.Request_Unsafe(key, active: true)).ForEach(delegate(GameObject go)
		{
			AddObject(go);
			NetworkServer.Spawn(go, assetKey.AssetId);
		});
	}

	public void WipeAndFill<T>() where T : Ressource, new()
	{
		WipeAndFill(new T());
	}

	public void DropAll()
	{
		List<GameObject> list = StoredGameObjectsSlinq.ToPooledList();
		DropAll(list);
		list.Dispose();
	}

	private void DropAll(List<GameObject> objects)
	{
		FirstObject(out var go);
		Vector3 vector = VolumeRegistry.ClosestAvailableDropPosition(base.transform, objects.Count, go);
		if (!(vector != Vector3.zero))
		{
			return;
		}
		ClasseMoi.Move(objects);
		if (objects.Count == 1)
		{
			if ((bool)objects[0].GetComponent<BaseVoxelTransform>())
			{
				objects[0].GetComponent<BaseVoxelTransform>().position = XYZ.FromVector3(vector);
			}
			else
			{
				objects[0].transform.position = vector;
			}
		}
	}

	public static void ScatterObjects(IEnumerable<GameObject> objects, Vector3 center)
	{
		SphereCollection sphereCollection = new SphereCollection();
		Dictionary<SphereCollection.Sphere, Transform> dictionary = new Dictionary<SphereCollection.Sphere, Transform>();
		foreach (GameObject @object in objects)
		{
			if (!@object.IsNullOrReleased() && @object.GetComponent<GenericParentTracker>().IsOnGround())
			{
				if (@object.GetComponent<ISoulevable>() != null && !@object.GetComponent<Garnotte>())
				{
					@object.transform.position = center;
				}
				else if (@object.GetComponent<Collider>() != null)
				{
					float smallestContainingRadius_Unscaled = ColliderUtils.GetSmallestContainingRadius_Unscaled(@object.GetComponent<Collider>());
					dictionary[sphereCollection.Add(smallestContainingRadius_Unscaled)] = @object.transform;
				}
				else
				{
					@object.transform.position = center;
				}
				if ((bool)@object.GetComponent<Garnotte>())
				{
					@object.transform.rotation = UnityEngine.Random.rotation;
				}
				@object.Deposer();
				@object.SetActive(value: true);
			}
		}
		sphereCollection.Shuffle(center, avoidCollisions: true, (Vector3 p) => Voxel.IsEmpty(XYZ.FromVector3(p)));
		foreach (KeyValuePair<SphereCollection.Sphere, Transform> item in dictionary)
		{
			item.Value.position = item.Key.center;
		}
	}

	public void SwitchContentWith(Recepteur recepteurB)
	{
		List<GameObject> list = StoredGameObjectsSlinq.ToPooledList();
		List<GameObject> list2 = recepteurB.StoredGameObjectsSlinq.ToPooledList();
		list.Slinq().Concat(list2.Slinq()).ForEach(delegate(GameObject go)
		{
			go.transform.parent = null;
		});
		for (int num = 0; num < list.Count; num++)
		{
			recepteurB.AddObject(list[num]);
		}
		for (int num2 = 0; num2 < list2.Count; num2++)
		{
			AddObject(list2[num2]);
		}
		list.Dispose();
		list2.Dispose();
	}

	public void RebuildVisionForStoredObjects()
	{
		StoredGameObjectsSlinq.ForEach(delegate(GameObject storedObject)
		{
			NetworkIdentity component2 = storedObject.GetComponent<NetworkIdentity>();
			if ((bool)component2)
			{
				component2.RebuildObservers(initialize: false);
			}
		});
		Toolbag component = GetComponent<Toolbag>();
		if ((bool)component)
		{
			component.UpdateToolsVision();
		}
	}

	public Profile Profile(bool plusCharacter = false)
	{
		if (plusCharacter)
		{
			return _profilePerso ?? (_profilePerso = new ProfilePlusPersonnage(CurrentCapacity));
		}
		return _profile ?? (_profile = new Profile(CurrentCapacity));
	}

	public Tuple<int, int> GetProportionEncombrement()
	{
		int t = _baseCapacity.Value(Adjectif.encombrement);
		int t2 = _contentDescription.Value(Adjectif.encombrement);
		return Tuple.Create(t, t2);
	}

	public float GetProportionUsed()
	{
		return GetProportionUsed(FirstResource());
	}

	public float GetProportionUsed(Quantite q)
	{
		if (q == null)
		{
			return 0f;
		}
		int num = _baseCapacity.Value(q);
		return (num != 0) ? ((float)_contentDescription.Value(q) / (float)num) : 0f;
	}

	public int GetSpaceFilled()
	{
		return _contentDescription.Value<Encombrement>();
	}

	public bool IsFull()
	{
		return _currentCapacity.NoneOrZeros();
	}

	public bool IsFull(Quantite q)
	{
		Quantite quantite = _currentCapacity.Get(q);
		return quantite == null || quantite.valeur == 0;
	}

	public bool IsFull<T>() where T : Quantite
	{
		T val = _currentCapacity.Get<T>();
		return val == null || val.valeur == 0;
	}

	public bool IsEmpty()
	{
		return _storedItems.Count == 0;
	}

	public bool CarriesSomething()
	{
		return _storedItems.Count != 0;
	}

	public bool Carries(GameObject thing)
	{
		return thing.WithComponent((Apparence a, Recepteur r) => r._storedItems.Contains(a), _default: false, this);
	}

	public bool Carries(Ressource thing)
	{
		Type type = thing.GetType();
		foreach (IDescriptor storedItem in _storedItems)
		{
			Ressource principale = storedItem.Principale;
			if (principale != null && principale.GetType() == type)
			{
				return true;
			}
		}
		return false;
	}

	public bool CarriesSomethingElseThan(Adjectif adjective)
	{
		return _storedItems.Slinq().Any((IDescriptor d, Adjectif aAdjective) => !d.Description.Adjectifs.Contains(aAdjective), adjective);
	}

	public bool CarriesSomethingElseThan(Description things)
	{
		return things.Adjectifs.Any((Adjectif thing, Recepteur self) => self.CarriesSomethingElseThan(thing), this);
	}

	public bool Carries(GameComponent gameComponent)
	{
		return gameComponent != null && _storedItems.Slinq().Any((IDescriptor x, GameComponent aGameComponent) => x.GameObject == aGameComponent.gameObject, gameComponent);
	}

	public bool RequiresBothHands()
	{
		Ressource ressource = ContentDescription.GetRessources().FirstOrDefault();
		if (ressource == null)
		{
			return false;
		}
		Factory.AssetKey prop = this.GetProp(ressource);
		return prop != Accessories.Bag;
	}

	public int CanFitUpTo(Factory.AssetKey key)
	{
		if (IsFull() || key == null)
		{
			return 0;
		}
		Ressource adjectif = Apparence.PrincipaleOf(Factory.Peek(key));
		return CurrentCapacity.Value(adjectif);
	}

	public bool HasRoomFor(Factory.AssetKey key)
	{
		return HasRoomFor((IDescriptor)Factory.Peek(key).GetComponent<Apparence>());
	}

	public bool HasBaseRoomFor(Factory.AssetKey key)
	{
		return BaseCapacity.PeutPrendre((IDescriptor)Factory.Peek(key).GetComponent<Apparence>());
	}

	public bool HasRoomForOneOf(IEnumerable<GameObject> gos)
	{
		return gos.Slinq().Any((GameObject go) => HasRoomFor(go));
	}

	public bool HasRoomForOneOf(IEnumerable<Factory.AssetKey> keys)
	{
		return keys.Slinq().Any((Factory.AssetKey k) => HasRoomFor(k));
	}

	public bool HasOneOf(HashSet<Factory.AssetKey> keys)
	{
		HashSet<Factory.AssetKey> hashSet = CollectionPool.RequestHashSet<Factory.AssetKey>();
		UniqueStoredItemsKeys(hashSet);
		bool result = hashSet.Slinq().Intersect(keys.Slinq()).Any();
		hashSet.Dispose();
		return result;
	}

	public bool HasRoomForOneOf(IEnumerable<IDescriptor> descriptors)
	{
		return descriptors.Slinq().Any((IDescriptor d, Recepteur self) => self.HasRoomFor(d), this);
	}

	public bool HasBaseCapacityForOneOf(HashSet<Factory.AssetKey> keys)
	{
		return keys.Slinq().Any((Factory.AssetKey k, Recepteur r) => r.HasRoomFor(k), this);
	}

	public bool HasRoomForSomeOf(GameObject container)
	{
		Recepteur component = container.GetComponent<Recepteur>();
		if (!base.recepteur)
		{
			return false;
		}
		return HasRoomForSomeOf(component);
	}

	public bool HasRoomForSomeOf(Recepteur container)
	{
		if (container.storeOnly)
		{
			return false;
		}
		return container.StoredItems.Slinq().Any((IDescriptor go, Recepteur aRecepteur) => aRecepteur.HasRoomFor(go), this);
	}

	public bool HasRoomForSomeOf(Toolbag bag)
	{
		List<GameObject> result = CollectionPool.RequestList<GameObject>();
		bag.Tools(ref result);
		bool result2 = result.Slinq().Any((GameObject go, Recepteur aRecepteur) => aRecepteur.HasRoomFor(go), this);
		result.Dispose();
		return result2;
	}

	public bool HasRoomForAllOf(Toolbag bag)
	{
		List<GameObject> result = CollectionPool.RequestList<GameObject>();
		bag.Tools(ref result);
		bool result2 = result.Slinq().All((GameObject go, Recepteur aRecepteur) => aRecepteur.HasRoomFor(go), this);
		result.Dispose();
		return result2;
	}

	public bool FirstObject(out GameObject go)
	{
		go = StoredGameObjectsSlinq.FirstOrDefault();
		return go != null;
	}

	public string ContentString()
	{
		if (FirstObject(out var go))
		{
			return (!Apparence.GoesInBag(go)) ? Apparence.PrincipaleOf(go).ToString() : "Fill";
		}
		return "Empty";
	}

	public Ressource FirstResource()
	{
		Ressource result = null;
		if (FirstObject(out var go))
		{
			result = Apparence.PrincipaleOf(go);
		}
		return result;
	}

	public Ressource GetContainedResourceType()
	{
		Ressource ressource = null;
		FirstObject(out var go);
		return Apparence.PrincipaleOf(go);
	}

	public GameObject FirstObjectOf(Profile profile)
	{
		return StoredGameObjectsSlinq.Where((GameObject o, Tuple<Recepteur, Profile> p) => p._2.MatchGameObject(o, p._1), Tuple.Create(this, profile)).FirstOrDefault();
	}

	private Slinq<GameObject, PredicateContext<GameObject, SelectContext<GameObject, IDescriptor, HashSetSlinqContext<IDescriptor>>, Ressource>> _ObjectsOf(Ressource adj)
	{
		return StoredGameObjectsSlinq.Where(Apparence.PrincipaleIs, adj);
	}

	public GameObject FirstObjectOf(Description desc)
	{
		return desc.GetRessources().SelectMany((Ressource a, Recepteur r) => r._ObjectsOf(a), this).FirstOrDefault();
	}

	public bool FirstObjectOf(Ressource adj, out GameObject go)
	{
		go = _ObjectsOf(adj).FirstOrDefault();
		return go != null;
	}

	public GameObject FirstObjectOf(Ressource adj)
	{
		return _ObjectsOf(adj).FirstOrDefault();
	}

	public bool FirstObjectOf(HashSet<Factory.AssetKey> keys, out GameObject go)
	{
		go = StoredGameObjectsSlinq.Where((GameObject o, HashSet<Factory.AssetKey> _keys) => _keys.Contains(o.GetComponent<GameComponent>().AssetKey), keys).FirstOrDefault();
		return go != null;
	}

	public bool FirstObjectOf(Factory.AssetKey key, out GameObject go)
	{
		go = StoredGameObjectsSlinq.Where((GameObject o, Factory.AssetKey _key) => o.GetComponent<GameComponent>().AssetKey == _key, key).FirstOrDefault();
		return go != null;
	}

	public bool Has(Factory.AssetKey key)
	{
		HashSet<Factory.AssetKey> hashSet = CollectionPool.RequestHashSet<Factory.AssetKey>();
		UniqueStoredItemsKeys(hashSet);
		bool result = hashSet.Contains(key);
		hashSet.Dispose();
		return result;
	}

	public bool HasRoomFor(GameObject o)
	{
		if (o.IsNullOrReleased())
		{
			return false;
		}
		if (Carries(o))
		{
			return true;
		}
		return _currentCapacity.PeutPrendre(o);
	}

	public bool HasRoomFor(GameComponent o)
	{
		return _currentCapacity.PeutPrendre(o);
	}

	public bool HasRoomFor(IDescriptor d)
	{
		return _currentCapacity.PeutPrendre(d);
	}

	public bool HasRoomFor(Description d)
	{
		return _currentCapacity.PeutPrendre(d);
	}

	public Factory.AssetKey CarryType()
	{
		Factory.AssetKey result = null;
		if (FirstObject(out var go))
		{
			Ressource adj = Apparence.PrincipaleOf(go);
			result = this.GetProp(adj);
		}
		return result;
	}

	public static bool AFitsInB(GameObject A, GameObject B)
	{
		Recepteur component = B.GetComponent<Recepteur>();
		if (!component)
		{
			return false;
		}
		return component.HasRoomFor(A);
	}

	public bool HasRoomInBag()
	{
		return IsEmpty() || (!IsFull() && !StoredItems.Slinq().Any((IDescriptor a) => !a.Description.Has<Granulaire>()));
	}

	public static bool CharactersCouldGrabFrom(GameObject container)
	{
		Recepteur component = container.GetComponent<Recepteur>();
		if (!component)
		{
			return false;
		}
		return !component.IsEmpty() && !component.storeOnly;
	}

	public GameObject FirstThatFitsIn(Recepteur storage)
	{
		return StoredItems.Slinq().Where((IDescriptor s, Recepteur _storage) => _storage.HasRoomFor(s), storage).FirstOrDefault()?.GameObject;
	}

	public bool SomeContentFitsIn(Recepteur storage)
	{
		return FirstThatFitsIn(storage) != null;
	}

	public bool SomeContentFitsIn(GameObject B)
	{
		return B.WithComponent((Recepteur r, Recepteur self) => self.SomeContentFitsIn(r), _default: false, this);
	}

	public bool HasSomeResourceFor(Description ingredients)
	{
		return ingredients.DicoAdjectif.Slinq().Any((KeyValuePair<Type, Adjectif> kvp, Description d) => d.HasPresenceOrValue(kvp.Value), ContentDescription);
	}

	private Tuple<bool, float> ComputePresenceRatio(Ressource r)
	{
		float num = BaseCapacityWithExclusions.Value(r);
		float num2 = ContentDescription.Value(r);
		return Tuple.Create(num2 > 0f, (!(num > 0f)) ? 0f : (num2 / num));
	}

	public float PickupDistanceBonus()
	{
		Ressource ressource = FirstResource();
		if (ressource == null)
		{
			return 0f;
		}
		Tuple<bool, float> tuple = ComputePresenceRatio(ressource);
		return PickupBonuses.ForPickingUp(ressource, tuple._1, 1f - tuple._2);
	}

	public float StoreDistanceBonusFor(Ressource item)
	{
		if (DistanceHackForToolRack(out var bonus, item, pickup: false))
		{
			return bonus;
		}
		Ressource ressource = FirstResource();
		if (ressource == null)
		{
			return 0f;
		}
		Tuple<bool, float> tuple = ComputePresenceRatio(ressource);
		return PickupBonuses.ForStoring(ressource, tuple._1, tuple._2);
	}

	private bool DistanceHackForToolRack(out float bonus, Ressource item, bool pickup)
	{
		bonus = -1f;
		if (item is Outil && (bool)_rack)
		{
			bonus = ((!pickup) ? _rack.StoreBonusFor((Outil)item) : _rack.PickUpBonusFor((Outil)item));
		}
		return bonus != -1f;
	}

	protected override void Start()
	{
		base.Start();
		ResetDescriptions();
		base.gameObject.WithComponent(delegate(ToolRack tr)
		{
			tr.RefreshExclusivity();
		});
		LuaHookBase.InvokeFromCLR(lua_OnNewRecepteur, Ld);
	}

	public override void Initialize()
	{
		base.Initialize();
		_rack = GetComponent<ToolRack>();
		if (_container == null && _container == null)
		{
			_container = base.transform;
		}
		InventoryChanged.Connect(delegate(Recepteur aRecepteur)
		{
			aRecepteur._changed.Invoke();
		}, this);
		ExclusionChanged.Connect(delegate(Ressource resource, Recepteur aRecepteur)
		{
			aRecepteur._changed.Invoke();
		}, this);
		if ((bool)state)
		{
			state.Changed.Connect(delegate(Recepteur resource)
			{
				resource.ExclusionChanged.Invoke(null);
			}, this);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Wipe();
		ItemsAddedSignal.Clear();
		OnResourceAdded.Clear();
		OnResourceRemoved.Clear();
	}

	public void ResetDescriptions()
	{
		_contentDescription.Clear();
		StoredItems.Slinq().ForEach(delegate(IDescriptor d, Recepteur self)
		{
			self._contentDescription.AddAllFrom(d.Description);
		}, this);
		_baseCapacityWithExclusions.CopyQuantifiablesFrom(_baseCapacity);
		_baseCapacityWithExclusions.ExceptWith(_exclusions);
		_currentCapacity.CopyQuantifiablesFrom(_baseCapacityWithExclusions);
		_currentCapacity.Subtract(_contentDescription);
		if (!_acceptMultipleResouceType && !IsEmpty())
		{
			_currentCapacity.IntersectWith(_contentDescription);
		}
		if (ExcludeAll)
		{
			DoExcludeAll();
		}
		RegenerateDescriptionsWithExclusions();
	}

	private Ressource GetFirstResource()
	{
		return (from kvp in ContentDescription.DicoAdjectif.Slinq()
			select kvp.Value as Ressource into adj
			where adj != null
			select adj).FirstOrDefault();
	}

	private void RegenerateDescriptionsWithExclusions()
	{
		if (AssetKey == ObjetsDynamiques.Palette && !_exclusions.ContainsKey(Adjectif.outil.GetType()))
		{
			_exclusions.Add(Adjectif.outil);
		}
		_baseCapacityWithExclusions.CopyQuantifiablesFrom(_baseCapacity);
		_baseCapacityWithExclusions.ExceptWith(_exclusions);
		_currentCapacity.CopyQuantifiablesFrom(_baseCapacityWithExclusions);
		_currentCapacity.Subtract(_contentDescription);
		if (!_acceptMultipleResouceType && !IsEmpty())
		{
			_currentCapacity.IntersectWith(_contentDescription);
		}
	}

	public void ToggleExclusion(Ressource r)
	{
		if (Exclusions.Slinq().Any())
		{
			Exclusions.Clear();
			return;
		}
		foreach (Adjectif item in BaseCapacity.Adjectifs.Where((Adjectif a, Ressource _r) => a is Ressource && a.GetType() != _r.GetType(), r).NoLeak())
		{
			AddExclusion(item as Ressource);
		}
	}

	public bool IsExcluding(Ressource r)
	{
		if (_queuedAddedExclusion != null && r.Equals(_queuedAddedExclusion))
		{
			return true;
		}
		if (_queuedRemovedExclusion != null && r.Equals(_queuedRemovedExclusion))
		{
			return false;
		}
		return _exclusions.Contains(r);
	}

	public bool HasExclusions()
	{
		return ExclusiveRessource != null || ExcludeAll;
	}

	public void MakeExclusive(IEnumerable<Ressource> ressources)
	{
		ClearExclusions();
		List<Ressource> list = ressources.Slinq().ToPooledList();
		ExclusiveRessource = list.Slinq().First();
		foreach (Ressource item in BaseCapacity.GetRessources().NoLeak())
		{
			if (!list.Slinq().Any((Ressource r, Ressource a) => a.GetType() == r.GetType(), item))
			{
				AddExclusion(item);
			}
		}
		ExclusionChanged.Invoke(null);
		list.Dispose();
	}

	public void ClearExclusions()
	{
		ExcludeAll = false;
		ExclusiveRessource = null;
		_exclusions.Clear();
		_queuedAddedExclusion = null;
		_queuedRemovedExclusion = null;
		ResetDescriptions();
		ExclusionChanged.Invoke(null);
	}

	public void AddExclusion(Ressource r)
	{
		string ressourceString = r.GetType().ToString();
		User.LocalUser.GetComponent<UNetRecepteurCmd>().CallCmdAddExclusion(base.gameObject, ressourceString);
	}

	public void DoExcludeAll()
	{
		ExcludeAll = true;
		_currentCapacity.IntersectWith(Description.Empty);
		ExclusiveRessource = null;
		ExclusionChanged.Invoke(null);
	}

	public void _AddExclusion(Ressource r)
	{
		_queuedAddedExclusion = null;
		_exclusions[r.GetType()] = r;
		RegenerateDescriptionsWithExclusions();
	}

	public void RemoveExclusion(Ressource r)
	{
		string ressourceString = r.GetType().ToString();
		User.LocalUser.GetComponent<UNetRecepteurCmd>().CallCmdRemoveExclusion(base.gameObject, ressourceString);
	}

	public void _RemoveExclusion(Ressource r)
	{
		_queuedRemovedExclusion = null;
		if (_exclusions.Contains(r))
		{
			_exclusions.Remove(r);
			OnExclusionRemoved.Invoke(r);
			RegenerateDescriptionsWithExclusions();
		}
	}

	public Transform ContainerFor(GameObject itemGO)
	{
		return _container;
	}

	public void ObjectUntracked(GameObject itemGO)
	{
		IDescriptor component = itemGO.GetComponent<IDescriptor>();
		logger.InfoFormat("Removing GameObject '{0}' from recepteur '{1}'", itemGO, this);
		RemoveFrom(this, component);
		if (itemGO.GetComponent<GenericParentTracker>().IsOnGround())
		{
			toScatter.Add(itemGO);
		}
		DealSingleResourceRecepteur();
		SetInventoryChanged();
		SetResourceRemoved(component);
	}

	private void ProcessParentlessObjets()
	{
		int count = toScatter.Count;
		if (count > 0)
		{
			Vector3 center = VolumeRegistry.ClosestAvailableDropPosition(base.transform, count, null, forceDrop: true);
			ScatterObjects(toScatter, center);
		}
		toScatter.Clear();
	}

	public static void RemoveFrom(IRecepteur recept, IDescriptor item)
	{
		recept.StoredItems.Remove(item);
		if (recept.StoredItems.Count == 0)
		{
			recept.ContentDescription.Clear();
			recept.CurrentCapacity.ConvertTo(recept.BaseCapacityWithExclusions);
		}
		else
		{
			recept.ContentDescription.Subtract(item.Description);
			recept.CurrentCapacity.Increment<Quantite>(item.Description);
		}
	}

	public void OnObjectTracked(GameObject itemGO)
	{
		IDescriptor component = itemGO.GetComponent<IDescriptor>();
		GameObject gameObject = component.GameObject;
		bool flag = false;
		GenericParentTracker component2 = gameObject.GetComponent<GenericParentTracker>();
		if (component2 is RecepteurParentTracker)
		{
			flag = (component2 as RecepteurParentTracker).leaveFree;
		}
		gameObject.SetActive(flag);
		if (!flag)
		{
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.Soulever();
		}
		AddTo(this, component);
		DealSingleResourceRecepteur();
		SetResourceAdded(component);
		SetInventoryChanged();
	}

	private void DealSingleResourceRecepteur()
	{
		if (!(AssetKey != ObjetsDynamiques.Palette) || !(GetComponent<Labor>() == null))
		{
			AcceptMultipleResourceType = base.recepteur.IsEmpty();
			RegenerateDescriptionsWithExclusions();
		}
	}

	public static void AddTo(IRecepteur recept, IDescriptor item)
	{
		recept.StoredItems.Add(item);
		recept.ContentDescription.AddAllFrom(item.Description);
		if (!recept.AcceptMultipleResourceType)
		{
			recept.CurrentCapacity.IntersectWith(item.Description);
		}
		recept.CurrentCapacity.Subtract(item.Description);
	}

	public override void ResetComponent()
	{
		Wipe();
		base.ResetComponent();
	}

	private void FixedUpdate()
	{
		base.enabled = false;
		ProcessInventoryChanged();
		ProcessParentlessObjets();
	}

	private void SetInventoryChanged()
	{
		_inventoryChanged = true;
		base.enabled = true;
	}

	private void SetResourceAdded(IDescriptor item)
	{
		OnResourceAdded.Invoke(item);
		OnReceive.Invoke(item.GameObject);
	}

	private void SetResourceRemoved(IDescriptor item)
	{
		OnResourceRemoved.Invoke(item);
	}

	private void ProcessInventoryChanged()
	{
		if (_inventoryChanged)
		{
			_inventoryChanged = false;
			InventoryChanged.Invoke();
			LuaHookBase.InvokeFromCLR(lua_OnRecepteurInventoryChanged, Ld);
			previousResource = GetContainedResourceType();
		}
	}

	[LuaDrivable("RecepteurProportionFilled")]
	private static DynValue lua_GetProportionUsed(ScriptExecutionContext context, CallbackArguments args)
	{
		Recepteur recepteur = args.AsComponent<Recepteur>(0, "RecepteurProportionFilled");
		return DynValue.NewNumber(recepteur.GetProportionUsed());
	}

	[LuaDrivable("RecepteurIsFull")]
	private static DynValue lua_IsFull(ScriptExecutionContext context, CallbackArguments args)
	{
		Recepteur recepteur = args.AsComponent<Recepteur>(0, "RecepteurIsFull");
		return DynValue.NewBoolean(recepteur.IsFull());
	}

	[LuaDrivable("RecepteurIsEmpty")]
	private static DynValue lua_IsEmpty(ScriptExecutionContext context, CallbackArguments args)
	{
		Recepteur recepteur = args.AsComponent<Recepteur>(0, "RecepteurIsEmpty");
		return DynValue.NewBoolean(recepteur.IsEmpty());
	}

	[LuaDrivable("RecepteurHasSomething")]
	private static DynValue lua_CarriesSomething(ScriptExecutionContext context, CallbackArguments args)
	{
		Recepteur recepteur = args.AsComponent<Recepteur>(0, "RecepteurHasSomething");
		return DynValue.NewBoolean(recepteur.CarriesSomething());
	}

	[LuaDrivable("RecepteurCarries")]
	private static DynValue lua_Carries(ScriptExecutionContext context, CallbackArguments args)
	{
		Recepteur recepteur = args.AsComponent<Recepteur>(0, "RecepteurCarries");
		GameObject thing = args.AsGameObject(1, "RecepteurCarries");
		return DynValue.NewBoolean(recepteur.Carries(thing));
	}

	[LuaDrivable("WipeAll")]
	public static DynValue lua_WipeAll(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		Recepteur recepteur = args.AsComponent<Recepteur>(0, "WipeAll");
		recepteur.Wipe();
		return DynValue.Void;
	}

	[LuaDrivable("FillWithRessource")]
	public static DynValue lua_FillWithRessource(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		Recepteur p = args.AsComponent<Recepteur>(0, "FillWithRessource");
		string parameter = args.AsString(1, "FillWithRessource");
		AdjectiveInfo.GetInfosFromType(AdjectiveInfo.AdjectiveType.Ressource).Slinq().FirstOrNone((AdjectiveInfo i, string aRequestResource) => i.codeName == aRequestResource, parameter)
			.ForEach(delegate(AdjectiveInfo i, Recepteur aSelf)
			{
				aSelf.WipeAndFill(Adjectif.StringToAdjectif(i.codeName) as Ressource);
			}, p);
		return DynValue.Void;
	}

	[LuaDrivable("ContainsSome")]
	public static DynValue lua_ContainsSome(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		Recepteur recepteur = args.AsComponent<Recepteur>(0, "ContainsSome");
		string adjectifString = args.AsString(1, "ContainsSome");
		if (recepteur.ContainsSomeOf(Description.RepresentativeOf(Adjectif.StringToAdjectif(adjectifString) as Ressource)))
		{
			return DynValue.NewBoolean(v: true);
		}
		return DynValue.NewBoolean(v: false);
	}

	[LuaDrivable("GetQuantity")]
	public static DynValue lua_GetQuantity(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		Recepteur recepteur = args.AsComponent<Recepteur>(0, "GetQuantity");
		LuaCrafting.LuaResource luaResource = args.AsEnum<LuaCrafting.LuaResource>(1, "GetQuantity");
		if (luaResource == LuaCrafting.LuaResource.None)
		{
			return DynValue.NewNumber(1.0);
		}
		Ressource adjectif = LuaCrafting.ToResource(luaResource);
		return DynValue.NewNumber(recepteur._contentDescription.Value(adjectif));
	}

	[LuaDrivable("GetContainedResource")]
	public static DynValue lua_GetContainedResource(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		Recepteur recepteur = args.AsComponent<Recepteur>(0, "GetContainedResource");
		Ressource containedResourceType = recepteur.GetContainedResourceType();
		return DynValue.FromObject(executionContext.OwnerScript, LuaCrafting.ToLuaResource(containedResourceType));
	}

	[LuaDrivable("GetPreviousResource")]
	public static DynValue lua_GetPreviousResource(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		Recepteur recepteur = args.AsComponent<Recepteur>(0, "GetPreviousResource");
		return DynValue.FromObject(executionContext.OwnerScript, LuaCrafting.ToLuaResource(recepteur.previousResource));
	}

	GameObject IObjectTracker.get_gameObject()
	{
		return base.gameObject;
	}
}
