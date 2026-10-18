using System.Collections.Generic;
using System.Linq;
using Brix.Engine;
using Brix.Engine.Blocks.MeshSelector;
using Brix.External.Factories;
using Brix.Game.Components.Volumes;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.Components.MeshSwappers;

public class BlueprintBeamPlugMeshSwapper : GameComponent, IBlueprintMeshSwapper
{
	[SerializeField]
	private GameObject _forwardBeamPlug;

	[SerializeField]
	private GameObject _backBeamPlug;

	[SerializeField]
	private GameObject _rightBeamPlug;

	[SerializeField]
	private GameObject _leftBeamPlug;

	private DefaultBlockMeshSelector _beamPlugZPositiveCulled;

	[SerializeField]
	private Factory.AssetKey _beamPlugZPositiveCulledAssetKey;

	private DefaultBlockMeshSelector _beamPlugZNegativeCulled;

	[SerializeField]
	private Factory.AssetKey _beamPlugZNegativeCulledAssetKey;

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
		if ((bool)_forwardBeamPlug)
		{
			ConfigurePlug(_forwardBeamPlug, Vector3.forward, _beamPlugZPositiveCulled);
		}
		if ((bool)_backBeamPlug)
		{
			ConfigurePlug(_backBeamPlug, -Vector3.forward, _beamPlugZNegativeCulled);
		}
		if ((bool)_rightBeamPlug)
		{
			ConfigurePlug(_rightBeamPlug, Vector3.right, _beamPlugZPositiveCulled);
		}
		if ((bool)_leftBeamPlug)
		{
			ConfigurePlug(_leftBeamPlug, -Vector3.right, _beamPlugZNegativeCulled);
		}
	}

	private void ConfigurePlug(GameObject plug, Vector3 defaultOrient, DefaultBlockMeshSelector mesh)
	{
		XYZ deltaP = XYZ.FromVector3(base.transform.rotation * defaultOrient);
		BlockInfo bi = Blocks.Wood_Beam.ToBlockInfo();
		ConfigurePlugIfMatchingBlockAndDirection(deltaP, bi, plug, mesh);
	}

	private void ConfigurePlugIfMatchingBlockAndDirection(XYZ deltaP, BlockInfo bi, GameObject plugGO, DefaultBlockMeshSelector meshSelector)
	{
		using IVoxel voxel = Voxel.GetVoxel(base.voxel.position + deltaP);
		Vector3 vector = deltaP.ToVector3();
		if ((voxel.BlockInfo == bi && IsBlockRelativeZAxisFacing(bi.Key, voxel, vector)) || IsBlueprintRelativeZAxisFacing(base.voxel.position + deltaP, bi.BlueprintKey, vector))
		{
			plugGO.SetActive(value: true);
			plugGO.GetComponent<MeshFilter>().sharedMesh = meshSelector.MeshVariantFor(base.voxel.position, XYZ.forward);
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
