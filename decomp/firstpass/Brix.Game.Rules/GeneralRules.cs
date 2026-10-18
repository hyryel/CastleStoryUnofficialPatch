using System;
using System.Runtime.InteropServices;
using Brix.External.Signals;
using Brix.Lua;
using Brix.Network.Util;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Rules;

[Serializable]
[JsonObject(MemberSerialization.OptIn)]
[RequireComponent(typeof(RuleMaster))]
[SerializerTarget(new string[] { "save" })]
public class GeneralRules : NetworkBehaviour
{
	private CompCache<RuleMaster> _ruleMaster;

	private static GeneralRules _main;

	public Signal CanCaptureCapturablesChanged = new Signal();

	[SyncVar(hook = "canCaptureCapturablesChangedHook")]
	[JsonProperty]
	[SerializeField]
	private bool _canCaptureCapturables;

	public Signal BuilderCanCaptureChanged = new Signal();

	[JsonProperty]
	[SerializeField]
	private bool _builderCanCapture;

	public Signal PlayerRelationsChanged = new Signal();

	[SerializeField]
	private Affiliation.Relation _playerRelations = Affiliation.Relation.Allied;

	public Signal ForcePlayerFirefliesToPlayerCrystalChanged = new Signal();

	[JsonProperty]
	[SerializeField]
	private bool _forcePlayerFirefliesToPlayerCrystal = true;

	private int _bricktronCap = 15;

	private int _bricktronRefund = 100;

	private int _minitronRefund = 10;

	private int _corruptronRefund = 20;

	private int _biftronRefund = 80;

	private int _crawlerRefund = 25;

	private float _proportionalRefund = 1f;

	private bool _respawnWithTools = true;

	[SyncVar]
	[JsonProperty]
	public int homeCrystalSpawnRequiredEnergyBase = 100;

	[SyncVar]
	[JsonProperty]
	public int homeCrystalSpawnRequiredEnergyFactor = 60;

	[JsonProperty]
	[SyncVar]
	public int homeCrystalSpawnRequiredEnergyBeginCount = 6;

	public Signal CanSpawnMilitaryUnitsChanged = new Signal();

	[JsonProperty]
	[SerializeField]
	private bool _canSpawnMilitaryUnits = true;

	public Signal FogOfWarDistanceChanged = new Signal();

	[JsonProperty]
	[SerializeField]
	private int _fogOfWarDistance = 40;

	[LuaDrivable("allowCrafting")]
	public static bool AllowCrafting;

	[LuaDrivable("allowToDropTools")]
	public static bool AllowToDropTools;

	[LuaDrivable("allowPartialToolsets")]
	public static bool AllowPartialToolsets;

	[LuaDrivable("allowMixedToolsets")]
	public static bool AllowMixedToolSets;

	private static int _defaultSearchPathBudget = 100;

	private static int _freeAgentSearchPathBudget = 40;

	private static int _corruptronSearchPathBudget = 20;

	private static int _buildProjectSearchPathBudget = 40;

	private static int _knowledgeUpdaterSearchPathBudget = 100;

	private static int _knowledgeExcluderSearchPathBudget = 40;

	[LuaDrivable("hk_OnSetCanCaptureCapturables")]
	private LuaHook lua_OnSetCanCaptureCapturables = new LuaHook();

	public static GeneralRules Main => _main;

	[LuaDrivable("canCaptureCapturables")]
	public bool CanCaptureCapturables
	{
		get
		{
			return _canCaptureCapturables;
		}
		set
		{
			if (_canCaptureCapturables != value)
			{
				Network_canCaptureCapturables = value;
			}
		}
	}

	[LuaDrivable("builderCanCapture")]
	public bool BuilderCanCapture
	{
		get
		{
			return _builderCanCapture;
		}
		set
		{
			if (_builderCanCapture != value)
			{
				_builderCanCapture = value;
				BuilderCanCaptureChanged.Invoke();
			}
		}
	}

	[LuaDrivable("playerRelations")]
	public Affiliation.Relation PlayerRelations
	{
		get
		{
			return _playerRelations;
		}
		set
		{
			if (_playerRelations != value)
			{
				_playerRelations = value;
				PlayerRelationsChanged.Invoke();
			}
		}
	}

	[LuaDrivable("forcePlayerFirefliesToPlayerCrystal")]
	public bool ForcePlayerFirefliesToPlayerCrystal
	{
		get
		{
			return _forcePlayerFirefliesToPlayerCrystal;
		}
		set
		{
			if (_forcePlayerFirefliesToPlayerCrystal != value)
			{
				_forcePlayerFirefliesToPlayerCrystal = value;
				ForcePlayerFirefliesToPlayerCrystalChanged.Invoke();
			}
		}
	}

	[LuaDrivable("bricktronCap")]
	public int bricktronCap
	{
		get
		{
			return _bricktronCap;
		}
		set
		{
			_bricktronCap = value;
		}
	}

	[LuaDrivable("bricktronRefund")]
	public int bricktronRefund
	{
		get
		{
			return _bricktronRefund;
		}
		set
		{
			_bricktronRefund = value;
		}
	}

	[LuaDrivable("minitronRefund")]
	public int minitronRefund
	{
		get
		{
			return _minitronRefund;
		}
		set
		{
			_minitronRefund = value;
		}
	}

	[LuaDrivable("corruptronRefund")]
	public int corruptronRefund
	{
		get
		{
			return _corruptronRefund;
		}
		set
		{
			_corruptronRefund = value;
		}
	}

	[LuaDrivable("biftronRefund")]
	public int biftronRefund
	{
		get
		{
			return _biftronRefund;
		}
		set
		{
			_biftronRefund = value;
		}
	}

	[LuaDrivable("crawlerRefund")]
	public int crawlerRefund
	{
		get
		{
			return _crawlerRefund;
		}
		set
		{
			_crawlerRefund = value;
		}
	}

	[LuaDrivable("proportionalRefund")]
	public float proportionalRefund
	{
		get
		{
			return _proportionalRefund;
		}
		set
		{
			_proportionalRefund = value;
		}
	}

	[LuaDrivable("respawnWithTools")]
	public bool RespawnWithTools
	{
		get
		{
			return _respawnWithTools;
		}
		set
		{
			_respawnWithTools = value;
		}
	}

	[LuaDrivable("canSpawnMilitaryUnits")]
	public bool CanSpawnMilitaryUnits
	{
		get
		{
			return _canSpawnMilitaryUnits;
		}
		set
		{
			if (_canSpawnMilitaryUnits != value)
			{
				_canSpawnMilitaryUnits = value;
				CanSpawnMilitaryUnitsChanged.Invoke();
			}
		}
	}

	[LuaDrivable("fogOfWarDistance")]
	public int FogOfWarDistance
	{
		get
		{
			return _fogOfWarDistance;
		}
		set
		{
			if (_fogOfWarDistance != value)
			{
				_fogOfWarDistance = value;
				FogOfWarDistanceChanged.Invoke();
			}
		}
	}

	[LuaDrivable("allowMovingWards")]
	public static bool AllowMovingWards { get; private set; }

	[LuaDrivable("allowStoringBarrels")]
	public static bool AllowStoringBarrels { get; private set; }

	[LuaDrivable("brimstoneBurnTimeMs")]
	public static int BrimstoneBurnTimeMs { get; private set; }

	[LuaDrivable("allowPauseInMultiplayer")]
	public static bool AllowPauseInMultiplayer { get; private set; }

	[LuaDrivable("scarfsInSinglePlayer")]
	public static bool ScarfsInSinglePlayer { get; private set; }

	[LuaDrivable("defaultSearchPathBudget")]
	public static int DefaultSearchPathBudget
	{
		get
		{
			return _defaultSearchPathBudget;
		}
		private set
		{
			_defaultSearchPathBudget = value;
		}
	}

	[LuaDrivable("freeAgentSearchPathBudget")]
	public static int FreeAgentSearchPathBudget
	{
		get
		{
			return _freeAgentSearchPathBudget;
		}
		private set
		{
			_freeAgentSearchPathBudget = value;
		}
	}

	[LuaDrivable("corruptronSearchPathBudget")]
	public static int CorruptronSearchPathBudget
	{
		get
		{
			return _corruptronSearchPathBudget;
		}
		private set
		{
			_corruptronSearchPathBudget = value;
		}
	}

	[LuaDrivable("buildProjectSearchPathBudget")]
	public static int BuildProjectSearchPathBudget
	{
		get
		{
			return _buildProjectSearchPathBudget;
		}
		private set
		{
			_buildProjectSearchPathBudget = value;
		}
	}

	[LuaDrivable("knowledgeUpdaterSearchPathBudget ")]
	public static int KnowledgeUpdaterSearchPathBudget
	{
		get
		{
			return _knowledgeUpdaterSearchPathBudget;
		}
		private set
		{
			_knowledgeUpdaterSearchPathBudget = value;
		}
	}

	[LuaDrivable("knowledgeExcluderSearchPathBudget ")]
	public static int KnowledgeExcluderSearchPathBudget
	{
		get
		{
			return _knowledgeExcluderSearchPathBudget;
		}
		private set
		{
			_knowledgeExcluderSearchPathBudget = value;
		}
	}

	[LuaDrivable("homeCrystalSpawnRequiredEnergyBase")]
	private int lua_homeCrystalSpawnRequiredEnergyBase
	{
		get
		{
			return homeCrystalSpawnRequiredEnergyBase;
		}
		set
		{
			NetworkhomeCrystalSpawnRequiredEnergyBase = value;
		}
	}

	[LuaDrivable("homeCrystalSpawnRequiredEnergyFactor")]
	private int lua_homeCrystalSpawnRequiredEnergyFactor
	{
		get
		{
			return homeCrystalSpawnRequiredEnergyFactor;
		}
		set
		{
			NetworkhomeCrystalSpawnRequiredEnergyFactor = value;
		}
	}

	[LuaDrivable("homeCrystalSpawnRequiredEnergyBeginCount")]
	private int lua_homeCrystalSpawnRequiredEnergyBeginCount
	{
		get
		{
			return homeCrystalSpawnRequiredEnergyBeginCount;
		}
		set
		{
			NetworkhomeCrystalSpawnRequiredEnergyBeginCount = value;
		}
	}

	public bool Network_canCaptureCapturables
	{
		get
		{
			return _canCaptureCapturables;
		}
		[param: In]
		set
		{
			ref bool canCaptureCapturables = ref _canCaptureCapturables;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				canCaptureCapturablesChangedHook(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref canCaptureCapturables, 1u);
		}
	}

	public int NetworkhomeCrystalSpawnRequiredEnergyBase
	{
		get
		{
			return homeCrystalSpawnRequiredEnergyBase;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref homeCrystalSpawnRequiredEnergyBase, 2u);
		}
	}

	public int NetworkhomeCrystalSpawnRequiredEnergyFactor
	{
		get
		{
			return homeCrystalSpawnRequiredEnergyFactor;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref homeCrystalSpawnRequiredEnergyFactor, 4u);
		}
	}

	public int NetworkhomeCrystalSpawnRequiredEnergyBeginCount
	{
		get
		{
			return homeCrystalSpawnRequiredEnergyBeginCount;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref homeCrystalSpawnRequiredEnergyBeginCount, 8u);
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

	private void OnDestroy()
	{
		CanCaptureCapturablesChanged.Clear();
		CanSpawnMilitaryUnitsChanged.Clear();
		PlayerRelationsChanged.Clear();
		ForcePlayerFirefliesToPlayerCrystalChanged.Clear();
	}

	private void canCaptureCapturablesChangedHook(bool value)
	{
		Network_canCaptureCapturables = value;
		CanCaptureCapturablesChanged.Invoke();
		LuaHookBase.InvokeFromCLR(lua_OnSetCanCaptureCapturables);
	}

	public int GetHomeCrystalSpawnRequiredEnergy(int activeCharacters)
	{
		return homeCrystalSpawnRequiredEnergyBase + homeCrystalSpawnRequiredEnergyFactor * Mathf.Max(activeCharacters - homeCrystalSpawnRequiredEnergyBeginCount, 0);
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(_canCaptureCapturables);
			writer.WritePackedUInt32((uint)homeCrystalSpawnRequiredEnergyBase);
			writer.WritePackedUInt32((uint)homeCrystalSpawnRequiredEnergyFactor);
			writer.WritePackedUInt32((uint)homeCrystalSpawnRequiredEnergyBeginCount);
			return true;
		}
		bool flag = false;
		if ((base.syncVarDirtyBits & 1) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(_canCaptureCapturables);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)homeCrystalSpawnRequiredEnergyBase);
		}
		if ((base.syncVarDirtyBits & 4) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)homeCrystalSpawnRequiredEnergyFactor);
		}
		if ((base.syncVarDirtyBits & 8) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)homeCrystalSpawnRequiredEnergyBeginCount);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			_canCaptureCapturables = reader.ReadBoolean();
			homeCrystalSpawnRequiredEnergyBase = (int)reader.ReadPackedUInt32();
			homeCrystalSpawnRequiredEnergyFactor = (int)reader.ReadPackedUInt32();
			homeCrystalSpawnRequiredEnergyBeginCount = (int)reader.ReadPackedUInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			canCaptureCapturablesChangedHook(reader.ReadBoolean());
		}
		if ((num & 2) != 0)
		{
			homeCrystalSpawnRequiredEnergyBase = (int)reader.ReadPackedUInt32();
		}
		if ((num & 4) != 0)
		{
			homeCrystalSpawnRequiredEnergyFactor = (int)reader.ReadPackedUInt32();
		}
		if ((num & 8) != 0)
		{
			homeCrystalSpawnRequiredEnergyBeginCount = (int)reader.ReadPackedUInt32();
		}
	}
}
