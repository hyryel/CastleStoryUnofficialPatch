using UnityEngine;

namespace Brix.Utils;

public class AttachToCamera : MonoBehaviour
{
	public Camera parent;

	private void Start()
	{
		if (parent == null)
		{
			parent = Camera.main;
		}
		base.transform.SetParent(parent.transform);
		base.transform.localPosition = Vector3.zero;
		base.transform.localRotation = Quaternion.identity;
	}
}
