using Brix.Audio;
using UnityEngine;

namespace Brix.Legacy;

public class Torche : MonoBehaviour
{
	public Light pointLight;

	protected void Start()
	{
		SoundEngine.Play("Torch_Start", base.gameObject);
	}

	private void OnDestroy()
	{
		SoundEngine.Play("Torch_Stop", base.gameObject);
	}

	private void Update()
	{
		if (pointLight != null)
		{
			SoundEngine.SetParam("Torch_Intensity", pointLight.intensity, base.gameObject);
		}
	}
}
