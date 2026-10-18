using System;
using System.Collections.Generic;
using Brix.Components;
using Brix.Engine;
using Brix.Engine.Nature;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Game.Network;
using Brix.Lifecycle.Pooling;
using Brix.Lua;
using Brix.UI;
using Brix.UI.GameSelector;
using Brix.Utils.Logging;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Input;

public class InputModeController : MonoBehaviour
{
	private static BrixLogger logger = new BrixLogger(typeof(InputModeController).FullName);

	public static GameSignals.GameSignal<InputMode> InputModeChanged = new GameSignals.GameSignal<InputMode>();

	public static Signal SimpleInputModeChanged = new Signal();

	private static InputMode _mode = DefaultMode;

	private static InputMode _previousMode = DefaultMode;

	private static Dictionary<InputMode, Action> _enterMode;

	private static Dictionary<InputMode, Action> _exitMode;

	private static bool _modeShiftInitialised = false;

	private InputMode defaultMode = InputMode.command;

	[LuaDrivable("pickerLocked")]
	private static bool _pickLock = false;

	private static bool _buildModeLock = false;

	private static List<Factory.AssetKey> projectPickers = new List<Factory.AssetKey>
	{
		Pickers.ProjectPlacement,
		Pickers.HarvestPlacement,
		Pickers.MinePlacement,
		Pickers.LandscapePlacement,
		Pickers.BeaconPlacement
	};

	private static bool bricksPanelButtonPressed = false;

	private static bool structuresPanelButtonPressed = false;

	private static bool blueprintsPanelButtonPressed = false;

	private bool preStep = true;

	private bool preStepBricks = true;

	private bool preStepStructures = true;

	private bool preStepBlueprints = true;

	public static Signal<InputContextComponent> InputContextLoaded = new Signal<InputContextComponent>();

	[LuaDrivable("hk_OnSetInputMode")]
	private static LuaHook lua_OnSetInputMode = new LuaHook();

	private static LuaEnumDescriptor<InputMode> _inputDesc = new LuaEnumDescriptor<InputMode>();

	private static LuaObjectDescriptor<InputModeController> _desc = new LuaObjectDescriptor<InputModeController>();

	public static InputMode PreviousMode => _previousMode;

	public static InputMode DefaultMode
	{
		get
		{
			return (!Instance) ? InputMode.command : Instance.defaultMode;
		}
		set
		{
			Instance.defaultMode = value;
		}
	}

	public static bool PickerLocked => _pickLock;

	public static bool BuildModeLocked => __buildModeLock;

	[LuaDrivable("buildmodeLocked")]
	private static bool __buildModeLock
	{
		get
		{
			return _buildModeLock;
		}
		set
		{
			if (value)
			{
				SetCurrentMode(InputMode.blueprint);
			}
			_buildModeLock = value;
		}
	}

	public static InputModeController Instance { get; set; }

	private bool BuildModeButtonDown => PickingUtility.Player.GetButtonDown("cam_buildmode") && !UnityEngine.Input.GetKey(KeyCode.LeftAlt) && !_buildModeLock;

	private bool BricksModeButtonDown => bricksPanelButtonPressed && !UnityEngine.Input.GetKey(KeyCode.LeftAlt) && !_buildModeLock && GetCurrentMode() != InputMode.blueprint;

	private bool StructuresModeButtonDown => structuresPanelButtonPressed && !UnityEngine.Input.GetKey(KeyCode.LeftAlt) && !_buildModeLock && GetCurrentMode() != InputMode.blueprint;

	private bool BlueprintModeButtonDown => blueprintsPanelButtonPressed && !UnityEngine.Input.GetKey(KeyCode.LeftAlt) && !_buildModeLock && GetCurrentMode() != InputMode.blueprint;

	private bool EscapeModeButtonUp => UnityEngine.Input.GetKeyUp(KeyCode.Escape);

	private bool PauseButtonDown => PickingUtility.Player.GetButtonDown("cam_pause_game");

	private void Awake()
	{
		Instance = this;
	}

	public static InputMode GetCurrentMode()
	{
		return _mode;
	}

	public static bool IsInEditMode()
	{
		return IsInEditMode(_mode);
	}

	public static bool PausedInEditMode()
	{
		return GetCurrentMode() == InputMode.pause && IsInEditMode(PreviousMode);
	}

	public static bool GamePlayPaused()
	{
		InputMode currentMode = GetCurrentMode();
		return currentMode == InputMode.pause || currentMode == InputMode.menu;
	}

	public static bool IsInEditMode(InputMode mode)
	{
		return InputMode.editMode.HasFlag(mode);
	}

	public static void ForceCurrentMode(InputMode mode)
	{
		_SetMode(mode);
	}

	public static void SetCurrentMode(InputMode mode)
	{
		if (_mode == InputMode.menu)
		{
			logger.Error("Trying to hijack out of menu mode. Don't! (or at least let the in-game system menu know)");
		}
		else
		{
			_SetMode(mode);
		}
	}

	private static void _SetMode(InputMode mode)
	{
		if (GetCurrentMode() != mode)
		{
			ModeShift(_mode, mode);
			_mode = mode;
			GameSignals.Invoke(InputModeChanged, _mode);
			SimpleInputModeChanged.Invoke();
			LuaHookBase.InvokeFromCLR(lua_OnSetInputMode);
		}
	}

	public static void SetCurrentMode_UNSAFE(InputModeBundle modes, bool force)
	{
		if (!force && _mode == InputMode.menu)
		{
			logger.Warn("Trying to hijack out of menu mode. Don't! (or at least let the in-game system menu know)");
			return;
		}
		for (int num = 1; num > 16; num *= 2)
		{
			InputMode inputMode = (InputMode)num;
			if (modes.Contains(inputMode))
			{
				_mode = inputMode;
				ModeShift(_mode, inputMode);
				break;
			}
		}
	}

	public static bool IsPaused()
	{
		return _mode == InputMode.pause;
	}

	public static bool IsInMenu()
	{
		return _mode == InputMode.menu;
	}

	public static void TogglePause()
	{
		if (_mode != InputMode.worldEditor && !Neo.IsMultiplayer)
		{
			if (!IsPaused())
			{
				MenuSound.PlaySound(MenuSound.Prefix.Other, MenuSound.Suffix.PauseIn);
			}
			else
			{
				MenuSound.PlaySound(MenuSound.Prefix.Other, MenuSound.Suffix.PauseOut);
			}
			ToggleMode(InputMode.pause);
		}
	}

	public static void ToggleMainMenu()
	{
		GameSignals.Invoke(GameSignals.OnMenuToggleRequest);
	}

	public static void PlaceBeacon(Factory.AssetKey key)
	{
		if (key == Groupes.Tunnel)
		{
			Picking.Instance.SendPicker(Pickers.ProjectPlacement, key);
		}
		else if (key == Groupes.Harvest)
		{
			Picking.Instance.SendPicker(Pickers.HarvestPlacement, key);
		}
		else if (key == Groupes.Mine)
		{
			Picking.Instance.SendPicker(Pickers.MinePlacement, key);
		}
		else if (key == Groupes.Landscape)
		{
			Picking.Instance.SendPicker(Pickers.LandscapePlacement, key);
		}
		else
		{
			Picking.Instance.SendPicker(Pickers.BeaconPlacement, key);
		}
	}

	public static void PlaceBeacon(Factory.AssetKey key, IPickerComponentOptions options)
	{
		if (key == Groupes.Tunnel)
		{
			Picking.Instance.SendPicker(Pickers.ProjectPlacement, key, options);
		}
		else if (key == Groupes.Harvest)
		{
			Picking.Instance.SendPicker(Pickers.HarvestPlacement, key, options);
		}
		else if (key == Groupes.Mine)
		{
			Picking.Instance.SendPicker(Pickers.MinePlacement, key, options);
		}
		else if (key == Groupes.Landscape)
		{
			Picking.Instance.SendPicker(Pickers.LandscapePlacement, key, options);
		}
		else
		{
			Picking.Instance.SendPicker(Pickers.BeaconPlacement, key, options);
		}
	}

	public static void PlaceBlueprint(Factory.AssetKey key)
	{
		if (_mode != InputMode.blueprint)
		{
			SetCurrentMode(InputMode.blueprint);
		}
		UIGameSelector.UnselectAll(SelectionType.Blueprint);
		Picking.Instance.SendPicker(Pickers.BlueprintPlacement, key);
	}

	public static void PlaceDefaultBlueprint()
	{
		PlaceBlueprint(Blueprints.Stone_DoubleBrick);
	}

	public static void PlaceDefaultStructuresBlueprint()
	{
		PlaceBlueprint(Blueprints.Pallet);
	}

	private static bool IsHoldingKey()
	{
		return !Picking.Instance.AwaitingClearPick() && Picking.Instance.OverridePicker != null && (Picking.Instance.OverridePicker.LastHeldKey != null || Picking.Instance.OverridePicker.HeldKey != null);
	}

	public static bool IsPlacingProject()
	{
		if (!Picking.Instance.OverridePickerActive())
		{
			return false;
		}
		return projectPickers.Contains(Picking.Instance.OverridePicker.AssetKey);
	}

	public static Factory.AssetKey ProjectBeingPlaced()
	{
		if (IsPlacingProject())
		{
			return Picking.Instance.OverridePicker.LastHeldKey;
		}
		return null;
	}

	public static NatureType HarvestBeingPlaced()
	{
		if (ProjectBeingPlaced() != Groupes.Harvest)
		{
			return NatureType.none;
		}
		return (Picking.Instance.OverridePicker.LastOptions as HarvestProjectPlacementOptions).AcceptedTypes;
	}

	public static bool IsSelectingBlueprints()
	{
		return _mode == InputMode.blueprint && !IsHoldingKey();
	}

	public static bool IsPlacingBlueprints()
	{
		return _mode == InputMode.blueprint && IsHoldingKey();
	}

	public static void SelectBlueprints()
	{
		Picking.Instance.ClearPick();
		if (_mode != InputMode.blueprint)
		{
			SetCurrentMode(InputMode.blueprint);
		}
	}

	public static bool IsSelectingStorage()
	{
		return _mode == InputMode.storage;
	}

	public static void SelectStorages()
	{
		Picking.Instance.ClearPick();
		if (_mode != InputMode.storage)
		{
			SetCurrentMode(InputMode.storage);
		}
	}

	public static void Grab(Factory.AssetKey key)
	{
		Picking.Instance.SendPicker(Pickers.BasicPlacement, key);
	}

	public static void ConnectToSignals()
	{
		UIGameObserver.projects.Selected.Connect(OnInputContextSelect);
		UIGameObserver.projects.Unselected.Connect(OnInputContextUnselect);
		UIGameObserver.dynamicObjects.Selected.Connect(OnInputContextSelect);
		UIGameObserver.dynamicObjects.Unselected.Connect(OnInputContextUnselect);
		GameSignals.Connect(GameSignals.LevelReady, Instance.ResetInput);
	}

	private static void ToggleMode(InputMode mode)
	{
		if (_mode == mode)
		{
			SetCurrentMode(_previousMode);
			return;
		}
		_previousMode = _mode;
		SetCurrentMode(mode);
	}

	private static void ModeShift(InputMode previous, InputMode next)
	{
		if (previous != next)
		{
			if (!_modeShiftInitialised)
			{
				InitializeModeShift();
			}
			if (_exitMode.ContainsKey(previous))
			{
				_exitMode[previous]();
			}
			if (_exitMode.ContainsKey(next))
			{
				_enterMode[next]();
			}
		}
	}

	private static void InitializeModeShift()
	{
		_enterMode = new Dictionary<InputMode, Action>();
		_exitMode = new Dictionary<InputMode, Action>();
		_enterMode.Add(InputMode.command, EnterCommand);
		_enterMode.Add(InputMode.blueprint, EnterBlueprint);
		_enterMode.Add(InputMode.planning, EnterPlanning);
		_enterMode.Add(InputMode.pause, EnterPause);
		_enterMode.Add(InputMode.worldEditor, EnterWorldEditor);
		_enterMode.Add(InputMode.menu, EnterMenu);
		_exitMode.Add(InputMode.command, ExitCommand);
		_exitMode.Add(InputMode.blueprint, ExitBlueprint);
		_exitMode.Add(InputMode.planning, ExitPlanning);
		_exitMode.Add(InputMode.pause, ExitPause);
		_exitMode.Add(InputMode.worldEditor, ExitWorldEditor);
		_exitMode.Add(InputMode.menu, ExitMenu);
		_modeShiftInitialised = true;
	}

	private void Update()
	{
		if (BuildModeButtonDown)
		{
			BuildModeButtonPressed();
		}
		else if (BricksModeButtonDown)
		{
			BricksModeButtonPressed();
		}
		else if (StructuresModeButtonDown)
		{
			StructuresModeButtonPressed();
		}
		else if (BlueprintModeButtonDown)
		{
			BlueprintModeButtonPressed();
		}
		else if (EscapeModeButtonUp)
		{
			UIGameActor.SelectIdleProject();
			SetCurrentMode(DefaultMode);
		}
		else if (PauseButtonDown)
		{
			TogglePause();
		}
		bricksPanelButtonPressed = false;
		structuresPanelButtonPressed = false;
	}

	public void ChangeInputToMenu()
	{
		ControlContext.SetContext("Chat", focus: true);
	}

	public void ChangeInputToGame()
	{
		CreateInputLink();
		ControlContext.SetContext("Chat", focus: false);
	}

	public void ResetInput()
	{
		ChangeInputToMenu();
		ChangeInputToGame();
	}

	public void CreateInputLink()
	{
		ControlContext.CreateLink("Chat", "Camera");
		ControlContext.CreateLink("Chat", "General");
		ControlContext.CreateLink("Chat", "tb_constr_menu");
		ControlContext.CreateLink("Chat", "Bricktron");
		ControlContext.CreateLink("Chat", "TaskMenu");
		ControlContext.CreateLink("Chat", "Projects");
		ControlContext.CreateLink("Chat", "BlocksMenu");
		ControlContext.CreateLink("Chat", "StructuresMenu");
		ControlContext.CreateLink("Chat", "Blueprints");
		ControlContext.CreateLink("Chat", "Crafting");
		ControlContext.CreateLink("Chat", "we_menu");
		ControlContext.CreateLink("Chat", "we_terrain_menu");
		ControlContext.CreateLink("Chat", "we_markers_menu");
	}

	private void BuildModeButtonPressed()
	{
		if (DefaultMode != InputMode.command)
		{
			return;
		}
		if (preStep && (IsSelectingStorage() || IsSelectingBlueprints()))
		{
			if ((bool)UIGameObserver.projects.CurrentSelected)
			{
				UIGameObserver.projects.CurrentSelected.GetComponent<InputContextComponent>().LoadContext();
			}
			preStep = false;
			Neo.Relais(delegate
			{
				BuildModeButtonPressed();
			}, "BuildModePressed");
			return;
		}
		preStep = true;
		if (GetCurrentMode() != InputMode.command)
		{
			UIGameSelector.UnselectAllInTrack(SelectionTrackID.Blueprints);
			UIGameActor.SelectIdleProject();
			return;
		}
		Project lastSelected = UIGameObserver.projects.LastSelected;
		if ((bool)lastSelected)
		{
			UIGameSelector.Select(lastSelected.gameObject);
		}
		else
		{
			User.LocalUser.GetComponent<UNetGroupe>().CallCmdCreate();
		}
	}

	private void BricksModeButtonPressed()
	{
		if (DefaultMode != InputMode.command)
		{
			return;
		}
		if (preStepBricks && (IsSelectingStorage() || IsSelectingBlueprints()))
		{
			if ((bool)UIGameObserver.projects.CurrentSelected)
			{
				UIGameObserver.projects.CurrentSelected.GetComponent<InputContextComponent>().LoadContext();
			}
			preStepBricks = false;
			Neo.Relais(delegate
			{
				BricksModeButtonPressed();
			}, "BricksModePressed");
		}
		else
		{
			preStepBricks = true;
			Project lastSelectedBuild = UIGameObserver.projects.LastSelectedBuild;
			if ((bool)lastSelectedBuild)
			{
				UIGameSelector.Select(lastSelectedBuild.gameObject);
				PlaceDefaultBlueprint();
			}
			else
			{
				User.LocalUser.GetComponent<UNetGroupe>().CallCmdCreate();
			}
		}
	}

	private void StructuresModeButtonPressed()
	{
		if (DefaultMode != InputMode.command)
		{
			return;
		}
		if (preStepStructures && (IsSelectingStorage() || IsSelectingBlueprints()))
		{
			if ((bool)UIGameObserver.projects.CurrentSelected)
			{
				UIGameObserver.projects.CurrentSelected.GetComponent<InputContextComponent>().LoadContext();
			}
			preStepStructures = false;
			Neo.Relais(delegate
			{
				StructuresModeButtonPressed();
			}, "StructuresModePressed");
		}
		else
		{
			preStepStructures = true;
			Project lastSelectedBuild = UIGameObserver.projects.LastSelectedBuild;
			if ((bool)lastSelectedBuild)
			{
				UIGameSelector.Select(lastSelectedBuild.gameObject);
				PlaceDefaultStructuresBlueprint();
			}
			else
			{
				User.LocalUser.GetComponent<UNetGroupe>().CallCmdCreate();
			}
		}
	}

	private void BlueprintModeButtonPressed()
	{
		if (DefaultMode != InputMode.command)
		{
			return;
		}
		if (preStepBlueprints && (IsSelectingStorage() || IsSelectingBlueprints()))
		{
			if ((bool)UIGameObserver.projects.CurrentSelected)
			{
				UIGameObserver.projects.CurrentSelected.GetComponent<InputContextComponent>().LoadContext();
			}
			preStepBlueprints = false;
			Neo.Relais(delegate
			{
				BlueprintModeButtonPressed();
			}, "BlueprintsModePressed");
		}
		else
		{
			preStepBlueprints = true;
			Project lastSelectedBuild = UIGameObserver.projects.LastSelectedBuild;
			if ((bool)lastSelectedBuild)
			{
				UIGameSelector.Select(lastSelectedBuild.gameObject);
				PlaceDefaultBlueprint();
			}
			else
			{
				User.LocalUser.GetComponent<UNetGroupe>().CallCmdCreate();
			}
		}
	}

	private static bool _LocalUserOwnsNoConstructionGroup()
	{
		HashSet<GameObject> instances = BrixSingleton<AutoList>.Instance.GetInstances(Groupes.Construction);
		return !instances.Slinq().Any((GameObject construction) => construction.GetComponent<AffiliationComponent>().faction == User.LocalUser.faction);
	}

	private static void EnterCommand()
	{
	}

	private static void ExitCommand()
	{
		UIGameSelector.UnselectAllInTrack(SelectionTrackID.Command);
	}

	private static void EnterBlueprint()
	{
	}

	private static void ExitBlueprint()
	{
	}

	private static void EnterPlanning()
	{
	}

	private static void ExitPlanning()
	{
	}

	private static void EnterWorldEditor()
	{
	}

	private static void ExitWorldEditor()
	{
	}

	private static void EnterMenu()
	{
		Instance.ChangeInputToMenu();
	}

	private static void ExitMenu()
	{
		Instance.ChangeInputToGame();
	}

	private static void EnterPause()
	{
		ClockConfig.Paused = true;
	}

	private static void ExitPause()
	{
		ClockConfig.Paused = false;
	}

	private static void OnInputContextSelect(Project p)
	{
		InputContextComponent component = p.GetComponent<InputContextComponent>();
		if ((bool)component && !p.IsBeingPlaced)
		{
			OnInputContextSelect(component);
		}
	}

	private static void OnInputContextUnselect(Project p)
	{
		InputContextComponent component = p.GetComponent<InputContextComponent>();
		if ((bool)component && !p.IsBeingPlaced)
		{
			OnInputContextUnselect(component);
		}
	}

	private static void OnInputContextSelect(Transform t)
	{
		InputContextComponent component = t.GetComponent<InputContextComponent>();
		if ((bool)component)
		{
			OnInputContextSelect(component);
		}
	}

	private static void OnInputContextUnselect(Transform t)
	{
		InputContextComponent component = t.GetComponent<InputContextComponent>();
		if ((bool)component)
		{
			OnInputContextUnselect(component);
		}
	}

	private static void OnInputContextSelect(InputContextComponent icc)
	{
		icc.LoadContext();
		Picking.Instance.PickingObserver.OverridePickerUpdateComplete.ConnectOnce(delegate
		{
			InputContextLoaded.Invoke(icc);
		});
	}

	private static void OnInputContextUnselect(InputContextComponent icc)
	{
		icc.SaveCurrentContext();
	}

	public static void RegisterClass(Script script)
	{
		script.Globals.Set("InputModeController", _desc.GetStaticProxy());
	}

	[LuaDrivable("IsInEditMode")]
	private static DynValue lua_IsInEditMode(ScriptExecutionContext sec, CallbackArguments ca)
	{
		return DynValue.NewBoolean(IsInEditMode(_mode));
	}

	[LuaDrivable("IsPaused")]
	private static DynValue lua_IsPaused(ScriptExecutionContext sec, CallbackArguments ca)
	{
		return DynValue.NewBoolean(IsPaused());
	}

	[LuaDrivable("TogglePause")]
	private static DynValue lua_togglePause(ScriptExecutionContext sec, CallbackArguments ca)
	{
		TogglePause();
		return DynValue.Void;
	}

	[LuaDrivable("ChangeInputToText")]
	private static DynValue lua_changeInputToText(ScriptExecutionContext sec, CallbackArguments ca)
	{
		Instance.ChangeInputToMenu();
		return DynValue.Void;
	}

	[LuaDrivable("ChangeInputToGame")]
	private static DynValue lua_changeInputToGame(ScriptExecutionContext sec, CallbackArguments ca)
	{
		Instance.ChangeInputToGame();
		return DynValue.Void;
	}

	[LuaDrivable("BricksPanelButtonPressed")]
	private static DynValue lua_bricksPanelButtonPressed(ScriptExecutionContext sec, CallbackArguments ca)
	{
		bricksPanelButtonPressed = true;
		return DynValue.Void;
	}

	[LuaDrivable("StructuresPanelButtonPressed")]
	private static DynValue lua_structuresPanelButtonPressed(ScriptExecutionContext sec, CallbackArguments ca)
	{
		structuresPanelButtonPressed = true;
		return DynValue.Void;
	}

	[LuaDrivable("BlueprintsPanelButtonPressed")]
	private static DynValue lua_blueprintsPanelButtonPressed(ScriptExecutionContext sec, CallbackArguments ca)
	{
		blueprintsPanelButtonPressed = true;
		return DynValue.Void;
	}
}
