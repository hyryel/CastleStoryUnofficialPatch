using UnityEngine;

namespace Brix.Utils.Unity;

public class EditorComponentPopupAttribute : PropertyAttribute
{
	public string lambda;

	public EditorComponentPopupAttribute(string lambda = "")
	{
		this.lambda = lambda;
	}
}
