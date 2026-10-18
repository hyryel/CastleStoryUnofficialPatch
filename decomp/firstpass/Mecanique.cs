using Brix.Utils.Logging;
using UnityEngine;

public class Mecanique
{
	public delegate void MyFunction(int i);

	protected class Voxel
	{
		public static int GetScalaire(int x, int y, int z)
		{
			return x | (z << 10) | (y << 20);
		}
	}

	private static BrixLogger logger = new BrixLogger(typeof(Mecanique).FullName);

	private static float peser(MyFunction f)
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		for (int i = 0; i < 10000000; i++)
		{
			f(i);
		}
		return Time.realtimeSinceStartup - realtimeSinceStartup;
	}

	public static void TestScalaire()
	{
		Scalaire s = Scalaire.zero;
		Scalaire s2 = new Scalaire(1, 0, 0);
		Scalaire s3 = new Scalaire(0, 1, 0);
		SplitScalaire ss = new SplitScalaire(0, 0, 0);
		SplitScalaire ss2 = new SplitScalaire(1, 0, 0);
		SplitScalaire ss3 = new SplitScalaire(0, 1, 0);
		Vector3 v = Vector3.zero;
		Vector3 v2 = new Vector3(1f, 0f, 0f);
		Vector3 v3 = new Vector3(0f, 1f, 0f);
		int x = 0;
		int x2 = 1;
		int x3 = 2;
		long l = 0L;
		long l2 = 1L;
		long l3 = 2L;
		MyFunction[] array = new MyFunction[19]
		{
			delegate
			{
				s += s3;
			},
			delegate
			{
				s.PlusEgal(s3);
			},
			delegate
			{
				ss += ss3;
			},
			delegate
			{
				ss.PlusEgal(ss3);
			},
			delegate
			{
				v += v3;
			},
			delegate
			{
				x += x3;
			},
			delegate
			{
				l += l3;
			},
			delegate
			{
				s = s2 + s3;
				s3 = s + s2;
			},
			delegate
			{
				ss = ss2 + ss3;
				ss3 = ss + ss2;
			},
			delegate
			{
				v = v2 + v3;
				v3 = v + v2;
			},
			delegate
			{
				x = x2 + x3;
				x3 = x + x2;
			},
			delegate
			{
				l = l2 + l3;
				l3 = l + l2;
			},
			delegate(int i)
			{
				s = new Scalaire(i, i, i);
			},
			delegate(int i)
			{
				ss = new SplitScalaire(i, i, i);
			},
			delegate(int i)
			{
				v = new Vector3(i, i, i);
			},
			delegate(int i)
			{
				x = Voxel.GetScalaire(i, i, i);
			},
			delegate
			{
				s.x = s.y + s.z;
			},
			delegate
			{
				ss.x = (short)(ss.y + ss.z);
			},
			delegate
			{
				v.x = v.y + v.z;
			}
		};
		string[] array2 = new string[19]
		{
			"Scalaire += Scalaire", "Scalaire.PlusEgal()", "SplitScalaire += SplitScalaire", "SplitScalaire.PlusEgal()", "Vector3 += Vector3", "int += int", "long += long", "Scalaire = Scalaire + Scalaire", "SplitScalaire = SplitScalaire + SplitScalaire", "Vector3 = Vector3 + Vector3",
			"int = int + int", "long = long + long", "new Scalaire(i,i,i)", "new SplitScalaire(i,i,i)", "new Vector3(i,i,i)", "Voxel.GetScalaire(i,i,i)", "Scalaire x=y+z", "SplitScalaire x=y+z", "Vector3 x=y+z"
		};
		float num = peser(delegate
		{
		});
		logger.Info("Vide : " + num);
		for (int num2 = 0; num2 < array.Length; num2++)
		{
			logger.Info(array2[num2] + " : " + (peser(array[num2]) - num));
		}
		logger.Info("3627.Inv32() = " + 3627.Chien12());
	}
}
