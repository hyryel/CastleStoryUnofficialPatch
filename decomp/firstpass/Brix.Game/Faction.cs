using System.Collections.Generic;
using System.Runtime.InteropServices;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Lua;
using Brix.Network;
using Brix.Network.Util;
using Brix.UI.Icons;
using Brix.Utils.Factories;
using Brix.Utils.Signals;
using MoonSharp.Interpreter;
using Newtonsoft.Json;
using Smooth.Slinq;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game;

[JsonObject(MemberSerialization.OptIn)]
[SerializerTarget(new string[] { "save" })]
public class Faction : NetworkGameComponent
{
	public class SyncListGameObject : SyncList<GameObject>
	{
		protected override void SerializeItem(NetworkWriter writer, GameObject item)
		{
			writer.Write(item);
		}

		protected override GameObject DeserializeItem(NetworkReader reader)
		{
			return reader.ReadGameObject();
		}
	}

	public struct FactionMeta
	{
		public string name;

		public string ownerName;

		public string materialPath;

		public Color color;

		public bool assigned;
	}

	private static readonly List<Faction> factions;

	private static Signal<Faction> _created;

	public Dictionary<Faction, Affiliation.Relation> AffiliationMap = new Dictionary<Faction, Affiliation.Relation>();

	public SyncListGameObject OwnerSyncList = new SyncListGameObject();

	public SyncListGameObject SyncListAllies = new SyncListGameObject();

	public SyncListGameObject SyncListNeutral = new SyncListGameObject();

	public SyncListGameObject SyncListEnemies = new SyncListGameObject();

	public List<Faction> EnemyFactions = new List<Faction>();

	public List<Faction> NeutralFactions = new List<Faction>();

	public List<Faction> AllyFactions = new List<Faction>();

	public List<Faction> EnemyOrNeutralFactions = new List<Faction>();

	public SharedFactionVision SharedVision;

	[SyncVar]
	public int ID;

	public Signal MaximumPopulationChanged = new Signal();

	[SyncVar(hook = "maximumPopulationChanged")]
	[JsonProperty]
	public int MaximumPopulation = -1;

	[SyncVar(hook = "OnNameChanged")]
	public new string name;

	[LuaDrivable("GatherPoint")]
	[JsonProperty]
	public Vector3 Gatherpoint;

	[JsonProperty]
	public bool usesFireflies;

	[JsonProperty]
	private bool assigned;

	[SyncVar]
	public bool UseCreepShader;

	[SyncVar]
	public bool isPlayable;

	[SyncVar]
	public bool HasGlobalFogVision;

	[SyncVar]
	public bool isAI;

	[SyncVar]
	public bool IsSpectator;

	[LuaDrivable("factionIcon")]
	public Sprite FactionIcon;

	[LuaDrivable("crystalIcon")]
	public Sprite CrystalIcon;

	[SyncVar(hook = "OnFactionColorChanged")]
	public Color FactionColor;

	public Color CaptureCircleColor;

	public Color LightColor;

	public FactionStorage Storage;

	public CrystalVisuals.CrystalColorEnum CrystalColor;

	public static Dictionary<string, IconKey> IconKeyMap;

	private int _unitCount;

	private static Faction _defaultAllied;

	private static Faction _defaultEnemy;

	private static Faction _defaultNeutral;

	private static Faction _spectator;

	private static int kListOwnerSyncList;

	private static int kListSyncListAllies;

	private static int kListSyncListNeutral;

	private static int kListSyncListEnemies;

	public static IReadOnlySignal<Faction> Created => _created;

	public List<Faction> AsList { get; private set; }

	[LuaDrivable("FactionName")]
	public string lua_FactionName => name;

	[LuaDrivable("hasGlobalFogVision")]
	private bool lua_hasGlobalFogVision
	{
		get
		{
			return HasGlobalFogVision;
		}
		set
		{
			NetworkHasGlobalFogVision = value;
		}
	}

	[LuaDrivable("isAI")]
	private bool lua_isAI
	{
		get
		{
			return isAI;
		}
		set
		{
			NetworkisAI = value;
		}
	}

	[LuaDrivable("factionColor")]
	private Color lua_FactionColor
	{
		get
		{
			return FactionColor;
		}
		set
		{
			NetworkFactionColor = value;
		}
	}

	[LuaDrivable("owner")]
	[SerializeField]
	public User Owner
	{
		get
		{
			GameObject gameObject = OwnerSyncList.Slinq().FirstOrDefault();
			return (!(gameObject != null)) ? null : gameObject.GetComponent<User>();
		}
	}

	[LuaDrivable("allOwners")]
	public List<User> AllOwners => (from player in OwnerSyncList.Slinq()
		select player.GetComponent<User>()).ToList();

	[LuaDrivable("unitCount")]
	public int UnitCount => _unitCount;

	[LuaDrivable("defaultAllied")]
	public static Faction DefaultAllied => User.LocalUser.faction;

	[LuaDrivable("defaultEnemy")]
	public static Faction DefaultEnemy
	{
		get
		{
			return (_defaultEnemy != null) ? _defaultEnemy : (_defaultEnemy = factions.Slinq().FirstOrDefault((Faction f) => f.isAI));
		}
		set
		{
			_defaultEnemy = value;
		}
	}

	[LuaDrivable("defaultNeutral")]
	public static Faction DefaultNeutral
	{
		get
		{
			return (_defaultNeutral != null) ? _defaultNeutral : (_defaultNeutral = FindByName("Neutral"));
		}
		set
		{
			_defaultNeutral = value;
		}
	}

	[LuaDrivable("spectator")]
	public static Faction Spectator
	{
		get
		{
			return (_spectator != null) ? _spectator : (_spectator = factions.Slinq().FirstOrDefault((Faction f) => f.IsSpectator));
		}
		set
		{
			_spectator = value;
		}
	}

	public override bool IsOwnedByMe => this == DefaultAllied;

	public bool IsAlliedOrNeutralToMe => Affiliation.IsAlliedOrNeutral(this, DefaultAllied);

	public bool IsAlliedToMe => Affiliation.IsAllied(this, DefaultAllied);

	public bool IsEnemyToMe => Affiliation.IsEnemy(this, DefaultAllied);

	public bool IsNeutralToMe => Affiliation.IsNeutral(this, DefaultAllied);

	public int NetworkID
	{
		get
		{
			return ID;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref ID, 16u);
		}
	}

	public int NetworkMaximumPopulation
	{
		get
		{
			return MaximumPopulation;
		}
		[param: In]
		set
		{
			ref int maximumPopulation = ref MaximumPopulation;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				maximumPopulationChanged(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref maximumPopulation, 32u);
		}
	}

	public string Networkname
	{
		get
		{
			return name;
		}
		[param: In]
		set
		{
			ref string fieldValue = ref name;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				OnNameChanged(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 64u);
		}
	}

	public bool NetworkUseCreepShader
	{
		get
		{
			return UseCreepShader;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref UseCreepShader, 128u);
		}
	}

	public bool NetworkisPlayable
	{
		get
		{
			return isPlayable;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref isPlayable, 256u);
		}
	}

	public bool NetworkHasGlobalFogVision
	{
		get
		{
			return HasGlobalFogVision;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref HasGlobalFogVision, 512u);
		}
	}

	public bool NetworkisAI
	{
		get
		{
			return isAI;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref isAI, 1024u);
		}
	}

	public bool NetworkIsSpectator
	{
		get
		{
			return IsSpectator;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref IsSpectator, 2048u);
		}
	}

	public Color NetworkFactionColor
	{
		get
		{
			return FactionColor;
		}
		[param: In]
		set
		{
			ref Color factionColor = ref FactionColor;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				OnFactionColorChanged(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref factionColor, 4096u);
		}
	}

	public override void Initialize()
	{
		base.Initialize();
		AsList = new List<Faction>(1) { this };
	}

	private void maximumPopulationChanged(int value)
	{
		NetworkMaximumPopulation = value;
		MaximumPopulationChanged.Invoke();
	}

	public void SetMaximumPopulation(int value)
	{
		NetworkMaximumPopulation = value;
	}

	public int GetMaximumPopulation()
	{
		return MaximumPopulation;
	}

	public void OnFactionColorChanged(Color newColor)
	{
		NetworkFactionColor = newColor;
	}

	public void OnNameChanged(string newName)
	{
		Networkname = newName;
		base.gameObject.name = name;
		base.gameObject.SetId(name);
	}

	public void GenerateFlagIconFromName()
	{
		FactionIcon = IconDB.GetIcon(new IconKey("Flags", "Flag" + name)).Image64;
	}

	public bool HasOwner()
	{
		return !IsSpectator && OwnerSyncList.Slinq().Any();
	}

	public static List<Faction> GetAllFactions()
	{
		return factions;
	}

	public static FactionMeta[] GetAllFactionsMeta()
	{
		FactionMeta[] array = new FactionMeta[GetAllFactions().Count];
		int num = 0;
		foreach (Faction allFaction in GetAllFactions())
		{
			array[num].name = allFaction.name;
			array[num].color = allFaction.FactionColor;
			array[num].ownerName = ((!(allFaction.Owner == null)) ? allFaction.Owner.ScreenName : "Unassigned");
			array[num].assigned = allFaction.assigned;
			num++;
		}
		return array;
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		GetComponent<AffiliationComponent>().faction = this;
		if (!string.IsNullOrEmpty(name))
		{
			OnNameChanged(name);
		}
		_created.Invoke(this);
		SharedVision = base.gameObject.GetOrAddComponent<SharedFactionVision>();
		SharedVision.Faction = this;
	}

	public override void OnStartServerOrHost()
	{
		base.OnStartServerOrHost();
		SyncListEnemies.Callback = EnemyListChangedServer;
		SyncListAllies.Callback = AllyListChangedServer;
		SyncListNeutral.Callback = NeutralListChangedServer;
		GameSignals.LateConnectOnce(GameSignals.LevelReady, delegate
		{
			SetDirtyBit(100000u);
		});
	}

	public override void OnStartPureClient()
	{
		base.OnStartPureClient();
		OwnerSyncList.Callback = OwnerListChanged;
		SyncListAllies.Callback = AlliesListChanged;
		SyncListNeutral.Callback = NeutralListChanged;
		SyncListEnemies.Callback = EnemyListChangedClient;
	}

	public void AddOwner(User user)
	{
		if (NetworkServer.active && !OwnerSyncList.Contains(user.gameObject))
		{
			OwnerSyncList.Add(user.gameObject);
			assigned = true;
		}
		user.faction = this;
	}

	public void RemoveOwner(User user)
	{
		if (NetworkServer.active && OwnerSyncList.Contains(user.gameObject))
		{
			OwnerSyncList.Remove(user.gameObject);
		}
	}

	private void AlliesListChanged(SyncList<GameObject>.Operation op, int itemIndex)
	{
		switch (op)
		{
		case SyncList<GameObject>.Operation.OP_ADD:
			SetFactionRelation(SyncListAllies[itemIndex].GetComponent<Faction>(), Affiliation.Relation.Allied);
			break;
		}
	}

	private void NeutralListChanged(SyncList<GameObject>.Operation op, int itemIndex)
	{
		switch (op)
		{
		case SyncList<GameObject>.Operation.OP_ADD:
			SetFactionRelation(SyncListNeutral[itemIndex].GetComponent<Faction>(), Affiliation.Relation.Neutral);
			break;
		}
	}

	private void EnemyListChangedClient(SyncList<GameObject>.Operation op, int itemIndex)
	{
		switch (op)
		{
		case SyncList<GameObject>.Operation.OP_ADD:
			SetFactionRelation(SyncListEnemies[itemIndex].GetComponent<Faction>(), Affiliation.Relation.Enemy);
			break;
		}
		EnemyFactions = (from go in SyncListEnemies.Slinq()
			select go.GetComponent<Faction>()).ToList();
		EnemyOrNeutralFactions = EnemyFactions.Slinq().Concat(NeutralFactions.Slinq()).ToList();
	}

	private void EnemyListChangedServer(SyncList<GameObject>.Operation op, int itemIndex)
	{
		EnemyFactions = (from go in SyncListEnemies.Slinq()
			select go.GetComponent<Faction>()).ToList();
		EnemyOrNeutralFactions = EnemyFactions.Slinq().Concat(NeutralFactions.Slinq()).ToList();
	}

	private void AllyListChangedServer(SyncList<GameObject>.Operation op, int itemIndex)
	{
		AllyFactions = (from go in SyncListAllies.Slinq()
			select go.GetComponent<Faction>()).ToList();
	}

	private void NeutralListChangedServer(SyncList<GameObject>.Operation op, int itemIndex)
	{
		NeutralFactions = (from go in SyncListNeutral.Slinq()
			select go.GetComponent<Faction>()).ToList();
		EnemyOrNeutralFactions = EnemyFactions.Slinq().Concat(NeutralFactions.Slinq()).ToList();
	}

	private void OwnerListChanged(SyncList<GameObject>.Operation op, int itemIndex)
	{
		switch (op)
		{
		}
	}

	public Affiliation.Relation GetRelation(Faction faction)
	{
		return AffiliationMap.GetValueOrDefault(faction, Affiliation.Relation.Undefined);
	}

	public void SetFactionRelation(Faction faction, Affiliation.Relation relation)
	{
		if (faction == this)
		{
			return;
		}
		AffiliationMap[faction] = relation;
		if (NetworkServer.active)
		{
			if (SyncListAllies.Contains(faction.gameObject))
			{
				SyncListAllies.Remove(faction.gameObject);
			}
			if (SyncListNeutral.Contains(faction.gameObject))
			{
				SyncListNeutral.Remove(faction.gameObject);
			}
			if (SyncListEnemies.Contains(faction.gameObject))
			{
				SyncListEnemies.Remove(faction.gameObject);
			}
			switch (relation)
			{
			case Affiliation.Relation.Allied:
				SyncListAllies.Add(faction.gameObject);
				break;
			case Affiliation.Relation.Neutral:
				SyncListNeutral.Add(faction.gameObject);
				break;
			case Affiliation.Relation.Enemy:
				SyncListEnemies.Add(faction.gameObject);
				break;
			}
		}
	}

	public bool IsMemberOfFaction(User user)
	{
		return OwnerSyncList.Contains(user.gameObject);
	}

	protected override void Awake()
	{
		base.Awake();
		factions.Add(this);
		GameSignals.LateConnectOnce(GameSignals.LevelReady, delegate(Faction self)
		{
			self.CheckForOwner();
		}, this).DisconnectWhen(base.OnReleaseOrDestroySignal);
		GameSignals.Connect(AffiliationComponent.AnyObjectChangedFaction, delegate(GameObject o, Faction a, Faction b, Faction self)
		{
			self.AnyUnitChangedFaction(o, a, b);
		}, this).DisconnectWhen(base.OnReleaseOrDestroySignal);
		GameSignals.Connect(Labor.LaborDeleted, delegate(Labor labor, Faction self)
		{
			self.RemoveUnit(labor);
		}, this).DisconnectWhen(base.OnReleaseOrDestroySignal);
		OwnerSyncList.InitializeBehaviour(this, kListOwnerSyncList);
		SyncListAllies.InitializeBehaviour(this, kListSyncListAllies);
		SyncListNeutral.InitializeBehaviour(this, kListSyncListNeutral);
		SyncListEnemies.InitializeBehaviour(this, kListSyncListEnemies);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		factions.Remove(this);
	}

	private void RemoveUnit(Labor labor)
	{
		if (labor.faction == this)
		{
			_unitCount--;
		}
	}

	private void AnyUnitChangedFaction(GameObject unit, Faction oldFaction, Faction newFaction)
	{
		Labor component = unit.GetComponent<Labor>();
		if (component != null)
		{
			if (newFaction == this)
			{
				_unitCount++;
			}
			if (oldFaction == this)
			{
				_unitCount--;
			}
		}
	}

	public static Faction GetFaction(GameObject go)
	{
		AffiliationComponent component = go.GetComponent<AffiliationComponent>();
		if (component != null)
		{
			Faction faction = component.faction;
			if (faction != null)
			{
				return faction;
			}
		}
		return GetDefault(go);
	}

	public bool IsSame(GameObject go)
	{
		return go.WithComponent((AffiliationComponent f, Faction self) => f.faction == self, _default: false, this);
	}

	public bool IsAlliedTo(GameObject go)
	{
		return Affiliation.IsAllied(this, go);
	}

	public bool IsAlliedOrNeutral(GameObject go)
	{
		return Affiliation.IsAlliedOrNeutral(this, go);
	}

	public bool IsNeutralTo(GameObject go)
	{
		return Affiliation.IsNeutral(base.gameObject, go);
	}

	public bool IsEnemyOrNeutralTo(GameObject go)
	{
		return Affiliation.IsEnemyOrNeutral(base.gameObject, go);
	}

	public bool IsEnemyTo(GameObject go)
	{
		return Affiliation.IsEnemy(base.gameObject, go);
	}

	private void CheckForOwner()
	{
	}

	public static bool DefaultIsMine(GameObject g)
	{
		return GetDefault(g).IsOwnedByMe;
	}

	public static Faction GetDefault(GameObject g)
	{
		return DefaultNeutral;
	}

	public static Faction FindByName(string name)
	{
		return factions.Slinq().FirstOrDefault((Faction f) => f.name == name);
	}

	public void SetFaction(GameObject go)
	{
		if ((bool)faction)
		{
			AffiliationComponent component = go.GetComponent<AffiliationComponent>();
			if (component != null)
			{
				component.faction = faction;
			}
		}
	}

	public static int GetFirstAvalaibleFaction()
	{
		int maxFactionCount = GameParam.MaxFactionCount;
		bool flag = maxFactionCount <= UNetManager.instance.Lobby_AssignedFaction().Count;
		GameParam.FactionInfo[] currentFactionInfos = GameParam.CurrentFactionInfos;
		for (int i = 0; i < currentFactionInfos.Length; i++)
		{
			if (IsFactionEmpty(i) && !flag)
			{
				return i;
			}
			if (!IsFactionEmpty(i) && flag)
			{
				return i;
			}
			if (currentFactionInfos.Length == i)
			{
				return i;
			}
		}
		return 0;
	}

	public static int LobbyPlayerCountForFaction(int factionNumber)
	{
		return (from x in UNetManager.instance.LobbyPlayerList.Slinq()
			where x.factionNumber.Equals(factionNumber)
			select x).Count();
	}

	public static bool IsFactionEmpty(int factionNumber)
	{
		return !UNetManager.instance.LobbyPlayerList.Find((LobbyPlayer x) => x.factionNumber.Equals(factionNumber));
	}

	public static bool IsTeamEmpty(int factionNumber)
	{
		return !UNetManager.instance.LobbyPlayerList.Find((LobbyPlayer x) => x.TeamNumber.Equals(factionNumber));
	}

	public static int GetFirstAvalaibleTeam()
	{
		int maxTeamCount = GameParam.MaxTeamCount;
		for (int i = 0; i < maxTeamCount; i++)
		{
			if (IsTeamEmpty(i))
			{
				return i;
			}
		}
		return 0;
	}

	[LuaDrivable("SetMaximumPopulation")]
	public static DynValue SetMaximumPopulation(ScriptExecutionContext context, CallbackArguments args)
	{
		Faction faction = args.AsComponent<Faction>(0, "SetMaximumPopulation");
		int maximumPopulation = args.AsInt(1, "SetMaximumPopulation");
		faction.SetMaximumPopulation(maximumPopulation);
		return DynValue.Void;
	}

	[LuaDrivable("GetMaximumPopulation")]
	public static DynValue GetMaximumPopulation(ScriptExecutionContext context, CallbackArguments args)
	{
		Faction faction = args.AsComponent<Faction>(0, "GetMaximumPopulation");
		return DynValue.NewNumber(faction.GetMaximumPopulation());
	}

	[LuaDrivable("HasOwner")]
	private static DynValue lua_HasOwner(ScriptExecutionContext context, CallbackArguments args)
	{
		Faction faction = args.AsComponent<Faction>(0, "HasOwner");
		return DynValue.NewBoolean(faction.HasOwner());
	}

	[LuaDrivable("IsAlliedTo")]
	private static DynValue lua_IsAlliedTo(ScriptExecutionContext context, CallbackArguments args)
	{
		GameObject a = args.AsGameObject(0, "IsAlliedTo");
		GameObject b = args.AsGameObject(1, "IsAlliedTo");
		return DynValue.NewBoolean(Affiliation.IsAllied(a, b));
	}

	[LuaDrivable("IsAlliedOrNeutralTo")]
	private static DynValue lua_IsAlliedOrNeutral(ScriptExecutionContext context, CallbackArguments args)
	{
		GameObject a = args.AsGameObject(0, "IsAlliedOrNeutral");
		GameObject b = args.AsGameObject(1, "IsAlliedOrNeutral");
		return DynValue.NewBoolean(Affiliation.IsAlliedOrNeutral(a, b));
	}

	[LuaDrivable("IsNeutralTo")]
	private static DynValue lua_IsNeutralTo(ScriptExecutionContext context, CallbackArguments args)
	{
		GameObject a = args.AsGameObject(0, "IsNeutralTo");
		GameObject b = args.AsGameObject(1, "IsNeutralTo");
		return DynValue.NewBoolean(Affiliation.IsNeutral(a, b));
	}

	[LuaDrivable("IsEnemyOrNeutralTo")]
	private static DynValue lua_IsEnemyOrNeutralTo(ScriptExecutionContext context, CallbackArguments args)
	{
		GameObject a = args.AsGameObject(0, "IsEnemyOrNeutralTo");
		GameObject b = args.AsGameObject(1, "IsEnemyOrNeutralTo");
		return DynValue.NewBoolean(Affiliation.IsEnemyOrNeutral(a, b));
	}

	[LuaDrivable("IsEnemyTo")]
	private static DynValue lua_IsEnemyTo(ScriptExecutionContext context, CallbackArguments args)
	{
		GameObject a = args.AsGameObject(0, "IsEnemyTo");
		GameObject b = args.AsGameObject(1, "IsEnemyTo");
		return DynValue.NewBoolean(Affiliation.IsEnemy(a, b));
	}

	[LuaDrivable("FindByName")]
	private static DynValue lua_FindByName(ScriptExecutionContext context, CallbackArguments args)
	{
		string text = args.AsString(0, "FindByName");
		return DynValue.FromObject(context.OwnerScript, FindByName(text));
	}

	[LuaDrivable("AllVisible")]
	private static DynValue lua_AllVisible(ScriptExecutionContext context, CallbackArguments args)
	{
		bool flag = args.AsBool(0, "AllVisible");
		LocalFactionVision.ToggleFogSystem(!flag);
		return DynValue.Void;
	}

	static Faction()
	{
		factions = new List<Faction>();
		_created = new Signal<Faction>();
		IconKeyMap = new Dictionary<string, IconKey>
		{
			{
				"Blue",
				IconKeys.FlagBlue
			},
			{
				"Green",
				IconKeys.FlagGreen
			},
			{
				"Orange",
				IconKeys.FlagOrange
			},
			{
				"Purple",
				IconKeys.FlagPurple
			},
			{
				"Red",
				IconKeys.FlagRed
			},
			{
				"Yellow",
				IconKeys.FlagYellow
			}
		};
		kListOwnerSyncList = 388376078;
		NetworkBehaviour.RegisterSyncListDelegate(typeof(Faction), kListOwnerSyncList, InvokeSyncListOwnerSyncList);
		kListSyncListAllies = 422240429;
		NetworkBehaviour.RegisterSyncListDelegate(typeof(Faction), kListSyncListAllies, InvokeSyncListSyncListAllies);
		kListSyncListNeutral = -1334555248;
		NetworkBehaviour.RegisterSyncListDelegate(typeof(Faction), kListSyncListNeutral, InvokeSyncListSyncListNeutral);
		kListSyncListEnemies = -489484817;
		NetworkBehaviour.RegisterSyncListDelegate(typeof(Faction), kListSyncListEnemies, InvokeSyncListSyncListEnemies);
		NetworkCRC.RegisterBehaviour("Faction", 0);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeSyncListOwnerSyncList(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("SyncList OwnerSyncList called on server.");
		}
		else
		{
			((Faction)obj).OwnerSyncList.HandleMsg(reader);
		}
	}

	protected static void InvokeSyncListSyncListAllies(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("SyncList SyncListAllies called on server.");
		}
		else
		{
			((Faction)obj).SyncListAllies.HandleMsg(reader);
		}
	}

	protected static void InvokeSyncListSyncListNeutral(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("SyncList SyncListNeutral called on server.");
		}
		else
		{
			((Faction)obj).SyncListNeutral.HandleMsg(reader);
		}
	}

	protected static void InvokeSyncListSyncListEnemies(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("SyncList SyncListEnemies called on server.");
		}
		else
		{
			((Faction)obj).SyncListEnemies.HandleMsg(reader);
		}
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			GeneratedNetworkCode._WriteSyncListGameObject_Faction(writer, OwnerSyncList);
			GeneratedNetworkCode._WriteSyncListGameObject_Faction(writer, SyncListAllies);
			GeneratedNetworkCode._WriteSyncListGameObject_Faction(writer, SyncListNeutral);
			GeneratedNetworkCode._WriteSyncListGameObject_Faction(writer, SyncListEnemies);
			writer.WritePackedUInt32((uint)ID);
			writer.WritePackedUInt32((uint)MaximumPopulation);
			writer.Write(name);
			writer.Write(UseCreepShader);
			writer.Write(isPlayable);
			writer.Write(HasGlobalFogVision);
			writer.Write(isAI);
			writer.Write(IsSpectator);
			writer.Write(FactionColor);
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
			GeneratedNetworkCode._WriteSyncListGameObject_Faction(writer, OwnerSyncList);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			GeneratedNetworkCode._WriteSyncListGameObject_Faction(writer, SyncListAllies);
		}
		if ((base.syncVarDirtyBits & 4) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			GeneratedNetworkCode._WriteSyncListGameObject_Faction(writer, SyncListNeutral);
		}
		if ((base.syncVarDirtyBits & 8) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			GeneratedNetworkCode._WriteSyncListGameObject_Faction(writer, SyncListEnemies);
		}
		if ((base.syncVarDirtyBits & 0x10) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.WritePackedUInt32((uint)ID);
		}
		if ((base.syncVarDirtyBits & 0x20) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.WritePackedUInt32((uint)MaximumPopulation);
		}
		if ((base.syncVarDirtyBits & 0x40) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(name);
		}
		if ((base.syncVarDirtyBits & 0x80) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(UseCreepShader);
		}
		if ((base.syncVarDirtyBits & 0x100) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(isPlayable);
		}
		if ((base.syncVarDirtyBits & 0x200) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(HasGlobalFogVision);
		}
		if ((base.syncVarDirtyBits & 0x400) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(isAI);
		}
		if ((base.syncVarDirtyBits & 0x800) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(IsSpectator);
		}
		if ((base.syncVarDirtyBits & 0x1000) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(FactionColor);
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
			OwnerSyncList = GeneratedNetworkCode._ReadSyncListGameObject_Faction(reader);
			SyncListAllies = GeneratedNetworkCode._ReadSyncListGameObject_Faction(reader);
			SyncListNeutral = GeneratedNetworkCode._ReadSyncListGameObject_Faction(reader);
			SyncListEnemies = GeneratedNetworkCode._ReadSyncListGameObject_Faction(reader);
			ID = (int)reader.ReadPackedUInt32();
			MaximumPopulation = (int)reader.ReadPackedUInt32();
			name = reader.ReadString();
			UseCreepShader = reader.ReadBoolean();
			isPlayable = reader.ReadBoolean();
			HasGlobalFogVision = reader.ReadBoolean();
			isAI = reader.ReadBoolean();
			IsSpectator = reader.ReadBoolean();
			FactionColor = reader.ReadColor();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			OwnerSyncList = GeneratedNetworkCode._ReadSyncListGameObject_Faction(reader);
		}
		if ((num & 2) != 0)
		{
			SyncListAllies = GeneratedNetworkCode._ReadSyncListGameObject_Faction(reader);
		}
		if ((num & 4) != 0)
		{
			SyncListNeutral = GeneratedNetworkCode._ReadSyncListGameObject_Faction(reader);
		}
		if ((num & 8) != 0)
		{
			SyncListEnemies = GeneratedNetworkCode._ReadSyncListGameObject_Faction(reader);
		}
		if ((num & 0x10) != 0)
		{
			ID = (int)reader.ReadPackedUInt32();
		}
		if ((num & 0x20) != 0)
		{
			maximumPopulationChanged((int)reader.ReadPackedUInt32());
		}
		if ((num & 0x40) != 0)
		{
			OnNameChanged(reader.ReadString());
		}
		if ((num & 0x80) != 0)
		{
			UseCreepShader = reader.ReadBoolean();
		}
		if ((num & 0x100) != 0)
		{
			isPlayable = reader.ReadBoolean();
		}
		if ((num & 0x200) != 0)
		{
			HasGlobalFogVision = reader.ReadBoolean();
		}
		if ((num & 0x400) != 0)
		{
			isAI = reader.ReadBoolean();
		}
		if ((num & 0x800) != 0)
		{
			IsSpectator = reader.ReadBoolean();
		}
		if ((num & 0x1000) != 0)
		{
			OnFactionColorChanged(reader.ReadColor());
		}
	}
}
