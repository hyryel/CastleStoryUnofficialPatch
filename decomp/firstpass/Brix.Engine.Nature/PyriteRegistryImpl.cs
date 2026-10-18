using System.Collections.Generic;
using Brix.External.Factories;
using UnityEngine;

namespace Brix.Engine.Nature;

public class PyriteRegistryImpl : MineralRegistry
{
	protected override string AddedSoundEvent => "WorldEditor_Terrain_Brimstone_Spawn";

	protected override string RemovedSoundEvent => "WorldEditor_Terrain_Brimstone_Removed";

	protected override Factory.AssetKey garnotteKey => RawResources.RawOrangeCrystal;

	protected override MineralData CreateMineral(float growth, int variation, XYZ rootPos)
	{
		return new PyriteData(growth, variation, rootPos);
	}

	protected override void GetWarpTex(ref Texture2D resultTex, List<XYZ> positions, XYZ mvOffset)
	{
		MegaVoxel.Common.pyriteDrawer.GetWarpTex(ref resultTex, positions, mvOffset);
	}

	protected override Mesh GetCombinedMesh(int count, int maxCountSoFar, int variation, int lod, int texWidth)
	{
		return MegaVoxel.Common.pyriteDrawer.GetCombinedMesh(count, maxCountSoFar, lod, texWidth, variation);
	}

	protected override Material GetMaterial()
	{
		return new Material(Factory.Peek(Brix.External.Factories.Environment.PyriteMat).GetComponent<LodMaterialList>().lod0Mat);
	}
}
