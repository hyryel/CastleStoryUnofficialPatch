using System.Collections;
using System.Collections.Generic;
using Brix.Engine.Volumes;
using Newtonsoft.Json;
using UnityEngine;

namespace Brix.Engine;

[JsonObject(MemberSerialization.Fields)]
public interface IVoxelVolume : IEnumerable<XYZ>, IEnumerable
{
	IEnumerable<XYZ> Voxels { get; }

	IEnumerable<VoxelSurfaceElement> Surface { get; }

	VoxelBounds Bounds { get; }

	VoxelBounds MinimalBounds { get; }

	Bounds VectorBounds { get; }

	bool Contains(XYZ p);
}
