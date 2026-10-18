using UnityEngine;

namespace Brix.Engine.Nature;

public class PlantData : NatureData
{
	public const int SizeVariations = 4;

	public PlantVariation Variation;

	public override Quaternion Orientation => Variation.Orientation;

	public override NatureType Type => NatureType.plant;

	public Vector3 Displacement => Variation.Displacement;

	public float MaxSize => Variation.MaxSize;

	public float Size => MaxSize * Growth / 1f;

	public int PlantType => Variation.PlantType;

	public int SizeVariation
	{
		get
		{
			float num = Mathf.InverseLerp(0f, 1f, Growth);
			return Mathf.FloorToInt(4f * (1f - num) * 0.999f);
		}
	}

	public float Scale
	{
		get
		{
			float num = 4f * Growth / 2f;
			return num - (float)(int)num;
		}
	}

	private PlantData(XYZ position)
	{
		RootPos = position;
	}

	public void Randomize(int plantType)
	{
		Variation = new PlantVariation(RootPos, plantType);
	}

	public static PlantData Randomized(XYZ voxel, int plantType)
	{
		PlantData plantData = new PlantData(voxel);
		plantData.Randomize(plantType);
		return plantData;
	}
}
