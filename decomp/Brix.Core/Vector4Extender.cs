using UnityEngine;

public static class Vector4Extender
{
	public static int CompareTo(this Vector4 u, Vector4 v)
	{
		return (u.x != v.x) ? u.x.CompareTo(v.x) : ((u.y != v.y) ? u.y.CompareTo(v.y) : ((u.z != v.z) ? u.z.CompareTo(v.z) : u.w.CompareTo(v.w)));
	}
}
