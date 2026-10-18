using System.Collections.Generic;
using System.Linq;
using Brix.Audio;
using Brix.Engine;
using Brix.Engine.Nature;
using Brix.Utils;
using UnityEngine;

namespace Brix.Input.WorldEditor;

public class SmoothBrush : BrushComponent
{
	private int RandomWeight = 1;

	private int RandomHeight = 1;

	public new SmoothBrushSettings brushSettings
	{
		get
		{
			if (_brushSettings == null)
			{
				CreateSettings();
			}
			return _brushSettings as SmoothBrushSettings;
		}
		set
		{
			if (value != null)
			{
				_brushSettings = value;
			}
			else
			{
				CreateSettings();
			}
		}
	}

	protected override void CreateSettings()
	{
		_brushSettings = new SmoothBrushSettings();
	}

	private int GetI(int x, int z, int L)
	{
		return x + L * z;
	}

	public override void OnActionTerrain(Vector3? clickedVoxel, ClickInfo clickInfo)
	{
		if (!(clickInfo.actionName == "cam_action1") || PickingUtility.isInViewMode())
		{
			return;
		}
		BrushUtils.BeginChanges(VoxelDirtyProperties.Terrain | VoxelDirtyProperties.Trees);
		int num = Mathf.CeilToInt(brushSettings.ConvertedSize / 2);
		int num2 = num * 2 + 1;
		int[] array = new int[num2 * num2];
		XYZ[] array2 = new XYZ[num2 * num2];
		for (int i = -num; i <= num; i++)
		{
			for (int j = -num; j <= num; j++)
			{
				int num3 = num;
				while ((float)num3 + _voxelRayHit.HitVector.y > 1f && num3 > -num && Voxel.IsEmpty(XYZ.FromVector3(new Vector3(i, num3, j) + _voxelRayHit.HitVector)))
				{
					num3--;
				}
				int i2 = GetI(i + num, j + num, num2);
				ref XYZ reference = ref array2[i2];
				reference = XYZ.FromVector3(new Vector3(i, num3, j) + _voxelRayHit.HitVector);
				array[i2] = Voxel.GetNiveau(array2[i2]) + array2[i2].y * 3;
			}
		}
		float[,] array3 = new float[5, 5]
		{
			{ 1f, 1f, 1f, 1f, 1f },
			{ 1f, 1f, 1f, 1f, 1f },
			{ 1f, 1f, 1f, 1f, 1f },
			{ 1f, 1f, 1f, 1f, 1f },
			{ 1f, 1f, 1f, 1f, 1f }
		};
		for (int k = 0; k < array.Length; k++)
		{
			int num4 = k % num2;
			int num5 = k / num2;
			float num6 = 0f;
			float num7 = 0f;
			for (int l = 0; l < array3.GetLength(0); l++)
			{
				for (int m = 0; m < array3.GetLength(1); m++)
				{
					int num8 = num4 + l - 2;
					int num9 = num5 + m - 2;
					if (num8 >= 0 && num8 < num2 && num9 >= 0 && num9 < num2)
					{
						float num10 = array3[l, m];
						num6 += (float)array[GetI(num8, num9, num2)] * num10;
						num7 += num10;
					}
				}
			}
			float num11 = Random.Range(0f, (float)RandomWeight * num7);
			float num12 = (float)array[k] + Random.Range(-1f, 1f) * (float)RandomHeight;
			num7 += num11;
			num6 += num12 * num11;
			float b = num6 / num7;
			float a = array[k];
			array[k] = Mathf.RoundToInt(Mathf.Lerp(a, b, brushSettings.Intensity));
		}
		Dictionary<XZ, int> dictionary = CollectionPool.RequestDictionary<XZ, int>();
		for (int n = 0; n < array.Length; n++)
		{
			int num13 = array[n] % 3;
			int num14 = (array[n] - num13) / 3 + 1;
			if (Voxel.IsCrust(array2[n]))
			{
				int x = array2[n].x;
				int z = array2[n].z;
				NatureRegistry.RemoveNatureAt(array2[n]);
				BrushUtils.RemoveTree(array2[n]);
				if (num14 >= VoxelEngine.GetColumnBottom(x, z) + 3)
				{
					VoxelEngine.SetColumnTop(x, z, (float)num14 + 1f / 3f * (float)num13);
					dictionary.Add(new XZ(x, z), num14 - 1);
				}
				else
				{
					VoxelEngine.RemoveColumn(x, z);
					dictionary.Add(new XZ(x, z), 0);
				}
			}
		}
		BrushUtils.ApplyChanges(VoxelDirtyProperties.Trees);
		if (dictionary.Any() && lastSound + soundDelay <= Time.unscaledTime)
		{
			SoundEngine.Play("WorldEditor_Terrain_Smooth");
			lastSound = Time.unscaledTime;
		}
		HashSet<XZ> hashSet = CollectionPool.RequestHashSet<XZ>();
		BrushUtils.PopulateAdjacentColumns(dictionary, hashSet);
		num++;
		for (int num15 = -num; num15 <= num; num15++)
		{
			for (int num16 = -num; num16 <= num; num16++)
			{
				XYZ currentVoxel = XYZ.FromVector3(_voxelRayHit.HitVector) + new XYZ(num15, 0, num16);
				if (IsInsideBounds(currentVoxel) && hashSet.Contains(new XZ(currentVoxel.x, currentVoxel.z)))
				{
					XYZ at = BrushUtils.ManualGetCrustInColumn(new XZ(currentVoxel.x, currentVoxel.z), dictionary);
					BrushUtils.AddOrFixGrass(at, dictionary);
				}
			}
		}
		hashSet.Dispose();
		dictionary.Dispose();
		BrushUtils.ApplyChanges();
		GameSignals.Invoke(GameSignals.OnTerrainEdited);
	}

	protected override void UpdateBrushHighlight()
	{
		base.UpdateBrushHighlight();
		if (base.Active && !base.StandBy)
		{
			Picking.Instance.BrushHighlight.BrushColor = Picking.Instance.BrushHighlight.DefaultBrushColor;
		}
	}
}
