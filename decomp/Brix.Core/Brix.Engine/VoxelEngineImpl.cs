using System;
using System.Collections.Generic;
using Brix.Utils;
using UnityEngine;

namespace Brix.Engine;

public abstract class VoxelEngineImpl
{
	public abstract bool PreferSynchronousUpdate { get; set; }

	public abstract bool PreferSynchronousDrawing { get; set; }

	public abstract bool UseDeepTypeNoise { get; set; }

	public abstract Material RenderMaterial { get; set; }

	public abstract XYZ PreferredOccupiedVoxelRegionSize { get; }

	public abstract int EstimateVisibleVoxelsInMap(string folder);

	public abstract XYZ EstimateMapCenter(string folder);

	public abstract Bounds ComputeOccupiedBounds();

	public abstract void LoadMap(string folder);

	public abstract void SaveMap(string folder);

	public abstract void RenderMap();

	public abstract void ClearMap();

	public abstract void FinishWork();

	public abstract bool IsWorking();

	public abstract void OnFinishedWorking(Action a);

	public abstract bool IsEditingHeightmap();

	public abstract bool IsGrassAutoRemoved();

	public abstract Bounds GetGeneratedBottomBounds(float thicknessFactor = 0.5f, float randomness = 0.5f, int yMaxPlane = 255);

	public abstract void GenerateDefaultBottom(XYZ translation = default(XYZ), float thicknessFactor = 0.5f, float randomness = 0.5f, int yMaxPlane = 255);

	public abstract void SetSoftAlgos(int softAlgos);

	public abstract void SetLateSoftAlgos(int lateSoftAlgos);

	public abstract void SetLodMax(int lodMax);

	public abstract int GetLodMax();

	public abstract void SetLodQuality(int lodQuality);

	public abstract Transform GetLodTerrainObject(int lod);

	public void SetVoxelData(VoxelHandle h, int t, int v, int d)
	{
		h._SetVoxelData(t, v, d);
	}

	public void SetVoxelType(VoxelHandle h, int t)
	{
		h._SetVoxelType(t);
	}

	public void SetVoxelVolume(VoxelHandle h, int v)
	{
		h._SetVoxelVolume(v);
	}

	public void RemoveVoxel(VoxelHandle h)
	{
		h._RemoveVoxel();
	}

	public void RemoveGrass(VoxelHandle h)
	{
		h._RemoveGrass();
	}

	public abstract void BeginChanges();

	public abstract void ApplyChanges(bool removeUnusedVoxels = false);

	public abstract void ClearNegativeLayer();

	public abstract void ClearTerrainSurfaceLayer();

	public void GetTerrainResources(VoxelHandle h, ICollection<int> resultTypes, ICollection<int> resultAmounts)
	{
		h._GetTerrainResources(resultTypes, resultAmounts);
	}

	public void AddTerrainResources(VoxelHandle h, VoxelTerrainResources r)
	{
		h._AddTerrainResources(r);
	}

	public void CompactCrust(VoxelHandle h)
	{
		h._CompactCrust();
	}

	public VoxelTerrainResources GetTerrainResources(VoxelHandle h)
	{
		List<int> list = CollectionPool.RequestList<int>();
		List<int> list2 = CollectionPool.RequestList<int>();
		GetTerrainResources(h, list, list2);
		VoxelTerrainResources result = new VoxelTerrainResources
		{
			types = list.ToArray(),
			amounts = list2.ToArray()
		};
		list.Dispose();
		list2.Dispose();
		return result;
	}

	public virtual void AddTerrainResources(VoxelHandle h, int t, int amount)
	{
		VoxelTerrainResources voxelTerrainResources = new VoxelTerrainResources();
		voxelTerrainResources.Add(t, amount);
		h._AddTerrainResources(voxelTerrainResources);
	}

	public XYZ GetVoxelPosition(VoxelHandle h)
	{
		return h._GetVoxelPosition();
	}

	public int GetVoxelType(VoxelHandle h)
	{
		return h._GetVoxelType();
	}

	public int GetVoxelVolume(VoxelHandle h)
	{
		return h._GetVoxelVolume();
	}

	public virtual bool IsVoxelNonEmpty(XYZ p)
	{
		return IsVoxelEmpty(GetVoxelHandle(p));
	}

	public virtual bool IsVoxelEmpty(XYZ p)
	{
		return !IsVoxelNonEmpty(p);
	}

	public bool IsVoxelNonEmpty(VoxelHandle h)
	{
		return h._IsVoxelNonEmpty();
	}

	public bool IsVoxelEmpty(VoxelHandle h)
	{
		return h._IsVoxelEmpty();
	}

	public bool IsVoxelVisible(VoxelHandle h)
	{
		return h._IsVoxelVisible();
	}

	public bool IsVoxelInternal(VoxelHandle h)
	{
		return h._IsVoxelInternal();
	}

	public bool IsVoxelSmooth(VoxelHandle h)
	{
		return h._IsVoxelSmooth();
	}

	public int EstimateVoxelLegacyLevels(VoxelHandle h)
	{
		return h._EstimateVoxelLegacyLevels();
	}

	public int EstimateVoxelLevels(VoxelHandle h)
	{
		return h._EstimateVoxelLevels();
	}

	public int EstimateVoxelConfig(VoxelHandle h)
	{
		return h._EstimateVoxelConfig();
	}

	public int EstimateVoxelDeepType(VoxelHandle h)
	{
		return h._EstimateVoxelDeepType();
	}

	public int ComputeVoxelLegacyLevels(VoxelHandle h)
	{
		return h._ComputeVoxelLegacyLevels();
	}

	public int ComputeVoxelLevels(VoxelHandle h)
	{
		return h._ComputeVoxelLevels();
	}

	public int ComputeVoxelConfig(VoxelHandle h)
	{
		return h._ComputeVoxelConfig();
	}

	public int ComputeVoxelDeepType(VoxelHandle h)
	{
		return h._ComputeVoxelDeepType();
	}

	public abstract int GetColumnBottom(int x, int z);

	public abstract int GetColumnTop(int x, int z);

	public abstract int GetColumnCrustVolume(int x, int z);

	public abstract int GetColumnCrustType(int x, int z);

	public abstract void GetColumnData(int x, int z, out int bottom, out int top);

	public abstract void GetColumnData(int x, int z, out int bottom, out int top, out int type, out int volume);

	public abstract void SetColumnBottom(int x, int z, int bottom);

	public abstract void SetColumnTop(int x, int z, int top);

	public abstract void SetColumnCrustVolume(int x, int z, int volume);

	public abstract void SetColumnCrustType(int x, int z, int type);

	public abstract void SetColumnData(int x, int z, int bottom, int top, int type, int volume);

	public abstract void AddColumnData(int x, int z, int bottom, int top, int type, int volume);

	public bool IsValidVoxelHandle(VoxelHandle h)
	{
		return h?._IsValid() ?? false;
	}

	public void AssignVoxelHandle(VoxelHandle h, XYZ p)
	{
		h._AssignVoxelHandle(p);
	}

	public abstract VoxelHandle GetVoxelHandle(XYZ p);

	public abstract IEnumerable<VoxelHandle> GetEmptyVoxels(Bounds b);

	public abstract IEnumerable<VoxelHandle> GetVisibleVoxels();

	public abstract IEnumerable<VoxelHandle> GetVisibleVoxels(Bounds b);

	public abstract IEnumerable<VoxelHandle> GetAllVoxels(Bounds b);

	public abstract IEnumerable<VoxelHandle> GetSurfaceVoxels();

	public abstract IEnumerable<VoxelHandle> GetSurfaceVoxels(Bounds b);

	public abstract IEnumerable<XYZ> GetSurfaceVoxelPositions();

	public abstract IEnumerable<XYZ> GetSurfaceVoxelPositions(Bounds b);

	public abstract IEnumerable<XYZ> GetSurfaceVoxelPositions(Bounds b, int lod);

	public abstract IEnumerable<XYZ> GetVisibleVoxelPositions();

	public abstract IEnumerable<XYZ> GetVisibleVoxelPositions(Bounds b);

	public abstract IEnumerable<XYZ> GetVisibleVoxelPositions(Bounds b, int lod);

	public abstract bool IsOccupiedVoxelRegion(XYZ region);

	public abstract bool IsOccupiedVoxelRegion(XYZ region, XYZ regionSize);

	public abstract IEnumerable<XYZ> GetOccupiedVoxelRegions(XYZ regionSize);

	public abstract IEnumerable<XYZ> GetOccupiedVoxelRegions(XYZ regionSize, Bounds b);

	public abstract IEnumerable<XYZ> GetVisibleVoxelPositionsFromLayers();

	public abstract IEnumerable<XYZ> GetTopVoxelPositionsFromLayers();

	public abstract IEnumerable<XYZ> GetNaturalBottomVoxelPositionsFromLayers();

	public abstract XYZ GetClosestNonEmptyVoxel(XYZ p);

	public abstract XYZ GetClosestNonEmptyVoxelLowRes(XYZ p, int maxDistance);

	public abstract IClosestNonEmptyVoxelEnumerator GetClosestNonEmptyVoxelEnumerator(XYZ p, int closeDistance, int maxDistance);

	public abstract Vector3 TerrainRaycast(Vector3 a, Vector3 b);

	public void AssignVoxelHandle(VoxelHandle h, Vector3 p)
	{
		AssignVoxelHandle(h, XYZ.FromVector3(p));
	}

	public void AssignVoxelHandle(VoxelHandle h, int scalaire)
	{
		AssignVoxelHandle(h, XYZ.FromScalaire(scalaire));
	}

	public VoxelHandle GetVoxelHandle(Vector3 p)
	{
		return GetVoxelHandle(XYZ.FromVector3(p));
	}

	public VoxelHandle GetVoxelHandle(int scalaire)
	{
		return GetVoxelHandle(XYZ.FromScalaire(scalaire));
	}
}
