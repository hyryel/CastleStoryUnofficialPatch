using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine;

public static class ComponentExtender
{
	public static T GetInterface<T>(this Component c)
	{
		return (T)(object)c.GetComponent(typeof(T));
	}

	public static T[] GetInterfaces<T>(this Component c)
	{
		return c.GetComponents(typeof(T)).Cast<object>().Cast<T>()
			.ToArray();
	}

	public static bool TryGetComponent<T>(this Component c, out T component)
	{
		component = (T)(object)c.GetComponent(typeof(T));
		return !EqualityComparer<T>.Default.Equals(component, default(T));
	}

	public static T GetOrAddComponent<T>(this Component c) where T : Component
	{
		return c.gameObject.GetOrAddComponent<T>();
	}

	public static Component GetOrAddComponent(this Component c, Type t)
	{
		return c.gameObject.GetOrAddComponent(t);
	}

	public static T GetComponentInChildrenBreadthFirst<T>(this Component c) where T : Component
	{
		return c.GetComponentInChildrenBreadthFirst<T>(includeInactives: false, includeSelf: true);
	}

	public static T GetComponentInChildrenBreadthFirst<T>(this Component c, bool includeInactives) where T : Component
	{
		return c.GetComponentInChildrenBreadthFirst<T>(includeInactives, includeSelf: true);
	}

	public static T GetComponentInChildrenBreadthFirst<T>(this Component c, bool includeInactives, bool includeSelf) where T : Component
	{
		return (T)Convert.ChangeType(c.GetComponentInChildrenBreadthFirst(typeof(T), includeInactives, includeSelf), typeof(T));
	}

	public static Component GetComponentInChildrenBreadthFirst(this Component c, Type type)
	{
		return c.GetComponentInChildrenBreadthFirst(type, includeInactives: false, includeSelf: true);
	}

	public static Component GetComponentInChildrenBreadthFirst(this Component c, Type type, bool includeInactives)
	{
		return c.GetComponentInChildrenBreadthFirst(type, includeInactives, includeSelf: true);
	}

	public static Component GetComponentInChildrenBreadthFirst(this Component c, Type type, bool includeInactives, bool includeSelf)
	{
		Queue<Transform> queue = new Queue<Transform>();
		Transform transform = c.transform;
		Component component = null;
		if (includeSelf)
		{
			component = transform.GetComponent(type);
			if (component != null)
			{
				return component;
			}
		}
		while (true)
		{
			foreach (Transform item in transform)
			{
				if (item.gameObject.activeSelf || includeInactives)
				{
					component = transform.GetComponent(type);
					if (component != null)
					{
						return component;
					}
					queue.Enqueue(item);
				}
			}
			if (queue.Count == 0)
			{
				break;
			}
			transform = queue.Dequeue();
		}
		return null;
	}

	public static IEnumerable<T> GetComponentsInChildrenBreadthFirst<T>(this Component c) where T : Component
	{
		return c.GetComponentsInChildrenBreadthFirst<T>(includeInactives: false, includeSelf: true);
	}

	public static IEnumerable<T> GetComponentsInChildrenBreadthFirst<T>(this Component c, bool includeInactives) where T : Component
	{
		return c.GetComponentsInChildrenBreadthFirst<T>(includeInactives, includeSelf: true);
	}

	public static IEnumerable<T> GetComponentsInChildrenBreadthFirst<T>(this Component c, bool includeInactives, bool includeSelf) where T : Component
	{
		return c.GetComponentsInChildrenBreadthFirst(typeof(T), includeInactives, includeSelf).Cast<T>();
	}

	public static IEnumerable<Component> GetComponentsInChildrenBreadthFirst(this Component c, Type type)
	{
		return c.GetComponentsInChildrenBreadthFirst(type, includeInactives: false, includeSelf: true);
	}

	public static IEnumerable<Component> GetComponentsInChildrenBreadthFirst(this Component c, Type type, bool includeInactives)
	{
		return c.GetComponentsInChildrenBreadthFirst(type, includeInactives, includeSelf: true);
	}

	public static IEnumerable<Component> GetComponentsInChildrenBreadthFirst(this Component c, Type type, bool includeInactives, bool includeSelf)
	{
		Queue<Transform> toScan = new Queue<Transform>();
		Transform currentNode = c.transform;
		if (includeSelf)
		{
			Component[] components = currentNode.GetComponents(type);
			for (int i = 0; i < components.Length; i++)
			{
				yield return components[i];
			}
		}
		while (true)
		{
			foreach (Transform t in currentNode)
			{
				if (t.gameObject.activeSelf || includeInactives)
				{
					toScan.Enqueue(t);
				}
			}
			if (toScan.Count == 0)
			{
				break;
			}
			currentNode = toScan.Dequeue();
			Component[] components2 = currentNode.GetComponents(type);
			for (int j = 0; j < components2.Length; j++)
			{
				yield return components2[j];
			}
		}
	}

	public static T GetComponentInParentExcludingSelf<T>(this Component c) where T : class
	{
		Transform parent = c.transform.parent;
		Component component = null;
		while (parent != null && component == null)
		{
			component = parent.GetComponent(typeof(T));
			parent = parent.parent;
		}
		return component as T;
	}

	public static Component GetComponentInParentExcludingSelf(this Component c, Type type)
	{
		Transform parent = c.transform.parent;
		Component component = null;
		while (parent != null && component == null)
		{
			component = parent.GetComponent(type);
			parent = parent.parent;
		}
		return component;
	}

	public static T GetComponentInParent<T>(this Component c, bool includeInactive) where T : Component
	{
		if (includeInactive)
		{
			return c.transform.GetComponentInParentIncludingDisabled<T>();
		}
		return c.GetComponentInParent<T>();
	}

	public static T GetComponentInChildren<T>(this Component c, bool includeInactive) where T : Component
	{
		if (includeInactive)
		{
			return c.transform.GetComponentInChildrenIncludingDisabled<T>();
		}
		return c.GetComponentInChildren<T>();
	}

	internal static T GetComponentInParentIncludingDisabled<T>(this Transform t) where T : Component
	{
		T val = null;
		while (t != null && val == null)
		{
			val = t.GetComponent<T>();
			t = t.parent;
		}
		return val;
	}

	internal static T GetComponentInChildrenIncludingDisabled<T>(this Transform t) where T : Component
	{
		T val = t.GetComponent<T>();
		for (int i = 0; i < t.childCount; i++)
		{
			if (!(val == null))
			{
				break;
			}
			val = t.GetComponentInChildrenIncludingDisabled<T>();
		}
		return val;
	}
}
