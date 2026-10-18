using UnityEngine;

namespace Brix.Engine.Nature;

public struct PlantVariation
{
	public readonly Vector3 Displacement;

	public readonly Quaternion Orientation;

	public readonly int PlantType;

	public readonly float MaxSize;

	public const int RotationVariations = 24;

	public const int MeshVariations = 5;

	public PlantVariation(XYZ voxel, int meshVariation)
	{
		Random.InitState(voxel.GetHashCode());
		Displacement = new Vector3(Random.value - 0.5f, 0f, Random.value - 0.5f);
		Orientation = Quaternion.Euler(0f, Random.Range(0, 24) * 15, 0f);
		PlantType = ((meshVariation >= 0) ? meshVariation : Random.Range(0, 5));
		MaxSize = Random.Range(0.8f, 1.2f);
		Displacement.y = Architecte.SurfaceRaycast(voxel.ToVector3() + Displacement).y - (float)voxel.y;
	}
}
