using System.Collections.Generic;
using Brix.External.Factories;
using UnityEngine;

namespace Brix.Engine.Nature;

public class IronRegistryImpl : MineralRegistry
{
	protected override string AddedSoundEvent => "WorldEditor_Terrain_Iron_Spawn";

	protected override string RemovedSoundEvent => "WorldEditor_Terrain_Iron_Removed";

	protected override Factory.AssetKey garnotteKey => RawResources.RawIron;

	protected override MineralData CreateMineral(float growth, int variation, XYZ rootPos)
	{
		return new IronData(growth, variation, rootPos);
	}

	protected override void GetWarpTex(ref Texture2D resultTex, List<XYZ> positions, XYZ mvOffset)
	{
		MegaVoxel.Common.ironDrawer.GetWarpTex(ref resultTex, positions, mvOffset);
	}

	protected override Mesh GetCombinedMesh(int count, int maxCountSoFar, int variation, int lod, int texWidth)
	{
		return MegaVoxel.Common.ironDrawer.GetCombinedMesh(count, maxCountSoFar, lod, texWidth, variation);
	}

	protected override Material GetMaterial()
	{
		return new Material(Factory.Peek(Brix.External.Factories.Environment.IronMat).GetComponent<LodMaterialList>().lod0Mat);
	}
}
