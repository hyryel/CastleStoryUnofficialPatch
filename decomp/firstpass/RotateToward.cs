using UnityEngine;

public class RotateToward : MonoBehaviour
{
	[SerializeField]
	private RectTransform ObjectToRotate;

	[SerializeField]
	private GameObject Target3D;

	[SerializeField]
	private RectTransform Target2D;

	[SerializeField]
	private bool Is3D;

	public void Follow3D(GameObject go)
	{
		Is3D = true;
		Target3D = go;
	}

	public void Follow2D(RectTransform go)
	{
		Is3D = false;
		Target2D = go;
	}

	private void Update()
	{
		if (!(ObjectToRotate == null))
		{
			if (Is3D)
			{
				DoFollow3D();
			}
			else
			{
				DoFollow2D();
			}
		}
	}

	private void DoFollow3D()
	{
		if (!(Target3D == null))
		{
			Vector3 position = Camera.main.WorldToScreenPoint(Target3D.transform.position);
			position.z = 0f;
			RotateTowards(position);
		}
	}

	private void DoFollow2D()
	{
		if (!(Target2D == null))
		{
			RotateTowards(Target2D.transform.position);
		}
	}

	private void RotateTowards(Vector3 position)
	{
		Vector3 vector = position - ObjectToRotate.transform.position;
		float num = Vector3.Angle(vector, Vector3.up);
		num *= (float)((vector.x < 0f) ? 1 : (-1));
		ObjectToRotate.rotation = Quaternion.Euler(Vector3.forward * num);
	}
}
