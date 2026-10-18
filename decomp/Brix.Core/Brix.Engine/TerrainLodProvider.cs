using System;
using Brix.Engine.FlatWorld;
using UnityEngine;

namespace Brix.Engine;

public abstract class TerrainLodProvider : MonoBehaviour
{
	public abstract bool GetVoxelCheck(XYZ scalaire);

	public abstract void SetVoxel(XYZ scalaire, int type);

	public abstract void InitAllLods();

	public abstract void UpdateLod(Func<Vector3, float, int, int> lodFunc);

	public abstract void DiscardUnusedLod();

	public abstract void RenderMissingLod();

	public abstract MondeAtlas GetAtlas();

	public abstract void FinishWork();

	public abstract void DrawDirtyMegaVoxels();

	public abstract LodSorter GetLodSorter();
}
