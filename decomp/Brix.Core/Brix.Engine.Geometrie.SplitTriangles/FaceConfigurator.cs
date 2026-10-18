using UnityEngine;

namespace Brix.Engine.Geometrie.SplitTriangles;

public class FaceConfigurator : MonoBehaviour
{
	public VoxelizedMesh.FaceDirection direction;

	public VoxelizedMesh.FaceType configType;

	[HideInInspector]
	public float area;

	[ContextMenu("Reevaluate Visibility")]
	public void ReevaluateVisibility()
	{
		if (Mathf.Abs(area - 1f) < 0.01f)
		{
			configType = VoxelizedMesh.FaceType.Full;
		}
		else if (configType == VoxelizedMesh.FaceType.Full)
		{
			configType = VoxelizedMesh.FaceType.None;
		}
	}
}
