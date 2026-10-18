using UnityEngine;

namespace Brix.Legacy;

public class Stars : MonoBehaviour
{
	public Journee Journee;

	public float Heure;

	public Gradient StarsGradient;

	private void Update()
	{
		Heure = Journee.Heure;
		float time = Heure / 24f;
		base.gameObject.GetComponent<Renderer>().material.SetColor("_TintColor", StarsGradient.Evaluate(time));
	}
}
