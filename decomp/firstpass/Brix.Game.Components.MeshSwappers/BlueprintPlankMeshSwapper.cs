using System.Collections.Generic;
using System.Linq;
using Brix.Engine;
using Brix.Engine.Blocks.MeshSelector;
using Brix.External.Factories;
using Brix.Game.Components.Volumes;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.Components.MeshSwappers;

[ExecuteInEditMode]
public class BlueprintPlankMeshSwapper : GameComponent, IBlueprintMeshSwapper
{
	private enum Dir
	{
		forward,
		backward
	}

	[SerializeField]
	private DefaultBlockMeshSelector _plankUnculled;

	[SerializeField]
	private Factory.AssetKey _plankUnculledAssetKey;

	[SerializeField]
	private DefaultBlockMeshSelector _plankZPositiveCulled;

	[SerializeField]
	private Factory.AssetKey _plankZPositiveCulledAssetKey;

	[SerializeField]
	private DefaultBlockMeshSelector _plankZNegativeCulled;

	[SerializeField]
	private Factory.AssetKey _plankZNegativeCulledAssetKey;

	[SerializeField]
	private DefaultBlockMeshSelector _plankCulled;

	[SerializeField]
	private Factory.AssetKey _plankCulledAssetKey;

	private CompCache<MeshFilter> _plankMeshFilter;

	private CompCache<IVolume> _volume;

	private static HashSet<Factory.AssetKey> _topAssetKeys = new HashSet<Factory.AssetKey>
	{
		Blueprints.Wood_Ramp_TopA,
		Blueprints.Wood_Ramp_TopB,
		Blueprints.Wood_Ramp_JunctionA,
		Blueprints.Wood_Ramp_JunctionB,
		Blueprints.Wood_Ramp_JunctionC,
		Blueprints.Wood_Ramp_JunctionD,
		Blueprints.Wood_Ramp_JunctionE,
		Blueprints.Wood_Ramp_JunctionF
	};

	private static HashSet<Factory.AssetKey> _midAssetKeys = new HashSet<Factory.AssetKey>
	{
		Blueprints.Wood_Ramp_MidA,
		Blueprints.Wood_Ramp_MidB
	};

	private static HashSet<Factory.AssetKey> _botAssetKeys = new HashSet<Factory.AssetKey>
	{
		Blueprints.Wood_Ramp_BotA,
		Blueprints.Wood_Ramp_BotB
	};

	private static HashSet<Factory.AssetKey> _straightAssetKeys = new HashSet<Factory.AssetKey>
	{
		Blueprints.Wood_SinglePlank,
		Blueprints.Wood_Ramp_JunctionA,
		Blueprints.Wood_Ramp_JunctionB,
		Blueprints.Wood_Ramp_JunctionC,
		Blueprints.Wood_Ramp_JunctionD
	};

	private static Dictionary<Factory.AssetKey, HashSet<Factory.AssetKey>[]> _keyMatchLibrary = new Dictionary<Factory.AssetKey, HashSet<Factory.AssetKey>[]>
	{
		{
			Blueprints.Wood_Ramp_TopA,
			new HashSet<Factory.AssetKey>[2] { _botAssetKeys, _midAssetKeys }
		},
		{
			Blueprints.Wood_Ramp_TopB,
			new HashSet<Factory.AssetKey>[2] { _midAssetKeys, _botAssetKeys }
		},
		{
			Blueprints.Wood_Ramp_BotA,
			new HashSet<Factory.AssetKey>[2] { _midAssetKeys, _topAssetKeys }
		},
		{
			Blueprints.Wood_Ramp_BotB,
			new HashSet<Factory.AssetKey>[2] { _topAssetKeys, _midAssetKeys }
		},
		{
			Blueprints.Wood_Ramp_MidA,
			new HashSet<Factory.AssetKey>[2] { _topAssetKeys, _botAssetKeys }
		},
		{
			Blueprints.Wood_Ramp_MidB,
			new HashSet<Factory.AssetKey>[2] { _botAssetKeys, _topAssetKeys }
		},
		{
			Blueprints.Wood_Ramp_JunctionA,
			new HashSet<Factory.AssetKey>[2] { _straightAssetKeys, _midAssetKeys }
		},
		{
			Blueprints.Wood_Ramp_JunctionB,
			new HashSet<Factory.AssetKey>[2] { _midAssetKeys, _straightAssetKeys }
		},
		{
			Blueprints.Wood_Ramp_JunctionC,
			new HashSet<Factory.AssetKey>[2] { _straightAssetKeys, _botAssetKeys }
		},
		{
			Blueprints.Wood_Ramp_JunctionD,
			new HashSet<Factory.AssetKey>[2] { _botAssetKeys, _straightAssetKeys }
		},
		{
			Blueprints.Wood_Ramp_JunctionE,
			new HashSet<Factory.AssetKey>[2] { _midAssetKeys, _midAssetKeys }
		},
		{
			Blueprints.Wood_Ramp_JunctionF,
			new HashSet<Factory.AssetKey>[2] { _botAssetKeys, _botAssetKeys }
		},
		{
			Blueprints.Wood_SinglePlank,
			new HashSet<Factory.AssetKey>[2] { _straightAssetKeys, _straightAssetKeys }
		}
	};

	protected override void Awake()
	{
		base.Awake();
		_plankMeshFilter = new CompCache<MeshFilter>(base.gameObject);
		_volume = new CompCache<IVolume>(base.gameObject);
	}

	public override void Initialize()
	{
		base.Initialize();
		_plankUnculled = Factory.Peek(_plankUnculledAssetKey).GetComponent<DefaultBlockMeshSelector>();
		_plankZPositiveCulled = Factory.Peek(_plankZPositiveCulledAssetKey).GetComponent<DefaultBlockMeshSelector>();
		_plankZNegativeCulled = Factory.Peek(_plankZNegativeCulledAssetKey).GetComponent<DefaultBlockMeshSelector>();
		_plankCulled = Factory.Peek(_plankCulledAssetKey).GetComponent<DefaultBlockMeshSelector>();
	}

	public void RecalculateMesh()
	{
		_plankMeshFilter.Get.sharedMesh = InferMeshFromSurrounding();
	}

	private Mesh InferMeshFromSurrounding()
	{
		Quaternion canonicalRotation = base.voxel.CanonicalRotation;
		Vector3 vector = canonicalRotation * Vector3.forward;
		XYZ heightAtRelativePos = GetHeightAtRelativePos(XYZ.FromVector3(vector));
		XYZ heightAtRelativePos2 = GetHeightAtRelativePos(XYZ.FromVector3(-vector));
		DefaultBlockMeshSelector defaultBlockMeshSelector;
		using (IBlockData ibd = Voxel.GetBlockData(base.voxel.position + heightAtRelativePos))
		{
			using IBlockData ibd2 = Voxel.GetBlockData(base.voxel.position + heightAtRelativePos2);
			defaultBlockMeshSelector = ((!HasMatchingBlockWithSameOrientation(ibd, vector, Dir.forward) && !HasMatchingBlueprintWithSameOrientation(base.voxel.position + heightAtRelativePos, Dir.forward)) ? ((!HasMatchingBlockWithSameOrientation(ibd2, vector, Dir.backward) && !HasMatchingBlueprintWithSameOrientation(base.voxel.position + heightAtRelativePos2, Dir.backward)) ? _plankUnculled : _plankZNegativeCulled) : ((!HasMatchingBlockWithSameOrientation(ibd2, vector, Dir.backward) && !HasMatchingBlueprintWithSameOrientation(base.voxel.position + heightAtRelativePos2, Dir.backward)) ? _plankZPositiveCulled : _plankCulled));
		}
		return defaultBlockMeshSelector.MeshVariantFor(base.voxel.position, heightAtRelativePos);
	}

	private XYZ GetHeightAtRelativePos(XYZ position)
	{
		XYZ xYZ = base.voxel.position + position;
		if (_volume.Get.HasPropertyAt(VProperty.mainSupportReq, xYZ))
		{
			return position;
		}
		if (_volume.Get.HasPropertyAt(VProperty.mainSupportReq, xYZ + XYZ.up))
		{
			return position + XYZ.up;
		}
		if (_volume.Get.HasPropertyAt(VProperty.mainSupportReq, xYZ - XYZ.up))
		{
			return position - XYZ.up;
		}
		return XYZ.zero;
	}

	private bool HasMatchingBlueprintWithSameOrientation(XYZ index, Dir direction)
	{
		if (!_keyMatchLibrary.ContainsKey(AssetKey))
		{
			return false;
		}
		BlueprintVolume blueprintVolume = VolumeRegistry.VolumesWithPropertyAt(index, VProperty.plankSupport).OfType<BlueprintVolume>().FirstOrDefault((BlueprintVolume v) => _keyMatchLibrary[AssetKey][(int)direction].Contains(v.AssetKey) && VolumeMatch(v));
		return blueprintVolume != null && blueprintVolume.voxel.CanonicalRotation == base.voxel.CanonicalRotation;
	}

	private bool HasMatchingBlockWithSameOrientation(IBlockData ibd, Vector3 referenceForward, Dir direction)
	{
		if (ibd.IsNull || !_keyMatchLibrary.ContainsKey(AssetKey))
		{
			return false;
		}
		if (ibd is MultiBlockData multiBlockData)
		{
			ibd = multiBlockData.ChildBlocks.Slinq().Where((IBlockData childBD, Dir _direction) => _keyMatchLibrary[AssetKey][(int)_direction].Contains(childBD.BlockInfo.BlueprintKey) && VolumeMatch(childBD.Volume), direction).FirstOrDefault();
			return ibd != null && Mathf.Abs(Vector3.Dot(ibd.Rotation * Vector3.forward, referenceForward)) > 0.5f;
		}
		return _keyMatchLibrary[AssetKey][(int)direction].Contains(ibd.BlockInfo.BlueprintKey) && VolumeMatch(ibd.Volume) && Mathf.Abs(Vector3.Dot(ibd.Rotation * Vector3.forward, referenceForward)) > 0.5f;
	}

	public DefaultBlockMeshSelector ChooseMeshSelector(DefaultBlockMeshSelector noNailsSelector, DefaultBlockMeshSelector zAlignedNailsSelector, DefaultBlockMeshSelector xAlignedNailsSelector)
	{
		Volume volume = VolumeRegistry.VolumesWithPropertyAt(base.voxel.position, VProperty.fantome).OfType<Volume>().FirstOrDefault((Volume v) => v.AssetKey == Blueprints.Wood_Beam);
		if (volume != null)
		{
			return (!(volume.voxel.CanonicalRotation == base.voxel.CanonicalRotation)) ? xAlignedNailsSelector : zAlignedNailsSelector;
		}
		using IBlockData blockData = Voxel.GetBlockData(base.voxel.position);
		if (!blockData.IsOfType(Blocks.Wood_Beam))
		{
			return noNailsSelector;
		}
		Quaternion canonicalRotation = blockData.CanonicalRotation;
		if (blockData.IsMultiBlock)
		{
			IBlockData blockData2 = ((MultiBlockData)blockData).ChildBlocks.Slinq().FirstOrDefault((IBlockData blockData3) => blockData3.BlockInfo.Key == Blocks.Wood_Beam);
			if (blockData2 != null)
			{
				canonicalRotation = blockData2.CanonicalRotation;
			}
		}
		return (!(canonicalRotation == base.voxel.CanonicalRotation)) ? xAlignedNailsSelector : zAlignedNailsSelector;
	}

	private bool VolumeMatch(IVolume other)
	{
		return _volume.Get.HasPropertyInAny(VProperty.mainSupportReq, other.GetCases(VProperty.plankSupport)) && other.HasPropertyInAny(VProperty.mainSupportReq, _volume.Get.GetCases(VProperty.plankSupport));
	}
}
