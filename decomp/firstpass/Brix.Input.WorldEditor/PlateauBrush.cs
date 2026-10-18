using Brix.Audio;
using Brix.Engine;
using Brix.Engine.Nature;
using UnityEngine;

namespace Brix.Input.WorldEditor;

public class PlateauBrush : BrushComponent
{
	[SerializeField]
	private AnimationCurve _falloff = new AnimationCurve();

	private Vector3 _height;

	private Vector3 _firstHeight;

	public new PlateauBrushSettings brushSettings
	{
		get
		{
			if (_brushSettings == null)
			{
				CreateSettings();
			}
			return _brushSettings as PlateauBrushSettings;
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
		_brushSettings = new PlateauBrushSettings();
	}

	public override void OnFirstActionTerrain(Vector3? clickedVoxel, ClickInfo clickInfo)
	{
		if (clickInfo.actionName == "cam_action1" && !PickingUtility.isInViewMode())
		{
			_firstHeight = _voxelRayHit.HitVector;
			_firstHeight.y += ((!brushSettings.UseLevelOverride) ? VoxelEngine.GetVolume(_voxelRayHit.HitVector) : ((float)brushSettings.LevelOverride / 3f));
		}
	}

	public override void OnActionTerrain(Vector3? clickedVoxel, ClickInfo clickInfo)
	{
		if (!(clickInfo.actionName == "cam_action1") || PickingUtility.isInViewMode())
		{
			return;
		}
		if (!brushSettings.LockHeight)
		{
			_height = _voxelRayHit.HitVector;
			_height.y += ((!brushSettings.UseLevelOverride) ? VoxelEngine.GetVolume(_voxelRayHit.HitVector) : ((float)brushSettings.LevelOverride / 3f));
		}
		else
		{
			_height = _firstHeight;
		}
		bool flag = false;
		if (!(_height.y > 0f))
		{
			return;
		}
		BrushUtils.BeginChanges(VoxelDirtyProperties.Terrain | VoxelDirtyProperties.Trees);
		int num = Mathf.CeilToInt(brushSettings.ConvertedSize / 2);
		for (int i = -num; i <= num; i++)
		{
			for (int j = -num; j <= num; j++)
			{
				int num2 = num;
				while ((float)num2 + _voxelRayHit.HitVector.y > 1f && num2 > -num && Voxel.IsEmpty(XYZ.FromVector3(new Vector3(i, num2, j) + _voxelRayHit.HitVector)))
				{
					num2--;
				}
				Vector3 vector = _voxelRayHit.HitVector + new Vector3(i, num2, j);
				vector.y += VoxelEngine.GetVolume(vector);
				XYZ crustInColumn = BrushUtils.GetCrustInColumn((int)vector.x, (int)vector.z);
				if (IsInsideBrushRange(crustInColumn.ToVector3()))
				{
					NatureRegistry.RemoveNatureAt(crustInColumn);
					BrushUtils.RemoveTree(crustInColumn);
					float num3 = GetFalloffFactorAt(vector, _falloff) * brushSettings.Intensity;
					if (vector.y > _height.y)
					{
						float num4 = vector.y - _height.y;
						num3 = ((!(num3 <= num4)) ? num4 : num3);
						VoxelEngine.SetColumnType((int)vector.x, (int)vector.z, VoxelTerrainType.Sand);
						VoxelEngine.AddColumnHeight((int)vector.x, (int)vector.z, 0f - num3);
						flag = true;
					}
					else if (vector.y < _height.y)
					{
						float num5 = _height.y - vector.y;
						num3 = ((!(num3 <= num5)) ? num5 : num3);
						VoxelEngine.SetColumnType((int)vector.x, (int)vector.z, VoxelTerrainType.Sand);
						VoxelEngine.AddColumnHeight((int)vector.x, (int)vector.z, num3);
						flag = true;
					}
				}
			}
		}
		BrushUtils.ApplyChanges(VoxelDirtyProperties.Terrain | VoxelDirtyProperties.Trees);
		if (flag && lastSound + soundDelay <= Time.unscaledTime)
		{
			SoundEngine.Play("WorldEditor_Terrain_Plateau");
			lastSound = Time.unscaledTime;
		}
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
