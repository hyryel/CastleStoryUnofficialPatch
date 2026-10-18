using System;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.Engine;

public static class VoxelEngine
{
	private static BrixLogger logger = new BrixLogger(typeof(VoxelEngine).FullName);

	public static VoxelEngineImpl Impl;

	public const VoxelTerrainType DefaultVoxelType = VoxelTerrainType.Sand;

	private static bool loaded = false;

	public static bool Ready => Impl != null;

	public static bool Working => Impl.IsWorking();

	public static void Terminer()
	{
		Impl = null;
		loaded = false;
	}

	public static float GetVolume(Vector3 p)
	{
		return GetVolume(XYZ.FromVector3(p));
	}

	public static float GetVolume(XYZ p)
	{
		return (float)GetNiveau(p) / 3f;
	}

	public static bool IsEmpty(Vector3 p)
	{
		return IsEmpty(XYZ.FromVector3(p));
	}

	public static bool IsEmpty(XYZ p)
	{
		if (Impl == null)
		{
			return true;
		}
		int columnTop = Impl.GetColumnTop(p.x, p.z);
		int columnBottom = Impl.GetColumnBottom(p.x, p.z);
		return p.y < columnBottom || p.y >= columnTop;
	}

	public static int GetNiveau(XYZ p)
	{
		if (Impl == null)
		{
			return 0;
		}
		int columnTop = Impl.GetColumnTop(p.x, p.z);
		int columnBottom = Impl.GetColumnBottom(p.x, p.z);
		if (p.y < columnBottom || p.y >= columnTop)
		{
			return 0;
		}
		if (p.y >= columnBottom && p.y < columnTop - 1)
		{
			return 3;
		}
		return Mathf.Clamp(Impl.GetColumnCrustVolume(p.x, p.z) - 1, 0, 3);
	}

	public static void SetColumnType(int x, int z, VoxelTerrainType type)
	{
		Impl.SetColumnCrustType(x, z, (int)type);
	}

	public static void SetColumnTopVoxel(int x, int z, int top)
	{
		Impl.SetColumnTop(x, z, top);
	}

	public static void SetColumnCrustVolume(int x, int z, float crustVolume)
	{
		int volume = Mathf.Clamp(Mathf.RoundToInt(crustVolume * 3f) + 1, 1, 4);
		Impl.SetColumnCrustVolume(x, z, volume);
	}

	public static int SetColumnTop(int x, int z, float height)
	{
		int num = Mathf.FloorToInt(height);
		height -= (float)num;
		int volume = Mathf.Clamp(Mathf.RoundToInt(height * 3f) + 1, 1, 4);
		Impl.SetColumnTop(x, z, num);
		Impl.SetColumnCrustVolume(x, z, volume);
		return num;
	}

	public static void SetColumnBottom(int x, int z, int height)
	{
		Impl.SetColumnBottom(x, z, height);
	}

	public static int GetColumnBottom(int x, int z)
	{
		return Impl.GetColumnBottom(x, z);
	}

	public static int GetColumnTop(int x, int z)
	{
		return Impl.GetColumnTop(x, z);
	}

	public static VoxelTerrainType GetColumnType(int x, int z)
	{
		return (VoxelTerrainType)Impl.GetColumnCrustType(x, z);
	}

	public static void GetColumnData(int x, int z, out int bottom, out int top)
	{
		Impl.GetColumnData(x, z, out bottom, out top, out var _, out var _);
	}

	public static void GetColumnData(int x, int z, out int bottom, out int top, out VoxelTerrainType outType)
	{
		Impl.GetColumnData(x, z, out bottom, out top, out var type, out var _);
		outType = (VoxelTerrainType)type;
	}

	public static void GetColumnData(int x, int z, out int bottom, out int top, out VoxelTerrainType outType, out float crustVolume)
	{
		Impl.GetColumnData(x, z, out bottom, out top, out var type, out var volume);
		outType = (VoxelTerrainType)type;
		crustVolume = (float)Mathf.Clamp(volume - 1, 0, 3) / 3f;
	}

	public static int AddColumnHeight(int x, int z, float amount)
	{
		int columnTop = Impl.GetColumnTop(x, z);
		int columnBottom = Impl.GetColumnBottom(x, z);
		if (columnBottom >= columnTop)
		{
			return 0;
		}
		int columnCrustVolume = Impl.GetColumnCrustVolume(x, z);
		amount += (float)Mathf.Clamp(columnCrustVolume - 1, 0, 3) / 3f;
		return SetColumnTop(x, z, (float)columnTop + amount);
	}

	public static void RemoveColumnTop(int x, int z)
	{
		int columnTop = Impl.GetColumnTop(x, z);
		if (columnTop != 255)
		{
			Impl.SetColumnTop(x, z, columnTop - 1);
		}
	}

	public static void RemoveColumn(int x, int z)
	{
		Impl.SetColumnBottom(x, z, 255);
		Impl.SetColumnTop(x, z, 255);
	}

	public static bool IsEmptyColumn(int x, int z)
	{
		GetColumnData(x, z, out var bottom, out var top);
		return bottom >= top;
	}

	public static void Load(string folder)
	{
		if (Impl != null)
		{
			Impl.LoadMap(folder);
			loaded = true;
		}
	}

	public static void Save(string folder)
	{
		if (Impl != null)
		{
			Impl.SaveMap(folder);
		}
	}

	public static void StartRendering()
	{
		if (Impl != null)
		{
			if (!loaded)
			{
				throw new InvalidOperationException();
			}
			Impl.RenderMap();
		}
	}

	public static void StopRendering()
	{
		if (Impl != null)
		{
			Impl.ClearMap();
		}
	}

	public static void BeginChanges()
	{
		if (Impl != null)
		{
			Impl.BeginChanges();
		}
	}

	public static void ApplyChanges(bool removeUnusedVoxels = false)
	{
		if (Impl != null)
		{
			if (!loaded)
			{
				logger.Warn("VoxelEngine.ApplyChanges() : map data isn't loaded yet! Ignoring request.");
			}
			else
			{
				Impl.ApplyChanges();
			}
		}
	}
}
