using UnityEngine;

namespace Brix.Legacy.Geometrie;

public static class Cube
{
	public static Face[] f;

	public static Sommet[] s;

	public static Arete[] a;

	static Cube()
	{
		f = new Face[6];
		for (int i = 0; i < 6; i++)
		{
			f[i] = new Face(i);
		}
		s = new Sommet[8];
		for (int j = 0; j < 8; j++)
		{
			s[j] = new Sommet(j);
		}
		a = new Arete[24];
		for (int k = 0; k < 24; k++)
		{
			a[k] = new Arete(k);
		}
		f[0].s = new Sommet[4]
		{
			s[0],
			s[1],
			s[3],
			s[2]
		};
		f[0].a = new Arete[4]
		{
			a[0],
			a[1],
			a[2],
			a[3]
		};
		f[0].normale = Vector3.up;
		f[1].s = new Sommet[4]
		{
			s[1],
			s[0],
			s[4],
			s[5]
		};
		f[1].a = new Arete[4]
		{
			a[12],
			a[4],
			a[8],
			a[17]
		};
		f[1].normale = Vector3.forward;
		f[2].s = new Sommet[4]
		{
			s[2],
			s[3],
			s[7],
			s[6]
		};
		f[2].a = new Arete[4]
		{
			a[14],
			a[6],
			a[10],
			a[19]
		};
		f[2].normale = -Vector3.forward;
		f[3].s = new Sommet[4]
		{
			s[3],
			s[1],
			s[5],
			s[7]
		};
		f[3].a = new Arete[4]
		{
			a[13],
			a[5],
			a[9],
			a[18]
		};
		f[3].normale = Vector3.right;
		f[4].s = new Sommet[4]
		{
			s[0],
			s[2],
			s[6],
			s[4]
		};
		f[4].a = new Arete[4]
		{
			a[15],
			a[7],
			a[11],
			a[16]
		};
		f[4].normale = -Vector3.right;
		f[5].s = new Sommet[4]
		{
			s[6],
			s[7],
			s[5],
			s[4]
		};
		f[5].a = new Arete[4]
		{
			a[22],
			a[21],
			a[20],
			a[23]
		};
		f[5].normale = -Vector3.up;
		Vector3 vector = Vector3.right * -0.5f;
		Vector3 vector2 = Vector3.right * 0.5f;
		Vector3 vector3 = Vector3.up * 0.5f;
		Vector3 vector4 = Vector3.up * -0.5f;
		Vector3 vector5 = Vector3.forward * 0.5f;
		Vector3 vector6 = Vector3.forward * -0.5f;
		s[0].position = vector + vector3 + vector5;
		s[1].position = vector2 + vector3 + vector5;
		s[2].position = vector + vector3 + vector6;
		s[3].position = vector2 + vector3 + vector6;
		s[4].position = vector + vector4 + vector5;
		s[5].position = vector2 + vector4 + vector5;
		s[6].position = vector + vector4 + vector6;
		s[7].position = vector2 + vector4 + vector6;
		for (int l = 0; l < 6; l++)
		{
			for (int m = 0; m < 4; m++)
			{
				Ajouter(f[l].s[m].a, f[l].a[m]);
				f[l].a[m].f[0] = f[l];
				f[l].a[m].s[0] = f[l].s[m];
				f[l].a[m].s[1] = f[l].s[(m + 1) % 4];
				f[l].a[m].vecteur = f[l].a[m].s[1].position - f[l].a[m].s[0].position;
			}
		}
		for (int n = 0; n < 8; n++)
		{
			Vector3 lhs = s[n].a[1].vecteur - s[n].a[0].vecteur;
			Vector3 rhs = s[n].a[2].vecteur - s[n].a[1].vecteur;
			Vector3 rhs2 = s[n].a[0].vecteur + s[n].a[1].vecteur + s[n].a[2].vecteur;
			if (Vector3.Dot(Vector3.Cross(lhs, rhs), rhs2) < 0f)
			{
				Arete arete = s[n].a[1];
				s[n].a[1] = s[n].a[2];
				s[n].a[2] = arete;
			}
			for (int num = 0; num < 3; num++)
			{
				s[n].f[num] = s[n].a[num].f[0];
			}
		}
		for (int num2 = 0; num2 < 12; num2++)
		{
			a[num2].f[1] = a[num2 + 12].f[0];
			a[num2 + 12].f[1] = a[num2].f[0];
		}
	}

	private static void Ajouter<T>(T[] array, T obj)
	{
		int i;
		for (i = 0; i < array.Length && array[i] != null; i++)
		{
		}
		array[i] = obj;
	}
}
