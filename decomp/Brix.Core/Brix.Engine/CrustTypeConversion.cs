using System;

namespace Brix.Engine;

[Serializable]
public class CrustTypeConversion
{
	[VoxelTerrainType]
	public int lightType;

	[VoxelTerrainType]
	public int denseType;
}
