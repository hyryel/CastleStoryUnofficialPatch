using System.Collections.Generic;
using Brix.Lua;
using UnityEngine;

namespace Brix.Game.Components;

public class CorruptronDefenseTracker : GameComponent
{
	[LuaDrivable("hk_SpawnDefenders")]
	private static LuaHook<LuaDriver> lua_SpawnDefenders = new LuaHook<LuaDriver>();

	private int _bricktronCount;

	private float _lastAttackTime;

	private float _spawnCooldown = 30f;

	public void UnitsRefresh(HashSet<Trackable> units)
	{
		Faction faction = Faction.FindByName("Corruptrons");
		int num = 0;
		foreach (Trackable unit in units)
		{
			if (unit.faction != faction)
			{
				num++;
			}
		}
		if (base.faction == faction)
		{
			if (num > 0 && _bricktronCount == 0 && _lastAttackTime + _spawnCooldown < Time.fixedUnscaledTime)
			{
				LuaHookBase.InvokeFromCLR(lua_SpawnDefenders, GetComponent<LuaDriver>());
			}
			if (num == 0 && _bricktronCount > 0)
			{
				_lastAttackTime = Time.fixedUnscaledTime;
			}
		}
		_bricktronCount = num;
	}
}
