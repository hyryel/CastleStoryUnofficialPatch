using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Brix.Engine.Volumes;

[Serializable]
public class VoxelVolumeUnion : IVoxelVolume, IEnumerable<XYZ>, IEnumerable
{
	public IVoxelVolume v1;

	public IVoxelVolume v2;

	public IEnumerable<XYZ> Voxels => v1.Union(v2);

	public VoxelBounds Bounds
	{
		get
		{
			VoxelBounds bounds = v1.Bounds;
			VoxelBounds bounds2 = v2.Bounds;
			return new VoxelBounds
			{
				min = XYZ.Min(bounds.min, bounds2.min),
				max = XYZ.Max(bounds.max, bounds2.max)
			};
		}
	}

	public VoxelBounds MinimalBounds => new VoxelBounds(Voxels);

	public Bounds VectorBounds => Bounds.ToVectorBounds();

	public IEnumerable<VoxelSurfaceElement> Surface
	{
		get
		{
			IEnumerable<VoxelSurfaceElement> first = v1.Surface.Where((VoxelSurfaceElement e) => !v2.Contains(e.OuterVoxel));
			IEnumerable<VoxelSurfaceElement> second = v2.Surface.Where((VoxelSurfaceElement e) => !v1.Contains(e.OuterVoxel));
			return first.Union(second);
		}
	}

	IEnumerator<XYZ> IEnumerable<XYZ>.GetEnumerator()
	{
		return Voxels.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return Voxels.GetEnumerator();
	}

	public bool Contains(XYZ p)
	{
		return v1.Contains(p) || v2.Contains(p);
	}
}
