using Brix.Game.Components;
using UnityEngine;

public static class SignificantParent<T> where T : MonoBehaviour
{
	public static T Of(GameObject go)
	{
		GameObject gameObject = SignificantParent.Of(go);
		if (gameObject == null)
		{
			return (T)null;
		}
		return gameObject.GetComponent<T>();
	}

	public static T Of(Component c)
	{
		if (c == null)
		{
			return (T)null;
		}
		return Of(c.gameObject);
	}

	public static T Of(Collider c)
	{
		if (c == null)
		{
			return (T)null;
		}
		return Of(c.gameObject);
	}

	public static T Of(RaycastHit h)
	{
		if (h.transform == null)
		{
			return (T)null;
		}
		return Of(h.transform.gameObject);
	}
}
public class SignificantParent : MonoBehaviour
{
	public GameObject Get;

	public static GameObject Of(GameObject go)
	{
		if (go.IsNullOrReleased())
		{
			return null;
		}
		SignificantParent component = go.GetComponent<SignificantParent>();
		return (!(component != null)) ? go : component.Get;
	}

	public static GameObject Of(RaycastHit hit)
	{
		if (hit.transform == null)
		{
			return null;
		}
		return Of(hit.transform.gameObject);
	}

	public static GameObject Of(Component component)
	{
		if (component == null)
		{
			return null;
		}
		return Of(component.gameObject);
	}

	public static GameObject Of(Collider _collider)
	{
		if (_collider == null)
		{
			return null;
		}
		return Of(_collider.gameObject);
	}
}
