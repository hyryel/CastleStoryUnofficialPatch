using Brix.Audio;
using Brix.Game.AI;
using Brix.Game.Components;
using UnityEngine;

namespace Brix.Game.Audio;

public class FlailingImpactAudio : MonoBehaviour
{
	private float _lastImpactTime;

	private float _lastImpactStrength;

	public void PlayImpactSoundEvent(Locomotion4 locomotion, float impactVelocity)
	{
		if (!(impactVelocity < 0.1f) && (!(Time.time < _lastImpactTime + 1f) || !(_lastImpactStrength > impactVelocity)))
		{
			_lastImpactTime = Time.time;
			_lastImpactStrength = impactVelocity;
			SoundEngine.SetParam("Impact_Strength", 100f * impactVelocity / 50f, locomotion.gameObject);
			if (locomotion.Labor.Profession is Builder)
			{
				SoundEngine.Play("Bricktron_Impact", locomotion.gameObject);
			}
			else
			{
				SoundEngine.Play("Corruptron_Impact", locomotion.gameObject);
			}
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		Locomotion4 componentInChildren = GetComponentInChildren<Locomotion4>();
		if (!componentInChildren.IsNullOrReleased() && componentInChildren.IsFlailing())
		{
			PlayImpactSoundEvent(componentInChildren, collision.relativeVelocity.magnitude);
		}
	}
}
