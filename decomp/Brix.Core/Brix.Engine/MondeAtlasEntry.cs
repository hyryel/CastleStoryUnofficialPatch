using System;
using Brix.Engine.TerrainAtlas;

namespace Brix.Engine;

[Serializable]
public class MondeAtlasEntry
{
	[VoxelTerrainType]
	public int terrainType;

	public DensityLevel densityLevel;

	public TerrainAtlasVisualEntry visuals;
}
