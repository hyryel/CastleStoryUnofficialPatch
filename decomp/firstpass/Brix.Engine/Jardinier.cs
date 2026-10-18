using System.Collections.Generic;
using UnityEngine;

namespace Brix.Engine;

public class Jardinier
{
	private readonly Dictionary<XYZ, int> m2i = new Dictionary<XYZ, int>();

	private readonly List<MegaVoxel> listeMV = new List<MegaVoxel>();

	private int iMega;

	private const int MaxWorkPerFrame = 100;

	public int WorkPerFrame;

	public void Travailler()
	{
		for (int i = 0; i < WorkPerFrame; i++)
		{
			TraiterMegaVoxel(listeMV[iMega++ % listeMV.Count]);
		}
	}

	public void AjouterMegaVoxel(MegaVoxel m)
	{
		if (!m2i.ContainsKey(m.scalaire))
		{
			m2i.Add(m.scalaire, listeMV.Count);
			listeMV.Add(m);
			WorkPerFrame = Mathf.Min(listeMV.Count, 100);
		}
	}

	public void EnleverMegaVoxel(MegaVoxel m)
	{
		if (m2i.ContainsKey(m.scalaire))
		{
			int num = m2i[m.scalaire];
			listeMV[num] = listeMV[listeMV.Count - 1];
			m2i[listeMV[num].scalaire] = num;
			m2i.Remove(m.scalaire);
			listeMV.RemoveAt(listeMV.Count - 1);
			WorkPerFrame = Mathf.Min(listeMV.Count, 100);
		}
	}

	private void TraiterMegaVoxel(MegaVoxel m)
	{
		m.CalculateNatureLod();
	}
}
