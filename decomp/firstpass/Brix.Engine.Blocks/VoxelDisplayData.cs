using UnityEngine;

namespace Brix.Engine.Blocks;

public struct VoxelDisplayData
{
	public string ID;

	public Mesh Mesh;

	public Material[] Mats;

	public Vector2 uvTilingOffset;

	public Matrix4x4 Matrix;

	public Vector3 Position => Matrix * new Vector4(0f, 0f, 0f, 1f);

	public VoxelDisplayData(string id, Mesh mesh, Material[] mats, Matrix4x4 matrix)
	{
		ID = id;
		Mesh = mesh;
		Mats = mats;
		Matrix = matrix;
		uvTilingOffset = Vector2.zero;
	}

	public VoxelDisplayData(string id, Mesh mesh, Material[] mats, Vector3 position, Quaternion rotation)
		: this(id, mesh, mats, Matrix4x4.TRS(position, rotation, Vector3.one))
	{
	}
}
