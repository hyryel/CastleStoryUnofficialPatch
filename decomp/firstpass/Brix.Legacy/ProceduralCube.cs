using System;
using System.Linq;
using Brix.Engine;
using Brix.Engine.Abstract;
using Brix.Engine.FlatWorld;
using Brix.Engine.Geometrie;
using Brix.Util;
using Brix.Utils;
using UnityEngine;

namespace Brix.Legacy;

public static class ProceduralCube
{
	private static readonly Vector2 UvhautGauche = new Vector2(0f, 0f);

	private static readonly Vector2 UvhautDroite = new Vector2(1f, 0f);

	private static readonly Vector2 UvbasGauche = new Vector2(0f, 1f);

	private static readonly Vector2 UvbasDroite = new Vector2(1f, 1f);

	private static readonly Vector2 UvCentre = new Vector2(0.5f, 0.5f);

	private static readonly int[][] StairVertexIndices = new int[4][]
	{
		new int[4] { 3, 2, 5, 4 },
		new int[4] { 0, 1, 6, 7 },
		new int[4] { 2, 0, 7, 5 },
		new int[4] { 1, 3, 4, 6 }
	};

	public static ICubeMeshDescriptor CubeMesh = new CubeMesh2();

	private static readonly Vector3[] _tunnelFaceSommets = Cubeur.Sommets.ToArray();

	private static readonly int[][] WallFaceIndices = (from i in Enumerable.Range(0, 6)
		select (from j in Enumerable.Range(0, 4)
			select CubeMesh.FaceEdgeIndices[i * 4 + j]).ToArray()).ToArray();

	private static bool[] bords = new bool[9];

	private static Vector3[] positions = new Vector3[9];

	private static int[] indices = new int[9];

	private static Vector3[] normaleLUT = null;

	private static float[] sommetsLUT = null;

	private static Mesh CarreSimple()
	{
		Vector3 right = Vector3.right;
		Vector3 up = Vector3.up;
		Vector3 forward = Vector3.forward;
		Mesh mesh = new Mesh();
		mesh.vertices = new Vector3[4]
		{
			-right / 2f + up / 2f,
			right / 2f + up / 2f,
			right / 2f - up / 2f,
			-right / 2f - up / 2f
		};
		mesh.normals = new Vector3[4]
		{
			-forward,
			-forward,
			-forward,
			-forward
		};
		mesh.uv = new Vector2[4]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(1f, 1f),
			new Vector2(0f, 1f)
		};
		mesh.uv2 = new Vector2[4];
		mesh.triangles = new int[6] { 0, 1, 2, 0, 2, 3 };
		return mesh;
	}

	private static int GetVertexIndex(int[] GrilleImport, int premierVertex, int decalage, byte face, byte config, int index)
	{
		if (Calcule.GetBit(config, face))
		{
			GrilleImport[decalage] = premierVertex + index;
			index++;
			GrilleImport[1 + decalage] = premierVertex + index;
			index++;
			GrilleImport[2 + decalage] = premierVertex + index;
			index++;
			GrilleImport[3 + decalage] = premierVertex + index;
			index++;
		}
		else
		{
			GrilleImport[decalage] = -1;
			GrilleImport[1 + decalage] = -1;
			GrilleImport[2 + decalage] = -1;
			GrilleImport[3 + decalage] = -1;
		}
		return index;
	}

	private static int BuildGrilleArray(int[] GrilleImport, int premierVertex, bool croute, byte config)
	{
		int index = 0;
		index = GetVertexIndex(GrilleImport, premierVertex, 0, 0, config, index);
		if (Calcule.GetBit(config, 0) && croute)
		{
			GrilleImport[4] = premierVertex + 4;
			index++;
		}
		else
		{
			GrilleImport[4] = -1;
		}
		index = GetVertexIndex(GrilleImport, premierVertex, 5, 1, config, index);
		index = GetVertexIndex(GrilleImport, premierVertex, 9, 2, config, index);
		index = GetVertexIndex(GrilleImport, premierVertex, 13, 3, config, index);
		index = GetVertexIndex(GrilleImport, premierVertex, 17, 4, config, index);
		return GetVertexIndex(GrilleImport, premierVertex, 21, 5, config, index);
	}

	public static void SapinLod3(Vector3 pos, Vector2 uvBase, float taille, int n, MeshAugmentable meshInfo)
	{
		float num = taille / 2f;
		float num2 = taille * 2f;
		Vector3[] array = new Vector3[n];
		for (int i = 0; i < n; i++)
		{
			ref Vector3 reference = ref array[i];
			reference = num * new Vector3(Mathf.Cos((float)Math.PI * 2f * (float)i / (float)n), 0f, Mathf.Sin((float)Math.PI * 2f * (float)i / (float)n));
		}
		Vector3 vector = num2 * Vector3.up;
		int vertexCount = meshInfo.GetVertexCount();
		for (int i = 0; i < n; i++)
		{
			Vector3[] array2 = new Vector3[4]
			{
				array[i % n],
				array[(1 + i) % n],
				array[(2 + i) % n],
				vector
			};
			Vector2 vector2 = new Vector2(1f * (float)i / (float)n, 0f);
			Vector2 zero = Vector2.zero;
			Vector3 n2 = Vector3.Cross(array2[3] - array2[1], array2[2] - array2[0]);
			n2.Normalize();
			meshInfo.Sommet(pos + array2[1], n2, uvBase + vector2, zero);
			meshInfo.Triangle(vertexCount + i % n, vertexCount + n, vertexCount + (1 + i) % n);
		}
		meshInfo.Sommet(pos + vector, Vector3.up, uvBase + new Vector2(0.5f, 1f), new Vector2(0f, 0f));
	}

	public static void Planche(int mode, Vector3 pos, float taille, MeshInfo meshInfo, int type)
	{
		Vector3 vector = new Vector3((0f - taille) / 2f, 0f, taille / 2f);
		Vector3 vector2 = new Vector3(taille / 2f, 0f, taille / 2f);
		Vector3 vector3 = new Vector3((0f - taille) / 2f, 0f, (0f - taille) / 2f);
		Vector3 vector4 = new Vector3(taille / 2f, 0f, (0f - taille) / 2f);
		Vector2 vector5 = new Vector2(0f, 0f);
		switch (type)
		{
		case 0:
			vector5 = new Vector2(0f, 0f);
			break;
		case 1:
			vector5 = new Vector2(1f, 0f);
			break;
		case 2:
			vector5 = new Vector2(2f, 0f);
			break;
		}
		Vector3[] vertices = meshInfo.vertices;
		Vector2[] uv = meshInfo.uv;
		Vector3[] normals = meshInfo.normals;
		int[] triangles = meshInfo.triangles;
		vertices = vertices.Add(pos + vector);
		int item = vertices.Length - 1;
		uv = uv.Add(vector5 + UvhautGauche);
		normals = normals.Add(Vector3.up);
		vertices = vertices.Add(pos + vector2);
		int item2 = vertices.Length - 1;
		uv = uv.Add(vector5 + UvhautDroite);
		normals = normals.Add(Vector3.up);
		vertices = vertices.Add(pos + vector3);
		int item3 = vertices.Length - 1;
		uv = uv.Add(vector5 + UvbasGauche);
		normals = normals.Add(Vector3.up);
		vertices = vertices.Add(pos + vector4);
		int item4 = vertices.Length - 1;
		uv = uv.Add(vector5 + UvbasDroite);
		normals = normals.Add(Vector3.up);
		triangles = triangles.Add(item);
		triangles = triangles.Add(item2);
		triangles = triangles.Add(item4);
		triangles = triangles.Add(item4);
		triangles = triangles.Add(item3);
		triangles = triangles.Add(item);
	}

	public static void Specific(Matrix4x4 transform, MeshAugmentable meshInfo, BuiltinMeshInfo mesh)
	{
		int vertexCount = meshInfo.GetVertexCount();
		for (int i = 0; i < mesh.vertices.Length; i++)
		{
			meshInfo.Sommet(transform.MultiplyPoint(mesh.vertices[i]), transform.MultiplyVector(mesh.normals[i]), mesh.uv[i], Vector2.zero);
		}
		for (int j = 0; j < mesh.triangles.Length; j += 3)
		{
			meshInfo.Triangle(mesh.triangles[j] + vertexCount, mesh.triangles[j + 1] + vertexCount, mesh.triangles[j + 2] + vertexCount);
		}
	}

	public static void MeshTransform(BuiltinMeshInfo mesh, Matrix4x4 transform)
	{
		for (int i = 0; i < mesh.vertices.Length; i++)
		{
			ref Vector3 reference = ref mesh.vertices[i];
			reference = transform.MultiplyPoint(mesh.vertices[i]);
		}
		for (int j = 0; j < mesh.normals.Length; j++)
		{
			ref Vector3 reference2 = ref mesh.normals[j];
			reference2 = transform.MultiplyVector(mesh.normals[j]).normalized;
		}
		if (mesh.HasTangents())
		{
			for (int k = 0; k < mesh.tangents.Length; k++)
			{
				Vector4 vector = mesh.tangents[k];
				Vector4 vector2 = transform.MultiplyVector(vector).normalized;
				vector2.w = vector.w;
				mesh.tangents[k] = vector2;
			}
		}
	}

	public static void TunnelWalls(int index, Vector2 uv, MeshAugmentable meshInfo)
	{
		Vector3[] sommets = Cubeur.Sommets;
		GetSommetsDessus(13107, sommets);
		Vector3 vecteur = Calcule.GetVecteur(index);
		int vertexCount = meshInfo.GetVertexCount();
		for (int i = 0; i < 4; i++)
		{
			meshInfo.Sommet(vecteur + sommets[4 + i], Vector3.up, uv + new Vector2(i & 1, i >> 1), Vector2.zero);
		}
		meshInfo.DoubleTriangle(vertexCount, vertexCount + 1, vertexCount + 3);
		meshInfo.DoubleTriangle(vertexCount + 3, vertexCount + 2, vertexCount);
	}

	public static void TunnelStairs(int index, int dir, MeshAugmentable meshInfo)
	{
		Vector3[] sommets = Cubeur.Sommets;
		GetSommetsDessus(13107, sommets);
		Vector3 vecteur = Calcule.GetVecteur(index);
		int vertexCount = meshInfo.GetVertexCount();
		int[] array = StairVertexIndices[dir];
		Vector3 vector = Cubeur.Dirs[dir + 1];
		for (int i = 0; i < 4; i++)
		{
			int num = array[i];
			Vector3 normalized = (vector + Vector3.up).normalized;
			meshInfo.Sommet(vecteur + sommets[num], normalized, new Vector2(i & 1, i >> 1), Vector2.zero);
		}
		meshInfo.DoubleTriangle(vertexCount, vertexCount + 1, vertexCount + 3);
		meshInfo.DoubleTriangle(vertexCount + 3, vertexCount + 2, vertexCount);
	}

	public static void TunnelOuterArea(int index, int niveaux, Func<int, bool> isMinable, Func<int, Vector2, Vector2> uvFunc, MeshAugmentable meshInfo)
	{
		int config = VoxelConfigEvaluator.Calculate(XYZ.FromScalaire(index), croute: true, (XYZ xyz) => isMinable(xyz.ToScalaire()));
		TunnelOuterArea(index, niveaux, config, uvFunc, meshInfo);
	}

	public static void TunnelOuterArea(int index, int niveaux, int config, Func<int, Vector2, Vector2> uvFunc, MeshAugmentable meshInfo)
	{
		Vector3[] sommets = Cubeur.Sommets;
		GetSommetsDessus(niveaux, sommets);
		Vector3 vecteur = Calcule.GetVecteur(index);
		for (int i = 0; i < 6; i++)
		{
			if ((config & (1 << i)) != 0)
			{
				Vector3 n = Cubeur.Dirs[i];
				Vector4 tangent = CubeMesh.SharedTangents[i];
				bool flag = false;
				int wallFaceNeighborBits = GetWallFaceNeighborBits(config, i);
				int vertexCount = meshInfo.GetVertexCount();
				for (int j = 0; j < 4; j++)
				{
					Vector3 p = vecteur + sommets[CubeMesh.FaceVertexIndices[4 * i + j]];
					meshInfo.Sommet(p, n, uvFunc(wallFaceNeighborBits, CubeMesh.SharedUvs[j]), Vector2.zero, tangent);
				}
				if (flag)
				{
					meshInfo.Sommet(vecteur + sommets[8], n, uvFunc(wallFaceNeighborBits, new Vector2(0.5f, 0.5f)), Vector2.zero, tangent);
					meshInfo.DoubleTriangle(vertexCount, vertexCount + 1, vertexCount + 4);
					meshInfo.DoubleTriangle(vertexCount + 1, vertexCount + 3, vertexCount + 4);
					meshInfo.DoubleTriangle(vertexCount + 3, vertexCount + 2, vertexCount + 4);
					meshInfo.DoubleTriangle(vertexCount + 2, vertexCount, vertexCount + 4);
				}
				else
				{
					meshInfo.Triangle(vertexCount + 2, vertexCount, vertexCount + 1);
					meshInfo.Triangle(vertexCount + 1, vertexCount + 3, vertexCount + 2);
				}
			}
		}
	}

	public static void TunnelOuterFace(XYZ position, int niveaux, int niveauxDessous, int areaConfig, int faceIndex, Func<int, Vector2, Vector2> uvFunc, MeshAugmentable meshInfo)
	{
		Vector3[] tunnelFaceSommets = _tunnelFaceSommets;
		GetSommetsDessus(niveauxDessous, tunnelFaceSommets);
		for (int i = 0; i < 4; i++)
		{
			Vector3 vector = tunnelFaceSommets[i];
			ref Vector3 reference = ref tunnelFaceSommets[i + 4];
			reference = new Vector3(vector.x, 0f - vector.y, vector.z);
		}
		GetSommetsDessus(niveaux, tunnelFaceSommets);
		Vector3 vector2 = position.ToVector3();
		Vector3 n = Cubeur.Dirs[faceIndex];
		Vector4 tangent = CubeMesh.SharedTangents[faceIndex];
		int wallFaceNeighborBits = GetWallFaceNeighborBits(areaConfig, faceIndex);
		int vertexCount = meshInfo.GetVertexCount();
		for (int j = 0; j < 4; j++)
		{
			Vector3 p = vector2 + tunnelFaceSommets[CubeMesh.FaceVertexIndices[4 * faceIndex + j]];
			Vector2 uv = uvFunc(wallFaceNeighborBits, CubeMesh.SharedUvs[j]);
			meshInfo.Sommet(p, n, uv, Vector2.zero, tangent);
		}
		meshInfo.Triangle(vertexCount + 2, vertexCount, vertexCount + 1);
		meshInfo.Triangle(vertexCount + 1, vertexCount + 3, vertexCount + 2);
	}

	private static int GetWallFaceNeighborBits(int config, int face)
	{
		int num = 0;
		int[] array = WallFaceIndices[face];
		for (int i = 0; i < 4; i++)
		{
			if ((config & (1 << array[i])) == 0)
			{
				num |= 1 << i;
			}
		}
		return num;
	}

	public static void ContourVoxel(XYZ pos, IVoxelVolume volume, IVoxelVolume totalVolume, MeshAugmentable meshInfo, int config, int niveaux = -1)
	{
		for (int i = 0; i < 6; i++)
		{
			if (Calcule.GetBit(config, i) && !volume.Contains(pos + CubeModele.XYZDirs[i]))
			{
				ContourVoxelFace(pos, totalVolume, meshInfo, i, niveaux);
			}
		}
	}

	public static void ContourVoxel_Count(XYZ pos, IVoxelVolume volume, IVoxelVolume totalVolume, int config, ref int vertices, ref int triangles, int niveaux = -1)
	{
		for (int i = 0; i < 6; i++)
		{
			if (Calcule.GetBit(config, i) && !volume.Contains(pos + CubeModele.XYZDirs[i]))
			{
				ContourVoxelFace_Count(pos, totalVolume, i, ref vertices, ref triangles);
			}
		}
	}

	private static bool Check(XYZ pos, int faceIndex, IVoxelVolume volume)
	{
		XYZ xYZ = CubeModele.XYZDirs[faceIndex];
		return !volume.Contains(pos) && !volume.Contains(pos - xYZ) && !volume.Contains(pos + xYZ);
	}

	private static void Contour_ComputeBords(ref bool[] bords, IVoxelVolume volume, XYZ pos, int faceIndex)
	{
		XYZ xYZ = CubeModele.XYZTangents[faceIndex];
		XYZ xYZ2 = CubeModele.XYZTangents2[faceIndex];
		bool flag = Check(pos - xYZ, faceIndex, volume);
		bool flag2 = Check(pos - xYZ2, faceIndex, volume);
		bool flag3 = Check(pos + xYZ, faceIndex, volume);
		bool flag4 = Check(pos + xYZ2, faceIndex, volume);
		bool flag5 = flag2 || flag;
		bool flag6 = flag2 || flag3;
		bool flag7 = flag4 || flag;
		bool flag8 = flag4 || flag3;
		bords[0] = flag5;
		bords[1] = flag2;
		bords[2] = flag6;
		bords[3] = flag3;
		bords[4] = flag8;
		bords[5] = flag4;
		bords[6] = flag7;
		bords[7] = flag;
		bords[8] = false;
	}

	private static void Contour_CountBords(ref bool[] bords, IVoxelVolume volume, XYZ pos, int faceIndex)
	{
		XYZ xYZ = CubeModele.XYZTangents[faceIndex];
		XYZ xYZ2 = CubeModele.XYZTangents2[faceIndex];
		bool flag = Check(pos - xYZ, faceIndex, volume);
		bool flag2 = Check(pos - xYZ2, faceIndex, volume);
		bool flag3 = Check(pos + xYZ, faceIndex, volume);
		bool flag4 = Check(pos + xYZ2, faceIndex, volume);
		bool flag5 = flag2 || flag;
		bool flag6 = flag2 || flag3;
		bool flag7 = flag4 || flag;
		bool flag8 = flag4 || flag3;
		bords[0] = flag5;
		bords[1] = flag2;
		bords[2] = flag6;
		bords[3] = flag3;
		bords[4] = flag8;
		bords[5] = flag4;
		bords[6] = flag7;
		bords[7] = flag;
		bords[8] = false;
	}

	private static void Contour_ComputePositions(ref Vector3[] positions, int faceIndex, int niveaux)
	{
		Vector3[] sommets = Cubeur.Sommets;
		GetSommetsDessus(niveaux, sommets);
		Vector3 vector = sommets[Cubeur.SommetIndices[4 * faceIndex]];
		Vector3 vector2 = sommets[Cubeur.SommetIndices[4 * faceIndex + 1]];
		Vector3 vector3 = sommets[Cubeur.SommetIndices[4 * faceIndex + 2]];
		Vector3 vector4 = sommets[Cubeur.SommetIndices[4 * faceIndex + 3]];
		Vector3 vector5 = (vector + vector3) / 2f;
		Vector3 vector6 = (vector2 + vector4) / 2f;
		Vector3 vector7 = (vector + vector2) / 2f;
		Vector3 vector8 = (vector3 + vector4) / 2f;
		Vector3 vector9 = (vector + vector2 + vector3 + vector4) / 4f;
		positions[0] = vector;
		positions[1] = vector7;
		positions[2] = vector2;
		positions[3] = vector6;
		positions[4] = vector4;
		positions[5] = vector8;
		positions[6] = vector3;
		positions[7] = vector5;
		positions[8] = vector9;
	}

	public static void ContourVoxelFace(XYZ pos, IVoxelVolume volume, MeshAugmentable meshInfo, int faceIndex, int niveaux)
	{
		Contour_ComputeBords(ref bords, volume, pos, faceIndex);
		Contour_ComputePositions(ref positions, faceIndex, niveaux);
		Vector3 vector = pos.ToVector3();
		Vector3 n = CubeModele.Dirs[faceIndex];
		for (int i = 0; i < 9; i++)
		{
			if (i == 8 || bords[i] || bords[(i + 1) % 8] || bords[(i + 7) % 8])
			{
				indices[i] = meshInfo.Sommet(vector + positions[i], n, new Vector2(0f, bords[i] ? 1 : 0), Vector2.zero);
			}
		}
		for (int j = 0; j < 8; j++)
		{
			if (bords[j] || bords[(j + 1) % 8])
			{
				meshInfo.Triangle(indices[j], indices[(j + 1) % 8], indices[8]);
			}
		}
	}

	private static void ContourVoxelFace_Count(XYZ pos, IVoxelVolume volume, int faceIndex, ref int vertex, ref int triangle)
	{
		Contour_ComputeBords(ref bords, volume, pos, faceIndex);
		for (int i = 0; i < 9; i++)
		{
			if (i == 8 || bords[i] || bords[(i + 1) % 8] || bords[(i + 7) % 8])
			{
				vertex++;
			}
		}
		for (int j = 0; j < 8; j++)
		{
			if (bords[j] || bords[(j + 1) % 8])
			{
				triangle++;
			}
		}
	}

	public static void ContourVoxel(int index, IntClasse sel, MeshAugmentable meshInfo, int config, int niveaux, int mode)
	{
		int[] SommetIndices = Cubeur.SommetIndices;
		int[] ScalaireDirs = Cubeur.ScalaireDirs;
		int[] ScalaireTangents1 = Cubeur.ScalaireTangents1;
		int[] ScalaireTangents2 = Cubeur.ScalaireTangents2;
		Vector3[] sommets = Cubeur.Sommets;
		GetSommetsDessus(niveaux, sommets);
		Vector3 vecteur = Calcule.GetVecteur(index);
		Func<int, bool> ContientIndex = sel.Contains;
		Func<int, int, bool> Check0 = (int ind, int num2) => (num2 == 0) ? (!ContientIndex(ind) && !ContientIndex(ind - 1048576) && !ContientIndex(ind + 1048576)) : (!ContientIndex(ind) || (num2 != 0 && (Brix.Engine.Voxel.GetNiveaux(XYZ.FromScalaire(ind)) & 0x3333) != 13107));
		Func<int, int, bool> func = (int ind, int num2) => Check0(ind, num2) || ContientIndex(ind + ScalaireDirs[num2]);
		Func<int, int, bool> Check1 = (((mode & 1) != 0) ? func : Check0);
		for (int iFace = 0; iFace < 6; iFace++)
		{
			if (!Calcule.GetBit(config, iFace) || (iFace != 0 && (Brix.Engine.Voxel.GetNiveaux(XYZ.FromScalaire(index)) & 0x3333) != 13107))
			{
				continue;
			}
			Vector3 vector = Cubeur.Dirs[iFace];
			Func<bool[]> func2 = delegate
			{
				int num2 = ScalaireTangents1[iFace];
				int num3 = ScalaireTangents2[iFace];
				bool flag = Check1(index - num2, iFace);
				bool flag2 = Check1(index - num3, iFace);
				bool flag3 = Check1(index + num2, iFace);
				bool flag4 = Check1(index + num3, iFace);
				bool flag5;
				bool flag6;
				bool flag7;
				bool flag8;
				if ((mode & 2) == 0)
				{
					flag5 = flag2 || flag || Check1(index - num2 - num3, iFace);
					flag6 = flag2 || flag3 || Check1(index + num2 - num3, iFace);
					flag7 = flag4 || flag || Check1(index - num2 + num3, iFace);
					flag8 = flag4 || flag3 || Check1(index + num2 + num3, iFace);
				}
				else
				{
					flag5 = Check1(index - num2 - num3, iFace);
					flag6 = Check1(index + num2 - num3, iFace);
					flag7 = Check1(index - num2 + num3, iFace);
					flag8 = Check1(index + num2 + num3, iFace);
				}
				return new bool[9] { flag5, flag2, flag6, flag3, flag8, flag4, flag7, flag, false };
			};
			Func<Vector3[]> func3 = delegate
			{
				Vector3 vector2 = sommets[SommetIndices[4 * iFace]];
				Vector3 vector3 = sommets[SommetIndices[4 * iFace + 1]];
				Vector3 vector4 = sommets[SommetIndices[4 * iFace + 2]];
				Vector3 vector5 = sommets[SommetIndices[4 * iFace + 3]];
				Vector3 vector6 = (vector2 + vector4) / 2f;
				Vector3 vector7 = (vector3 + vector5) / 2f;
				Vector3 vector8 = (vector2 + vector3) / 2f;
				Vector3 vector9 = (vector4 + vector5) / 2f;
				Vector3 vector10 = (vector2 + vector3 + vector4 + vector5) / 4f;
				return new Vector3[9] { vector2, vector8, vector3, vector7, vector5, vector9, vector4, vector6, vector10 };
			};
			if ((mode & 2) == 0)
			{
				int[] array = new int[9];
				for (int num = 0; num < 9; num++)
				{
					if (num == 8 || func2()[num] || func2()[(num + 1) % 8] || func2()[(num + 7) % 8])
					{
						array[num] = meshInfo.Sommet(vecteur + func3()[num], vector, new Vector2(0f, func2()[num] ? 1 : 0), Vector2.zero);
					}
				}
				for (int num = 0; num < 8; num++)
				{
					if (func2()[num] || func2()[(num + 1) % 8])
					{
						meshInfo.Triangle(array[num], array[(num + 1) % 8], array[8]);
					}
				}
				continue;
			}
			int[] array2 = new int[16];
			for (int num = 0; num < 8; num++)
			{
				if (func2()[num] || func2()[(num + 1) % 8] || func2()[(num + 7) % 8])
				{
					array2[num] = meshInfo.Sommet(vecteur + func3()[num], vector, Vector2.up, Vector2.zero);
					array2[num + 8] = meshInfo.Sommet(vecteur + func3()[num] * 2f - Vector3.Project(func3()[num], vector), vector, Vector2.zero, Vector2.zero);
				}
			}
			for (int num = 0; num < 8; num++)
			{
				if (!func2()[num])
				{
					continue;
				}
				if (num == 1 || num == 3 || num == 5 || num == 7)
				{
					meshInfo.Triangle(array2[num + 8], array2[(num + 1) % 8], array2[(num + 7) % 8]);
					continue;
				}
				if (func2()[(num + 1) % 8])
				{
					meshInfo.Triangle(array2[num], array2[num + 8], array2[(num + 1) % 8 + 8]);
				}
				if (func2()[(num + 7) % 8])
				{
					meshInfo.Triangle(array2[num + 8], array2[num], array2[(num + 7) % 8 + 8]);
				}
			}
		}
	}

	public static void Arc(Vector3 p, Vector3 u, Vector3 v, Vector3 w, float angleMin, float angleMax, float petitRayon, float grandRayon, MeshAugmentable meshInfo, float espace, float densite)
	{
		Vector2 zero = Vector2.zero;
		float num = Mathf.Min(petitRayon, grandRayon);
		float num2 = Mathf.Max(petitRayon, grandRayon);
		float num3 = espace / 2f;
		float num4 = ((!Mathf.Approximately(num, 0f)) ? Mathf.Asin(num3 / num) : 0f);
		float num5 = ((!Mathf.Approximately(num2, 0f)) ? Mathf.Asin(num3 / num2) : 0f);
		float num6 = angleMin * ((float)Math.PI / 180f);
		float num7 = (angleMax - angleMin) * ((float)Math.PI / 180f);
		if (num7 < 0f)
		{
			num7 += (float)Math.PI * 2f;
		}
		float num8 = num6 + num4;
		float num9 = num7 - 2f * num4;
		float num10 = num6 + num5;
		float num11 = num7 - 2f * num5;
		Vector2 zero2 = Vector2.zero;
		int num12 = Mathf.CeilToInt(Mathf.Abs(num2) * num7 * densite);
		for (int i = 0; i < num12; i++)
		{
			if (num12 > 1)
			{
				float num13 = (float)i / (float)num12;
				float num14 = ((float)i + 1f) / (float)num12;
				float f = num8 + num9 * num13;
				float f2 = num10 + num11 * num13;
				float f3 = num8 + num9 * num14;
				float f4 = num10 + num11 * num14;
				meshInfo.Quad(meshInfo.Sommet(p + (u * Mathf.Cos(f2) + v * Mathf.Sin(f2)) * num2, w, zero + new Vector2(num13, 0f), zero2), meshInfo.Sommet(p + (u * Mathf.Cos(f) + v * Mathf.Sin(f)) * num, w, zero + new Vector2(num13, 1f), zero2), meshInfo.Sommet(p + (u * Mathf.Cos(f3) + v * Mathf.Sin(f3)) * num, w, zero + new Vector2(num14, 1f), zero2), meshInfo.Sommet(p + (u * Mathf.Cos(f4) + v * Mathf.Sin(f4)) * num2, w, zero + new Vector2(num14, 0f), zero2));
			}
		}
	}

	public static void Disque(int index, int config, int niveaux, float diametre, MeshAugmentable meshInfo)
	{
		if (config == 0)
		{
			return;
		}
		Vector3 vecteur = Calcule.GetVecteur(index);
		for (int i = 0; i < 6; i++)
		{
			if ((config & (1 << i)) != 0 && Brix.Engine.Abstract.Voxel.FacePleine(niveaux, i))
			{
				float hauteurCentre = GetHauteurCentre(niveaux);
				Vector3 p = vecteur + ((i != 0) ? (Cubeur.Dirs[i] * 0.5f) : (Vector3.up * hauteurCentre));
				Arc(p, Cubeur.Tangents2[i], Cubeur.Tangents1[i], Cubeur.Dirs[i], 0f, 360f, 0f, diametre / 2f, meshInfo, 0f, 20f);
			}
		}
	}

	public static void DommageOverlay(Vector3 pos, byte paireDirection, int config, int niveaux, float taille, MeshAugmentable meshInfo, int type, int variante, bool isPivot, int pv)
	{
		Vector3[] sommets = Cubeur.Sommets;
		GetSommetsDessus(niveaux, sommets);
		Vector2 zero = Vector2.zero;
		Vector2 zero2 = Vector2.zero;
		int num;
		switch (pv)
		{
		case 1:
		case 2:
			num = 0;
			break;
		case 3:
		case 4:
			num = 1;
			break;
		case 5:
		case 6:
			num = 2;
			break;
		default:
			num = 3;
			break;
		}
		int num2 = num;
		bool flag = true;
		Quaternion quaternion = Quaternion.Euler(90 * (variante % 4), 0f, 0f);
		if (variante / 4 % 2 != 0)
		{
			isPivot = !isPivot;
		}
		int[] array = new int[25]
		{
			0, 1, 2, 3, 4, 5, 1, 5, 0, 3,
			4, 2, 5, 2, 1, 3, 4, 0, 2, 0,
			5, 3, 4, 1, 0
		};
		float num3 = 0f;
		for (int i = 0; i < 6; i++)
		{
			int num4 = array[variante % 4 * 6 + i];
			byte b = (byte)Calcule.FaceIndexToConfig(num4);
			Vector3 vector = Calcule.ConfigToVecteur(b);
			Quaternion quaternion2 = Quaternion.identity;
			if (paireDirection != 0)
			{
				Vector3 b2 = Calcule.ConfigToVecteur((!isPivot) ? Calcule.InvertConfig(paireDirection) : paireDirection);
				num3 = Calcule.AngleHorizontal(Vector3.right, b2);
				quaternion2 = Quaternion.Euler(0f, num3, 0f);
			}
			byte b3 = (byte)num4;
			if (num4 != 0 && num4 != 5)
			{
				Vector3 vecteur = quaternion2 * vector;
				b3 = (byte)Calcule.VecteurToFaceIndex(vecteur);
			}
			quaternion2 = Quaternion.Euler(0f, 0f - num3, 0f) * quaternion;
			Vector3 vector2 = quaternion2 * Cubeur.Dirs[b3];
			Vector3 vector3 = quaternion2 * Cubeur.Tangents1[b3];
			Vector3 vector4 = quaternion2 * Cubeur.Tangents2[b3];
			int index = Calcule.VecteurToFaceIndex(vector2);
			if (Calcule.GetBit(config, index))
			{
				bool flag2 = b != paireDirection && Calcule.InvertConfig(b) != paireDirection && paireDirection != 0;
				bool flag3 = isPivot;
				int num5 = ((!flag3) ? 1 : 0);
				bool flag4 = false;
				bool flag5 = false;
				float num6 = 0f;
				switch (b3)
				{
				case 0:
					num6 = 1 - num5;
					break;
				case 1:
					num6 = 3 - num5;
					flag4 = true;
					flag5 = true;
					break;
				case 2:
					num6 = 7 - num5;
					break;
				case 3:
					num6 = 8f;
					flag4 = true;
					flag5 = true;
					break;
				case 4:
					num6 = 9f;
					flag4 = true;
					flag5 = true;
					break;
				case 5:
					num6 = 5 - num5;
					break;
				}
				float num7 = ((!flag2) ? 0.95f : 0.975f);
				num6 += ((!flag2 || !flag3) ? 0.025f : 0f);
				vector3 *= (float)((!flag4) ? 1 : (-1));
				vector4 *= (float)((!flag5) ? 1 : (-1));
				Vector3 vector5 = vector2 * 0.5f;
				Vector3 vector6 = vector3 * 0.5f;
				Vector3 vector7 = vector4 * 0.5f;
				Vector3 vector8 = vector5 - vector6 - vector7;
				Vector3 vector9 = vector5 + vector6 - vector7;
				Vector3 vector10 = vector5 - vector6 + vector7;
				Vector3 vector11 = vector5 + vector6 + vector7;
				zero = new Vector2(num6 + num7 / 2f, 0.5f + (float)num2);
				Vector2 vector12 = new Vector2(num7 / 2f, 0f);
				Vector2 vector13 = new Vector2(0f, 0.475f);
				int num8;
				int b4;
				int b5;
				int num9;
				if (flag)
				{
					Vector4 tangent = new Vector4(vector3.x, vector3.y, vector3.z, -1f);
					num8 = meshInfo.Sommet(pos + vector8 * taille, vector2, zero - vector12 - vector13, zero2 + UvhautGauche, tangent);
					b4 = meshInfo.Sommet(pos + vector9 * taille, vector2, zero + vector12 - vector13, zero2 + UvhautDroite, tangent);
					b5 = meshInfo.Sommet(pos + vector10 * taille, vector2, zero - vector12 + vector13, zero2 + UvbasGauche, tangent);
					num9 = meshInfo.Sommet(pos + vector11 * taille, vector2, zero + vector12 + vector13, zero2 + UvbasDroite, tangent);
				}
				else
				{
					num8 = meshInfo.Sommet(pos + vector8 * taille, vector2, zero - vector12 - vector13, zero2 + UvhautGauche);
					b4 = meshInfo.Sommet(pos + vector9 * taille, vector2, zero + vector12 - vector13, zero2 + UvhautDroite);
					b5 = meshInfo.Sommet(pos + vector10 * taille, vector2, zero - vector12 + vector13, zero2 + UvbasGauche);
					num9 = meshInfo.Sommet(pos + vector11 * taille, vector2, zero + vector12 + vector13, zero2 + UvbasDroite);
				}
				meshInfo.Triangle(num8, b4, num9);
				meshInfo.Triangle(num9, b5, num8);
			}
		}
	}

	public static void DommageOverlay_1(Vector3 pos, byte paireDirection, int config, int niveaux, float taille, MeshAugmentable meshInfo, int type, int variante)
	{
		Vector3[] sommets = Cubeur.Sommets;
		GetSommetsDessus(niveaux, sommets);
		Vector2 zero = Vector2.zero;
		for (int i = 0; i < 6; i++)
		{
			if (Calcule.GetBit(config, i))
			{
				Vector3 n = Cubeur.Dirs[i];
				Vector2 zero2 = Vector2.zero;
				int num = 4 * i;
				Vector3 vector = sommets[Cubeur.SommetIndices[num++]];
				Vector3 vector2 = sommets[Cubeur.SommetIndices[num++]];
				Vector3 vector3 = sommets[Cubeur.SommetIndices[num++]];
				Vector3 vector4 = sommets[Cubeur.SommetIndices[num]];
				int num2 = meshInfo.Sommet(pos + vector * taille, n, zero + UvhautGauche, zero2 + UvhautGauche);
				int b = meshInfo.Sommet(pos + vector2 * taille, n, zero + UvhautDroite, zero2 + UvhautDroite);
				int b2 = meshInfo.Sommet(pos + vector3 * taille, n, zero + UvbasGauche, zero2 + UvbasGauche);
				int num3 = meshInfo.Sommet(pos + vector4 * taille, n, zero + UvbasDroite, zero2 + UvbasDroite);
				meshInfo.Triangle(num2, b, num3);
				meshInfo.Triangle(num3, b2, num2);
			}
		}
	}

	public static void TextOverlay(Vector3 pos, int config, int niveaux, MeshAugmentable meshInfo, string text)
	{
		if (!string.IsNullOrEmpty(text))
		{
			string text2 = text + "        ";
			CubeNiveaux modele = new CubeNiveaux(niveaux);
			if (text.Length > 0)
			{
				string s = text2.Substring(0, 4);
				Vector2 asciiUv = Brix.Engine.FlatWorld.Display.GetAsciiUv(s);
				modele.Render(meshInfo, pos, asciiUv, config);
			}
			if (text.Length > 4)
			{
				string s2 = text2.Substring(4, 4);
				Vector2 asciiUv2 = Brix.Engine.FlatWorld.Display.GetAsciiUv(s2);
				modele.Render(meshInfo, pos, asciiUv2 + Vector2.up, config);
			}
		}
	}

	public static void CubeLOD2(Vector3 pos, float taille, int config, int niveaux, MeshInfoClasse meshInfo, int type)
	{
		Vector3[] sommets = Cubeur.Sommets;
		GetSommetsDessus(niveaux, sommets);
		Vector2 x = Cubeur.Uvs[type];
		Classeur<Vector3> vertices = meshInfo.vertices;
		Classeur<Vector2> uv = meshInfo.uv;
		Classeur<Vector2> uv2 = meshInfo.uv2;
		Classeur<Vector3> normals = meshInfo.normals;
		IntClasse triangles = meshInfo.triangles;
		for (int i = 0; i < 6; i++)
		{
			if (Calcule.GetBit(config, i))
			{
				Vector3 x2 = Cubeur.Dirs[i];
				Vector2 zero = Vector2.zero;
				int num = 4 * i;
				Vector3 vector = sommets[Cubeur.SommetIndices[num++]];
				Vector3 vector2 = sommets[Cubeur.SommetIndices[num++]];
				Vector3 vector3 = sommets[Cubeur.SommetIndices[num++]];
				Vector3 vector4 = sommets[Cubeur.SommetIndices[num]];
				int x3 = vertices.Add(pos + vector * taille);
				int x4 = vertices.Add(pos + vector2 * taille);
				int x5 = vertices.Add(pos + vector3 * taille);
				int x6 = vertices.Add(pos + vector4 * taille);
				uv.Add(x);
				uv2.Add(zero);
				normals.Add(x2);
				uv.Add(x);
				uv2.Add(zero);
				normals.Add(x2);
				uv.Add(x);
				uv2.Add(zero);
				normals.Add(x2);
				uv.Add(x);
				uv2.Add(zero);
				normals.Add(x2);
				triangles.Add(x3);
				triangles.Add(x4);
				triangles.Add(x6);
				triangles.Add(x6);
				triangles.Add(x5);
				triangles.Add(x3);
			}
		}
	}

	public static void Cube(int Mode, Vector3 Pos, byte paireDirection, byte Config, int niveaux, float Taille, MeshAugmentable meshInfo, int type, int variante)
	{
		Cube(Mode, Pos, paireDirection, Config, niveaux, Taille, meshInfo, type, variante, inverser: false, foncer: false, -1);
	}

	public static void Cube(int Mode, Vector3 Pos, byte paireDirection, byte Config, int niveaux, float Taille, MeshAugmentable meshInfo, int type, int variante, bool inverser, bool foncer, int fonce)
	{
		Vector3 paire = Calcule.ConfigToVecteur(paireDirection);
		float[] Sommets = SommetsPosition(Mode, Pos, Taille, niveaux);
		bool libre = Calcule.GetBit(Config, 5);
		if (inverser)
		{
			Sommets = new float[9]
			{
				Taille - Sommets[4],
				Taille - Sommets[5],
				Taille - Sommets[6],
				Taille - Sommets[7],
				Taille - Sommets[0],
				Taille - Sommets[1],
				Taille - Sommets[2],
				Taille - Sommets[3],
				Sommets[8]
			};
		}
		Action<int, Vector3> action = delegate(int iFace, Vector3 Dir)
		{
			Vector2 zero = Vector2.zero;
			bool flag = Mode == 1 && iFace == 0 && !inverser;
			float num2 = 0f;
			float[] array = new float[4];
			Vector2 uvhautGauche = UvhautGauche;
			array[0] = uvhautGauche.x;
			Vector2 uvhautDroite = UvhautDroite;
			array[1] = uvhautDroite.x;
			Vector2 uvbasGauche = UvbasGauche;
			array[2] = uvbasGauche.x;
			Vector2 uvbasDroite = UvbasDroite;
			array[3] = uvbasDroite.x;
			float[] array2 = array;
			float[] array3 = new float[4];
			Vector2 uvhautGauche2 = UvhautGauche;
			array3[0] = uvhautGauche2.y;
			Vector2 uvhautDroite2 = UvhautDroite;
			array3[1] = uvhautDroite2.y;
			Vector2 uvbasGauche2 = UvbasGauche;
			array3[2] = uvbasGauche2.y;
			Vector2 uvbasDroite2 = UvbasDroite;
			array3[3] = uvbasDroite2.y;
			float[] array4 = array3;
			Vector3 vector;
			Vector3 vector2;
			Vector3 vector3;
			Vector3 vector4;
			switch (iFace)
			{
			case 0:
				vector = new Vector3((0f - Taille) / 2f, Sommets[0], Taille / 2f);
				vector2 = new Vector3(Taille / 2f, Sommets[1], Taille / 2f);
				vector3 = new Vector3((0f - Taille) / 2f, Sommets[2], (0f - Taille) / 2f);
				vector4 = new Vector3(Taille / 2f, Sommets[3], (0f - Taille) / 2f);
				break;
			case 3:
				vector = new Vector3(Taille / 2f, Sommets[3], (0f - Taille) / 2f);
				vector2 = new Vector3(Taille / 2f, Sommets[1], Taille / 2f);
				vector3 = new Vector3(Taille / 2f, Sommets[7], (0f - Taille) / 2f);
				vector4 = new Vector3(Taille / 2f, Sommets[5], Taille / 2f);
				break;
			case 1:
				vector = new Vector3(Taille / 2f, Sommets[1], Taille / 2f);
				vector2 = new Vector3((0f - Taille) / 2f, Sommets[0], Taille / 2f);
				vector3 = new Vector3(Taille / 2f, Sommets[5], Taille / 2f);
				vector4 = new Vector3((0f - Taille) / 2f, Sommets[4], Taille / 2f);
				break;
			case 4:
				vector = new Vector3((0f - Taille) / 2f, Sommets[0], Taille / 2f);
				vector2 = new Vector3((0f - Taille) / 2f, Sommets[2], (0f - Taille) / 2f);
				vector3 = new Vector3((0f - Taille) / 2f, Sommets[4], Taille / 2f);
				vector4 = new Vector3((0f - Taille) / 2f, Sommets[6], (0f - Taille) / 2f);
				break;
			case 2:
				vector = new Vector3((0f - Taille) / 2f, Sommets[2], (0f - Taille) / 2f);
				vector2 = new Vector3(Taille / 2f, Sommets[3], (0f - Taille) / 2f);
				vector3 = new Vector3((0f - Taille) / 2f, Sommets[6], (0f - Taille) / 2f);
				vector4 = new Vector3(Taille / 2f, Sommets[7], (0f - Taille) / 2f);
				break;
			default:
				vector = new Vector3((0f - Taille) / 2f, Sommets[6], (0f - Taille) / 2f);
				vector2 = new Vector3(Taille / 2f, Sommets[7], (0f - Taille) / 2f);
				vector3 = new Vector3((0f - Taille) / 2f, Sommets[4], Taille / 2f);
				vector4 = new Vector3(Taille / 2f, Sommets[5], Taille / 2f);
				break;
			}
			Vector3 vector5 = new Vector3(0f, Sommets[8], 0f);
			switch (type)
			{
			case 5:
			case 6:
				zero.y = 15f;
				zero.x = variante & 0xF;
				break;
			case 7:
				zero.y = 14f;
				if (iFace != 0)
				{
					zero.x = 1f;
				}
				break;
			case 8:
				zero.y = 8f;
				if (iFace != 0)
				{
					zero.x = 1f;
				}
				break;
			case 9:
				zero.y = 10f;
				if (iFace != 0)
				{
					zero.x = 1f;
				}
				break;
			case 10:
				zero.y = 12f;
				if (iFace != 0)
				{
					zero.x = 1f;
				}
				break;
			case 11:
				if (libre)
				{
					zero.y = 4f;
					zero.x = variante % 4 + 12;
					if (Dir == paire)
					{
						zero.x = variante % 7;
					}
					if (Dir == -paire)
					{
						zero.x = (variante + 1) % 7;
					}
					if (iFace == 0 || iFace == 5)
					{
						zero.x -= 4f;
					}
				}
				else
				{
					zero.y = 4f;
					if (iFace != 0)
					{
						zero.x = 1f;
					}
				}
				break;
			case 1:
				if (libre)
				{
					int num3 = iFace;
					if (num3 == 0 || num3 == 5)
					{
						if (paire == Vector3.zero)
						{
							zero.x = 3f;
						}
						else if (paire == Vector3.forward)
						{
							zero.x = 4f;
						}
						else if (paire == -Vector3.forward)
						{
							zero.x = 5f;
						}
						else if (paire == Vector3.right)
						{
							zero.x = 6f;
						}
						else if (paire == -Vector3.right)
						{
							zero.x = 7f;
						}
					}
					else if (Dir == -paire)
					{
						zero.x = 0f;
					}
					else
					{
						switch (num3)
						{
						case 1:
							zero.x = ((!(paire == Vector3.right)) ? 1 : 2);
							break;
						case 2:
							zero.x = ((paire == Vector3.right) ? 1 : 2);
							break;
						case 3:
							zero.x = ((paire == Vector3.forward) ? 1 : 2);
							break;
						case 4:
							zero.x = ((!(paire == Vector3.forward)) ? 1 : 2);
							break;
						}
					}
				}
				else
				{
					zero.y = 1f;
					if (iFace != 0 && iFace != 5)
					{
						zero.x = 1f;
					}
				}
				break;
			case 2:
			{
				zero.y = 4 + variante;
				int num3 = iFace;
				if (num3 == 0 || num3 == 5)
				{
					if (paire == Vector3.zero)
					{
						zero.x = 3f;
					}
					else if (paire == Vector3.forward)
					{
						zero.x = 4f;
					}
					else if (paire == -Vector3.forward)
					{
						zero.x = 5f;
					}
					else if (paire == Vector3.right)
					{
						zero.x = 6f;
					}
					else if (paire == -Vector3.right)
					{
						zero.x = 7f;
					}
				}
				else if (Dir == -paire)
				{
					zero.x = 0f;
				}
				else
				{
					switch (num3)
					{
					case 1:
						zero.x = ((!(paire == Vector3.right)) ? 1 : 2);
						break;
					case 2:
						zero.x = ((paire == Vector3.right) ? 1 : 2);
						break;
					case 3:
						zero.x = ((paire == Vector3.forward) ? 1 : 2);
						break;
					case 4:
						zero.x = ((!(paire == Vector3.forward)) ? 1 : 2);
						break;
					}
				}
				break;
			}
			case 3:
			{
				zero.y = 4f;
				int num3 = iFace;
				if (num3 == 0 || num3 == 5)
				{
					if (paire == Vector3.zero)
					{
						zero.x = 3f;
					}
					else if (paire == Vector3.forward)
					{
						zero.x = 4f;
					}
					else if (paire == -Vector3.forward)
					{
						zero.x = 5f;
					}
					else if (paire == Vector3.right)
					{
						zero.x = 6f;
					}
					else if (paire == -Vector3.right)
					{
						zero.x = 7f;
					}
				}
				else if (Dir == -paire)
				{
					zero.x = 0f;
				}
				else
				{
					switch (num3)
					{
					case 1:
						zero.x = ((!(paire == Vector3.right)) ? 1 : 2);
						break;
					case 2:
						zero.x = ((paire == Vector3.right) ? 1 : 2);
						break;
					case 3:
						zero.x = ((paire == Vector3.forward) ? 1 : 2);
						break;
					case 4:
						zero.x = ((!(paire == Vector3.forward)) ? 1 : 2);
						break;
					}
				}
				break;
			}
			case 4:
				zero.y = 5f;
				zero.x = variante & 0xF;
				break;
			}
			zero += Vector2.one * num2;
			int num4 = meshInfo.Sommet(Pos + vector, Dir, zero + new Vector2(array2[0], array4[0]) * (1f - 2f * num2));
			int num5 = meshInfo.Sommet(Pos + vector2, Dir, zero + new Vector2(array2[1], array4[1]) * (1f - 2f * num2));
			int num6 = meshInfo.Sommet(Pos + vector3, Dir, zero + new Vector2(array2[2], array4[2]) * (1f - 2f * num2));
			int num7 = meshInfo.Sommet(Pos + vector4, Dir, zero + new Vector2(array2[3], array4[3]) * (1f - 2f * num2));
			if (!flag)
			{
				meshInfo.Triangle(num4, num5, num7);
				meshInfo.Triangle(num7, num6, num4);
			}
			else
			{
				int c = meshInfo.Sommet(Pos + vector5, Vector3.up, zero + UvCentre * (1f - 2f * num2));
				meshInfo.Triangle(num4, num5, c);
				meshInfo.Triangle(num5, num7, c);
				meshInfo.Triangle(num7, num6, c);
				meshInfo.Triangle(num6, num4, c);
			}
		};
		for (int num = 0; num < 6; num++)
		{
			if (Calcule.GetBit(Config, num))
			{
				action(num, Cubeur.Dirs[num]);
			}
		}
	}

	public static float[] SommetsPosition(int Mode, Vector3 Pos, float Taille, int branchements)
	{
		float[] array = new float[9];
		float num;
		float num2;
		float num3;
		float num4;
		float num5;
		if (Mode == 0 && (branchements & 0xFFFF) == 65535)
		{
			num = Taille / 2f;
			num2 = Taille / 2f;
			num3 = Taille / 2f;
			num4 = Taille / 2f;
			num5 = Taille / 2f;
		}
		else
		{
			float num6 = branchements & 3;
			num = num6 * 1f / 3f - Taille / 2f;
			float num7 = (branchements >> 4) & 3;
			num2 = num7 * 1f / 3f - Taille / 2f;
			float num8 = (branchements >> 8) & 3;
			num4 = num8 * 1f / 3f - Taille / 2f;
			float num9 = (branchements >> 12) & 3;
			num3 = num9 * 1f / 3f - Taille / 2f;
			float num10 = (num6 / 3f + num7 / 3f + num8 / 3f + num9 / 3f) / 4f;
			num5 = num10 - Taille / 2f;
		}
		float num11 = (0f - Taille) / 2f;
		float num12 = (0f - Taille) / 2f;
		float num13 = (0f - Taille) / 2f;
		float num14 = (0f - Taille) / 2f;
		array[0] = num;
		array[1] = num2;
		array[2] = num3;
		array[3] = num4;
		array[4] = num11;
		array[5] = num12;
		array[6] = num13;
		array[7] = num14;
		array[8] = num5;
		return array;
	}

	public static int GetShadowmap(int[] rubik)
	{
		int num = 0;
		for (int i = 0; i < 8; i++)
		{
			if ((rubik[i] & 0x20) != 0 || (rubik[i] & 8) == 0)
			{
				num += 128 >> i;
			}
		}
		return num;
	}

	private static void ConstruireDonneesProcedurales()
	{
		normaleLUT = new Vector3[256];
		sommetsLUT = new float[1280];
		MeshInfoClasse meshInfoClasse = new MeshInfoClasse();
		for (int i = 0; i < 256; i++)
		{
			int num = (i & 3) | (((i >> 2) & 3) << 4) | (((i >> 4) & 3) << 8) | (((i >> 6) & 3) << 12);
			Cube(1, new Vector3(i & 0xF, 0f, i >> 4) * 2f, 1, 1, num, 1f, meshInfoClasse, 0, 0);
			float[] array = SommetsPosition(1, Vector3.zero, 1f, num);
			for (int j = 0; j < 5; j++)
			{
				sommetsLUT[i * 5 + j] = array[(j >= 4) ? 8 : j];
			}
		}
		Mesh mesh = new Mesh();
		meshInfoClasse.Rendre(mesh);
		mesh.RecalculateNormals();
		Vector3[] normals = mesh.normals;
		for (int k = 0; k < 256; k++)
		{
			int num2 = k * (meshInfoClasse.normals.Count >> 8) + 4;
			ref Vector3 reference = ref normaleLUT[k];
			reference = normals[num2];
		}
		UnityEngine.Object.Destroy(mesh);
	}

	public static Vector3 GetNormaleDessus(int niveaux)
	{
		if (normaleLUT == null)
		{
			ConstruireDonneesProcedurales();
		}
		int num = (niveaux & 3) | (((niveaux >> 4) & 3) << 2) | (((niveaux >> 8) & 3) << 4) | (((niveaux >> 12) & 3) << 6);
		return normaleLUT[num];
	}

	public static void GetSommetsDessus(int niveaux, Vector3[] resultat)
	{
		if (sommetsLUT == null)
		{
			ConstruireDonneesProcedurales();
		}
		int num = niveaux;
		int num2 = (num & 3) | (((num >> 4) & 3) << 2) | (((num >> 8) & 3) << 4) | (((num >> 12) & 3) << 6);
		num = 5 * num2;
		resultat[0].y = sommetsLUT[num++];
		resultat[1].y = sommetsLUT[num++];
		resultat[2].y = sommetsLUT[num++];
		resultat[3].y = sommetsLUT[num++];
		resultat[8].y = sommetsLUT[num];
	}

	public static float GetHauteurCentre(int niveaux)
	{
		if (sommetsLUT == null)
		{
			ConstruireDonneesProcedurales();
		}
		int num = niveaux;
		int num2 = (num & 3) | (((num >> 4) & 3) << 2) | (((num >> 8) & 3) << 4) | (((num >> 12) & 3) << 6);
		num = 5 * num2;
		return sommetsLUT[num + 4];
	}
}
