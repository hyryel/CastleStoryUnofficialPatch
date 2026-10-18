using System;
using System.Collections.Generic;
using Brix.External.Factories;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Brix.Game.Semantique;
using Brix.Utils;
using Brix.Utils.Factories;
using Smooth.Slinq;

namespace Brix.Game.AI.KnowledgeSpace;

public class KnowledgeRegion : IPoolable, IResetable, IDisposable
{
	private Dictionary<IVolume, int> _members = new Dictionary<IVolume, int>();

	private readonly Dictionary<Factory.AssetKey, List<IVolume>> _resourcesByAssetKey = new Dictionary<Factory.AssetKey, List<IVolume>>();

	public int Count => _members.Count;

	public Dictionary<IVolume, int> Members => _members;

	public bool Contains(IGameComponent gc)
	{
		return _members.ContainsKey(gc.volume);
	}

	public List<IVolume> MembersWithKey(Factory.AssetKey key)
	{
		return _resourcesByAssetKey.GetValueOrDefault(key, Empty.List<IVolume>());
	}

	public void UniqueResources(HashSet<Factory.AssetKey> uniqueItems)
	{
		foreach (KeyValuePair<Factory.AssetKey, List<IVolume>> item in _resourcesByAssetKey)
		{
			if (IsStorage(item.Key))
			{
				item.Value.Slinq().ForEach(delegate(IVolume v, HashSet<Factory.AssetKey> items)
				{
					v.AsComponent().WithRecepteur(delegate(Recepteur r, HashSet<Factory.AssetKey> results)
					{
						r.UniqueStoredItemsKeys(results);
					}, items);
				}, uniqueItems);
			}
			else
			{
				uniqueItems.Add(item.Key);
			}
		}
	}

	private bool IsStorage(Factory.AssetKey key)
	{
		GameComponent component = Factory.Peek(key).GetComponent<GameComponent>();
		return component.recepteur != null && component.recepteur.Fonction == FonctionDeContenant.entrepot;
	}

	public void Add(IVolume gc, int accessIndices)
	{
		int valueOrDefault = _members.GetValueOrDefault(gc);
		_members[gc] = accessIndices | valueOrDefault;
		if (valueOrDefault == 0 && accessIndices != 0)
		{
			AddUniqueResource(gc);
		}
	}

	public void AddAll(Dictionary<IVolume, int> members)
	{
		foreach (KeyValuePair<IVolume, int> member in members)
		{
			_members.Add(member.Key, member.Value);
		}
		foreach (KeyValuePair<IVolume, int> member2 in _members)
		{
			if (member2.Key != null)
			{
				AddUniqueResource(member2.Key);
			}
		}
	}

	public void Dispose()
	{
		ObjectPool<KnowledgeRegion>.Release(this);
	}

	public void ResetObject()
	{
		_members.Clear();
		foreach (KeyValuePair<Factory.AssetKey, List<IVolume>> item in _resourcesByAssetKey)
		{
			item.Value.Dispose();
		}
		_resourcesByAssetKey.Clear();
	}

	private void AddUniqueResource(IVolume v)
	{
		if (!v.AsComponent().IsNullOrReleased())
		{
			Factory.AssetKey assetKey = Description.RepresentativeOf(v.GameComponent) ?? v.AssetKey;
			if (Knowledge.InterestingResourceObject.Contains(assetKey))
			{
				_resourcesByAssetKey.GetValueOrCreate(assetKey, CollectionPool.RequestList<IVolume>).Add(v);
			}
		}
	}
}
