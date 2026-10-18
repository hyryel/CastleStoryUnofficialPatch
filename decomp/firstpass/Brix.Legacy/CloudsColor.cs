using UnityEngine;

namespace Brix.Legacy;

public class CloudsColor : MonoBehaviour
{
	public Journee Journee;

	public Material MaterielNuage;

	public Gradient CloudGradient;

	public Gradient FalloffGradient;

	private Color NuagesCouleur;

	private void Update()
	{
		float time = Journee.Heure / 24f;
		MaterielNuage.color = CloudGradient.Evaluate(time);
		MaterielNuage.SetColor("_BottomColor", FalloffGradient.Evaluate(time));
	}
}
