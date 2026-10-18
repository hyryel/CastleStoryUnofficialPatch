namespace Brix.Input.WorldEditor;

public class CreateIslandBrushSettings : BrushSettings, ICreateIsland
{
	private bool dontCreate;

	private bool dontLower;

	private bool dontRaise;

	private bool useLevelOverride = true;

	private int levelOverride = 1;

	public virtual bool DontCreate
	{
		get
		{
			return dontCreate;
		}
		set
		{
			dontCreate = value;
		}
	}

	public bool DontLower
	{
		get
		{
			return dontLower;
		}
		set
		{
			dontLower = value;
		}
	}

	public bool DontRaise
	{
		get
		{
			return dontRaise;
		}
		set
		{
			dontRaise = value;
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
