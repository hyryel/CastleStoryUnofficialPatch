using UnityEngine;

public class TaggedListAttribute : PropertyAttribute
{
	public string keys;

	public TaggedListAttribute(string keys)
	{
		this.keys = keys;
	}
}
