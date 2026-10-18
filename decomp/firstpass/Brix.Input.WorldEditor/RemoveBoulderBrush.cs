using System.Collections.Generic;
using Brix.Audio;
using Brix.Engine;
using Brix.Engine.Nature;
using Brix.Game.Components.Volumes;
using UnityEngine;

namespace Brix.Input.WorldEditor;

public class RemoveBoulderBrush : BrushComponent
{
	private bool _lastFrameGrowth;

	private List<XYZ> _brushSpace = new List<XYZ>();

	private HashSet<BoulderData> _foundBoulders = new HashSet<BoulderData>();

	private HashSet<BoulderData> _foundBouldersAfter = new HashSet<BoulderData>();

	private List<XYZ> _toRegrass = new List<XYZ>();

	public new BoulderBrushSettings brushSettings
	{
		get
		{
			if (_brushSettings == null)
			{
				CreateSettings();
			}
			return _brushSettings as BoulderBrushSettings;
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
		_brushSettings = new BoulderBrushSettings();
	}

	public override void OnActionTerrain(Vector3? clickedVoxel, ClickInfo clickInfo)
	{
		if (!(clickInfo.actionName == "cam_action1") || PickingUtility.isInViewMode())
		{
			return;
		}
		BrushUtils.BeginChanges(VoxelDirtyProperties.Terrain | VoxelDirtyProperties.Minerals);
		_brushSpace.Clear();
		int num = Mathf.CeilToInt(brushSettings.ConvertedSize / 2);
		for (int i = -num; i <= num; i++)
		{
			for (int j = -num; j <= num; j++)
			{
				XYZ xYZ = _voxelRayHit.VoxelPosition + new XYZ(i, 0, j);
				if (IsInsideBrushRange(xYZ.ToVector3()))
				{
					XYZ crustInColumn = BrushUtils.GetCrustInColumn(xYZ.x, xYZ.z);
					_brushSpace.Add(crustInColumn);
				}
			}
		}
		_foundBoulders.Clear();
		_toRegrass.Clear();
		BoulderRegistry.GetBoulders(_brushSpace, ref _foundBoulders);
		foreach (BoulderData foundBoulder in _foundBoulders)
		{
			foreach (XYZ @case in foundBoulder.Volume.GetCases(VProperty.occupy))
			{
				_toRegrass.Add(@case);
			}
		}
		bool flag = BoulderRegistry.RemoveOrUngrow(_brushSpace, brushSettings.ConvertedIntensity);
		BrushUtils.ApplyChanges(VoxelDirtyProperties.Minerals);
		foreach (XYZ item in _toRegrass)
		{
			BrushUtils.AddOrFixGrass(item);
		}
		BrushUtils.ApplyChanges();
		GameSignals.Invoke(GameSignals.OnTerrainEdited);
		_foundBouldersAfter.Clear();
		BoulderRegistry.GetBoulders(_brushSpace, ref _foundBouldersAfter);
		if (_foundBouldersAfter.Count < _foundBoulders.Count)
		{
			SoundEngine.Play("WorldEditor_Terrain_Boulder_Removed");
		}
		if (flag && !_lastFrameGrowth)
		{
			SoundEngine.Play("WorldEditor_Terrain_Boulder_Decrease_Start");
		}
		else if (!flag && _lastFrameGrowth)
		{
			SoundEngine.Play("WorldEditor_Terrain_Boulder_Decrease_Stop");
		}
		_lastFrameGrowth = flag;
	}

	public override void OnLastActionTerrain(Vector3? clickedVoxel, ClickInfo clickInfo)
	{
		SoundEngine.Play("WorldEditor_Terrain_Boulder_Decrease_Stop");
		_lastFrameGrowth = false;
	}

	protected override void OnActiveChange()
	{
		SoundEngine.Play("WorldEditor_Terrain_Boulder_Decrease_Stop");
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
