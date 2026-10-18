namespace Brix.Input.WorldEditor;

public class SmoothBrushSettings : BrushSettings, IIntensity
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

	protected override int minSize => 2;
}
