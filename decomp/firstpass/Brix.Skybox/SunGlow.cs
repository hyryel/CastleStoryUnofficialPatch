using UnityEngine;

namespace Brix.Skybox;

public class SunGlow : MonoBehaviour
{
	public GameObject SunObject;

	public Color color;

	private void Update()
	{
		SunObject.GetComponent<Renderer>().material.SetColor("_EmissionColor", color);
	}
}
