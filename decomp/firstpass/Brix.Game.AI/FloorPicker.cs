using System.Collections.Generic;
using Brix.Engine;
using Brix.Engine.Nature;
using Brix.Game.AI.Pathfinding;
using Brix.Game.Components.Volumes;
using Brix.Pathfinding.Test;
using Brix.Util;
using UnityEngine;

namespace Brix.Game.AI;

public static class FloorPicker
{
	private const VPropertyMask FloorPositionBlockers = VPropertyMask.obstacle;

	private const VPropertyMask FloorPositionBlockersAndCharacters = VPropertyMask.obstacle | VPropertyMask.character;

	public static float GetAdjustedHeight(float floorHeight, float floorOffset, int roundedHeight)
	{
		return (float)roundedHeight + (floorHeight - (float)roundedHeight) * (1f - floorOffset);
	}

	public static float GetHeight(Vector3 origin, float floorOffset)
	{
		_GetHeight(origin, floorOffset, out var height);
		return height;
	}

	private static bool _GetHeight(Vector3 origin, float floorOffset, out float height, bool underOnly = false, int maxDist = 20)
	{
		XYZ xYZ = XYZ.FromVector3(origin);
		for (int num = 0; num <= maxDist; num = ((!underOnly) ? (-num + ((num <= 0) ? 1 : 0)) : (num + 1)))
		{
			XYZ xYZ2 = xYZ - XYZ.up * num;
			if (IsOpen(xYZ2) && !IsOpen(xYZ2 - XYZ.up))
			{
				WalkablePathNode walkablePathNode = PathNodeRegistry.At(xYZ2);
				if (walkablePathNode != null && !walkablePathNode.IsEmpty)
				{
					height = GetAdjustedHeight(walkablePathNode.GetSurfaceHeight(origin + Vector3.up * num), floorOffset, walkablePathNode.RepresentativePosition.y);
					return true;
				}
				break;
			}
		}
		height = origin.y;
		return false;
	}

	public static bool GetNearestAligned(Vector3 origin, float floorOffset, out Vector3 aligned)
	{
		return _GetNearestAligned(origin, floorOffset, underOnly: false, 1, out aligned);
	}

	public static bool GetNearestAlignedUnder(Vector3 origin, float floorOffset, int maxDist, out Vector3 aligned)
	{
		return _GetNearestAligned(origin, floorOffset, underOnly: true, maxDist, out aligned);
	}

	private static bool _GetNearestAligned(Vector3 origin, float floorOffset, bool underOnly, int maxDist, out Vector3 aligned)
	{
		aligned = Calcule.RoundVecteur(origin);
		return _GetHeight(aligned, floorOffset, out aligned.y, underOnly, maxDist);
	}

	public static bool IsGrounded(Vector3 origin)
	{
		if (!_GetHeight(origin, 0f, out var height))
		{
			return false;
		}
		if (origin.y - height >= 1f)
		{
			return false;
		}
		if (IsOnOrInsideGround(origin))
		{
			return true;
		}
		return false;
	}

	public static bool IsOccupiedOrInsideGround(GameObject exceptThis, Vector3 origin, bool avoidCharacterCollisions)
	{
		XYZ xYZ = XYZ.FromVector3(origin);
		return !IsOpen(xYZ) || IsOccupied(exceptThis, xYZ, avoidCharacterCollisions);
	}

	public static bool IsOccupied(GameObject exceptThis, XYZ position, bool avoidCharacterCollisions)
	{
		VPropertyMask p = (VPropertyMask)((!avoidCharacterCollisions) ? 2 : 1026);
		return VolumeRegistry.HasVolumesWithPropertyAt(position, p, CanPassThrough, exceptThis);
	}

	private static bool CanPassThrough(IVolume volume, GameObject except)
	{
		return volume.GameObject != except && !volume.PassThrough.CanPassThrough(except);
	}

	private static bool IsOnOrInsideGround(Vector3 origin)
	{
		XYZ xYZ = XYZ.FromVector3(origin + Vector3.up / 2f);
		return Voxel.IsNonEmpty(xYZ) || Voxel.IsNonEmpty(xYZ - XYZ.up) || PathNodeRegistry.At(xYZ) != null;
	}

	public static Vector3 GetNearestFreePosition(GameObject originGO, Vector3 origin, float floorOffset, bool avoidCharacterCollisions, int maxReach)
	{
		XYZ[] array = new XYZ[9]
		{
			XYZ.zero,
			XYZ.forward,
			-XYZ.forward,
			XYZ.right,
			-XYZ.right,
			XYZ.forward + XYZ.right,
			XYZ.forward - XYZ.right,
			-XYZ.forward + XYZ.right,
			-XYZ.forward - XYZ.right
		};
		XYZ xYZ = XYZ.FromVector3(origin);
		for (int i = 1; i <= maxReach; i++)
		{
			XYZ[] array2 = array;
			foreach (XYZ xYZ2 in array2)
			{
				XYZ voxel = xYZ + xYZ2 * i;
				WalkablePathNode walkablePathNode = PathNodeRegistry.WalkableAt(voxel, i);
				if (walkablePathNode != null)
				{
					Vector3 vector = voxel.ToVector3();
					vector.y = GetAdjustedHeight(walkablePathNode.GetSurfaceHeight(vector), floorOffset, walkablePathNode.RepresentativePosition.y);
					if (!IsOccupiedOrInsideGround(originGO, vector, avoidCharacterCollisions))
					{
						return vector;
					}
				}
			}
		}
		return origin;
	}

	private static bool IsOpen(XYZ voxel)
	{
		return Voxel.IsOpen(voxel) && !NatureRegistry.HasNatureBlockerAt(voxel);
	}

	private static IEnumerable<Vector3> GetCandidatePositions(Vector3 origin)
	{
		yield return origin;
		yield return origin + Vector3.up;
		yield return origin - Vector3.up;
		yield return origin + Vector3.up * 2f;
	}
}
