using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.Audio;

public class AudioDeleteOnInactive : MonoBehaviour
{
	private static BrixLogger logger = new BrixLogger(typeof(AudioDeleteOnInactive).FullName);

	public string eventName;

	public float expirationDate;

	private void Update()
	{
		if (!SoundEngine.IsPlaying(eventName, base.gameObject))
		{
			if (expirationDate == -1f || Time.time >= expirationDate)
			{
				Object.Destroy(base.gameObject);
			}
			else if (expirationDate == 0f)
			{
				expirationDate = Time.time + 10f;
				logger.Warn("DeleteAudioOnInactive: SoundEngine.IsPlaying(" + eventName + ", " + base.gameObject.name + ") was false on arrival. This is not normal.");
			}
		}
		else
		{
			expirationDate = -1f;
		}
	}
}
