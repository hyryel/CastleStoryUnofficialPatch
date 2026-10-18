using System;
using System.Collections.Generic;
using Brix.Audio;
using Brix.Common.Collections;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.AI;
using Brix.Game.AI.KnowledgeSpace;
using Brix.Game.Locomotion;
using Brix.Game.Network;
using Brix.Game.Semantique;
using Brix.Lua;
using Brix.UI.GameSelector;
using Brix.UI.Icons;
using Brix.Utils;
using Brix.Utils.Signals;
using MoonSharp.Interpreter;
using Motus.Behavior;
using Smooth.Algebraics;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Components;

public abstract class CraftingStation : GameComponent, IOperable<Labor>
{
	public enum Mode
	{
		idle,
		queue
	}

	public enum Action
	{
		none,
		process,
		startWork,
		work,
		endWork
	}

	public enum Actions
	{
		Nothing,
		GoFetch,
		Pickup,
		Work
	}

	[SerializeField]
	protected CraftingState state;

	[SerializeField]
	protected CraftingLabor craftingLabor;

	public Signal changed = new Signal();

	[SerializeField]
	private CraftingInventory inventory;

	private static readonly LuaHook lua_OnSetSelected = new LuaHook();

	private static SignalConnection recepteurConnection;

	private static readonly LuaHook lua_OnSetContent = new LuaHook();

	private static SignalConnection queueConnection;

	private static readonly LuaHook lua_OnSetQueue = new LuaHook();

	private static SignalConnection loopConnection;

	private static readonly LuaHook lua_OnSetLoop = new LuaHook();

	private static readonly Func<ScriptExecutionContext, CallbackArguments, DynValue> lua_GetSelected = delegate
	{
		CraftingStation craftingStation = SelectedStation();
		return craftingStation.IsNullOrReleased() ? DynValue.Nil : UserData.CreateWithDescriptor(craftingStation.GetComponent<LuaDriver>(), LuaDriver.desc);
	};

	private static readonly Func<ScriptExecutionContext, CallbackArguments, DynValue> lua_DeselectAll = delegate
	{
		UIGameSelector.UnselectAll(SelectionType.Crafting);
		return DynValue.Void;
	};

	private static readonly Func<ScriptExecutionContext, CallbackArguments, DynValue> lua_GetQueue = delegate(ScriptExecutionContext sec, CallbackArguments arg)
	{
		CraftingStation craftingStation = SelectedStation();
		DynValue result = DynValue.NewTable(sec.OwnerScript);
		if (craftingStation.IsNullOrReleased())
		{
			return result;
		}
		foreach (RecipeInfo queuedRecipe in craftingStation.craftingLabor.QueuedRecipes)
		{
			result.Table.Append(DynValue.FromObject(sec.OwnerScript, LuaCrafting.ToLuaRecipe(queuedRecipe)));
		}
		return result;
	};

	private static readonly Func<ScriptExecutionContext, CallbackArguments, DynValue> lua_AddToQueue = delegate(ScriptExecutionContext sec, CallbackArguments arg)
	{
		CraftingStation craftingStation = SelectedStation();
		if (craftingStation.IsNullOrReleased())
		{
			return DynValue.Void;
		}
		LuaCrafting.LuaRecipe value = arg.AsEnum<LuaCrafting.LuaRecipe>(0, "AddToQueue");
		List<RecipeInfo> list = LuaCrafting.ToRecipeInfo(value);
		foreach (RecipeInfo item in list)
		{
			localUserProject.CallCmdcraftingQueueRecipe(craftingStation.gameObject, item.id, 1);
		}
		return DynValue.Nil;
	};

	private static readonly Func<ScriptExecutionContext, CallbackArguments, DynValue> lua_ClearQueue = delegate
	{
		CraftingStation craftingStation = SelectedStation();
		if (craftingStation.IsNullOrReleased())
		{
			return DynValue.Void;
		}
		localUserProject.CallCmdcraftingDoNothing(craftingStation.gameObject);
		return DynValue.Void;
	};

	private static readonly Func<ScriptExecutionContext, CallbackArguments, DynValue> lua_GetIsQueueLooping = delegate
	{
		CraftingStation craftingStation = SelectedStation();
		return craftingStation.IsNullOrReleased() ? DynValue.False : ((!craftingStation.craftingLabor.LoopRecipe) ? DynValue.False : DynValue.True);
	};

	private static readonly Func<ScriptExecutionContext, CallbackArguments, DynValue> lua_SetIsQueueLooping = delegate(ScriptExecutionContext sec, CallbackArguments arg)
	{
		CraftingStation craftingStation = SelectedStation();
		if (craftingStation.IsNullOrReleased())
		{
			return DynValue.Void;
		}
		bool value = arg.AsBool(0, "SetIsQueueLooping");
		localUserProject.CallCmdcraftingLoop(craftingStation.gameObject, value);
		return DynValue.Void;
	};

	private static readonly Func<ScriptExecutionContext, CallbackArguments, DynValue> lua_RemoveFromQueue = delegate(ScriptExecutionContext sec, CallbackArguments arg)
	{
		CraftingStation craftingStation = SelectedStation();
		if (craftingStation.IsNullOrReleased())
		{
			return DynValue.Void;
		}
		int index = arg.AsInt(0, "RemoveFromQueue");
		localUserProject.CallCmdcraftingRemoveAt(craftingStation.gameObject, index);
		return DynValue.Void;
	};

	private static readonly Func<ScriptExecutionContext, CallbackArguments, DynValue> lua_GetContent = delegate(ScriptExecutionContext sec, CallbackArguments arg)
	{
		CraftingStation craftingStation = SelectedStation();
		DynValue result = DynValue.NewTable(sec.OwnerScript);
		if (craftingStation.IsNullOrReleased())
		{
			return result;
		}
		HashSet<Factory.AssetKey> hashset = CollectionPool.RequestHashSet<Factory.AssetKey>();
		foreach (Ressource item2 in craftingStation.recepteur.ContentDescription.GetRessources().NoLeak())
		{
			if (item2.valeur != 0)
			{
				DynValue key = DynValue.FromObject(sec.OwnerScript, LuaCrafting.ToLuaResource(item2));
				result.Table.Set(key, DynValue.NewNumber(item2.valeur));
			}
		}
		hashset.Dispose();
		return result;
	};

	public abstract IconKey Icon { get; }

	public abstract string PieMenuTag { get; }

	public IReadOnlySignal Changed => changed;

	public List<RecipeInfo> QueuedRecipes => craftingLabor.QueuedRecipes;

	public RecipeInfo FirstRecipe => craftingLabor.FirstRecipe;

	public Recipe CurrentRecipe => craftingLabor.CurrentRecipe;

	public int QueueCount => craftingLabor.QueueCount();

	private CraftingInstructions instructions => craftingLabor.instructions;

	protected abstract bool CanStackOrders { get; }

	public Mode CurrentMode => state.HasRecipe() ? Mode.queue : Mode.idle;

	public bool AutoStore
	{
		get
		{
			return state.AutoStore;
		}
		set
		{
			state.AutoStore = value;
		}
	}

	public bool Looping
	{
		get
		{
			return craftingLabor.LoopRecipe;
		}
		set
		{
			craftingLabor.LoopRecipe = value;
		}
	}

	public Labor Operator
	{
		get
		{
			return state.Operator;
		}
		set
		{
			state.Operator = value;
		}
	}

	public bool IsStillAlive => !this.IsNullOrReleased();

	public abstract ProjectPriority PriorityWeights { get; }

	public ProjectPriority.Bias PriorityBias
	{
		get
		{
			return ProjectPriority.Bias.High;
		}
		set
		{
		}
	}

	public int MaximumCrewSize => 1;

	public ConflictResolver ConflictResolver { get; private set; }

	private static UNetProjectCmd localUserProject => User.LocalUser.GetComponent<UNetProjectCmd>();

	protected abstract void SetDefault();

	public override void Initialize()
	{
		base.Initialize();
		craftingLabor.Init(new CraftingInstructions(this));
		ConflictResolver = new ConflictResolver(() => ConflictResolver.ResolutionModel.Operation);
		inventory = new CraftingInventory(base.recepteur);
		base.recepteur.InventoryChanged.Connect(delegate(CraftingStation self)
		{
			self.OnInventoryChanged();
		}, this).DisconnectWhen(base.OnReleaseOrDestroySignal);
		state.Updated.Connect(delegate(CraftingStation self)
		{
			self.OnDataChanged();
		}, this).DisconnectWhen(base.OnReleaseOrDestroySignal);
		state.WorkStarted.Connect(delegate
		{
			OnWorkStarted();
		}, delegate
		{
			OnWork();
		}, delegate
		{
			OnWorkEnded();
		}, base.OnReleaseOrDestroySignal);
		state.Processing.Connect(OnProcess).DisconnectWhen(base.OnReleaseOrDestroySignal);
	}

	protected override void Start()
	{
		base.Start();
		if (NetworkServer.active)
		{
			if (!state.Initilized)
			{
				SetDefault();
			}
			ProjectDatabase.For(base.faction).Register(this);
		}
	}

	private void OnDataChanged()
	{
		RefeshInventory();
		OnQueueChanged();
		changed.Invoke();
	}

	private void RefeshInventory()
	{
		if (CanStackOrders)
		{
			inventory.SetRecipes(QueuedRecipes);
		}
		else if (FirstRecipe == null)
		{
			inventory.Clear();
		}
		else
		{
			inventory.SetRecipe(FirstRecipe);
		}
	}

	public virtual Working.WorkStates WorkAnimation()
	{
		return Working.WorkStates.Workbench;
	}

	protected virtual void OnQueueChanged()
	{
	}

	protected virtual void OnInventoryChanged()
	{
	}

	public void Process(int ms)
	{
		state.Process(ms);
	}

	protected virtual void OnProcess(int ms)
	{
	}

	public void WorkStarted()
	{
		state.Working = true;
	}

	protected virtual void OnWorkStarted()
	{
	}

	public void WorkContinued()
	{
		state.Working = true;
	}

	protected virtual void OnWork()
	{
	}

	public void WorkEnded()
	{
		state.Working = false;
	}

	protected virtual void OnWorkEnded()
	{
	}

	protected void PlaySound(string sound)
	{
		SoundEngine.Play(sound, base.gameObject);
	}

	private bool HasRoomFor(Labor labor)
	{
		return Operator == null || Operator == labor;
	}

	public void Finish(Recipe recipe)
	{
		craftingLabor.FinishRecipe(recipe);
	}

	public bool ResourceToPickup(out GameObject go)
	{
		return inventory.ResourceToRemove(out go);
	}

	public bool ProductionToPickup(out GameObject go)
	{
		return inventory.ItemToPickup(out go);
	}

	public bool ProductionIs(Ressource ressource)
	{
		Factory.AssetKey asset = Description.RepresentativeOf(ressource);
		return inventory.HasProductsOfType(asset);
	}

	public bool HasWorkFor(Labor labor)
	{
		return HasRoomFor(labor) && DecideWhatToDo(labor) != Actions.Nothing;
	}

	private Actions DecideWhatToDo(Labor labor)
	{
		if (!Knowledge.Instance.CanReach(labor, this))
		{
			return Actions.Nothing;
		}
		if (CanStore(labor))
		{
			return Actions.Pickup;
		}
		if (CanFetch(labor))
		{
			return Actions.GoFetch;
		}
		if (NeedsWork(CurrentRecipe) && CanWork(labor))
		{
			return Actions.Work;
		}
		return Actions.Nothing;
	}

	private bool CanStore(Labor labor)
	{
		return AutoStore && CanPickUpProduction(labor, andStore: true);
	}

	public bool CanPickUpRessource(Labor labor, bool andStore = false)
	{
		GameObject go;
		return inventory.ResourceToRemove(out go) && labor.recepteur.HasRoomFor(go) && (!andStore || labor.BestRecepteurToStore(go) != null);
	}

	public bool CanPickUpProduction(Labor labor, bool andStore = false)
	{
		GameObject go;
		return inventory.ItemToPickup(out go) && labor.recepteur.HasRoomFor(go) && (!andStore || labor.BestRecepteurToStore(go) != null);
	}

	public bool CanWork(Labor labor)
	{
		return NeedsWork(CurrentRecipe) && labor.Profession.OccupationType == Occupation.Type.Builder && !labor.recepteur.RequiresBothHands();
	}

	private bool NeedsWork(Recipe recipe)
	{
		return recipe != null && recipe.NeedsWork && recipe.HasIngredientsIn(base.recepteur) && (recipe.DropOnCompletion || base.recepteur.HasRoomFor(recipe.Creates));
	}

	private bool CanFetch(Labor labor)
	{
		Description description = StuffToFetch();
		GameObject o = ((description == null) ? null : FindResourceFunc(labor)(description));
		return labor.recepteur.HasRoomFor(o);
	}

	public Description StuffToFetch()
	{
		if (CurrentRecipe != null && CurrentRecipe.NeedsHeat && !base.recepteur.Carries(Adjectif.orangeCrystal))
		{
			return Adjectif.orangeCrystal.Profile.description;
		}
		if (inventory.RequiresMoreRessources())
		{
			return inventory.StuffToFecth();
		}
		return null;
	}

	protected virtual Func<Description, GameObject> FindResourceFunc(Labor labor)
	{
		return labor.Find;
	}

	private bool IsSelf(IDescriptor d)
	{
		return d.GameObject.transform.parent == base.gameObject;
	}

	public OrderPackage<Labor> GetPackage(Actions action)
	{
		OrderPackage<Labor> orderPackage = OrderPackage<Labor>.Empty;
		switch (action)
		{
		case Actions.Nothing:
			return orderPackage;
		case Actions.Pickup:
		{
			if (inventory.ItemToPickup(out var go))
			{
				orderPackage = Order.Package.OfGoIntTuple(Order.Transfer.GoPickUp, Tuple.Create(go, -1), Labor.DecisionSource.Project);
			}
			break;
		}
		case Actions.GoFetch:
			orderPackage = instructions.FindPackage();
			break;
		case Actions.Work:
			orderPackage = instructions.WorkPackage(CurrentRecipe);
			break;
		}
		orderPackage.SetPrepare((BaseLabor aLabor) => Hire((Labor)aLabor));
		return orderPackage;
	}

	public Node GetInstruction(Labor labor, Var<OrderPackage<Labor>> result)
	{
		return (StaticNode)Node.Sequence.Do(delegate(Labor aLabor, Var<OrderPackage<Labor>> aResult, CraftingStation aCraftingStation)
		{
			Actions action = aCraftingStation.DecideWhatToDo(aLabor);
			OrderPackage<Labor> package = aCraftingStation.GetPackage(action);
			aResult.Set(package);
		}, labor, result, this);
	}

	public OrderPackage<Labor> GetContextualAction(Labor labor)
	{
		if (inventory.ItemToPickup(out var go) && labor.recepteur.HasRoomFor(go))
		{
			return Order.Package.OfGoIntTuple(Order.Transfer.GoPickUp, Tuple.Create(go, -1), Labor.DecisionSource.User);
		}
		if (base.recepteur.HasRoomForSomeOf(labor.recepteur))
		{
			return Order.Package.OfGameComponent(Order.Transfer.GoStoreInto, this, Labor.DecisionSource.User);
		}
		if (NeedsWork(CurrentRecipe))
		{
			return GetPackage(Actions.Work);
		}
		return OrderPackage<Labor>.Empty;
	}

	public Node VerifyWorkFor(Labor labor)
	{
		return Node.Empty;
	}

	public ProjectPriority PriorityInput(Labor labor)
	{
		GameObject go;
		return new ProjectPriority
		{
			bias = 1f,
			crewSize = CrewCount(),
			globalCrewSize = GlobalCrewCount(),
			remainingCrewSpace = MaximumCrewSize - CrewCount(),
			goalCount = QueueCount,
			deathCount = 0f,
			distance = Project.DistanceIncrement(this, labor),
			isCurrent = ((!(Operator == labor)) ? 0f : 1f),
			hasCleanup = ((!inventory.ItemToPickup(out go)) ? 0f : 1f),
			manualBias = (float)PriorityBias
		};
	}

	public int CrewCount()
	{
		return (Operator != null) ? 1 : 0;
	}

	public int GlobalCrewCount()
	{
		return ProjectDatabase.For(base.faction).WorkerInTaskSimilarTasksTo(this);
	}

	public bool Hire(Labor labor)
	{
		return IOperableExtender.Hire(this, labor);
	}

	public void Fire(Labor labor)
	{
		IOperableExtender.Fire(this, labor);
	}

	public static void LuaRegister(Script script)
	{
		DynValue value = DynValue.NewTable(script);
		value.Table.Set("GetSelected", DynValue.NewCallback(lua_GetSelected));
		value.Table.Set("DeselectAll", DynValue.NewCallback(lua_DeselectAll));
		value.Table.Set("hk_OnSetSelected", UserData.CreateWithDescriptor(lua_OnSetSelected, LuaHookBase.desc));
		value.Table.Set("GetContent", DynValue.NewCallback(lua_GetContent));
		value.Table.Set("hk_OnSetContent", UserData.CreateWithDescriptor(lua_OnSetContent, LuaHookBase.desc));
		value.Table.Set("AddToQueue", DynValue.NewCallback(lua_AddToQueue));
		value.Table.Set("RemoveFromQueue", DynValue.NewCallback(lua_RemoveFromQueue));
		value.Table.Set("ClearQueue", DynValue.NewCallback(lua_ClearQueue));
		value.Table.Set("GetQueue", DynValue.NewCallback(lua_GetQueue));
		value.Table.Set("hk_OnSetQueue", UserData.CreateWithDescriptor(lua_OnSetQueue, LuaHookBase.desc));
		value.Table.Set("SetIsQueueLooping", DynValue.NewCallback(lua_SetIsQueueLooping));
		value.Table.Set("GetIsQueueLooping", DynValue.NewCallback(lua_GetIsQueueLooping));
		value.Table.Set("hk_OnSetIsQueueLooping", UserData.CreateWithDescriptor(lua_OnSetLoop, LuaHookBase.desc));
		script.Globals.Set("CraftingStation", value);
	}

	public static void lua_UpdateSelection()
	{
		LuaHookBase.InvokeFromCLR(lua_OnSetSelected);
		UpdateConnections(UIGameObserver.crafting.CurrentSelected);
	}

	private static void UpdateConnections(CraftingStation station)
	{
		recepteurConnection.Disconnect();
		queueConnection.Disconnect();
		loopConnection.Disconnect();
		if (!station.IsNullOrReleased())
		{
			recepteurConnection = station.recepteur.InventoryChanged.Connect(delegate(LuaHook h)
			{
				LuaHookBase.InvokeFromCLR(h);
			}, lua_OnSetContent);
			CraftingState component = station.GetComponent<CraftingState>();
			queueConnection = component.QueueChanged.Connect(delegate(LuaHook h)
			{
				LuaHookBase.InvokeFromCLR(h);
			}, lua_OnSetQueue);
			loopConnection = component.LoopChanged.Connect(delegate(LuaHook h)
			{
				LuaHookBase.InvokeFromCLR(h);
			}, lua_OnSetLoop);
		}
	}

	private static CraftingStation SelectedStation()
	{
		return UIGameObserver.crafting.CurrentSelected;
	}

	IReadOnlySignal<GameObject> IOperable<Labor>.get_OnReleaseOrDestroySignal()
	{
		return base.OnReleaseOrDestroySignal;
	}

	GameObject IOperable<Labor>.get_gameObject()
	{
		return base.gameObject;
	}
}
