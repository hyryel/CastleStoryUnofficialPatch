using System.Collections;
using Brix.Engine;
using Brix.Lifecycle.Pooling;
using UnityEngine;

namespace Brix.Game.Components;

public class Splinter : MonoBehaviour
{
	public enum DeleteMode
	{
		None,
		Delete,
		Sink
	}

	public DeleteMode AutoDelete = DeleteMode.Delete;

	public float AutoDeleteDelay = 10f;

	public const float SinkingSpeed = 0.7f;

	public const float MaxSinkingTime = 3f;

	private bool sinking;

	private float sinkingStartTime;

	private float sinkingDeadline;

	private Vector3 sinkingStartPosition;

	private IEnumerator Start()
	{
		if (AutoDelete == DeleteMode.Delete)
		{
			yield return new WaitForSeconds(AutoDeleteDelay);
			Delete();
		}
		else if (AutoDelete == DeleteMode.Sink)
		{
			yield return new WaitForSeconds(AutoDeleteDelay);
			sinking = true;
			sinkingStartTime = Time.time;
			sinkingDeadline = sinkingStartTime + 3f;
			sinkingStartPosition = base.transform.position;
		}
	}

	private void Update()
	{
		if (sinking)
		{
			float num = Time.time - sinkingStartTime;
			base.transform.position = sinkingStartPosition - Vector3.up * num * 0.7f;
			if (Time.time >= sinkingDeadline || Voxel.IsFull(XYZ.FromVector3(base.transform.position)))
			{
				Delete();
			}
		}
	}

	private void Delete()
	{
		ObjectPoolSingleton.Release(base.gameObject);
	}
}
