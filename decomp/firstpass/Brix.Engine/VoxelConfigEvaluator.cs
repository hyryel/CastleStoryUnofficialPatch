using System;
using Brix.Engine.Geometrie;
using Brix.External.Factories;
using Brix.Legacy;
using Brix.Util;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Engine;

public class VoxelConfigEvaluator
{
	public static int Calculate(XYZ position, bool croute, Func<XYZ, bool> nonVide)
	{
		int num = 0;
		for (int i = 0; i < 6; i++)
		{
			int scalaire = position.ToScalaire() + Cubeur.ScalaireDirs[i];
			if (croute && (num & 1) != 0 && i >= 1 && i <= 4)
			{
				scalaire = GetVoxelTop(XYZ.FromScalaire(scalaire), nonVide).ToScalaire();
			}
			if (!nonVide(XYZ.FromScalaire(scalaire)))
			{
				num |= 1 << i;
			}
		}
		return num;
	}

	public static int Calculate(XYZ position)
	{
		if (Voxel.IsEmpty(position))
		{
			return 0;
		}
		using IVoxel voxel = Voxel.GetVoxel(position);
		if (voxel.IsNull)
		{
			return 0;
		}
		int num = 0;
		if (!HasOccluderAt(voxel.Position, XYZ.up))
		{
			num++;
		}
		if (!HasOccluderAt(voxel.Position, XYZ.forward))
		{
			num += 2;
		}
		if (!HasOccluderAt(voxel.Position, -XYZ.forward))
		{
			num += 4;
		}
		if (!HasOccluderAt(voxel.Position, XYZ.right))
		{
			num += 8;
		}
		if (!HasOccluderAt(voxel.Position, -XYZ.right))
		{
			num += 16;
		}
		if (!HasOccluderAt(voxel.Position, -XYZ.up))
		{
			num += 32;
		}
		return num;
	}

	public static int GetCote(int niveaux, int cote)
	{
		int coin = GetCoin(niveaux, cote);
		int coin2 = GetCoin(niveaux, (cote + 1) & 3);
		return coin + coin2;
	}

	private static int GetCoin(int niveaux, int coin)
	{
		return (niveaux >> (coin << 2)) & 3;
	}

	private static XYZ GetVoxelTop(XYZ position, Func<XYZ, bool> nonVide)
	{
		if (nonVide(position))
		{
			XYZ xYZ = position + XYZ.up;
			return (!nonVide(xYZ) || nonVide(xYZ + XYZ.up)) ? position : xYZ;
		}
		XYZ xYZ2 = position - XYZ.up;
		return (!nonVide(xYZ2)) ? position : xYZ2;
	}

	private static bool HasOccluderAt(XYZ position, XYZ delta)
	{
		XYZ neighborPosition = position + delta;
		IVoxel bd = Voxel.GetVoxel(neighborPosition);
		if (bd.IsNull)
		{
			return false;
		}
		if (bd.IsTerrain)
		{
			return false;
		}
		if (bd.IsMultiBlock)
		{
			return false;
		}
		Factory.AssetKey key = bd.BlockInfo.Key;
		Factory.AssetKey key2 = new Factory.AssetKey("Blocks_MeshDescriptors", key.Name);
		if (!Factory.Exist(key2))
		{
			return false;
		}
		VoxelizedMesh voxelMesh = Factory.Peek(key2).GetComponent<VoxelizedMeshDescriptor>().voxelMesh;
		VoxelizedMesh.VoxelData voxelData = voxelMesh.voxels.Slinq().FirstOrDefault((VoxelizedMesh.VoxelData v) => RotateAndTranslateXYZ(v.position, (bd as IBlockData).AbsolutePivotPosition, bd.Rotation, bd.BlockInfo.CenterOffset) == neighborPosition);
		if (voxelData == null)
		{
			return false;
		}
		VoxelizedMesh.FaceData faceData = voxelData.faces.Slinq().FirstOrDefault((VoxelizedMesh.FaceData f) => GetDelta(f.direction, bd.Rotation) == -delta);
		if (faceData == null)
		{
			return false;
		}
		return faceData.type == VoxelizedMesh.FaceType.Full;
	}

	private static XYZ GetDelta(VoxelizedMesh.FaceDirection direction, Quaternion rotation)
	{
		return (direction >= VoxelizedMesh.FaceDirection.None) ? XYZ.zero : XYZ.FromVector3(rotation * Calcule.FaceIndexToVecteur((int)direction));
	}

	private static XYZ RotateAndTranslateXYZ(XYZ p, Vector3 t, Quaternion r, Vector3 centerOffset)
	{
		Vector3 vector = t + r * centerOffset;
		return XYZ.FromVector3(vector + r * p.ToVector3());
	}
}
