using Brix.Engine.Blocks;
using Brix.External.Factories;
using UnityEngine;

namespace Brix.Engine;

public class NullBlockInfo : BlockInfo
{
	public static NullBlockInfo Instance = new NullBlockInfo();

	private NullBlockInfo()
	{
		ID = 0;
		Groups = BlockGroups.None;
		Surface = VoxelSurface.Empty;
		ResourceKey = RawResources.RawStone;
		Key = Brix.External.Factories.Blocks.Stone_Brick;
		BlueprintKey = Blueprints.Stone_Brick;
		IsPartialBloc = false;
		Type = BlockType.None;
		CenterOffset = Vector3.zero;
		MergeableBloc = true;
		base.VoxelDrawer = new DefaultVoxelDrawer(new NullMeshMaterialCombineGenerator());
	}
}
