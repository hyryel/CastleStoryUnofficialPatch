using System;
using System.Collections.Generic;
using System.IO;
using Brix.Components;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.AI.Damage;
using Brix.Game.Components.Volumes;
using Brix.Game.Interfaces;
using Brix.Game.Network;
using Brix.Game.Semantique;
using Brix.Lifecycle.Pooling;
using Brix.Lua;
using Brix.Utils;
using Brix.Utils.Logging;
using Newtonsoft.Json;
using UnityEngine;
using log4net;

namespace Brix.Game.Components;

[JsonObject(MemberSerialization.OptIn)]
public class GameComponent : BrixComponent, IEditorSelectable, IGameComponent, IBrixComponent, IPoolableComponent, IInitializable, IRequestable, Brix.Lifecycle.Pooling.IResetable
{
	private BrixLogger logger = new BrixLogger(typeof(GameComponent).FullName);

	private GameComponentPoco _gcPoco;

	public GameObject GameObject => _gcPoco.GameObject;

	public virtual bool IsOwnedByMe => _gcPoco.IsOwnedByMe;

	public virtual Factory.AssetKey AssetKey
	{
		get
		{
			if (_gcPoco.AssetKey == null)
			{
				throw new InvalidDataException("Asset Key is not ready");
			}
			return _gcPoco.AssetKey;
		}
	}

	public virtual Factory.AssetKey RepresentativeKey
	{
		get
		{
			if (_gcPoco.AssetKey == null)
			{
				throw new InvalidDataException("Asset Key is not ready");
			}
			return _gcPoco.AssetKey;
		}
	}

	public virtual Blueprint Blueprint => _gcPoco.Blueprint;

	public bool IsInPicker => _gcPoco.IsInPicker;

	public BaseVoxelTransform voxel => _gcPoco.voxel;

	public IVolume volume => _gcPoco.volume;

	public IDamageReceiver damageReceiver => _gcPoco.damageReceiver;

	public bool selection => _gcPoco.selection;

	public bool hover => _gcPoco.hover;

	public bool isInDanger => _gcPoco.isInDanger;

	public IDescriptor Descriptor => _gcPoco.Descriptor;

	public Apparence apparence => _gcPoco.apparence;

	public Recepteur recepteur => _gcPoco.recepteur;

	public Recepteur ParentRecepteur => _gcPoco.ParentRecepteur;

	public GenericParentTracker ParentTracker => _gcPoco.ParentTracker;

	public SignalHandler signal => _gcPoco.signal;

	public Faction faction
	{
		get
		{
			return _gcPoco.faction;
		}
		set
		{
			_gcPoco.faction = value;
		}
	}

	public LuaDriver luaDriver => _gcPoco.luaDriver;

	public AffiliationComponent affiliation => _gcPoco.affiliation;

	public LocalFactionVision vision => _gcPoco.vision;

	public Signal OnEditorSelected => null;

	public Signal OnEditorDeselected => null;

	public override void Initialize()
	{
		base.Initialize();
		_gcPoco = GetComponent<IGameComponent>().GetCompCacheList();
	}

	public GameComponentPoco GetCompCacheList()
	{
		return _gcPoco ?? (_gcPoco = new GameComponentPoco(base.gameObject));
	}

	public bool IsInRecepteur()
	{
		return ParentRecepteur != null && ParentRecepteur.gameObject != base.gameObject;
	}

	public virtual bool CheckTag(string tag)
	{
		return _gcPoco.CheckTag(tag);
	}

	public void GetDescriptions(List<Apparence> results)
	{
		CraftingStation component = GetComponent<CraftingStation>();
		if (component != null)
		{
			if (component.ProductionToPickup(out var go))
			{
				Apparence component2 = go.GetComponent<Apparence>();
				results.Add(component2);
			}
			return;
		}
		if (recepteur != null && recepteur.Fonction == FonctionDeContenant.entrepot)
		{
			foreach (IDescriptor storedItem in recepteur.StoredItems)
			{
				Apparence apparence = storedItem as Apparence;
				if (apparence.IsNullOrReleased())
				{
					logger.WarnFormat("About to provide a null apparence in {0}", this);
				}
				else
				{
					results.Add(apparence);
				}
			}
			return;
		}
		if (this.apparence.IsNullOrReleased())
		{
			logger.WarnFormat("About to provide a null apparence in {0}", this);
		}
		else
		{
			results.Add(this.apparence);
		}
	}

	public Apparence GetMatchingDescription(Func<Apparence, bool> matchFunc)
	{
		List<Apparence> list = CollectionPool.RequestList<Apparence>();
		GetDescriptions(list);
		Apparence result = null;
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			if (matchFunc(list[i]))
			{
				result = list[i];
				break;
			}
		}
		list.Dispose();
		return result;
	}

	public Apparence GetMatchingDescription<T>(FonctionDeContenant fonction, Func<Apparence, T, bool> matchFunc, T arg)
	{
		List<Apparence> list = CollectionPool.RequestList<Apparence>();
		GetDescriptions(list);
		Apparence result = null;
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			if (matchFunc(list[i], arg))
			{
				result = list[i];
				break;
			}
		}
		list.Dispose();
		return result;
	}

	private ILog CreateInstanceLogger()
	{
		return new BrixLogger(GetType().FullName);
	}
}
