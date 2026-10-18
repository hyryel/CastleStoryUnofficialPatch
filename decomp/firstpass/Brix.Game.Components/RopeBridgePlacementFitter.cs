using Brix.Engine;
using Brix.Input;
using UnityEngine;

namespace Brix.Game.Components;

public class RopeBridgePlacementFitter : DefaultFitter
{
	[SerializeField]
	protected int _skip;

	public RopeBridgeConfigurator RopeBridgeConfigurator;

	private XYZ _exPosition;

	private int _exRotation;

	private void Update()
	{
		if (UnityEngine.Input.GetKeyDown(KeyCode.KeypadPlus))
		{
			_skip++;
			if (!TrySetFit(_skip))
			{
				_skip = 0;
				TrySetFit(_skip);
			}
		}
		UpdateBlueprintState();
	}

	public override void FitAndOrient(VoxelRaycastHit voxelHit, out Vector3 absolutePosition)
	{
		base.FitAndOrient(voxelHit, out absolutePosition);
		if (_exPosition != _voxelTransform.Get.position || _exRotation != _voxelTransform.Get.RotationIndex)
		{
			_exPosition = _voxelTransform.Get.position;
			_exRotation = _voxelTransform.Get.RotationIndex;
			_skip = 0;
			TrySetFit(_skip);
		}
	}

	public override void ChangeFit()
	{
		base.ChangeFit();
		base.transform.rotation = _rotation;
		_skip = 0;
	}

	private bool TrySetFit(int skip)
	{
		XYZ position = _voxelTransform.Get.position;
		XYZ dir = XYZ.FromVector3(_rotation * Vector3.forward);
		bool success;
		XYZ end = RopeBridgeConfigurator.ComputeEndPoint(position, dir, skip, out success);
		success = success && GetComponent<ValidatorComponent>().Validate() >= ValidationResult.Valid;
		RopeBridgeConfigurator.IsPlacedAtInvalidPosition = !success;
		RopeBridgeConfigurator.SetEndPoints(position, end);
		RopeBridgeConfigurator.InitLineRenderers();
		return success;
	}

	private void UpdateBlueprintState()
	{
		if (RopeBridgeConfigurator.IsPlacedAtInvalidPosition)
		{
			RopeBridgeConfigurator.GetComponent<Blueprint>().State &= ~PlacementFlags.valid;
		}
		else
		{
			RopeBridgeConfigurator.GetComponent<Blueprint>().State |= PlacementFlags.valid;
		}
	}
}
