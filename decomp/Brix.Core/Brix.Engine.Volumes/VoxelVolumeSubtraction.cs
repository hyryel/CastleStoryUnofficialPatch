using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Brix.Engine.Volumes;

[Serializable]
public class VoxelVolumeSubtraction : IVoxelVolume, IEnumerable<XYZ>, IEnumerable
{
	public IVoxelVolume v1;

	public IVoxelVolume v2;

	public IEnumerable<XYZ> Voxels => v1.Except(v2);

	public VoxelBounds Bounds => v1.Bounds;

	public VoxelBounds MinimalBounds => new VoxelBounds(Voxels);

	public Bounds VectorBounds => Bounds.ToVectorBounds();

	public IEnumerable<VoxelSurfaceElement> Surface
	{
		get
		{
			IEnumerable<VoxelSurfaceElement> first = v1.Surface.Where((VoxelSurfaceElement e) => !v2.Contains(e.InnerVoxel));
			IEnumerable<VoxelSurfaceElement> second = from e in v2.Surface
				select e.Inverse into e
				where v1.Contains(e.InnerVoxel)
				select e;
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
		return v1.Contains(p) && !v2.Contains(p);
	}
}
