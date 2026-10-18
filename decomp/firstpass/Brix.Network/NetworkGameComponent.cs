using Brix.Components;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game;
using Brix.Game.AI.Damage;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Brix.Game.Interfaces;
using Brix.Game.Network;
using Brix.Game.Semantique;
using Brix.Lifecycle.Pooling;
using Brix.Lua;
using Brix.Utils.Logging;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using log4net;

namespace Brix.Network;

[JsonObject(MemberSerialization.OptIn)]
public class NetworkGameComponent : NetworkBrixComponent, IEditorSelectable, IGameComponent, IBrixComponent, IPoolableComponent, IInitializable, IRequestable, IResetable
{
	private GameComponentPoco _gcPoco;

	public GameObject GameObject => _gcPoco.GameObject;

	public virtual bool IsOwnedByMe => _gcPoco.IsOwnedByMe;

	public virtual Factory.AssetKey AssetKey => _gcPoco.AssetKey;

	public virtual Blueprint Blueprint => _gcPoco.Blueprint;

	public bool IsInPicker => _gcPoco.IsInPicker;

	public BaseVoxelTransform voxel => _gcPoco.voxel;

	public IVolume volume => _gcPoco.volume;

	public IDamageReceiver damageReceiver => _gcPoco.damageReceiver;

	public bool selection => _gcPoco.selection;

	public bool hover => _gcPoco.hover;

	public IDescriptor Descriptor => _gcPoco.Descriptor;

	public Apparence apparence => _gcPoco.apparence;

	public Recepteur recepteur => _gcPoco.recepteur;

	public Recepteur ParentRecepteur => _gcPoco.ParentRecepteur;

	public GenericParentTracker ParentTracker => _gcPoco.ParentTracker;

	public SignalHandler signal => _gcPoco.signal;

	public virtual Faction faction
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

	public AffiliationComponent affiliation => _gcPoco.affiliation;

	public LuaDriver luaDriver => _gcPoco.luaDriver;

	public LocalFactionVision vision => _gcPoco.vision;

	public Signal OnEditorSelected => null;

	public Signal OnEditorDeselected => null;

	public override void OnStartClient()
	{
		if (!NetworkServer.active)
		{
			OnStartPureClient();
			OnStartAnywhere();
		}
		else
		{
			OnStartHost();
			OnStartAnywhere();
		}
	}

	public override void OnStartServer()
	{
		OnStartServerOrHost();
	}

	public virtual void OnStartAnywhere()
	{
	}

	public virtual void OnStartPureClient()
	{
	}

	public virtual void OnStartHost()
	{
	}

	public virtual void OnStartServerOrHost()
	{
	}

	public override void Initialize()
	{
		base.Initialize();
		_gcPoco = GetComponent<IGameComponent>().GetCompCacheList();
	}

	public GameComponentPoco GetCompCacheList()
	{
		return _gcPoco ?? (_gcPoco = new GameComponentPoco(base.gameObject));
	}

	public virtual bool CheckTag(string tag)
	{
		return _gcPoco.CheckTag(tag);
	}

	private ILog CreateInstanceLogger()
	{
		return new BrixLogger(GetType().FullName);
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		bool flag2 = default(bool);
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
	}
}
