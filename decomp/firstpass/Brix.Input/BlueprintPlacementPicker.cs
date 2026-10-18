using Brix.Audio;
using Brix.External.Factories;
using Brix.Game;
using Brix.Game.Components;
using Brix.Game.Components.Wards;
using Brix.Game.Network;
using Brix.UI.GameSelector;
using UnityEngine;

namespace Brix.Input;

public class BlueprintPlacementPicker : ObjectPlacementPicker
{
	public class BlueprintPlacementOptions : IPickerComponentOptions
	{
		public bool instant;

		public BlueprintPlacementOptions(bool instant)
		{
			this.instant = instant;
		}
	}

	private bool _instant;

	private bool IsOnUnstackable;

	private WardLink heldWardLink;

	public override void SetOptions(IPickerComponentOptions options)
	{
		if (options is BlueprintPlacementOptions)
		{
			BlueprintPlacementOptions blueprintPlacementOptions = (BlueprintPlacementOptions)options;
			_instant = blueprintPlacementOptions.instant;
		}
		else if (options is PickerComponentOptionsReset)
		{
			_instant = false;
		}
	}

	public override void OnEnterBlueprint(Blueprint hoveredBlueprint)
	{
		if (!hoveredBlueprint.Stackable && !_holdingClick)
		{
			_heldObject.DeactivateHeldObject();
			UIGameSelector.Hover(hoveredBlueprint.gameObject);
			IsOnUnstackable = true;
		}
	}

	public override void OnLeaveBlueprint(Blueprint hoveredBlueprint)
	{
		_heldObject.ActivateHeldObject();
		IsOnUnstackable = false;
		if (!hoveredBlueprint.IsNullOrReleased())
		{
			UIGameSelector.Unhover(hoveredBlueprint.gameObject);
		}
	}

	public override void OnHoverAny(bool hovering)
	{
		if (_heldObject == null)
		{
			if (_masterKey == null)
			{
				_masterKey = Blueprints.Stone_DoubleBrick;
			}
			HoldKey(_masterKey);
		}
		if (IsOnUnstackable)
		{
			return;
		}
		if (!_holdingClick)
		{
			if (_heldDragger != null)
			{
				_heldDragger.ClearSlaves();
			}
			_heldObject.GetHeldComponent<IFitter>().FitAndOrient(_voxelRayHit);
			_heldObject.GetHeldComponent<ValidatorComponent>().Validate();
		}
		if (_heldObject.HeldObject.activeSelf && _holdingClick && _heldDragger != null)
		{
			_heldDragger.GenerateSlaves(_voxelRayHit);
		}
		if ((bool)heldWardLink)
		{
			heldWardLink.PreviewConnections();
		}
	}

	public override void HoldKey(Factory.AssetKey key)
	{
		base.HoldKey(key);
		heldWardLink = _heldObject.GetHeldComponent<WardLink>();
		if ((bool)heldWardLink)
		{
			WardLink.ShowLinks();
		}
		else
		{
			WardLink.HideLinks();
		}
	}

	protected override void OnActiveChange()
	{
		base.OnActiveChange();
		IsOnUnstackable = false;
		UIGameSelector.UnselectAllInTrack(SelectionTrackID.Blueprints);
		if ((bool)heldWardLink && heldWardLink.isActiveAndEnabled)
		{
			WardLink.ShowLinks();
		}
		else
		{
			WardLink.HideLinks();
		}
	}

	public override void OnLeaveAny(bool hovering)
	{
		base.OnLeaveAny(hovering);
		if ((bool)heldWardLink)
		{
			heldWardLink.StopConnectionsPreview();
		}
	}

	protected override void PlaceHeldObjects()
	{
		if (IsOnUnstackable)
		{
			return;
		}
		WardLink heldComponent = _heldObject.GetHeldComponent<WardLink>();
		if ((bool)heldComponent)
		{
			heldComponent.LinkLineRenderer.SetPositions(new Vector3[2]
			{
				Vector3.zero,
				Vector3.zero
			});
		}
		PlaceBlueprint(_heldObject);
		if (_heldDragger == null)
		{
			return;
		}
		foreach (Slave slafe in _heldDragger.GetSlaves())
		{
			PlaceBlueprint(slafe);
		}
		_heldDragger.ClearSlaves();
	}

	private void PlaceBlueprint(Slave s)
	{
		Blueprint heldComponent = s.GetHeldComponent<Blueprint>();
		ValidatorComponent heldComponent2 = s.GetHeldComponent<ValidatorComponent>();
		if (heldComponent.AssetKey == Blueprints.RopeBridgePlacement)
		{
			RopeBridgeConfigurator component = heldComponent.GetComponent<RopeBridgeConfigurator>();
			if (component != null && !component.IsPlacedAtInvalidPosition && (heldComponent.State & PlacementFlags.valid) != PlacementFlags.none)
			{
				User.LocalUser.GetComponent<UNetBlueprint>().CallCmdSpawnPlacedRopeBridge(heldComponent.voxel.GetOffsettedPosition(component.StartPoint), heldComponent.voxel.GetOffsettedPosition(component.EndPoint), _instant || base.IsEditor, (!(UIGameObserver.projects.CurrentSelected == null)) ? UIGameObserver.projects.CurrentSelected.gameObject : null, heldComponent.GetComponent<VoxelTransform>().PlacementOffset, spawnSelected: false);
				SoundEngine.Play("Brick_Place");
			}
		}
		else
		{
			ValidationResult currentState = heldComponent2.GetCurrentState();
			if (currentState != ValidationResult.Obstructed && (heldComponent2.CanPlaceInvalid || currentState != ValidationResult.Invalid))
			{
				User.LocalUser.GetComponent<UNetBlueprint>().CallCmdSpawnPlacedBlueprint(heldComponent.AssetKey.Factory, heldComponent.AssetKey.Name, heldComponent.transform.position, heldComponent.transform.rotation, _instant || base.IsEditor, (!(UIGameObserver.projects.CurrentSelected == null)) ? UIGameObserver.projects.CurrentSelected.gameObject : null, spawnSelected: false);
				SoundEngine.Play("Brick_Place");
			}
		}
	}
}
