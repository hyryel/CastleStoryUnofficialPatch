using UnityEngine;

namespace Brix.Legacy;

public class BrasseurDriver : MonoBehaviour
{
	public Brasseur brasseur;

	public AnimationCurve amplitudeCurve;

	public AnimationCurve frequencyCurve;

	public float duration;

	private float tempsDebut;

	private float translationDebut;

	private float rotationDebut;

	private float frequenceDebut;

	public void Debut()
	{
		if (tempsDebut > 0f)
		{
			Fin();
		}
		translationDebut = brasseur.translation;
		rotationDebut = brasseur.rotation;
		frequenceDebut = brasseur.frequency;
		tempsDebut = Time.time;
		brasseur.enabled = true;
	}

	public void Fin()
	{
		brasseur.enabled = false;
		tempsDebut = 0f;
		brasseur.translation = translationDebut;
		brasseur.rotation = rotationDebut;
		brasseur.frequency = frequenceDebut;
	}

	private void Update()
	{
		if (tempsDebut > 0f)
		{
			float num = Mathf.Clamp((Time.time - tempsDebut) / duration, 0f, 1f);
			brasseur.translation = Mathf.Lerp(translationDebut, 0f, amplitudeCurve.Evaluate(num));
			brasseur.rotation = Mathf.Lerp(rotationDebut, 0f, amplitudeCurve.Evaluate(num));
			brasseur.frequency = Mathf.Lerp(frequenceDebut, 0f, frequencyCurve.Evaluate(num));
			if (num >= 1f)
			{
				Fin();
			}
		}
	}
}
