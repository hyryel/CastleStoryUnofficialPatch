using System;
using System.IO;
using Brix.Engine.Blocks;
using Brix.External.Factories;
using Brix.Game.Components;
using UnityEngine;

namespace Brix.Engine;

public class BlockInfoMeta : MonoBehaviour
{
	private class DefaultBlocInfo : BlockInfo
	{
	}

	public int ID;

	public BlockGroups Groups;

	public VoxelSurface Surface;

	public Factory.AssetKey RawResourceKey;

	public ResourceType Ressource = ResourceType.StoneBlock;

	public BlockType Type;

	public bool IsPartialBloc;

	public bool MergeableBloc;

	public bool OverrideMultiBlockNiveaux;

	public bool IsTerrainOverlay;

	public BlockInfo.AcceptNatureType AcceptNature;

	public Factory.AssetKey BlueprintKey;

	public CrackMaterialHolder CrackMatHolder;

	public virtual BlockInfo CreateBlocInfo()
	{
		Validate();
		BlockInfo blockInfo = InstantiateConcreteBlocInfoInstance();
		blockInfo.ID = ID;
		blockInfo.Groups = Groups;
		blockInfo.Surface = Surface;
		blockInfo.ResourceKey = RawResourceKey;
		blockInfo.Ressource = Ressource;
		blockInfo.Key = blockInfo.Key ?? GetComponent<AssetKeyHolder>().AssetKey;
		blockInfo.BlueprintKey = BlueprintKey;
		blockInfo.IsPartialBloc = IsPartialBloc;
		blockInfo.IsTerrainOverlay = IsTerrainOverlay;
		blockInfo.AcceptNature = AcceptNature;
		blockInfo.Type = Type;
		blockInfo.CenterOffset = GetComponent<BaseVoxelTransform>().CenterOffset;
		blockInfo.MergeableBloc = MergeableBloc;
		blockInfo.MaxMissingSupport = (Factory.Exist(BlueprintKey) ? Factory.Peek(BlueprintKey).GetComponent<ValidatorComponent>().MaxMissingSupport : 0);
		blockInfo.OverrideMultiBlockNiveaux = OverrideMultiBlockNiveaux;
		blockInfo.VoxelDrawer = blockInfo.VoxelDrawer ?? new DefaultVoxelDrawer(GetComponent<IVoxelDisplayDataGenerator>() ?? new DefaultVoxelDisplayDataGenerator(blockInfo.Key, CrackMatHolder));
		return blockInfo;
	}

	protected virtual void Validate()
	{
		if (Type != BlockType.Block)
		{
			throw new InvalidDataException($"Configuration error! Block '{base.gameObject.name}' was set as BlockType.{Enum.GetName(typeof(BlockType), Type)} but it is configured using a regular BlockInfoMeta in the Factory.");
		}
	}

	protected virtual BlockInfo InstantiateConcreteBlocInfoInstance()
	{
		return new DefaultBlocInfo();
	}
}
