using UnityEngine;

namespace Brix.NewUI.LoadingScreen;

public class Rotate1 : MonoBehaviour
{
	public bool Up;

	public bool Forward = true;

	public bool Right;

	public float Speed = 0.025f;

	private void FixedUpdate()
	{
		if (Up)
		{
			base.transform.Rotate(Vector3.up, Speed);
		}
		if (Forward)
		{
			base.transform.Rotate(Vector3.forward, Speed);
		}
		if (Right)
		{
			base.transform.Rotate(Vector3.right, Speed);
		}
	}
}
