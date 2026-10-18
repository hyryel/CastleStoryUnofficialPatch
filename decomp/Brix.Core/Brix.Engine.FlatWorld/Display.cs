using System;
using System.Collections.Generic;
using System.Linq;
using Brix.Core;
using Brix.Engine.Geometrie;
using Brix.Utils;
using UnityEngine;

namespace Brix.Engine.FlatWorld;

public class Display
{
	public struct ColorGeometry : IGeometrie
	{
		public readonly Color32 c;

		public readonly bool dessous;

		public readonly Color32 topNormals;

		public ColorGeometry(Color32 c, bool dessous, Color32 topNormals)
		{
			this.c = c;
			this.dessous = dessous;
			this.topNormals = topNormals;
		}

		public Vector3 GetSommet(int i)
		{
			Vector3 result = CubeModele.SommetPositions[i];
			bool flag = i >= 4;
			if (flag == dessous)
			{
				byte nth = GetNth(c, i % 4);
				result.y = (float)(int)nth / 255f - 0.5f;
				if (flag)
				{
					result.y = 0f - result.y;
				}
			}
			return result;
		}

		public Vector3 GetNormale(int i)
		{
			if (i < 6)
			{
				return CubeModele.Normales[i];
			}
			if (i < 10)
			{
				return Soft.DecodeNormal(GetNth(topNormals, i - 6));
			}
			Vector3 result = Soft.DecodeNormal(GetNth(topNormals, i - 26));
			result.y = 0f - result.y;
			return result;
		}

		public Vector3 GetTangente(int i)
		{
			if (i < 6)
			{
				return CubeModele.Tangents[i];
			}
			Vector3 vector = Soft.DecodeNormal(GetNth(topNormals, i - 6));
			return (Vector3.right - vector * vector.x).normalized;
		}

		private byte GetNth(Color32 c, int i)
		{
			return i switch
			{
				0 => c.r, 
				1 => c.g, 
				2 => c.b, 
				3 => c.a, 
				_ => 0, 
			};
		}
	}

	internal static readonly int[,] FaceTransBits = new int[6, 4]
	{
		{ 1, 2, 3, 4 },
		{ 0, 5, 4, 3 },
		{ 0, 5, 3, 4 },
		{ 0, 5, 1, 2 },
		{ 0, 5, 2, 1 },
		{ 2, 1, 3, 4 }
	};

	public static void CountLocalizedMesh(ref int vertexCount, ref int triangleCount, int xzStart, int xzWidth, XYZ xyz, XYZ size, Color32[] xzColors, Color32[] columnColors, Color32[] configColors)
	{
		for (int i = 0; i < size.z; i++)
		{
			for (int j = 0; j < size.x; j++)
			{
				int num = xyz.x + j;
				int num2 = xyz.z + i;
				int num3 = xzStart + num + num2 * xzWidth;
				if (num3 < 0 || num3 >= xzColors.Length)
				{
					continue;
				}
				Color32 c = xzColors[num3];
				if (c.r == 0 && c.g == 0 && c.b == 0 && c.a == 0)
				{
					continue;
				}
				int num4 = Soft.DecodeInt(c);
				c = columnColors[num3];
				int num5 = Mathf.Max(c.r, xyz.y);
				int num6 = Mathf.Min(c.b, xyz.y + size.y);
				int num7 = Mathf.Max(c.a, xyz.y);
				int num8 = Mathf.Min(c.g, xyz.y + size.y);
				for (int num9 = ((num5 < num6) ? num5 : num7); num9 < num8; num9 = ((num9 == num6 - 1) ? num7 : (num9 + 1)))
				{
					int num10 = num4 + num9 - c.r;
					if (num9 >= c.a)
					{
						num10 -= c.a - c.b;
					}
					int num11 = configColors?[num10].r ?? 63;
					if (num11 != 0 && vertexCount <= 2000000)
					{
						num11 |= 0x80;
						Cube.Modele.Count(num11, ref vertexCount, ref triangleCount);
					}
				}
			}
		}
	}

	public static int CreateLocalizedMesh(MeshAugmentable meshInfo, CubeDrawer drawer, int xzStart, int xzWidth, XYZ xyz, XYZ size, Color32[] xzColors, Color32[] columnColors, Color32[] offsetColors, Color32[] configColors, Color32[] dessousColors, Color32[] levelColors, Color32[] normalColors, Color32[] uvBaseColors, Color32[] uvTransColors, Color32[] uvSpecialColors, int lod = 0)
	{
		int num = 0;
		for (int i = 0; i < size.z; i++)
		{
			for (int j = 0; j < size.x; j++)
			{
				int num2 = xyz.x + j;
				int num3 = xyz.z + i;
				int num4 = xzStart + num2 + num3 * xzWidth;
				if (num4 < 0 || num4 >= xzColors.Length)
				{
					continue;
				}
				Color32 c = xzColors[num4];
				if (c.r == 0 && c.g == 0 && c.b == 0 && c.a == 0)
				{
					continue;
				}
				int num5 = Soft.DecodeInt(c);
				c = columnColors[num4];
				int num6 = Mathf.Max(c.r, xyz.y);
				int num7 = Mathf.Min(c.b, xyz.y + size.y);
				int num8 = Mathf.Max(c.a, xyz.y);
				int num9 = Mathf.Min(c.g, xyz.y + size.y);
				for (int num10 = ((num6 < num7) ? num6 : num8); num10 < num9; num10 = ((num10 == num7 - 1) ? num8 : (num10 + 1)))
				{
					int num11 = num5 + num10 - c.r;
					if (num10 >= c.a)
					{
						num11 -= c.a - c.b;
					}
					int num12 = configColors?[num11].r ?? 63;
					if (num12 != 0)
					{
						Color32 c2 = ((levelColors == null) ? new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue) : levelColors[num11]);
						bool dessous = (dessousColors[num11].r & 1) == 0 && (dessousColors[num11].r & 0x20) != 0;
						Color32 topNormals = ((normalColors == null) ? new Color32(136, 136, 136, 136) : normalColors[num11]);
						ColorGeometry colorGeometry = new ColorGeometry(c2, dessous, topNormals);
						Vector3 p = new Vector3(num2, num10 - xyz.y, num3);
						num++;
						if (meshInfo.GetVertexCount() <= 2000000)
						{
							Color32 color = uvBaseColors[num11];
							Color32 color2 = uvSpecialColors[num11];
							Vector2 uv = (new Vector2((int)color.r, (int)color.g) * 4f + new Vector2((int)color.b, (int)color.a)) * (1f / 85f);
							Vector2 uv2;
							if (color2.r < 215 || color2.g < 215)
							{
								uv2 = new Vector2((float)(color2.r * 4 + color2.b + (color2.g * 4 + color2.a) * 16) * (1f / 85f) + 0f, 65535f);
							}
							else
							{
								Color32 color3 = uvTransColors[num11];
								uv2 = new Vector2((int)color3.r, (int)color3.g) * 1f + new Vector2((int)color3.b, (int)color3.a) * 256f;
							}
							num12 |= 0x80;
							if (meshInfo.HasTangents())
							{
								drawer.RenderWithTangents(colorGeometry, meshInfo, p, uv, uv2, CubeModele.GetConfig(num12), uv2Decal: true);
							}
							else
							{
								drawer.Render(colorGeometry, meshInfo, p, uv, uv2, CubeModele.GetConfig(num12), uv2Decal: true);
							}
						}
					}
				}
			}
		}
		return num;
	}

	public static int CreateLocalizedCollider(MeshAugmentable meshInfo, CubeDrawer drawer, int xzStart, int xzWidth, XYZ xyz, XYZ size, Color32[] xzColors, Color32[] columnColors, Color32[] offsetColors, Color32[] configColors, Color32[] dessousColors, Color32[] levelColors, Color32[] normalColors, Color32[] uvBaseColors, Color32[] uvTransColors, Color32[] uvSpecialColors)
	{
		int num = 0;
		List<int> list = CollectionPool.RequestList<int>();
		List<Vector3> list2 = CollectionPool.RequestList<Vector3>();
		Dictionary<Vector3, int> dictionary = CollectionPool.RequestDictionary<Vector3, int>();
		int[] array = new int[8];
		for (int i = 0; i < size.z; i++)
		{
			for (int j = 0; j < size.x; j++)
			{
				int num2 = xyz.x + j;
				int num3 = xyz.z + i;
				int num4 = xzStart + num2 + num3 * xzWidth;
				if (num4 < 0 || num4 >= xzColors.Length)
				{
					continue;
				}
				Color32 c = xzColors[num4];
				if (c.r == 0 && c.g == 0 && c.b == 0 && c.a == 0)
				{
					continue;
				}
				int num5 = Soft.DecodeInt(c);
				c = columnColors[num4];
				int num6 = Mathf.Max(c.r, xyz.y);
				int num7 = Mathf.Min(c.b, xyz.y + size.y);
				int num8 = Mathf.Max(c.a, xyz.y);
				int num9 = Mathf.Min(c.g, xyz.y + size.y);
				for (int num10 = ((num6 < num7) ? num6 : num8); num10 < num9; num10 = ((num10 == num7 - 1) ? num8 : (num10 + 1)))
				{
					int num11 = num5 + num10 - c.r;
					if (num10 >= c.a)
					{
						num11 -= c.a - c.b;
					}
					int num12 = configColors?[num11].r ?? 63;
					if (num12 != 0)
					{
						Color32 c2 = ((levelColors == null) ? new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue) : levelColors[num11]);
						bool dessous = (dessousColors[num11].r & 1) == 0 && (dessousColors[num11].r & 0x20) != 0;
						ColorGeometry colorGeometry = new ColorGeometry(c2, dessous, default(Color32));
						Vector3 vector = new Vector3(num2, num10 - xyz.y, num3);
						num++;
						if (meshInfo.GetVertexCount() <= 2000000)
						{
							Config config = CubeModele.GetConfig(num12);
							for (int k = 0; k < config.sommetsUtiles.Length; k++)
							{
								Vector3 vector2 = vector + colorGeometry.GetSommet(config.sommetsUtiles[k]);
								if (!dictionary.TryGetValue(vector2, out array[k]))
								{
									dictionary.Add(vector2, array[k] = dictionary.Count());
									list2.Add(vector2);
								}
							}
							for (int l = 0; l < config.triangles.Length; l++)
							{
								list.Add(array[config.iSommets[config.triangles[l]]]);
							}
						}
					}
				}
			}
		}
		meshInfo.SetTriangles(list.ToArray());
		meshInfo.SetVertices(list2.ToArray());
		list.Dispose();
		list2.Dispose();
		dictionary.Dispose();
		return num;
	}

	public static bool CheckRegionVisible(Color32[] columnColors, int xzStart, int xzWidth, XYZ xyz, XYZ size)
	{
		int num = xzStart + xyz.x + xyz.z * xzWidth;
		for (int i = 0; i < size.x; i++)
		{
			for (int j = 0; j < size.z; j++)
			{
				int num2 = num + i + j * xzWidth;
				if (num2 >= 0 && num2 < columnColors.Length)
				{
					Color32 color = columnColors[num2];
					if (color.r < color.g && (xyz.y < color.b || xyz.y + size.y > color.a) && xyz.y < color.g && xyz.y + size.y > color.r)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public static IEnumerable<XYZ> GetVisibleRegions(Color32[] columnColors, int xzStart, int xzWidth, XYZ worldSize, XYZ sampleSize, bool forceCreate = false)
	{
		for (int y = 0; y < worldSize.y; y += sampleSize.y)
		{
			for (int z = 0; z < worldSize.z; z += sampleSize.z)
			{
				for (int x = 0; x < worldSize.x; x += sampleSize.x)
				{
					XYZ xyz = new XYZ(x, y, z);
					if (forceCreate || CheckRegionVisible(columnColors, xzStart, xzWidth, xyz, sampleSize))
					{
						yield return xyz;
					}
				}
			}
		}
	}

	public static MegaVoxelGeo GetMV(Transform lodParent, XYZ parentPosition, LodSorter lodSorter, XYZ childPosition, XYZ sampleSize, int lod, bool searchParent = true)
	{
		XYZ xYZ = childPosition + parentPosition;
		GameObject gameObject = lodSorter.GetLodChildObject(xYZ, lod);
		if (gameObject == null)
		{
			gameObject = RegisterChildObject(lodParent, xYZ, lodSorter, childPosition, sampleSize, lod, searchParent);
		}
		return gameObject.GetComponent<MegaVoxelGeo>();
	}

	public static Transform GetChildObject(Transform parent, string childName, Action<Transform> init = null, bool searchParent = true)
	{
		Transform transform = null;
		if (searchParent)
		{
			transform = parent.Find(childName);
		}
		if (transform == null)
		{
			transform = NewChildObject(parent, childName);
			init?.Invoke(transform);
		}
		return transform;
	}

	private static Transform NewChildObject(Transform parent, string childName)
	{
		Transform transform = new GameObject(childName, typeof(MeshFilter), typeof(MeshRenderer)).transform;
		transform.GetComponent<Renderer>().sharedMaterial = parent.GetComponent<Renderer>().sharedMaterial;
		transform.parent = parent;
		transform.localPosition = Vector3.zero;
		transform.localScale = Vector3.one;
		transform.gameObject.layer = parent.gameObject.layer;
		return transform;
	}

	private static void InitChildObject(Transform child, XYZ xyzAbs, XYZ size, int lod)
	{
		MegaVoxelGeo megaVoxelGeo = child.gameObject.AddComponent<MegaVoxelGeo>();
		size.y = Mathf.Min(size.y, 256);
		child.localPosition = xyzAbs.ToVector3();
		megaVoxelGeo.lodFactor = lod;
	}

	private static GameObject RegisterChildObject(Transform lodParent, XYZ position, LodSorter lodSorter, XYZ childPosition, XYZ sampleSize, int lod, bool searchParent)
	{
		string childName = "(" + childPosition.x + ", " + childPosition.y + ", " + childPosition.z + ")";
		Transform childObject = GetChildObject(lodParent, childName, delegate(Transform newChild)
		{
			InitChildObject(newChild, childPosition, sampleSize << lod, lod);
		}, searchParent);
		GameObject gameObject = childObject.gameObject;
		lodSorter.RegisterChildObject(gameObject, position, lod);
		return gameObject;
	}

	public static Vector2 GetAsciiUv(string s)
	{
		int num = ((s.Length > 0) ? s[0] : ' ');
		int num2 = ((s.Length > 1) ? s[1] : ' ');
		int num3 = ((s.Length > 2) ? s[2] : ' ');
		int num4 = ((s.Length > 3) ? s[3] : ' ');
		int num5 = (num % 16) | (num2 % 16 << 4) | (num3 % 16 << 8) | (num4 % 16 << 12);
		int num6 = (num / 16) | (num2 / 16 << 4) | (num3 / 16 << 8) | (num4 / 16 << 12);
		return new Vector2(num5, num6) * 2f;
	}

	public static void CreateDataPreviewMesh(GameObject gameObject, Color32[] offsetColors, Color32[] configColors, Tex dataTex, int lod = 0)
	{
		MeshAugmentableSimple meshAugmentableSimple = new MeshAugmentableSimple();
		int num = 0;
		Matrix4x4 matrix4x = Matrix4x4.TRS(Vector3.one / 2f * ((1 << lod) - 1), Quaternion.identity, Vector3.one * (1 << lod));
		Matrix4x4 normalMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one / (1 << lod)) * matrix4x;
		Color32[] pixels = dataTex.GetPixels32();
		for (int i = 0; i < offsetColors.Length; i++)
		{
			Color32 c = offsetColors[i];
			if (c.r != 0 || c.g != 0 || c.b != 0 || c.a != 0 || i <= 10)
			{
				int config = ((configColors == null) ? 63 : Soft.DecodeInt(configColors[i]));
				Cube modele = Cube.Modele;
				Vector3 p = Soft.DecodeVector3(Soft.DecodeInt(c));
				c = pixels[i];
				Vector2 asciiUv = GetAsciiUv($"{c.r:x2}{c.g:x2}");
				Vector2 asciiUv2 = GetAsciiUv($"{c.b:x2}{c.a:x2}");
				num++;
				if (meshAugmentableSimple.GetVertexCount() <= 2000000)
				{
					modele.Render(meshAugmentableSimple, p, asciiUv, config);
					modele.Render(meshAugmentableSimple, p, asciiUv2 + Vector2.up, config);
				}
			}
		}
		if (lod > 0)
		{
			meshAugmentableSimple.Transform(matrix4x, normalMatrix);
		}
		int start = MeshCombineMaison.SplitToMeshes(meshAugmentableSimple, MeshCombineMaison.PopulateMeshes(gameObject, 100), immediate: true).Count();
		MeshCombineMaison.ClearMeshes(gameObject, start);
		Debug.Log("Rendered " + num + " visible voxels");
	}
}
