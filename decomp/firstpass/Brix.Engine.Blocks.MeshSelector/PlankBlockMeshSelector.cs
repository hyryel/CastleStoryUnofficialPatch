using System.Collections.Generic;
using Brix.External.Factories;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Engine.Blocks.MeshSelector;

public class PlankBlockMeshSelector : DefaultBlockMeshSelector
{
	[SerializeField]
	protected List<Mesh> _meshesWithZAlignedNails = new List<Mesh>();

	[SerializeField]
	protected List<Mesh> _meshesWithXAlignedNails = new List<Mesh>();

	public override Mesh MeshVariantFor(BlockData bd, XYZ dominantAxis)
	{
		List<Mesh> meshes;
		using (IBlockData blockData = Voxel.GetBlockData(bd.Position))
		{
			if (blockData.IsMultiBlock)
			{
				meshes = _meshes;
			}
			else
			{
				IBlockData blockData2 = (blockData as MultiBlockData).ChildBlocks.Slinq().First((IBlockData blockData3) => blockData3.BlockInfo.Key == Brix.External.Factories.Blocks.Wood_Beam);
				meshes = ((!(blockData2.CanonicalRotation == bd.CanonicalRotation)) ? _meshesWithXAlignedNails : _meshesWithZAlignedNails);
			}
		}
		return GetMaterialVariation(bd.Position, dominantAxis, meshes);
	}
}
