using System.Linq;
using Brix.Engine;
using Brix.Engine.Blocks.MeshSelector;
using Brix.External.Factories;
using Brix.Game.Components.Volumes;
using Brix.Util;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.Components.MeshSwappers;

public class BlueprintBeamMeshSwapper : GameComponent, IBlueprintMeshSwapper
{
	private DefaultBlockMeshSelector _beamUnculled;

	[SerializeField]
	private Factory.AssetKey _beamUnculledAssetKey;

	private DefaultBlockMeshSelector _beamZPositiveCulled;

	[SerializeField]
	private Factory.AssetKey _beamZPositiveCulledAssetKey;

	private DefaultBlockMeshSelector _beamZNegativeCulled;

	[SerializeField]
	private Factory.AssetKey _beamZNegativeCulledAssetKey;

	private DefaultBlockMeshSelector _beamCulled;

	[SerializeField]
	private Factory.AssetKey _beamCulledAssetKey;

	private CompCache<MeshFilter> beamMeshSwapper;

	protected override void Awake()
	{
		beamMeshSwapper = new CompCache<MeshFilter>(base.gameObject);
	}

	public override void Initialize()
	{
		base.Initialize();
		_beamUnculled = Factory.Peek(_beamUnculledAssetKey).GetComponent<DefaultBlockMeshSelector>();
		_beamZPositiveCulled = Factory.Peek(_beamZPositiveCulledAssetKey).GetComponent<DefaultBlockMeshSelector>();
		_beamZNegativeCulled = Factory.Peek(_beamZNegativeCulledAssetKey).GetComponent<DefaultBlockMeshSelector>();
		_beamCulled = Factory.Peek(_beamCulledAssetKey).GetComponent<DefaultBlockMeshSelector>();
	}

	public void RecalculateMesh()
	{
		beamMeshSwapper.Get.sharedMesh = InferMeshFromSurrounding();
	}

	private Mesh InferMeshFromSurrounding()
	{
		Quaternion canonicalRotation = base.voxel.CanonicalRotation;
		Vector3 vector = canonicalRotation * Vector3.forward;
		XYZ xYZ = XYZ.FromVector3(vector);
		DefaultBlockMeshSelector defaultBlockMeshSelector;
		using (IBlockData ibd = Voxel.GetBlockData(base.voxel.position + xYZ))
		{
			using IBlockData ibd2 = Voxel.GetBlockData(base.voxel.position - xYZ);
			defaultBlockMeshSelector = ((!HasMatchingBlockWithSameOrientation(ibd, vector) && !HasMatchingBlueprintWithSameOrientation(base.voxel.position + xYZ) && !HasLogOrPlug(base.voxel.position + xYZ, base.voxel.position)) ? ((!HasMatchingBlockWithSameOrientation(ibd2, vector) && !HasMatchingBlueprintWithSameOrientation(base.voxel.position - xYZ) && !HasLogOrPlug(base.voxel.position - xYZ, base.voxel.position)) ? _beamUnculled : _beamZNegativeCulled) : ((!HasMatchingBlockWithSameOrientation(ibd2, vector) && !HasMatchingBlueprintWithSameOrientation(base.voxel.position - xYZ) && !HasLogOrPlug(base.voxel.position - xYZ, base.voxel.position)) ? _beamZPositiveCulled : _beamCulled));
		}
		return defaultBlockMeshSelector.MeshVariantFor(base.voxel.position, xYZ);
	}

	public int ToFaceIndex(Vector3 v3LocalForward)
	{
		return Calcule.FaceIndexToScalaire(Calcule.VecteurToFaceIndex(v3LocalForward));
	}

	private bool HasMatchingBlueprintWithSameOrientation(XYZ index)
	{
		Volume volume = VolumeRegistry.VolumesWithPropertyAt(index, VProperty.fantome).OfType<Volume>().FirstOrDefault((Volume v) => v.AssetKey == AssetKey);
		return volume != null && volume.voxel.CanonicalRotation == base.voxel.CanonicalRotation;
	}

	private bool HasMatchingBlockWithSameOrientation(IBlockData ibd, Vector3 referenceForward)
	{
		if (ibd.IsNull)
		{
			return false;
		}
		if (ibd is MultiBlockData multiBlockData)
		{
			ibd = multiBlockData.ChildBlocks.Slinq().FirstOrDefault((IBlockData childBD) => childBD.BlockInfo.BlueprintKey == AssetKey);
			return ibd != null && Mathf.Abs(Vector3.Dot(ibd.Rotation * Vector3.forward, referenceForward)) > 0.5f;
		}
		return ibd.IsOfType(Blocks.Wood_SingleLog) || (ibd.BlockInfo.BlueprintKey == AssetKey && Mathf.Abs(Vector3.Dot(ibd.Rotation * Vector3.forward, referenceForward)) > 0.5f);
	}

	private bool HasLogOrPlug(XYZ index, XYZ hook)
	{
		return VolumeRegistry.VolumesWithPropertyAt(index, VProperty.logSupport).Slinq().Any() || VolumeRegistry.VolumesWithPropertyAt(index, VPropertyMask.occupy | VPropertyMask.fantome).Slinq().Any((IVolume v, XYZ h) => v.HasPropertyAt(VProperty.canHookBeam, h), hook);
	}
}
