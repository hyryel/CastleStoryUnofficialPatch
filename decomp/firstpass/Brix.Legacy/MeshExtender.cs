using UnityEngine;

namespace Brix.Legacy;

public static class MeshExtender
{
	public static void ClearMeshFilter(this GameObject gameObject)
	{
		gameObject.ClearMeshFilter(removeComponent: false);
	}

	public static void ClearMeshFilter(this GameObject gameObject, bool removeComponent)
	{
		MeshFilter component = gameObject.GetComponent<MeshFilter>();
		if (!(component == null))
		{
			Mesh sharedMesh = component.sharedMesh;
			if (sharedMesh != null)
			{
				component.sharedMesh = null;
				sharedMesh.Clear();
				Object.Destroy(sharedMesh);
			}
			if (removeComponent)
			{
				Object.Destroy(component);
			}
		}
	}

	public static void ClearMeshCollider(this GameObject gameObject)
	{
		gameObject.ClearMeshCollider(removeComponent: false);
	}

	public static void ClearMeshCollider(this GameObject gameObject, bool removeComponent)
	{
		MeshCollider component = gameObject.GetComponent<MeshCollider>();
		if (!(component == null))
		{
			Mesh sharedMesh = component.sharedMesh;
			if (sharedMesh != null)
			{
				component.sharedMesh = null;
				sharedMesh.Clear();
				Object.Destroy(sharedMesh);
			}
			if (removeComponent)
			{
				Object.Destroy(component);
			}
		}
	}
}
