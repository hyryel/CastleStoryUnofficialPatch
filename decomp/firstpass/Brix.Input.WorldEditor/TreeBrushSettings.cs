using UnityEngine;

namespace Brix.Input.WorldEditor;

public class TreeBrushSettings : BrushSettings, IIntensity
{
	private float intensity = 0.4f;

	private float[] arbitraryIntensityMapping = new float[11]
	{
		0f, 0.065f, 0.07f, 0.08f, 0.1f, 0.13f, 0.18f, 0.25f, 0.33f, 0.5f,
		1f
	};

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

	public float ConvertedIntensity
	{
		get
		{
			int num = Mathf.FloorToInt(intensity * 10f);
			return arbitraryIntensityMapping[num];
		}
	}
}
