using UnityEngine;

public class MegaVoxelGeo : MonoBehaviour
{
	public int lodFactor = -1;

	public int lodWanted = -1;

	public int lodAuxWanted = -1;

	public bool started = false;

	public bool hasCollider = false;

	public MeshFilter filter;

	public MeshCollider meshCollider;

	public MeshRenderer meshRenderer;

	public Mesh mesh;

	public Mesh colliderMesh;

	public Material mainMaterial;

	public void _Start()
	{
		base.gameObject.isStatic = true;
		filter = GetComponent<MeshFilter>();
		meshCollider = GetComponent<MeshCollider>();
		meshRenderer = GetComponent<MeshRenderer>();
		meshRenderer.enabled = lodFactor == lodWanted;
		mainMaterial = meshRenderer.sharedMaterial;
		started = true;
	}

	public void SetVisible(bool visible)
	{
		if (meshRenderer != null)
		{
			meshRenderer.enabled = visible;
		}
	}

	public void ClearMesh()
	{
		if (filter != null)
		{
			filter.sharedMesh = null;
		}
		if (meshCollider != null)
		{
			meshCollider.sharedMesh = null;
		}
		if (mesh != null)
		{
			Object.Destroy(mesh);
			mesh = null;
		}
		if (colliderMesh != null)
		{
			Object.Destroy(colliderMesh);
			colliderMesh = null;
		}
	}

	public void RefreshCollider(PhysicMaterial mat)
	{
		if (hasCollider)
		{
			if (meshCollider == null)
			{
				meshCollider = base.gameObject.AddComponent<MeshCollider>();
			}
			meshCollider.sharedMesh = ((colliderMesh != null) ? colliderMesh : mesh);
			meshCollider.enabled = false;
			meshCollider.enabled = true;
			if (mat != null)
			{
				meshCollider.sharedMaterial = mat;
			}
		}
		else if (meshCollider != null)
		{
			Object.Destroy(meshCollider);
			meshCollider = null;
		}
	}

	public void OnDestroy()
	{
		ClearMesh();
	}
}
