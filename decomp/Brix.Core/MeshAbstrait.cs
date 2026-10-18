using UnityEngine;

public abstract class MeshAbstrait
{
	public abstract MeshModifiable Transcrire(MeshModifiable meshDst);

	public abstract int GetVertexCount();

	public abstract int GetTriangleCount();

	public MeshModifiable GetModifiable()
	{
		return (this is MeshModifiable) ? (this as MeshModifiable) : Transcrire(new MeshInfoSimple());
	}

	public virtual Mesh Rendre(Mesh meshOut)
	{
		return GetModifiable().Rendre(meshOut);
	}

	public virtual Mesh Rendre(Mesh meshOut, Matrix4x4 matrix, MeshRendreOptions option = MeshRendreOptions.Fast)
	{
		return GetModifiable().Rendre(meshOut, matrix, option);
	}

	public void Rendre(GameObject obj)
	{
		MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
		if (meshFilter == null)
		{
			meshFilter = obj.AddComponent<MeshFilter>();
		}
		Rendre(meshFilter, shared: false);
	}

	public void RendreShared(GameObject obj)
	{
		MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
		if (meshFilter == null)
		{
			meshFilter = obj.AddComponent<MeshFilter>();
		}
		Rendre(meshFilter, shared: true);
	}

	public void RendreCollider(GameObject obj)
	{
		MeshCollider meshCollider = obj.GetComponent<MeshCollider>();
		if (meshCollider == null)
		{
			meshCollider = obj.AddComponent<MeshCollider>();
			meshCollider.sharedMesh = null;
		}
		Rendre(meshCollider);
	}

	private void Rendre(MeshFilter filter, bool shared)
	{
		Mesh mesh = (shared ? filter.sharedMesh : filter.mesh);
		if (mesh == null)
		{
			mesh = new Mesh();
			if (shared)
			{
				filter.sharedMesh = mesh;
			}
			else
			{
				filter.mesh = mesh;
			}
		}
		Rendre(mesh);
	}

	private void Rendre(MeshCollider collider)
	{
		Mesh mesh = collider.sharedMesh;
		if (mesh == null)
		{
			mesh = (collider.sharedMesh = new Mesh());
		}
		Rendre(mesh);
		collider.enabled = false;
		collider.enabled = true;
	}
}
