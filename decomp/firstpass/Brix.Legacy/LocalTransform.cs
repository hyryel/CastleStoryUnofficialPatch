using UnityEngine;

namespace Brix.Legacy;

public struct LocalTransform
{
	public Vector3 position;

	public Quaternion rotation;

	public Vector3 scale;

	public LocalTransform(Vector3 position, Quaternion rotation, Vector3 scale)
	{
		this.position = position;
		this.rotation = rotation;
		this.scale = scale;
	}

	public LocalTransform(Transform t)
	{
		position = t.localPosition;
		rotation = t.localRotation;
		scale = t.localScale;
	}

	public void CopyTo(Transform t)
	{
		t.localPosition = position;
		t.localRotation = rotation;
		t.localScale = scale;
	}
}
