using Brix.External.Factories;
using Brix.Input;
using UnityEngine;

namespace Brix.Game.Components;

internal class LanternWardFitter : DefaultFitter
{
	private int _fitState;

	private Factory.AssetKey[] _keyCycle = new Factory.AssetKey[6]
	{
		Blueprints.LanternWard,
		Blueprints.LanternWard,
		Blueprints.LanternWall,
		Blueprints.LanternWall,
		Blueprints.LanternWall,
		Blueprints.LanternWall
	};

	private static Quaternion[] _rotationCycle = new Quaternion[6]
	{
		Quaternion.identity,
		Quaternion.Euler(0f, 90f, 0f),
		Quaternion.identity,
		Quaternion.Euler(0f, 90f, 0f),
		Quaternion.Euler(0f, 180f, 0f),
		Quaternion.Euler(0f, 270f, 0f)
	};

	public override void FitAndOrient(VoxelRaycastHit voxelHit, out Vector3 absolutePosition)
	{
		base.FitAndOrient(voxelHit, out absolutePosition);
		AutoFit();
	}

	public override void ChangeFit()
	{
		if (_fitState >= 5)
		{
			_fitState = 0;
		}
		else
		{
			_fitState++;
		}
		SetAs(_keyCycle[_fitState]);
		_rotation = _rotationCycle[_fitState];
		if (_keyCycle[_fitState] == Blueprints.LanternWard)
		{
			_baseOnTopOfGround = true;
		}
		else
		{
			_baseOnTopOfGround = false;
		}
	}

	private void SetAs(Factory.AssetKey key)
	{
		if (GetComponent<Picker>() == null && !(key == null))
		{
			GameObject gameObject = Factory.Peek(key);
			GetComponent<VoxelTransform>().InitialCenterOffset = gameObject.GetComponent<VoxelTransform>().InitialCenterOffset;
			GetComponent<CapsuleCollider>().radius = gameObject.GetComponent<CapsuleCollider>().radius;
			GetComponent<CapsuleCollider>().height = gameObject.GetComponent<CapsuleCollider>().height;
			GetComponent<FactoryImprint>().AssetKey = key;
			ValidatorComponent component = GetComponent<ValidatorComponent>();
			component.validatorType = gameObject.GetComponent<ValidatorComponent>().validatorType;
			component.ResetValidatorCache();
			MeshFilter[] componentsInChildren = gameObject.GetComponentsInChildren<MeshFilter>();
			MeshFilter[] componentsInChildren2 = GetComponentsInChildren<MeshFilter>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren2[i].sharedMesh = componentsInChildren[i].sharedMesh;
			}
		}
	}

	private void AutoFit()
	{
		int num = 6;
		ValidatorComponent component = GetComponent<ValidatorComponent>();
		for (int i = 0; i < num; i++)
		{
			ValidationResult validationResult = component.Validate();
			if (validationResult == ValidationResult.Valid || validationResult == ValidationResult.Buildable)
			{
				break;
			}
			ChangeFit();
			base.gameObject.transform.rotation = _rotation;
		}
	}

	public override void CloneInternals(IFitter original)
	{
		base.CloneInternals(original);
		LanternWardFitter lanternWardFitter = original as LanternWardFitter;
		if (lanternWardFitter != null)
		{
			_fitState = lanternWardFitter._fitState;
			SetAs(_keyCycle[_fitState]);
		}
	}

	public override IFitter CopyTo(GameObject go)
	{
		Object.Destroy(go.GetInterface<LanternWardFitter>());
		LanternWardFitter lanternWardFitter = go.AddComponent<LanternWardFitter>();
		lanternWardFitter.CloneInternals(this);
		return lanternWardFitter;
	}
}
