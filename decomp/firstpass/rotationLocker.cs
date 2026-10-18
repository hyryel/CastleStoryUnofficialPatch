using UnityEngine;

public class rotationLocker : MonoBehaviour
{
	private void LateUpdate()
	{
		base.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
	}
}
