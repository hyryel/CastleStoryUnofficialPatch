using System;
using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.AI;
using Brix.Game.Components;
using UnityEngine;

namespace Brix.Input;

public class PickerComponent : MonoBehaviour
{
	private Picker _parent;

	protected VoxelRaycastHit _voxelRayHit;

	protected VoxelRaycastHit _utilityVRH = new VoxelRaycastHit();

	private Signal _activated = new Signal();

	private Signal _deactivated = new Signal();

	private bool _active;

	private bool _standBy;

	public Action<bool> OnEnterAnyAction;

	public Action<bool> OnHoverAnyAction;

	public Action<bool> OnLeaveAnyAction;

	public Action<bool, ClickInfo> OnFirstActionAnyAction;

	public Action<bool, ClickInfo> OnActionAnyAction;

	public Action<bool, ClickInfo> OnLastActionAnyAction;

	public Action<Vector3?> OnEnterTerrainAction;

	public Action<Vector3?> OnHoverTerrainAction;

	public Action<Vector3?> OnLeaveTerrainAction;

	public Action<Vector3?, ClickInfo> OnFirstActionTerrainAction;

	public Action<Vector3?, ClickInfo> OnActionTerrainAction;

	public Action<Vector3?, ClickInfo> OnLastActionTerrainAction;

	public Action<Blueprint> OnEnterBlueprintAction;

	public Action<Blueprint> OnHoverBlueprintAction;

	public Action<Blueprint> OnLeaveBlueprintAction;

	public Action<Blueprint, ClickInfo> OnFirstActionBlueprintAction;

	public Action<Blueprint, ClickInfo> OnActionBlueprintAction;

	public Action<Blueprint, ClickInfo> OnLastActionBlueprintAction;

	public Action<Labor> OnEnterCharacterAction;

	public Action<Labor> OnHoverCharacterAction;

	public Action<Labor> OnLeaveCharacterAction;

	public Action<Labor, ClickInfo> OnFirstActionCharacterAction;

	public Action<Labor, ClickInfo> OnActionCharacterAction;

	public Action<Labor, ClickInfo> OnLastActionCharacterAction;

	public Action<GameObject> OnEnterGameObjectAction;

	public Action<GameObject> OnHoverGameObjectAction;

	public Action<GameObject> OnLeaveGameObjectAction;

	public Action<GameObject, ClickInfo> OnFirstActionGameObjectAction;

	public Action<GameObject, ClickInfo> OnActionGameObjectAction;

	public Action<GameObject, ClickInfo> OnLastActionGameObjectAction;

	protected static readonly PickerOrderGiver OrderGiver = new PickerOrderGiver();

	public Picker ParentPicker => _parent;

	public IReadOnlySignal Activated => _activated;

	public IReadOnlySignal Deactivated => _deactivated;

	public bool IsEditor => Architecte.editorMode;

	public bool Active
	{
		get
		{
			return _active;
		}
		set
		{
			if (_active != value)
			{
				_active = value;
				OnActiveChange();
				if (_active)
				{
					_activated.Invoke();
					Picking.Instance.PickingObserver.PickerCompActivated.Invoke(this);
				}
				else
				{
					_deactivated.Invoke();
					Picking.Instance.PickingObserver.PickerCompDeactivated.Invoke(this);
				}
			}
		}
	}

	public bool StandBy
	{
		get
		{
			return _standBy;
		}
		set
		{
			if (_standBy != value)
			{
				_standBy = value;
				OnStandByChange();
			}
		}
	}

	public void SetPickingParent(Picker parent)
	{
		_parent = parent;
	}

	public void SetVoxelRaycastHit(VoxelRaycastHit voxRayHit)
	{
		_voxelRayHit = voxRayHit;
	}

	private void Awake()
	{
		OnEnterAnyAction = OnEnterAny;
		OnHoverAnyAction = OnHoverAny;
		OnLeaveAnyAction = OnLeaveAny;
		OnFirstActionAnyAction = OnFirstActionAny;
		OnActionAnyAction = OnActionAny;
		OnLastActionAnyAction = OnLastActionAny;
		OnEnterTerrainAction = OnEnterTerrain;
		OnHoverTerrainAction = OnHoverTerrain;
		OnLeaveTerrainAction = OnLeaveTerrain;
		OnFirstActionTerrainAction = OnFirstActionTerrain;
		OnActionTerrainAction = OnActionTerrain;
		OnLastActionTerrainAction = OnLastActionTerrain;
		OnEnterBlueprintAction = OnEnterBlueprint;
		OnHoverBlueprintAction = OnHoverBlueprint;
		OnLeaveBlueprintAction = OnLeaveBlueprint;
		OnFirstActionBlueprintAction = OnFirstActionBlueprint;
		OnActionBlueprintAction = OnActionBlueprint;
		OnLastActionBlueprintAction = OnLastActionBlueprint;
		OnEnterCharacterAction = OnEnterCharacter;
		OnHoverCharacterAction = OnHoverCharacter;
		OnLeaveCharacterAction = OnLeaveCharacter;
		OnFirstActionCharacterAction = OnFirstActionCharacter;
		OnActionCharacterAction = OnActionCharacter;
		OnLastActionCharacterAction = OnLastActionCharacter;
		OnEnterGameObjectAction = OnEnterGameObject;
		OnHoverGameObjectAction = OnHoverGameObject;
		OnLeaveGameObjectAction = OnLeaveGameObject;
		OnFirstActionGameObjectAction = OnFirstActionGameObject;
		OnActionGameObjectAction = OnActionGameObject;
		OnLastActionGameObjectAction = OnLastActionGameObject;
	}

	protected virtual void OnActiveChange()
	{
	}

	protected virtual void OnStandByChange()
	{
	}

	public virtual object GetPieMenuTarget()
	{
		return null;
	}

	public virtual void HoldKey(Factory.AssetKey key)
	{
	}

	public virtual void SetOptions(IPickerComponentOptions options)
	{
	}

	public virtual void OnEnterAny(bool hovering)
	{
	}

	public virtual void OnHoverAny(bool hovering)
	{
	}

	public virtual void OnLeaveAny(bool hovering)
	{
	}

	public virtual void OnFirstActionAny(bool actioning, ClickInfo clickInfo)
	{
	}

	public virtual void OnActionAny(bool actioning, ClickInfo clickInfo)
	{
	}

	public virtual void OnLastActionAny(bool actioning, ClickInfo clickInfo)
	{
	}

	public virtual void OnEnterTerrain(Vector3? hoveredVoxel)
	{
	}

	public virtual void OnHoverTerrain(Vector3? hoveredVoxel)
	{
	}

	public virtual void OnLeaveTerrain(Vector3? hoveredVoxel)
	{
	}

	public virtual void OnFirstActionTerrain(Vector3? clickedVoxel, ClickInfo clickInfo)
	{
	}

	public virtual void OnActionTerrain(Vector3? clickedVoxel, ClickInfo clickInfo)
	{
	}

	public virtual void OnLastActionTerrain(Vector3? clickedVoxel, ClickInfo clickInfo)
	{
	}

	public virtual void OnEnterBlueprint(Blueprint hoveredBlueprint)
	{
	}

	public virtual void OnHoverBlueprint(Blueprint hoveredBlueprint)
	{
	}

	public virtual void OnLeaveBlueprint(Blueprint hoveredBlueprint)
	{
	}

	public virtual void OnFirstActionBlueprint(Blueprint clickedBlueprint, ClickInfo clickInfo)
	{
	}

	public virtual void OnActionBlueprint(Blueprint clickedBlueprint, ClickInfo clickInfo)
	{
	}

	public virtual void OnLastActionBlueprint(Blueprint clickedBlueprint, ClickInfo clickInfo)
	{
	}

	public virtual void OnEnterCharacter(Labor hoveredCharacter)
	{
	}

	public virtual void OnHoverCharacter(Labor hoveredCharacter)
	{
	}

	public virtual void OnLeaveCharacter(Labor hoveredCharacter)
	{
	}

	public virtual void OnFirstActionCharacter(Labor clickedCharacter, ClickInfo clickInfo)
	{
	}

	public virtual void OnActionCharacter(Labor clickedCharacter, ClickInfo clickInfo)
	{
	}

	public virtual void OnLastActionCharacter(Labor clickedCharacter, ClickInfo clickInfo)
	{
	}

	public virtual void OnEnterGameObject(GameObject hoveredGameObject)
	{
	}

	public virtual void OnHoverGameObject(GameObject hoveredGameObject)
	{
	}

	public virtual void OnLeaveGameObject(GameObject hoveredGameObject)
	{
	}

	public virtual void OnFirstActionGameObject(GameObject clickedGameObject, ClickInfo clickInfo)
	{
	}

	public virtual void OnActionGameObject(GameObject clickedGameObject, ClickInfo clickInfo)
	{
	}

	public virtual void OnLastActionGameObject(GameObject clickedGameObject, ClickInfo clickInfo)
	{
	}

	protected void DirectOrder(List<GameObject> listGO, Vector3 target)
	{
		OrderGiver.DirectOrder(listGO, target);
	}

	protected void DirectOrder(List<GameObject> listGO, GameObject target)
	{
		OrderGiver.DirectOrder(listGO, target);
	}
}
