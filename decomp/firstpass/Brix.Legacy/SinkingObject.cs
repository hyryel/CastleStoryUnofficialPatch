using Brix.Engine;
using Brix.Game.Components;
using UnityEngine;

namespace Brix.Legacy;

public class SinkingObject : MonoBehaviour
{
	private float sinkingDeadline;

	private float sinkingStartTime;

	private Vector3 sinkingStartPosition;

	public const float SinkingSpeed = 0.7f;

	public const float MaxSinkingTime = 3f;

	public const float MinSinkingTime = 0.5f;

	private void Awake()
	{
		sinkingStartTime = Time.time;
		sinkingDeadline = sinkingStartTime + 3f;
		sinkingStartPosition = base.transform.position;
	}

	private void Update()
	{
		float num = Time.time - sinkingStartTime;
		base.transform.position = sinkingStartPosition - Vector3.up * num * 0.7f;
		if (Time.time >= sinkingDeadline || Voxel.IsFull(XYZ.FromVector3(base.transform.position)))
		{
			base.gameObject.SafeNetworkDestroy();
		}
	}

	private void OnDisable()
	{
		Object.Destroy(this);
	}
}
