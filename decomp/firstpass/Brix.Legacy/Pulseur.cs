using System;
using UnityEngine;

namespace Brix.Legacy;

public class Pulseur
{
	public static Journee journee;

	public float longFade;

	public int dernierCheck;

	public float longFadeDuration;

	public float shortPluseDuration;

	public Vector2 interval;

	public float pulseOffset;

	public float heure
	{
		get
		{
			float num = ((!(journee != null)) ? 6f : journee.Heure);
			return (num + 24f - 18f) % 24f;
		}
	}

	public float LongFade
	{
		get
		{
			if (dernierCheck != Time.frameCount)
			{
				float num = ((!(journee != null)) ? 250f : journee.Longueur);
				float num2 = num / 24f;
				float num3 = longFadeDuration / num2;
				float num4 = (11.5f - num3) * num2;
				float num5 = heure * num2;
				dernierCheck = Time.frameCount;
				longFade = Mathf.Min(Mathf.Lerp(0f, 1f, num5 / longFadeDuration), Mathf.Lerp(1f, 0f, (num5 - num4) / longFadeDuration));
			}
			return longFade;
		}
	}

	public float ShortFade => Mathf.Min(longFade * 5f, 1f);

	public float ShortPulse
	{
		get
		{
			float periode = (Time.time + pulseOffset) / shortPluseDuration * ((float)Math.PI * 2f);
			return ShortFade * modulation(interval.x, interval.y, periode);
		}
	}

	public float LongPulse
	{
		get
		{
			float periode = (Time.time + pulseOffset) / (shortPluseDuration * 5f) * ((float)Math.PI * 2f);
			return ShortFade * modulation(interval.x, interval.y, periode);
		}
	}

	public Pulseur(float longFadeDuration, float shortPluseDuration)
		: this(longFadeDuration, shortPluseDuration, 0f, 1f)
	{
	}

	public Pulseur(float longFadeDuration, float shortPluseDuration, float min, float max)
	{
		if (journee == null)
		{
			journee = Journee.instance;
		}
		this.longFadeDuration = longFadeDuration;
		this.shortPluseDuration = shortPluseDuration;
		interval = new Vector2(min, max);
		pulseOffset = UnityEngine.Random.value * (float)Math.PI * 4f;
	}

	public float ShortPulseInterval(float min, float max)
	{
		float periode = (Time.time + pulseOffset) / shortPluseDuration * ((float)Math.PI * 2f);
		return ShortFade * modulation(min, max, periode);
	}

	public float modulation(float min, float max, float periode)
	{
		return Mathf.Lerp(min, max, 0.5f - Mathf.Cos(periode) / 2f);
	}
}
