using System;
using Brix.Engine.FlatWorld;
using UnityEngine;

namespace Brix.Engine;

public class VoxelTerrainTypeAttribute : PropertyAttribute
{
	public static Type MissingType = typeof(TerrainType);
}
