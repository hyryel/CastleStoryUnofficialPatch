using UnityEngine;

namespace Brix.Legacy;

public class SkyColor : MonoBehaviour
{
	public Journee Journee;

	public float Heure;

	public GameObject Soleil;

	public Orbite CielOrbite;

	public Gradient SkyGradient;

	public Gradient SkyRimGradient;

	public Gradient HorizonGradientAM;

	public Gradient HorizonGradientPM;

	private void Update()
	{
		Heure = Journee.Heure;
		float num = Heure / 24f;
		if ((bool)CielOrbite)
		{
			CielOrbite.Phase = num;
		}
		base.transform.LookAt(Soleil.transform);
		base.transform.Rotate(90f, 0f, 0f);
		Color value = ((!((double)num > 0.5)) ? HorizonGradientAM.Evaluate(Mathf.Lerp(0f, 1f, num * 2f)) : HorizonGradientPM.Evaluate(Mathf.Lerp(1f, 0f, (num - 0.5f) * 2f)));
		Color value2 = SkyGradient.Evaluate(num);
		if ((bool)Soleil)
		{
			Color color = Soleil.GetComponent<Light>().color;
			base.gameObject.GetComponent<Renderer>().material.SetColor("_sunColor", color);
		}
		base.gameObject.GetComponent<Renderer>().material.SetColor("_horizonColor", value);
		base.gameObject.GetComponent<Renderer>().material.SetFloat("_horizonPower", Mathf.Lerp(0.8f, 0.3f, value.a));
		base.gameObject.GetComponent<Renderer>().material.SetFloat("_sunPower", Mathf.Lerp(100f, 25f, value.a));
		base.gameObject.GetComponent<Renderer>().material.SetColor("_skyColor", value2);
	}
}
