using UnityEngine;

namespace Brix.Util;

public class CourbeCubique
{
	public Classeur<Vector3> points = new Classeur<Vector3>();

	public Classeur<Vector3> vitesses = new Classeur<Vector3>();

	public Classeur<Vector3[]> wLUT = new Classeur<Vector3[]>();

	public int n = -1;

	public static CourbeCubique courbe = new CourbeCubique();

	public void Clear()
	{
		points.Clear();
		vitesses.Clear();
		n = -1;
		wLUT = null;
	}

	public void AddPoint(Vector3 p, Vector3 v)
	{
		points.Add(p);
		vitesses.Add(v);
		n++;
	}

	public Vector3 Sample(float s)
	{
		if (n < 0)
		{
			return Vector3.zero;
		}
		if (n == 0)
		{
			return points[0];
		}
		s *= (float)n;
		int num = Mathf.Min(n - 1, Mathf.FloorToInt(s));
		float num2 = s - (float)num;
		if (wLUT == null)
		{
			wLUT.Alloc(n);
		}
		if (wLUT[num] == null)
		{
			wLUT[num] = CubiqueCoefs(points[num], vitesses[num], points[num + 1], vitesses[num + 1]);
		}
		return wLUT[num][0] + num2 * (wLUT[num][1] + num2 * (wLUT[num][2] + num2 * wLUT[num][3]));
	}

	private static Vector3[] CubiqueCoefs(Vector3 a, Vector3 va, Vector3 b, Vector3 vb)
	{
		return new Vector3[4]
		{
			a,
			va,
			3f * (b - a) - 2f * va - vb,
			vb + va - 2f * (b - a)
		};
	}

	public static void Init(Vector3[] points, Vector3[] vitesses)
	{
		courbe.Clear();
		for (int i = 0; i < points.Length; i++)
		{
			courbe.AddPoint(points[i], vitesses[i]);
		}
	}

	public static Vector3 Eval(float s)
	{
		return courbe.Sample(s);
	}
}
