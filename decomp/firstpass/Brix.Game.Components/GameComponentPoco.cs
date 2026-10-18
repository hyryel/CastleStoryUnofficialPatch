using Brix.External.Factories;
using Brix.Game.AI;
using Brix.Game.AI.Damage;
using Brix.Game.Components.Volumes;
using Brix.Game.Network;
using Brix.Game.Semantique;
using Brix.Input;
using Brix.Legacy;
using Brix.Lua;
using Brix.UI.GameSelector;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.Game.Components;

public class GameComponentPoco
{
	private static BrixLogger logger = new BrixLogger(typeof(GameComponentPoco).FullName);

	private CompCache<FactoryImprint> _assetkey;

	private CompCache<AssetKeyOverride> _repKey;

	private CompCache<Blueprint> _blueprint;

	private CompCache<IDescriptor> _descriptor;

	private CompCache<BaseVoxelTransform> _voxel;

	private CompCache<IVolume> _volume;

	private CompCache<IDamageReceiver> _damageReceiver;

	private CompCache<Selectionnable> _selectionable;

	private CompCache<Apparence> _apparence;

	private CompCache<SignalHandler> _signal;

	private CompCache<AffiliationComponent> _affiliation;

	private CompCache<LuaDriver> _luaDriver;

	private CompCache<LocalFactionVision> _vision;

	private CompCache<GenericParentTracker> parentTracker;

	private CompCache<Recepteur> _recepteur;

	private GameObject _gameObject;

	public GameObject GameObject => _gameObject;

	public bool IsInPicker => _gameObject.GetComponentInParent<PickerComponent>();

	public bool IsOwnedByMe => faction != null && faction.IsOwnedByMe;

	public Factory.AssetKey AssetKey => _assetkey.Get.AssetKey;

	public Factory.AssetKey RepresentativeKey => (!(_repKey.Get != null)) ? AssetKey : _repKey.Get.KeyOverride;

	public Blueprint Blueprint => _blueprint.GetOrNull;

	public BaseVoxelTransform voxel => _voxel.GetOrNull;

	public IVolume volume => _volume.GetOrNull;

	public IDamageReceiver damageReceiver => _damageReceiver.GetOrNull;

	public bool selection => UIGameSelector.IsSelected(_gameObject);

	public bool hover => UIGameSelector.IsHovered(_gameObject);

	public bool isInDanger => UIGameObserver.bricktrons.GetDangerFlag(_gameObject.GetComponent<Labor>());

	public IDescriptor Descriptor => _descriptor.GetOrNull;

	public Apparence apparence => _apparence.GetOrNull;

	public Recepteur recepteur => _recepteur.GetOrNull;

	public Recepteur ParentRecepteur
	{
		get
		{
			Recepteur component = recepteur;
			if (component == null)
			{
				GenericParentTracker genericParentTracker = ParentTracker;
				if (genericParentTracker != null)
				{
					GameObject gameObject = genericParentTracker.TrackingParent();
					if (gameObject != null)
					{
						component = gameObject.GetComponent<Recepteur>();
					}
				}
			}
			return component;
		}
	}

	public GenericParentTracker ParentTracker => parentTracker.GetOrNull;

	public SignalHandler signal => _signal.GetOrNull;

	public Faction faction
	{
		get
		{
			AffiliationComponent affiliationComponent = affiliation;
			if (affiliationComponent == null)
			{
				return Faction.GetDefault(_gameObject);
			}
			Faction faction = affiliationComponent.faction;
			if (faction == null)
			{
				return Faction.GetDefault(_gameObject);
			}
			return faction;
		}
		set
		{
			_affiliation.GetOrAdd.faction = value;
		}
	}

	public AffiliationComponent affiliation => _affiliation.GetOrNull;

	public LuaDriver luaDriver => _luaDriver.GetOrNull;

	public LocalFactionVision vision => _vision.GetOrNull;

	public GameComponentPoco(GameObject gameObject)
	{
		_gameObject = gameObject;
		_assetkey = new CompCache<FactoryImprint>(gameObject);
		_repKey = new CompCache<AssetKeyOverride>(gameObject);
		_blueprint = new CompCache<Blueprint>(gameObject);
		_descriptor = new CompCache<IDescriptor>(gameObject);
		_voxel = new CompCache<BaseVoxelTransform>(gameObject);
		_volume = new CompCache<IVolume>(gameObject);
		_damageReceiver = new CompCache<IDamageReceiver>(gameObject);
		_selectionable = new CompCache<Selectionnable>(gameObject);
		_apparence = new CompCache<Apparence>(gameObject);
		_signal = new CompCache<SignalHandler>(gameObject);
		_affiliation = new CompCache<AffiliationComponent>(gameObject);
		_luaDriver = new CompCache<LuaDriver>(gameObject);
		_vision = new CompCache<LocalFactionVision>(gameObject);
		parentTracker = new CompCache<GenericParentTracker>(gameObject);
		_recepteur = new CompCache<Recepteur>(gameObject);
	}

	public GameComponentPoco GetCompCacheList()
	{
		return this;
	}

	public bool CheckTag(string tag)
	{
		return _selectionable.GetOrNull?.CheckTag(tag) ?? false;
	}
}
