using Brix.Audio;
using UnityEngine;

namespace Brix.Legacy;

public class LanternSoundController : MonoBehaviour
{
	public Light pointLight;

	private void Start()
	{
		SoundEngine.Play("Lantern_Start", base.gameObject);
	}

	private void OnDestroy()
	{
		SoundEngine.Play("Lantern_Stop", base.gameObject);
	}

	private void Update()
	{
		if (pointLight != null)
		{
			SoundEngine.SetParam("Torch_Intensity", pointLight.intensity, base.gameObject);
		}
	}
}
