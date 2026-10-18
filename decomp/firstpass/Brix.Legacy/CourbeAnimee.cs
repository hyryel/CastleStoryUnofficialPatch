using System;
using UnityEngine;

namespace Brix.Legacy;

[Serializable]
public class CourbeAnimee
{
	public float min;

	public float max;

	public float delai;

	public string courbe;

	public AnimationCurve curveData;

	public void Init()
	{
		curveData = Courbes.Get(courbe);
	}

	public float Evaluer(float deltaTemps)
	{
		if (deltaTemps >= delai)
		{
			return max;
		}
		return min + (max - min) * curveData.Evaluate(deltaTemps / delai);
	}
}
