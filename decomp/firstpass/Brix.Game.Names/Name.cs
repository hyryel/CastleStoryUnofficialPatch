namespace Brix.Game.Names;

public class Name
{
	public enum Nature
	{
		error,
		official,
		reference,
		joke
	}

	public string name;

	public Nature nature;

	public int quality;

	public int set;

	public Name(string name, Nature nature, int quality, int set)
	{
		this.name = name;
		this.nature = nature;
		this.quality = quality;
		this.set = set;
	}
}
