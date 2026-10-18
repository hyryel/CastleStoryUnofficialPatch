using UnityEngine;

public static class Vector2Extender
{
	public static int CompareTo(this Vector2 u, Vector2 v)
	{
		return (u.x != v.x) ? u.x.CompareTo(v.x) : u.y.CompareTo(v.y);
	}
}
