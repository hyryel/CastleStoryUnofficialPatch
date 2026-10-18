using System.Collections.Generic;
using Brix.Common.Collections;
using Brix.Engine;
using Brix.External.Factories;
using Brix.Game.Components.Volumes;
using Brix.Input;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.Components;

internal class RampFitter : PlankFitter
{
	private XYZ _position = XYZ.zero;

	private int _fitState;

	private Factory.AssetKey[] _activeCycle = _fitCycleA;

	private static Factory.AssetKey[] _fitCycleA = new Factory.AssetKey[4]
	{
		Blueprints.Wood_Ramp_TopA,
		Blueprints.Wood_Ramp_TopA,
		Blueprints.Wood_Ramp_TopB,
		Blueprints.Wood_Ramp_TopB
	};

	private static Factory.AssetKey[] _fitCycleB = new Factory.AssetKey[4]
	{
		Blueprints.Wood_Ramp_BotA,
		Blueprints.Wood_Ramp_BotA,
		Blueprints.Wood_Ramp_BotB,
		Blueprints.Wood_Ramp_BotB
	};

	private static Factory.AssetKey[] _fitCycleC = new Factory.AssetKey[4]
	{
		Blueprints.Wood_Ramp_MidA,
		Blueprints.Wood_Ramp_MidA,
		Blueprints.Wood_Ramp_MidB,
		Blueprints.Wood_Ramp_MidB
	};

	private static Quaternion[] _rotationCycle = new Quaternion[4]
	{
		Quaternion.identity,
		Quaternion.Euler(0f, 90f, 0f),
		Quaternion.identity,
		Quaternion.Euler(0f, 90f, 0f)
	};

	private XYZ[] _tempPosArray = new XYZ[3];

	private static List<Factory.AssetKey[]> _cycles = new List<Factory.AssetKey[]> { _fitCycleC, _fitCycleB, _fitCycleA };

	public override void FitAndOrient(VoxelRaycastHit voxelHit, out Vector3 absolutePosition)
	{
		Vector3 vector;
		if ((IsFitInCrust(voxelHit) && voxelHit.IsRaycastResult) || HasFreeSpotOnBeam(voxelHit.VoxelPosition))
		{
			vector = voxelHit.VoxelPosition.ToVector3();
			_position = XYZ.FromVector3(vector);
			_activeCycle = _fitCycleA;
		}
		else
		{
			IVolume vol = null;
			Factory.AssetKey hit = null;
			AssetKeyHolder component = voxelHit.HitGameObject.GetComponent<AssetKeyHolder>();
			if (component != null)
			{
				vol = voxelHit.HitGameObject.GetComponent<IVolume>();
				hit = component.AssetKey;
			}
			else
			{
				IVoxel hitIVoxel = voxelHit.HitIVoxel;
				if (hitIVoxel.IsMultiBlock)
				{
					IBlockData blockData = (from bd in ((MultiBlockData)hitIVoxel).ChildBlocks.Slinq()
						where bd.BlockInfo.BlueprintKey != Blueprints.Wood_Beam
						select bd).FirstOrDefault();
					vol = blockData.Volume;
					hit = blockData.BlockInfo.BlueprintKey;
				}
				else if (!hitIVoxel.IsNull)
				{
					vol = hitIVoxel.Volume;
					hit = hitIVoxel.BlockInfo.BlueprintKey;
				}
			}
			SetByContext(voxelHit.VoxelNormal, vol, hit);
			vector = _position.ToVector3();
		}
		SetAs(_activeCycle[_fitState]);
		base.gameObject.transform.rotation = _rotation;
		base.gameObject.transform.position = vector;
		absolutePosition = vector;
	}

	public void FitAndOrientFrom(VoxelRaycastHit voxelHit, out Vector3 absolutePosition, GameObject fakeHit)
	{
		voxelHit.FakeHitGameObject = fakeHit;
		FitAndOrient(voxelHit, out absolutePosition);
	}

	public void FitAndOrientFrom(VoxelRaycastHit voxelHit, GameObject fakeHit)
	{
		voxelHit.FakeHitGameObject = fakeHit;
		FitAndOrient(voxelHit, out var _);
	}

	public override void ChangeFit()
	{
		if (_fitState >= 3)
		{
			_fitState = 0;
		}
		else
		{
			_fitState++;
		}
		SetAs(_activeCycle[_fitState]);
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
			base.transform.position = _position.ToVector3();
			base.transform.rotation = _rotation;
		}
	}

	private void SetByContext(XYZ pos, IVolume vol, Factory.AssetKey hit)
	{
		if (vol == null || hit == null)
		{
			_position = pos;
			_activeCycle = _fitCycleA;
			SetAs(_activeCycle[_fitState]);
			return;
		}
		ref XYZ reference = ref _tempPosArray[0];
		reference = pos + XYZ.up;
		ref XYZ reference2 = ref _tempPosArray[1];
		reference2 = pos - XYZ.up;
		_tempPosArray[2] = pos;
		XYZ[] tempPosArray = _tempPosArray;
		for (int i = 0; i < tempPosArray.Length && !vol.HasPropertyAt(VProperty.mainSupportReq, _position = tempPosArray[i]); i++)
		{
		}
		foreach (Factory.AssetKey[] item in _cycles.Slinq().Where((Factory.AssetKey[] c, Factory.AssetKey h) => !c.Slinq().Contains(h), hit).NoLeak())
		{
			_activeCycle = item;
			SetAs(_activeCycle[_fitState]);
			if (GetComponent<IVolume>().HasPropertyInAny(VProperty.mainSupportReq, vol.GetCases(VPropertyMask.occupy | VPropertyMask.fantome)))
			{
				return;
			}
		}
		_activeCycle = _fitCycleA;
	}

	public override void CloneInternals(IFitter original)
	{
		base.CloneInternals(original);
		RampFitter rampFitter = original as RampFitter;
		if (rampFitter != null)
		{
			_fitState = rampFitter._fitState;
			_activeCycle = rampFitter._activeCycle;
			SetAs(_activeCycle[_fitState]);
		}
	}

	public override IFitter CopyTo(GameObject go)
	{
		Object.Destroy((MonoBehaviour)go.GetInterface<IFitter>());
		RampFitter rampFitter = go.AddComponent<RampFitter>();
		rampFitter.CloneInternals(this);
		return rampFitter;
	}
}
