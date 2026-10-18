using System.Collections.Generic;
using Brix.Audio;
using Brix.Engine;
using Brix.Engine.Nature;
using Brix.Legacy;
using Brix.Utils;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Input.WorldEditor;

public class BrushComponent : PickerComponent
{
	[SerializeField]
	protected bool _unlimitedBrushHeight;

	protected float lastSound;

	protected float soundDelay = 0.2f;

	protected BrushSettings _brushSettings;

	public BrushSettings brushSettings
	{
		get
		{
			if (_brushSettings == null)
			{
				CreateSettings();
			}
			return _brushSettings;
		}
		set
		{
			_brushSettings = value;
		}
	}

	public bool IsInsideBounds(XYZ currentVoxel)
	{
		float num = currentVoxel.x;
		Vector3 minBounds = WorldGridController.MinBounds;
		int result;
		if (num >= minBounds.x)
		{
			float num2 = currentVoxel.x;
			Vector3 maxBounds = WorldGridController.MaxBounds;
			if (num2 <= maxBounds.x)
			{
				float num3 = currentVoxel.y;
				Vector3 minBounds2 = WorldGridController.MinBounds;
				if (num3 >= minBounds2.y)
				{
					float num4 = currentVoxel.y;
					Vector3 maxBounds2 = WorldGridController.MaxBounds;
					if (num4 <= maxBounds2.y)
					{
						float num5 = currentVoxel.z;
						Vector3 minBounds3 = WorldGridController.MinBounds;
						if (num5 >= minBounds3.z)
						{
							float num6 = currentVoxel.z;
							Vector3 maxBounds3 = WorldGridController.MaxBounds;
							result = ((num6 <= maxBounds3.z) ? 1 : 0);
							goto IL_00a1;
						}
					}
				}
			}
		}
		result = 0;
		goto IL_00a1;
		IL_00a1:
		return (byte)result != 0;
	}

	protected bool IsInsideBrushRange(Vector3 position, int sizeOverride = 0)
	{
		int areaSize = ((sizeOverride <= 0) ? brushSettings.ConvertedSize : sizeOverride);
		return BrushUtils.IsInsideRange(position, GetSnappedPosition(), areaSize, _unlimitedBrushHeight);
	}

	protected float GetFalloffFactorAt(Vector3 position, AnimationCurve curve, float sizeOverride = 0f)
	{
		float areaSize = ((!(sizeOverride > 0f)) ? ((float)brushSettings.ConvertedSize) : sizeOverride);
		return BrushUtils.GetFalloffFactorAt(position, curve, GetSnappedPosition(), areaSize);
	}

	protected virtual void CreateSettings()
	{
		_brushSettings = new BrushSettings();
	}

	public override void OnHoverTerrain(Vector3? hoveredVoxel)
	{
		if (brushSettings.ConvertedSize <= 3)
		{
			Picking.Instance.BrushHighlight.position = GetSnappedPosition();
		}
		else
		{
			Picking.Instance.BrushHighlight.position = _voxelRayHit.WorldHitVector;
		}
	}

	public override void OnEnterTerrain(Vector3? hoveredVoxel)
	{
		Picking.Instance.BrushHighlight.active = true;
	}

	public override void OnLeaveTerrain(Vector3? hoveredVoxel)
	{
		Picking.Instance.BrushHighlight.active = false;
	}

	protected override void OnStandByChange()
	{
		UpdateBrushHighlight();
	}

	protected override void OnActiveChange()
	{
		UpdateBrushHighlight();
	}

	protected virtual void UpdateBrushHighlight()
	{
		Picking.Instance.BrushHighlight.active = base.Active && !base.StandBy;
		Picking.Instance.BrushHighlight.Size = brushSettings.ConvertedSize;
	}

	protected Vector3 GetSnappedPosition()
	{
		if (brushSettings.ConvertedSize % 2 == 1)
		{
			return _voxelRayHit.HitVector;
		}
		Vector3 hitVector = _voxelRayHit.HitVector;
		if (_voxelRayHit.LocalHitVector.x < 0f)
		{
			hitVector.x -= 0.5f;
		}
		else
		{
			hitVector.x += 0.5f;
		}
		if (_voxelRayHit.LocalHitVector.z < 0f)
		{
			hitVector.z -= 0.5f;
		}
		else
		{
			hitVector.z += 0.5f;
		}
		return hitVector;
	}

	protected void CreateOrRemoveIsland(Vector3 hitVoxel, CreateIslandBrushSettings brushSettings, bool removeMode)
	{
		BrushUtils.BeginChanges(VoxelDirtyProperties.Terrain | VoxelDirtyProperties.Trees);
		int num = Mathf.CeilToInt(brushSettings.ConvertedSize / 2);
		VoxelTerrainType voxelTerrainType = VoxelTerrainType.Grass;
		Dictionary<XZ, int> dictionary = CollectionPool.RequestDictionary<XZ, int>();
		for (int i = -num; i <= num; i++)
		{
			for (int j = -num; j <= num; j++)
			{
				bool flag = false;
				Vector3 vector = hitVoxel + new Vector3(i, 0f, j);
				XYZ currentVoxel = XYZ.FromVector3(vector);
				if (!IsInsideBounds(currentVoxel) || !IsInsideBrushRange(vector))
				{
					continue;
				}
				float num2 = ((!brushSettings.UseLevelOverride) ? 0f : ((float)brushSettings.LevelOverride / 3f));
				float num3 = hitVoxel.y + num2 + 2f;
				int num4 = 3;
				VoxelEngine.GetColumnData(currentVoxel.x, currentVoxel.z, out var bottom, out var top, out var outType, out var crustVolume);
				XYZ xYZ = new XYZ(currentVoxel.x, top - 1, currentVoxel.z);
				if (NatureRegistry.HasNatureAt(xYZ))
				{
					NatureRegistry.RemoveNatureAt(xYZ);
					flag = true;
				}
				if (TreeRegistry.Instance.HasTree(xYZ))
				{
					BrushUtils.RemoveTree(xYZ);
					flag = true;
				}
				if (removeMode)
				{
					if (bottom < top)
					{
						VoxelEngine.RemoveColumn(currentVoxel.x, currentVoxel.z);
						flag = true;
					}
				}
				else
				{
					if (bottom >= top)
					{
						if (!brushSettings.DontCreate)
						{
							VoxelEngine.SetColumnBottom(currentVoxel.x, currentVoxel.z, Mathf.FloorToInt(num3) - num4);
							top = VoxelEngine.SetColumnTop(currentVoxel.x, currentVoxel.z, num3);
							flag = true;
						}
					}
					else
					{
						if (Mathf.FloorToInt(num3) - num4 < bottom)
						{
							VoxelEngine.SetColumnBottom(currentVoxel.x, currentVoxel.z, Mathf.FloorToInt(num3) - num4);
						}
						if (Mathf.Abs((float)top + crustVolume - num3) > 0.1f && ((!brushSettings.DontRaise && hitVoxel.y + 1f >= (float)top) || (!brushSettings.DontLower && hitVoxel.y + 1f <= (float)top)))
						{
							top = VoxelEngine.SetColumnTop(currentVoxel.x, currentVoxel.z, num3);
							flag = true;
						}
					}
					if (outType != voxelTerrainType)
					{
						VoxelEngine.SetColumnType(currentVoxel.x, currentVoxel.z, voxelTerrainType);
						flag = true;
					}
				}
				if (flag)
				{
					dictionary.Add(new XZ(currentVoxel.x, currentVoxel.z), (!removeMode) ? (top - 1) : 0);
				}
			}
		}
		BrushUtils.ApplyChanges(VoxelDirtyProperties.Trees);
		if (dictionary.Slinq().Any() && lastSound + soundDelay <= Time.unscaledTime)
		{
			SoundEngine.Play((!removeMode) ? "WorldEditor_Terrain_Island_Add" : "WorldEditor_Terrain_Island_Remove");
			lastSound = Time.unscaledTime;
		}
		if (BlockInfo.GetFromID((int)voxelTerrainType).IsTerrainOverlay)
		{
			HashSet<XZ> hashSet = CollectionPool.RequestHashSet<XZ>();
			BrushUtils.PopulateAdjacentColumns(dictionary, hashSet);
			int y = Mathf.RoundToInt(hitVoxel.y);
			foreach (XZ item in hashSet)
			{
				XYZ currentVoxel2 = new XYZ(item.x, y, item.z);
				if (IsInsideBounds(currentVoxel2))
				{
					XYZ at = BrushUtils.ManualGetTerrainTopInColumn(item, dictionary);
					BrushUtils.AddOrFixGrass(at, dictionary);
				}
			}
			hashSet.Dispose();
		}
		BrushUtils.ApplyChanges();
		dictionary.Dispose();
		GameSignals.Invoke(GameSignals.OnTerrainEdited);
	}
}
