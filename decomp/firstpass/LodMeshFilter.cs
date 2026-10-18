using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class LodMeshFilter : MonoBehaviour
{
	public Mesh lod0Mesh;

	public Mesh lod1Mesh;

	public Mesh lod2Mesh;

	public Mesh lod3Mesh;

	public Mesh lod4Mesh;

	public Mesh lod5Mesh;

	private void Awake()
	{
		GetComponent<MeshFilter>().sharedMesh = lod0Mesh;
	}
}
