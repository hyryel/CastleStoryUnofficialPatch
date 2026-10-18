using UnityEngine;

public static class Vector3Extender
{
	public static int CompareTo(this Vector3 u, Vector3 v)
	{
		return (u.x != v.x) ? u.x.CompareTo(v.x) : ((u.y != v.y) ? u.y.CompareTo(v.y) : u.z.CompareTo(v.z));
	}
}
