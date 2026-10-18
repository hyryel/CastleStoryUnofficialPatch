using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Brix.Legacy;

public class Brasseur : MonoBehaviour
{
	public enum TargetOption
	{
		SelfOnly,
		ChildrenOnly,
		ChildrenAndSelf
	}

	private class BrasseData
	{
		public float phase;

		public Vector3 delta;

		public Vector3 direction;

		public void Brasser(float amplitude, float frequency, float deltaTime)
		{
			float num = phase + frequency * deltaTime * (float)Math.PI;
			if (Mathf.FloorToInt(phase / (float)Math.PI) != Mathf.FloorToInt(num / (float)Math.PI))
			{
				direction = Vector3.zero;
			}
			if (direction == Vector3.zero)
			{
				direction = UnityEngine.Random.insideUnitSphere;
			}
			phase = num;
			delta = amplitude * direction * Mathf.Sin(Mathf.Max(0f, phase));
		}
	}

	public TargetOption target;

	public Space space = Space.Self;

	public float translation = 0.1f;

	public float rotation = 5f;

	public float frequency = 20f;

	private Dictionary<Transform, LocalTransform> originalTransforms = new Dictionary<Transform, LocalTransform>();

	private BrasseData brasseTrans = new BrasseData();

	private BrasseData brasseRot = new BrasseData();

	public IEnumerable<Transform> transforms
	{
		get
		{
			if (target == TargetOption.SelfOnly || target == TargetOption.ChildrenAndSelf)
			{
				yield return base.transform;
			}
			if (target != TargetOption.ChildrenOnly && target != TargetOption.ChildrenAndSelf)
			{
				yield break;
			}
			IEnumerator enumerator = base.transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					yield return (Transform)enumerator.Current;
				}
			}
			finally
			{
				IDisposable disposable2;
				IDisposable disposable = (disposable2 = enumerator as IDisposable);
				if (disposable2 != null)
				{
					disposable.Dispose();
				}
			}
		}
	}

	private void ResetTransforms()
	{
		foreach (Transform transform in transforms)
		{
			if (transform != null && originalTransforms.TryGetValue(transform, out var value))
			{
				value.CopyTo(transform);
			}
		}
	}

	private void Start()
	{
		foreach (Transform transform in transforms)
		{
			Reinitialiser(transform);
			originalTransforms[transform] = new LocalTransform(transform);
		}
	}

	private void Update()
	{
		foreach (Transform transform in transforms)
		{
			DeplacementNegatif(transform, brasseTrans.delta, brasseRot.delta);
		}
		brasseTrans.Brasser(translation, frequency, Time.deltaTime);
		brasseRot.Brasser(rotation, frequency, Time.deltaTime);
		foreach (Transform transform2 in transforms)
		{
			DeplacementPositif(transform2, brasseTrans.delta, brasseRot.delta);
		}
	}

	private void OnDisable()
	{
		foreach (Transform transform in transforms)
		{
			Reinitialiser(transform);
		}
		ResetTransforms();
	}

	public void Reinitialiser(Transform transform)
	{
		DeplacementNegatif(transform, brasseTrans.delta, brasseRot.delta);
		brasseTrans = new BrasseData();
		brasseRot = new BrasseData();
		brasseTrans.phase = Mathf.Lerp(-0.75f, -0.25f, UnityEngine.Random.value) * (float)Math.PI;
	}

	public void DeplacementNegatif(Transform transform, Vector3 position, Vector3 euler)
	{
		transform.Rotate(new Vector3(0f, 0f, 0f - euler.z), space);
		transform.Rotate(new Vector3(0f, 0f - euler.y, 0f), space);
		transform.Rotate(new Vector3(0f - euler.x, 0f, 0f), space);
		transform.Translate(-position, space);
	}

	public void DeplacementPositif(Transform transform, Vector3 position, Vector3 euler)
	{
		transform.Translate(position, space);
		transform.Rotate(new Vector3(euler.x, 0f, 0f), space);
		transform.Rotate(new Vector3(0f, euler.y, 0f), space);
		transform.Rotate(new Vector3(0f, 0f, euler.z), space);
	}

	public void Brasser(Transform transform, float deltaTime)
	{
		DeplacementNegatif(transform, brasseTrans.delta, brasseRot.delta);
		brasseTrans.Brasser(translation, frequency, deltaTime);
		brasseRot.Brasser(rotation, frequency, deltaTime);
		DeplacementPositif(transform, brasseTrans.delta, brasseRot.delta);
	}
}
