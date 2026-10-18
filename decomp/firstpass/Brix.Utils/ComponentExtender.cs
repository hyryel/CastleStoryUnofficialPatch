using UnityEngine;

namespace Brix.Utils;

public static class ComponentExtender
{
	public static T Get<T>(this Component c) where T : Object
	{
		if (typeof(T).IsAssignableFrom(typeof(GameObject)))
		{
			return c.gameObject as T;
		}
		return c.GetComponent<T>();
	}
}
