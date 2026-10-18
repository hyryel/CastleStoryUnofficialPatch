using UnityEngine;

namespace Brix.Skybox;

public class LightScattering : MonoBehaviour
{
	public Animator Light;

	public Animator Sun;

	private void Update()
	{
		Light.Play("b", 0, Sun.transform.GetComponent<Orbit>().LimitedElevation);
	}
}
