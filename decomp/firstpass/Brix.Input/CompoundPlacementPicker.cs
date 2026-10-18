using Brix.Audio;
using Brix.Engine;
using Brix.Game.Components;
using Brix.Game.Compounders;
using Brix.Lifecycle.Pooling;
using Brix.Transactions;
using UnityEngine;

namespace Brix.Input;

public class CompoundPlacementPicker : PickerComponent
{
	private BlueprintCompoundComponent _compound;

	protected SpawnTransactionMonitor<Blueprint> _lastMonitor;

	public override void OnHoverAny(bool hovering)
	{
		if (_compound != null && _compound.gameObject.activeSelf)
		{
			_compound.MoveAt(GetPlacementPosition(_voxelRayHit));
		}
	}

	private Vector3 GetPlacementPosition(VoxelRaycastHit vrh)
	{
		if (vrh.HitIVoxel.IsCrust && Voxel.GetCubeFantome(vrh.VoxelPosition) == null)
		{
			return vrh.HitVector;
		}
		return vrh.HitVector + vrh.Normal;
	}

	public override void OnFirstActionAny(bool actioning, ClickInfo clickInfo)
	{
		if (clickInfo.actionName == "cam_action1" && clickInfo.clickState == ClickState.down && (_lastMonitor == null || _lastMonitor.State != TransactionState.Running))
		{
			BlueprintCompounder.PlaceCompoundBlueprint(_compound);
			_compound.transform.SetParent(null);
			_compound = null;
			Picking.Instance.ClearPick();
		}
		else if (clickInfo.actionName == "cam_action2")
		{
			Rotate();
		}
	}

	private void Rotate()
	{
		_compound.Rotate();
		SoundEngine.Play("Brick_Turn");
	}

	public override void OnEnterAny(bool hovering)
	{
		_compound.SetActive(b: true);
	}

	public override void OnLeaveAny(bool hovering)
	{
		if (!_voxelRayHit.HasHit)
		{
			_compound.SetActive(b: false);
		}
	}

	public override void SetOptions(IPickerComponentOptions options)
	{
		if (options is CompoundPlacementOptions compoundPlacementOptions)
		{
			if (_compound != null)
			{
				ObjectPoolSingleton.Release(_compound.gameObject);
			}
			_compound = compoundPlacementOptions.Compound;
			_compound.transform.SetParent(base.transform);
			_compound.SetActive(base.Active);
		}
	}

	protected override void OnStandByChange()
	{
		if (_compound != null)
		{
			_compound.gameObject.SetActive(!base.StandBy);
		}
	}

	protected override void OnActiveChange()
	{
		if (!base.Active && !_compound.IsNullOrReleased())
		{
			BlueprintCompounder.CancelCompoundBlueprintPlacement(_compound);
		}
	}
}
