using UnityEngine;

namespace Brix.Skybox;

public class OrbitMimic : MonoBehaviour
{
	public Animator OrbitAnimator;

	public AnimationCurve ElevationBias;

	public Orbit mimicTarget;

	public Vector2 elevationLimits;

	private void Update()
	{
		OrbitAnimator.SetFloat("Cardinal", mimicTarget.CardinalOffsetTime);
		OrbitAnimator.SetFloat("Elevation", Mathf.Lerp(elevationLimits.x, elevationLimits.y, ElevationBias.Evaluate(mimicTarget.Elevation)));
		OrbitAnimator.SetFloat("Distance", mimicTarget.OrbitDistance);
	}
}
