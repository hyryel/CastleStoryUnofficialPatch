using Brix.Engine;
using Brix.External.Factories;
using UnityEngine;

public class GetMeshHere : MonoBehaviour
{
	[SerializeField]
	private MeshFilter meshFilter;

	public bool ReplaceMeshNow;

	private Vector3 blockPositon;

	private Quaternion blockRotation;

	private Mesh blockMesh;

	public Color color
	{
		get
		{
			return GetComponent<Renderer>().material.color;
		}
		set
		{
			GetComponent<Renderer>().material.color = value;
		}
	}

	private void GetBlockInfo()
	{
		IVoxel blockData = Voxel.GetBlockData(XYZ.FromVector3(base.transform.position));
		Factory.AssetKey blueprintKey = blockData.BlockInfo.BlueprintKey;
		blockMesh = Factory.Peek(blueprintKey).GetComponent<MeshFilter>().sharedMesh;
		blockPositon = blockData.AbsolutePivotPosition;
		blockRotation = blockData.Rotation;
	}

	private void ReplaceMesh()
	{
		GetBlockInfo();
		meshFilter.mesh = blockMesh;
		base.transform.Rotate(blockRotation.eulerAngles);
		base.transform.position = blockPositon;
	}

	private void Start()
	{
		ReplaceMesh();
	}
}
