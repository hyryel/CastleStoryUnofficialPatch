using System;
using System.Collections.Generic;
using System.Linq;
using Brix.Audio;
using Brix.Engine;
using Brix.Engine.Nature;
using Brix.Legacy;
using Brix.Util;
using Brix.Utils.Locomotion;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Input.WorldEditor;

public static class BrushUtils
{
	private static readonly XYZ[] CheckedVoxels = new XYZ[9];

	internal static readonly XZ[] NeighborDeltas = Calcule.VolumeBetween(new XYZ(-1, 0, -1), new XYZ(1, 0, 1)).Select(XZ.FromXYZ).ToArray();

	internal static void AddTree(XYZ at)
	{
		TreeRegistry.Instance.Add(at);
	}

	internal static void RemoveTree(XYZ at)
	{
		TreeRegistry.Instance.Remove(at);
	}

	internal static bool GrowTree(XYZ at, float fraction)
	{
		int tree = TreeRegistry.Instance.GetTree(at);
		int num = (tree >> 4) & 0xF;
		int num2 = num + Mathf.RoundToInt(fraction * 15f);
		if (num2 == num && tree != 0)
		{
			return false;
		}
		if (num2 >= 0)
		{
			if (tree == 0)
			{
				TreeRegistry.Instance.Add(at);
				tree = TreeRegistry.Instance.GetTree(at);
				SoundEngine.Play("WorldEditor_Terrain_Tree_Spawn");
			}
			tree = (tree & 0xF) | (Mathf.Clamp(num2, 0, 15) << 4);
			TreeRegistry.Instance.ModifyTree(at, tree);
			return true;
		}
		return false;
	}

	internal static bool UngrowTree(XYZ at, float fraction)
	{
		int tree = TreeRegistry.Instance.GetTree(at);
		int num = (tree >> 4) & 0xF;
		int num2 = num + Mathf.RoundToInt((0f - fraction) * 15f);
		if (tree == 0)
		{
			return false;
		}
		if (num2 > 0 && num2 != num)
		{
			tree = (tree & 0xF) | (Mathf.Clamp(num2, 0, 15) << 4);
			TreeRegistry.Instance.ModifyTree(at, tree);
			return true;
		}
		if (num2 <= 1)
		{
			RemoveTree(at);
			SoundEngine.Play("WorldEditor_Terrain_Tree_Removed");
		}
		return false;
	}

	internal static bool ValidateGrass(XYZ centerXYZ, Dictionary<XZ, int> modifiedCrustPositions = null)
	{
		if (BoulderRegistry.HasBoulders(centerXYZ) || IronRegistry.HasMinerals(centerXYZ) || PyriteRegistry.HasMinerals(centerXYZ))
		{
			return false;
		}
		return _ValidateGrass(centerXYZ, modifiedCrustPositions);
	}

	private static bool _ValidateGrass(XYZ centerXYZ, Dictionary<XZ, int> modifiedCrustPositions)
	{
		int y = centerXYZ.y;
		int y2 = centerXYZ.y;
		int num = 0;
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				XZ column = new XZ(centerXYZ.x + i, centerXYZ.z + j);
				XYZ xYZ = ((modifiedCrustPositions == null) ? GetCrustInColumn(column.x, column.z) : ManualGetCrustInColumn(column, modifiedCrustPositions));
				if (xYZ == XYZ.zero || TreeRegistry.Instance.HasTree(xYZ))
				{
					return false;
				}
				if (xYZ.y > y)
				{
					y = xYZ.y;
				}
				if (xYZ.y < y2)
				{
					y2 = xYZ.y;
				}
				ref XYZ reference = ref CheckedVoxels[num++];
				reference = new XYZ(i, xYZ.y - centerXYZ.y, j);
			}
		}
		if (Math.Abs(y - y2) < 2)
		{
			return true;
		}
		if (!ValidateGrassCorners(CheckedVoxels))
		{
			return false;
		}
		return true;
	}

	internal static bool ValidateGrassCorners(XYZ[] voxels)
	{
		for (int i = 0; i < voxels.Length; i++)
		{
			XYZ xYZ = voxels[i];
			if (xYZ.Flatten().Manhattan() != 2)
			{
				continue;
			}
			int y = xYZ.y;
			int y2 = xYZ.y;
			for (int j = 0; j < voxels.Length; j++)
			{
				XYZ xYZ2 = voxels[j];
				if (xYZ2.Flatten().Manhattan() != 2 && (xYZ.x == xYZ2.x || xYZ.z == xYZ2.z || xYZ2.x + xYZ2.z == 0))
				{
					if (xYZ2.y > y)
					{
						y = xYZ2.y;
					}
					if (xYZ2.y < y2)
					{
						y2 = xYZ2.y;
					}
				}
			}
			if (Math.Abs(y - y2) >= 2)
			{
				return false;
			}
		}
		return true;
	}

	internal static void FixGrass(XYZ at, Dictionary<XZ, int> modifiedCrustPositions = null, VoxelTerrainType sandType = VoxelTerrainType.Sand)
	{
		if (ManualIsCrustAndOverlay(at, modifiedCrustPositions) && !ValidateGrass(at, modifiedCrustPositions))
		{
			VoxelEngine.SetColumnType(at.x, at.z, sandType);
		}
	}

	internal static void AddOrFixGrass(XYZ at, Dictionary<XZ, int> modifiedCrustPositions = null, VoxelTerrainType sandType = VoxelTerrainType.Sand, VoxelTerrainType grassType = VoxelTerrainType.Grass)
	{
		if (ManualIsCrust(at, modifiedCrustPositions))
		{
			if (ValidateGrass(at, modifiedCrustPositions))
			{
				VoxelEngine.SetColumnType(at.x, at.z, grassType);
			}
			else
			{
				VoxelEngine.SetColumnType(at.x, at.z, sandType);
			}
		}
	}

	internal static void RemoveGrass(XYZ at)
	{
		if (Voxel.IsCrust(at))
		{
			VoxelEngine.SetColumnType(at.x, at.z, VoxelTerrainType.Sand);
		}
	}

	internal static XYZ GetCrustInColumn(int x, int z)
	{
		XYZ xYZ = new XYZ(x, VoxelEngine.GetColumnTop(x, z) - 1, z);
		return (!Voxel.IsCrust(xYZ)) ? XYZ.zero : xYZ;
	}

	internal static XYZ GetTerrainTopInColumn(int x, int z)
	{
		XYZ xYZ = new XYZ(x, VoxelEngine.GetColumnTop(x, z) - 1, z);
		return (!Voxel.IsTerrain(xYZ)) ? XYZ.zero : xYZ;
	}

	internal static XYZ ManualGetCrustInColumn(XZ column, Dictionary<XZ, int> modifiedCrustPositions)
	{
		if (modifiedCrustPositions.TryGetValue(column, out var value))
		{
			return new XYZ(column.x, value, column.z);
		}
		return GetCrustInColumn(column.x, column.z);
	}

	internal static XYZ ManualGetTerrainTopInColumn(XZ column, Dictionary<XZ, int> modifiedCrustPositions)
	{
		if (modifiedCrustPositions.TryGetValue(column, out var value))
		{
			return new XYZ(column.x, value, column.z);
		}
		return GetTerrainTopInColumn(column.x, column.z);
	}

	internal static bool IsModifiedCrust(XYZ at, Dictionary<XZ, int> modifiedCrustPositions)
	{
		if (modifiedCrustPositions == null)
		{
			return false;
		}
		XZ key = XZ.FromXYZ(at);
		int value;
		return modifiedCrustPositions.TryGetValue(key, out value) && value == at.y;
	}

	internal static bool ManualIsCrust(XYZ at, Dictionary<XZ, int> modifiedCrustPositions)
	{
		return IsModifiedCrust(at, modifiedCrustPositions) || Voxel.IsCrust(at);
	}

	internal static bool ManualIsCrustAndOverlay(XYZ at, Dictionary<XZ, int> modifiedCrustPositions)
	{
		return IsModifiedCrust(at, modifiedCrustPositions) || (Voxel.IsCrust(at) && Voxel.GetBlockInfo(at).IsTerrainOverlay);
	}

	internal static void PopulateAdjacentColumns(Dictionary<XZ, int> changedColumns, HashSet<XZ> resultColumns)
	{
		foreach (KeyValuePair<XZ, int> changedColumn in changedColumns)
		{
			for (int i = 0; i < 9; i++)
			{
				resultColumns.Add(changedColumn.Key + NeighborDeltas[i]);
			}
		}
	}

	internal static bool IsAdjacentToChangedColumn(XZ column, Dictionary<XZ, int> changedColumns)
	{
		return changedColumns.Slinq().Any((KeyValuePair<XZ, int> kv, XZ col) => IsAdjacent(kv.Key, col), column);
	}

	private static bool IsAdjacent(XZ a, XZ b)
	{
		return Math.Abs(a.x - b.x) <= 1 && Math.Abs(a.z - b.z) <= 1;
	}

	internal static void BeginChanges(VoxelDirtyProperties properties = VoxelDirtyProperties.Terrain)
	{
		if (properties.HasFlag(VoxelDirtyProperties.Terrain))
		{
			VoxelEngine.BeginChanges();
		}
	}

	internal static void ApplyChanges(VoxelDirtyProperties properties = VoxelDirtyProperties.Terrain)
	{
		if (properties.HasFlag(VoxelDirtyProperties.Terrain))
		{
			VoxelEngine.ApplyChanges();
		}
	}

	public static bool IsInsideSphere(Vector3 center, Vector3 position, float radius)
	{
		return (position - center).sqrMagnitude <= radius * radius;
	}

	public static bool IsInsideCylinder(Vector3 center, Vector3 position, float radius)
	{
		return (position - center).Flatten().sqrMagnitude <= radius * radius;
	}

	public static bool IsInsideBox(Vector3 center, Vector3 position, float radius)
	{
		Vector3 vector = position - center;
		return Mathf.Abs(vector.x) <= radius && Mathf.Abs(vector.y) <= radius && Mathf.Abs(vector.z) <= radius;
	}

	public static bool IsInsideSquare(Vector3 center, Vector3 position, float radius)
	{
		Vector3 vector = (position - center).Flatten();
		return Mathf.Abs(vector.x) <= radius && Mathf.Abs(vector.z) <= radius;
	}

	public static bool IsInsideRange(Vector3 position, Vector3 rangeCenter, int areaSize, bool unlimitedInY)
	{
		if (areaSize <= 3)
		{
			if (unlimitedInY)
			{
				return IsInsideSquare(rangeCenter, position, (float)areaSize / 2f);
			}
			return IsInsideBox(rangeCenter, position, (float)areaSize / 2f);
		}
		if (unlimitedInY)
		{
			return IsInsideCylinder(rangeCenter, position, (float)areaSize / 2f);
		}
		return IsInsideSphere(rangeCenter, position, (float)areaSize / 2f);
	}

	public static float GetFalloffFactorAt(Vector3 position, AnimationCurve curve, Vector3 center, float areaSize)
	{
		if (areaSize > 3f && curve != null)
		{
			float num = Mathf.Abs((center - position).magnitude);
			float num2 = areaSize / 2f;
			return curve.Evaluate(num / num2);
		}
		return 1f;
	}

	public static Vector3 GetSnappedCenterPosition(Vector3 position, int areaSize)
	{
		Vector3 vector = Calcule.RoundVecteur(position);
		Vector3 vector2 = position - vector;
		if (areaSize % 2 == 1)
		{
			return vector;
		}
		if (vector2.x < 0f)
		{
			vector.x -= 0.5f;
		}
		else
		{
			vector.x += 0.5f;
		}
		if (vector2.z < 0f)
		{
			vector.z -= 0.5f;
		}
		else
		{
			vector.z += 0.5f;
		}
		return vector;
	}
}
