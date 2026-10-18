using System.Collections.Generic;
using UnityEngine;

namespace Brix.Noise;

public class MeshData
{
	public List<Vector3> Vertices;

	public List<Vector3> Normals;

	public List<int> Triangles;

	public List<Vector2> Uvs;

	public List<Color32> Colors;

	public Vector3 Position;

	public Dictionary<VoxelViewerTypes, int> VoxelCount = new Dictionary<VoxelViewerTypes, int>();

	public MeshData()
	{
		Vertices = new List<Vector3>();
		Normals = new List<Vector3>();
		Triangles = new List<int>();
		Uvs = new List<Vector2>();
		Colors = new List<Color32>();
	}
}
