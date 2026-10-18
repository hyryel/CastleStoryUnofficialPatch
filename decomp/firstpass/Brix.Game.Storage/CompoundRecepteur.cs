using System.Collections.Generic;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Game.Semantique;
using Brix.Utils;
using Brix.Utils.Signals;
using Smooth.Algebraics;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using UnityEngine;

namespace Brix.Game.Storage;

public class CompoundRecepteur : GameComponent, IRecepteur
{
	[SerializeField]
	private HashSet<Recepteur> TrackedRecepteurs = new HashSet<Recepteur>();

	private Signal _changed = new Signal();

	private HashSet<IDescriptor> _storedItems = new HashSet<IDescriptor>();

	private Description _selfDescription = new Description();

	private Description _contentDescription = new Description();

	private Description _baseCapacity = new Description();

	private Description _baseCapacityWithExclusions = new Description();

	private Description _currentCapacity = new Description();

	private Dictionary<Factory.AssetKey, Tuple<IDescriptor, int>> _uniqueStoredItems = new Dictionary<Factory.AssetKey, Tuple<IDescriptor, int>>();

	[SerializeField]
	protected List<GameObject> uniqueItemsForDebug = new List<GameObject>();

	private Signal reset = new Signal
	{
		MaximumConnections = 5000
	};

	private readonly Dictionary<Recepteur, SignalConnection> _resourceAddedConnections = new Dictionary<Recepteur, SignalConnection>();

	private readonly Dictionary<Recepteur, SignalConnection> _resourceRemovedConnections = new Dictionary<Recepteur, SignalConnection>();

	private readonly Dictionary<Recepteur, SignalConnection> _releaseOrDestroyConnections = new Dictionary<Recepteur, SignalConnection>();

	public int StorageUnitCount => TrackedRecepteurs.Count;

	public IReadOnlySignal Changed => _changed;

	public HashSet<IDescriptor> StoredItems => _storedItems;

	public Description SelfDescription => _selfDescription;

	public Description ContentDescription => _contentDescription;

	public Description BaseCapacity => _baseCapacity;

	public Description BaseCapacityWithExclusions => _baseCapacityWithExclusions;

	public Description CurrentCapacity => _currentCapacity;

	public bool AcceptMultipleResourceType => true;

	public HashSet<Recepteur> GetRecepteurs()
	{
		return TrackedRecepteurs;
	}

	private void ShowUniqueItemsForDebug()
	{
		uniqueItemsForDebug.Clear();
		(from i in UniqueStoredItemsSlinq()
			select i.GameObject).AddTo(uniqueItemsForDebug);
	}

	public Slinq<IDescriptor, SelectContext<IDescriptor, KeyValuePair<Factory.AssetKey, Tuple<IDescriptor, int>>, DictionaryContext<Factory.AssetKey, Tuple<IDescriptor, int>>>> UniqueStoredItemsSlinq()
	{
		return from kv in _uniqueStoredItems.Slinq()
			select kv.Value._1;
	}

	public Slinq<Recepteur, PredicateContext<Recepteur, HashSetSlinqContext<Recepteur>, Factory.AssetKey>> RecepteursWithSpaceForSlinq(Factory.AssetKey key)
	{
		return TrackedRecepteurs.Slinq().Where((Recepteur r, Factory.AssetKey k) => r.HasRoomFor(k), key);
	}

	public IEnumerable<Recepteur> GetCompoundedRecepteurs()
	{
		return TrackedRecepteurs;
	}

	public Slinq<Recepteur, PredicateContext<Recepteur, HashSetSlinqContext<Recepteur>, HashSet<Factory.AssetKey>>> RecepteursWithSpaceForOneOfSlinq(HashSet<Factory.AssetKey> keys)
	{
		return TrackedRecepteurs.Slinq().Where(RecepteurHasRoomForOneOf, keys);
	}

	private static bool RecepteurHasRoomForOneOf(Recepteur recepteur, HashSet<Factory.AssetKey> keys)
	{
		if (recepteur.TakeOnly)
		{
			return false;
		}
		if (keys.Count == 0)
		{
			return true;
		}
		return keys.Slinq().Select(Factory.Peek).Any((GameObject go, Recepteur r) => r.HasRoomFor(go), recepteur);
	}

	public Slinq<Recepteur, PredicateContext<Recepteur, HashSetSlinqContext<Recepteur>, List<GameObject>>> RecepteursWithSpaceForOneOfSlinq(List<GameObject> keys)
	{
		return TrackedRecepteurs.Slinq().Where(RecepteurHasRoomForOneOf, keys);
	}

	private static bool RecepteurHasRoomForOneOf(Recepteur recepteur, List<GameObject> keys)
	{
		if (recepteur.TakeOnly)
		{
			return false;
		}
		if (keys.Count == 0)
		{
			return true;
		}
		return keys.Slinq().Any((GameObject go, Recepteur r) => r.HasRoomFor(go), recepteur);
	}

	public bool HasRecepteursWithOneOf(HashSet<Factory.AssetKey> keys)
	{
		return TrackedRecepteurs.Slinq().Where((Recepteur r, HashSet<Factory.AssetKey> _keys) => r.HasOneOf(_keys), keys).Any();
	}

	public int RecepteursWithCount(HashSet<Factory.AssetKey> keys)
	{
		return TrackedRecepteurs.Slinq().Where((Recepteur r, HashSet<Factory.AssetKey> _keys) => r.HasOneOf(_keys), keys).Count();
	}

	public int RecepteursWithCount(Adjectif adj)
	{
		return TrackedRecepteurs.Slinq().Where((Recepteur r, Adjectif _adj) => r.ContentDescription.Value(_adj) > 0, adj).Count();
	}

	public int RecepteursFullOfCount(Adjectif adj)
	{
		return TrackedRecepteurs.Slinq().Where((Recepteur r, Adjectif _adj) => r.ContentDescription.Value(_adj) > 0 && r.CurrentCapacity.Value(_adj) == 0, adj).Count();
	}

	public int RecepteursEmptyCount()
	{
		return (from r in TrackedRecepteurs.Slinq()
			where r.IsEmpty()
			select r).Count();
	}

	public void RecepteursWithOneOf(HashSet<Factory.AssetKey> keys, ref HashSet<Recepteur> recepteurs)
	{
		TrackedRecepteurs.Slinq().Where((Recepteur r, HashSet<Factory.AssetKey> _keys) => r.HasOneOf(_keys), keys).AddTo(recepteurs);
	}

	public Slinq<Recepteur, PredicateContext<Recepteur, HashSetSlinqContext<Recepteur>, Description>> RecepteursWithOneOf(Description desc)
	{
		return TrackedRecepteurs.Slinq().Where((Recepteur r, Description d) => d.Adjectifs.Any((Adjectif adj, Recepteur _r) => _r.ContentDescription.Has(adj), r), desc);
	}

	public Slinq<Recepteur, PredicateContext<Recepteur, HashSetSlinqContext<Recepteur>, Factory.AssetKey>> RecepteursWith(Factory.AssetKey key, Recepteur except = null)
	{
		return TrackedRecepteurs.Slinq().Where((Recepteur r, Factory.AssetKey k) => r.Has(k) && r != except, key);
	}

	public void RecepteursWith(ref HashSet<Recepteur> recepteurs, Apparence apparence)
	{
		foreach (Recepteur trackedRecepteur in TrackedRecepteurs)
		{
			if (trackedRecepteur.ContainsSomeOf(apparence.AssetKey))
			{
				recepteurs.Add(trackedRecepteur);
			}
		}
	}

	public Slinq<Recepteur, PredicateContext<Recepteur, HashSetSlinqContext<Recepteur>, IList<Adjectif>>> RecepteursWithOneOf(IList<Adjectif> adjectifs)
	{
		return TrackedRecepteurs.Slinq().Where((Recepteur r, IList<Adjectif> _adjectifs) => _adjectifs.Slinq().Any((Adjectif adj, Recepteur _r) => _r.ContentDescription.Has(adj), r), adjectifs);
	}

	public void Reset()
	{
		TrackedRecepteurs.Clear();
		CurrentCapacity.Clear();
		StoredItems.Clear();
		SelfDescription.Clear();
		ContentDescription.Clear();
		BaseCapacity.Clear();
		BaseCapacityWithExclusions.Clear();
		CurrentCapacity.Clear();
		reset.Invoke();
	}

	public void FromCollection(HashSet<Recepteur> recepteurs)
	{
		Reset();
		(from r in recepteurs.Slinq()
			where r != null
			select r).ForEach(delegate(Recepteur r, CompoundRecepteur self)
		{
			self.Add(r);
		}, this);
	}

	public void FromCollection(HashSet<GameObject> recepteurs)
	{
		Reset();
		(from c in recepteurs.Slinq()
			where c != null
			select c.GetComponent<Recepteur>()).ForEach(delegate(Recepteur r, CompoundRecepteur self)
		{
			self.Add(r);
		}, this);
	}

	public void FromCollection(List<GameObject> recepteurs)
	{
		Reset();
		(from c in recepteurs.Slinq()
			where c != null
			select c.GetComponent<Recepteur>()).ForEach(delegate(Recepteur r, CompoundRecepteur self)
		{
			self.Add(r);
		}, this);
	}

	private void AddStoredObjects(IDescriptor item)
	{
		StoredItems.Add(item);
		AddUniqueItem(item);
	}

	public void Add(Recepteur recepteur)
	{
		if (!TrackedRecepteurs.Contains(recepteur))
		{
			TrackedRecepteurs.Add(recepteur);
			SignalConnection newValue = recepteur.OnResourceAdded.Connect(delegate(IDescriptor item, CompoundRecepteur self)
			{
				self.AddRessource(item);
			}, this).DisconnectWhen(reset);
			_resourceAddedConnections.GetValueOrDefaultThenUpdate(recepteur, newValue, SignalConnection.Empty).Disconnect();
			SignalConnection newValue2 = recepteur.OnResourceRemoved.Connect(delegate(IDescriptor item, CompoundRecepteur self)
			{
				self.RemoveResource(item);
			}, this).DisconnectWhen(reset);
			_resourceRemovedConnections.GetValueOrDefaultThenUpdate(recepteur, newValue2, SignalConnection.Empty).Disconnect();
			SignalConnection newValue3 = recepteur.OnReleaseOrDestroySignal.ConnectOnce(delegate(GameObject go, CompoundRecepteur self)
			{
				self.Remove(go.GetComponent<Recepteur>());
			}, this).DisconnectWhen(reset);
			_releaseOrDestroyConnections.GetValueOrDefaultThenUpdate(recepteur, newValue3, SignalConnection.Empty).Disconnect();
			recepteur.StoredItems.Slinq().ForEach(AddStoredObjects);
			BaseCapacity.Add<Quantite>(recepteur.BaseCapacity);
			BaseCapacityWithExclusions.Add<Quantite>(recepteur.BaseCapacityWithExclusions);
			ContentDescription.Add<Quantite>(recepteur.ContentDescription);
			CurrentCapacity.Add<Quantite>(recepteur.CurrentCapacity);
			_changed.Invoke();
		}
	}

	public void Remove(Recepteur recepteur)
	{
		if (TrackedRecepteurs.Contains(recepteur))
		{
			TrackedRecepteurs.Remove(recepteur);
			_resourceAddedConnections.GetValueOrDefaultAndRemove(recepteur, SignalConnection.Empty).Disconnect();
			_resourceRemovedConnections.GetValueOrDefaultAndRemove(recepteur, SignalConnection.Empty).Disconnect();
			_releaseOrDestroyConnections.GetValueOrDefaultAndRemove(recepteur, SignalConnection.Empty).Disconnect();
			BaseCapacity.Subtract(recepteur.BaseCapacity);
			BaseCapacityWithExclusions.Subtract(recepteur.BaseCapacityWithExclusions);
			ContentDescription.Subtract(recepteur.ContentDescription);
			CurrentCapacity.Subtract(recepteur.CurrentCapacity);
			_changed.Invoke();
		}
	}

	private void AddUniqueItem(IDescriptor item)
	{
		Factory.AssetKey assetKey = GetAssetKey(item);
		if (!_uniqueStoredItems.ContainsKey(assetKey))
		{
			_uniqueStoredItems.Add(assetKey, new Tuple<IDescriptor, int>(Factory.Peek(assetKey).GetComponent<Apparence>(), 1));
			return;
		}
		Tuple<IDescriptor, int> tuple = _uniqueStoredItems[assetKey];
		_uniqueStoredItems[assetKey] = new Tuple<IDescriptor, int>(tuple._1, tuple._2 + 1);
	}

	private void RemoveUniqueItem(IDescriptor item)
	{
		Factory.AssetKey assetKey = GetAssetKey(item);
		if (_uniqueStoredItems.ContainsKey(assetKey))
		{
			Tuple<IDescriptor, int> tuple = _uniqueStoredItems[assetKey];
			if (tuple._2 <= 1)
			{
				_uniqueStoredItems.Remove(assetKey);
			}
			else
			{
				_uniqueStoredItems[assetKey] = new Tuple<IDescriptor, int>(tuple._1, tuple._2 - 1);
			}
		}
	}

	private Factory.AssetKey GetAssetKey(IDescriptor descriptor)
	{
		return descriptor.GameObject.GetComponent<GameComponent>().AssetKey;
	}

	private void AddRessource(IDescriptor item)
	{
		Recepteur.AddTo(this, item);
		AddUniqueItem(item);
		_changed.Invoke();
	}

	private void RemoveResource(IDescriptor item)
	{
		Recepteur.RemoveFrom(this, item);
		RemoveUniqueItem(item);
		_changed.Invoke();
	}

	public bool ContainsSomeOf(Factory.AssetKey key)
	{
		return StoredItems.Slinq().Any((IDescriptor d, Factory.AssetKey _key) => d.AssetKey == _key, key);
	}

	public bool ContainsSomeOf(HashSet<Factory.AssetKey> keys)
	{
		return StoredItems.Slinq().Any((IDescriptor d, HashSet<Factory.AssetKey> _key) => keys.Contains(d.AssetKey), keys);
	}
}
