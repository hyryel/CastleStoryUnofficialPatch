using System;
using Brix.Game.Components;
using Brix.Utils.Unity;
using UnityEngine;

namespace Brix.Skybox;

public class MoonLightIntensity : UpdatableGameComponent<MoonLightIntensity>
{
	public Light Light;

	public Light SunLight;

	public float GlobalIntensityMaximum = 1f;

	public float GlobalIntensityThreshold = 0.01f;

	public float VanishZenithAngle = 90f;

	public float VanishZenithSpan = 10f;

	public float VanishSunAngle = 300f;

	public float VanishSunIntensity = 0.1f;

	public Vector3 WorldCenter = Vector3.zero;

	public bool AutoDetectIntensityMaximum = true;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public float ZenithAngleMultiplier;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public float SunAngleMultiplier;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public float SunIntensityMultiplier;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public float GlobalIntensityMultiplier;

	protected override void Start()
	{
		base.Start();
		if (AutoDetectIntensityMaximum)
		{
			GlobalIntensityMaximum = Light.intensity;
		}
	}

	protected override void BrixSlowUpdate()
	{
		ZenithAngleMultiplier = GetZenithAngleMultiplier();
		SunAngleMultiplier = GetSunAngleMultiplier();
		SunIntensityMultiplier = GetSunIntensityMultiplier();
		GlobalIntensityMultiplier = ZenithAngleMultiplier * SunAngleMultiplier * SunIntensityMultiplier;
		Apply();
	}

	private void Apply()
	{
		if (Light != null)
		{
			Light.intensity = GlobalIntensityMultiplier * GlobalIntensityMaximum;
			Light.enabled = Light.intensity >= GlobalIntensityThreshold;
		}
	}

	private float GetZenithAngleMultiplier()
	{
		if (VanishZenithSpan <= 0f)
		{
			return 1f;
		}
		float num = Vector3.Angle(base.transform.position - WorldCenter, base.transform.up);
		return 1f - Mathf.Clamp01((num * ((float)Math.PI / 180f) - VanishZenithAngle) / VanishZenithSpan);
	}

	private float GetSunAngleMultiplier()
	{
		if (VanishSunAngle <= 0f)
		{
			return 1f;
		}
		float num = Vector3.Angle(base.transform.position - WorldCenter, WorldCenter - SunLight.transform.position);
		return Mathf.Clamp01(Mathf.Cos(num * ((float)Math.PI / 180f) * 90f / VanishSunAngle));
	}

	private float GetSunIntensityMultiplier()
	{
		if (VanishSunIntensity <= 0f)
		{
			return 1f;
		}
		float intensity = SunLight.intensity;
		return 1f - Mathf.Clamp01(intensity / VanishSunIntensity);
	}
}
