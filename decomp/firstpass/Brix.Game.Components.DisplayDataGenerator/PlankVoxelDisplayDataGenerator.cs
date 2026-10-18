using System.Collections.Generic;
using Brix.Engine;
using Brix.Engine.Blocks;
using Brix.Engine.Blocks.MeshSelector;
using Brix.External.Factories;
using Brix.Game.Components.Volumes;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.Components.DisplayDataGenerator;

public class PlankVoxelDisplayDataGenerator : GameComponent, IVoxelDisplayDataGenerator
{
	private enum Dir
	{
		forward,
		backward
	}

	private DefaultBlockMeshSelector _plankUnculled;

	[SerializeField]
	private Factory.AssetKey _plankUnculledAssetKey;

	private DefaultBlockMeshSelector _plankZPositiveCulled;

	[SerializeField]
	private Factory.AssetKey _plankZPositiveCulledAssetKey;

	private DefaultBlockMeshSelector _plankZNegativeCulled;

	[SerializeField]
	private Factory.AssetKey _plankZNegativeCulledAssetKey;

	private DefaultBlockMeshSelector _plankCulled;

	[SerializeField]
	private Factory.AssetKey _plankCulledAssetKey;

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

	public override void Initialize()
	{
		base.Initialize();
		_plankUnculled = Factory.Peek(_plankUnculledAssetKey).GetComponent<DefaultBlockMeshSelector>();
		_plankZPositiveCulled = Factory.Peek(_plankZPositiveCulledAssetKey).GetComponent<DefaultBlockMeshSelector>();
		_plankZNegativeCulled = Factory.Peek(_plankZNegativeCulledAssetKey).GetComponent<DefaultBlockMeshSelector>();
		_plankCulled = Factory.Peek(_plankCulledAssetKey).GetComponent<DefaultBlockMeshSelector>();
	}

	public void GenerateVoxelDisplayData(BlockData bd, IVoxelDispatcher dispatcher)
	{
		AssetKeyHolder component = GetComponent<AssetKeyHolder>();
		Mesh mesh = SelectMeshFromSurrounding(bd);
		dispatcher.Dispatch(new VoxelDisplayData(component.AssetKey.ToString(), mesh, _plankCulled.Material, bd.AbsolutePivotPosition, bd.CanonicalRotation));
	}

	private Mesh SelectMeshFromSurrounding(BlockData bd)
	{
		Vector3 vector = bd.CanonicalRotation * Vector3.forward;
		XYZ heightAtRelativePos = GetHeightAtRelativePos(XYZ.FromVector3(vector), bd);
		XYZ heightAtRelativePos2 = GetHeightAtRelativePos(XYZ.FromVector3(-vector), bd);
		DefaultBlockMeshSelector defaultBlockMeshSelector;
		using (IBlockData ibd = Voxel.GetBlockData(bd.Position + heightAtRelativePos))
		{
			using IBlockData ibd2 = Voxel.GetBlockData(bd.Position + heightAtRelativePos2);
			defaultBlockMeshSelector = (HasMatchingBlockWithSameOrientation(bd, ibd, vector, Dir.forward) ? ((!HasMatchingBlockWithSameOrientation(bd, ibd2, vector, Dir.backward)) ? _plankZPositiveCulled : _plankCulled) : ((!HasMatchingBlockWithSameOrientation(bd, ibd2, vector, Dir.backward)) ? _plankUnculled : _plankZNegativeCulled));
		}
		return defaultBlockMeshSelector.MeshVariantFor(bd, heightAtRelativePos);
	}

	private XYZ GetHeightAtRelativePos(XYZ position, BlockData bd)
	{
		XYZ xYZ = bd.Position + position;
		if (bd.Volume.HasPropertyAt(VProperty.mainSupportReq, xYZ))
		{
			return position;
		}
		if (bd.Volume.HasPropertyAt(VProperty.mainSupportReq, xYZ + XYZ.up))
		{
			return position + XYZ.up;
		}
		if (bd.Volume.HasPropertyAt(VProperty.mainSupportReq, xYZ - XYZ.up))
		{
			return position - XYZ.up;
		}
		return XYZ.zero;
	}

	private bool HasMatchingBlockWithSameOrientation(BlockData bd, IBlockData ibd, Vector3 referenceForward, Dir direction)
	{
		if (!_keyMatchLibrary.ContainsKey(bd.BlockInfo.BlueprintKey))
		{
			return false;
		}
		if (ibd is MultiBlockData multiBlockData)
		{
			ibd = multiBlockData.ChildBlocks.Slinq().FirstOrDefault((IBlockData childBD) => _keyMatchLibrary[bd.BlockInfo.BlueprintKey][(int)direction].Contains(childBD.BlockInfo.BlueprintKey) && VolumeMatch(bd.Volume, childBD.Volume));
			return ibd != null && Mathf.Abs(Vector3.Dot(ibd.Rotation * Vector3.forward, referenceForward)) > 0.5f;
		}
		return (ibd.IsOfType(Blocks.Wood_SingleLog) && bd.BlockInfo.BlueprintKey == Blueprints.Wood_SinglePlank) || (_keyMatchLibrary[bd.BlockInfo.BlueprintKey][(int)direction].Contains(ibd.BlockInfo.BlueprintKey) && VolumeMatch(bd.Volume, ibd.Volume) && Mathf.Abs(Vector3.Dot(ibd.Rotation * Vector3.forward, referenceForward)) > 0.5f);
	}

	public DefaultBlockMeshSelector ChooseMeshSelector(BlockData bd, DefaultBlockMeshSelector noNailsSelector, DefaultBlockMeshSelector zAlignedNailsSelector, DefaultBlockMeshSelector xAlignedNailsSelector)
	{
		using IBlockData blockData = Voxel.GetBlockData(bd.Position);
		if (!blockData.IsMultiBlock)
		{
			return noNailsSelector;
		}
		IBlockData blockData2 = (blockData as MultiBlockData).ChildBlocks.Slinq().FirstOrDefault((IBlockData blockData3) => blockData3.BlockInfo.Key == Blocks.Wood_Beam);
		if (blockData2 != null)
		{
			return (!(blockData2.CanonicalRotation == bd.CanonicalRotation)) ? xAlignedNailsSelector : zAlignedNailsSelector;
		}
		return noNailsSelector;
	}

	private bool VolumeMatch(IVolume current, IVolume other)
	{
		return current.HasPropertyInAny(VProperty.mainSupportReq, other.GetCases(VProperty.plankSupport)) && other.HasPropertyInAny(VProperty.mainSupportReq, current.GetCases(VProperty.plankSupport));
	}
}
