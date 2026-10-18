using System;
using Brix.Engine;
using Brix.Lua;
using Brix.Network.Util;
using MoonSharp.Interpreter;
using Newtonsoft.Json;
using UnityEngine;

namespace Brix.Game.Rules;

[Serializable]
[SerializerTarget(new string[] { "save" })]
[RequireComponent(typeof(RuleMaster))]
[JsonObject(MemberSerialization.OptIn)]
public class TerrainRules : MonoBehaviour
{
	[SerializeField]
	[EnumFlag]
	private Diggables _digRules = Diggables.All;

	[EnumFlag]
	[SerializeField]
	private Diggables _explosionRules = Diggables.All;

	[SerializeField]
	[JsonProperty]
	private bool _useDigRulesForExplosions;

	[JsonProperty]
	[SerializeField]
	private bool _canCutTrees = true;

	public GameSignals.GameSignal<Diggables> OnDigRulesChanged = new GameSignals.GameSignal<Diggables>();

	public GameSignals.GameSignal<Diggables> OnExplosionRulesChanged = new GameSignals.GameSignal<Diggables>();

	private CompCache<RuleMaster> _ruleMaster;

	private static TerrainRules _main;

	public static TerrainRules Main => _main;

	[LuaDrivable("digRules")]
	public Diggables DigRules
	{
		get
		{
			return _digRules;
		}
		set
		{
			_digRules = value;
			GameSignals.Invoke(OnDigRulesChanged, _digRules);
		}
	}

	[LuaDrivable("explosionRules")]
	public Diggables ExplosionRules
	{
		get
		{
			return _explosionRules;
		}
		set
		{
			_explosionRules = value;
			GameSignals.Invoke(OnExplosionRulesChanged, _explosionRules);
		}
	}

	[LuaDrivable("canCutTrees")]
	public bool CanCutTrees
	{
		get
		{
			return _canCutTrees;
		}
		set
		{
			_canCutTrees = value;
		}
	}

	private void Awake()
	{
		_ruleMaster = new CompCache<RuleMaster>(base.gameObject);
		if (_ruleMaster.Get.Priority == -1)
		{
			_main = this;
		}
	}

	public void SetDigRules(Table rules)
	{
		Diggables diggables = Diggables.None;
		foreach (DynValue value in rules.Values)
		{
			if (value.IsNotNil())
			{
				Diggables diggables2 = value.ToEnum<Diggables>();
				if (diggables2 != Diggables.None)
				{
					diggables |= diggables2;
				}
			}
		}
		DigRules = diggables;
	}

	public bool CanDigAt(Vector3 v3)
	{
		if (_ruleMaster.Get.Priority == -1)
		{
			TerrainRules highestPriorityRulesAt = _ruleMaster.Get.GetHighestPriorityRulesAt<TerrainRules>(v3);
			if (highestPriorityRulesAt != null)
			{
				return highestPriorityRulesAt.CanDigAt(v3);
			}
		}
		XYZ position = XYZ.FromVector3(v3);
		using IVoxel voxel = Voxel.GetVoxel(position);
		if (_digRules == Diggables.None)
		{
			return false;
		}
		if (_digRules.HasFlag(Diggables.Crust) && voxel.IsCrust)
		{
			return true;
		}
		if (_digRules.HasFlag(Diggables.Dirt) && (voxel.IsOfType(4) || voxel.IsOfType(6)))
		{
			return true;
		}
		if (_digRules.HasFlag(Diggables.Rock) && (voxel.IsOfType(8) || voxel.IsOfType(38) || voxel.IsOfType(44) || voxel.IsOfType(54)))
		{
			return true;
		}
		if (_digRules.HasFlag(Diggables.Artificial) && !voxel.IsOfType(0) && !voxel.IsOfType(2) && !voxel.IsOfType(4) && !voxel.IsOfType(6) && !voxel.IsOfType(8) && !voxel.IsOfType(38) && !voxel.IsOfType(44) && !voxel.IsOfType(54))
		{
			return true;
		}
		return false;
	}

	public void SetExplosionRules(Table rules)
	{
		Diggables diggables = Diggables.None;
		foreach (DynValue value in rules.Values)
		{
			if (value.IsNotNil())
			{
				Diggables diggables2 = value.ToEnum<Diggables>();
				if (diggables2 != Diggables.None)
				{
					diggables |= diggables2;
				}
			}
		}
		ExplosionRules = diggables;
	}

	public bool CanExplodeAt(Vector3 v3)
	{
		if (_useDigRulesForExplosions)
		{
			return CanDigAt(v3);
		}
		if (_ruleMaster.Get.Priority == -1)
		{
			TerrainRules highestPriorityRulesAt = _ruleMaster.Get.GetHighestPriorityRulesAt<TerrainRules>(v3);
			if (highestPriorityRulesAt != null)
			{
				return highestPriorityRulesAt.CanExplodeAt(v3);
			}
		}
		XYZ position = XYZ.FromVector3(v3);
		using IVoxel voxel = Voxel.GetVoxel(position);
		if (_digRules == Diggables.None)
		{
			return false;
		}
		if (_digRules.HasFlag(Diggables.Crust) && voxel.IsCrust)
		{
			return true;
		}
		if (_digRules.HasFlag(Diggables.Dirt) && voxel.IsTerrain && (voxel.IsOfType(6) || voxel.IsOfType(4)) && !voxel.IsInvincible)
		{
			return true;
		}
		if (_digRules.HasFlag(Diggables.Rock) && (voxel.IsOfType(8) || voxel.IsOfType(38) || voxel.IsOfType(44) || voxel.IsOfType(54)) && !voxel.IsInvincible)
		{
			return true;
		}
		if (_digRules.HasFlag(Diggables.Artificial) && !voxel.IsOfType(0) && !voxel.IsOfType(2) && !voxel.IsOfType(4) && !voxel.IsOfType(6) && !voxel.IsOfType(8) && !voxel.IsOfType(38) && !voxel.IsOfType(44) && !voxel.IsOfType(54))
		{
			return true;
		}
		return false;
	}

	public bool CanCutTreesAt(Vector3 v3)
	{
		if (_ruleMaster.Get.Priority == -1)
		{
			TerrainRules highestPriorityRulesAt = _ruleMaster.Get.GetHighestPriorityRulesAt<TerrainRules>(v3);
			if (highestPriorityRulesAt != null)
			{
				return highestPriorityRulesAt.CanCutTrees;
			}
		}
		return CanCutTrees;
	}

	[LuaDrivable("SetExplosionRules")]
	private static DynValue lua_SetExplosionRules(ScriptExecutionContext context, CallbackArguments args)
	{
		TerrainRules terrainRules = args.AsComponent<TerrainRules>(0, "SetExplosionRules");
		Table explosionRules = args.AsTable(1, "SetExplosionRules");
		terrainRules.SetExplosionRules(explosionRules);
		return DynValue.Void;
	}

	[LuaDrivable("SetDigRules")]
	private static DynValue lua_SetDigRules(ScriptExecutionContext context, CallbackArguments args)
	{
		TerrainRules terrainRules = args.AsComponent<TerrainRules>(0, "SetDigRules");
		Table digRules = args.AsTable(1, "SetDigRules");
		terrainRules.SetDigRules(digRules);
		return DynValue.Void;
	}

	[LuaDrivable("CanExplodeAt")]
	private static DynValue lua_CanExplodeAt(ScriptExecutionContext context, CallbackArguments args)
	{
		TerrainRules terrainRules = args.AsComponent<TerrainRules>(0, "CanExplodeAt");
		Vector3 v = args.AsUserData<Vector3>(1, "CanExplodeAt");
		return DynValue.NewBoolean(terrainRules.CanExplodeAt(v));
	}

	[LuaDrivable("CanDigAt")]
	private static DynValue lua_CanDigAt(ScriptExecutionContext context, CallbackArguments args)
	{
		TerrainRules terrainRules = args.AsComponent<TerrainRules>(0, "CanDigAt");
		Vector3 v = args.AsUserData<Vector3>(1, "CanDigAt");
		return DynValue.NewBoolean(terrainRules.CanDigAt(v));
	}

	[LuaDrivable("CanCutTreesAt")]
	private static DynValue lua_CanCutTreesAt(ScriptExecutionContext context, CallbackArguments args)
	{
		TerrainRules terrainRules = args.AsComponent<TerrainRules>(0, "CanCutTreesAt");
		Vector3 v = args.AsUserData<Vector3>(1, "CanCutTreesAt");
		return DynValue.NewBoolean(terrainRules.CanCutTreesAt(v));
	}
}
