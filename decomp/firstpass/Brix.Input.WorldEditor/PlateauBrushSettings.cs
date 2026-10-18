namespace Brix.Input.WorldEditor;

public class PlateauBrushSettings : BrushSettings, IIntensity, ILockHeight
{
	private float intensity = 0.75f;

	private bool lockHeight;

	private bool useLevelOverride = true;

	private int levelOverride = 1;

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

	public bool LockHeight
	{
		get
		{
			return lockHeight;
		}
		set
		{
			lockHeight = value;
		}
	}

	public bool UseLevelOverride
	{
		get
		{
			return useLevelOverride;
		}
		set
		{
			useLevelOverride = value;
		}
	}

	public int LevelOverride
	{
		get
		{
			return levelOverride;
		}
		set
		{
			levelOverride = value;
			levelOverride = ((levelOverride <= 3) ? levelOverride : 3);
			levelOverride = ((levelOverride >= 0) ? levelOverride : 0);
		}
	}
}
