using System.Collections.Generic;
using Brix.Audio;
using Brix.Engine;
using Brix.Engine.Nature;
using Brix.Legacy;
using UnityEngine;

namespace Brix.Input.WorldEditor;

public class TreeBrush : BrushComponent
{
	[SerializeField]
	private AnimationCurve _falloff = new AnimationCurve();

	public Texture2D BlueNoiseMap;

	private bool _lastFrameGrowth;

	private static List<XYZ> _changedTrees = new List<XYZ>();

	public new TreeBrushSettings brushSettings
	{
		get
		{
			if (_brushSettings == null)
			{
				CreateSettings();
			}
			return _brushSettings as TreeBrushSettings;
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
		_brushSettings = new TreeBrushSettings();
	}

	public override void OnActionTerrain(Vector3? clickedVoxel, ClickInfo clickInfo)
	{
		if (clickInfo.actionName == "cam_action1" && !PickingUtility.isInViewMode())
		{
			bool flag = GrowTrees(_voxelRayHit.WorldHitVector, brushSettings.ConvertedSize, brushSettings.ConvertedIntensity, _unlimitedBrushHeight, _falloff, BlueNoiseMap);
			if (flag && !_lastFrameGrowth)
			{
				SoundEngine.Play("WorldEditor_Terrain_Tree_Growth_Start");
			}
			else if (!flag && _lastFrameGrowth)
			{
				SoundEngine.Play("WorldEditor_Terrain_Tree_Growth_Stop");
			}
			_lastFrameGrowth = flag;
		}
	}

	public override void OnLastActionTerrain(Vector3? clickedVoxel, ClickInfo clickInfo)
	{
		SoundEngine.Play("WorldEditor_Terrain_Tree_Growth_Stop");
		_lastFrameGrowth = false;
	}

	protected override void OnActiveChange()
	{
		SoundEngine.Play("WorldEditor_Terrain_Tree_Growth_Stop");
		_lastFrameGrowth = false;
	}

	protected override void UpdateBrushHighlight()
	{
		base.UpdateBrushHighlight();
		if (base.Active && !base.StandBy)
		{
			Picking.Instance.BrushHighlight.BrushColor = Picking.Instance.BrushHighlight.DefaultBrushColor;
		}
	}

	private static bool IsValidTreePosition(Vector3 position)
	{
		IVoxel voxel = Voxel.GetVoxel(XYZ.FromVector3(position - Vector3.up));
		if (!voxel.IsTerrain)
		{
			return false;
		}
		Vector3 vector = new Vector3
		{
			x = -1f
		};
		while (vector.x != 2f)
		{
			vector.z = -1f;
			while (vector.z != 2f)
			{
				voxel = Voxel.GetVoxel(XYZ.FromVector3(position + vector + Vector3.up));
				if (voxel.IsTerrain && !voxel.IsCrust)
				{
					return false;
				}
				voxel = Voxel.GetVoxel(XYZ.FromVector3(position + vector - Vector3.up * 2f));
				if (!voxel.IsTerrain || voxel.IsCrust)
				{
					return false;
				}
				vector.z += 1f;
			}
			vector.x += 1f;
		}
		return true;
	}

	private static bool IsNoiseOnMap(Vector3 position, Texture2D NoiseMap)
	{
		return NoiseMap == null || NoiseMap.GetPixel((int)position.x % 64, (int)position.z % 64) != Color.white;
	}

	public static bool GrowTrees(Vector3 center, int areaSize, float intensity, bool unlimitedInY, AnimationCurve falloff = null, Texture2D NoiseMap = null)
	{
		bool result = false;
		_changedTrees.Clear();
		BrushUtils.BeginChanges(VoxelDirtyProperties.Terrain | VoxelDirtyProperties.Trees);
		int num = Mathf.CeilToInt((float)areaSize / 2f);
		for (int i = -num; i <= num; i++)
		{
			for (int j = -num; j <= num; j++)
			{
				XYZ xYZ = XYZ.FromVector3(center) + new XYZ(i, 0, j);
				XYZ crustInColumn = BrushUtils.GetCrustInColumn(xYZ.x, xYZ.z);
				if (BrushUtils.IsInsideRange(crustInColumn.ToVector3(), center, areaSize, unlimitedInY) && IsNoiseOnMap(xYZ.ToVector3(), NoiseMap) && IsValidTreePosition(crustInColumn.ToVector3()))
				{
					if (!TreeRegistry.Instance.HasTree(crustInColumn))
					{
						_changedTrees.Add(crustInColumn);
					}
					float fraction = intensity * BrushUtils.GetFalloffFactorAt(xYZ.ToVector3(), falloff, center, areaSize);
					PlantRegistry.Remove(crustInColumn);
					if (!NatureRegistry.HasNatureAt(crustInColumn) && BrushUtils.GrowTree(crustInColumn, fraction))
					{
						result = true;
					}
				}
			}
		}
		BrushUtils.ApplyChanges(VoxelDirtyProperties.Trees);
		foreach (XYZ changedTree in _changedTrees)
		{
			for (int k = -1; k <= 1; k++)
			{
				for (int l = -1; l <= 1; l++)
				{
					XYZ xYZ2 = changedTree + new XYZ(k, 0, l);
					XYZ crustInColumn2 = BrushUtils.GetCrustInColumn(xYZ2.x, xYZ2.z);
					if (BrushUtils.IsInsideRange(crustInColumn2.ToVector3(), center, areaSize + 3, unlimitedInY))
					{
						BrushUtils.AddOrFixGrass(crustInColumn2);
					}
				}
			}
		}
		BrushUtils.ApplyChanges();
		GameSignals.Invoke(GameSignals.OnTerrainEdited);
		return result;
	}
}
