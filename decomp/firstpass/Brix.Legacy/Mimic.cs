using UnityEngine;

namespace Brix.Legacy;

[ExecuteInEditMode]
public class Mimic : MonoBehaviour
{
	public enum MimicAxis
	{
		X,
		Y,
		Z
	}

	public GameObject target;

	public MimicAxis Axis = MimicAxis.Y;

	public bool flip;

	private void LateUpdate()
	{
		if ((bool)target)
		{
			base.transform.rotation = target.transform.rotation;
		}
		switch (Axis)
		{
		case MimicAxis.X:
			base.transform.Rotate(0f, -90f, 0f);
			if (flip)
			{
				base.transform.Rotate(180f, 0f, 0f);
			}
			break;
		case MimicAxis.Y:
			base.transform.Rotate(90f, 0f, 0f);
			if (flip)
			{
				base.transform.Rotate(180f, 0f, 0f);
			}
			break;
		case MimicAxis.Z:
			base.transform.Rotate(0f, 0f, -90f);
			if (flip)
			{
				base.transform.Rotate(180f, 0f, 0f);
			}
			break;
		}
	}
}
