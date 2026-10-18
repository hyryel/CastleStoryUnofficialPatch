using Brix.Game.Components;
using UnityEngine;

namespace Brix.Input;

public class CameraDistanceScaler : UpdatableGameComponent<CameraDistanceScaler>
{
	private CompCache<CameraScript> _cam;

	public Vector3 SmallScale = Vector3.one;

	public Vector3 BigScale = Vector3.one;

	public Vector2 DistanceLimits = Vector2.one;

	protected override void Awake()
	{
		base.Awake();
		_cam = new CompCache<CameraScript>(Camera.main.gameObject);
	}

	protected override void BrixFixedUpdate()
	{
		float num = Vector3.Distance(_cam.Get.transform.position, base.transform.position) - DistanceLimits.x;
		base.transform.localScale = Vector3.Lerp(SmallScale, BigScale, num / (DistanceLimits.y - DistanceLimits.x));
	}
}
