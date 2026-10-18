using Brix.Engine.Geometrie;
using Brix.External.Factories;
using Brix.Util;
using UnityEngine;

namespace Brix.Engine.Blocks;

public class DefaultVoxelDisplayDataGenerator : IVoxelDisplayDataGenerator
{
	private Factory.AssetKey assetKey;

	private Factory.AssetKey meshDescriptorAssetKey;

	private string ID;

	private Mesh _mesh;

	private Material[] _mats;

	private Material[] damagedMat1 = new Material[1];

	private Material[] damagedMat2 = new Material[1];

	private Material[] damagedMat3 = new Material[1];

	private Material[] damagedMat4 = new Material[1];

	public DefaultVoxelDisplayDataGenerator(Factory.AssetKey key, CrackMaterialHolder holder)
	{
		Material[] array = null;
		if (holder != null)
		{
			array = holder.DamagedMats;
		}
		assetKey = key;
		meshDescriptorAssetKey = new Factory.AssetKey("Blocks_MeshDescriptors", assetKey.Name);
		ID = key.ToString();
		_mesh = Factory.Peek(key).GetComponent<MeshFilter>().sharedMesh;
		_mats = Factory.Peek(key).GetComponent<MeshRenderer>().sharedMaterials;
		if (array != null && array.Length == 4)
		{
			damagedMat1[0] = array[0];
			damagedMat2[0] = array[1];
			damagedMat3[0] = array[2];
			damagedMat4[0] = array[3];
		}
	}

	public void GenerateVoxelDisplayData(BlockData bd, IVoxelDispatcher dispatcher)
	{
		Material[] array = _mats;
		if (bd.IsDamaged)
		{
			array = ResolveDamageMaterial(bd) ?? array;
		}
		if (Factory.Exist(meshDescriptorAssetKey))
		{
			GenerateOccludedVoxelDisplayData(bd, dispatcher, array);
		}
		else if (!(_mesh == null))
		{
			dispatcher.Dispatch(new VoxelDisplayData(ID, _mesh, array, bd.OffsetedPosition, bd.Rotation));
		}
	}

	private Material[] ResolveDamageMaterial(BlockData bd)
	{
		float damage = bd.Damage;
		if (damage > 0f && damage <= 0.25f)
		{
			return damagedMat1;
		}
		if (damage > 0.25f && damage <= 0.5f)
		{
			return damagedMat2;
		}
		if (damage > 0.5f && damage <= 0.75f)
		{
			return damagedMat3;
		}
		return damagedMat4;
	}

	private void GenerateOccludedVoxelDisplayData(BlockData bd, IVoxelDispatcher dispatcher, Material[] mats)
	{
		int faceIndex = Calcule.VecteurToFaceIndex(Calcule.GetDiscreteVector(bd.Rotation * Vector3.forward));
		VoxelizedMesh voxelMesh = Factory.Peek(meshDescriptorAssetKey).GetComponent<VoxelizedMeshDescriptor>().voxelMesh;
		VoxelizedMesh.VoxelData[] voxels = voxelMesh.voxels;
		for (int i = 0; i < voxels.Length; i++)
		{
			Vector3 vector = bd.Position.ToVector3() + bd.Rotation * voxels[i].position.ToVector3();
			XYZ position = XYZ.FromVector3(Calcule.RoundVecteur(vector));
			int num = Calcule.InverseRotatedConfig(VoxelConfigEvaluator.Calculate(position), faceIndex);
			Mesh mesh = voxels[i].partialMeshes[num];
			dispatcher.Dispatch(new VoxelDisplayData(ID, mesh, mats, vector, bd.Rotation));
		}
	}
}
