using System;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.Legacy.Rendering;

public class BarreDeProgresAnimee : MonoBehaviour
{
	private static BrixLogger logger = new BrixLogger(typeof(BarreDeProgresAnimee).FullName);

	public BarreDeProgres barreDeProgres;

	public float fraction;

	public Func<float, float> formule;

	public float animDuree;

	public AnimationCurve animCurve;

	public bool forceAcceptCurve;

	private float animDebut;

	private float fractionDebut;

	private float fractionFin;

	public float time;

	private void Start()
	{
		barreDeProgres = base.gameObject.GetOrAddComponent<BarreDeProgres>();
		if (!forceAcceptCurve && (animCurve == null || !Mathf.Approximately(animCurve.Evaluate(1f), 1f)))
		{
			string text = ((animCurve != null) ? (string.Empty + animCurve.Evaluate(0f) + " -> " + animCurve.Evaluate(0.5f) + " -> " + animCurve.Evaluate(1f)) : "null");
			logger.Warn("BarreDeProgresAnimee.animCurve for " + base.name + " doesn't look a a proper animation (" + text + "). Replacing it with EaseInOut");
			animCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
		}
	}

	private void Update()
	{
		time = Time.time;
		if (barreDeProgres.fraction != fraction)
		{
			if (fractionFin != fraction)
			{
				animDebut = time;
				fractionDebut = barreDeProgres.fraction;
				fractionFin = fraction;
			}
			if (time >= animDebut + animDuree)
			{
				barreDeProgres.fraction = fraction;
			}
			else
			{
				barreDeProgres.fraction = Mathf.Lerp(fractionDebut, fractionFin, animCurve.Evaluate((time - animDebut) / animDuree));
			}
			barreDeProgres.fractionCouleur = ((formule == null) ? fraction : formule(fraction));
		}
	}
}
