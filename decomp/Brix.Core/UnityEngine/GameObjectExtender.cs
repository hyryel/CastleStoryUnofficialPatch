using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Brix.Utils;

namespace UnityEngine;

public static class GameObjectExtender
{
	public static bool IsParentOf(this GameObject parentGO, GameObject potentialChild)
	{
		Transform parent = potentialChild.transform.parent;
		Transform transform = parentGO.transform;
		while (parent != null)
		{
			if (parent == transform)
			{
				return true;
			}
			parent = parent.parent;
		}
		return false;
	}

	public static GameObject RootGameObject(this GameObject go)
	{
		return go.transform.root.gameObject;
	}

	public static T GetInterface<T>(this GameObject gameObject)
	{
		return (T)(object)gameObject.GetComponent(typeof(T));
	}

	public static T GetInterfaceInChildren<T>(this GameObject gameObject)
	{
		return (T)(object)gameObject.GetComponentInChildren(typeof(T));
	}

	public static T GetInterfaceInChildrenBreadthFirst<T>(this GameObject gameObject)
	{
		return (T)(object)gameObject.GetComponentInChildrenBreadthFirst(typeof(T));
	}

	public static T GetInterfaceInChildrenBreadthFirst<T>(this GameObject gameObject, bool includeInactives)
	{
		return (T)(object)gameObject.GetComponentInChildrenBreadthFirst(typeof(T), includeInactives);
	}

	public static T GetInterfaceInChildrenBreadthFirst<T>(this GameObject gameObject, bool includeInactives, bool includeSelf)
	{
		return (T)(object)gameObject.GetComponentInChildrenBreadthFirst(typeof(T), includeInactives, includeSelf);
	}

	public static T GetInterfaceInParent<T>(this GameObject gameObject)
	{
		return (T)(object)gameObject.GetComponentInParent(typeof(T));
	}

	public static T[] GetInterfaces<T>(this GameObject gameObject)
	{
		return gameObject.GetComponents(typeof(T)).Cast<object>().Cast<T>()
			.ToArray();
	}

	public static T[] GetInterfacesInChildren<T>(this GameObject gameObject)
	{
		return gameObject.GetComponentsInChildren(typeof(T)).Cast<object>().Cast<T>()
			.ToArray();
	}

	public static T[] GetInterfacesInChildren<T>(this GameObject gameObject, bool includeInactive)
	{
		return gameObject.GetComponentsInChildren(typeof(T), includeInactive).Cast<object>().Cast<T>()
			.ToArray();
	}

	public static T[] GetInterfacesInChildrenBreadthFirst<T>(this GameObject gameObject)
	{
		return gameObject.GetComponentsInChildrenBreadthFirst(typeof(T)).Cast<object>().Cast<T>()
			.ToArray();
	}

	public static T[] GetInterfacesInChildrenBreadthFirst<T>(this GameObject gameObject, bool includeInactives)
	{
		return gameObject.GetComponentsInChildrenBreadthFirst(typeof(T), includeInactives).Cast<object>().Cast<T>()
			.ToArray();
	}

	public static T[] GetInterfacesInChildrenBreadthFirst<T>(this GameObject gameObject, bool includeInactives, bool includeSelf)
	{
		return gameObject.GetComponentsInChildrenBreadthFirst(typeof(T), includeInactives, includeSelf).Cast<object>().Cast<T>()
			.ToArray();
	}

	public static T[] GetInterfacesInParent<T>(this GameObject gameObject)
	{
		return gameObject.GetComponentsInParent(typeof(T)).Cast<object>().Cast<T>()
			.ToArray();
	}

	public static T[] GetInterfacesInParent<T>(this GameObject gameObject, bool includeInactive)
	{
		return gameObject.GetComponentsInParent(typeof(T), includeInactive).Cast<object>().Cast<T>()
			.ToArray();
	}

	public static bool TryGetComponent<T>(this GameObject c, out T component)
	{
		component = (T)(object)c.GetComponent(typeof(T));
		return !EqualityComparer<T>.Default.Equals(component, default(T));
	}

	public static bool IsSelected(this GameObject g)
	{
		return IsSelected(g.transform);
	}

	private static bool IsSelected(Transform t)
	{
		return false;
	}

	public static string Path(this GameObject go)
	{
		if (go == null)
		{
			return "/";
		}
		StringBuilder stringBuilder = _BuildPath(go);
		return stringBuilder.ToString();
	}

	public static string FullPath(this GameObject go)
	{
		if (go == null)
		{
			return "/";
		}
		StringBuilder stringBuilder = _BuildPath(go).Append("/").Append(go.name);
		return stringBuilder.ToString();
	}

	private static StringBuilder _BuildPath(GameObject go)
	{
		CollectionPool.Request<string>(out List<string> list);
		StringBuilder stringBuilder = new StringBuilder();
		Transform transform = go.transform;
		while (transform != null)
		{
			list.Add(transform.name);
			transform = transform.parent;
		}
		for (int num = list.Count - 1; num > 0; num--)
		{
			string value = list[num];
			stringBuilder.Append("/").Append(value);
		}
		CollectionPool.Release(ref list);
		return stringBuilder;
	}

	public static T GetComponentInParentOrChildren<T>(this GameObject g) where T : Component
	{
		T componentInParent = g.GetComponentInParent<T>();
		if (componentInParent != null)
		{
			return componentInParent;
		}
		T componentInChildren = g.GetComponentInChildren<T>();
		if (componentInChildren != null)
		{
			return componentInChildren;
		}
		return null;
	}

	public static T GetComponentInParentOrChildren<T>(this Component c) where T : Component
	{
		return c.gameObject.GetComponentInParentOrChildren<T>();
	}

	public static T GetComponentInParent<T>(this GameObject g, bool includeInactive) where T : Component
	{
		if (includeInactive)
		{
			return g.transform.GetComponentInParentIncludingDisabled<T>();
		}
		return g.GetComponentInParent<T>();
	}

	public static T GetComponentInChildren<T>(this GameObject g, bool includeInactive) where T : Component
	{
		if (includeInactive)
		{
			return g.transform.GetComponentInChildrenIncludingDisabled<T>();
		}
		return g.GetComponentInChildren<T>();
	}

	public static T GetOrAddComponent<T>(this GameObject go) where T : Component
	{
		T val = go.GetComponent<T>();
		if (val == null)
		{
			val = go.AddComponent<T>();
		}
		return val;
	}

	public static Component GetOrAddComponent(this GameObject go, Type t)
	{
		Component component = go.GetComponent(t);
		if (component == null)
		{
			component = go.AddComponent(t);
		}
		return component;
	}

	public static IEnumerable<T> GetComponentsInChildrenBreadthFirst<T>(this GameObject go) where T : Component
	{
		return go.transform.GetComponentsInChildrenBreadthFirst<T>(includeInactives: false, includeSelf: true);
	}

	public static IEnumerable<T> GetComponentsInChildrenBreadthFirst<T>(this GameObject go, bool includeInactives) where T : Component
	{
		return go.transform.GetComponentsInChildrenBreadthFirst<T>(includeInactives, includeSelf: true);
	}

	public static IEnumerable<T> GetComponentsInChildrenBreadthFirst<T>(this GameObject go, bool includeInactives, bool includeSelf) where T : Component
	{
		return go.transform.GetComponentsInChildrenBreadthFirst<T>(includeInactives, includeSelf);
	}

	public static IEnumerable<Component> GetComponentsInChildrenBreadthFirst(this GameObject go, Type type)
	{
		return go.transform.GetComponentsInChildrenBreadthFirst(type, includeInactives: false, includeSelf: true);
	}

	public static IEnumerable<Component> GetComponentsInChildrenBreadthFirst(this GameObject go, Type type, bool includeInactives)
	{
		return go.transform.GetComponentsInChildrenBreadthFirst(type, includeInactives, includeSelf: true);
	}

	public static IEnumerable<Component> GetComponentsInChildrenBreadthFirst(this GameObject go, Type type, bool includeInactives, bool includeSelf)
	{
		return go.transform.GetComponentsInChildrenBreadthFirst(type, includeInactives, includeSelf);
	}

	public static T GetComponentInChildrenBreadthFirst<T>(this GameObject go) where T : Component
	{
		return go.transform.GetComponentInChildrenBreadthFirst<T>(includeInactives: false, includeSelf: true);
	}

	public static T GetComponentInChildrenBreadthFirst<T>(this GameObject go, bool includeInactives) where T : Component
	{
		return go.transform.GetComponentInChildrenBreadthFirst<T>(includeInactives, includeSelf: true);
	}

	public static T GetComponentInChildrenBreadthFirst<T>(this GameObject go, bool includeInactives, bool includeSelf) where T : Component
	{
		return go.transform.GetComponentInChildrenBreadthFirst<T>(includeInactives, includeSelf);
	}

	public static Component GetComponentInChildrenBreadthFirst(this GameObject go, Type type)
	{
		return go.transform.GetComponentInChildrenBreadthFirst(type, includeInactives: false, includeSelf: true);
	}

	public static Component GetComponentInChildrenBreadthFirst(this GameObject go, Type type, bool includeInactives)
	{
		return go.transform.GetComponentInChildrenBreadthFirst(type, includeInactives, includeSelf: true);
	}

	public static Component GetComponentInChildrenBreadthFirst(this GameObject go, Type type, bool includeInactives, bool includeSelf)
	{
		return go.transform.GetComponentInChildrenBreadthFirst(type, includeInactives, includeSelf);
	}
}
