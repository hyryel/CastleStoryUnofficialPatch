using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.AI;
using Brix.Game.Network;
using Brix.Game.Rules;
using Brix.Game.Semantique;
using Brix.Lifecycle.Pooling;
using Brix.Lua;
using Brix.Transactions;
using Brix.Utils;
using Brix.Utils.Signals;
using MoonSharp.Interpreter;
using Motus.Behavior;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Components;

public class FireflyNest : NetworkBehavior
{
	private class Brewery
	{
		public FireflyNest Nest;

		private float _lastBrewedTime;

		private Recipe _currentPassiveRecipe;

		private bool isHome => Nest.isHome;

		private Recepteur mainRecepteur => Nest.mainRecepteur;

		private Faction faction => Nest.faction;

		private bool doBrew => Nest.doBrew;

		private float brewDuration => Nest.brewDuration;

		private int fireflyFilling => Nest.fireflyFilling;

		private Recipe _fireflyRecipe
		{
			get
			{
				Recipe recipe = CookBook.FireFly.GetRecipe();
				recipe.SetIngredient(Adjectif.blueCrystal, fireflyFilling);
				return recipe;
			}
		}

		public StaticNode CheckRecipeNode()
		{
			return Node.Sequence.Do((Brewery aFireflyNest) => Node.WaitForTicks((!aFireflyNest.isHome) ? 50 : 10), this).Do(delegate(Brewery aFireflyNest)
			{
				aFireflyNest.CheckRecipe();
			}, this).Loop();
		}

		public StaticNode CheckBrewNode()
		{
			return Node.Sequence.Do(Node.WaitForTicks(5)).Do(delegate(Brewery aFireflyNest)
			{
				if (aFireflyNest.CanBrew())
				{
					aFireflyNest.ExecuteBrew();
				}
			}, this).Loop();
		}

		private void CheckRecipe()
		{
			if (_currentPassiveRecipe == null || _currentPassiveRecipe.Finished)
			{
				_currentPassiveRecipe = _fireflyRecipe;
			}
			else if (_currentPassiveRecipe.IsReadyToFinish(mainRecepteur))
			{
				_currentPassiveRecipe.Finish(mainRecepteur, ProcessFirefly);
			}
		}

		private bool CanBrew()
		{
			return doBrew && faction.usesFireflies && Nest.GetActiveOrSpawnableCharacterCount() < GeneralRules.Main.bricktronCap;
		}

		private void ExecuteBrew()
		{
			if (brewDuration > 0f && GameClock.TimeSinceGameStart >= _lastBrewedTime + brewDuration)
			{
				if (mainRecepteur.HasRoomFor(RawResources.RawBlueCrystal))
				{
					GameObject gameObject = Transactor.SpawnUNet(RawResources.RawBlueCrystal, null, Vector3.zero, Quaternion.identity);
					mainRecepteur.Add(gameObject.GetComponent<GameComponent>());
				}
				_lastBrewedTime = GameClock.TimeSinceGameStart;
			}
		}

		private void ProcessFirefly(GameObject go)
		{
			Firefly component = go.GetComponent<Firefly>();
			component.SetFirefly(mainRecepteur.faction, string.Empty, Face.Expression.Blank, Occupation.Type.Unassigned, isOld: false);
			component.pureEnergy = fireflyFilling;
		}
	}

	[JsonProperty]
	[SyncVar]
	public bool isHome;

	[SyncVar]
	[JsonProperty]
	public bool doBrew;

	[SyncVar]
	[JsonProperty]
	public float brewDuration = 1f;

	[Range(1f, 100f)]
	[SyncVar]
	[JsonProperty]
	public int fireflyFilling = 100;

	public Recepteur mainRecepteur;

	private Signal onDestroyed = new Signal();

	private Brewery brewery;

	private static readonly List<FireflyNest> FireflyNests = new List<FireflyNest>();

	private int exAvailablePureEnergy;

	private int exNewSpawnableFireflyCount;

	private int exAutoRespawnFireflyCount;

	private int exNewFireflyRequiredEnergy;

	private float exCrystalConversionProgress;

	[LuaDrivable("hk_OnSetIsFireflyHome")]
	private LuaHook lua_OnSetIsFireflyHome = new LuaHook();

	[LuaDrivable("hk_OnSetFireflyCount")]
	private LuaHook lua_OnSetFireflyCount = new LuaHook();

	[LuaDrivable("hk_OnSetNewFireflyRequiredEnergy")]
	private LuaHook lua_OnSetNewFireflyRequiredEnergy = new LuaHook();

	[LuaDrivable("hk_OnSetCrystalConversionProgress")]
	private LuaHook lua_OnSetCrystalConversionProgress = new LuaHook();

	[LuaDrivable("hk_OnSetAvailablePureEnergy")]
	private LuaHook lua_OnSetAvailablePureEnergy = new LuaHook();

	private static readonly LuaHook lua_OnSetCurrentXp = new LuaHook();

	private static readonly LuaHook lua_OnSetTotalXp = new LuaHook();

	private static readonly LuaHook lua_OnSetSpawnableCount = new LuaHook();

	public static LuaHookCollapsed lua_OnSetInfo;

	private static bool lua_hasInit = false;

	[LuaDrivable("availableBlueCrystal")]
	private int availableBlueCrystal => mainRecepteur.ContentDescription.Get<BlueCrystal>()?.quantifiable.valeur ?? 0;

	[LuaDrivable("crystalConversionProgress")]
	public float crystalConversionProgress => (float)availableBlueCrystal / (float)fireflyFilling;

	[LuaDrivable("availablePureEnergy")]
	private int lua_availableEnergy => GetAvailablePurifiedEnergy();

	[LuaDrivable("autoRespawnFireflyCount")]
	private int autoRespawnFireflyCount => Firefly.RespawningFirefliesForFaction(faction);

	[LuaDrivable("newSpawnableFireflyCount")]
	private int newSpawnableFireflyCount => GetNewSpawnableCharacterCount();

	[LuaDrivable("newFireflyRequiredEnergy")]
	public int NewFireflyRequiredEnergy => GeneralRules.Main.GetHomeCrystalSpawnRequiredEnergy(GetActiveOrSpawnableCharacterCount());

	[LuaDrivable("pointCount")]
	private int pointCount
	{
		get
		{
			int num = 0;
			num += mainRecepteur.ContentDescription.Get<Pumpkin>()?.quantifiable.valeur ?? 0;
			return num + (mainRecepteur.ContentDescription.Get<CrystalObjective>()?.quantifiable.valeur ?? 0);
		}
	}

	[LuaDrivable("isFireflyHome")]
	private bool lua_isHome
	{
		get
		{
			return isHome;
		}
		set
		{
			if (isHome != value)
			{
				NetworkisHome = value;
				LuaHookBase.InvokeFromCLR(lua_OnSetIsFireflyHome);
			}
		}
	}

	[LuaDrivable("doCrystalBrew")]
	private bool lua_doBrew
	{
		get
		{
			return doBrew;
		}
		set
		{
			NetworkdoBrew = value;
		}
	}

	[LuaDrivable("crystalBrewInterval")]
	private float lua_brewDuration
	{
		get
		{
			return brewDuration;
		}
		set
		{
			NetworkbrewDuration = value;
		}
	}

	[LuaDrivable("crystalFillingPerFirefly")]
	private int lua_fireflyFillings
	{
		get
		{
			return fireflyFilling;
		}
		set
		{
			NetworkfireflyFilling = value;
		}
	}

	public bool NetworkisHome
	{
		get
		{
			return isHome;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref isHome, 1u);
		}
	}

	public bool NetworkdoBrew
	{
		get
		{
			return doBrew;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref doBrew, 2u);
		}
	}

	public float NetworkbrewDuration
	{
		get
		{
			return brewDuration;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref brewDuration, 4u);
		}
	}

	public int NetworkfireflyFilling
	{
		get
		{
			return fireflyFilling;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref fireflyFilling, 8u);
		}
	}

	private int CountFireflyOfCondition(Func<Firefly, bool> condition)
	{
		int num = 0;
		foreach (IDescriptor storedItem in mainRecepteur.StoredItems)
		{
			GameObject gameObject = storedItem.GameObject;
			if (gameObject.CompareTag("Firefly"))
			{
				Firefly component = gameObject.GetComponent<Firefly>();
				if (condition(component))
				{
					num++;
				}
			}
		}
		return num;
	}

	private Firefly GetFireflyOfCondition(Func<Firefly, bool> condition)
	{
		foreach (IDescriptor storedItem in mainRecepteur.StoredItems)
		{
			GameObject gameObject = storedItem.GameObject;
			if (gameObject.CompareTag("Firefly"))
			{
				Firefly component = gameObject.GetComponent<Firefly>();
				if (condition(component))
				{
					return component;
				}
			}
		}
		return null;
	}

	protected override void Awake()
	{
		base.Awake();
		mainRecepteur.signal.SpawnUnit.Connect(delegate(Occupation.Type aOccupation, Factory.AssetKey aAssetKey, FireflyNest aFireflyNest)
		{
			aFireflyNest.AssignJobRequest(aOccupation, aAssetKey);
		}, this).DisconnectWhen(onDestroyed);
		brewery = new Brewery
		{
			Nest = this
		};
		if (!FireflyNests.Contains(this))
		{
			FireflyNests.Add(this);
		}
	}

	public override void OnRequested()
	{
		base.OnRequested();
		if (!FireflyNests.Contains(this))
		{
			FireflyNests.Add(this);
		}
	}

	protected override void OnDestroy()
	{
		if (FireflyNests.Contains(this))
		{
			FireflyNests.Remove(this);
		}
		base.OnDestroy();
		onDestroyed.Invoke();
	}

	public override void ResetComponent()
	{
		if (FireflyNests.Contains(this))
		{
			FireflyNests.Remove(this);
		}
		base.ResetComponent();
	}

	public static void ReleaseAllFireflies(FireflyNest nest)
	{
		nest.mainRecepteur.DropAll();
	}

	public void Hack_AssignJobToNamedFirefly(Occupation.Type occupation)
	{
		Firefly namedButJoblessFirefly = GetNamedButJoblessFirefly();
		if (namedButJoblessFirefly != null)
		{
			namedButJoblessFirefly.NetworklaborOccupation = occupation;
			SpawnBricktronFromFirefly(namedButJoblessFirefly);
		}
	}

	public void AssignJobRequest(Occupation.Type occupation, Factory.AssetKey key)
	{
		User.LocalUser.GetComponent<UNetProjectCmd>().CallCmdSpawnFireflyNest(mainRecepteur.gameObject, occupation);
	}

	public int GetAvailablePurifiedEnergy()
	{
		int num = 0;
		foreach (IDescriptor storedItem in mainRecepteur.StoredItems)
		{
			GameObject gameObject = storedItem.GameObject;
			if (gameObject.CompareTag("Firefly"))
			{
				Firefly component = gameObject.GetComponent<Firefly>();
				num += component.pureEnergy;
			}
		}
		return num;
	}

	public int GetNewSpawnableCharacterCount()
	{
		return Firefly.SpawnableFirefliesForFaction(faction);
	}

	public int GetActiveOrSpawnableCharacterCount()
	{
		return faction.UnitCount + GetNewSpawnableCharacterCount();
	}

	public int GetExistingCharacterCount()
	{
		return GetActiveOrSpawnableCharacterCount() + Firefly.RespawningFirefliesForFaction(faction);
	}

	private int ConsumeAllUnnamedFireflies()
	{
		return ConsumeUnnamedFirefliesUpTo(int.MaxValue);
	}

	private int ConsumeUnnamedFirefliesUpTo(int requiredEnergy)
	{
		List<Firefly> list = null;
		int num = 0;
		foreach (IDescriptor storedItem in mainRecepteur.StoredItems)
		{
			GameObject gameObject = storedItem.GameObject;
			if (!gameObject.CompareTag("Firefly"))
			{
				continue;
			}
			Firefly component = gameObject.GetComponent<Firefly>();
			if (component.IsNamed)
			{
				continue;
			}
			int pureEnergy = component.pureEnergy;
			num += pureEnergy;
			if (list == null)
			{
				list = CollectionPool.RequestList<Firefly>();
			}
			list.Add(component);
			if (num >= requiredEnergy)
			{
				if (num > requiredEnergy)
				{
					CreateUnnamedFirefly(num - requiredEnergy);
				}
				break;
			}
		}
		if (list != null)
		{
			for (int num2 = list.Count - 1; num2 != -1; num2--)
			{
				ObjectPoolSingleton.Release(list[num2].gameObject);
			}
			list.Dispose();
		}
		return num;
	}

	public Firefly GetNamedButJoblessFirefly()
	{
		return GetFireflyOfCondition((Firefly ff) => ff.IsNamed && !ff.LaborHasOccupation);
	}

	protected Node PassiveNode()
	{
		if (mainRecepteur == null)
		{
			return Node.Empty;
		}
		if (!NetworkServer.active)
		{
			return ClientNode();
		}
		return (StaticNode)Node.Parallel.Do(brewery.CheckRecipeNode()).Do(brewery.CheckBrewNode()).Do(CheckHomeSpawnsNode())
			.Do(CheckHomeEnergyCombineNode())
			.Do(ClientNode());
	}

	private StaticNode ClientNode()
	{
		return Node.Sequence.Do(() => Node.WaitForTicks(50)).Do(delegate(FireflyNest aFireflyNest)
		{
			aFireflyNest.ProcessClientSignals();
		}, this).Loop();
	}

	private void ProcessClientSignals()
	{
		if (exAvailablePureEnergy != GetAvailablePurifiedEnergy())
		{
			exAvailablePureEnergy = GetAvailablePurifiedEnergy();
			LuaHookBase.InvokeFromCLR(lua_OnSetAvailablePureEnergy);
			LuaHookBase.InvokeFromCLR(lua_OnSetCurrentXp);
		}
		if (exNewSpawnableFireflyCount != newSpawnableFireflyCount)
		{
			exNewSpawnableFireflyCount = newSpawnableFireflyCount;
			LuaHookBase.InvokeFromCLR(lua_OnSetFireflyCount);
			LuaHookBase.InvokeFromCLR(lua_OnSetSpawnableCount);
		}
		if (exAutoRespawnFireflyCount != autoRespawnFireflyCount)
		{
			exAutoRespawnFireflyCount = autoRespawnFireflyCount;
			LuaHookBase.InvokeFromCLR(lua_OnSetFireflyCount);
			LuaHookBase.InvokeFromCLR(lua_OnSetSpawnableCount);
		}
		if (exNewFireflyRequiredEnergy != NewFireflyRequiredEnergy)
		{
			exNewFireflyRequiredEnergy = NewFireflyRequiredEnergy;
			LuaHookBase.InvokeFromCLR(lua_OnSetNewFireflyRequiredEnergy);
			LuaHookBase.InvokeFromCLR(lua_OnSetTotalXp);
		}
		if (exCrystalConversionProgress != crystalConversionProgress)
		{
			exCrystalConversionProgress = crystalConversionProgress;
			LuaHookBase.InvokeFromCLR(lua_OnSetCrystalConversionProgress);
		}
	}

	protected override Node CreateTree()
	{
		return (StaticNode)Node.Sequence.Do((FireflyNest aFireflyNest) => aFireflyNest.PassiveNode(), this).Loop();
	}

	private StaticNode CheckHomeSpawnsNode()
	{
		return Node.Sequence.Do(() => Node.WaitForTicks(10)).Do(delegate(FireflyNest aFireflyNest)
		{
			aFireflyNest.ProcessHomeSpawns();
		}, this).Loop();
	}

	private StaticNode CheckHomeEnergyCombineNode()
	{
		return Node.Sequence.Do(() => Node.WaitForTicks(100)).Do(delegate(FireflyNest aFireflyNest)
		{
			aFireflyNest.CheckEnergyCombine();
		}, this).Loop();
	}

	private void ProcessHomeSpawns()
	{
		if (isHome)
		{
			QueueReadyFireflies();
			FeedQueuedFireflies();
			SpawnNamedFireflies();
		}
	}

	private void CheckEnergyCombine()
	{
		if (!isHome)
		{
			return;
		}
		int num = CountFireflyOfCondition((Firefly ff) => !ff.IsNamed);
		int num2 = GetAvailablePurifiedEnergy();
		int newFireflyRequiredEnergy = NewFireflyRequiredEnergy;
		int num3 = 10 * num2 / newFireflyRequiredEnergy;
		if (num3 > 0 && num > num3 * 2 + 1)
		{
			ConsumeAllUnnamedFireflies();
			for (int num4 = 0; num4 < num3; num4++)
			{
				int num5 = num2 / (num3 - num4);
				CreateUnnamedFirefly(num5);
				num2 -= num5;
			}
		}
	}

	private void QueueReadyFireflies()
	{
		while (true)
		{
			Firefly fireflyOfCondition = GetFireflyOfCondition((Firefly ff) => ff.LaborHasOccupation && !ff.queued);
			if (fireflyOfCondition == null)
			{
				break;
			}
			int storedEnergy = fireflyOfCondition.storedEnergy;
			fireflyOfCondition.NetworkstoredEnergy = 0;
			CreateUnnamedFirefly(storedEnergy);
			fireflyOfCondition.queued = true;
		}
	}

	private void FeedQueuedFireflies()
	{
		int num = GetActiveOrSpawnableCharacterCount();
		int homeCrystalSpawnRequiredEnergy = GeneralRules.Main.GetHomeCrystalSpawnRequiredEnergy(num);
		int num2 = GetAvailablePurifiedEnergy();
		while (num2 >= homeCrystalSpawnRequiredEnergy)
		{
			Firefly fireflyOfCondition = GetFireflyOfCondition((Firefly ff) => ff.queued && !ff.IsSpawning && ff.captureTimer.Check());
			if (fireflyOfCondition == null)
			{
				break;
			}
			ConsumeUnnamedFirefliesUpTo(homeCrystalSpawnRequiredEnergy);
			SpawnBricktronFromFirefly(fireflyOfCondition);
			num2 -= homeCrystalSpawnRequiredEnergy;
			num++;
			homeCrystalSpawnRequiredEnergy = GeneralRules.Main.GetHomeCrystalSpawnRequiredEnergy(num);
		}
	}

	private void SpawnNamedFireflies()
	{
		int num = GetExistingCharacterCount();
		if (num < GeneralRules.Main.bricktronCap)
		{
			int homeCrystalSpawnRequiredEnergy = GeneralRules.Main.GetHomeCrystalSpawnRequiredEnergy(num);
			int num2 = GetAvailablePurifiedEnergy();
			while (num2 >= homeCrystalSpawnRequiredEnergy)
			{
				ConsumeUnnamedFirefliesUpTo(homeCrystalSpawnRequiredEnergy);
				CreateNamedFirefly(homeCrystalSpawnRequiredEnergy);
				num2 -= homeCrystalSpawnRequiredEnergy;
				num++;
				homeCrystalSpawnRequiredEnergy = GeneralRules.Main.GetHomeCrystalSpawnRequiredEnergy(num);
			}
		}
	}

	public bool CaptureFirefly(Firefly ff)
	{
		Faction faction = ff.GetComponent<AffiliationComponent>().faction;
		bool flag = this.faction == faction;
		if (!((!faction.isAI) ? flag : (!this.faction.isAI)))
		{
			return false;
		}
		if (flag && ff.IsNamed)
		{
			int networkstoredEnergy = ff.storedEnergy + ff.pureEnergy;
			ff.pureEnergy = 0;
			ff.NetworkstoredEnergy = networkstoredEnergy;
		}
		mainRecepteur.AddObject(ff.gameObject, keepWorldPosition: true);
		return true;
	}

	private void CreateUnnamedFirefly(int storedEnergy)
	{
		GameObject gameObject = Transactor.SpawnUNet(Brix.External.Factories.Environment.Firefly, faction, base.transform.position + Vector3.up, Quaternion.identity);
		Firefly component = gameObject.GetComponent<Firefly>();
		component.pureEnergy = storedEnergy;
	}

	private void CreateNamedFirefly(int storedEnergy)
	{
		ObjectPoolSingleton.RequestComponent<Firefly>(out var t, Brix.External.Factories.Environment.Firefly, base.transform.position, Quaternion.identity);
		AffiliationComponent component = t.GetComponent<AffiliationComponent>();
		component.faction = faction;
		component.NetworkUserCreator = null;
		if (NetworkServer.active)
		{
			NetworkServer.Spawn(t.gameObject, Brix.External.Factories.Environment.Firefly.AssetId);
		}
		t.pureEnergy = 0;
		t.captureTimer.Set(0f);
		t.AssignName();
		t.NetworklaborFace = (Face.Expression)UnityEngine.Random.Range(0, 14);
		t.NetworkstoredEnergy = storedEnergy;
	}

	private void SpawnBricktronFromFirefly(Firefly ff)
	{
		ff.transform.SetParent(null, worldPositionStays: true);
		ff.StartSpawning(base.transform.position + Vector3.up * 4f);
	}

	public bool CanSpawnFor(Faction f)
	{
		return faction == f && GetNamedButJoblessFirefly() != null;
	}

	public bool CanSpawnMilitaryFor(Faction f)
	{
		return GeneralRules.Main.CanSpawnMilitaryUnits && CanSpawnFor(f);
	}

	[OnDeserialized]
	private void OnDeserialized(StreamingContext sc)
	{
		SetDirtyBit(uint.MaxValue);
	}

	[LuaDrivable("ReleaseAllFireflies")]
	private static DynValue lua_ReleaseAllFireflies(ScriptExecutionContext context, CallbackArguments args)
	{
		FireflyNest fireflyNest = args.AsComponent<FireflyNest>(0, "ReleaseAllFireflies");
		fireflyNest.mainRecepteur.DropAll();
		return DynValue.Void;
	}

	private static FireflyNest GetAlliedFireflyNest()
	{
		Faction faction = User.LocalUser.faction;
		foreach (FireflyNest fireflyNest in FireflyNests)
		{
			if (!fireflyNest.isHome || fireflyNest.faction != faction)
			{
				continue;
			}
			return fireflyNest;
		}
		throw new Exception("no firefly nest found");
	}

	private static DynValue lua_GetInfo(ScriptExecutionContext context, CallbackArguments args)
	{
		FireflyNest alliedFireflyNest = GetAlliedFireflyNest();
		DynValue result = DynValue.NewTable(context.OwnerScript);
		float num = alliedFireflyNest.GetAvailablePurifiedEnergy();
		float num2 = GeneralRules.Main.GetHomeCrystalSpawnRequiredEnergy(alliedFireflyNest.GetActiveOrSpawnableCharacterCount());
		float num3 = alliedFireflyNest.newSpawnableFireflyCount;
		float num4 = alliedFireflyNest.autoRespawnFireflyCount;
		result.Table.Set("currentXp", DynValue.NewNumber(num));
		result.Table.Set("totalXp", DynValue.NewNumber(num2));
		result.Table.Set("spawnableCount", DynValue.NewNumber(num3));
		result.Table.Set("autoRespawnFireflyCount", DynValue.NewNumber(num4));
		return result;
	}

	private static DynValue lua_SpawnUnit(ScriptExecutionContext context, CallbackArguments args)
	{
		FireflyNest alliedFireflyNest = GetAlliedFireflyNest();
		alliedFireflyNest.AssignJobRequest(Occupation.Type.Builder, null);
		return DynValue.Void;
	}

	public static void RegisterClass(Script script)
	{
		if (!lua_hasInit)
		{
			lua_hasInit = true;
			lua_OnSetInfo = new LuaHookCollapsed(PersistentCoroutineRunner.UpdateSignal, new LuaHook[3] { lua_OnSetCurrentXp, lua_OnSetTotalXp, lua_OnSetSpawnableCount });
		}
		DynValue value = DynValue.NewTable(script);
		value.Table.Set("hk_OnSetInfo", UserData.CreateWithDescriptor(lua_OnSetInfo, LuaHookBase.desc));
		value.Table.Set("GetInfo", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_GetInfo(exec, args)));
		value.Table.Set("SpawnUnit", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_SpawnUnit(exec, args)));
		script.Globals.Set("FireflyNest", value);
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.Write(isHome);
			writer.Write(doBrew);
			writer.Write(brewDuration);
			writer.WritePackedUInt32((uint)fireflyFilling);
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
			writer.Write(isHome);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(doBrew);
		}
		if ((base.syncVarDirtyBits & 4) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(brewDuration);
		}
		if ((base.syncVarDirtyBits & 8) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.WritePackedUInt32((uint)fireflyFilling);
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
			isHome = reader.ReadBoolean();
			doBrew = reader.ReadBoolean();
			brewDuration = reader.ReadSingle();
			fireflyFilling = (int)reader.ReadPackedUInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			isHome = reader.ReadBoolean();
		}
		if ((num & 2) != 0)
		{
			doBrew = reader.ReadBoolean();
		}
		if ((num & 4) != 0)
		{
			brewDuration = reader.ReadSingle();
		}
		if ((num & 8) != 0)
		{
			fireflyFilling = (int)reader.ReadPackedUInt32();
		}
	}
}
