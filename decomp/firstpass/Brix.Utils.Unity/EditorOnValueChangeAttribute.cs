using UnityEngine;

namespace Brix.Utils.Unity;

public class EditorOnValueChangeAttribute : PropertyAttribute
{
	public string lambda;

	public bool enabled = true;

	public EditorOnValueChangeAttribute(string lambda = "")
	{
		this.lambda = lambda;
	}
}
