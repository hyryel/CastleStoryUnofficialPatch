using UnityEngine;

namespace Brix.Legacy.Rendering;

public class BarreDeProgres : MonoBehaviour
{
	public Color defaultColor;

	public bool useDefault;

	public Gradient gradient;

	public float fraction;

	public float fractionCouleur;

	public ArcRenderer arc;

	public int angleMin;

	public int angleMax;

	private void Start()
	{
		arc = base.gameObject.GetOrAddComponent<ArcRenderer>();
	}

	private void Update()
	{
		arc.angleMin = Mathf.Lerp(180f, angleMin, fraction);
		arc.angleMax = Mathf.Lerp(180f, angleMax, fraction);
		float alpha = arc.Alpha;
		if (fractionCouleur == 1f && useDefault)
		{
			arc.Color = defaultColor;
		}
		else
		{
			arc.Color = gradient.Evaluate(fractionCouleur);
		}
		arc.Alpha = alpha;
	}
}
