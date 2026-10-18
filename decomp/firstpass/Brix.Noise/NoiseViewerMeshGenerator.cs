using System;
using System.Collections.Generic;
using Brix.Engine.FlatWorld;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.Noise;

public class NoiseViewerMeshGenerator
{
	private static BrixLogger logger = new BrixLogger(typeof(NoiseViewerMeshGenerator).FullName);

	public Queue<MeshData> MeshDataQueue = new Queue<MeshData>();

	public Dictionary<VoxelViewerTypes, int> TypeStats = new Dictionary<VoxelViewerTypes, int>();

	private uint megaVoxelSize;

	private Dictionary<Vector3, VoxelViewerTypes[,,]> NoiseViewerVoxelData = new Dictionary<Vector3, VoxelViewerTypes[,,]>();

	private List<VoxelViewerTypes> voxelTypesCache;

	private bool fillEmpty;

	private VoxelViewerTypes _emptyViewerType = new VoxelViewerTypes();

	private Texture2D heightMap;

	private TexColors heightMapTex = new TexColors();

	public void SetFillEmpty(bool value)
	{
		fillEmpty = value;
	}

	public void SetFillColor(Color32 color)
	{
		_emptyViewerType.Color = color;
	}

	public void SetMegaVoxelSize(uint size)
	{
		megaVoxelSize = size;
	}

	public void SetVoxelTypes(List<VoxelViewerTypes> types)
	{
		voxelTypesCache = types;
	}

	public void RefreshNoises()
	{
		foreach (VoxelViewerTypes item in voxelTypesCache)
		{
			item.NoiseComponent.Refresh();
		}
	}

	public void SetHeightMap(Texture2D heightMap)
	{
		this.heightMap = heightMap;
		Tex tex = new Tex();
		tex.LoadPNG(this.heightMap.EncodeToPNG());
		heightMapTex.CopyFrom(tex);
		Tex.Destroy(tex);
	}

	public void GenerateMegaVoxel(Vector3 position)
	{
		_generateMegaVoxel(position);
	}

	private void _generateMegaVoxel(Vector3 position)
	{
		try
		{
			if (_generateVoxelData(position))
			{
				QueueMeshData(_generateMesh(position));
			}
		}
		catch (Exception exception)
		{
			logger.Error(exception, "Error generating megavoxel in noise viewer");
		}
	}

	public VoxelViewerTypes DetermineVoxelType(float x, float y, float z)
	{
		return DetermineVoxelType(new Vector3(x, y, z));
	}

	private bool isPositionEmptyInHeightMap(Vector3 position)
	{
		if ((float)(int)heightMapTex.GetPixel((int)position.x, (int)position.z).r > position.y || (float)(int)heightMapTex.GetPixel((int)position.x, (int)position.z).g < position.y)
		{
			return true;
		}
		return false;
	}

	private float GetDistanceFromTop(Vector3 position)
	{
		return (float)(int)heightMapTex.GetPixel((int)position.x, (int)position.z).g - position.y;
	}

	private float GetDistanceFromBottom(Vector3 position)
	{
		return (float)(int)heightMapTex.GetPixel((int)position.x, (int)position.z).r + position.y;
	}

	private VoxelViewerTypes GetTypeForVoxel(Vector3 voxelPos)
	{
		double num = 0.0;
		float num2 = 112f - voxelPos.y;
		if (heightMap != null && isPositionEmptyInHeightMap(voxelPos))
		{
			return null;
		}
		if (heightMap != null)
		{
			num2 = GetDistanceFromTop(voxelPos);
		}
		foreach (VoxelViewerTypes item in voxelTypesCache)
		{
			num = item.NoiseComponent.GetValue(voxelPos);
			float threshold = item.Threshold;
			if ((!item.UseRange || (num2 >= (float)item.Start && num2 <= (float)item.End)) && num > (double)threshold)
			{
				return item;
			}
		}
		return GetEmptyType();
	}

	private VoxelViewerTypes GetEmptyType()
	{
		return (!fillEmpty) ? null : _emptyViewerType;
	}

	public VoxelViewerTypes DetermineVoxelType(Vector3 voxelPos)
	{
		VoxelViewerTypes voxelViewerTypes = null;
		return GetTypeForVoxel(voxelPos);
	}

	public VoxelViewerTypes GetVoxelType(Vector3 megaVoxelPos, float x, float y, float z)
	{
		return GetVoxelType(megaVoxelPos, new Vector3(x, y, z));
	}

	public VoxelViewerTypes GetVoxelType(Vector3 megaVoxelPos, Vector3 voxelPos)
	{
		if (voxelPos.x < 0f || voxelPos.y < 0f || voxelPos.z < 0f || voxelPos.x >= (float)megaVoxelSize || voxelPos.y >= (float)megaVoxelSize || voxelPos.z >= (float)megaVoxelSize)
		{
			return null;
		}
		NoiseViewerVoxelData.TryGetValue(megaVoxelPos, out var value);
		if (value != null)
		{
			return value[(int)voxelPos.x, (int)voxelPos.y, (int)voxelPos.z];
		}
		return null;
	}

	private bool _generateVoxelData(Vector3 position)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		bool flag = false;
		VoxelViewerTypes[,,] array = new VoxelViewerTypes[megaVoxelSize, megaVoxelSize, megaVoxelSize];
		for (num = 0f; num < (float)megaVoxelSize; num += 1f)
		{
			for (num2 = 0f; num2 < (float)megaVoxelSize; num2 += 1f)
			{
				if (heightMap != null && heightMapTex.GetPixel((int)position.x + (int)num, (int)position.z + (int)num2).b == 0)
				{
					continue;
				}
				for (num3 = 0f; num3 < (float)megaVoxelSize; num3 += 1f)
				{
					if ((array[(int)num, (int)num3, (int)num2] = DetermineVoxelType(position + new Vector3(num, num3, num2))) != null)
					{
						flag = true;
					}
				}
			}
		}
		if (flag)
		{
			SetVoxelDataForPosition(position, array);
		}
		return flag;
	}

	private void SetVoxelDataForPosition(Vector3 position, VoxelViewerTypes[,,] data)
	{
		lock (NoiseViewerVoxelData)
		{
			NoiseViewerVoxelData[position] = data;
		}
	}

	private MeshData _generateMesh(Vector3 position)
	{
		int num = 1;
		MeshData meshData = new MeshData();
		try
		{
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			for (num3 = 0f; num3 < (float)megaVoxelSize; num3 += 1f)
			{
				for (num4 = 0f; num4 < (float)megaVoxelSize; num4 += 1f)
				{
					for (num2 = 0f; num2 < (float)megaVoxelSize; num2 += 1f)
					{
						Color32 color = Color.white;
						VoxelViewerTypes voxelType = GetVoxelType(position, new Vector3(num2, num4, num3));
						if (voxelType != null)
						{
							if (!meshData.VoxelCount.ContainsKey(voxelType))
							{
								meshData.VoxelCount[voxelType] = 0;
							}
							meshData.VoxelCount[voxelType]++;
							color = voxelType.Color;
							if (GetVoxelType(position, num2, num4 + 1f, num3) == null)
							{
								meshData.Colors.Add(color);
								meshData.Colors.Add(color);
								meshData.Colors.Add(color);
								meshData.Colors.Add(color);
								AddQuadTriangles(meshData, flipVisibleFace: false);
								meshData.Vertices.Add(new Vector3(num2 * (float)num, num4 * (float)num + (float)num, num3 * (float)num));
								meshData.Vertices.Add(new Vector3(num2 * (float)num + (float)num, num4 * (float)num + (float)num, num3 * (float)num));
								meshData.Vertices.Add(new Vector3(num2 * (float)num + (float)num, num4 * (float)num + (float)num, num3 * (float)num + (float)num));
								meshData.Vertices.Add(new Vector3(num2 * (float)num, num4 * (float)num + (float)num, num3 * (float)num + (float)num));
							}
							if (GetVoxelType(position, num2, num4 - 1f, num3) == null)
							{
								AddQuadTriangles(meshData, flipVisibleFace: true);
								meshData.Colors.Add(color);
								meshData.Colors.Add(color);
								meshData.Colors.Add(color);
								meshData.Colors.Add(color);
								meshData.Vertices.Add(new Vector3(num2 * (float)num, num4 * (float)num, num3 * (float)num));
								meshData.Vertices.Add(new Vector3(num2 * (float)num + (float)num, num4 * (float)num, num3 * (float)num));
								meshData.Vertices.Add(new Vector3(num2 * (float)num + (float)num, num4 * (float)num, num3 * (float)num + (float)num));
								meshData.Vertices.Add(new Vector3(num2 * (float)num, num4 * (float)num, num3 * (float)num + (float)num));
							}
							if (GetVoxelType(position, num2 - 1f, num4, num3) == null)
							{
								AddQuadTriangles(meshData, flipVisibleFace: true);
								meshData.Colors.Add(color);
								meshData.Colors.Add(color);
								meshData.Colors.Add(color);
								meshData.Colors.Add(color);
								meshData.Vertices.Add(new Vector3(num2 * (float)num, num4 * (float)num, num3 * (float)num));
								meshData.Vertices.Add(new Vector3(num2 * (float)num, num4 * (float)num, num3 * (float)num + (float)num));
								meshData.Vertices.Add(new Vector3(num2 * (float)num, num4 * (float)num + (float)num, num3 * (float)num + (float)num));
								meshData.Vertices.Add(new Vector3(num2 * (float)num, num4 * (float)num + (float)num, num3 * (float)num));
							}
							if (GetVoxelType(position, num2 + 1f, num4, num3) == null)
							{
								AddQuadTriangles(meshData, flipVisibleFace: false);
								meshData.Colors.Add(color);
								meshData.Colors.Add(color);
								meshData.Colors.Add(color);
								meshData.Colors.Add(color);
								meshData.Vertices.Add(new Vector3(num2 * (float)num + (float)num, num4 * (float)num, num3 * (float)num));
								meshData.Vertices.Add(new Vector3(num2 * (float)num + (float)num, num4 * (float)num, num3 * (float)num + (float)num));
								meshData.Vertices.Add(new Vector3(num2 * (float)num + (float)num, num4 * (float)num + (float)num, num3 * (float)num + (float)num));
								meshData.Vertices.Add(new Vector3(num2 * (float)num + (float)num, num4 * (float)num + (float)num, num3 * (float)num));
							}
							if (GetVoxelType(position, num2, num4, num3 - 1f) == null)
							{
								AddQuadTriangles(meshData, flipVisibleFace: false);
								meshData.Colors.Add(color);
								meshData.Colors.Add(color);
								meshData.Colors.Add(color);
								meshData.Colors.Add(color);
								meshData.Vertices.Add(new Vector3(num2 * (float)num, num4 * (float)num, num3 * (float)num));
								meshData.Vertices.Add(new Vector3(num2 * (float)num + (float)num, num4 * (float)num, num3 * (float)num));
								meshData.Vertices.Add(new Vector3(num2 * (float)num + (float)num, num4 * (float)num + (float)num, num3 * (float)num));
								meshData.Vertices.Add(new Vector3(num2 * (float)num, num4 * (float)num + (float)num, num3 * (float)num));
							}
							if (GetVoxelType(position, num2, num4, num3 + 1f) == null)
							{
								AddQuadTriangles(meshData, flipVisibleFace: true);
								meshData.Colors.Add(color);
								meshData.Colors.Add(color);
								meshData.Colors.Add(color);
								meshData.Colors.Add(color);
								meshData.Vertices.Add(new Vector3(num2 * (float)num, num4 * (float)num, num3 * (float)num + (float)num));
								meshData.Vertices.Add(new Vector3(num2 * (float)num + (float)num, num4 * (float)num, num3 * (float)num + (float)num));
								meshData.Vertices.Add(new Vector3(num2 * (float)num + (float)num, num4 * (float)num + (float)num, num3 * (float)num + (float)num));
								meshData.Vertices.Add(new Vector3(num2 * (float)num, num4 * (float)num + (float)num, num3 * (float)num + (float)num));
							}
						}
					}
				}
			}
			meshData.Position = position;
			return meshData;
		}
		catch (Exception exception)
		{
			logger.Error(exception, "Error generating mesh data");
			return null;
		}
	}

	private void QueueMeshData(MeshData data)
	{
		lock (MeshDataQueue)
		{
			MeshDataQueue.Enqueue(data);
		}
	}

	private void AddQuadTriangles(MeshData meshData, bool flipVisibleFace)
	{
		Vector3 vector = default(Vector3);
		Vector3 vector2 = default(Vector3);
		vector2.x = 1f;
		vector2.y = 3f;
		vector2.z = 2f;
		Vector3 vector3 = default(Vector3);
		vector3.x = 0f;
		vector3.y = 3f;
		vector3.z = 1f;
		if (flipVisibleFace)
		{
			vector = vector2;
			vector2.y = vector2.z;
			vector2.z = vector.y;
			vector = vector3;
			vector3.y = vector3.z;
			vector3.z = vector.y;
		}
		meshData.Triangles.Add(meshData.Vertices.Count + (int)vector2.x);
		meshData.Triangles.Add(meshData.Vertices.Count + (int)vector2.y);
		meshData.Triangles.Add(meshData.Vertices.Count + (int)vector2.z);
		meshData.Triangles.Add(meshData.Vertices.Count + (int)vector3.x);
		meshData.Triangles.Add(meshData.Vertices.Count + (int)vector3.y);
		meshData.Triangles.Add(meshData.Vertices.Count + (int)vector3.z);
	}
}
