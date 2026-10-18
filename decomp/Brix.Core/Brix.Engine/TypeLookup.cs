using System;
using UnityEngine;

namespace Brix.Engine;

[Serializable]
public class TypeLookup
{
	public Color color;

	[VoxelTerrainType]
	public int type;
}
