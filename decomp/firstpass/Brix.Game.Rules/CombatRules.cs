using System;
using Brix.Lua;
using Brix.Network.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Brix.Game.Rules;

[Serializable]
[RequireComponent(typeof(RuleMaster))]
[JsonObject(MemberSerialization.OptIn)]
[SerializerTarget(new string[] { "save" })]
public class CombatRules : MonoBehaviour
{
	private CompCache<RuleMaster> _ruleMaster;

	private static CombatRules _main;

	public static CombatRules Main => _main;

	[LuaDrivable("unitToUnitDamageMultiplier")]
	public static float UnitToUnitDamageMultiplier { get; private set; }

	[LuaDrivable("unitToObjectDamageMultiplier")]
	public static float UnitToObjectDamageMultiplier { get; private set; }

	[LuaDrivable("unitToVoxelDamageMultiplier")]
	public static float UnitToVoxelDamageMultiplier { get; private set; }

	[LuaDrivable("biftronToObjectDamageMultiplier")]
	public static float BiftronToObjectDamageMultiplier { get; private set; }

	[LuaDrivable("arrowDamageMultiplier")]
	public static float ArrowDamageMultiplier { get; private set; }

	[LuaDrivable("arrowSpeedMultiplier")]
	public static float ArrowSpeedMultiplier { get; private set; }

	[LuaDrivable("magicArrows")]
	public static bool UseMagicArrows { get; private set; }

	[LuaDrivable("arrowInaccuracy")]
	public static float ArrowInaccuracy { get; private set; }

	[LuaDrivable("arrowAimTime")]
	public static float ArrowAimTime { get; private set; }

	[LuaDrivable("arrowShootThroughAllies")]
	public static bool ArrowShootThroughAllies { get; private set; }

	[LuaDrivable("stoneKnockback")]
	public static bool StoneKnockback { get; private set; }

	private void Awake()
	{
		_ruleMaster = new CompCache<RuleMaster>(base.gameObject);
		if (_ruleMaster.Get.Priority == -1)
		{
			_main = this;
		}
	}
}
