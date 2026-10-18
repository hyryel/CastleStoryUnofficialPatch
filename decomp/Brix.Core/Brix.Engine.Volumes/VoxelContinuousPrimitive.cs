using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Brix.Engine.Volumes;

[Serializable]
public abstract class VoxelContinuousPrimitive : IVoxelVolume, IEnumerable<XYZ>, IEnumerable
{
	public Vector3 center = Vector3.zero;

	public Vector3 extents = Vector3.one;

	public VoxelBounds Bounds => new VoxelBounds(XYZ.Floor(center - extents), XYZ.Ceil(center + extents));

	public VoxelBounds MinimalBounds => Bounds;

	public Bounds VectorBounds => new Bounds(center, extents * 2f);

	public IEnumerable<XYZ> Voxels => Bounds.Where(Contains);

	public IEnumerable<VoxelSurfaceElement> Surface => this.ComputeSurface();

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
		return CheckPoint(p.ToVector3() - center);
	}

	protected abstract bool CheckPoint(Vector3 d);

	protected float Sq(float n)
	{
		return n * n;
	}

	protected float SqE(float n)
	{
		return n * (n + Mathf.Epsilon);
	}
}
