using Brix.External.Factories;
using Brix.Input;
using UnityEngine;

namespace Brix.Game.Components;

internal class RampJunctionFitter : PlankFitter
{
	private int _fitState;

	private static Factory.AssetKey[] _fitCycle = new Factory.AssetKey[12]
	{
		Blueprints.Wood_Ramp_JunctionA,
		Blueprints.Wood_Ramp_JunctionA,
		Blueprints.Wood_Ramp_JunctionB,
		Blueprints.Wood_Ramp_JunctionB,
		Blueprints.Wood_Ramp_JunctionC,
		Blueprints.Wood_Ramp_JunctionC,
		Blueprints.Wood_Ramp_JunctionD,
		Blueprints.Wood_Ramp_JunctionD,
		Blueprints.Wood_Ramp_JunctionE,
		Blueprints.Wood_Ramp_JunctionE,
		Blueprints.Wood_Ramp_JunctionF,
		Blueprints.Wood_Ramp_JunctionF
	};

	private static Quaternion[] _rotationCycle = new Quaternion[12]
	{
		Quaternion.identity,
		Quaternion.Euler(0f, 90f, 0f),
		Quaternion.identity,
		Quaternion.Euler(0f, 90f, 0f),
		Quaternion.identity,
		Quaternion.Euler(0f, 90f, 0f),
		Quaternion.identity,
		Quaternion.Euler(0f, 90f, 0f),
		Quaternion.identity,
		Quaternion.Euler(0f, 90f, 0f),
		Quaternion.identity,
		Quaternion.Euler(0f, 90f, 0f)
	};

	public override void ChangeFit()
	{
		if (_fitState >= 11)
		{
			_fitState = 0;
		}
		else
		{
			_fitState++;
		}
		SetAs(_fitCycle[_fitState]);
		_rotation = _rotationCycle[_fitState];
	}

	private void SetAs(Factory.AssetKey key)
	{
		if (GetComponent<Picker>() == null && !(key == null))
		{
			GameObject gameObject = Factory.Peek(key);
			GetComponent<VoxelTransform>().InitialCenterOffset = gameObject.GetComponent<VoxelTransform>().InitialCenterOffset;
			GetComponent<MeshFilter>().sharedMesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
			GetComponent<BoxCollider>().size = gameObject.GetComponent<BoxCollider>().size;
			GetComponent<FactoryImprint>().AssetKey = key;
			base.transform.rotation = _rotation;
		}
	}

	public override void CloneInternals(IFitter original)
	{
		base.CloneInternals(original);
		RampJunctionFitter rampJunctionFitter = original as RampJunctionFitter;
		if (rampJunctionFitter != null)
		{
			_fitState = rampJunctionFitter._fitState;
			SetAs(_fitCycle[_fitState]);
		}
	}

	public override IFitter CopyTo(GameObject go)
	{
		Object.Destroy((MonoBehaviour)go.GetInterface<IFitter>());
		RampJunctionFitter rampJunctionFitter = go.AddComponent<RampJunctionFitter>();
		rampJunctionFitter.CloneInternals(this);
		return rampJunctionFitter;
	}
}
