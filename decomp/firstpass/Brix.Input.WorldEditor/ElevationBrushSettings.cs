namespace Brix.Input.WorldEditor;

public class ElevationBrushSettings : BrushSettings, IIntensity
{
	private float intensity = 0.75f;

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
}
