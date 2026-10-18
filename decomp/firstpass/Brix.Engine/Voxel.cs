using System;
using System.Collections.Generic;
using Brix.Components;
using Brix.Engine.Abstract;
using Brix.Engine.Blocks;
using Brix.Engine.Nature;
using Brix.External.Factories;
using Brix.Game;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Brix.Legacy;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.Engine;

public class Voxel : Brix.Engine.Abstract.Voxel
{
	private static BrixLogger logger = new BrixLogger(typeof(Voxel).FullName);

	public static VoxelEngineImpl Engine;

	private static VoxelHandle _handle = null;

	private static List<int> _ditch = new List<int>();

	private static List<int> _voxelTypes = new List<int>();

	public static void SetVoxelEngine(VoxelEngineImpl engine)
	{
		Engine = engine;
		_handle = engine.GetVoxelHandle(XYZ.zero);
	}

	public static Blueprint GetCubeFantome(XYZ position)
	{
		IVolume volume = VolumeRegistry.VolumesWithPropertyAt(position, VProperty.fantome).Slinq().FirstOrDefault();
		if (volume == null)
		{
			return null;
		}
		if (volume.Faction == User.LocalUser.faction)
		{
			return volume.GetComponent<Blueprint>();
		}
		return null;
	}

	public static bool IsNonEmpty(XYZ position)
	{
		return Engine.IsVoxelNonEmpty(position) || BrixSingleton<BlockEngine>.Instance.IsBlock(position);
	}

	public static bool IsEmpty(XYZ position)
	{
		return !IsNonEmpty(position);
	}

	public static bool HasEmptyNeighbor(XYZ voxel)
	{
		if (IsEmpty(voxel + XYZ.up) && IsEmpty(voxel + XYZ.up + XYZ.up))
		{
			return true;
		}
		if (IsEmpty(voxel - XYZ.up) && IsEmpty(voxel - XYZ.up - XYZ.up))
		{
			return true;
		}
		if (IsEmpty(voxel + XYZ.right) && (IsEmpty(voxel + XYZ.right + XYZ.up) || IsEmpty(voxel + XYZ.right - XYZ.up)))
		{
			return true;
		}
		if (IsEmpty(voxel - XYZ.right) && (IsEmpty(voxel - XYZ.right + XYZ.up) || IsEmpty(voxel - XYZ.right - XYZ.up)))
		{
			return true;
		}
		if (IsEmpty(voxel + XYZ.forward) && (IsEmpty(voxel + XYZ.forward + XYZ.up) || IsEmpty(voxel + XYZ.forward - XYZ.up)))
		{
			return true;
		}
		if (IsEmpty(voxel - XYZ.forward) && (IsEmpty(voxel - XYZ.forward + XYZ.up) || IsEmpty(voxel - XYZ.forward - XYZ.up)))
		{
			return true;
		}
		return false;
	}

	public static bool IsInvincible(XYZ position)
	{
		using IVoxel voxel = GetVoxel(position);
		return voxel.IsInvincible;
	}

	private static VoxelHandle GetVoxelHandle(XYZ index)
	{
		Engine.AssignVoxelHandle(_handle, index);
		return _handle;
	}

	private static int GetID(XYZ position)
	{
		VoxelHandle voxelHandle = GetVoxelHandle(position);
		int num = Engine.GetVoxelType(voxelHandle);
		if (num != 0)
		{
			if (num == 255)
			{
				num = Engine.EstimateVoxelDeepType(voxelHandle);
			}
			return num;
		}
		return BrixSingleton<BlockEngine>.Instance.GetBlockID(position);
	}

	public static BlockInfo GetBlockInfo(XYZ position)
	{
		int iD = GetID(position);
		return (iD <= 0) ? NullBlockInfo.Instance : iD.BlockInfoIDToBlockInfo();
	}

	public static IVoxel GetVoxel(XYZ position)
	{
		return BrixSingleton<BlockEngine>.Instance.GetBlockData(position) ?? GetTerrainBlockData(position);
	}

	public static IBlockData GetBlockData(XYZ position)
	{
		if (!BrixSingleton<BlockEngine>.Instance)
		{
			return NullBlockData.Make();
		}
		return BrixSingleton<BlockEngine>.Instance.GetBlockData(position) ?? NullBlockData.Make();
	}

	public static IVoxel GetTerrainBlockData(XYZ position)
	{
		return (GetBlockInfo(position) is TerrainBlockInfoMeta.TerrainBlockInfo terrainBlockInfo) ? ((IVoxel)terrainBlockInfo.GenerateBlocData(position)) : ((IVoxel)NullBlockData.Make());
	}

	public static IVoxel GetVoxelDataWithId(XYZ position, int blockId)
	{
		IBlockData blockData = BrixSingleton<BlockEngine>.Instance.GetBlockData(position);
		if (blockData != null && blockData.IsOfType(blockId))
		{
			return blockData;
		}
		IVoxel terrainBlockData = GetTerrainBlockData(position);
		if (terrainBlockData.IsOfType(blockId))
		{
			return terrainBlockData;
		}
		return NullBlockData.Make();
	}

	public static bool IsTerrain(XYZ position)
	{
		return Engine.IsVoxelNonEmpty(position);
	}

	public static bool IsBlock(XYZ position)
	{
		return BrixSingleton<BlockEngine>.Instance.IsBlock(position);
	}

	public static void RemoveTerrain(TerrainBlockData tbd)
	{
		bool heightmapMode = Architecte.editorMode && Architecte.heightmapMode;
		RemoveTerrain(tbd.Position, tbd.VoxelHandle, heightmapMode);
	}

	private static void RemoveTerrain(XYZ position, VoxelHandle handle, bool heightmapMode)
	{
		if (Engine.GetVoxelType(handle) != 0)
		{
			if (heightmapMode)
			{
				VoxelEngine.RemoveColumnTop(position.x, position.z);
			}
			else
			{
				Engine.RemoveVoxel(handle);
			}
		}
	}

	public static int GetNiveaux(XYZ position)
	{
		if (IsTerrain(position))
		{
			if (IsTerrain(position + XYZ.up))
			{
				return 13107;
			}
			using IVoxel voxel = GetTerrainBlockData(position);
			return voxel.Niveaux;
		}
		using IBlockData blockData = BrixSingleton<BlockEngine>.Instance.GetBlockData(position) ?? NullBlockData.Make();
		return blockData.Niveaux;
	}

	public static bool IsFull(XYZ position)
	{
		return IsNonEmpty(position) && (GetNiveaux(position) & 0x3333) == 13107;
	}

	public static bool IsCrust(XYZ position)
	{
		if (IsTerrain(position + XYZ.up))
		{
			return false;
		}
		using IVoxel voxel = GetTerrainBlockData(position);
		return voxel.IsCrust;
	}

	public static bool IsOpen(XYZ position)
	{
		if (IsNonEmpty(position + XYZ.up))
		{
			return false;
		}
		if (IsNonEmpty(position))
		{
			return !IsFull(position);
		}
		return true;
	}

	public static bool IsOpenAndWalkable(XYZ position)
	{
		if (IsNonEmpty(position + XYZ.up))
		{
			return false;
		}
		if (IsNonEmpty(position))
		{
			return !IsFull(position);
		}
		return IsFull(position - XYZ.up);
	}

	public static VoxelSurface GetSurfaceType(XYZ position)
	{
		VoxelSurface result = VoxelSurface.Empty;
		NatureData natureData = NatureRegistry.NatureAt(position);
		switch (natureData.Type)
		{
		case NatureType.boulder:
			result = VoxelSurface.Boulder;
			break;
		case NatureType.iron:
			result = VoxelSurface.Iron;
			break;
		case NatureType.plant:
			result = VoxelSurface.Plants;
			break;
		case NatureType.pyrite:
			result = VoxelSurface.Brimstone;
			break;
		case NatureType.none:
			result = GetBlockInfo(position).Surface;
			break;
		}
		return result;
	}

	public static string GetSurfaceName(VoxelSurface surface)
	{
		return Enum.GetName(typeof(VoxelSurface), surface);
	}

	public static string GetSurfaceName(XYZ position)
	{
		return Enum.GetName(typeof(VoxelSurface), GetSurfaceType(position));
	}

	public static void GetGarnotteValue(XYZ position, ICollection<Factory.AssetKey> resultTypes)
	{
		GetGarnotteValue(position, resultTypes, _ditch);
		_ditch.Clear();
	}

	public static void GetGarnotteValue(XYZ position, ICollection<Factory.AssetKey> resultTypes, ICollection<int> resultAmounts)
	{
		if (IsTerrain(position))
		{
			GetTerrainGarnotteValue(position, resultTypes, resultAmounts);
			return;
		}
		BlockInfo blockInfo = GetBlockInfo(position);
		if (blockInfo.IsNotNull && !blockInfo.ResourceKey.IsEmpty())
		{
			resultTypes.Add(blockInfo.ResourceKey);
			resultAmounts.Add(10);
		}
	}

	private static void GetTerrainGarnotteValue(XYZ position, ICollection<Factory.AssetKey> resultTypes, ICollection<int> resultAmounts)
	{
		_voxelTypes.Clear();
		Engine.GetTerrainResources(Engine.GetVoxelHandle(position), _voxelTypes, resultAmounts);
		foreach (int voxelType in _voxelTypes)
		{
			VoxelTerrainType voxelTerrainType = (VoxelTerrainType)voxelType;
			if (voxelTerrainType == VoxelTerrainType.Internal)
			{
				logger.InfoFormat("Attempting to get garnotte type from internal terrain position. Garnotte generation will use default type for voxel {0}", position);
				resultTypes.Add(RawResources.RawStone);
				continue;
			}
			BlockInfo blockInfo = ((int)voxelTerrainType).BlockInfoIDToBlockInfo();
			if (blockInfo == null)
			{
				logger.ErrorFormat("No matching terrain blockID were found for value '{0}'. Make sure it has been configured in the factory. Garnotte generation will be skipped for voxel {1}", (int)voxelTerrainType, position);
			}
			else if (blockInfo.ResourceKey == null)
			{
				logger.ErrorFormat("No resource key is set for terrain blockID '{0}'. Make sure it has been configured in the factory. Garnotte generation will be skipped for voxel {1}", blockInfo.Key, position);
			}
			else
			{
				resultTypes.Add(blockInfo.ResourceKey);
			}
		}
	}

	public static float ComputeSurfaceHeightFromNiveaux(int niveaux, float xOffset, float zOffset)
	{
		Vector3[] sommets = Cubeur.Sommets;
		ProceduralCube.GetSommetsDessus(niveaux, sommets);
		if (xOffset >= 0f - zOffset)
		{
			Vector3 vector = sommets[0] - sommets[1];
			Vector3 vector2 = sommets[3] - sommets[1];
			return sommets[1].y + (0.5f - xOffset) * vector.y + (0.5f - zOffset) * vector2.y;
		}
		Vector3 vector3 = sommets[3] - sommets[2];
		Vector3 vector4 = sommets[0] - sommets[2];
		return sommets[2].y + (0.5f + xOffset) * vector3.y + (0.5f + zOffset) * vector4.y;
	}

	public static float ComputeSurfaceHeightFromNiveauxWithCenter(int niveaux, float xOffset, float zOffset)
	{
		Vector3[] sommets = Cubeur.Sommets;
		ProceduralCube.GetSommetsDessus(niveaux, sommets);
		int num = ((xOffset >= 0f - zOffset) ? 1 : 0) + ((xOffset >= zOffset) ? 2 : 0);
		float t = 0f;
		float t2 = 0f;
		int num2 = 0;
		int num3 = 0;
		switch (num)
		{
		case 0:
			t = 0.5f + zOffset;
			t2 = -2f * xOffset;
			num2 = 2;
			num3 = 0;
			break;
		case 1:
			t = 0.5f + xOffset;
			t2 = 2f * zOffset;
			num2 = 0;
			num3 = 1;
			break;
		case 2:
			t = 0.5f - xOffset;
			t2 = -2f * zOffset;
			num2 = 3;
			num3 = 2;
			break;
		case 3:
			t = 0.5f - zOffset;
			t2 = 2f * xOffset;
			num2 = 1;
			num3 = 3;
			break;
		}
		return Mathf.Lerp(sommets[8].y, Mathf.Lerp(sommets[num2].y, sommets[num3].y, t), t2);
	}

	public static int GetNiveau(XYZ position)
	{
		using TerrainBlockData terrainBlockData = GetTerrainBlockData(position) as TerrainBlockData;
		return (terrainBlockData == null) ? 3 : Mathf.Clamp(terrainBlockData.TerrainVolume - 1, 0, 3);
	}
}
