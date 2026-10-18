using System;
using UnityEngine;

namespace Brix.Engine.Geometrie;

[Serializable]
public class VoxelizedMesh
{
	[Serializable]
	public class VoxelData
	{
		public XYZ position;

		public FaceData[] faces;

		public Mesh[] partialMeshes = new Mesh[64];
	}

	[Serializable]
	public class FaceData
	{
		public FaceDirection direction;

		public FaceType type;

		public int[] triangles;
	}

	public enum FaceDirection
	{
		Up,
		Forward,
		Back,
		Right,
		Left,
		Down,
		None
	}

	public enum FaceType
	{
		None,
		Full,
		FlipRotationSymmetry,
		RotationSymmetry,
		FlipSymmetry,
		HorizontalFlipSymmetry,
		VerticalFlipSymmetry
	}

	public Mesh originalMesh;

	public Vector3 pivot;

	public VoxelData[] voxels;
}
