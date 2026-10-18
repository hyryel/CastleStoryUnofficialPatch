using System;
using System.IO;
using Brix.Engine.Blocks;
using Brix.External.Factories;
using Brix.Game;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;

namespace Brix.Engine;

public class TerrainBlockInfoMeta : BlockInfoMeta
{
	public class TerrainBlockInfo : BlockInfo
	{
		public IValidator validator;

		public TerrainBlockData GenerateBlocData(XYZ p)
		{
			return TerrainBlockData.Make(p, this);
		}

		public ValidationData ValidationData(TerrainBlockData tbd)
		{
			FreeVolume volume = tbd.Volume;
			return new ValidationData(tbd.Position, 0, volume, validator, MaxMissingSupport, Faction.DefaultNeutral);
		}
	}

	protected override BlockInfo InstantiateConcreteBlocInfoInstance()
	{
		TerrainBlockInfo terrainBlockInfo = new TerrainBlockInfo();
		terrainBlockInfo.VoxelDrawer = new DefaultVoxelDrawer(new NullMeshMaterialCombineGenerator());
		return terrainBlockInfo;
	}

	public override BlockInfo CreateBlocInfo()
	{
		TerrainBlockInfo terrainBlockInfo = (TerrainBlockInfo)base.CreateBlocInfo();
		terrainBlockInfo.validator = Factory.Peek(terrainBlockInfo.Key).GetComponent<IValidator>();
		return terrainBlockInfo;
	}

	protected override void Validate()
	{
		if (Type != BlockType.Terrain)
		{
			throw new InvalidDataException($"Configuration error! Block '{base.gameObject.name}' was set as BlockType.{Enum.GetName(typeof(BlockType), Type)} but it is configured using a TerrainBlockInfoMeta in the Factory.");
		}
	}
}
