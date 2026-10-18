using System.Collections.Generic;
using System.Runtime.InteropServices;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Game.AI.KnowledgeSpace;
using Brix.Game.Bricktron;
using Brix.Game.Components;
using Brix.Lua;
using Brix.Network;
using MoonSharp.Interpreter;
using Newtonsoft.Json;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Legacy;

public class SpawnerCristal : NetworkGameComponent
{
	public static GameSignals.GameSignal<SpawnerCristal> CrystalStarted = GameSignals.CreateStaticSignal<SpawnerCristal>();

	public static GameSignals.GameSignal<SpawnerCristal> CrystalDeleted = GameSignals.CreateStaticSignal<SpawnerCristal>();

	public static readonly HashSet<SpawnerCristal> AllCrystals = new HashSet<SpawnerCristal>();

	[JsonProperty]
	public HashSet<SpawnerCristal> KnownCrystals;

	public Signal SelfCrystalDeleted = new Signal();

	[SyncVar(hook = "SetCrystalType")]
	[JsonProperty]
	private int _crystalType;

	[LuaDrivable("hk_OnSetCrystalType")]
	private LuaHook lua_OnSetCrystalType = new LuaHook();

	public GameObject SpawnShards;

	public GameObject CaptureShards;

	private CompCache<FireflyNest> _ffNest;

	[JsonProperty]
	public List<string> RefundedNamesList = new List<string>();

	private SignalConnection FactionChangedConnection = SignalConnection.Empty;

	[LuaDrivable("crystalType")]
	private int lua_crystalType => _crystalType;

	public FireflyNest FireflyNest => _ffNest.Get;

	public int Network_crystalType
	{
		get
		{
			return _crystalType;
		}
		[param: In]
		set
		{
			ref int crystalType = ref _crystalType;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetCrystalType(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref crystalType, 1u);
		}
	}

	public void SetCrystalType(int type)
	{
		Network_crystalType = type;
		SetCrystalTypeAction();
	}

	protected override void Awake()
	{
		base.Awake();
		_ffNest = new CompCache<FireflyNest>(base.gameObject);
		SetCrystalTypeAction();
		if (NetworkServer.active)
		{
			RefundedNamesList.Slinq().ForEach(GestionnaireDeNom.Reserver);
		}
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		SetDirtyBit(1u);
	}

	private void SetCrystalTypeAction()
	{
		if (!SpawnShards.IsNullOrReleased() && !CaptureShards.IsNullOrReleased())
		{
			if (_crystalType == 0)
			{
				SpawnShards.SetActive(value: true);
				CaptureShards.SetActive(value: false);
				GetComponent<CapsuleCollider>().radius = 1f;
			}
			else if (_crystalType == 1)
			{
				SpawnShards.SetActive(value: false);
				CaptureShards.SetActive(value: true);
				GetComponent<CapsuleCollider>().radius = 0.7f;
			}
		}
		LuaHookBase.InvokeFromCLR(lua_OnSetCrystalType);
	}

	[LuaDrivable("SetAsSpawnCrystal")]
	private static DynValue SetAsSpawnCrystal(ScriptExecutionContext context, CallbackArguments args)
	{
		SpawnerCristal spawnerCristal = args.AsComponent<SpawnerCristal>(0, "SetAsSpawnCrystal");
		spawnerCristal.Network_crystalType = 0;
		return DynValue.Void;
	}

	[LuaDrivable("SetAsCaptureCrystal")]
	private static DynValue SetAsCaptureCrystal(ScriptExecutionContext context, CallbackArguments args)
	{
		SpawnerCristal spawnerCristal = args.AsComponent<SpawnerCristal>(0, "SetAsSpawnCrystal");
		spawnerCristal.Network_crystalType = 1;
		return DynValue.Void;
	}

	protected override void Start()
	{
		base.Start();
		GameSignals.Invoke(CrystalStarted, this);
	}

	public override void OnRequested()
	{
		base.OnRequested();
		base.enabled = true;
		AllCrystals.Add(this);
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		FactionChangedConnection.Disconnect();
		GameSignals.Invoke(CrystalDeleted, this);
		SelfCrystalDeleted.Invoke();
		AllCrystals.Remove(this);
	}

	private void Update()
	{
		if (KnownCrystals != null)
		{
			base.enabled = false;
		}
		else
		{
			if (!Knowledge.Instance.KnowsAbout(base.gameObject))
			{
				return;
			}
			KnownCrystals = new HashSet<SpawnerCristal>();
			foreach (SpawnerCristal allCrystal in AllCrystals)
			{
				if (Knowledge.Instance.CanReach(this, allCrystal))
				{
					KnownCrystals.Add(allCrystal);
				}
			}
			base.enabled = false;
		}
	}

	public bool HasPotentialConnection(SpawnerCristal otherCrystal)
	{
		if (KnownCrystals != null && KnownCrystals.Contains(otherCrystal))
		{
			return true;
		}
		return Knowledge.Instance.CanReach(this, otherCrystal);
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.WritePackedUInt32((uint)_crystalType);
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
			writer.WritePackedUInt32((uint)_crystalType);
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
			_crystalType = (int)reader.ReadPackedUInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			SetCrystalType((int)reader.ReadPackedUInt32());
		}
	}
}
