using System;
using System.Collections.Generic;
using UnityEngine;

namespace Brix.Engine;

public abstract class VoxelChecker
{
	public abstract bool GetVoxelCheck(int scalaire);
}
public class VoxelChecker<IVoxel> : VoxelChecker
{
	private const int mvMasque = 1057997808;

	private const int step = -1048576;

	private const int mvStep = -16777216;

	private HashSet<int> mvSurface;

	private Dictionary<int, bool> scalaires;

	private Dictionary<int, bool> mvScalaires;

	private int mvMin = 1057997808;

	private int mvMax = 0;

	private Func<int, bool> CheckHorsBornes;

	private bool CheckHorsBornesDefault(int scalaire)
	{
		return false;
	}

	public VoxelChecker(IEnumerable<IVoxel> voxels, Func<IVoxel, int> GetScalaire, Func<IVoxel, bool> GetCheck, IEnumerable<IVoxel> voxelsDessus, Func<int, bool> CheckHorsBornes = null)
	{
		scalaires = new Dictionary<int, bool>();
		mvSurface = new HashSet<int>();
		foreach (IVoxel voxel in voxels)
		{
			int num = GetScalaire(voxel);
			bool flag = GetCheck(voxel);
			scalaires[num] = flag;
			if (flag)
			{
				int num2 = num & 0x3F0FC3F0;
				mvSurface.Add(num2);
				Donnees.Diminish(ref mvMin, num2);
				Donnees.Augment(ref mvMax, num2 + Mathf.Abs(-16777216));
			}
		}
		foreach (IVoxel voxelsDessu in voxelsDessus)
		{
			int num3 = GetScalaire(voxelsDessu);
			scalaires[num3 + 1048576] = false;
			int item = (num3 + 1048576) & 0x3F0FC3F0;
			mvSurface.Add(item);
		}
		mvScalaires = new Dictionary<int, bool>();
		this.CheckHorsBornes = ((CheckHorsBornes != null) ? CheckHorsBornes : new Func<int, bool>(CheckHorsBornesDefault));
	}

	public override bool GetVoxelCheck(int scalaire)
	{
		if (scalaire < mvMin || scalaire >= mvMax)
		{
			if (scalaire - 1048576 < mvMax)
			{
				return GetVoxelCheck_Direct(scalaire);
			}
			return CheckHorsBornes(scalaire);
		}
		int num = scalaire & 0x3F0FC3F0;
		if (!mvSurface.Contains(num))
		{
			return GetMegaVoxelCheck_Direct(num);
		}
		return GetVoxelCheck_Direct(scalaire);
	}

	private bool GetVoxelCheck_Direct(int scalaire)
	{
		if (scalaires.TryGetValue(scalaire, out var value))
		{
			return value;
		}
		value = CalculerVoxelCheck(scalaire);
		scalaires.Add(scalaire, value);
		return value;
	}

	private bool GetMegaVoxelCheck_Direct(int canonique)
	{
		if (mvScalaires.TryGetValue(canonique, out var value))
		{
			return value;
		}
		value = CalculerMegaVoxelCheck(canonique);
		mvScalaires.Add(canonique, value);
		return value;
	}

	private bool CalculerMegaVoxelCheck(int canonique)
	{
		int num = canonique + -16777216;
		if (num < mvMin || num >= mvMax)
		{
			return CheckHorsBornes(num);
		}
		if (!mvSurface.Contains(num))
		{
			return GetMegaVoxelCheck_Direct(num);
		}
		return GetVoxelCheck_Direct(canonique + -1048576);
	}

	private bool CalculerVoxelCheck(int scalaire)
	{
		int num = scalaire + -1048576;
		if (num < mvMin || num >= mvMax)
		{
			return CheckHorsBornes(num);
		}
		return GetVoxelCheck_Direct(num);
	}
}
