using UnityEngine;

namespace Brix.Skybox;

public class DirectionToMaterialSlot : MonoBehaviour
{
	public GameObject TargetObject;

	public GameObject CameraObject;

	public Material DestinationMaterial;

	public string PropertyName;

	private void Update()
	{
		DestinationMaterial.SetVector(PropertyName, -Vector3.Normalize(CameraObject.transform.position - TargetObject.transform.position));
	}
}
