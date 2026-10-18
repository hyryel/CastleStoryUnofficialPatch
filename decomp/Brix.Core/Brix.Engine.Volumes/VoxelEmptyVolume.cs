using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Brix.Engine.Volumes;

[Serializable]
public class VoxelEmptyVolume : IVoxelVolume, IEnumerable<XYZ>, IEnumerable
{
	public VoxelBounds Bounds => new VoxelBounds(XYZ.zero, XYZ.zero);

	public VoxelBounds MinimalBounds => Bounds;

	public Bounds VectorBounds => default(Bounds);

	public IEnumerable<XYZ> Voxels => Enumerable.Empty<XYZ>();

	public IEnumerable<VoxelSurfaceElement> Surface => Enumerable.Empty<VoxelSurfaceElement>();

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
		return false;
	}
}
