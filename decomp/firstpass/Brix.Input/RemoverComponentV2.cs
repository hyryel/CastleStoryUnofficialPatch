using System;
using Brix.Audio;
using Brix.Engine;
using Brix.Engine.Nature;
using Brix.External.Factories;
using Brix.Game;
using Brix.Game.Components;
using Brix.Game.Factories;
using Brix.Game.Network;
using Brix.Game.Rules;
using UnityEngine;

namespace Brix.Input;

public class RemoverComponentV2 : GrabberComponentV2
{
	protected Blueprint hoveredMemory;

	protected XYZ _lastTerrain;

	protected bool refresh;

	private Blueprint CurrentAnti => (_heldObject != null) ? _heldObject.GetHeldComponent<Blueprint>() : null;

	public override void OnEnterGameObject(GameObject hoveredGameObject)
	{
		FactoryImprint componentInParent = hoveredGameObject.GetComponentInParent<FactoryImprint>();
		if (!(componentInParent != null))
		{
			return;
		}
		Factory.AssetKey assetKey = BlueprintAssetKeyResolver.Resolve(componentInParent.AssetKey);
		if (assetKey.IsNullOrInvalid())
		{
			return;
		}
		if (CurrentAnti == null || CurrentAnti.GetComponent<FactoryImprint>().AssetKey != assetKey)
		{
			HoldKey(assetKey, componentInParent.transform.position, componentInParent.transform.rotation);
		}
		else
		{
			_heldObject.HeldObject.transform.position = componentInParent.transform.position;
			_heldObject.HeldObject.transform.rotation = componentInParent.transform.rotation;
		}
		ActivateHeldObjects();
		SetDiggingFlags();
		if ((bool)_heldObject.GetHeldComponent<RopeBridgeConfigurator>())
		{
			RopeBridgeConfigurator heldComponent = _heldObject.GetHeldComponent<RopeBridgeConfigurator>();
			RopeBridgeConfigurator componentInParent2 = hoveredGameObject.GetComponentInParent<RopeBridgeConfigurator>();
			if (componentInParent2 != null)
			{
				heldComponent.transform.position = componentInParent2.transform.position;
				heldComponent.transform.rotation = componentInParent2.transform.rotation;
				heldComponent.SetEndPoints(componentInParent2.StartPoint, componentInParent2.EndPoint);
				heldComponent.CreateVolume(isBlueprint: true);
				heldComponent.InitRopes();
				heldComponent.IsPlacedAtInvalidPosition = false;
			}
			else
			{
				heldComponent.IsPlacedAtInvalidPosition = true;
			}
		}
	}

	public override void OnFirstActionGameObject(GameObject clickedGameObject, ClickInfo clickInfo)
	{
		if (clickInfo.actionName == "cam_action1" && _heldObject != null)
		{
			PlaceAnti();
		}
	}

	public override void OnEnterBlueprint(Blueprint hoveredBlueprint)
	{
		DeactivateHeldObjects();
		hoveredBlueprint.State |= PlacementFlags.mayRemove;
		hoveredMemory = hoveredBlueprint;
	}

	public override void OnHoverBlueprint(Blueprint hoveredBlueprint)
	{
		DeactivateHeldObjects();
		if (hoveredMemory == null)
		{
			OnEnterBlueprint(hoveredBlueprint);
		}
	}

	public override void OnLeaveBlueprint(Blueprint hoveredBlueprint)
	{
		if (hoveredMemory != null)
		{
			hoveredBlueprint.State &= ~PlacementFlags.mayRemove;
			hoveredMemory = null;
		}
	}

	public override void OnFirstActionBlueprint(Blueprint clickedBlueprint, ClickInfo clickInfo)
	{
		if (clickInfo.actionName == "cam_action1")
		{
			hoveredMemory = null;
			SoundEngine.Play("Brick_Cancel");
			User.LocalUser.GetComponent<UNetBlueprint>().CallCmdDestroyBluePrint(clickedBlueprint.gameObject);
		}
	}

	public override void OnEnterTerrain(Vector3? hoveredVoxel)
	{
		ActivateHeldObjects();
	}

	public override void OnHoverTerrain(Vector3? hoveredVoxel)
	{
		IVoxel rootOrSelf = Voxel.GetVoxel(_voxelRayHit.VoxelNormal).RootOrSelf;
		if (_voxelRayHit.Normal != Vector3.up || !rootOrSelf.IsCrust)
		{
			rootOrSelf = _voxelRayHit.HitIVoxel.RootOrSelf;
		}
		if (rootOrSelf.IsNull)
		{
			return;
		}
		if (rootOrSelf.Position != _lastTerrain || refresh)
		{
			BlockInfo blockInfo = rootOrSelf.BlockInfo;
			_lastTerrain = rootOrSelf.Position;
			refresh = false;
			Factory.AssetKey assetKey = blockInfo.BlueprintKey;
			if (assetKey == Blueprints.RopeBridgePlank)
			{
				DeactivateHeldObjects();
				return;
			}
			if (assetKey.IsNullOrInvalid())
			{
				assetKey = Blueprints.Stone_Brick;
			}
			if (_heldObject != null && _heldObject.GetHeldComponent<FactoryImprint>().AssetKey == assetKey)
			{
				_heldObject.HeldObject.transform.position = rootOrSelf.Position.ToVector3();
				_heldObject.HeldObject.transform.rotation = rootOrSelf.Rotation;
			}
			else
			{
				HoldKey(assetKey, rootOrSelf.Position.ToVector3(), rootOrSelf.Rotation);
			}
			BaseVoxelTransform heldComponent = _heldObject.GetHeldComponent<BaseVoxelTransform>();
			if (heldComponent != null)
			{
				_heldObject.HeldObject.transform.Translate(-heldComponent.CenterOffset, Space.Self);
			}
		}
		ActivateHeldObjects();
	}

	public override void OnFirstActionTerrain(Vector3? clickedVoxel, ClickInfo clickInfo)
	{
		if (clickInfo.actionName == "cam_action1" && _heldObject != null)
		{
			PlaceAnti();
		}
	}

	public override void OnHoverAny(bool hovering)
	{
	}

	public override void OnFirstActionAny(bool actioning, ClickInfo clickInfo)
	{
	}

	public override void OnLastActionAny(bool actioning, ClickInfo clickInfo)
	{
	}

	public void PlaceAnti()
	{
		if (InputModeController.DefaultMode != InputMode.worldEditor && !NatureRegistry.HasNatureBlockerAt(_voxelRayHit.VoxelPosition))
		{
			GameObject heldObject = _heldObject.HeldObject;
			Blueprint component = heldObject.GetComponent<Blueprint>();
			if (component.GetComponent<RopeBridgeConfigurator>() != null && !component.GetComponent<RopeBridgeConfigurator>().IsPlacedAtInvalidPosition)
			{
				component.State |= PlacementFlags.valid;
			}
			if (!(component.AssetKey == Blueprints.RopeBridgePlank) && component != null && component.State.HasFlag(PlacementFlags.valid))
			{
				Factory.AssetKey assetKey = component.GetInterface<AssetKeyHolder>().AssetKey;
				GameObject buildProject = ((!UIGameObserver.projects.CurrentSelected) ? null : UIGameObserver.projects.CurrentSelected.gameObject);
				bool instant = base.IsEditor || InputModeController.DefaultMode == InputMode.worldEditor;
				User.LocalUser.GetComponent<UNetBlueprint>().CallCmdPlaceAnti(assetKey.Factory, assetKey.Name, component.transform.position, component.transform.rotation, instant, buildProject);
				_lastTerrain = XYZ.zero;
				_heldObject.DestroyHeldObject();
				_heldObject = null;
			}
		}
	}

	public override void HoldKey(Factory.AssetKey key, Vector3 position, Quaternion rotation)
	{
		base.HoldKey(key, position, rotation);
		_heldObject.HeldObject.transform.localScale *= 1.01f;
		Blueprint currentAnti = CurrentAnti;
		currentAnti.State |= PlacementFlags.valid | PlacementFlags.mayRemove;
		UnityEngine.Object.Destroy(currentAnti.GetComponent(typeof(IFitter)));
		UnityEngine.Object.Destroy(currentAnti.GetComponent(typeof(IDragger)));
		_heldDragger = null;
	}

	private void SetDiggingFlags()
	{
		Blueprint currentAnti = CurrentAnti;
		if (CanDig(_heldObject.GetHeldComponent<VoxelTransform>().position))
		{
			currentAnti.State |= PlacementFlags.valid | PlacementFlags.mayRemove;
			return;
		}
		currentAnti.State &= ~PlacementFlags.mayRemove;
		currentAnti.State &= ~PlacementFlags.valid;
	}

	public override void DestroyHeldObjects()
	{
		base.DestroyHeldObjects();
		_lastTerrain = XYZ.zero;
	}

	public bool CanDig(XYZ position)
	{
		if (Voxel.IsEmpty(position))
		{
			return true;
		}
		return TerrainRules.Main.CanDigAt(position.ToVector3());
	}

	protected override void OnStandByChange()
	{
		Reset();
	}

	protected override void OnActiveChange()
	{
		Reset();
	}

	private void Reset()
	{
		if (!base.Active || base.StandBy)
		{
			OnLeaveBlueprint(hoveredMemory);
			OnLeaveAny(hovering: false);
			_lastTerrain = XYZ.zero;
		}
	}

	private void CycleBlockData()
	{
		throw new NotImplementedException();
	}
}
