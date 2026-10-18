using UnityEngine;

namespace Brix.Skybox;

public class SunGain : MonoBehaviour
{
	public GameObject SunObject;

	public float gain;

	private void Update()
	{
		SunObject.GetComponent<Renderer>().material.SetFloat("_EmissionGain", gain);
	}
}
