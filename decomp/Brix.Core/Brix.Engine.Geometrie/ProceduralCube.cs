using Brix.Core;
using UnityEngine;

namespace Brix.Engine.Geometrie;

public class ProceduralCube : MonoBehaviour
{
	public class CubeNiveau : ICube, IGeometrie
	{
		public int niveau;

		public Vector3 GetSommet(int i)
		{
			Vector3 sommet = Cube.Modele.GetSommet(i);
			if (i < 4)
			{
				sommet.y = (float)niveau / 3f - 0.5f;
			}
			return sommet;
		}

		public Vector3 GetNormale(int i)
		{
			return Cube.Modele.GetNormale(i);
		}

		public Vector3 GetTangente(int i)
		{
			return Cube.Modele.GetTangente(i);
		}
	}

	public Vector3 CalculerPointMoyen(Vector3 centre, Vector3[] voisins)
	{
		bool[] array = new bool[8];
		for (int i = 0; i < 8; i++)
		{
			array[i] = voisins[i] == Vector3.zero;
		}
		Vector3 zero = Vector3.zero;
		int num = 0;
		for (int j = 0; j < 8; j++)
		{
			if (!array[j])
			{
				zero += voisins[j];
				num++;
			}
		}
		Vector3 vector = zero / num;
		int[] array2 = new int[4] { -1, -1, -1, -1 };
		int[] array3 = new int[4] { -1, -1, -1, -1 };
		int[] array4 = new int[4] { -1, -1, -1, -1 };
		for (int k = 0; k < 4; k++)
		{
			array2[((!array[2 * k]) ? 1u : 0u) | (uint)((!array[2 * k + 1]) ? 2 : 0)] = k;
			array3[((!array[k + 4]) ? 1u : 0u) | (uint)((!array[k]) ? 2 : 0)] = k;
			array4[((!array[k % 2 + k / 2 * 4 + 2]) ? 1u : 0u) | (uint)((!array[k % 2 + k / 2 * 4]) ? 2 : 0)] = k;
		}
		Vector3 result = vector;
		int[][] array5 = new int[3][] { array2, array3, array4 };
		for (int l = 0; l < 3; l++)
		{
			int[] array6 = array5[l];
			if (array6[3] >= 0 || (array6[1] >= 0 && array6[2] >= 0))
			{
				result[l] = centre[l];
			}
			else if (array6[0] >= 0)
			{
				result[l] = centre[l];
				if (array6[1] >= 0)
				{
					result[l]++;
				}
				if (array6[2] >= 0)
				{
					result[l]--;
				}
			}
		}
		return result;
	}

	public Vector3 CalculerPointMoyen(Vector3 centre, ICube[] cubes)
	{
		Vector3[] array = new Vector3[8];
		for (int i = 0; i < 8; i++)
		{
			Vector3 vector = new Vector3(i % 2 * 2 - 1, 1 - i / 4 * 2, 1 - i % 4 / 2 * 2);
			array[i] = cubes[i].GetSommet(i ^ 7) + vector;
		}
		return CalculerPointMoyen(centre, array);
	}

	public Vector3 CalculerPointMoyen(Vector3 centre, ICube[] cubes, int i)
	{
		Vector3 vector = new Vector3(i % 2 * 2 - 1, 1 - i / 4 * 2, 1 - i % 4 / 2 * 2);
		Vector3[] array = new Vector3[8];
		for (int j = 0; j < 8; j++)
		{
			array[j] = cubes[i % 2 + j % 2 + (i % 4 / 2 + j % 4 / 2) * 3 + (i / 4 + j / 4) * 9].GetSommet(j ^ 7);
		}
		return vector + CalculerPointMoyen(centre, array);
	}

	public static float ComputeSurfaceHeightFromNiveaux(int niveaux, float xOffset, float zOffset)
	{
		if (xOffset >= 0f - zOffset)
		{
			float num = (float)(niveaux & 3) / 3f - 0.5f;
			float num2 = (float)((niveaux >> 4) & 3) / 3f - 0.5f;
			float num3 = (float)((niveaux >> 8) & 3) / 3f - 0.5f;
			float num4 = num - num2;
			float num5 = num3 - num2;
			return num2 + (0.5f - xOffset) * num4 + (0.5f - zOffset) * num5;
		}
		float num6 = (float)((niveaux >> 8) & 3) / 3f - 0.5f;
		float num7 = (float)((niveaux >> 12) & 3) / 3f - 0.5f;
		float num8 = (float)(niveaux & 3) / 3f - 0.5f;
		float num9 = num6 - num7;
		float num10 = num8 - num7;
		return num7 + (0.5f + xOffset) * num9 + (0.5f + zOffset) * num10;
	}

	public static float ComputeSurfaceHeightFromLevels(int levels, float xOffset, float zOffset)
	{
		if (xOffset >= 0f - zOffset)
		{
			float num = (float)(levels & 0xFF) / 255f - 0.5f;
			float num2 = (float)((levels >> 8) & 0xFF) / 255f - 0.5f;
			float num3 = (float)((levels >> 24) & 0xFF) / 255f - 0.5f;
			float num4 = num - num2;
			float num5 = num3 - num2;
			return num2 + (0.5f - xOffset) * num4 + (0.5f - zOffset) * num5;
		}
		float num6 = (float)((levels >> 24) & 0xFF) / 255f - 0.5f;
		float num7 = (float)((levels >> 16) & 0xFF) / 255f - 0.5f;
		float num8 = (float)(levels & 0xFF) / 255f - 0.5f;
		float num9 = num6 - num7;
		float num10 = num8 - num7;
		return num7 + (0.5f + xOffset) * num9 + (0.5f + zOffset) * num10;
	}
}
