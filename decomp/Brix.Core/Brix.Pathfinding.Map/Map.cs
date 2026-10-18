using System.Collections.Generic;
using Brix.Engine;
using UnityEngine;

namespace Brix.Pathfinding.Map;

public abstract class Map : MonoBehaviour
{
	public XYZ start;

	public XYZ size;

	public abstract IEnumerable<XYZ> SurfaceVoxels { get; }

	public abstract bool Contains(XYZ p);
}
