using UnityEngine;

namespace Brix.Skybox;

public class OceanReflections : MonoBehaviour
{
	public GameObject SunObject;

	public GameObject MoonObject;

	public GameObject SkyboxCamera;

	public Material OceanMaterial;

	private void Update()
	{
		OceanMaterial.SetVector("_SunDirection", -Vector3.Normalize(SkyboxCamera.transform.position - SunObject.transform.position));
		OceanMaterial.SetVector("_MoonDirection", -Vector3.Normalize(SkyboxCamera.transform.position - MoonObject.transform.position));
	}
}
