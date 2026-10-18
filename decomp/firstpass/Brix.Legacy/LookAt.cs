using Brix.Utils.Locomotion;
using UnityEngine;

namespace Brix.Legacy;

[ExecuteInEditMode]
public class LookAt : MonoBehaviour
{
	public enum Modes
	{
		None,
		Up,
		Point,
		Target,
		Camera,
		CameraFlat,
		ParentForward
	}

	[SerializeField]
	private Modes mode;

	public Vector3 lookAtPoint;

	public GameObject target;

	public Vector3 offset;

	public Modes Mode
	{
		get
		{
			return mode;
		}
		set
		{
			mode = value;
			Refresh();
		}
	}

	private void OnValidate()
	{
		Refresh();
	}

	private void LateUpdate()
	{
		Refresh();
	}

	private void Refresh()
	{
		bool flag = true;
		switch (mode)
		{
		case Modes.Point:
			base.transform.LookAt(lookAtPoint);
			break;
		case Modes.Target:
			base.transform.LookAt(target.transform.position);
			break;
		case Modes.Camera:
			if ((bool)Camera.main)
			{
				base.transform.rotation = Camera.main.transform.rotation;
			}
			break;
		case Modes.CameraFlat:
			if ((bool)Camera.main)
			{
				base.transform.LookAt(Camera.main.transform.position.Flatten(base.transform.position), Camera.main.transform.right);
			}
			break;
		case Modes.ParentForward:
			ParentFoward();
			break;
		case Modes.Up:
			base.transform.LookAt(base.transform.position + Vector3.up, base.transform.parent.right);
			break;
		default:
			flag = false;
			break;
		}
		base.enabled = flag;
		base.transform.Rotate(offset);
	}

	private void ParentFoward()
	{
		Vector3 forward = base.transform.parent.forward;
		float num = Mathf.Atan2(Vector3.Dot(forward, base.transform.up), Vector3.Dot(forward, base.transform.right));
		base.transform.Rotate(Vector3.forward, 90f + num * 57.29578f, Space.Self);
	}
}
