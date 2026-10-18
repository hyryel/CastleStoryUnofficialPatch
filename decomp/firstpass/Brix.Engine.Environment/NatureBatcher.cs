using System;
using Brix.Legacy;
using Brix.Utils.Logging;
using Smooth.Algebraics;
using UnityEngine;

namespace Brix.Engine.Environment;

public class NatureBatcher
{
	private static class Binder<T1, T2>
	{
		public static MeshModifiable _BuildColliderMesh(MeshModifiable modelMesh, int count, Func<int, T1, T2, Matrix4x4> f, T1 t1, T2 t2)
		{
			return BuildColliderMesh(modelMesh, count, (int i, Tuple<T1, T2, Func<int, T1, T2, Matrix4x4>> tuple) => tuple._3(i, tuple._1, tuple._2), Tuple.Create(t1, t2, f));
		}

		public static void _GetWarpTex(ref Texture2D resultTex, int count, Func<int, T1, T2, Color32> f, T1 t1, T2 t2)
		{
			GetWarpTex(ref resultTex, count, (int i, Tuple<T1, T2, Func<int, T1, T2, Color32>> tuple) => tuple._3(i, tuple._1, tuple._2), Tuple.Create(t1, t2, f));
		}
	}

	private static class Binder<T1, T2, T3>
	{
		public static MeshModifiable _BuildColliderMesh(MeshModifiable modelMesh, int count, Func<int, T1, T2, T3, Matrix4x4> f, T1 t1, T2 t2, T3 t3)
		{
			return BuildColliderMesh(modelMesh, count, (int i, Tuple<T1, T2, T3, Func<int, T1, T2, T3, Matrix4x4>> tuple) => tuple._4(i, tuple._1, tuple._2, tuple._3), Tuple.Create(t1, t2, t3, f));
		}

		public static void _GetWarpTex(ref Texture2D resultTex, int count, Func<int, T1, T2, T3, Color32> f, T1 t1, T2 t2, T3 t3)
		{
			GetWarpTex(ref resultTex, count, (int i, Tuple<T1, T2, T3, Func<int, T1, T2, T3, Color32>> tuple) => tuple._4(i, tuple._1, tuple._2, tuple._3), Tuple.Create(t1, t2, t3, f));
		}
	}

	private static BrixLogger logger = new BrixLogger(typeof(NatureBatcher).FullName);

	private const int WarpTexSize = 32;

	private static Color32[] WarpTexColorBuf = new Color32[1024];

	public static void BuildWarpableMesh(MeshAugmentablePrealloue resultMesh, MeshModifiable modelMesh, int startOffset, int count, int texWidth, Matrix4x4 transform)
	{
		Vector3[] vertices = modelMesh.GetVertices();
		Vector3[] normals = modelMesh.GetNormals();
		Vector4[] tangents = modelMesh.GetTangents();
		Vector2[] uv = modelMesh.GetUv();
		int[] triangles = modelMesh.GetTriangles();
		Matrix4x4 transpose = transform.inverse.transpose;
		for (int i = 0; i < count; i++)
		{
			MeshInfo.CopyTriangles(triangles, resultMesh.triangles, resultMesh.nv, resultMesh.nt, triangles.Length);
			MeshInfo.CopyVertices(vertices, resultMesh.vertices, transform, resultMesh.nv, vertices.Length);
			if (resultMesh.normals != null)
			{
				MeshInfo.CopyNormals(normals, resultMesh.normals, transpose, resultMesh.nv, normals.Length);
			}
			if (resultMesh.tangents != null)
			{
				MeshInfo.CopyTangents(tangents, resultMesh.tangents, transpose, resultMesh.nv, tangents.Length);
			}
			if (resultMesh.uv != null)
			{
				MeshInfo.CopyUv(uv, resultMesh.uv, resultMesh.nv, uv.Length);
			}
			int num = startOffset + i;
			MeshInfo.SetUv(new Vector2(num % texWidth, num / texWidth), resultMesh.uv2, resultMesh.nv, vertices.Length);
			resultMesh.nv += vertices.Length;
			resultMesh.nt += triangles.Length;
		}
	}

	public static BuiltinMeshInfo BuildWarpableMesh(MeshModifiable modelMesh, int startOffset, int count, int texWidth, Matrix4x4 transform)
	{
		Vector3[] vertices = modelMesh.GetVertices();
		Vector3[] normals = modelMesh.GetNormals();
		Vector4[] tangents = modelMesh.GetTangents();
		Vector2[] uv = modelMesh.GetUv();
		int[] triangles = modelMesh.GetTriangles();
		int num = modelMesh.GetVertexCount() * count;
		int num2 = modelMesh.GetTriangleCount() * count;
		Matrix4x4 transpose = transform.inverse.transpose;
		if (num > 65000 || num2 > 195000)
		{
			logger.WarnFormat("BuildWarpableMesh has too many vertices or triangles: {0}, {1}", num, num2);
			count = 65000 / Mathf.Max(modelMesh.GetVertexCount(), modelMesh.GetTriangleCount() / 3);
			num = modelMesh.GetVertexCount() * count;
			num2 = modelMesh.GetTriangleCount() * count;
		}
		BuiltinMeshInfo builtinMeshInfo = new BuiltinMeshInfo();
		builtinMeshInfo.triangles = new int[num2];
		builtinMeshInfo.vertices = new Vector3[num];
		builtinMeshInfo.normals = ((normals == null || normals.Length <= 0) ? null : new Vector3[num]);
		builtinMeshInfo.tangents = ((tangents == null || tangents.Length <= 0) ? null : new Vector4[num]);
		builtinMeshInfo.uv = ((uv == null || uv.Length <= 0) ? null : new Vector2[num]);
		builtinMeshInfo.uv2 = new Vector2[num];
		BuiltinMeshInfo builtinMeshInfo2 = builtinMeshInfo;
		int num3 = 0;
		int num4 = 0;
		for (int i = 0; i < count; i++)
		{
			MeshInfo.CopyTriangles(triangles, builtinMeshInfo2.triangles, num4, num3, triangles.Length);
			MeshInfo.CopyVertices(vertices, builtinMeshInfo2.vertices, transform, num4, vertices.Length);
			if (builtinMeshInfo2.normals != null)
			{
				MeshInfo.CopyNormals(normals, builtinMeshInfo2.normals, transpose, num4, normals.Length);
			}
			if (builtinMeshInfo2.tangents != null)
			{
				MeshInfo.CopyTangents(tangents, builtinMeshInfo2.tangents, transpose, num4, tangents.Length);
			}
			if (builtinMeshInfo2.uv != null)
			{
				MeshInfo.CopyUv(uv, builtinMeshInfo2.uv, num4, uv.Length);
			}
			int num5 = startOffset + i;
			Vector2 vector = new Vector2(num5 % texWidth, num5 / texWidth);
			for (int j = 0; j < vertices.Length; j++)
			{
				builtinMeshInfo2.uv2[num4 + j] = vector;
			}
			num4 += vertices.Length;
			num3 += triangles.Length;
		}
		return builtinMeshInfo2;
	}

	public static MeshModifiable BuildColliderMesh(MeshModifiable modelMesh, int count, Func<int, Matrix4x4> transformFunc)
	{
		return BuildColliderMesh(modelMesh, count, (int i, Func<int, Matrix4x4> f) => f(i), transformFunc);
	}

	public static MeshModifiable BuildColliderMesh<T>(MeshModifiable modelMesh, int count, Func<int, T, Matrix4x4> transformFunc, T arg)
	{
		Vector3[] vertices = modelMesh.GetVertices();
		int[] triangles = modelMesh.GetTriangles();
		int num = modelMesh.GetVertexCount() * count;
		int num2 = modelMesh.GetTriangleCount() * count;
		BuiltinMeshInfo builtinMeshInfo = new BuiltinMeshInfo();
		builtinMeshInfo.triangles = new int[num2];
		builtinMeshInfo.vertices = new Vector3[num];
		BuiltinMeshInfo builtinMeshInfo2 = builtinMeshInfo;
		int num3 = 0;
		int num4 = 0;
		for (int i = 0; i < count; i++)
		{
			MeshInfo.CopyTriangles(triangles, builtinMeshInfo2.triangles, num4, num3, triangles.Length);
			MeshInfo.CopyVertices(vertices, builtinMeshInfo2.vertices, transformFunc(i, arg), num4, vertices.Length);
			num4 += vertices.Length;
			num3 += triangles.Length;
		}
		return builtinMeshInfo2;
	}

	public static MeshModifiable BuildColliderMesh<T1, T2>(MeshModifiable modelMesh, int count, Func<int, T1, T2, Matrix4x4> transformFunc, T1 t1, T2 t2)
	{
		return Binder<T1, T2>._BuildColliderMesh(modelMesh, count, transformFunc, t1, t2);
	}

	public static MeshModifiable BuildColliderMesh<T1, T2, T3>(MeshModifiable modelMesh, int count, Func<int, T1, T2, T3, Matrix4x4> transformFunc, T1 t1, T2 t2, T3 t3)
	{
		return Binder<T1, T2, T3>._BuildColliderMesh(modelMesh, count, transformFunc, t1, t2, t3);
	}

	public static void GetWarpTex(ref Texture2D resultTex, int count, Func<int, Color32> colorFunc)
	{
		GetWarpTex(ref resultTex, count, (int i, Func<int, Color32> f) => f(i), colorFunc);
	}

	public static void GetWarpTex<T>(ref Texture2D resultTex, int count, Func<int, T, Color32> colorFunc, T arg)
	{
		int num = 32;
		if (resultTex == null || resultTex.width != num || resultTex.height != num)
		{
			if (resultTex == null)
			{
				resultTex = new Texture2D(num, num, TextureFormat.ARGB32, mipmap: false, QualitySettings.activeColorSpace == ColorSpace.Linear)
				{
					filterMode = FilterMode.Point,
					anisoLevel = 0
				};
			}
			if (resultTex.width != num || resultTex.height != num)
			{
				resultTex.Resize(num, num, TextureFormat.ARGB32, hasMipMap: false);
			}
		}
		Color32[] warpTexColorBuf = WarpTexColorBuf;
		Array.Clear(warpTexColorBuf, 0, warpTexColorBuf.Length);
		count = Math.Min(count, warpTexColorBuf.Length);
		for (int i = 0; i < count; i++)
		{
			ref Color32 reference = ref warpTexColorBuf[i];
			reference = colorFunc(i, arg);
		}
		resultTex.SetPixels32(warpTexColorBuf);
		resultTex.Apply();
	}

	public static void GetWarpTex<T1, T2>(ref Texture2D resultTex, int count, Func<int, T1, T2, Color32> colorFunc, T1 t1, T2 t2)
	{
		Binder<T1, T2>._GetWarpTex(ref resultTex, count, colorFunc, t1, t2);
	}

	public static void GetWarpTex<T1, T2, T3>(ref Texture2D resultTex, int count, Func<int, T1, T2, T3, Color32> colorFunc, T1 t1, T2 t2, T3 t3)
	{
		Binder<T1, T2, T3>._GetWarpTex(ref resultTex, count, colorFunc, t1, t2, t3);
	}
}
