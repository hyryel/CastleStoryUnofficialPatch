using Brix.Game.Components;
using UnityEngine;

namespace Brix.Input;

public class CameraDistanceColliderScaler : UpdatableGameComponent<CameraDistanceScaler>
{
	private CompCache<CameraScript> _cam;

	public float SmallScale = 1f;

	public float BigScale = 1f;

	public Vector2 DistanceLimits = Vector2.one;

	public SphereCollider Sphere;

	public override void OnRequested()
	{
		base.OnRequested();
		_cam = new CompCache<CameraScript>(Camera.main.gameObject);
	}

	protected override void BrixFixedUpdate()
	{
		float num = Vector3.Distance(_cam.Get.transform.position, base.transform.position) - DistanceLimits.x;
		float radius = Mathf.Lerp(SmallScale, BigScale, num / (DistanceLimits.y - DistanceLimits.x));
		Sphere.radius = radius;
	}
}
