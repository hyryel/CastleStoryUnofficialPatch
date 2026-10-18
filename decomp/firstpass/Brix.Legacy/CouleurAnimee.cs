using System;
using UnityEngine;

namespace Brix.Legacy;

[Serializable]
public class CouleurAnimee
{
	public Gradient gradient;

	public float delai;

	public string courbe;

	public AnimationCurve curveData;

	public Color min => gradient.Evaluate(0f);

	public Color max => gradient.Evaluate(1f);

	public void Init()
	{
		curveData = Courbes.Get(courbe);
	}

	public Color Evaluer(float deltaTemps)
	{
		if (deltaTemps >= delai)
		{
			return gradient.Evaluate(1f);
		}
		return gradient.Evaluate(curveData.Evaluate(deltaTemps / delai));
	}
}
