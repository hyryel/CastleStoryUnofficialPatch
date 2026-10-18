using System;
using System.Collections.Generic;
using Brix.Engine.Geometrie;
using UnityEngine;

namespace Brix.Engine;

public class CalculateurDeNiveaux<IVoxel>
{
	public Func<int, IVoxel> GetVoxel;

	public Func<int, bool> GetCheck;

	public Func<IVoxel, bool> IsDessus;

	public Func<IVoxel, bool> IsDessous;

	public Func<IVoxel, int> GetNiveau;

	private Dictionary<int, float> i2topHautGauche = new Dictionary<int, float>();

	public const int NiveauMax = 3;

	public const float HauteurMax = 0.5f;

	public const float HauteurMin = -0.5f;

	public const float HauteurNeutre = 0f;

	public float GetTopHautGauche(int scalaire)
	{
		if (!i2topHautGauche.TryGetValue(scalaire, out var value))
		{
			value = CalculerTopHautGauche(scalaire);
			i2topHautGauche.Add(scalaire, value);
		}
		return value;
	}

	public void OublierTopHautGauche(int scalaire)
	{
		i2topHautGauche.Remove(scalaire);
	}

	private float CalculerTopHautGauche(int scalaire)
	{
		int num = 0;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		for (int i = 0; i < 4; i++)
		{
			int arg = scalaire + (i % 2 - 1) + (1 - i / 2) * 1024;
			if (!GetCheck(arg))
			{
				flag = true;
				continue;
			}
			IVoxel val = GetVoxel(arg);
			if (IsDessus(val))
			{
				num += GetNiveau(val);
				flag2 = true;
				continue;
			}
			if (IsDessous(val))
			{
				num += GetNiveauDessous(val);
				flag3 = true;
				continue;
			}
			return 0.5f;
		}
		if (flag2 && flag3)
		{
			return 0f;
		}
		if (flag)
		{
			return -0.5f;
		}
		int num2 = (num + 2) / 4;
		return Mathf.Lerp(-0.5f, 0.5f, (float)num2 / 3f);
	}

	private int GetNiveauDessous(IVoxel iVoxel)
	{
		return GetNiveau(iVoxel);
	}

	public Vector3 GetSommet(int scalaire, int i, bool dessous)
	{
		Vector3 sommet = Cube.Modele.GetSommet(i);
		bool flag = i >= 4;
		if (flag == dessous)
		{
			int scalaire2 = scalaire + i % 2 - i / 2 % 2 * 1024;
			sommet.y = GetTopHautGauche(scalaire2);
			if (flag)
			{
				sommet.y = 0f - sommet.y;
			}
		}
		return sommet;
	}

	public void OublierSommets(int scalaire)
	{
		for (int i = 0; i < 4; i++)
		{
			int scalaire2 = scalaire + i % 2 - i / 2 % 2 * 1024;
			OublierTopHautGauche(scalaire2);
		}
	}
}
