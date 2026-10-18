using UnityEngine;

namespace Brix.Legacy;

public class AmbientLight : MonoBehaviour
{
	public Journee Journee;

	public float Heure;

	public Gradient AmbientGradient;

	public Gradient BrumeGradientAM;

	public Gradient BrumeGradientPM;

	public float BrumeDistance = 10000f;

	private void Update()
	{
		Heure = Journee.Heure;
		float num = Heure / 24f;
		Color fogColor = ((!((double)num > 0.5)) ? BrumeGradientAM.Evaluate(Mathf.Lerp(0f, 1f, num * 2f)) : BrumeGradientPM.Evaluate(Mathf.Lerp(1f, 0f, (num - 0.5f) * 2f)));
		RenderSettings.ambientLight = AmbientGradient.Evaluate(num);
		RenderSettings.fogColor = fogColor;
		RenderSettings.fogEndDistance = BrumeDistance;
	}
}
