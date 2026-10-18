using System.Collections.Generic;
using UnityEngine;

namespace Brix.Engine;

public abstract class NaturePrinter
{
	public abstract void GetWarpTex(ref Texture2D resultTex, List<XYZ> positions, XYZ mvOffset);

	public abstract Mesh GetCombinedMesh(int count, int maxCountSoFar, int lod, int texWidth, int variation);

	public abstract MeshModifiable ComputeColliderMeshInfo(List<XYZ> positions, int lod, Vector3 baseTranslation);
}
