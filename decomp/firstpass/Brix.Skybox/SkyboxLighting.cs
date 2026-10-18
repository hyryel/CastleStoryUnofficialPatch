using UnityEngine;

namespace Brix.Skybox;

public class SkyboxLighting : MonoBehaviour
{
	public Animator SkyAnimator;

	public Material SkyMaterial;

	public Orbit Sun;

	public AnimationCurve ElevationBias;

	private void Update()
	{
		SkyAnimator.SetFloat("Time", ElevationBias.Evaluate(Sun.LimitedElevation));
		SkyAnimator.Play("Horizon", 2, ElevationBias.Evaluate(Sun.LimitedElevation));
		SkyMaterial.SetFloat("_SpotDirection", Sun.CurrentTime);
	}
}
