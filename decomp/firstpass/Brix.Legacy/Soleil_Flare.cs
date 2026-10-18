using UnityEngine;

namespace Brix.Legacy;

public class Soleil_Flare : MonoBehaviour
{
	public Journee Journee;

	public float Heure;

	public GameObject SunFlare;

	public Orbite SoleilOrbite;

	public Gradient SoleilGradient;

	public Gradient FlareGradient;

	private void Update()
	{
		Heure = Journee.Heure;
		float num = Heure / 24f;
		if ((bool)SoleilOrbite)
		{
			SoleilOrbite.Phase = num;
		}
		Color color = SoleilGradient.Evaluate(num);
		Color color2 = FlareGradient.Evaluate(num);
		SunFlare.GetComponent<LensFlare>().color = color;
		SunFlare.GetComponent<LensFlare>().brightness = color2.a;
	}
}
