using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Brix.Engine.Volumes;

[Serializable]
[JsonObject(MemberSerialization.OptIn)]
public class VoxelVolumeWithProperty : IVoxelVolume, IEnumerable<XYZ>, IEnumerable
{
	[JsonProperty]
	public IVoxelVolume v;

	public Func<XYZ, bool> HasProperty;

	public IEnumerable<XYZ> Voxels => v.Voxels.Where(HasProperty);

	public VoxelBounds Bounds => v.Bounds;

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

	public bool Contains(XYZ p)
	{
		return v.Contains(p) && HasProperty(p);
	}
}
