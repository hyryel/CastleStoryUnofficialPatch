using UnityEngine;

namespace Brix.Legacy;

public class Sky : MonoBehaviour
{
	public Journee Journee;

	public float Heure;

	public GameObject SkyboxSphere;

	public GameObject Soleil;

	public Gradient SkyGradient;

	public Gradient SkyRimGradient;

	public Gradient HorizonGradientAM;

	public Gradient HorizonGradientPM;

	private void Update()
	{
		Heure = Journee.Heure;
		float num = Heure / 24f;
		Color value = ((!((double)num > 0.5)) ? HorizonGradientAM.Evaluate(Mathf.Lerp(0f, 1f, num * 2f)) : HorizonGradientPM.Evaluate(Mathf.Lerp(1f, 0f, (num - 0.5f) * 2f)));
		Color value2 = SkyGradient.Evaluate(num);
		Color color = Soleil.GetComponent<Light>().color;
		SkyboxSphere.GetComponent<Renderer>().material.SetColor("_sunColor", color);
		SkyboxSphere.GetComponent<Renderer>().material.SetColor("_horizonColor", value);
		SkyboxSphere.GetComponent<Renderer>().material.SetFloat("_horizonPower", Mathf.Lerp(0.8f, 0.3f, value.a));
		SkyboxSphere.GetComponent<Renderer>().material.SetFloat("_sunPower", Mathf.Lerp(100f, 25f, value.a));
		SkyboxSphere.GetComponent<Renderer>().material.SetColor("_skyColor", value2);
	}
}
