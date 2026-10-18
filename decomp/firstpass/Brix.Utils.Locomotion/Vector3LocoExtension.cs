using Brix.Util;
using UnityEngine;

namespace Brix.Utils.Locomotion;

public static class Vector3LocoExtension
{
	public static Vector3 Flatten(this Vector3 a)
	{
		return new Vector3(a.x, 0f, a.z);
	}

	public static Vector3 Flatten(this Vector3 a, Vector3 b)
	{
		return new Vector3(a.x, b.y, a.z);
	}

	public static Vector3 Flatten(this Vector3 a, float y)
	{
		return new Vector3(a.x, y, a.z);
	}

	public static Vector3 Mul(this Vector3 a, Vector3 b)
	{
		return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
	}

	public static int Manhattan(this Vector3 a)
	{
		return Mathf.Abs(Mathf.RoundToInt(a.x)) + Mathf.Abs(Mathf.RoundToInt(a.y)) + Mathf.Abs(Mathf.RoundToInt(a.z));
	}

	public static Vector3 Rounded(this Vector3 a)
	{
		return Calcule.RoundVecteur(a);
	}

	public static string ToPreciseString(this Vector3 v)
	{
		return "(" + v.x + ", " + v.y + ", " + v.z + ")";
	}
}
