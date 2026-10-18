using System;
using UnityEngine;

namespace Brix.Utils;

public class ColliderUtils
{
	public static float GetSmallestContainingRadius(Collider c)
	{
		if (c.enabled && c.gameObject.activeInHierarchy)
		{
			return c.bounds.extents.magnitude;
		}
		BoxCollider boxCollider = c as BoxCollider;
		if (boxCollider != null)
		{
			return Vector3.Scale(boxCollider.size, boxCollider.transform.localScale).magnitude;
		}
		SphereCollider sphereCollider = c as SphereCollider;
		if (sphereCollider != null)
		{
			return sphereCollider.radius * sphereCollider.transform.localScale.magnitude;
		}
		CapsuleCollider capsuleCollider = c as CapsuleCollider;
		if (capsuleCollider != null)
		{
			Vector3 vector = ((capsuleCollider.direction == 0) ? Vector3.right : ((capsuleCollider.direction != 1) ? Vector3.forward : Vector3.up));
			return Vector3.Scale(Vector3.one * capsuleCollider.radius + vector * capsuleCollider.height, capsuleCollider.transform.localScale).magnitude;
		}
		MeshCollider meshCollider = c as MeshCollider;
		if (meshCollider != null)
		{
			return Vector3.Scale(meshCollider.sharedMesh.bounds.extents, meshCollider.transform.localScale).magnitude;
		}
		throw new ArgumentOutOfRangeException();
	}

	public static float GetSmallestContainingRadius_Unscaled(Collider c)
	{
		if (c.enabled && c.gameObject.activeInHierarchy)
		{
			return c.bounds.extents.magnitude;
		}
		BoxCollider boxCollider = c as BoxCollider;
		if (boxCollider != null)
		{
			return boxCollider.size.magnitude;
		}
		SphereCollider sphereCollider = c as SphereCollider;
		if (sphereCollider != null)
		{
			return sphereCollider.radius;
		}
		CapsuleCollider capsuleCollider = c as CapsuleCollider;
		if (capsuleCollider != null)
		{
			Vector3 vector = ((capsuleCollider.direction == 0) ? Vector3.right : ((capsuleCollider.direction != 1) ? Vector3.forward : Vector3.up));
			return (Vector3.one * capsuleCollider.radius + vector * capsuleCollider.height).magnitude;
		}
		MeshCollider meshCollider = c as MeshCollider;
		if (meshCollider != null)
		{
			return meshCollider.sharedMesh.bounds.extents.magnitude;
		}
		throw new ArgumentOutOfRangeException();
	}
}
