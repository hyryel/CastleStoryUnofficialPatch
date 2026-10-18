using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Brix.Engine.Volumes;

[Serializable]
public abstract class VoxelPrimitive : IVoxelVolume, IEnumerable<XYZ>, IEnumerable
{
	public XYZ center = XYZ.zero;

	public XYZ extents = XYZ.one;

	public VoxelBounds Bounds => new VoxelBounds(center - extents, center + extents);

	public VoxelBounds MinimalBounds => Bounds;

	public Bounds VectorBounds => new Bounds(center.ToVector3(), extents.ToVector3() * 2f);

	public IEnumerable<XYZ> Voxels => Bounds;

	public IEnumerable<VoxelSurfaceElement> Surface => Bounds.Surface;

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
		return CheckPoint(p - center);
	}

	protected abstract bool CheckPoint(XYZ d);

	protected int Sq(int n)
	{
		return n * n;
	}

	protected int SqE(int n)
	{
		return n * (n + 1);
	}
}
