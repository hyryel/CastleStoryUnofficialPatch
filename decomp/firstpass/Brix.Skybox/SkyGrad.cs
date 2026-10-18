using UnityEngine;

namespace Brix.Skybox;

public class SkyGrad : MonoBehaviour
{
	public Animator SkyAnimator;

	public Animator Sun;

	public Material SkyMaterial;

	public AnimationCurve ElevationBias;

	public Orbit SunOrbit;

	private void Update()
	{
		SunOrbit = Sun.transform.GetComponent<Orbit>();
		SkyAnimator.SetFloat("Time", ElevationBias.Evaluate(SunOrbit.LimitedElevation));
		SkyAnimator.Play("Horizon", 2, ElevationBias.Evaluate(SunOrbit.LimitedElevation));
		SkyMaterial.SetFloat("_SpotDirection", SunOrbit.CurrentTime);
	}
}
