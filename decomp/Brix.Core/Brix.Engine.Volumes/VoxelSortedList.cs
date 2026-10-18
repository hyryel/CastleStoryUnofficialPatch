using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Brix.Engine.Volumes;

[Serializable]
public class VoxelSortedList : IVoxelVolume, IEnumerable<XYZ>, IEnumerable
{
	public Classeur<XYZ> voxels;

	public IEnumerable<XYZ> Voxels => voxels;

	public VoxelBounds Bounds => new VoxelBounds(voxels);

	public VoxelBounds MinimalBounds => new VoxelBounds(Voxels);

	public Bounds VectorBounds => Bounds.ToVectorBounds();

	public IEnumerable<VoxelSurfaceElement> Surface => this.ComputeSurface();

	IEnumerator<XYZ> IEnumerable<XYZ>.GetEnumerator()
	{
		return Voxels.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return Voxels.GetEnumerator();
	}

	public VoxelSortedList()
	{
		voxels = new Classeur<XYZ>();
	}

	public VoxelSortedList(IEnumerable<XYZ> voxels)
	{
		this.voxels = new Classeur<XYZ>(voxels);
	}

	public bool Contains(XYZ p)
	{
		return voxels.Contains(p);
	}
}
