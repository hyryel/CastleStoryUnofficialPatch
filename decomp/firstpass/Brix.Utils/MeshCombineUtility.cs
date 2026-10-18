using System.Collections.Generic;
using UnityEngine;

namespace Brix.Utils;

public class MeshCombineUtility
{
	public struct MeshInstance
	{
		public Mesh mesh;

		public int subMeshIndex;

		public Matrix4x4 transform;
	}

	public static Mesh Combine(List<MeshInstance> combines, bool generateStrips)
	{
		int num = 0;
		foreach (MeshInstance combine in combines)
		{
			if ((bool)combine.mesh)
			{
				num += combine.mesh.vertexCount;
			}
		}
		List<Vector3> list = CollectionPool.RequestList<Vector3>();
		List<Vector3> list2 = CollectionPool.RequestList<Vector3>();
		List<Vector4> list3 = CollectionPool.RequestList<Vector4>();
		List<Vector2> list4 = CollectionPool.RequestList<Vector2>();
		List<Vector2> list5 = CollectionPool.RequestList<Vector2>();
		List<Color> list6 = CollectionPool.RequestList<Color>();
		list.Capacity = num;
		list2.Capacity = num;
		list3.Capacity = num;
		list4.Capacity = num;
		list5.Capacity = num;
		list6.Capacity = num;
		foreach (MeshInstance combine2 in combines)
		{
			if ((bool)combine2.mesh)
			{
				List<Vector3> list7 = CollectionPool.RequestList<Vector3>();
				combine2.mesh.GetVertices(list7);
				int count = list7.Count;
				for (int i = 0; i < count; i++)
				{
					list.Add(combine2.transform.MultiplyPoint(list7[i]));
				}
				list7.Dispose();
			}
		}
		foreach (MeshInstance combine3 in combines)
		{
			if ((bool)combine3.mesh)
			{
				Matrix4x4 transpose = combine3.transform.inverse.transpose;
				List<Vector3> list8 = CollectionPool.RequestList<Vector3>();
				combine3.mesh.GetNormals(list8);
				int count2 = list8.Count;
				for (int j = 0; j < count2; j++)
				{
					list2.Add(transpose.MultiplyVector(list8[j]).normalized);
				}
				list8.Dispose();
			}
		}
		foreach (MeshInstance combine4 in combines)
		{
			if ((bool)combine4.mesh)
			{
				Matrix4x4 transpose2 = combine4.transform.inverse.transpose;
				List<Vector4> list9 = CollectionPool.RequestList<Vector4>();
				combine4.mesh.GetTangents(list9);
				int count3 = list9.Count;
				for (int k = 0; k < count3; k++)
				{
					Vector4 vector = list9[k];
					Vector3 v = new Vector3(vector.x, vector.y, vector.z);
					v = transpose2.MultiplyVector(v).normalized;
					list3.Add(new Vector4(v.x, v.y, v.z, vector.w));
				}
				list9.Dispose();
			}
		}
		foreach (MeshInstance combine5 in combines)
		{
			if ((bool)combine5.mesh)
			{
				List<Vector2> list10 = CollectionPool.RequestList<Vector2>();
				combine5.mesh.GetUVs(0, list10);
				int count4 = list10.Count;
				for (int l = 0; l < count4; l++)
				{
					list4.Add(list10[l]);
				}
				list10.Dispose();
			}
		}
		foreach (MeshInstance combine6 in combines)
		{
			if ((bool)combine6.mesh)
			{
				List<Vector2> list11 = CollectionPool.RequestList<Vector2>();
				combine6.mesh.GetUVs(1, list11);
				int count5 = list11.Count;
				for (int m = 0; m < count5; m++)
				{
					list5.Add(list11[m]);
				}
				list11.Dispose();
			}
		}
		foreach (MeshInstance combine7 in combines)
		{
			if ((bool)combine7.mesh)
			{
				List<Color> list12 = CollectionPool.RequestList<Color>();
				combine7.mesh.GetColors(list12);
				int count6 = list12.Count;
				for (int n = 0; n < count6; n++)
				{
					list6.Add(list12[n]);
				}
				list12.Dispose();
			}
		}
		List<int> list13 = CollectionPool.RequestList<int>();
		int num2 = 0;
		foreach (MeshInstance combine8 in combines)
		{
			if ((bool)combine8.mesh)
			{
				List<int> list14 = CollectionPool.RequestList<int>();
				combine8.mesh.GetTriangles(list14, combine8.subMeshIndex);
				int count7 = list14.Count;
				for (int num3 = 0; num3 < count7; num3++)
				{
					list13.Add(list14[num3] + num2);
				}
				num2 += combine8.mesh.vertexCount;
				list14.Dispose();
			}
		}
		Mesh mesh = new Mesh();
		mesh.name = "Combined Mesh";
		mesh.SetVertices(list);
		mesh.SetNormals(list2);
		mesh.SetColors(list6);
		mesh.SetUVs(0, list4);
		mesh.SetUVs(1, list5);
		mesh.SetTangents(list3);
		mesh.SetTriangles(list13, 0);
		list.Dispose();
		list2.Dispose();
		list6.Dispose();
		list4.Dispose();
		list5.Dispose();
		list3.Dispose();
		list13.Dispose();
		return mesh;
	}
}
