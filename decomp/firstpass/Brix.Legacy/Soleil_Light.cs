using UnityEngine;

namespace Brix.Legacy;

public class Soleil_Light : MonoBehaviour
{
	public Journee Journee;

	public float Heure;

	public Orbite SoleilOrbite;

	public Gradient SoleilGradient;

	private void Update()
	{
		Heure = Journee.Heure;
		float num = Heure / 24f;
		if ((bool)SoleilOrbite)
		{
			SoleilOrbite.Phase = num;
		}
		Color color = SoleilGradient.Evaluate(num);
		if ((double)color.a >= 0.01)
		{
			base.gameObject.GetComponent<Light>().enabled = true;
			base.gameObject.GetComponent<Light>().color = color;
			base.gameObject.GetComponent<Light>().intensity = color.a;
		}
		else
		{
			base.gameObject.GetComponent<Light>().enabled = false;
		}
	}
}
