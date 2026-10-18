using System;
using System.Collections.Generic;
using Brix.Engine;
using Brix.Engine.Nature;
using Brix.External.Signals;
using Brix.Game;
using Brix.Game.AI;
using Brix.Game.AI.Damage;
using Brix.Game.AI.Nodes;
using Brix.Game.Components;
using Brix.Game.Rules;
using Brix.Game.Semantique;
using Brix.Input.ContextualOrders;
using Brix.UI.GameSelector;
using Brix.Utils;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Input;

public class CursorController : MonoBehaviour
{
	public CursorState currentState;

	private List<CursorState> cursorStates = new List<CursorState>();

	public Signal Cleared = new Signal
	{
		MaximumConnections = 1000
	};

	private const float startAt = 0.3f;

	public bool UpdateOnNextFrame { get; set; }

	private Signal<Labor> CharacterEnterHover => UIGameObserver.bricktrons.EnterHover;

	private Signal<Labor> CharacterExitHover => UIGameObserver.bricktrons.ExitHover;

	private Labor CharacterHovered => UIGameObserver.bricktrons.CheckHover().Slinq().FirstOrDefault();

	private Blueprint BlueprintHovered => UIGameObserver.blueprints.CurrentHovered;

	private Blueprint BlueprintLastHovered => UIGameObserver.blueprints.LastHovered;

	private Signal<Blueprint> BlueprintHoverEnter => UIGameObserver.blueprints.Hovered;

	private Signal<Blueprint> BlueprintHoverExit => UIGameObserver.blueprints.EndHovered;

	private Signal<Blueprint> BlueprintEnterSelection => UIGameObserver.blueprints.Selected;

	private Signal<Blueprint> BlueprintExitSelection => UIGameObserver.blueprints.Unselected;

	private Signal<Labor> CharacterEnterSelection => UIGameObserver.bricktrons.EnterSelection;

	private Signal<Labor> CharacterExitSelection => UIGameObserver.bricktrons.ExitSelection;

	private Labor FirstSelectedCharacter => UIGameObserver.bricktrons.CheckSelection().Slinq().FirstOrDefault();

	private HashSet<Labor> SelectedCharacters => UIGameObserver.bricktrons.CheckSelection();

	private Signal<Labor> EnnemyDied => UIGameObserver.enemyBricktrons.ExitCreation;

	private Signal<Labor> EnnemyEnterHover => UIGameObserver.enemyBricktrons.EnterHover;

	private Signal<Labor> EnnemyExitHover => UIGameObserver.enemyBricktrons.ExitHover;

	private Labor EnnemyHovered => UIGameObserver.enemyBricktrons.CheckHover().Slinq().FirstOrDefault();

	private Signal<Labor> CharacterInventoryChanged => UIGameObserver.bricktrons.InventoryChanged;

	private Signal<Transform> GameObjectHoverEnter => UIGameObserver.dynamicObjects.Hovered;

	private Signal<Transform> GameObjectHoverExit => UIGameObserver.dynamicObjects.Unhovered;

	private Signal<Transform> GameObjectSelectionEnter => UIGameObserver.dynamicObjects.Selected;

	private Signal<Transform> GameObjectSelectionExit => UIGameObserver.dynamicObjects.Unselected;

	private Transform GameObjectHovered
	{
		get
		{
			List<GameObject> gos = CollectionPool.RequestList<GameObject>();
			UIGameSelector.GetHoveredEverything(ref gos);
			Transform result = (from g in gos.Slinq()
				select g.transform).FirstOrDefault();
			gos.Dispose();
			return result;
		}
	}

	private IEnumerable<Transform> GameObjectsSelected => UIGameObserver.dynamicObjects.AllSelected;

	private Signal<Project, Labor> ProjectHired => UIGameObserver.projects.Hired;

	private Signal<Project> ProjectEnterHover => UIGameObserver.projects.Hovered;

	private Signal<Project> ProjectExitHover => UIGameObserver.projects.EndHovered;

	private Project ProjectHovered => UIGameObserver.projects.CurrentHovered;

	private Signal TerrainHoveredChanged => CursorPicker.TerrainHoverdChanged;

	private Vector3 TerrainHovered => CursorPicker.TerrainHovered;

	private bool IsHoveringTerrain => CursorPicker.IsHoveringTerrain;

	private Signal<Vector3> TreeHovered => UIGameObserver.dynamicObjects.TreeHovered;

	private Signal<Vector3> TreeUnhovered => UIGameObserver.dynamicObjects.TreeUnhovered;

	private Vector3 HoveredTree => UIGameObserver.dynamicObjects.GetHoveredTree;

	private bool IsHoveringTree => UIGameObserver.dynamicObjects.IsHoveringTree;

	private Signal<Vector3> PlantHovered => UIGameObserver.dynamicObjects.PlantHovered;

	private Signal<Vector3> PlantUnhovered => UIGameObserver.dynamicObjects.PlantUnhovered;

	private Vector3 HoveredPlant => UIGameObserver.dynamicObjects.GetHoveredPlant;

	private bool IsHoveringPlant => UIGameObserver.dynamicObjects.IsHoveringPlant;

	private Signal ViewModeChanged => Picking.Instance.PickingObserver.IsRotateActionChanged;

	private bool ViewMode => Picking.Instance.PickingObserver.IsRotateAction;

	private Signal ToggleSelectModeChanged => Picking.Instance.PickingObserver.IsAltActionChanged;

	private bool ToggleSelectMode => Picking.Instance.PickingObserver.IsAltAction;

	private Signal SecondaryActionChanged => Picking.Instance.PickingObserver.IsSecondaryActionChanged;

	private bool SecondaryAction => Picking.Instance.PickingObserver.IsSecondaryAction;

	private Signal AlternateActionChanged => Picking.Instance.PickingObserver.IsAltActionChanged;

	private bool AlternateAction => Picking.Instance.PickingObserver.IsAltAction;

	private Signal SelectionRectEnabledChanged => CursorPicker.RectEnabledChanged;

	private bool SelectionRectEnabled => SelectionBox.active;

	private Signal InputModeChanged => InputModeController.SimpleInputModeChanged;

	private bool EditMode => InputModeController.IsInEditMode();

	private Signal<DragHandle> HandleDragged => UIGameObserver.handles.Dragged;

	private Signal<DragHandle> HandleEndDragged => UIGameObserver.handles.EndDragged;

	private Signal<DragHandle> HandleHovered => UIGameObserver.handles.Hovered;

	private Signal<DragHandle> HandleEndHovered => UIGameObserver.handles.EndHovered;

	private DragHandle FocusedHandle => UIGameObserver.handles.CurrentDragged ?? UIGameObserver.handles.CurrentHovered;

	private Signal PieMenuOpened => RadialLayoutGroup.PieMenuOnShow;

	private Signal PieMenuClosed => RadialLayoutGroup.PieMenuOnHide;

	private bool PieMenuIsOpen => RadialLayoutGroup.PieMenuIsOpen;

	private Signal PieMenuProgressChanged => RadialLayoutGroup.OpenProgressChanged;

	private float PieMenuOpenProgress => RadialLayoutGroup.OpenProcessProgression;

	public void Start()
	{
		GameSignals.LateConnect(UIGameObserver.IsReady, SetUpStates);
		GameSignals.DisableUntil(GameSignals.LevelReady, this);
	}

	public void OnDestroy()
	{
		Cleared.Invoke();
	}

	public CursorState NewState(CursorIcon icon)
	{
		CursorState cursorState = new CursorState(this);
		cursorState.icon = icon;
		cursorStates.Add(cursorState);
		return cursorState;
	}

	public CursorState NewDynamicState(Func<CursorIcon> getter)
	{
		DynamicCursorState dynamicCursorState = new DynamicCursorState(this);
		dynamicCursorState.iconGetter = getter;
		cursorStates.Add(dynamicCursorState);
		return dynamicCursorState;
	}

	private void Update()
	{
		if (UpdateOnNextFrame)
		{
			(from s in cursorStates.Slinq()
				where s.Dirty
				select s).ForEach(delegate(CursorState s)
			{
				s.RunUpdate();
			});
			UpdateOnNextFrame = false;
		}
	}

	public unsafe void SetUpStates()
	{
		NewState(CursorIcon.Look).UseWhen(new UtilityNodes.BoolFunc(this, (nint)__ldftn(CursorController.get_ViewMode))).UpdateOn(ViewModeChanged);
		NewState(CursorIcon.EditMode).UseWhen(new UtilityNodes.BoolFunc(this, (nint)__ldftn(CursorController.get_EditMode))).DontUseWhen(new UtilityNodes.BoolFunc(this, (nint)__ldftn(CursorController.get_ViewMode))).UpdateOn(ViewModeChanged)
			.UpdateOn(PieMenuClosed)
			.UpdateOn(InputModeChanged);
		NewState(CursorIcon.Selection).UseWhen(new UtilityNodes.BoolFunc(this, (nint)__ldftn(CursorController.get_SelectionRectEnabled))).UpdateOn(SelectionRectEnabledChanged);
		NewState(CursorIcon.FilledCircle).UseWhen(PieMenuIs_Open).UpdateOn(PieMenuOpened).UpdateOn(PieMenuClosed);
		NewState(CursorIcon.CursorProgress_1).UseWhen(() => IsRank(PieMenuOpenProgress, 1, 8) && !PieMenuIsOpen).UpdateOn(PieMenuOpened).UpdateOn(PieMenuProgressChanged);
		NewState(CursorIcon.CursorProgress_2).UseWhen(() => IsRank(PieMenuOpenProgress, 2, 8) && !PieMenuIsOpen).UpdateOn(PieMenuOpened).UpdateOn(PieMenuProgressChanged);
		NewState(CursorIcon.CursorProgress_3).UseWhen(() => IsRank(PieMenuOpenProgress, 3, 8) && !PieMenuIsOpen).UpdateOn(PieMenuOpened).UpdateOn(PieMenuProgressChanged);
		NewState(CursorIcon.CursorProgress_4).UseWhen(() => IsRank(PieMenuOpenProgress, 4, 8) && !PieMenuIsOpen).UpdateOn(PieMenuOpened).UpdateOn(PieMenuProgressChanged);
		NewState(CursorIcon.CursorProgress_5).UseWhen(() => IsRank(PieMenuOpenProgress, 5, 8) && !PieMenuIsOpen).UpdateOn(PieMenuOpened).UpdateOn(PieMenuProgressChanged);
		NewState(CursorIcon.CursorProgress_6).UseWhen(() => IsRank(PieMenuOpenProgress, 6, 8) && !PieMenuIsOpen).UpdateOn(PieMenuOpened).UpdateOn(PieMenuProgressChanged);
		NewState(CursorIcon.CursorProgress_7).UseWhen(() => IsRank(PieMenuOpenProgress, 7, 8) && !PieMenuIsOpen).UpdateOn(PieMenuOpened).UpdateOn(PieMenuProgressChanged);
		NewState(CursorIcon.Selectable).UseWhen(() => IsRank(PieMenuOpenProgress, 8, 8) && !PieMenuIsOpen).UpdateOn(PieMenuOpened).UpdateOn(PieMenuProgressChanged);
		NewState(CursorIcon.Idle).UseWhen(NotViewMode_nor_SelectionRect_NotHoveringASelectableObject).DontUseWhen(OpeningPieMenu).DontUseWhen(new UtilityNodes.BoolFunc(this, (nint)__ldftn(CursorController.get_EditMode)))
			.UpdateOn(ViewModeChanged)
			.UpdateOn(SelectionRectEnabledChanged)
			.UpdateOn(ToggleSelectModeChanged)
			.UpdateOn(GameObjectHoverEnter)
			.UpdateOn(GameObjectHoverExit)
			.UpdateOn(GameObjectSelectionEnter)
			.UpdateOn(GameObjectSelectionExit)
			.UpdateOn(EnnemyEnterHover)
			.UpdateOn(EnnemyExitHover)
			.UpdateOn(CharacterEnterHover)
			.UpdateOn(CharacterExitHover)
			.UpdateOn(PieMenuProgressChanged)
			.UpdateOn(AlternateActionChanged);
		NewState(CursorIcon.Minus).DontUseWhen(OpeningPieMenu).DontUseWhen(new UtilityNodes.BoolFunc(this, (nint)__ldftn(CursorController.get_ViewMode))).UseWhen(() => (ToggleSelectMode || InputModeController.GetCurrentMode() == InputMode.storage) && (bool)GameObjectHovered && UIGameSelector.IsSelected(GameObjectHovered.gameObject))
			.UpdateOn(GameObjectHoverEnter)
			.UpdateOn(GameObjectHoverExit)
			.UpdateOn(GameObjectSelectionEnter)
			.UpdateOn(GameObjectSelectionExit)
			.UpdateOn(ToggleSelectModeChanged)
			.UpdateOn(CharacterEnterSelection)
			.UpdateOn(CharacterExitSelection)
			.UpdateOn(BlueprintHoverEnter)
			.UpdateOn(BlueprintHoverExit)
			.UpdateOn(BlueprintEnterSelection)
			.UpdateOn(BlueprintExitSelection)
			.UpdateOn(ViewModeChanged)
			.UpdateOn(PieMenuProgressChanged)
			.UpdateOn(CharacterEnterHover)
			.UpdateOn(CharacterExitHover);
		NewState(CursorIcon.Plus).DontUseWhen(new UtilityNodes.BoolFunc(this, (nint)__ldftn(CursorController.get_ViewMode))).DontUseWhen(OpeningPieMenu).UseWhen(Hovering_Unselected_HoldingToggle_or_SelectingStorage)
			.UpdateOn(GameObjectHoverEnter)
			.UpdateOn(GameObjectHoverExit)
			.UpdateOn(GameObjectSelectionEnter)
			.UpdateOn(GameObjectSelectionExit)
			.UpdateOn(ToggleSelectModeChanged)
			.UpdateOn(CharacterEnterSelection)
			.UpdateOn(CharacterExitSelection)
			.UpdateOn(BlueprintHoverEnter)
			.UpdateOn(BlueprintHoverExit)
			.UpdateOn(BlueprintEnterSelection)
			.UpdateOn(BlueprintExitSelection)
			.UpdateOn(ViewModeChanged)
			.UpdateOn(PieMenuProgressChanged)
			.UpdateOn(CharacterEnterHover)
			.UpdateOn(CharacterExitHover);
		NewState(CursorIcon.Selected).DontUseWhen(new UtilityNodes.BoolFunc(this, (nint)__ldftn(CursorController.get_ViewMode))).DontUseWhen(OpeningPieMenu).UseWhen(HasSelectedCharacter_NotHovering_NonSelectedCharacter_Tree_Plant_Object)
			.UseWhen(() => !ToggleSelectMode && InputModeController.GetCurrentMode() != InputMode.storage && (bool)GameObjectHovered && UIGameSelector.IsSelected(GameObjectHovered.gameObject))
			.UpdateOn(GameObjectHoverEnter)
			.UpdateOn(GameObjectHoverExit)
			.UpdateOn(BlueprintEnterSelection)
			.UpdateOn(BlueprintExitSelection)
			.UpdateOn(ToggleSelectModeChanged)
			.UpdateOn(ViewModeChanged)
			.UpdateOn(PieMenuProgressChanged)
			.UpdateOn(CharacterEnterHover)
			.UpdateOn(CharacterExitHover)
			.UpdateOn(SecondaryActionChanged);
		NewState(CursorIcon.Selectable).DontUseWhen(new UtilityNodes.BoolFunc(this, (nint)__ldftn(CursorController.get_ViewMode))).DontUseWhen(OpeningPieMenu).UseWhen(() => !ToggleSelectMode && InputModeController.GetCurrentMode() != InputMode.storage && (bool)GameObjectHovered && Affiliation.BelongsToMe(GameObjectHovered.gameObject) && UIGameSelector.IsSelectable(GameObjectHovered.gameObject))
			.UpdateOn(BlueprintEnterSelection)
			.UpdateOn(BlueprintExitSelection)
			.UpdateOn(BlueprintHoverEnter)
			.UpdateOn(BlueprintHoverExit)
			.UpdateOn(ViewModeChanged)
			.UpdateOn(PieMenuProgressChanged)
			.UpdateOn(GameObjectHoverEnter)
			.UpdateOn(GameObjectHoverExit)
			.UpdateOn(CharacterEnterHover)
			.UpdateOn(CharacterExitHover);
		NewState(CursorIcon.Down).DontUseWhen(new UtilityNodes.BoolFunc(this, (nint)__ldftn(CursorController.get_ViewMode))).DontUseWhen(OpeningPieMenu).UseWhen(PressingAltButton_NotHoveringObject_WithBricktronSelected_WhoIsFull)
			.UseWhen(HoveringStorage_WithSelectedBricktron_WhoHasStuff_ThatFitsInStorage)
			.UpdateOn(SecondaryActionChanged)
			.UpdateOn(CharacterEnterSelection)
			.UpdateOn(CharacterExitSelection)
			.UpdateOn(CharacterInventoryChanged)
			.UpdateOn(GameObjectHoverEnter)
			.UpdateOn(GameObjectHoverExit)
			.UpdateOn(ViewModeChanged)
			.UpdateOn(PieMenuProgressChanged);
		NewState(CursorIcon.PickAxe).DontUseWhen(new UtilityNodes.BoolFunc(this, (nint)__ldftn(CursorController.get_ViewMode))).DontUseWhen(() => HoveringTerrain_NotBreakeable()).DontUseWhen(OpeningPieMenu)
			.UseWhen(HoveringTerrain_PressingAlt_WithSelectedBricktron_CanBreakVoxel)
			.UseWhen(HoveringNature_WithSelectedBricktron_WhoCanHarvestMinerals)
			.UpdateOn(TerrainHoveredChanged)
			.UpdateOn(CharacterEnterSelection)
			.UpdateOn(CharacterExitSelection)
			.UpdateOn(CharacterInventoryChanged)
			.UpdateOn(SecondaryActionChanged)
			.UpdateOn(ViewModeChanged)
			.UpdateOn(PieMenuProgressChanged);
		SetUpAttackState(Occupation.Type.Knight, CursorIcon.Sword);
		SetUpAttackState(Occupation.Type.Archer, CursorIcon.Bow);
		NewState(CursorIcon.MoveTo).DontUseWhen(new UtilityNodes.BoolFunc(this, (nint)__ldftn(CursorController.get_ViewMode))).DontUseWhen(OpeningPieMenu).UseWhen(HasCharacterSelected_Hovering_AJoinableProject)
			.UpdateOn(CharacterExitSelection)
			.UpdateOn(CharacterEnterSelection)
			.UpdateOn(ProjectEnterHover)
			.UpdateOn(ProjectExitHover)
			.UpdateOn(ProjectHired)
			.UpdateOn(ViewModeChanged)
			.UpdateOn(PieMenuProgressChanged);
		NewState(CursorIcon.Axe).DontUseWhen(new UtilityNodes.BoolFunc(this, (nint)__ldftn(CursorController.get_ViewMode))).DontUseWhen(OpeningPieMenu).UseWhen(HoveringTree_WithSelectedBricktron_AbleToChopTrees)
			.UpdateOn(TreeHovered)
			.UpdateOn(TreeUnhovered)
			.UpdateOn(CharacterEnterSelection)
			.UpdateOn(CharacterExitSelection)
			.UpdateOn(ViewModeChanged)
			.UpdateOn(PieMenuProgressChanged);
		NewState(CursorIcon.Hand).DontUseWhen(new UtilityNodes.BoolFunc(this, (nint)__ldftn(CursorController.get_ViewMode))).DontUseWhen(OpeningPieMenu).UseWhen(HoveringPlant_WithSelectedBricktron_WhoCanCollectPlants)
			.UseWhen(HoveringObject_WithSelectedBricktron_WhoHasRoomForObject)
			.UseWhen(HoveringTool_WithSelectedBricktron_WhoCanEquipTool)
			.UseWhen(HoveringStorage_WithSelectedBricktron_WhenStorageHasStuff_AndBricktronCanTakeSome)
			.UpdateOn(PlantHovered)
			.UpdateOn(PlantUnhovered)
			.UpdateOn(CharacterEnterSelection)
			.UpdateOn(CharacterExitSelection)
			.UpdateOn(CharacterEnterHover)
			.UpdateOn(CharacterExitHover)
			.UpdateOn(GameObjectHoverEnter)
			.UpdateOn(GameObjectHoverExit)
			.UpdateOn(CharacterInventoryChanged)
			.UpdateOn(ViewModeChanged)
			.UpdateOn(PieMenuProgressChanged);
		CursorState cursorState = NewDynamicState(() => FocusedHandle.RecommendedCursor());
		cursorState.UseWhen(() => (bool)FocusedHandle && FocusedHandle.Showing);
		cursorState.UpdateOn(HandleDragged);
		cursorState.UpdateOn(HandleEndDragged);
		cursorState.UpdateOn(HandleHovered);
		cursorState.UpdateOn(HandleEndHovered);
	}

	private unsafe void SetUpAttackState(Occupation.Type type, CursorIcon icon)
	{
		CursorState cursorState = NewState(icon);
		cursorState.DontUseWhen(new UtilityNodes.BoolFunc(this, (nint)__ldftn(CursorController.get_ViewMode)));
		cursorState.DontUseWhen(OpeningPieMenu);
		if (icon == CursorIcon.Sword)
		{
			cursorState.UseWhen(HoveringTerrain_PressingShift_WithSelectedBricktron);
		}
		if (type == Occupation.Type.Knight)
		{
			cursorState.UseWhen(Hovering_SomethingAttackable_by_WhatIsSelected);
		}
		cursorState.UpdateOn(CharacterEnterSelection).UpdateOn(CharacterExitSelection).UpdateOn(EnnemyEnterHover)
			.UpdateOn(EnnemyExitHover)
			.UpdateOn(EnnemyDied)
			.UpdateOn(TerrainHoveredChanged)
			.UpdateOn(CharacterEnterHover)
			.UpdateOn(CharacterExitHover)
			.UpdateOn(GameObjectHoverEnter)
			.UpdateOn(GameObjectHoverExit)
			.UpdateOn(ViewModeChanged)
			.UpdateOn(PieMenuProgressChanged)
			.UpdateOn(AlternateActionChanged);
	}

	private bool Hovering_SomethingAttackable_by_WhatIsSelected()
	{
		return (bool)FirstSelectedCharacter && FirstSelectedCharacter.Profession.HasMeleeAttack() && (bool)GameObjectHovered && Affiliation.BelongsToEnnemy(GameObjectHovered.gameObject) && !GameObjectHovered.GetComponent<DamageReceiver>().IsInvincible;
	}

	private bool OpeningPieMenu()
	{
		return PieMenuOpenProgress > 0.3f;
	}

	private bool IsRank(float value, int rank, int maxRank)
	{
		float num = 0.7f / (float)(maxRank + 1);
		float num2 = 0.3f + num * (float)rank;
		float num3 = 0.3f + num * (float)(rank + 1);
		return value >= num2 && value < num3;
	}

	private bool Hovering_Unselected_HoldingToggle_or_SelectingStorage()
	{
		if (!ToggleSelectMode && InputModeController.GetCurrentMode() != InputMode.storage)
		{
			return false;
		}
		if (!GameObjectHovered)
		{
			return false;
		}
		return !UIGameSelector.IsSelected(GameObjectHovered.gameObject) && UIGameSelector.IsSelectable(GameObjectHovered.gameObject);
	}

	private bool NotViewMode_nor_SelectionRect_NotHoveringASelectableObject()
	{
		if (ViewMode || SelectionRectEnabled)
		{
			return false;
		}
		return !GameObjectHovered || ((bool)GameObjectHovered && !UIGameSelector.IsSelectable(GameObjectHovered.gameObject)) || ((bool)FirstSelectedCharacter && (bool)GameObjectHovered && !ContextualActor.ShouldAttackByDefault(FirstSelectedCharacter, GameObjectHovered.gameObject));
	}

	private bool HasCharacterSelected_Hovering_AJoinableProject()
	{
		return (bool)FirstSelectedCharacter && (bool)ProjectHovered && ProjectHovered.CanHire(FirstSelectedCharacter);
	}

	private bool HasSelectedCharacter_NotHovering_NonSelectedCharacter_Tree_Plant_Object()
	{
		return (bool)FirstSelectedCharacter && (!CharacterHovered || (SelectedCharacters.Contains(CharacterHovered) && !ToggleSelectMode)) && !IsHoveringTree && !IsHoveringPlant && !GameObjectHovered;
	}

	private bool PieMenuIs_Open()
	{
		return PieMenuIsOpen;
	}

	private bool HoveringTerrain_NotBreakeable()
	{
		return !TerrainRules.Main.CanDigAt(TerrainHovered) && !NatureRegistry.NatureAt((int)TerrainHovered.x, (int)TerrainHovered.z, (NatureType)14).IsValid;
	}

	private bool HoveringTerrain_PressingAlt_WithSelectedBricktron_CanBreakVoxel()
	{
		return IsHoveringTerrain && SecondaryAction && (bool)FirstSelectedCharacter && FirstSelectedCharacter.Occupation.Profession.CanBreakVoxels();
	}

	private bool HoveringTerrain_PressingShift_WithSelectedBricktron()
	{
		return IsHoveringTerrain && !GameObjectHovered && AlternateAction && (bool)FirstSelectedCharacter;
	}

	private bool HoveringNature_WithSelectedBricktron_WhoCanHarvestMinerals()
	{
		return NatureRegistry.NatureAt((int)TerrainHovered.x, (int)TerrainHovered.z, (NatureType)14).IsValid && (bool)FirstSelectedCharacter && FirstSelectedCharacter.Occupation.Profession.CanHarvestMinerals();
	}

	private bool HoveringStorage_WithSelectedBricktron_WhenStorageHasStuff_AndBricktronCanTakeSome()
	{
		return (bool)GameObjectHovered && (bool)FirstSelectedCharacter && Recepteur.CharactersCouldGrabFrom(GameObjectHovered.gameObject) && FirstSelectedCharacter.recepteur.HasRoomForSomeOf(GameObjectHovered.gameObject);
	}

	private bool HoveringStorage_WithSelectedBricktron_WhoHasStuff_ThatFitsInStorage()
	{
		return (bool)GameObjectHovered && (bool)FirstSelectedCharacter && !FirstSelectedCharacter.recepteur.IsEmpty() && FirstSelectedCharacter.recepteur.SomeContentFitsIn(GameObjectHovered.gameObject);
	}

	public bool PressingAltButton_NotHoveringObject_WithBricktronSelected_WhoIsFull()
	{
		return SecondaryAction && (bool)FirstSelectedCharacter && !GameObjectHovered && FirstSelectedCharacter.recepteur.IsFull();
	}

	private bool HoveringObject_WithSelectedBricktron_WhoHasRoomForObject()
	{
		return (bool)FirstSelectedCharacter && (bool)GameObjectHovered && FirstSelectedCharacter.recepteur.HasRoomFor(GameObjectHovered.gameObject);
	}

	private bool HoveringTool_WithSelectedBricktron_WhoCanEquipTool()
	{
		return (bool)FirstSelectedCharacter && (bool)GameObjectHovered && (bool)GameObjectHovered.GetComponent<Tool>() && GeneralRules.AllowPartialToolsets && FirstSelectedCharacter.Profession.CanEquip(GameObjectHovered.gameObject);
	}

	private bool HoveringPlant_WithSelectedBricktron_WhoCanCollectPlants()
	{
		return IsHoveringPlant && (bool)FirstSelectedCharacter && FirstSelectedCharacter.Profession.CanCollectPlants();
	}

	private bool HoveringTree_WithSelectedBricktron_AbleToChopTrees()
	{
		return IsHoveringTree && !GameObjectHovered && (bool)FirstSelectedCharacter && FirstSelectedCharacter.Profession.CanChopTrees();
	}

	public void CheckAllExcept(CursorState state)
	{
		for (int i = 0; i < cursorStates.Count && (cursorStates[i] == state || !cursorStates[i].Check()); i++)
		{
		}
	}
}
