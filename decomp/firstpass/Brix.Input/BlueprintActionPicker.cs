using Brix.Engine;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Brix.UI.GameSelector;
using UnityEngine;

namespace Brix.Input;

internal class BlueprintActionPicker : PickerComponent
{
	public override void OnFirstActionBlueprint(Blueprint clickedBlueprint, ClickInfo clickInfo)
	{
		if (clickInfo.actionName == "cam_action1" && !PickingUtility.isInViewMode() && clickedBlueprint.Project.selection)
		{
			if (PickingUtility.isInToggleSelectMode())
			{
				UIGameSelector.ToggleSelection(clickedBlueprint.gameObject);
			}
			else if (clickInfo.clickType == ClickType.doubleClick)
			{
				UIGameSelectorUtilities.SelectAllConnectedVolumes(SelectionType.Blueprint, VProperty.fantome);
			}
			else
			{
				UIGameSelector.Select(clickedBlueprint.gameObject);
			}
			GameSignals.Invoke(PickingObserver.lua_BlueprintSelectionActionTaken);
		}
	}

	public override void OnFirstActionTerrain(Vector3? clickedVoxel, ClickInfo clickInfo)
	{
		if (clickInfo.actionName == "cam_action1" && !PickingUtility.isInViewMode() && Voxel.GetBlockData(XYZ.FromVector3(clickedVoxel.Value)) is BlockData blockData)
		{
			if (PickingUtility.isInToggleSelectMode())
			{
				UIGameSelector.ToggleSelection(blockData.PlacedBlock);
			}
			else if (PickingUtility.isInBatchSelectMode(clickInfo.clickType))
			{
				UIGameSelectorUtilities.SelectAllConnectedVolumes(SelectionType.Blocks, VProperty.occupy);
			}
			else
			{
				UIGameSelector.Select(blockData.PlacedBlock);
			}
			GameSignals.Invoke(PickingObserver.lua_BlueprintSelectionActionTaken);
		}
	}

	public override void OnEnterTerrain(Vector3? hoveredVoxel)
	{
		base.OnEnterTerrain(hoveredVoxel);
		if (!PickingUtility.Player.GetButton("cam_action1") && Voxel.GetBlockData(XYZ.FromVector3(hoveredVoxel.Value)) is BlockData blockData)
		{
			UIGameSelector.Hover(blockData.PlacedBlock);
		}
	}

	public override void OnLeaveTerrain(Vector3? hoveredVoxel)
	{
		base.OnEnterTerrain(hoveredVoxel);
		if (!PickingUtility.Player.GetButton("cam_action1") && Voxel.GetBlockData(XYZ.FromVector3(hoveredVoxel.Value)) is BlockData blockData)
		{
			UIGameSelector.Unhover(blockData.PlacedBlock);
		}
	}

	public override void OnEnterBlueprint(Blueprint hoveredBlueprint)
	{
		base.OnEnterBlueprint(hoveredBlueprint);
		if (!PickingUtility.Player.GetButton("cam_action1"))
		{
			UIGameSelector.Hover(hoveredBlueprint.gameObject);
		}
	}

	public override void OnLeaveBlueprint(Blueprint hoveredBlueprint)
	{
		base.OnLeaveBlueprint(hoveredBlueprint);
		UIGameSelector.Unhover(hoveredBlueprint.gameObject);
	}
}
