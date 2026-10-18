using Brix.Engine;
using Brix.Game.AI.Damage;
using Brix.Lifecycle.Pooling;
using Brix.Lua;
using Brix.Network;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Components;

public class ExplodeOnDeath : NetworkGameComponent
{
	public bool DoDamage = true;

	public float intensity = 5f;

	public bool evenIfParentDisabled;

	[LuaDrivable("hk_OnExplosion")]
	private LuaHook<LuaDriver> lua_OnExplosion = new LuaHook<LuaDriver>();

	[LuaDrivable("hk_OnExplosionFaction")]
	private LuaHook<Faction> lua_OnExplosionFaction = new LuaHook<Faction>();

	protected override void Start()
	{
		base.Start();
		GetComponent<DamageReceiver>().OnDeathSignal.Connect(OnDeath);
		base.OnReleaseOrDestroySignal.Connect(OnRelease);
	}

	private void OnRelease(GameObject go)
	{
		LuaDriver component = GetComponent<LuaDriver>();
		if (component != null)
		{
			LuaHookBase.InvokeFromCLR(lua_OnExplosion, component);
			AffiliationComponent component2 = go.GetComponent<AffiliationComponent>();
			LuaHookBase.InvokeFromCLR(lua_OnExplosionFaction, component2.faction);
		}
	}

	private void OnDeath()
	{
		if ((base.gameObject.activeInHierarchy || evenIfParentDisabled) && base.enabled)
		{
			Explode();
		}
	}

	private void Explode()
	{
		Architecte.ExplosionSimple(base.gameObject, base.transform.position, intensity, (!DoDamage) ? "Safe" : "Explosion");
		ObjectPoolSingleton.Release(base.gameObject);
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
