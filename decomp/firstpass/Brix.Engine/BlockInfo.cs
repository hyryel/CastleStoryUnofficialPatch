using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Brix.Engine.Blocks;
using Brix.External.Factories;
using Brix.Game;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Brix.Utils.Logging;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Engine;

public class BlockInfo
{
	public enum AcceptNatureType
	{
		Accepts,
		RemovesOverlay,
		DoesntAccept
	}

	private static BrixLogger logger = new BrixLogger(typeof(BlockInfo).FullName);

	public int ID;

	public BlockGroups Groups;

	public VoxelSurface Surface;

	public string SurfaceString;

	public Factory.AssetKey ResourceKey;

	public ResourceType Ressource;

	public BlockType Type;

	public Factory.AssetKey Key;

	public bool IsPartialBloc;

	public bool IsTerrainOverlay;

	public AcceptNatureType AcceptNature;

	public Vector3 CenterOffset;

	public bool MergeableBloc;

	public bool OverrideMultiBlockNiveaux;

	public int MaxMissingSupport;

	public Factory.AssetKey BlueprintKey;

	private static List<BlockInfo> __empty = new List<BlockInfo>();

	public DefaultVoxelDrawer VoxelDrawer { get; set; }

	public bool IsNull => Type == BlockType.None;

	public bool IsNotNull => Type != BlockType.None;

	public BlockInfo()
	{
		SurfaceString = Enum.GetName(typeof(VoxelSurface), Surface);
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		IEnumerable<BlockGroups> source = Enum.GetValues(typeof(BlockGroups)).Cast<BlockGroups>();
		foreach (BlockGroups item in source.Where((BlockGroups value) => Groups.HasFlag(value)))
		{
			stringBuilder.Append(item.ToString()).Append(",");
		}
		return $"BlockInfo: ID = {ID} Groups = [{stringBuilder}] Key = {Key}";
	}

	public virtual void GenerateBlocData(XYZ rootPosition, int rotationIndex, GameObject blockGo, float dmg)
	{
		RootBlockData rootBlockData = RootBlockData.Make(rootPosition, rotationIndex, this, blockGo, dmg);
		rootBlockData.Volume.GetCases(VProperty.occupy).Slinq().Where((XYZ p, XYZ rp) => p != rp, rootPosition)
			.ForEach(delegate(XYZ p, RootBlockData rbd)
			{
				BlockData.Make(p, rbd);
			}, rootBlockData);
	}

	public IVolume Volume(XYZ position, int rotationIndex, int volumeConfig = 0)
	{
		return FreeVolume.Make(Key, position, rotationIndex, volumeConfig);
	}

	public virtual int CalculateNiveaux(IBlockData bd)
	{
		return -1;
	}

	public virtual bool IsOfType(int blockTypeId)
	{
		return ID == blockTypeId;
	}

	public Vector3 OffsetedPosition(XYZ position, Quaternion rotation)
	{
		return position.ToVector3() - rotation * CenterOffset;
	}

	public static BlockInfo GetFromID(int id)
	{
		if (BlockInfoGenerator._idToBlockInfos.TryGetValue(id, out var value))
		{
			return value;
		}
		logger.WarnFormat("BlockInfo with id '{0} could not be resolved. Make sure a BlockInfoMeta with id '{0}' was present in the Factory.'", id);
		return null;
	}

	public static BlockInfo GetFromKey(Factory.AssetKey key)
	{
		return (!key.IsNullOrInvalid()) ? BlockInfoGenerator._blockKeyToBlockInfos.GetValueOrDefault(key) : null;
	}

	public static BlockInfo GetFromBlueprint(Factory.AssetKey key)
	{
		return (!key.IsNullOrInvalid()) ? BlockInfoGenerator._blueprintKeyToBlockInfos.GetValueOrDefault(key) : null;
	}

	public static List<BlockInfo> GetAllInGroup(BlockGroups group)
	{
		return BlockInfoGenerator._blockGroupToBlockInfos.GetValueOrDefault(group, __empty);
	}

	public virtual float GetSurfaceHeight(IBlockData bd, float xOffset, float zOffset)
	{
		return 0.5f;
	}

	public ValidationData GenerateVData(RootBlockData bd)
	{
		IVolume volume = bd.Volume;
		return new ValidationData(bd.Position, bd.RotationIndex, volume, bd.Volume.GameObject.GetComponent<ValidatorComponent>(), MaxMissingSupport, Faction.DefaultNeutral, volume.GameObject);
	}
}
