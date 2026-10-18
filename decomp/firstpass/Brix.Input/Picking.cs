using System;
using System.Collections.Generic;
using Brix.Common.Collections;
using Brix.Engine;
using Brix.External.Factories;
using Brix.Game.Components;
using Brix.Game.Rules;
using Brix.Input.WorldEditor;
using Brix.Lifecycle.Pooling;
using Brix.Lua;
using Brix.UI.GameSelector;
using Brix.Utils;
using Brix.Utils.Collections;
using Brix.Utils.Logging;
using MoonSharp.Interpreter;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Brix.Input;

public class Picking : UpdatableGameComponent<Picking>
{
	private static BrixLogger logger = new BrixLogger(typeof(Picking).FullName);

	public static Picking Instance;

	public XRay xRay;

	public float MaxDistance = 1000f;

	public PickingObserver PickingObserver = new PickingObserver();

	[LuaDrivable("hk_PickerOptionChanged")]
	private static LuaHook _lua_OverridePickerSetWithOptions = new LuaHook(luaOverridePickerSetWithOptions);

	[LuaDrivable("hk_PickerChanged")]
	private static LuaHook<Picker, Picker> _lua_OnSetUserActivePicker = new LuaHook<Picker, Picker>(lua_OnSetUserActivePicker);

	private Picker _overridePicker;

	private bool _overridePickerActive;

	private List<Picker> _registeredPickers = new List<Picker>();

	private List<Picker> _registeredSlowPickers = new List<Picker>();

	private List<Picker> _activePickers = new List<Picker>();

	private List<Picker> _activeSlowPickers = new List<Picker>();

	private List<ClickInfo> _referenceClickInfos = new List<ClickInfo>
	{
		new ClickInfo("cam_action1"),
		new ClickInfo("cam_action2")
	};

	public BrushHighlight BrushHighlight;

	public WorldGridController WorldEditorGrid;

	private static Factory.AssetKey _toSetPickerKey = null;

	private static Factory.AssetKey _toSetHeldKey = null;

	private static IPickerComponentOptions _toSetPickerOptions = null;

	private static bool _clearOverridePicker = false;

	private bool _uiHovered;

	private bool _UILock;

	private bool _lastFrameLocked;

	[LuaDrivable("hk_OnSetPickerOptions")]
	private static LuaHook lua_OnSetPickerOptions = new LuaHook(PickingObserver.lua_OverridePickerSetWithOptions);

	[LuaDrivable("hk_OnSetPicker")]
	private static LuaHook<Picker, Picker> lua_OnSetPicker = new LuaHook<Picker, Picker>(PickingObserver.lua_OnSetUserActivePicker);

	[LuaDrivable("hk_OnSetInputMode")]
	private static LuaHook<InputMode> lua_OnSetInputMode = new LuaHook<InputMode>(InputModeController.InputModeChanged);

	[LuaDrivable("hk_BPSelectionChanged")]
	private static LuaHook lua_BPSelectionChanged = new LuaHook(PickingObserver.lua_BlueprintSelectionActionTaken);

	public static GameSignals.GameSignal luaOverridePickerSetWithOptions => PickingObserver.lua_OverridePickerSetWithOptions;

	public static GameSignals.GameSignal<Picker, Picker> lua_OnSetUserActivePicker => PickingObserver.lua_OnSetUserActivePicker;

	public bool UILocked
	{
		get
		{
			return _UILock || _uiHovered || !Curseur.IsInGameWindow();
		}
		set
		{
			_UILock = value;
		}
	}

	public static bool actif { get; set; }

	public Picker OverridePicker
	{
		get
		{
			return _overridePicker;
		}
		set
		{
			if (_overridePicker != value)
			{
				GameSignals.Invoke(PickingObserver.lua_OnSetUserActivePicker, _overridePicker, value);
				_overridePicker = value;
			}
		}
	}

	public object PieMenuTarget
	{
		get
		{
			Picker picker = _activePickers.Slinq().FirstOrDefault((Picker f) => f.type == PickerType.piemenu);
			if (picker != null)
			{
				object result = picker.PieMenuTarget;
				if (picker.PieMenuTarget is GameObject)
				{
					result = SignificantParent.Of((GameObject)picker.PieMenuTarget);
				}
				else if (picker.PieMenuTarget is Component)
				{
					result = SignificantParent.Of((Component)picker.PieMenuTarget);
				}
				return result;
			}
			return null;
		}
	}

	public List<ClickInfo> ReferenceClicks => _referenceClickInfos;

	protected override float SlowUpdateRate => 0.05f;

	public bool OverridePickerActive()
	{
		return _overridePickerActive && _overridePicker != null;
	}

	protected override void Awake()
	{
		base.Awake();
		Instance = this;
	}

	protected override void Start()
	{
		base.Start();
		GameSignals.Once(GameSignals.LevelLoaded, delegate(Picking self)
		{
			self.FirstPick();
		}, this);
		GameSignals.DisableUntil(GameSignals.LevelReady, this);
		GameSignals.Once(GameSignals.LevelLoaded, InitializeInputMode);
		GameSignals.Once(GameSignals.LeaveGame, InputModeController.Instance.ChangeInputToGame);
	}

	private void FirstPick()
	{
		if (InputModeController.DefaultMode == InputMode.none)
		{
			InputModeController.DefaultMode = InputMode.command;
		}
		InputModeController.ConnectToSignals();
		InputModeController.ForceCurrentMode(InputModeController.DefaultMode);
		AddPick(Pickers.CommandSelection);
		AddPick(Pickers.VolumeHandle);
		AddPick(Pickers.BlueprintSelection);
		AddPick(Pickers.WorldEditorSelection);
		AddPick(Pickers.UnitAction);
		AddPick(Pickers.ObjectAction);
		AddPick(Pickers.TerrainAction);
		AddPick(Pickers.BlueprintAction);
		AddPick(Pickers.BeaconPiemenu);
		AddPick(Pickers.UnitPiemenu);
		AddPick(Pickers.ObjectPiemenu);
		AddPick(Pickers.TerrainPiemenu);
		AddPick(Pickers.BlueprintPiemenu);
		AddPick(Pickers.CatapultTargetPiemenu);
		AddPick(Pickers.BlueprintPlacement);
		AddPick(Pickers.Cursor);
		AddPick(Pickers.EditModeCursor);
		AddPick(Pickers.BoxDragPicker);
		AddPick(Pickers.UniversalGrabber);
		AddPick(Pickers.BasicPlacement);
		AddPick(Pickers.Explosion);
	}

	private void Update()
	{
		foreach (ClickInfo referenceClickInfo in _referenceClickInfos)
		{
			referenceClickInfo.SetClickStatus();
		}
		OnUpdate(_registeredPickers, ref _activePickers);
	}

	protected override void BrixSlowUpdate()
	{
		OnUpdate(_registeredSlowPickers, ref _activeSlowPickers);
	}

	private void OnUpdate(List<Picker> pickerPool, ref List<Picker> pickingMemory)
	{
		UpdateActionModifiers();
		_uiHovered = (bool)EventSystem.current && EventSystem.current.IsPointerOverGameObject();
		UpdateOverridePicker();
		List<Picker> pickersToSetActive = CollectionPool.RequestList<Picker>();
		SelectActivePickers(pickerPool, ref pickersToSetActive);
		SetPickersActive(ref pickersToSetActive, ref pickingMemory);
		pickersToSetActive.Dispose();
		foreach (Picker item in pickingMemory)
		{
			item.UpdateClickInfo();
			if (!actif || UILocked || _lastFrameLocked)
			{
				if (item.locked && (item.type == PickerType.targetless || item.type == PickerType.placement))
				{
					item.OnAction();
				}
				else
				{
					item.EnterStandBy();
				}
			}
			else
			{
				item.OnAction();
			}
		}
		_lastFrameLocked = !actif || UILocked;
		actif = true;
	}

	private void AddPieMenuPicker(List<Picker> pickerSet, ref List<Picker> pickersToSetActive, Vector3 mousePos)
	{
		Picker picker = null;
		Slinq<Picker, PredicateContext<Picker, IListContext<Picker>>> slinq = from p in pickerSet.Slinq()
			where p.type == PickerType.piemenu
			select p;
		foreach (Picker item in slinq.NoLeak())
		{
			if (item.PlaceAtRaycast(Camera.main.ScreenPointToRay(mousePos), MaxDistance) && (picker == null || item.actionPriority > picker.actionPriority || (item.actionPriority == picker.actionPriority && item.RaycastHit.DistanceToHit < picker.RaycastHit.DistanceToHit)))
			{
				picker = item;
			}
		}
		if (picker != null)
		{
			pickersToSetActive.Add(picker);
		}
	}

	private bool AddOverridePicker(List<Picker> pickerSet, ref List<Picker> pickersToSetActive, Vector3 mousePos)
	{
		if (_overridePicker != null && _overridePicker.RegisteredModes.Contains(InputModeController.GetCurrentMode()) && OverrideAllowedByTaskRules())
		{
			if (pickerSet.Contains(_overridePicker))
			{
				_overridePicker.PlaceAtRaycast(Camera.main.ScreenPointToRay(mousePos), MaxDistance);
				pickersToSetActive.Add(_overridePicker);
				if (!_overridePickerActive)
				{
					PickingObserver.OverridePickerActivated.Invoke();
					_overridePickerActive = true;
				}
			}
			return true;
		}
		if (_overridePickerActive)
		{
			PickingObserver.OverridePickerDeactivated.Invoke();
			_overridePickerActive = false;
		}
		return false;
	}

	public bool OverrideAllowedByTaskRules()
	{
		return !(_overridePicker.mainComponent is BlueprintPlacementPicker) || (TaskRules.Main.AllowedTasks & TaskType.Construction) > TaskType.None;
	}

	private void AddTargetLessPicker(List<Picker> pickerSet, ref List<Picker> pickersToSetActive, Vector3 mousePos)
	{
		Slinq<Picker, PredicateContext<Picker, IListContext<Picker>>> slinq = from p in pickerSet.Slinq()
			where p.type == PickerType.targetless
			select p;
		Ray ray = Camera.main.ScreenPointToRay(mousePos);
		foreach (Picker item in slinq.NoLeak())
		{
			item.PlaceAtRaycast(ray, MaxDistance);
			pickersToSetActive.Add(item);
		}
	}

	private void AddActionPicker(List<Picker> pickerSet, ref List<Picker> pickersToSetActive, Vector3 mousePos)
	{
		Picker picker = null;
		Slinq<Picker, PredicateContext<Picker, IListContext<Picker>>> slinq = from p in pickerSet.Slinq()
			where p.type == PickerType.action
			select p;
		Ray ray = Camera.main.ScreenPointToRay(mousePos);
		foreach (Picker item in slinq.NoLeak())
		{
			if (item.PlaceAtRaycast(ray, MaxDistance) && (picker == null || item.actionPriority > picker.actionPriority || (item.actionPriority == picker.actionPriority && item.RaycastHit.DistanceToHit < picker.RaycastHit.DistanceToHit)))
			{
				picker = item;
			}
		}
		if (picker != null)
		{
			pickersToSetActive.Add(picker);
		}
	}

	private void SelectActivePickers(List<Picker> pickerPool, ref List<Picker> pickersToSetActive)
	{
		Vector3 mousePos = PickingUtility.Player.controllers.Mouse.screenPosition;
		List<Picker> list = (from p in pickerPool.Slinq()
			where p.RegisteredModes.Contains(InputModeController.GetCurrentMode())
			select p).ToPooledList();
		AddPieMenuPicker(list, ref pickersToSetActive, mousePos);
		if (!AddOverridePicker(list, ref pickersToSetActive, mousePos))
		{
			AddTargetLessPicker(list, ref pickersToSetActive, mousePos);
			AddActionPicker(list, ref pickersToSetActive, mousePos);
			list.Dispose();
		}
	}

	public void UpdateOverridePicker()
	{
		bool pickerLocked = InputModeController.PickerLocked;
		bool buildModeLocked = InputModeController.BuildModeLocked;
		bool flag = false;
		if (_clearOverridePicker)
		{
			SetPick(null);
			_clearOverridePicker = false;
			flag = true;
		}
		else
		{
			if (!pickerLocked && !buildModeLocked && _toSetPickerKey != null)
			{
				SetPick(_toSetPickerKey);
				_toSetPickerKey = null;
				flag = true;
			}
			if (OverridePicker != null)
			{
				if (!pickerLocked && !buildModeLocked && _toSetHeldKey != null)
				{
					OverridePicker.HeldKey = _toSetHeldKey;
					OverridePicker.LastHeldKey = _toSetHeldKey;
					_toSetHeldKey = null;
					flag = true;
				}
				if (_toSetPickerOptions != null)
				{
					OverridePicker.SetOptions(_toSetPickerOptions);
					OverridePicker.LastOptions = _toSetPickerOptions;
					_toSetPickerOptions = null;
					flag = true;
				}
			}
		}
		if (flag)
		{
			GameSignals.Invoke(PickingObserver.lua_OverridePickerSetWithOptions);
			PickingObserver.OverridePickerSetWithOptions.Invoke();
		}
		PickingObserver.OverridePickerUpdateComplete.Invoke();
	}

	public void ForceUpdateActionModifiers()
	{
		UpdateActionModifiers();
	}

	private void UpdateActionModifiers()
	{
		PickingObserver.IsAltAction = PickingUtility.Player.GetButton("cam_mouse_alt");
		PickingObserver.IsRotateAction = PickingUtility.Player.GetButton("cam_mouse_movement") || PickingUtility.Player.GetButton("cam_mouse_rotate");
		PickingObserver.IsSecondaryAction = PickingUtility.Player.GetButton("cam_mouse_sec");
	}

	public void SendPicker(Factory.AssetKey pickerKey, Factory.AssetKey heldKey = null, IPickerComponentOptions options = null)
	{
		logger.InfoFormat("Picker key request sent with id '{0}'", pickerKey);
		if (pickerKey == null)
		{
			ClearPick();
			return;
		}
		_toSetPickerKey = pickerKey;
		_toSetHeldKey = heldKey;
		_toSetPickerOptions = options;
	}

	public void AddHeldKeyToPicker(Factory.AssetKey heldKey)
	{
		_toSetHeldKey = heldKey;
	}

	public void AddOptionToPicker(IPickerComponentOptions options)
	{
		_toSetPickerOptions = options;
	}

	public void ClearPick()
	{
		_clearOverridePicker = true;
	}

	public void CancelPickOrder()
	{
		_toSetPickerKey = null;
		_toSetHeldKey = null;
		_toSetPickerOptions = null;
	}

	public bool AwaitingClearPick()
	{
		return _clearOverridePicker;
	}

	private void SetPickersActive(ref List<Picker> toSetActive, ref List<Picker> pickingMemory)
	{
		UpdatePickerStates(toSetActive, pickingMemory);
		Donnees.Swap(ref toSetActive, ref pickingMemory);
	}

	private Picker SetPick(Factory.AssetKey key)
	{
		Picker pick = GetPick(key);
		if (pick != null)
		{
			OverridePicker = pick;
		}
		else
		{
			OverridePicker = AddPick(key);
		}
		return OverridePicker;
	}

	private Picker AddPick(Factory.AssetKey key)
	{
		if (key.IsNullOrInvalid())
		{
			return null;
		}
		RemovePick(key);
		Picker picker = InstantiatePicker(key);
		if (picker != null)
		{
			if (picker.OnSlowUpdate)
			{
				_registeredSlowPickers.Add(picker);
			}
			else
			{
				_registeredPickers.Add(picker);
			}
		}
		return picker;
	}

	private void DeletePicker(Picker pick)
	{
		if (pick != null)
		{
			pick.Deactivate();
			ObjectPoolSingleton.Release(pick.gameObject);
		}
	}

	private void RemovePick(Factory.AssetKey key)
	{
		Picker pick = GetPick(key);
		_registeredPickers.Remove(pick);
		_registeredSlowPickers.Remove(pick);
		DeletePicker(pick);
	}

	private Picker GetPick(Factory.AssetKey key)
	{
		Picker picker = _registeredPickers.Slinq().Where((Picker x, Factory.AssetKey aKey) => x != null && x.GetComponent<FactoryImprint>().AssetKey == aKey, key).FirstOrDefault();
		if (picker != null)
		{
			return picker;
		}
		return _registeredSlowPickers.Slinq().Where((Picker x, Factory.AssetKey aKey) => x != null && x.GetComponent<FactoryImprint>().AssetKey == aKey, key).FirstOrDefault();
	}

	private Picker InstantiatePicker(Factory.AssetKey key)
	{
		Picker t = null;
		if (key != null && !string.IsNullOrEmpty(key.Factory) && !string.IsNullOrEmpty(key.Name))
		{
			ObjectPoolSingleton.RequestComponent<Picker>(out t, key);
			t.Init();
			t.gameObject.transform.parent = base.transform;
			if (t.mainComponent is BrushComponent)
			{
				t.LoadBrushSettings();
			}
		}
		return t;
	}

	private void UpdatePickerStates(List<Picker> newPicks, List<Picker> oldPicks)
	{
		foreach (Picker oldPick in oldPicks)
		{
			if (!newPicks.Contains(oldPick))
			{
				oldPick.Deactivate();
			}
			else
			{
				oldPick.UpdateActiveComponent();
			}
		}
		foreach (Picker newPick in newPicks)
		{
			if (!oldPicks.Contains(newPick))
			{
				newPick.Activate();
			}
		}
	}

	public static bool IsSelectingStorage()
	{
		return InputModeController.IsSelectingStorage();
	}

	public static bool IsSelectingBlueprint()
	{
		return InputModeController.IsSelectingBlueprints();
	}

	public static void StartSelectingStorage()
	{
		InputModeController.SelectStorages();
	}

	public static void StopSelectingStorage()
	{
		InputMode defaultInputMode = UIGameObserver.GetCurrentProjectInputContext().defaultInputMode;
		InputModeController.SetCurrentMode(defaultInputMode);
	}

	public static void SetBlueprintInPicker(Factory.AssetKey key)
	{
		InputModeController.PlaceBlueprint(key);
	}

	public static Factory.AssetKey BlueprintBeingPlaced()
	{
		if (!InputModeController.IsPlacingBlueprints())
		{
			return null;
		}
		return Instance.OverridePicker.LastHeldKey;
	}

	public static bool IsModeStorageSelection()
	{
		return InputModeController.IsSelectingStorage();
	}

	public static void SetModeToStorageSelection()
	{
		InputModeController.SelectStorages();
	}

	public static void SetModeToBlueprintSelection()
	{
		InputModeController.SelectBlueprints();
	}

	public static void SetModeToBlueprintPlacement()
	{
		Instance.SendPicker(Pickers.BlueprintPlacement);
		InputModeController.SetCurrentMode(InputMode.blueprint);
	}

	public static void SetModeToDefault()
	{
		Instance.ClearPick();
		InputModeController.SetCurrentMode(UIGameObserver.GetCurrentProjectInputContext().defaultInputMode);
	}

	public static void InitializeInputMode()
	{
		InputModeController.ForceCurrentMode(InputMode.command);
	}

	[LuaDrivable("SetBlueprintPicker")]
	private static DynValue lua_SetBlueprintPicker(ScriptExecutionContext context, CallbackArguments args)
	{
		Factory.AssetKey heldKey = args.AsUserData<Factory.AssetKey>(0, "SetBlueprintPicker");
		bool instant = args.Count >= 2 && args.AsBool(1, "SetBlueprintPicker");
		bool flag = args.Count >= 3 && args.AsBool(2, "SetBlueprintPicker");
		Instance.SetPick((!flag) ? Pickers.BlueprintPlacementOnly : Pickers.BlueprintPlacement);
		Instance.OverridePicker.HeldKey = heldKey;
		Instance.OverridePicker.SetOptions(new BlueprintPlacementPicker.BlueprintPlacementOptions(instant));
		return DynValue.Void;
	}

	[LuaDrivable("IsEditMode")]
	private static DynValue lua_IsEditMode(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewBoolean(InputModeController.IsInEditMode());
	}

	[LuaDrivable("IsCommandMode")]
	private static DynValue lua_IsCommandMode(ScriptExecutionContext sec, CallbackArguments ca)
	{
		return DynValue.NewBoolean(InputModeController.GetCurrentMode() == InputMode.command);
	}

	[LuaDrivable("ResetCommandMode")]
	private static DynValue lua_SetAsCommandMode(ScriptExecutionContext sec, CallbackArguments ca)
	{
		InputModeController.SetCurrentMode(InputMode.command);
		return DynValue.Void;
	}

	[LuaDrivable("ResetWorldEditorMode")]
	private static DynValue lua_ResetWorldEditorMode(ScriptExecutionContext sec, CallbackArguments ca)
	{
		InputModeController.DefaultMode = InputMode.worldEditor;
		InputModeController.SetCurrentMode(InputMode.worldEditor);
		return DynValue.Void;
	}

	[LuaDrivable("IsHoldingBlueprints")]
	private static DynValue lua_IsHoldingBlueprints(ScriptExecutionContext sec, CallbackArguments ca)
	{
		return DynValue.NewBoolean(InputModeController.IsPlacingBlueprints());
	}

	[LuaDrivable("SetIsHoldingBlueprints")]
	public static DynValue lua_SetIsHoldingBlueprints(ScriptExecutionContext sec, CallbackArguments ca)
	{
		SetModeToBlueprintPlacement();
		return DynValue.Void;
	}

	[LuaDrivable("GetHeldBlueprint")]
	private static DynValue lua_GetHeldBlueprint(ScriptExecutionContext sec, CallbackArguments ca)
	{
		Factory.AssetKey assetKey = BlueprintBeingPlaced();
		return (!(assetKey == null)) ? DynValue.FromObject(sec.OwnerScript, assetKey) : DynValue.Void;
	}

	[LuaDrivable("SetHeldBlueprint")]
	public static DynValue lua_SetHeldBlueprint(ScriptExecutionContext sec, CallbackArguments ca)
	{
		Factory.AssetKey blueprintInPicker = ca.AsUserData<Factory.AssetKey>(0, "asset key");
		SetBlueprintInPicker(blueprintInPicker);
		return DynValue.Void;
	}

	[LuaDrivable("SetIsSelectingBlueprints")]
	public static DynValue lua_SetIsSelectingBlueprints(ScriptExecutionContext sec, CallbackArguments ca)
	{
		bool boolean = ca.AsType(0, "flag", DataType.Boolean).Boolean;
		if (boolean && !IsSelectingBlueprint())
		{
			if ((bool)UIGameObserver.projects.CurrentSelected)
			{
				UIGameObserver.projects.CurrentSelected.GetComponent<InputContextComponent>().SaveCurrentContext();
			}
			SetModeToBlueprintSelection();
		}
		else if (!boolean && IsSelectingBlueprint())
		{
			if ((bool)UIGameObserver.projects.CurrentSelected)
			{
				UIGameObserver.projects.CurrentSelected.GetComponent<InputContextComponent>().LoadContext();
			}
			if (IsSelectingBlueprint())
			{
				Instance.SendPicker(Pickers.BlueprintPlacement, Blueprints.Stone_DoubleBrick);
			}
		}
		return DynValue.Void;
	}

	[LuaDrivable("SetIsSelectingStorage")]
	public static DynValue lua_SetIsSelectingStorage(ScriptExecutionContext sec, CallbackArguments ca)
	{
		return StashModeAndChange(sec, ca, delegate
		{
			SetModeToStorageSelection();
		}, IsSelectingStorage);
	}

	private static DynValue StashModeAndChange(ScriptExecutionContext sec, CallbackArguments ca, Action modeAction, Func<bool> isCurrentMode)
	{
		if (ca.AsType(0, "flag", DataType.Boolean).Boolean && !isCurrentMode())
		{
			if ((bool)UIGameObserver.projects.CurrentSelected)
			{
				UIGameObserver.projects.CurrentSelected.GetComponent<InputContextComponent>().SaveCurrentContext();
			}
			modeAction();
		}
		else if (isCurrentMode() && (bool)UIGameObserver.projects.CurrentSelected)
		{
			UIGameObserver.projects.CurrentSelected.GetComponent<InputContextComponent>().LoadContext();
		}
		return DynValue.Void;
	}

	[LuaDrivable("DeselectAll")]
	private static DynValue lua_DeselectAll(ScriptExecutionContext context, CallbackArguments args)
	{
		UIGameSelector.UnselectEverything();
		return DynValue.Void;
	}

	[LuaDrivable("Clear")]
	private static DynValue lua_ClearPick(ScriptExecutionContext context, CallbackArguments args)
	{
		InputModeController.SetCurrentMode(InputModeController.DefaultMode);
		Instance.OverridePicker.SetOptions(new PickerComponentOptionsReset());
		Instance.ClearPick();
		return DynValue.Void;
	}

	[LuaDrivable("SetBlueprintPlacement")]
	private static DynValue lua_SetBlueprintInPicker(ScriptExecutionContext context, CallbackArguments args)
	{
		Factory.AssetKey key = args.AsUserData<Factory.AssetKey>(0, "SetBlueprintPlacement");
		InputModeController.PlaceBlueprint(key);
		return DynValue.Void;
	}

	[LuaDrivable("IsSelectingBlueprints")]
	private static DynValue lua_IsSelectingBlueprints(ScriptExecutionContext sec, CallbackArguments ca)
	{
		return DynValue.NewBoolean(InputModeController.IsSelectingBlueprints());
	}

	[LuaDrivable("IsPlacingBlueprints")]
	private static DynValue lua_IsPlacingBlueprints(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewBoolean(InputModeController.IsPlacingBlueprints());
	}

	[LuaDrivable("BlueprintBeingPlaced")]
	private static DynValue lua_BlueprintBeingPlaced(ScriptExecutionContext context, CallbackArguments args)
	{
		if (!InputModeController.IsPlacingBlueprints())
		{
			return DynValue.Invalid;
		}
		return DynValue.FromObject(context.OwnerScript, Instance.OverridePicker.LastHeldKey);
	}

	[LuaDrivable("IsSelectingStorage")]
	private static DynValue lua_IsSelectingStorage(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewBoolean(IsSelectingStorage());
	}

	[LuaDrivable("StartSelectingStorage")]
	private static DynValue lua_StartSelectingStorage(ScriptExecutionContext context, CallbackArguments args)
	{
		StartSelectingStorage();
		return DynValue.Void;
	}

	[LuaDrivable("StopSelectingStorage")]
	private static DynValue lua_StopSelectingStorage(ScriptExecutionContext context, CallbackArguments args)
	{
		StopSelectingStorage();
		return DynValue.Void;
	}

	[LuaDrivable("SetKeyInPicker")]
	private static DynValue lua_SetKeyInPicker(ScriptExecutionContext context, CallbackArguments args)
	{
		Factory.AssetKey heldKey = args.AsUserData<Factory.AssetKey>(0, "SetKeyInPicker");
		Factory.AssetKey assetKey = null;
		if (args.Count >= 2)
		{
			assetKey = args.AsUserData<Factory.AssetKey>(1, "SetKeyInPicker");
		}
		if (assetKey == null)
		{
			assetKey = Pickers.UniversalGrabber;
		}
		Instance.SendPicker(assetKey, heldKey);
		return DynValue.Void;
	}
}
