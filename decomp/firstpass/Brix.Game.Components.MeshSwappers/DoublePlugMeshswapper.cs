using System.Collections.Generic;
using System.Linq;
using Brix.Engine;
using Brix.Engine.Blocks.MeshSelector;
using Brix.External.Factories;
using Brix.Game.Components.Volumes;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.Components.MeshSwappers;

internal class DoublePlugMeshswapper : GameComponent, IBlueprintMeshSwapper
{
	[SerializeField]
	private GameObject _backRightBeamPlug;

	[SerializeField]
	private GameObject _backLeftBeamPlug;

	[SerializeField]
	private GameObject _frontLeftBeamPlug;

	[SerializeField]
	private Factory.AssetKey _beamPlugZPositiveCulledAssetKey;

	[SerializeField]
	private Factory.AssetKey _beamPlugZNegativeCulledAssetKey;

	private DefaultBlockMeshSelector _beamPlugZPositiveCulled;

	private DefaultBlockMeshSelector _beamPlugZNegativeCulled;

	public override void Initialize()
	{
		base.Initialize();
		_beamPlugZPositiveCulled = Factory.Peek(_beamPlugZPositiveCulledAssetKey).GetComponent<DefaultBlockMeshSelector>();
		_beamPlugZNegativeCulled = Factory.Peek(_beamPlugZNegativeCulledAssetKey).GetComponent<DefaultBlockMeshSelector>();
	}

	public void RecalculateMesh()
	{
		EnableRequiredBeamPlugs();
	}

	private void EnableRequiredBeamPlugs()
	{
		if ((bool)_backRightBeamPlug)
		{
			ConfigurePlug(_backRightBeamPlug, Vector3.right, _beamPlugZPositiveCulled);
		}
		if ((bool)_backLeftBeamPlug)
		{
			ConfigurePlug(_backLeftBeamPlug, -Vector3.right, _beamPlugZNegativeCulled);
		}
		if ((bool)_frontLeftBeamPlug)
		{
			ConfigurePlug(_frontLeftBeamPlug, -Vector3.right, _beamPlugZNegativeCulled);
		}
	}

	private void ConfigurePlug(GameObject plug, Vector3 defaultOrient, DefaultBlockMeshSelector mesh)
	{
		Vector3 deltaV = base.transform.rotation * defaultOrient;
		BlockInfo bi = Blocks.Wood_Beam.ToBlockInfo();
		ConfigurePlugIfMatchingBlockAndDirection(deltaV, bi, plug, mesh);
	}

	private void ConfigurePlugIfMatchingBlockAndDirection(Vector3 deltaV3, BlockInfo bi, GameObject plugGO, DefaultBlockMeshSelector meshSelector)
	{
		XYZ position = XYZ.FromVector3(plugGO.transform.position);
		XYZ xYZ = XYZ.FromVector3(plugGO.transform.position + deltaV3);
		using IVoxel voxel = Voxel.GetVoxel(xYZ);
		if ((voxel.BlockInfo == bi && IsBlockRelativeZAxisFacing(bi.Key, voxel, deltaV3)) || IsBlueprintRelativeZAxisFacing(xYZ, bi.BlueprintKey, deltaV3))
		{
			plugGO.SetActive(value: true);
			plugGO.GetComponent<MeshFilter>().sharedMesh = meshSelector.MeshVariantFor(position, XYZ.forward);
		}
		else
		{
			plugGO.SetActive(value: false);
		}
	}

	private bool IsBlueprintRelativeZAxisFacing(XYZ xyz, Factory.AssetKey key, Vector3 deltaV3)
	{
		IEnumerable<Volume> enumerable = VolumeRegistry.VolumesWithPropertyAt(xyz, VProperty.fantome).OfType<Volume>();
		foreach (Volume item in enumerable)
		{
			if (item.AssetKey == key && Mathf.Abs(Vector3.Dot(item.transform.rotation * Vector3.forward, deltaV3)) > 0.5f)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsBlockRelativeZAxisFacing(Factory.AssetKey assetKey, IVoxel ibd, Vector3 referenceForward)
	{
		if (ibd.IsNull)
		{
			return false;
		}
		if (ibd.IsMultiBlock)
		{
			ibd = ((MultiBlockData)ibd).ChildBlocks.Slinq().First((IBlockData bd) => bd.IsOfType(assetKey));
		}
		return Mathf.Abs(Vector3.Dot(ibd.Rotation * Vector3.forward, referenceForward)) > 0.5f;
	}
}
