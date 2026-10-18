using UnityEngine;

namespace Brix.Util;

public class Bezier
{
	public Classeur<Vector3> points = new Classeur<Vector3>();

	public int n = -1;

	public Vector3[] wLUT;

	public static Bezier bezier = new Bezier();

	public void Clear()
	{
		points.Clear();
		n = -1;
		wLUT = null;
	}

	public void AddPoint(Vector3 p)
	{
		points.Add(p);
		n++;
		wLUT = null;
	}

	public Vector3 Sample(float t)
	{
		if (n < 0)
		{
			return Vector3.zero;
		}
		int num = 0;
		int num2 = 0;
		if (wLUT == null)
		{
			wLUT = new Vector3[n + 1];
			int[] array = Calcule.Combinaisons(n);
			for (num = 0; num <= n; num++)
			{
				wLUT[num] = Vector3.zero;
				int[] array2 = Calcule.Combinaisons(num);
				for (num2 = 0; num2 <= num; num2++)
				{
					if (((num + num2) & 1) != 0)
					{
						wLUT[num] -= array2[num2] * array[num] * points[num2];
					}
					else
					{
						wLUT[num] += array2[num2] * array[num] * points[num2];
					}
				}
			}
		}
		Vector3 zero = Vector3.zero;
		float num3 = 1f;
		for (num = 0; num <= n; num++)
		{
			zero += wLUT[num] * num3;
			num3 *= t;
		}
		return zero;
	}

	public Vector3 Sample_2(float s)
	{
		if (n < 0)
		{
			return Vector3.zero;
		}
		if (n == 0)
		{
			return points[0];
		}
		if (n == 1)
		{
			return Vector3.Lerp(points[0], points[1], s);
		}
		s *= (float)n;
		if (s <= 1f)
		{
			return Interpolate3(points[0], points[1], points[2], 0.5f * s);
		}
		if (s >= (float)(n - 1))
		{
			return Interpolate3(points[n - 2], points[n - 1], points[n], 0.5f * (s - (float)n + 2f));
		}
		int num = Mathf.FloorToInt(s);
		float num2 = s - (float)num;
		return Vector3.Lerp(Interpolate3(points[num - 1], points[num], points[num + 1], 0.5f * (num2 + 1f)), Interpolate3(points[num], points[num + 1], points[num + 2], 0.5f * num2), num2);
	}

	private static Vector3 Interpolate3(Vector3 a, Vector3 b, Vector3 c, float t)
	{
		float num = 1f - t;
		return num * num * a + 2f * num * t * b + t * t * c;
	}

	public static void Init(Vector3[] points)
	{
		bezier.Clear();
		foreach (Vector3 p in points)
		{
			bezier.AddPoint(p);
		}
	}

	public static Vector3 Eval(float s)
	{
		return bezier.Sample(s);
	}
}
