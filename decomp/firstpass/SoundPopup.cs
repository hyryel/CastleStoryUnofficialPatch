using UnityEngine;

public class SoundPopup : PropertyAttribute
{
	public enum Type
	{
		Sound,
		SoundPrefix,
		SoundSuffix
	}

	public Type type;

	public string filter = string.Empty;

	public SoundPopup(string filter = "")
	{
		this.filter = filter;
	}

	public SoundPopup(Type type, string filter = "")
	{
		this.type = type;
		this.filter = filter;
	}
}
