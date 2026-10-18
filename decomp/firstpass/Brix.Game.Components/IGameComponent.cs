using Brix.Components;
using Brix.External.Factories;
using Brix.Game.AI.Damage;
using Brix.Game.Components.Volumes;
using Brix.Game.Semantique;
using Brix.Lifecycle.Pooling;
using Brix.Lua;
using UnityEngine;

namespace Brix.Game.Components;

public interface IGameComponent : IBrixComponent, IPoolableComponent, IInitializable, IRequestable, IResetable
{
	GameObject GameObject { get; }

	bool IsOwnedByMe { get; }

	Factory.AssetKey AssetKey { get; }

	BaseVoxelTransform voxel { get; }

	IVolume volume { get; }

	IDamageReceiver damageReceiver { get; }

	bool selection { get; }

	bool hover { get; }

	IDescriptor Descriptor { get; }

	Apparence apparence { get; }

	Recepteur recepteur { get; }

	Recepteur ParentRecepteur { get; }

	SignalHandler signal { get; }

	Faction faction { get; set; }

	AffiliationComponent affiliation { get; }

	LuaDriver luaDriver { get; }

	LocalFactionVision vision { get; }

	GameComponentPoco GetCompCacheList();
}
