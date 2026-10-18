using Brix.Input;
using Brix.Util;
using UnityEngine;

namespace Brix.Game.Components;

internal class WallFitterV2 : AbstractFitter
{
	protected Quaternion _startingRotation = Quaternion.identity;

	protected CompCache<VoxelTransform> _voxelTransform;

	private void Awake()
	{
		_voxelTransform = new CompCache<VoxelTransform>(base.gameObject);
	}

	public override void FitAndOrient(VoxelRaycastHit voxelHit, out Vector3 absolutePosition)
	{
		Vector3 vector = FindFitPosition(voxelHit);
		base.transform.position = _voxelTransform.Get.GetOffsettedPosition(vector);
		SetRotation(vector);
		absolutePosition = vector;
	}

	public override void FitForDrag(VoxelRaycastHit voxelHit, out Vector3 absolutePosition)
	{
		Vector3 vector = FindFitPosition(voxelHit);
		base.transform.position = _voxelTransform.Get.GetOffsettedPosition(vector);
		base.transform.rotation = _startingRotation;
		absolutePosition = vector;
	}

	private void SetRotation(Vector3 originalPos)
	{
		if (_lockRotation)
		{
			return;
		}
		int num = ((!_oneRotationPerAxis) ? 4 : 2);
		Quaternion quaternion = _startingRotation;
		ValidatorComponent component = GetComponent<ValidatorComponent>();
		for (int i = 0; i < num; i++)
		{
			base.transform.rotation = quaternion;
			if (_voxelTransform.Get.CenterOffset != Vector3.zero)
			{
				base.transform.position = _voxelTransform.Get.GetOffsettedPosition(originalPos);
			}
			ValidationResult validationResult = component.Validate();
			if (validationResult == ValidationResult.Valid || validationResult == ValidationResult.Buildable)
			{
				return;
			}
			if (_oneRotationPerAxis)
			{
				quaternion = ((!(quaternion == Quaternion.identity)) ? Quaternion.identity : AbstractFitter._rotation90);
			}
			else
			{
				quaternion *= Quaternion.Euler(0f, 90f, 0f);
			}
		}
		base.transform.rotation = _startingRotation;
		base.transform.position = originalPos;
	}

	public override void ChangeFit()
	{
		if (_lockRotation)
		{
			return;
		}
		if (_oneRotationPerAxis)
		{
			if (_startingRotation == Quaternion.identity)
			{
				_startingRotation = AbstractFitter._rotation90;
			}
			else
			{
				_startingRotation = Quaternion.identity;
			}
		}
		else
		{
			_startingRotation = base.gameObject.transform.rotation;
			_startingRotation *= Quaternion.Euler(0f, 90f, 0f);
		}
	}

	public override void CloneInternals(IFitter original)
	{
		base.CloneInternals(original);
		WallFitterV2 wallFitterV = original as WallFitterV2;
		if (wallFitterV != null && !_lockRotation)
		{
			if (_oneRotationPerAxis)
			{
				_startingRotation = Quaternion.LookRotation(Calcule.AbsVecteur(wallFitterV.transform.rotation * Vector3.forward), wallFitterV.transform.rotation * Vector3.up);
			}
			else
			{
				_startingRotation = wallFitterV.transform.rotation;
			}
		}
	}

	public override IFitter CopyTo(GameObject go)
	{
		Object.Destroy((MonoBehaviour)go.GetInterface<IFitter>());
		WallFitterV2 wallFitterV = go.AddComponent<WallFitterV2>();
		wallFitterV.CloneInternals(this);
		return wallFitterV;
	}
}
