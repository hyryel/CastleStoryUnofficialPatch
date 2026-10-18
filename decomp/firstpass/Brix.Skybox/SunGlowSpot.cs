using UnityEngine;

namespace Brix.Skybox;

public class SunGlowSpot : MonoBehaviour
{
	public GameObject SunObject;

	public Material SkyboxMaterial;

	private void Update()
	{
		SkyboxMaterial.SetVector("_SunDirection", Vector3.Normalize(base.transform.position - SunObject.transform.position));
	}
}
