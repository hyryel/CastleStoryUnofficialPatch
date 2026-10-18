using UnityEngine;

public class ConditionalAttribute : PropertyAttribute
{
	public string condition;

	public ConditionalAttribute(string condition)
	{
		this.condition = condition;
	}
}
