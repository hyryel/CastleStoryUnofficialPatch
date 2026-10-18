using System.Collections.Generic;
using System.Runtime.InteropServices;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Lua;
using Brix.Network;
using Brix.Network.Util;
using Brix.Utils.Factories;
using MoonSharp.Interpreter;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Components;

[JsonObject(MemberSerialization.OptIn)]
[DisallowMultipleComponent]
[SerializerTarget(new string[] { "save" })]
public class AffiliationComponent : NetworkGameComponent
{
	private Signal<Faction, Faction> _OnFactionChanged;

	public static GameSignals.GameSignal<GameObject, Faction, Faction> AnyObjectChangedFaction = new GameSignals.GameSignal<GameObject, Faction, Faction>();

	private GameObject oldFactionGO;

	[SyncVar(hook = "FactionChanged")]
	public GameObject factionGO;

	[SyncVar]
	public GameObject UserCreator;

	private GameObject _cachedFactionGO;

	private Faction _cachedFaction;

	private LuaHook<Faction, Faction> lua_OnSetFaction;

	private NetworkInstanceId ___factionGONetId;

	private NetworkInstanceId ___UserCreatorNetId;

	public IReadOnlySignal<Faction, Faction> OnFactionChanged
	{
		get
		{
			if (_OnFactionChanged == null)
			{
				_OnFactionChanged = new Signal<Faction, Faction>();
			}
			return _OnFactionChanged;
		}
	}

	[JsonProperty]
	[SerializeField]
	public override Faction faction
	{
		get
		{
			if (factionGO != null)
			{
				if (_cachedFactionGO != factionGO)
				{
					_cachedFactionGO = factionGO;
					_cachedFaction = factionGO.GetComponent<Faction>();
				}
				if (_cachedFaction != null)
				{
					return _cachedFaction;
				}
			}
			return Faction.DefaultNeutral;
		}
		set
		{
			NetworkfactionGO = ((!(value == null)) ? value.gameObject : null);
			FactionChangedActions();
		}
	}

	[LuaDrivable("hk_OnSetFaction")]
	private LuaHook<Faction, Faction> lua_OnSetFaction_Prop
	{
		get
		{
			if (lua_OnSetFaction == null)
			{
				lua_OnSetFaction = new LuaHook<Faction, Faction>();
			}
			return lua_OnSetFaction;
		}
	}

	[LuaDrivable("faction")]
	private DynValue lua_faction
	{
		get
		{
			return DynValue.FromObject(null, faction);
		}
		set
		{
			faction = value.ToObject<LuaDriver>().faction;
		}
	}

	public GameObject NetworkfactionGO
	{
		get
		{
			return factionGO;
		}
		[param: In]
		set
		{
			ref GameObject gameObjectField = ref factionGO;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				FactionChanged(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVarGameObject(value, ref gameObjectField, 1u, ref ___factionGONetId);
		}
	}

	public GameObject NetworkUserCreator
	{
		get
		{
			return UserCreator;
		}
		[param: In]
		set
		{
			SetSyncVarGameObject(value, ref UserCreator, 2u, ref ___UserCreatorNetId);
		}
	}

	public void FactionChanged(GameObject value)
	{
		NetworkfactionGO = value;
		if (!NetworkServer.active)
		{
			FactionChangedActions();
		}
	}

	private void FactionChangedActions()
	{
		if (base.gameObject != null && factionGO != oldFactionGO)
		{
			Faction faction = ((!(oldFactionGO != null)) ? Faction.DefaultNeutral : oldFactionGO.GetComponent<Faction>());
			Faction faction2 = ((!(factionGO != null)) ? Faction.DefaultNeutral : factionGO.GetComponent<Faction>());
			if (_OnFactionChanged != null)
			{
				_OnFactionChanged.SafeInvoke(faction, faction2);
			}
			GameSignals.Invoke(AnyObjectChangedFaction, base.gameObject, faction, faction2);
			if (lua_OnSetFaction != null)
			{
				LuaHookBase.InvokeFromCLR(lua_OnSetFaction, faction, faction2);
			}
			oldFactionGO = factionGO;
		}
	}

	public bool IsAllied(GameObject g)
	{
		return Affiliation.IsAllied(base.gameObject, g);
	}

	public bool IsAllied(Component c)
	{
		return Affiliation.IsAllied(base.gameObject, (!(c != null)) ? null : c.gameObject);
	}

	public bool IsNeutral(GameObject g)
	{
		return Affiliation.IsNeutral(base.gameObject, g);
	}

	public bool IsNeutral(Component c)
	{
		return Affiliation.IsNeutral(base.gameObject, (!(c != null)) ? null : c.gameObject);
	}

	public bool IsEnemy(GameObject g)
	{
		return Affiliation.IsEnemy(base.gameObject, g);
	}

	public bool IsEnemy(Component c)
	{
		return Affiliation.IsEnemy(base.gameObject, (!(c != null)) ? null : c.gameObject);
	}

	public void CopyFactionTo(List<GameObject> gos)
	{
		foreach (GameObject go in gos)
		{
			go.WithComponent(delegate(GameComponent a, Faction f)
			{
				a.faction = f;
			}, faction);
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.Write(factionGO);
			writer.Write(UserCreator);
			return true;
		}
		bool flag2 = false;
		if ((base.syncVarDirtyBits & 1) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(factionGO);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(UserCreator);
		}
		if (!flag2)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
		if (initialState)
		{
			___factionGONetId = reader.ReadNetworkId();
			___UserCreatorNetId = reader.ReadNetworkId();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			FactionChanged(reader.ReadGameObject());
		}
		if ((num & 2) != 0)
		{
			UserCreator = reader.ReadGameObject();
		}
	}

	public override void PreStartClient()
	{
		if (!___factionGONetId.IsEmpty())
		{
			NetworkfactionGO = ClientScene.FindLocalObject(___factionGONetId);
		}
		if (!___UserCreatorNetId.IsEmpty())
		{
			NetworkUserCreator = ClientScene.FindLocalObject(___UserCreatorNetId);
		}
	}
}
