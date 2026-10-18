using UnityEngine;

namespace Brix.Legacy;

public static class Cubeur
{
	public const int dessus = 0;

	public const int devant = 1;

	public const int derriere = 2;

	public const int droite = 3;

	public const int gauche = 4;

	public const int dessous = 5;

	public static readonly Vector3[] Dirs;

	public static readonly Vector3[] Tangents1;

	public static readonly Vector3[] Tangents2;

	public static readonly int[] ScalaireDirs;

	public static readonly int[] ScalaireTangents1;

	public static readonly int[] ScalaireTangents2;

	public static Vector2[] Uvs;

	public static Vector2[] Uvs0;

	public static int[] Layers;

	public static Vector3[] Sommets;

	public static Vector3[] SommetsDefaut;

	public static int[] SommetIndices;

	public static int[] SommetsConfig;

	static Cubeur()
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
		Tangents1 = new Vector3[6]
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
		ScalaireTangents1 = new int[6] { 1, -1, 1, 1024, -1024, 1 };
		ScalaireTangents2 = new int[6] { -1024, -1048576, -1048576, -1048576, -1048576, 1024 };
		Uvs = new Vector2[12]
		{
			Vector2.zero,
			Vector2.zero,
			Vector2.zero,
			Vector2.zero,
			Vector2.zero,
			Vector2.zero,
			Vector2.zero,
			Vector2.zero,
			Vector2.zero,
			Vector2.zero,
			Vector2.zero,
			Vector2.zero
		};
		Uvs0 = new Vector2[12]
		{
			Vector2.zero,
			Vector2.zero,
			Vector2.zero,
			Vector2.zero,
			Vector2.zero,
			Vector2.zero,
			Vector2.zero,
			Vector2.zero,
			Vector2.zero,
			Vector2.zero,
			Vector2.zero,
			Vector2.zero
		};
		Layers = new int[12];
		Sommets = new Vector3[9]
		{
			new Vector3(-0.5f, 0.5f, 0.5f),
			new Vector3(0.5f, 0.5f, 0.5f),
			new Vector3(-0.5f, 0.5f, -0.5f),
			new Vector3(0.5f, 0.5f, -0.5f),
			new Vector3(-0.5f, -0.5f, 0.5f),
			new Vector3(0.5f, -0.5f, 0.5f),
			new Vector3(-0.5f, -0.5f, -0.5f),
			new Vector3(0.5f, -0.5f, -0.5f),
			new Vector3(0f, 0.5f, 0f)
		};
		SommetsDefaut = new Vector3[8]
		{
			new Vector3(-0.5f, 0.5f, 0.5f),
			new Vector3(0.5f, 0.5f, 0.5f),
			new Vector3(-0.5f, 0.5f, -0.5f),
			new Vector3(0.5f, 0.5f, -0.5f),
			new Vector3(-0.5f, -0.5f, 0.5f),
			new Vector3(0.5f, -0.5f, 0.5f),
			new Vector3(-0.5f, -0.5f, -0.5f),
			new Vector3(0.5f, -0.5f, -0.5f)
		};
		SommetIndices = new int[24]
		{
			0, 1, 2, 3, 1, 0, 5, 4, 2, 3,
			6, 7, 3, 1, 7, 5, 0, 2, 4, 6,
			6, 7, 4, 5
		};
		SommetsConfig = new int[9] { 21, 13, 19, 11, 52, 44, 50, 42, 1 };
		int[] array = new int[12]
		{
			3, 0, 3, 3, 1, 2, 2, 2, 0, 1,
			0, 3
		};
		int[] array2 = new int[12]
		{
			3, 2, 3, 3, 2, 1, 1, 1, 0, 0,
			1, 1
		};
		for (int i = 0; i < 12; i++)
		{
			ref Vector2 reference = ref Uvs[i];
			reference = new Vector2(4 * array[i] + 2, 4 * array2[i] + 2);
		}
		int[] array3 = new int[12]
		{
			6, 0, 6, 6, 5, 15, 15, 14, 7, 10,
			12, 4
		};
		for (int j = 0; j < 12; j++)
		{
			ref Vector2 reference2 = ref Uvs0[j];
			reference2 = new Vector2(0f, array3[j]);
		}
		int[] array4 = new int[12]
		{
			0, 1, 0, 0, 1, 3, 3, 2, 6, 5,
			4, 1
		};
		for (int k = 0; k < 12; k++)
		{
			Layers[k] = array4[k];
		}
	}
}
