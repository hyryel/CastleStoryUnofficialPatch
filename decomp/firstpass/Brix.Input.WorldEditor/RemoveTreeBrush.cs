using System.Collections.Generic;
using Brix.Audio;
using Brix.Engine;
using Brix.Legacy;
using UnityEngine;

namespace Brix.Input.WorldEditor;

public class RemoveTreeBrush : BrushComponent
{
	[SerializeField]
	private AnimationCurve _falloff = new AnimationCurve();

	private bool _lastFrameGrowth;

	private List<XYZ> _changedTrees = new List<XYZ>();

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

	public override void OnActionAny(bool actioning, ClickInfo clickInfo)
	{
		if (!(clickInfo.actionName == "cam_action1") || PickingUtility.isInViewMode())
		{
			return;
		}
		bool flag = false;
		_changedTrees.Clear();
		BrushUtils.BeginChanges(VoxelDirtyProperties.Terrain | VoxelDirtyProperties.Trees);
		int num = Mathf.CeilToInt(brushSettings.ConvertedSize / 2);
		for (int i = -num; i <= num; i++)
		{
			for (int j = -num; j <= num; j++)
			{
				XYZ xYZ = _voxelRayHit.VoxelPosition + new XYZ(i, 0, j);
				XYZ crustInColumn = BrushUtils.GetCrustInColumn(xYZ.x, xYZ.z);
				if (IsInsideBrushRange(crustInColumn.ToVector3()) && TreeRegistry.Instance.HasTree(crustInColumn))
				{
					_changedTrees.Add(crustInColumn);
					float fraction = brushSettings.ConvertedIntensity * GetFalloffFactorAt(xYZ.ToVector3(), _falloff);
					if (BrushUtils.UngrowTree(crustInColumn, fraction))
					{
						flag = true;
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
					if (IsInsideBrushRange(crustInColumn2.ToVector3(), brushSettings.ConvertedSize + 3))
					{
						BrushUtils.AddOrFixGrass(crustInColumn2);
					}
				}
			}
		}
		BrushUtils.ApplyChanges();
		GameSignals.Invoke(GameSignals.OnTerrainEdited);
		if (flag && !_lastFrameGrowth)
		{
			SoundEngine.Play("WorldEditor_Terrain_Tree_Decrease_Start");
		}
		else if (!flag && _lastFrameGrowth)
		{
			SoundEngine.Play("WorldEditor_Terrain_Tree_Decrease_Stop");
		}
		_lastFrameGrowth = flag;
	}

	public override void OnLastActionTerrain(Vector3? clickedVoxel, ClickInfo clickInfo)
	{
		SoundEngine.Play("WorldEditor_Terrain_Tree_Decrease_Stop");
		_lastFrameGrowth = false;
	}

	protected override void OnActiveChange()
	{
		SoundEngine.Play("WorldEditor_Terrain_Tree_Decrease_Stop");
		_lastFrameGrowth = false;
	}

	protected override void UpdateBrushHighlight()
	{
		base.UpdateBrushHighlight();
		if (base.Active && !base.StandBy)
		{
			Picking.Instance.BrushHighlight.BrushColor = Picking.Instance.BrushHighlight.DefaultRemoverColor;
		}
	}
}
