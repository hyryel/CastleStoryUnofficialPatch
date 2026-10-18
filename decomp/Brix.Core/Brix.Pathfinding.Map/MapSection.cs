using System.Collections.Generic;
using Brix.Engine;

namespace Brix.Pathfinding.Map;

public class MapSection : Map
{
	public MapTexture map;

	public override IEnumerable<XYZ> SurfaceVoxels => map.GetSurface(start, size);

	public override bool Contains(XYZ p)
	{
		return map.Contains(p);
	}

	private void Start()
	{
		base.name = string.Concat("(", start, ")");
	}
}
