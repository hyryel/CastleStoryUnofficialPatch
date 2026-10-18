using UnityEngine;

namespace Brix.Input.WorldEditor;

public class MineralBrushSettings : BrushSettings, IIntensity
{
	private float intensity = 0.4f;

	private float[] arbitraryIntensityMapping = new float[11]
	{
		0f, 0.01f, 0.025f, 0.045f, 0.07f, 0.1f, 0.14f, 0.2f, 0.33f, 0.5f,
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

	protected override int maxSize => 2;

	protected override float defaultSizeRatio => 1f;

	public float ConvertedIntensity
	{
		get
		{
			int num = Mathf.FloorToInt(intensity * 10f);
			return arbitraryIntensityMapping[num];
		}
	}
}
