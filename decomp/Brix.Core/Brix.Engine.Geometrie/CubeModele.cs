using Brix.Core;
using UnityEngine;

namespace Brix.Engine.Geometrie;

public static class CubeModele
{
	public static readonly Vector3[] Dirs;

	public static readonly Vector3[] Tangents;

	public static readonly Vector3[] Tangents2;

	public static readonly int[] ScalaireDirs;

	public static readonly XYZ[] XYZDirs;

	public static readonly XYZ[] XYZTangents;

	public static readonly XYZ[] XYZTangents2;

	public static readonly Vector3[] SommetPositions;

	public static readonly int[] SommetIndices;

	public const float UvD = 1f / 32f;

	public static readonly Vector2[] UvDecals;

	public static readonly Vector3[] Normales;

	public static readonly Vector3[] Tangentes;

	private static Config[] configFromBitmask;

	private static CubeDrawer _drawer;

	public static Vector3 GetSommet(int i)
	{
		return SommetPositions[i];
	}

	public static Vector3 GetNormale(int i)
	{
		return Normales[i];
	}

	public static Vector3 GetTangente(int i)
	{
		return Tangentes[i];
	}

	private static Config BuildConfig(int config)
	{
		bool flag = (config & 0x40) != 0;
		bool flag2 = (config & 0x80) != 0;
		bool flag3 = (config & 0x100) != 0;
		bool flag4 = (config & 0x200) != 0;
		return Config.FromBitmask(config & 0x3F, (flag ? 1 : 0) + (flag3 ? 32 : 0), (flag2 ? 1 : 0) + (flag4 ? 32 : 0));
	}

	static CubeModele()
	{
		Dirs = new Vector3[6]
		{
			Vector3.up,
			Vector3.forward,
			-Vector3.forward,
			Vector3.right,
			-Vector3.right,
			-Vector3.up
		};
		Tangents = new Vector3[6]
		{
			Vector3.right,
			-Vector3.right,
			Vector3.right,
			Vector3.forward,
			-Vector3.forward,
			Vector3.right
		};
		Tangents2 = new Vector3[6]
		{
			-Vector3.forward,
			-Vector3.up,
			-Vector3.up,
			-Vector3.up,
			-Vector3.up,
			Vector3.forward
		};
		ScalaireDirs = new int[6] { 1048576, 1024, -1024, 1, -1, -1048576 };
		XYZDirs = new XYZ[6]
		{
			XYZ.up,
			XYZ.forward,
			-XYZ.forward,
			XYZ.right,
			-XYZ.right,
			-XYZ.up
		};
		XYZTangents = new XYZ[6]
		{
			XYZ.right,
			-XYZ.right,
			XYZ.right,
			XYZ.forward,
			-XYZ.forward,
			XYZ.right
		};
		XYZTangents2 = new XYZ[6]
		{
			-XYZ.forward,
			-XYZ.up,
			-XYZ.up,
			-XYZ.up,
			-XYZ.up,
			XYZ.forward
		};
		SommetPositions = new Vector3[14]
		{
			new Vector3(-0.5f, 0.5f, 0.5f),
			new Vector3(0.5f, 0.5f, 0.5f),
			new Vector3(-0.5f, 0.5f, -0.5f),
			new Vector3(0.5f, 0.5f, -0.5f),
			new Vector3(-0.5f, -0.5f, 0.5f),
			new Vector3(0.5f, -0.5f, 0.5f),
			new Vector3(-0.5f, -0.5f, -0.5f),
			new Vector3(0.5f, -0.5f, -0.5f),
			new Vector3(0f, 0.5f, 0f),
			new Vector3(0f, 0f, 0.5f),
			new Vector3(0f, 0f, -0.5f),
			new Vector3(0.5f, 0f, 0f),
			new Vector3(-0.5f, 0f, 0f),
			new Vector3(0f, 0.5f, 0f)
		};
		SommetIndices = new int[24]
		{
			0, 1, 2, 3, 1, 0, 5, 4, 2, 3,
			6, 7, 3, 1, 7, 5, 0, 2, 4, 6,
			6, 7, 4, 5
		};
		UvDecals = new Vector2[5]
		{
			new Vector2(1f / 32f, 1f / 32f),
			new Vector2(31f / 32f, 1f / 32f),
			new Vector2(1f / 32f, 31f / 32f),
			new Vector2(31f / 32f, 31f / 32f),
			Vector2.one / 2f
		};
		Normales = new Vector3[30]
		{
			Dirs[0],
			Dirs[1],
			Dirs[2],
			Dirs[3],
			Dirs[4],
			Dirs[5],
			Dirs[0],
			Dirs[0],
			Dirs[0],
			Dirs[0],
			Dirs[1],
			Dirs[1],
			Dirs[1],
			Dirs[1],
			Dirs[2],
			Dirs[2],
			Dirs[2],
			Dirs[2],
			Dirs[3],
			Dirs[3],
			Dirs[3],
			Dirs[3],
			Dirs[4],
			Dirs[4],
			Dirs[4],
			Dirs[4],
			Dirs[5],
			Dirs[5],
			Dirs[5],
			Dirs[5]
		};
		Tangentes = new Vector3[30]
		{
			Tangents[0],
			Tangents[1],
			Tangents[2],
			Tangents[3],
			Tangents[4],
			Tangents[5],
			Tangents[0],
			Tangents[0],
			Tangents[0],
			Tangents[0],
			Tangents[1],
			Tangents[1],
			Tangents[1],
			Tangents[1],
			Tangents[2],
			Tangents[2],
			Tangents[2],
			Tangents[2],
			Tangents[3],
			Tangents[3],
			Tangents[3],
			Tangents[3],
			Tangents[4],
			Tangents[4],
			Tangents[4],
			Tangents[4],
			Tangents[5],
			Tangents[5],
			Tangents[5],
			Tangents[5]
		};
		configFromBitmask = new Config[1024];
		_drawer = new CubeDrawer();
	}

	public static Config GetConfig(int config = -1)
	{
		config &= 0x3FF;
		if (configFromBitmask[config] == null)
		{
			configFromBitmask[config] = BuildConfig(config);
		}
		return configFromBitmask[config];
	}

	public static void InitRenderConfigs()
	{
		for (int i = 0; i < configFromBitmask.Length; i++)
		{
			configFromBitmask[i] = configFromBitmask[i] ?? BuildConfig(i);
		}
	}

	public static void Render(this IGeometrie modele, MeshAugmentable mesh, Vector3 p, Vector2 uv2, int config = -1)
	{
		modele.Render(mesh, p, uv2, GetConfig(config));
	}

	public static void Render(this IGeometrie modele, MeshAugmentable mesh, Vector3 p, Vector2 uv, Vector2 uv2, int config = -1, bool uv2Decal = false)
	{
		modele.Render(mesh, p, uv, uv2, GetConfig(config), uv2Decal);
	}

	public static void Render(this IGeometrie modele, MeshAugmentable mesh, Vector3 p, Vector2 uv2, Config config)
	{
		modele.Render(mesh, p, Vector2.zero, uv2, config, uv2Decal: false);
	}

	public static void Render(this IGeometrie modele, MeshAugmentable mesh, Vector3 p, Vector2 uv, Vector2 uv2, Config config, bool uv2Decal)
	{
		_drawer.Render(modele, mesh, p, uv, uv2, config, uv2Decal);
	}

	public static void Count(this IGeometrie modele, int config, ref int vertexCount, ref int triangleCount)
	{
		modele.Count(GetConfig(config), ref vertexCount, ref triangleCount);
	}

	public static void Count(this IGeometrie modele, Config config, ref int vertexCount, ref int triangleCount)
	{
		vertexCount += config.iSommets.Length;
		triangleCount += config.triangles.Length;
	}
}
