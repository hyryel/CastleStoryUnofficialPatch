using System;
using System.Collections.Generic;
using Brix.Components;
using Brix.External.Factories;
using Brix.Lifecycle.Pooling;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Components;

public static class GameComponentExtender
{
	public static string GetId(this GameObject gameObject)
	{
		return gameObject.GetInterface<IdentityHolder>()?.Id;
	}

	public static void SetId(this GameObject gameObject, string id)
	{
		IdentityHolder component = gameObject.GetComponent<IdentityHolder>();
		component.Id = id;
	}

	public static bool IsMine(this GameObject gameObject)
	{
		AffiliationComponent component = gameObject.GetComponent<AffiliationComponent>();
		if (component == null)
		{
			return false;
		}
		return component.faction == null || component.faction.IsOwnedByMe;
	}

	public static bool IsNullOrReleased(this IBrixComponent ibc)
	{
		return ibc == null || ibc.Released || ibc.Destroyed;
	}

	public static bool IsNullOrReleasedIgc(this IGameComponent igc)
	{
		return igc.IsNullOrReleased();
	}

	public static bool IsNullOrReleased(this GameObject go)
	{
		if (go == null)
		{
			return true;
		}
		IBrixComponent component = go.GetComponent<IBrixComponent>();
		return component != null && (component.Released || component.Destroyed);
	}

	public static bool IsNullOrReleased(this Transform t)
	{
		if (t == null)
		{
			return true;
		}
		IBrixComponent component = t.GetComponent<IBrixComponent>();
		return component != null && (component.Released || component.Destroyed);
	}

	public static void SafeNetworkDestroy(this GameObject go)
	{
		if (go != null)
		{
			NetworkIdentity component = go.GetComponent<NetworkIdentity>();
			if (component != null && !component.netId.IsEmpty() && NetworkServer.active)
			{
				NetworkServer.UnSpawn(go);
			}
			ObjectPoolSingleton.Release(go);
		}
	}

	public static T Max<T>(this IEnumerable<T> list, Func<T, T, bool> compare)
	{
		T val = default(T);
		bool flag = true;
		foreach (T item in list)
		{
			if (flag)
			{
				flag = false;
				val = item;
			}
			else if (compare(item, val))
			{
				val = item;
			}
		}
		return val;
	}

	public static float CollisonBoxSize(this Collider collider)
	{
		if (collider is BoxCollider)
		{
			Vector3 size = (collider as BoxCollider).size;
			return Mathf.Max(size.x, size.y, size.z);
		}
		if (collider is SphereCollider)
		{
			return (collider as SphereCollider).radius * 2f;
		}
		if (collider is MeshCollider)
		{
			Vector3 size2 = (collider as MeshCollider).sharedMesh.bounds.size;
			return Mathf.Max(size2.x, size2.y, size2.z);
		}
		if (collider is CapsuleCollider)
		{
			return Mathf.Max((collider as CapsuleCollider).height, (collider as CapsuleCollider).radius) * 2f;
		}
		return 1f;
	}
}
