using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Brix.Engine.Volumes;

[Serializable]
public class VoxelSet : IVoxelVolume, IEnumerable<XYZ>, IEnumerable
{
	public HashSet<XYZ> voxels;

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

	public VoxelSet()
	{
		voxels = new HashSet<XYZ>();
	}

	public VoxelSet(IEnumerable<XYZ> voxels)
	{
		this.voxels = new HashSet<XYZ>(voxels);
	}

	public void Clear()
	{
		voxels.Clear();
	}

	public bool Add(XYZ p)
	{
		return voxels.Add(p);
	}

	public bool Remove(XYZ p)
	{
		return voxels.Remove(p);
	}

	public void AddRange(IEnumerable<XYZ> ps)
	{
		voxels.UnionWith(ps);
	}

	public void RemoveRange(IEnumerable<XYZ> ps)
	{
		voxels.ExceptWith(ps);
	}

	public bool Contains(XYZ p)
	{
		return voxels.Contains(p);
	}
}
