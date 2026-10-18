using System.Collections.Generic;
using System.Linq;
using Brix.Engine;
using Brix.Util;
using UnityEngine;

namespace Brix.Utils;

public static class PreprocessedRotations
{
	public static readonly Quaternion[] RotationForAxisOrients = GenerateRotationForAxisOrients();

	public static readonly Quaternion[] DiscreteRotationForIndexes = GenerateDiscreteRotationForIndexes();

	public static readonly AxisOrient[] AxisOrientForIndexes = GetAxisOrientForIndexes();

	private static readonly int[] RotationIndexes = PopulateIndexesForDiscreteRotations();

	private const int AngleVariations = 8;

	public static readonly AxisOrient[] InverseAxisOrient = new AxisOrient[6]
	{
		AxisOrient.down,
		AxisOrient.back,
		AxisOrient.forward,
		AxisOrient.left,
		AxisOrient.right,
		AxisOrient.up
	};

	private static Quaternion[] GenerateDiscreteRotationForIndexes()
	{
		HashSet<XYZ> hashSet = new HashSet<XYZ>();
		for (int i = 0; i < 360; i += 90)
		{
			for (int j = 0; j < 360; j += 90)
			{
				for (int k = 0; k < 360; k += 90)
				{
					hashSet.Add(XYZ.FromVector3(Quaternion.Euler(i, j, k).eulerAngles));
				}
			}
		}
		return hashSet.Select((XYZ a) => Quaternion.Euler(a.ToVector3())).ToArray();
	}

	private static int[] PopulateIndexesForDiscreteRotations()
	{
		int[] array = new int[512];
		for (int i = 0; i < 8; i++)
		{
			for (int j = 0; j < 8; j++)
			{
				for (int k = 0; k < 8; k++)
				{
					int num = ((i & 7) * 8 + (j & 7)) * 8 + (k & 7);
					Quaternion q = Quaternion.Euler((float)i * 45f, (float)j * 45f, (float)k * 45f);
					array[num] = GetBestRotationIndexFromRotation(q);
				}
			}
		}
		return array;
	}

	private static Quaternion[] GenerateRotationForAxisOrients()
	{
		return (from i in Enumerable.Range(0, 6)
			select Calcule.FaceIndexToRotationDeBase((byte)i)).ToArray();
	}

	private static int GetBestRotationIndexFromRotation(Quaternion q)
	{
		return Calcule.Max(Enumerable.Range(0, DiscreteRotationForIndexes.Length), (int i) => Quaternion.Dot(q, DiscreteRotationForIndexes[i]));
	}

	public static AxisOrient GetBestAxisOrientFromRotation(Quaternion q)
	{
		return (AxisOrient)Calcule.Max(Enumerable.Range(0, RotationForAxisOrients.Length), (int i) => Quaternion.Dot(q, RotationForAxisOrients[i]));
	}

	private static AxisOrient[] GetAxisOrientForIndexes()
	{
		return DiscreteRotationForIndexes.Select(GetBestAxisOrientFromRotation).ToArray();
	}

	public static int RotationToDiscreteIndex(Quaternion q)
	{
		Vector3 eulerAngles = q.eulerAngles;
		XYZ xYZ = XYZ.FromVector3(eulerAngles / 45f);
		int num = ((xYZ.x & 7) * 8 + (xYZ.y & 7)) * 8 + (xYZ.z & 7);
		return RotationIndexes[num];
	}

	public static bool IsAlignedTo(this AxisOrient a, AxisOrient b)
	{
		return a == b || InverseAxisOrient[(int)a] == b;
	}
}
