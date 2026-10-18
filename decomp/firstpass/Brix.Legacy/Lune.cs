using UnityEngine;

namespace Brix.Legacy;

public class Lune : MonoBehaviour
{
	public Journee Journee;

	public float Heure;

	public Orbite LuneOrbite;

	public Gradient LuneGradient;

	private void Update()
	{
		Heure = Journee.Heure;
		float num = Heure / 24f;
		if ((bool)LuneOrbite)
		{
			LuneOrbite.Phase = num;
		}
		Color color = LuneGradient.Evaluate(num);
		if (color.a != 0f)
		{
			base.gameObject.GetComponent<Light>().enabled = true;
			base.gameObject.GetComponent<Light>().intensity = color.a;
			base.gameObject.GetComponent<Light>().color = color;
		}
		else
		{
			base.gameObject.GetComponent<Light>().enabled = false;
		}
	}
}
