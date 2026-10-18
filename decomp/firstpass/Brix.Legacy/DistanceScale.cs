using Brix.Game.Components;
using UnityEngine;

namespace Brix.Legacy;

public class DistanceScale : UpdatableGameComponent<DistanceScale>
{
	public float scaleFactor;

	public AnimationCurve scalingCurve;

	public float distanceMinimum;

	public float distanceMaximum;

	public bool lateUpdate;

	protected override void BrixUpdate()
	{
		if (!lateUpdate)
		{
			distanceScale();
		}
	}

	protected override void BrixLateUpdate()
	{
		if (lateUpdate)
		{
			distanceScale();
		}
	}

	public void distanceScale()
	{
		float value = Vector3.Distance(base.transform.position, Camera.main.transform.position);
		float time = Mathf.InverseLerp(distanceMinimum, distanceMaximum, value);
		float num = distanceMinimum / (distanceMaximum - distanceMinimum);
		float num2 = scaleFactor * (distanceMaximum - distanceMinimum) / (float)Screen.height;
		base.transform.localScale = Vector3.one * ((num + scalingCurve.Evaluate(time)) * num2);
	}
}
