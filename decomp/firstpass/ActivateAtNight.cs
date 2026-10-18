using Brix.Game.Components;
using UnityEngine;

public class ActivateAtNight : UpdatableGameComponent<ActivateAtNight>
{
	public Light Light;

	public GameComponent SignificantParent;

	public float SunElevationNightThreshold = 0.4f;

	public float SunElevationDayThreshold = 0.5f;

	public bool AlliesOnly = true;

	private float _intensityAtNight = 0.4f;

	protected override float SlowUpdateRate => 1f;

	protected override void Awake()
	{
		base.Awake();
		_intensityAtNight = Light.intensity;
	}

	protected override void BrixSlowUpdate()
	{
		bool flag = Journee.instance.SunOrbit.BiasedElevation < SunElevationDayThreshold;
		bool flag2 = !AlliesOnly || SignificantParent.faction.IsAlliedToMe;
		if (Light.enabled != (flag && flag2))
		{
			Light.enabled = flag && flag2;
		}
		if (Light.enabled)
		{
			Light.intensity = _intensityAtNight * Mathf.InverseLerp(SunElevationDayThreshold, SunElevationNightThreshold, Journee.instance.SunOrbit.BiasedElevation);
		}
	}
}
