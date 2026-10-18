using Brix.Game.Components;
using UnityEngine;

namespace Brix.Legacy;

public class Facing : UpdatableGameComponent<Facing>
{
	public bool regardeLaCamera;

	public bool gardeLeNord;

	public bool lateUpdate;

	public Vector3 alignement;

	protected override void BrixUpdate()
	{
		if (!lateUpdate)
		{
			Face();
		}
	}

	protected override void BrixLateUpdate()
	{
		if (lateUpdate)
		{
			Face();
		}
	}

	public void Face()
	{
		if (gardeLeNord)
		{
			base.transform.rotation = Camera.main.transform.rotation * Quaternion.AngleAxis(180f, Vector3.up);
		}
		else if (regardeLaCamera)
		{
			base.transform.LookAt(Camera.main.transform);
		}
		base.transform.Rotate(alignement, Space.Self);
	}
}
