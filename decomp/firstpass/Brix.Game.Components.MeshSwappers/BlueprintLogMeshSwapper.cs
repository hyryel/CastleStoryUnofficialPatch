using System.Collections.Generic;
using System.Linq;
using Brix.Engine;
using Brix.Engine.Blocks.MeshSelector;
using Brix.External.Factories;
using Brix.Game.Components.Volumes;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.Components.MeshSwappers;

public class BlueprintLogMeshSwapper : GameComponent, IBlueprintMeshSwapper
{
	private DefaultBlockMeshSelector _logUnculled;

	[SerializeField]
	private Factory.AssetKey _logUnculledAssetKey;

	private DefaultBlockMeshSelector _logCulled;

	[SerializeField]
	private Factory.AssetKey _logCulledAssetKey;

	private DefaultBlockMeshSelector _logTopFaceCulled;

	[SerializeField]
	private Factory.AssetKey _logTopFaceCulledAssetKey;

	private DefaultBlockMeshSelector _logBottomFaceCulled;

	[SerializeField]
	private Factory.AssetKey _logBottomFaceCulledAssetKey;

	private DefaultBlockMeshSelector _beamPlugZPositiveCulled;

	[SerializeField]
	private Factory.AssetKey _beamPlugZPositiveCulledAssetKey;

	private DefaultBlockMeshSelector _beamPlugZNegativeCulled;

	[SerializeField]
	private Factory.AssetKey _beamPlugZNegativeCulledAssetKey;

	private DefaultBlockMeshSelector _plankPlugZPositiveCulled;

	[SerializeField]
	private Factory.AssetKey _plankPlugZPositiveCulledAssetKey;

	private DefaultBlockMeshSelector _plankPlugZNegativeCulled;

	[SerializeField]
	private Factory.AssetKey _plankPlugZNegativeCulledAssetKey;

	[SerializeField]
	private GameObject _forwardBeamPlug;

	[SerializeField]
	private GameObject _backBeamPlug;

	[SerializeField]
	private GameObject _rightBeamPlug;

	[SerializeField]
	private GameObject _leftBeamPlug;

	[SerializeField]
	private GameObject _forwardPlankPlug;

	[SerializeField]
	private GameObject _backPlankPlug;

	[SerializeField]
	private GameObject _rightPlankPlug;

	[SerializeField]
	private GameObject _leftPlankPlug;

	private CompCache<MeshFilter> _logMeshFilter;

	public override void Initialize()
	{
		base.Initialize();
		_logUnculled = Factory.Peek(_logUnculledAssetKey).GetComponent<DefaultBlockMeshSelector>();
		_logCulled = Factory.Peek(_logCulledAssetKey).GetComponent<DefaultBlockMeshSelector>();
		_logTopFaceCulled = Factory.Peek(_logTopFaceCulledAssetKey).GetComponent<DefaultBlockMeshSelector>();
		_logBottomFaceCulled = Factory.Peek(_logBottomFaceCulledAssetKey).GetComponent<DefaultBlockMeshSelector>();
		_beamPlugZPositiveCulled = Factory.Peek(_beamPlugZPositiveCulledAssetKey).GetComponent<DefaultBlockMeshSelector>();
		_beamPlugZNegativeCulled = Factory.Peek(_beamPlugZNegativeCulledAssetKey).GetComponent<DefaultBlockMeshSelector>();
		_plankPlugZPositiveCulled = Factory.Peek(_plankPlugZPositiveCulledAssetKey).GetComponent<DefaultBlockMeshSelector>();
		_plankPlugZNegativeCulled = Factory.Peek(_plankPlugZNegativeCulledAssetKey).GetComponent<DefaultBlockMeshSelector>();
	}

	protected override void Awake()
	{
		_logMeshFilter = new CompCache<MeshFilter>(base.gameObject);
	}

	public void RecalculateMesh()
	{
		DefaultBlockMeshSelector defaultBlockMeshSelector = InferLogMeshSelectorFromSurrounding();
		_logMeshFilter.Get.sharedMesh = defaultBlockMeshSelector.MeshVariantFor(base.voxel.position, XYZ.up);
		EnableRequiredBeamPlugs();
		EnableRequiredPlankPlugs();
	}

	private void EnableRequiredBeamPlugs()
	{
		BlockInfo bi = Blocks.Wood_Beam.ToBlockInfo();
		ConfigurePlugIfMatchingBlockAndDirection(XYZ.forward, bi, _forwardBeamPlug, _beamPlugZPositiveCulled);
		ConfigurePlugIfMatchingBlockAndDirection(-XYZ.forward, bi, _backBeamPlug, _beamPlugZNegativeCulled);
		ConfigurePlugIfMatchingBlockAndDirection(XYZ.right, bi, _rightBeamPlug, _beamPlugZPositiveCulled);
		ConfigurePlugIfMatchingBlockAndDirection(-XYZ.right, bi, _leftBeamPlug, _beamPlugZNegativeCulled);
	}

	private void EnableRequiredPlankPlugs()
	{
		BlockInfo bi = Blocks.Wood_SinglePlank.ToBlockInfo();
		ConfigurePlugIfMatchingBlockAndDirection(XYZ.forward, bi, _forwardPlankPlug, _plankPlugZPositiveCulled);
		ConfigurePlugIfMatchingBlockAndDirection(-XYZ.forward, bi, _backPlankPlug, _plankPlugZNegativeCulled);
		ConfigurePlugIfMatchingBlockAndDirection(XYZ.right, bi, _rightPlankPlug, _plankPlugZPositiveCulled);
		ConfigurePlugIfMatchingBlockAndDirection(-XYZ.right, bi, _leftPlankPlug, _plankPlugZNegativeCulled);
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

	private DefaultBlockMeshSelector InferLogMeshSelectorFromSurrounding()
	{
		XYZ position = base.voxel.position + XYZ.up;
		XYZ position2 = base.voxel.position - XYZ.up;
		bool flag = !VolumeRegistry.VolumesWithPropertyAt(position, VProperty.fantome).Slinq().Any((IVolume v, Factory.AssetKey k) => v.AssetKey == k, AssetKey);
		bool flag2 = !VolumeRegistry.VolumesWithPropertyAt(position2, VProperty.fantome).Slinq().Any((IVolume v, Factory.AssetKey k) => v.AssetKey == k, AssetKey);
		using (Voxel.GetVoxel(position))
		{
			using (Voxel.GetVoxel(position2))
			{
				if (flag)
				{
					if (flag2)
					{
						return _logUnculled;
					}
					return _logBottomFaceCulled;
				}
				if (flag2)
				{
					return _logTopFaceCulled;
				}
				return _logCulled;
			}
		}
	}
}
