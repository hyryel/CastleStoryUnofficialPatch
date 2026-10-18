using Brix.Engine;
using Brix.Engine.Blocks;
using Brix.External.Factories;
using UnityEngine;

namespace Brix.Game.Components.DisplayDataGenerator;

public class RopeBridgePlankDisplayDataGenerator : GameComponent, IVoxelDisplayDataGenerator
{
	private string assetKey;

	private Mesh mesh;

	private Material[] mats;

	public override void Initialize()
	{
		base.Initialize();
		Factory.AssetKey assetKey = GetComponent<AssetKeyHolder>().AssetKey;
		this.assetKey = assetKey.ToString();
		mesh = Factory.Peek(assetKey).GetComponent<MeshFilter>().sharedMesh;
		mats = Factory.Peek(assetKey).GetComponent<MeshRenderer>().sharedMaterials;
	}

	public void GenerateVoxelDisplayData(BlockData bd, IVoxelDispatcher dispatcher)
	{
		RopeBridgeConfigurator ropeBridgeFromPlankAt = RopeBridgeConfigurator.GetRopeBridgeFromPlankAt(bd.Position);
		dispatcher.Dispatch(new VoxelDisplayData(matrix: (!(ropeBridgeFromPlankAt != null)) ? Matrix4x4.TRS(bd.AbsolutePivotPosition, bd.CanonicalRotation, Vector3.one) : ropeBridgeFromPlankAt.GetPlankTransform(bd.Position), id: assetKey, mesh: mesh, mats: mats));
	}
}
