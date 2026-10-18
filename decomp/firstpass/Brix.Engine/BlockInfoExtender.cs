using System.Collections.Generic;
using Brix.External.Factories;

namespace Brix.Engine;

public static class BlockInfoExtender
{
	public static BlockInfo ToBlockInfo(this Factory.AssetKey key)
	{
		return (!key.IsNullOrInvalid()) ? BlockInfoGenerator._blockKeyToBlockInfos.GetValueOrDefault(key) : null;
	}

	public static int ToBlockInfoID(this Factory.AssetKey key)
	{
		return BlockInfo.GetFromKey(key)?.ID ?? (-1);
	}

	public static BlockInfo BlockInfoIDToBlockInfo(this int id)
	{
		return BlockInfo.GetFromID(id);
	}

	public static Factory.AssetKey BlockInfoIDToKey(this int id)
	{
		return BlockInfo.GetFromID(id)?.Key;
	}

	public static List<BlockInfo> GetAllInGroup(this BlockGroups group)
	{
		return BlockInfo.GetAllInGroup(group);
	}
}
