using UnityEngine;

namespace Brix.Input.WorldEditor;

public class PlantBrushSettings : BrushSettings, IIntensity, IPlantType
{
	private float intensity = 0.4f;

	private bool validate = true;

	private float[] arbitraryIntensityMapping = new float[11]
	{
		0f, 0.01f, 0.025f, 0.045f, 0.07f, 0.1f, 0.14f, 0.2f, 0.33f, 0.5f,
		1f
	};

	private int plantType;

	public float Intensity
	{
		get
		{
			return intensity;
		}
		set
		{
			intensity = value;
		}
	}

	public bool Validate
	{
		get
		{
			return validate;
		}
		set
		{
			validate = value;
		}
	}

	public string ValidationName => "str_we_plant_brush_validation_name";

	public float ConvertedIntensity
	{
		get
		{
			int num = Mathf.FloorToInt(intensity * 10f);
			return arbitraryIntensityMapping[num];
		}
	}

	public int PlantType
	{
		get
		{
			return plantType;
		}
		set
		{
			plantType = value;
		}
	}
}
